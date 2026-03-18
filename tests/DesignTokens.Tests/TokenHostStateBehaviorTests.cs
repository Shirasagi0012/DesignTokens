using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using DesignTokens.Helpers;
using DesignTokens.Tests.TestUtils;
using Xunit;

namespace DesignTokens.Tests;

public class TokenHostStateBehaviorTests
{
    [AvaloniaFact]
    public void ResolveHost_PrefersTargetObjectOverProviderOwner()
    {
        var application = Assert.IsType<HeadlessTestApplication>(Application.Current);
        application.RequestedThemeVariant = ThemeVariant.Light;
        ColorTokenHost.SetResolver(application, TokenTestHelper.CreateColorResolver(Colors.Blue, Colors.CornflowerBlue));

        var owner = new Border();
        ColorTokenHost.SetResolver(owner, TokenTestHelper.CreateColorResolver(Colors.Red, Colors.DarkRed));

        var target = new Border();
        ColorTokenHost.SetResolver(target, TokenTestHelper.CreateColorResolver(Colors.Green, Colors.DarkGreen));

        var provider = new ResourceDictionary();
        ((IResourceProvider)provider).AddOwner(owner);

        using var state = new TokenHostState<Color, object?, ColorTokenHost>(
            new TokenBindingContext(provider, provider, target, null, null),
            application);

        Assert.Same(target, state.HostObject);
        Assert.Equal(Colors.Green, TokenTestHelper.ResolveColor(state));
    }

    [AvaloniaFact]
    public void ResolveHost_RebindsWhenProviderOwnerChanges()
    {
        var application = Assert.IsType<HeadlessTestApplication>(Application.Current);
        application.RequestedThemeVariant = ThemeVariant.Light;
        ColorTokenHost.SetResolver(application, TokenTestHelper.CreateColorResolver(Colors.Blue, Colors.CornflowerBlue));

        var owner1 = new Border();
        ColorTokenHost.SetResolver(owner1, TokenTestHelper.CreateColorResolver(Colors.Red, Colors.DarkRed));

        var owner2 = new Border();
        ColorTokenHost.SetResolver(owner2, TokenTestHelper.CreateColorResolver(Colors.Green, Colors.DarkGreen));

        var provider = new ResourceDictionary();

        using var state = new TokenHostState<Color, object?, ColorTokenHost>(
            new TokenBindingContext(provider, provider, null, null, null),
            application);

        Assert.Same(application, state.HostObject);
        Assert.Equal(Colors.Blue, TokenTestHelper.ResolveColor(state));

        ((IResourceProvider)provider).AddOwner(owner1);

        Assert.Same(owner1, state.HostObject);
        Assert.Equal(Colors.Red, TokenTestHelper.ResolveColor(state));

        ((IResourceProvider)provider).RemoveOwner(owner1);
        ((IResourceProvider)provider).AddOwner(owner2);

        Assert.Same(owner2, state.HostObject);
        Assert.Equal(Colors.Green, TokenTestHelper.ResolveColor(state));
    }

    [AvaloniaFact]
    public void ResolveHost_FallsBackToApplication()
    {
        var application = Assert.IsType<HeadlessTestApplication>(Application.Current);
        application.RequestedThemeVariant = ThemeVariant.Light;
        ColorTokenHost.SetResolver(application, TokenTestHelper.CreateColorResolver(Colors.Blue, Colors.CornflowerBlue));

        using var state = new TokenHostState<Color, object?, ColorTokenHost>(
            new TokenBindingContext(null, null, null, null, null),
            application);

        Assert.Same(application, state.HostObject);
        Assert.Equal(Colors.Blue, TokenTestHelper.ResolveColor(state));
    }

    [AvaloniaFact]
    public void ThemeVariant_ExplicitOverrideBeatsDictionaryAndActualTheme()
    {
        var application = Assert.IsType<HeadlessTestApplication>(Application.Current);

        var owner = new ThemeVariantScope
        {
            RequestedThemeVariant = ThemeVariant.Light
        };
        ColorTokenHost.SetResolver(owner, TokenTestHelper.CreateColorResolver(Colors.Red, Colors.DarkRed));

        var provider = new ResourceDictionary();
        ((IThemeVariantProvider)provider).Key = ThemeVariant.Light;
        ((IResourceProvider)provider).AddOwner(owner);

        using var state = new TokenHostState<Color, object?, ColorTokenHost>(
            new TokenBindingContext(provider, provider, null, ThemeVariant.Dark, ThemeVariant.Light),
            application);

        Assert.Equal(ThemeVariant.Dark, state.ThemeVariant);
        Assert.Equal(Colors.DarkRed, TokenTestHelper.ResolveColor(state));
    }

    [AvaloniaFact]
    public void ThemeVariant_DictionaryOverrideBeatsActualTheme()
    {
        var application = Assert.IsType<HeadlessTestApplication>(Application.Current);

        var owner = new ThemeVariantScope
        {
            RequestedThemeVariant = ThemeVariant.Dark
        };
        ColorTokenHost.SetResolver(owner, TokenTestHelper.CreateColorResolver(Colors.Red, Colors.DarkRed));

        var provider = new ResourceDictionary();
        ((IThemeVariantProvider)provider).Key = ThemeVariant.Light;
        ((IResourceProvider)provider).AddOwner(owner);

        using var state = new TokenHostState<Color, object?, ColorTokenHost>(
            new TokenBindingContext(provider, provider, null, null, ThemeVariant.Light),
            application);

        Assert.Equal(ThemeVariant.Light, state.ThemeVariant);
        Assert.Equal(Colors.Red, TokenTestHelper.ResolveColor(state));
    }

    [AvaloniaFact]
    public void ThemeVariant_FallsBackToActualThemeAndThenLight()
    {
        var owner = new ThemeVariantScope
        {
            RequestedThemeVariant = ThemeVariant.Dark
        };
        ColorTokenHost.SetResolver(owner, TokenTestHelper.CreateColorResolver(Colors.Red, Colors.DarkRed));

        using var stateWithOwner = new TokenHostState<Color, object?, ColorTokenHost>(
            new TokenBindingContext(owner, null, owner, null, null),
            null);

        Assert.Equal(ThemeVariant.Dark, stateWithOwner.ThemeVariant);
        Assert.Equal(Colors.DarkRed, TokenTestHelper.ResolveColor(stateWithOwner));

        using var stateWithoutHost = new TokenHostState<Color, object?, ColorTokenHost>(
            new TokenBindingContext(null, null, null, null, null),
            null);

        Assert.Equal(ThemeVariant.Light, stateWithoutHost.ThemeVariant);
    }
}