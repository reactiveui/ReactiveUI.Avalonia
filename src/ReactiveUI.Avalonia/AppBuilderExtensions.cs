// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
#if REACTIVE_SHIM
namespace ReactiveUI.Avalonia.Reactive;
#else
namespace ReactiveUI.Avalonia;
#endif

/// <summary>Provides extension methods for configuring Avalonia applications to use ReactiveUI.</summary>
/// <remarks>These extension methods simplify the setup of ReactiveUI in Avalonia applications by enabling automatic registration of views, integration with dependency injection containers, and configuration of ReactiveUI services. Methods in this class should be called during application startup, typically in the AppBuilder configuration pipeline, to ensure proper initialization before platform services are set up.</remarks>
public static class AppBuilderExtensions
{
    /// <summary>Extends Avalonia application builders.</summary>
    /// <param name="builder">The Avalonia application builder to extend.</param>
    extension(AppBuilder builder)
    {
    /// <summary>Configures the application to use ReactiveUI with Avalonia.</summary>
    /// <remarks>This method sets up core ReactiveUI services for Avalonia, including activation, property binding, and command binding. The provided callback allows registering additional services or modifying the ReactiveUI configuration before the application is built.</remarks>
    /// <param name="withReactiveUIBuilder">A callback that receives a ReactiveUI builder for further customization. Cannot be null.</param>
    /// <returns>The application builder instance, enabling further configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the builder or <paramref name="withReactiveUIBuilder"/> is null.</exception>
    public AppBuilder UseReactiveUI(Action<ReactiveUIBuilder> withReactiveUIBuilder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(withReactiveUIBuilder);

            return builder.AfterPlatformServicesSetup(platformBuilder =>
            {
                var rxuiBuilder = RxAppBuilder.CreateReactiveUIBuilder();
                _ = rxuiBuilder.WithAvalonia();

                withReactiveUIBuilder(rxuiBuilder);

                if (Splat.Builder.AppBuilder.HasBeenBuilt)
                {
                    return;
                }

                _ = rxuiBuilder.BuildApp();
            });
        }

    /// <summary>Registers ReactiveUI view types from the specified assemblies.</summary>
    /// <remarks>This method should be called before the application is started to ensure that ReactiveUI views are available for dependency resolution. Views are registered after platform services have been set up, allowing for proper integration with the application's lifecycle.</remarks>
    /// <param name="assemblies">An array of assemblies containing ReactiveUI view types to register. If null or empty, no views are registered.</param>
    /// <returns>The application builder instance, enabling further configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the builder is null.</exception>
    [RequiresUnreferencedCode("Scans assemblies and reflects over view types and attributes during ReactiveUI view registration.")]
    [RequiresDynamicCode("Creates closed generic view service types at runtime during ReactiveUI view registration.")]
    public AppBuilder RegisterReactiveUIViews(params Assembly[] assemblies)
        {
            ArgumentNullException.ThrowIfNull(builder);

            return builder.AfterPlatformServicesSetup(platformBuilder =>
                RegisterReactiveUIViews(AppLocator.CurrentMutable, assemblies));
        }

    /// <summary>Registers all ReactiveUI view types found in the assembly containing the specified marker type.</summary>
    /// <remarks>This method scans the assembly of <typeparamref name="TMarker"/> for ReactiveUI view types and registers them for use within the application. Use this method to simplify view registration when working with assemblies that contain ReactiveUI views.</remarks>
    /// <typeparam name="TMarker">The type used to identify the assembly from which ReactiveUI views will be registered.</typeparam>
    /// <returns>The same <see cref="AppBuilder"/> instance, enabling fluent configuration.</returns>
    [RequiresUnreferencedCode("Scans assemblies and reflects over view types and attributes during ReactiveUI view registration.")]
    [RequiresDynamicCode("Creates closed generic view service types at runtime during ReactiveUI view registration.")]
    public AppBuilder RegisterReactiveUIViewsFromAssemblyOf<TMarker>()
        {
            return builder.RegisterReactiveUIViews(typeof(TMarker).Assembly);
        }

