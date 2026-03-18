using Avalonia;
using Avalonia.Markup.Xaml.XamlIl.Runtime;
using Avalonia.Media;
using Avalonia.Styling;
using DesignTokens.Helpers;

namespace DesignTokens.Tests.TestUtils;

internal sealed class ColorTokenHost : TestTokenHost<Color, object?, ColorTokenHost>;

internal sealed class BrushTokenHost : TestTokenHost<IBrush, object?, BrushTokenHost>;

internal static class TokenTestHelper
{
    internal static TokenKey<Color, object?> ColorToken { get; } = new(null);

    internal static TokenKey<IBrush, object?> BrushToken { get; } = new(null);

    internal static ITokenResolver<Color, object?> CreateColorResolver(Color light, Color dark)
    {
        return new FakeTokenResolver(light, dark, null, null);
    }

    internal static ITokenResolver<IBrush, object?> CreateBrushResolver(Color light, Color dark)
    {
        return new FakeTokenResolver(null, null, light, dark);
    }

    internal static Color ResolveColor(TokenHostState<Color, object?, ColorTokenHost> state)
    {
        if (state.Resolver is null)
            throw new InvalidOperationException("Expected a resolver.");

        return state.Resolver.TryResolve(
            ColorToken,
            state.ThemeVariant,
            state.HostObject,
            out var value)
            ? value
            : throw new InvalidOperationException("Expected a color value.");
    }

    internal static IObservable<Color> CreateColorObservable(
        AvaloniaObject? source,
        IThemeVariantHost? themeHost,
        Color fallbackColor,
        ThemeVariant? themeVariant = null
    )
    {
        var context = new TokenBindingContext(
            (object?)themeHost ?? source,
            null,
            source,
            themeVariant,
            null);

        return TokenBinding.CreateObservable<Color, object?, ColorTokenHost>(context, ColorToken, fallbackColor);
    }

    internal static void ForceFullGc()
    {
        for (var i = 0; i < 3; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }
}

internal sealed class FakeTokenResolver(
    Color? lightColor,
    Color? darkColor,
    Color? lightBrushColor,
    Color? darkBrushColor
) : ITokenResolver<Color, object?>, ITokenResolver<IBrush, object?>
{
    bool ITokenResolver<Color, object?>.TryResolve(
        TokenKey<Color, object?> key,
        ThemeVariant themeVariant,
        AvaloniaObject? hostObject,
        out Color value
    )
    {
        value = default;

        if (ReferenceEquals(key, TokenTestHelper.ColorToken)
            && ResolveColor(themeVariant) is { } resolved)
        {
            value = resolved;
            return true;
        }

        return false;
    }

    bool ITokenResolver<IBrush, object?>.TryResolve(
        TokenKey<IBrush, object?> key,
        ThemeVariant themeVariant,
        AvaloniaObject? hostObject,
        out IBrush value
    )
    {
        value = default!;

        if (ReferenceEquals(key, TokenTestHelper.BrushToken)
            && ResolveBrush(themeVariant) is { } resolved)
        {
            value = resolved;
            return true;
        }

        return false;
    }

    private Color? ResolveColor(ThemeVariant themeVariant)
    {
        return ReferenceEquals(themeVariant, ThemeVariant.Dark) ? darkColor : lightColor;
    }

    private IBrush? ResolveBrush(ThemeVariant themeVariant)
    {
        var color = ReferenceEquals(themeVariant, ThemeVariant.Dark) ? darkBrushColor : lightBrushColor;
        return color is { } resolved ? new SolidColorBrush(resolved) : null;
    }
}

internal sealed class RecordingObserver<T> : IObserver<T>
{
    public List<T> Values { get; } = [];

    public void OnCompleted()
    {
    }

    public void OnError(Exception error)
    {
    }

    public void OnNext(T value)
    {
        Values.Add(value);
    }
}

internal sealed class TestParentStackProvider(IEnumerable<object> parents) : IAvaloniaXamlIlParentStackProvider
{
    public IEnumerable<object> Parents { get; } = parents;
}