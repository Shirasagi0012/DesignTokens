using Avalonia;

namespace DesignTokens;

public abstract class TokenHost<TValue, TKey> : AvaloniaObject
{
    public static readonly AttachedProperty<ITokenResolver<TValue, TKey>?> ResolverProperty =
        AvaloniaProperty.RegisterAttached<TokenHost<TValue, TKey>, AvaloniaObject, ITokenResolver<TValue, TKey>?>(
            "Resolver",
            inherits: true);

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