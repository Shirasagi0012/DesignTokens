using Avalonia;
using Avalonia.Styling;

namespace DesignTokens.Helpers;

internal sealed class TokenObservable<TValue, TKey, TTokenHost>(
    TokenBindingContext context,
    Application? application,
    TValue? fallbackValue,
    Func<ITokenResolver<TValue, TKey>?, ThemeVariant, AvaloniaObject?, TValue?, TValue?> resolveValue
) : IObservable<TValue?>
    where TTokenHost : ITokenHost<TValue, TKey, TTokenHost>
{
    private readonly TokenBindingContext _context = context;
    private readonly Application? _application = application;
    private readonly TValue? _fallbackValue = fallbackValue;

    private readonly Func<ITokenResolver<TValue, TKey>?, ThemeVariant, AvaloniaObject?, TValue?, TValue?> _resolveValue =
        resolveValue;

    public IDisposable Subscribe(IObserver<TValue?> observer)
    {
        return new Subscription(this, observer);
    }

    private sealed class Subscription : IDisposable
    {
        private readonly TokenObservable<TValue, TKey, TTokenHost> _owner;
        private readonly IObserver<TValue?> _observer;
        private readonly TokenHostState<TValue, TKey, TTokenHost> _hostState;
        private bool _isDisposed;

        public Subscription(TokenObservable<TValue, TKey, TTokenHost> owner, IObserver<TValue?> observer)
        {
            _owner = owner;
            _observer = observer;
            _hostState = new TokenHostState<TValue, TKey, TTokenHost>(_owner._context, _owner._application);
            _hostState.Changed += OnHostStateChanged;

            Publish();
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            _hostState.Changed -= OnHostStateChanged;
            _hostState.Dispose();
        }

        private void OnHostStateChanged(object? sender, EventArgs e)
        {
            Publish();
        }

        private void Publish()
        {
            if (_isDisposed)
                return;

            var value = _owner._resolveValue(
                _hostState.Resolver,
                _hostState.ThemeVariant,
                _hostState.HostObject,
                _owner._fallbackValue
            );

            _observer.OnNext(value);
        }
    }
}