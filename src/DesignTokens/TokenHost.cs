using Avalonia;

namespace DesignTokens;

public interface ITokenHost<TValue, TKey, TTokenHost>
    where TTokenHost : ITokenHost<TValue, TKey, TTokenHost>
{
    static abstract IObservable<ITokenResolver<TValue, TKey>?> GetTokenObservable(AvaloniaObject element);
    static abstract ITokenResolver<TValue, TKey>? GetResolver(AvaloniaObject element);
    static abstract void SetResolver(AvaloniaObject element, ITokenResolver<TValue, TKey>? value);
    static abstract void ClearResolver(AvaloniaObject element);
}