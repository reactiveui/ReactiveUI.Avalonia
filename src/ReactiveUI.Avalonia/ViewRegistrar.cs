// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
#if REACTIVE_SHIM
namespace ReactiveUI.Avalonia.Reactive;
#else
namespace ReactiveUI.Avalonia;
#endif

/// <summary>Discovers ReactiveUI view types by reflection and registers them with a dependency resolver.</summary>
/// <remarks>
/// A view is registered against <c>IViewFor&lt;TViewModel&gt;</c> for the view model it implements, under the contract
/// its view contract attribute declares. Resolution prefers the service locator and falls back to
/// <see cref="Activator"/>, so a view with no registration is still constructible.
/// </remarks>
internal static class ViewRegistrar
{
    /// <summary>Registers views with the resolver when both resolver and assemblies are available.</summary>
    /// <param name="resolver">The resolver to register into, or null when no mutable resolver is available.</param>
    /// <param name="assemblies">The assemblies to scan.</param>
    [RequiresUnreferencedCode("Scans assemblies and reflects over view types and attributes during ReactiveUI view registration.")]
    [RequiresDynamicCode("Creates closed generic view service types at runtime during ReactiveUI view registration.")]
    internal static void RegisterViews(IMutableDependencyResolver? resolver, Assembly[]? assemblies)
    {
        if (resolver is null || assemblies is null || assemblies.Length == 0)
        {
            return;
        }

        RegisterViewsInternal(resolver, assemblies);
    }

    /// <summary>Registers all non-abstract, non-interface view types implementing IViewFor{T}.</summary>
    /// <param name="resolver">The resolver to register the discovered views into.</param>
    /// <param name="assemblies">An array of assemblies to scan for view types implementing IViewFor{T}.</param>
    /// <remarks>
    /// A view contract attribute supplies the registration contract. Each view is resolved from the service locator or
    /// created through <see cref="Activator"/>. Duplicate assemblies are ignored.
    /// </remarks>
    [RequiresUnreferencedCode("Scans assemblies and reflects over view types and attributes during ReactiveUI view registration.")]
    [RequiresDynamicCode("Creates closed generic view service types at runtime during ReactiveUI view registration.")]
    internal static void RegisterViewsInternal(IMutableDependencyResolver resolver, Assembly[] assemblies)
    {
        var uniqueAssemblies = new HashSet<Assembly>();
        foreach (var assembly in assemblies)
        {
            if (!uniqueAssemblies.Add(assembly))
            {
                continue;
            }

            foreach (var viewType in assembly.GetTypes())
            {
                if (viewType.IsAbstract || viewType.IsInterface)
                {
                    continue;
                }

                var viewForInterface = FindViewForInterface(viewType);
                if (viewForInterface is null)
                {
                    continue;
                }

                var viewModelType = viewForInterface.GetGenericArguments()[0];
                var contract = GetViewContract(viewType);
                var serviceType = typeof(IViewFor<>).MakeGenericType(viewModelType);
                resolver.Register(
                    () => CreateView(viewType),
                    serviceType,
                    contract);
            }
        }
    }

    /// <summary>Finds the IViewFor{T} interface implemented by the supplied view type.</summary>
    /// <param name="viewType">The view type to inspect.</param>
    /// <returns>The matching IViewFor{T} interface, or null when the type is not a ReactiveUI view.</returns>
    [RequiresUnreferencedCode("Reads implemented interfaces from runtime view types.")]
    internal static Type? FindViewForInterface(Type viewType)
    {
        foreach (var viewInterface in viewType.GetInterfaces())
        {
            if (viewInterface.IsGenericType && viewInterface.GetGenericTypeDefinition() == typeof(IViewFor<>))
            {
                return viewInterface;
            }
        }

        return null;
    }

    /// <summary>Reads an optional ViewContract attribute contract value from the view type.</summary>
    /// <param name="viewType">The view type to inspect.</param>
    /// <returns>The view contract, or null when no contract is declared.</returns>
    [RequiresUnreferencedCode("Reads custom attributes and reflected properties from runtime view types.")]
    internal static string? GetViewContract(Type viewType)
    {
        foreach (var attribute in viewType.GetCustomAttributes(true))
        {
            var attributeType = attribute.GetType();
            if (!string.Equals(attributeType.Name, "ViewContractAttribute", StringComparison.Ordinal))
            {
                continue;
            }

            var property = attributeType.GetProperty("Contract", BindingFlags.Public | BindingFlags.Instance);
            return property?.GetValue(attribute) as string;
        }

        return null;
    }

    /// <summary>Creates or resolves an instance of the specified view type.</summary>
    /// <param name="viewType">The view type to create.</param>
    /// <returns>A resolved or newly-created view instance.</returns>
    [RequiresUnreferencedCode("Creates runtime-discovered view types by reflection.")]
    internal static object CreateView(Type viewType)
    {
        try
        {
            var resolved = AppLocator.Current.GetService(viewType);
            if (resolved is not null)
            {
                return resolved;
            }
        }
        catch (Exception error)
        {
            return CreateViewAfterResolutionFailure(viewType, error);
        }

        return CreateViewWithActivator(viewType);
    }

    /// <summary>Logs a view resolution failure and falls back to Activator creation.</summary>
    /// <param name="viewType">The view type that failed to resolve.</param>
    /// <param name="error">The service locator error.</param>
    /// <returns>The created view instance.</returns>
    internal static object CreateViewAfterResolutionFailure(Type viewType, Exception error)
    {
        LogHost.Default.Warn(error, $"Failed to resolve view type '{viewType}' from the service locator. Falling back to Activator.");
        return CreateViewWithActivator(viewType);
    }

    /// <summary>Creates a view instance through Activator.</summary>
    /// <param name="viewType">The view type to create.</param>
    /// <returns>The created view instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <c>Activator.CreateInstance(viewType)</c> is <see langword="null"/>.</exception>
    internal static object CreateViewWithActivator(Type viewType) =>
        Activator.CreateInstance(viewType)
        ?? throw new InvalidOperationException($"Failed to create view type '{viewType}'.");
}
