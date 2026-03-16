using Avalonia;
using Avalonia.Data;
using Avalonia.Markup.Xaml.XamlIl.Runtime;
using Avalonia.Styling;

namespace DesignTokens;

public static class TokenExtensionHelper<TValue, TKey>
{
    public static IObservable<TValue> ProvideObservable(
        IServiceProvider serviceProvider,
        TokenKey<TValue, TKey> tokenKey,
        ThemeVariant? theme,
        TValue? fallbackValue
    )
    {
        var (target, parentStack) = TokenBinding.GetContextServices(serviceProvider);

        var targetObject = target.TargetObject as AvaloniaObject;
        var context = TokenBinding.CaptureContext(parentStack, targetObject, theme);
        return TokenBinding.CreateObservable(context, tokenKey, fallbackValue);
    }
}