    /// <summary>Registers all ReactiveUI view types found in the application's entry assembly.</summary>
    /// <remarks>This method is typically used during application startup to automatically discover and register ReactiveUI views from the entry assembly. If the entry assembly cannot be determined, no views are registered and the builder is returned unchanged.</remarks>
    /// <returns>The same application builder instance, with ReactiveUI views registered if the entry assembly is available; otherwise, the original builder.</returns>
    [RequiresUnreferencedCode("Scans assemblies and reflects over view types and attributes during ReactiveUI view registration.")]
    [RequiresDynamicCode("Creates closed generic view service types at runtime during ReactiveUI view registration.")]
    public AppBuilder RegisterReactiveUIViewsFromEntryAssembly()
        {
            return RegisterReactiveUIViewsFromEntryAssembly(builder, Assembly.GetEntryAssembly());
        }

    /// <summary>Configures the application to use ReactiveUI with a custom dependency injection container and dependency resolver.</summary>
    /// <remarks>This method integrates ReactiveUI into the Avalonia application and sets up a custom dependency injection container for service registration and resolution. It should be called during application startup before platform services are initialized. The provided container and dependency resolver will be registered with the application's service locator.</remarks>
    /// <typeparam name="TContainer">The type of the dependency injection container to be used for service registration and resolution.</typeparam>
    /// <param name="containerFactory">A factory function that creates an instance of the dependency injection container.</param>
    /// <param name="containerConfig">An action that configures the dependency injection container after it has been created.</param>
    /// <param name="dependencyResolverFactory">A function that creates an IDependencyResolver from the dependency injection container.</param>
    /// <param name="configureReactiveUI">An action that configures ReactiveUI options and services.</param>
    /// <returns>The application builder instance, configured to use ReactiveUI with the specified dependency injection container and resolver.</returns>
    /// <exception cref="ArgumentNullException">Thrown if builder, containerFactory, containerConfig, or dependencyResolverFactory is null.</exception>
    public AppBuilder UseReactiveUIWithDIContainer<TContainer>(
            Func<TContainer> containerFactory,
            Action<TContainer> containerConfig,
            Func<TContainer, IDependencyResolver> dependencyResolverFactory,
            Action<ReactiveUIBuilder> configureReactiveUI)
            where TContainer : class
        {
            ArgumentNullException.ThrowIfNull(builder);

            return builder.UseReactiveUI(configureReactiveUI)
                .AfterPlatformServicesSetup(platformBuilder =>
                {
                    ConfigureReactiveUIDIContainer(
                        AppLocator.CurrentMutable,
                        containerFactory,
                        containerConfig,
                        dependencyResolverFactory);
                });
        }
    }

    /// <summary>Extends ReactiveUI builders with Avalonia registrations.</summary>
    /// <param name="builder">The ReactiveUI builder to extend.</param>
    extension(IReactiveUIBuilder builder)
    {

    /// <summary>Configures the specified ReactiveUI builder to use Avalonia-specific implementations.</summary>
    /// <remarks>This method sets up the main thread scheduler, task pool scheduler, and registers Avalonia-specific services for command binding and property observation.</remarks>
    /// <returns>The configured IReactiveUIBuilder instance with Avalonia support enabled.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the builder parameter is null.</exception>
    public IReactiveUIBuilder WithAvalonia()
        {
            ArgumentNullException.ThrowIfNull(builder);

            return builder
                .WithMainThreadScheduler(AvaloniaScheduler.Instance)
                .WithTaskPoolScheduler(TaskPoolScheduler.Default)
                .WithRegistration(splat =>
                {
                    splat.RegisterConstant<IActivationForViewFetcher>(new AvaloniaActivationForViewFetcher());
                    splat.RegisterConstant<IPropertyBindingHook>(new AutoDataTemplateBindingHook());
                    splat.RegisterConstant<ICreatesCommandBinding>(new AvaloniaCreatesCommandBinding());
                    splat.RegisterConstant<ICreatesObservableForProperty>(new AvaloniaObjectObservableForProperty());
                }).WithSuspensionHost<Unit>();
        }
    }

