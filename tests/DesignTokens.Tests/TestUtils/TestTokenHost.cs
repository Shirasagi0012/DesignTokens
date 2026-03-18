#pragma warning disable AVP1002
using Avalonia;

namespace DesignTokens.Tests.TestUtils;

internal abstract class TestTokenHost<TValue, TKey, TTokenHost> : AvaloniaObject, ITokenHost<TValue, TKey, TTokenHost>
    where TTokenHost : TestTokenHost<TValue, TKey, TTokenHost>
{
    private static readonly AttachedProperty<ITokenResolver<TValue, TKey>?> ResolverProperty =
        AvaloniaProperty.RegisterAttached<TTokenHost, AvaloniaObject, ITokenResolver<TValue, TKey>?>(
            "Resolver",
            inherits: true);

    public static IObservable<ITokenResolver<TValue, TKey>?> GetTokenObservable(AvaloniaObject element)
    {
        return element.GetObservable(ResolverProperty);
    }

    public static ITokenResolver<TValue, TKey>? GetResolver(AvaloniaObject element)
    {
        return element.GetValue(ResolverProperty);
    }

    public static void SetResolver(AvaloniaObject element, ITokenResolver<TValue, TKey>? value)
    {
        element.SetValue(ResolverProperty, value);
    }

    public static void ClearResolver(AvaloniaObject element)
    {
        element.ClearValue(ResolverProperty);
    }
}

#pragma warning restore AVP1002
