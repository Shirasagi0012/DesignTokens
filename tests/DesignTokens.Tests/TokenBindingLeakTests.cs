using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Markup.Xaml.XamlIl.Runtime;
using Avalonia.Media;
using Avalonia.Styling;
using DesignTokens.Tests.TestUtils;
using Xunit;

namespace DesignTokens.Tests;

public class TokenBindingLeakTests
{
    [AvaloniaFact]
    public void ObservableSubscription_Dispose_ReleasesObserver_FromSourceHost()
    {
        var application = Assert.IsType<HeadlessTestApplication>(Application.Current);
        var resolver = TokenTestHelper.CreateColorResolver(Colors.Red, Colors.DarkRed);
        TokenHost<Color, object?>.SetResolver(application, resolver);

        var weakObserver = CreateDisposedObserverFromSourceHost(application);

        TokenTestHelper.ForceFullGc();

        Assert.False(weakObserver.TryGetTarget(out _));
    }

    [AvaloniaFact]
    public void ObservableSubscription_Dispose_ReleasesObserver_FromApplicationFallback()
    {
        var application = Assert.IsType<HeadlessTestApplication>(Application.Current);
        application.RequestedThemeVariant = ThemeVariant.Light;

        var resolver = TokenTestHelper.CreateColorResolver(Colors.Red, Colors.DarkRed);
        TokenHost<Color, object?>.SetResolver(application, resolver);

        var weakObserver = CreateDisposedObserverFromApplicationFallback(application);

        TokenTestHelper.ForceFullGc();

        Assert.False(weakObserver.TryGetTarget(out _));
    }

    [AvaloniaFact]
    public void ObservableSubscription_Dispose_ReleasesObserver_FromProviderAnchor()
    {
        var application = Assert.IsType<HeadlessTestApplication>(Application.Current);
        application.RequestedThemeVariant = ThemeVariant.Light;

        var owner = new Border();
        TokenHost<Color, object?>.SetResolver(owner, TokenTestHelper.CreateColorResolver(Colors.Red, Colors.DarkRed));

        var provider = new ResourceDictionary();
        ((IResourceProvider)provider).AddOwner(owner);

        var weakObserver = CreateDisposedObserverFromProviderAnchor(provider);

        TokenTestHelper.ForceFullGc();

        Assert.False(weakObserver.TryGetTarget(out _));
    }

    [AvaloniaFact]
    public void OwnerChanged_UnsubscribesFromOldOwnerRuntimeState()
    {
        var application = Assert.IsType<HeadlessTestApplication>(Application.Current);
        application.RequestedThemeVariant = ThemeVariant.Light;
        TokenHost<Color, object?>.SetResolver(application, TokenTestHelper.CreateColorResolver(Colors.Blue, Colors.CornflowerBlue));

        var owner1Resolver = TokenTestHelper.CreateColorResolver(Colors.Red, Colors.DarkRed);
        var owner1 = new Border();
        TokenHost<Color, object?>.SetResolver(owner1, owner1Resolver);

        var owner2Resolver = TokenTestHelper.CreateColorResolver(Colors.Green, Colors.DarkGreen);
        var owner2 = new Border();
        TokenHost<Color, object?>.SetResolver(owner2, owner2Resolver);

        var provider = new ResourceDictionary();
        ((IResourceProvider)provider).AddOwner(owner1);

        var observer = new RecordingObserver<Color>();
        var context = TokenBinding.CaptureContext(new TestParentStackProvider([provider, owner1]), null, null);
        using var subscription = TokenBinding.CreateObservable(context, TokenTestHelper.ColorToken, Colors.Transparent)
            .Subscribe(observer);

        ((IResourceProvider)provider).RemoveOwner(owner1);
        ((IResourceProvider)provider).AddOwner(owner2);

        var countAfterSwap = observer.Values.Count;

        TokenHost<Color, object?>.SetResolver(owner1, new FakeTokenResolver(Colors.Yellow, Colors.Goldenrod, null, null));

        Assert.Equal(countAfterSwap, observer.Values.Count);

        TokenHost<Color, object?>.SetResolver(owner2, new FakeTokenResolver(Colors.Purple, Colors.MediumPurple, null, null));

        Assert.True(observer.Values.Count > countAfterSwap);
    }

