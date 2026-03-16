namespace DesignTokens;

/// <summary>
/// Identifies a token that resolves to <typeparamref name="TValue" />.
/// </summary>
/// <typeparam name="TValue">The resolved token value type.</typeparam>
/// <typeparam name="TKey">The token key type.</typeparam>
public sealed class TokenKey<TValue, TKey>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TokenKey{TValue,TKey}" /> class.
    /// </summary>
    public TokenKey()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenKey{TValue,TKey}" /> class with
    /// implementation-defined key data.
    /// </summary>
    /// <param name="value">Opaque key data understood by the resolver.</param>
    public TokenKey(TKey? value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the opaque key payload carried by this token key.
    /// </summary>
    public TKey? Value { get; }
}