    /// <summary>Configures ReactiveUI with a dependency injection container when a mutable resolver is available.</summary>
    /// <typeparam name="TContainer">The dependency injection container type.</typeparam>
    /// <param name="resolver">The mutable resolver used for container registration.</param>
    /// <param name="containerFactory">The factory used to create the container.</param>
    /// <param name="containerConfig">The configuration action for the container.</param>
    /// <param name="dependencyResolverFactory">The factory used to create the dependency resolver.</param>
    private static void ConfigureReactiveUIDIContainer<TContainer>(
        IMutableDependencyResolver? resolver,
        Func<TContainer> containerFactory,
        Action<TContainer> containerConfig,
        Func<TContainer, IDependencyResolver> dependencyResolverFactory)
        where TContainer : class
    {
        if (resolver is null)
        {
            return;
        }

        ArgumentNullException.ThrowIfNull(containerFactory, nameof(containerFactory));
        ArgumentNullException.ThrowIfNull(containerConfig, nameof(containerConfig));
        ArgumentNullException.ThrowIfNull(dependencyResolverFactory, nameof(dependencyResolverFactory));

        var container = containerFactory();
        resolver.RegisterConstant(container);
        var dependencyResolver = dependencyResolverFactory(container);
        AppLocator.SetLocator(dependencyResolver);
        RxSchedulers.MainThreadScheduler = AvaloniaScheduler.Instance;
        containerConfig(container);
    }

    /// <summary>Registers views from the supplied entry assembly when one is available.</summary>
    /// <param name="builder">The Avalonia application builder to extend.</param>
    /// <param name="entryAssembly">The entry assembly to scan, or null when no entry assembly is available.</param>
    /// <returns>The same application builder instance.</returns>
    [RequiresUnreferencedCode("Scans assemblies and reflects over view types and attributes during ReactiveUI view registration.")]
    [RequiresDynamicCode("Creates closed generic view service types at runtime during ReactiveUI view registration.")]
    private static AppBuilder RegisterReactiveUIViewsFromEntryAssembly(AppBuilder builder, Assembly? entryAssembly)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return entryAssembly is null ? builder : builder.RegisterReactiveUIViews(entryAssembly);
    }

    /// <summary>Registers views with the resolver when both resolver and assemblies are available.</summary>
    /// <param name="resolver">The resolver to register views into.</param>
    /// <param name="assemblies">The assemblies to scan.</param>
    [RequiresUnreferencedCode("Scans assemblies and reflects over view types and attributes during ReactiveUI view registration.")]
    [RequiresDynamicCode("Creates closed generic view service types at runtime during ReactiveUI view registration.")]
    private static void RegisterReactiveUIViews(IMutableDependencyResolver? resolver, Assembly[]? assemblies)
    {
        if (resolver is null || assemblies is null || assemblies.Length == 0)
        {
            return;
        }

        RegisterViewsInternal(resolver, assemblies);
    }

    /// <summary>Registers all non-abstract, non-interface view types implementing IViewFor{T}.</summary>
    /// <remarks>If a view type is decorated with a ViewContract attribute, its contract value is used during registration. Each view type is registered with the resolver using either an existing service instance or a new instance created via Activator. Duplicate assemblies are ignored.</remarks>
    /// <param name="resolver">The dependency resolver in which the discovered view types will be registered.</param>
    /// <param name="assemblies">An array of assemblies to scan for view types implementing IViewFor{T}.</param>
    [RequiresUnreferencedCode("Scans assemblies and reflects over view types and attributes during ReactiveUI view registration.")]
    [RequiresDynamicCode("Creates closed generic view service types at runtime during ReactiveUI view registration.")]
    private static void RegisterViewsInternal(IMutableDependencyResolver resolver, Assembly[] assemblies)
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
    private static Type? FindViewForInterface(Type viewType)
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
    private static string? GetViewContract(Type viewType)
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
    private static object CreateView(Type viewType)
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
    /// <param name="viewType">The view type to create.</param>
    /// <param name="error">The service locator error.</param>
    /// <returns>The created view instance.</returns>
    private static object CreateViewAfterResolutionFailure(Type viewType, Exception error)
    {
        LogHost.Default.Warn(error, $"Failed to resolve view type '{viewType}' from the service locator. Falling back to Activator.");
        return CreateViewWithActivator(viewType);
    }

    /// <summary>Creates a view instance through Activator.</summary>
    /// <param name="viewType">The view type to create.</param>
    /// <returns>The created view instance.</returns>
    private static object CreateViewWithActivator(Type viewType)
    {
        return Activator.CreateInstance(viewType)
            ?? throw new InvalidOperationException($"Failed to create view type '{viewType}'.");
    }
}
