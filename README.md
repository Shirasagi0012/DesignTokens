# DesignTokens

A prototype for a strongly-typed, dynamic design token system for Avalonia.

This project explores managing design tokens without relying on Avalonia's default string-based resource lookup. Instead, token values are resolved via typed resolvers and updated through observable bindings. Developers can use custom markup extensions to create dynamic bindings, similar to `{DynamicResource}`.

The idea originated from my repo [MaterialColorUtilities.Avalonia](https://github.com/shirasagi0012/materialcolorutilities), where I wanted to implement strongly-typed color tokens. I discovered that by leveraging Avalonia's inherited attached properties, we can emulate a dynamic resource system. This approach supports token overrides, dynamic updates, and seamless integration with Resource Dictionaries and Templates. After the concept is proven functional, I turn this part into a generic design token system, for future projects.

## Primitives

- **Typed Token Keys:** Uses `TokenKey<TValue, TKey>` to securely identify tokens with concrete types.
- **Typed Resolvers:** Uses `ITokenResolver<TValue, TKey>` to resolve values dynamically at runtime.
- **Inheritable Context:** Leverages `TokenHost<TValue, TKey>.Resolver` as an attached property, allowing resolvers to flow naturally down the visual tree, which is similar to the behavior of resource.
- **Bindings:** Uses `TokenBinding` to create bindings that react to resolver or theme changes.
- **Markup Extension:** Provides a generic markup extension `TokenExtensionBase<TValue, TKey>` that functions almost identically to Avalonia's `{DynamicResource}`.

## How It Works

1. Define a strongly-typed token key for a specific value.
2. Subtype `TokenHost<TValue, TKey>`, `TokenExtensionBase<TValue, TKey>` and implements `ITokenResolver<TValue, TKey>`. This is necessary because Avalonia lacks support for instantiating generic markup extensions or referencing generic attached properties.
3. Attach a resolver to the `Application` or a specific subtree root.
4. Bind to tokens using the custom markup extension instead of `StaticResource` / `DynamicResource`.
5. The system automatically re-evaluates bindings when the active resolver or theme variant changes.