    [AvaloniaFact]
    public void OwnerChanged_UnsubscribesFromOldThemeHost()
    {
        var application = Assert.IsType<HeadlessTestApplication>(Application.Current);
        application.RequestedThemeVariant = ThemeVariant.Light;

        var owner1 = new ThemeVariantScope
        {
            RequestedThemeVariant = ThemeVariant.Light
        };
        TokenHost<Color, object?>.SetResolver(owner1, TokenTestHelper.CreateColorResolver(Colors.Red, Colors.Blue));

        var owner2 = new ThemeVariantScope
        {
            RequestedThemeVariant = ThemeVariant.Light
        };
        TokenHost<Color, object?>.SetResolver(owner2, TokenTestHelper.CreateColorResolver(Colors.Red, Colors.Blue));

        var provider = new ResourceDictionary();
        ((IResourceProvider)provider).AddOwner(owner1);

        var observer = new RecordingObserver<Color>();
        var context = TokenBinding.CaptureContext(new TestParentStackProvider([provider, owner1]), null, null);
        using var subscription = TokenBinding.CreateObservable(context, TokenTestHelper.ColorToken, Colors.Transparent)
            .Subscribe(observer);

        ((IResourceProvider)provider).RemoveOwner(owner1);
        ((IResourceProvider)provider).AddOwner(owner2);

        var publishedCountAfterSwap = observer.Values.Count;

        owner1.RequestedThemeVariant = ThemeVariant.Dark;

        Assert.Equal(publishedCountAfterSwap, observer.Values.Count);

        owner2.RequestedThemeVariant = ThemeVariant.Dark;

        Assert.True(observer.Values.Count > publishedCountAfterSwap);
    }

    [AvaloniaFact]
    public void RuntimeStateSwap_UnsubscribesFromOldRuntimeState()
    {
        var application = Assert.IsType<HeadlessTestApplication>(Application.Current);
        application.RequestedThemeVariant = ThemeVariant.Light;
        TokenHost<Color, object?>.SetResolver(application, null);

        var source = new Border();
        var oldResolver = TokenTestHelper.CreateColorResolver(Colors.Red, Colors.DarkRed);
        var newResolver = TokenTestHelper.CreateColorResolver(Colors.Blue, Colors.DarkBlue);
        TokenHost<Color, object?>.SetResolver(source, oldResolver);

        var observer = new RecordingObserver<Color>();
        using var subscription = TokenTestHelper
            .CreateColorObservable(source, null, Colors.Transparent)
            .Subscribe(observer);

        Assert.Equal(Colors.Red, observer.Values[^1]);

        TokenHost<Color, object?>.SetResolver(source, newResolver);

        Assert.Equal(Colors.Blue, observer.Values[^1]);
        var publishedCountAfterSwap = observer.Values.Count;

        TokenHost<Color, object?>.SetResolver(source, newResolver);

        Assert.Equal(publishedCountAfterSwap, observer.Values.Count);

        TokenHost<Color, object?>.SetResolver(source, new FakeTokenResolver(Colors.Yellow, Colors.Goldenrod, null, null));

        Assert.True(observer.Values.Count > publishedCountAfterSwap);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<RecordingObserver<Color>> CreateDisposedObserverFromSourceHost(
        Application source
    )
    {
        var observer = new RecordingObserver<Color>();
        var subscription = TokenTestHelper
            .CreateColorObservable(source, null, Colors.Transparent)
            .Subscribe(observer);

        TokenHost<Color, object?>.SetResolver(source, new FakeTokenResolver(Colors.Blue, Colors.CornflowerBlue, null, null));

        subscription.Dispose();

        var publishedCount = observer.Values.Count;
        TokenHost<Color, object?>.SetResolver(source, new FakeTokenResolver(Colors.Green, Colors.DarkGreen, null, null));

        Assert.Equal(publishedCount, observer.Values.Count);

        return new WeakReference<RecordingObserver<Color>>(observer);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<RecordingObserver<Color>> CreateDisposedObserverFromApplicationFallback(
        Application application
    )
    {
        var observer = new RecordingObserver<Color>();
        var context = TokenBinding.CaptureContext(new TestParentStackProvider(Array.Empty<object>()), null, null);
        var subscription = TokenBinding.CreateObservable(context, TokenTestHelper.ColorToken, Colors.Transparent)
            .Subscribe(observer);

        TokenHost<Color, object?>.SetResolver(application, new FakeTokenResolver(Colors.Blue, Colors.CornflowerBlue, null, null));
        application.RequestedThemeVariant = ThemeVariant.Dark;

        subscription.Dispose();

        var publishedCount = observer.Values.Count;
        TokenHost<Color, object?>.SetResolver(application, new FakeTokenResolver(Colors.Green, Colors.DarkGreen, null, null));
        application.RequestedThemeVariant = ThemeVariant.Light;

        Assert.Equal(publishedCount, observer.Values.Count);

        return new WeakReference<RecordingObserver<Color>>(observer);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<RecordingObserver<Color>> CreateDisposedObserverFromProviderAnchor(
        ResourceDictionary provider
    )
    {
        var observer = new RecordingObserver<Color>();
        var context = TokenBinding.CaptureContext(new TestParentStackProvider([provider]), null, null);
        var subscription = TokenBinding.CreateObservable(context, TokenTestHelper.ColorToken, Colors.Transparent)
            .Subscribe(observer);

        subscription.Dispose();

        return new WeakReference<RecordingObserver<Color>>(observer);
    }
}