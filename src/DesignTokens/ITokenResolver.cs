using Avalonia;
using Avalonia.Styling;

namespace DesignTokens;

public interface ITokenResolver<TValue, TKey>
{
    bool TryResolve(
        TokenKey<TValue, TKey> key,
        ThemeVariant themeVariant,
        AvaloniaObject? hostObject,
        out TValue value
    );
}