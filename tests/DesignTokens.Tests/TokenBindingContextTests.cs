using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Headless.XUnit;
using Avalonia.Markup.Xaml.XamlIl.Runtime;
using Avalonia.Media;
using Avalonia.Styling;
using DesignTokens.Tests.TestUtils;
using Xunit;

namespace DesignTokens.Tests;

public class TokenBindingContextTests
{
    [AvaloniaFact]
    public void CaptureContext_PrefersProviderAnchorAndReadsDictionaryVariant()
    {
        var owner = new Border();
        var provider = new ResourceDictionary();
        ((IThemeVariantProvider)provider).Key = ThemeVariant.Dark;
        var target = new SolidColorBrush();

        var context = TokenBinding.CaptureContext(
            new TestParentStackProvider([provider, owner]),
            target,
            null);

        Assert.Same(provider, context.Anchor);
        Assert.Same(provider, context.ProviderAnchor);
        Assert.Same(target, context.TargetObject);
        Assert.Equal(ThemeVariant.Dark, context.DictionaryThemeVariant);
    }

    [AvaloniaFact]
    public void CreateObservable_UsesCapturedProviderOwner()
    {
        var owner = new ThemeVariantScope
        {
            RequestedThemeVariant = ThemeVariant.Light
        };
        ColorTokenHost.SetResolver(owner, TokenTestHelper.CreateColorResolver(Colors.Red, Colors.DarkRed));

        var provider = new ResourceDictionary();
        ((IResourceProvider)provider).AddOwner(owner);

        var observer = new RecordingObserver<Color>();
        var context = TokenBinding.CaptureContext(new TestParentStackProvider([provider, owner]), null, null);
        using var subscription = TokenBinding.CreateObservable<Color, object?, ColorTokenHost>(context, TokenTestHelper.ColorToken, Colors.Transparent)
            .Subscribe(observer);

        Assert.Equal(Colors.Red, observer.Values[^1]);
    }

    [AvaloniaFact]
    public void CreateObservable_UsesDictionaryThemeVariantFromParentStack()
    {
        var owner = new ThemeVariantScope
        {
            RequestedThemeVariant = ThemeVariant.Dark
        };
        ColorTokenHost.SetResolver(owner, TokenTestHelper.CreateColorResolver(Colors.Red, Colors.DarkRed));

        var provider = new ResourceDictionary();
        ((IThemeVariantProvider)provider).Key = ThemeVariant.Light;
        ((IResourceProvider)provider).AddOwner(owner);

        var observer = new RecordingObserver<Color>();
        var context = TokenBinding.CaptureContext(new TestParentStackProvider([provider, owner]), null, null);
        using var subscription = TokenBinding.CreateObservable<Color, object?, ColorTokenHost>(context, TokenTestHelper.ColorToken, Colors.Transparent)
            .Subscribe(observer);

        Assert.Equal(Colors.Red, observer.Values[^1]);
        Assert.NotEqual(Colors.DarkRed, observer.Values[^1]);
    }

    [AvaloniaFact]
    public void ToBinding_WhenAttachedToControlProperty_UpdatesAndReleasesTargetAfterClear()
    {
        var application = Assert.IsType<HeadlessTestApplication>(Application.Current);
        application.RequestedThemeVariant = ThemeVariant.Light;
        BrushTokenHost.SetResolver(application, null);

        var source = new Border();
        var sourceResolver = TokenTestHelper.CreateBrushResolver(Colors.Red, Colors.DarkRed);
        BrushTokenHost.SetResolver(source, sourceResolver);

        var weakTarget = CreateBoundTargetAndClearBinding(source);

        TokenTestHelper.ForceFullGc();

        Assert.False(weakTarget.TryGetTarget(out _));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<Border> CreateBoundTargetAndClearBinding(Border source)
    {
        var target = new Border();
        var context = TokenBinding.CaptureContext(new TestParentStackProvider([source]), null, null);
        var binding = TokenBinding.CreateObservable<IBrush, object?, BrushTokenHost>(context, TokenTestHelper.BrushToken, default!).ToBinding();

        var bindingHandle = target.Bind(Border.BackgroundProperty, binding);

        Assert.NotNull(target.Background);
        Assert.Equal(Colors.Red, Assert.IsType<SolidColorBrush>(target.Background).Color);

        BrushTokenHost.SetResolver(source, new FakeTokenResolver(null, null, Colors.Blue, Colors.DarkBlue));

        Assert.Equal(Colors.Blue, Assert.IsType<SolidColorBrush>(target.Background).Color);

        bindingHandle.Dispose();
        BrushTokenHost.SetResolver(source, new FakeTokenResolver(null, null, Colors.Green, Colors.DarkGreen));

        Assert.Null(target.Background);

        return new WeakReference<Border>(target);
    }
}