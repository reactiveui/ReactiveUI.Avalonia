// Copyright (c) 2025 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Avalonia;

/// <summary>
/// Provides extension methods for configuring Avalonia applications to use ReactiveUI, including integration with
/// custom dependency injection containers and automatic view registration.
/// </summary>
/// <remarks>These extension methods simplify the setup of ReactiveUI in Avalonia applications by enabling
/// automatic registration of views, integration with dependency injection containers, and configuration of ReactiveUI
/// services. Methods in this class should be called during application startup, typically in the AppBuilder
/// configuration pipeline, to ensure proper initialization before platform services are set up.</remarks>
public static class AppBuilderExtensions
{
    /// <summary>
    /// Configures the application to use ReactiveUI with Avalonia by registering required services and allowing
    /// additional customization via a builder callback.
    /// </summary>
    /// <remarks>This method sets up core ReactiveUI services for Avalonia, including activation, property
    /// binding, and command binding. The provided callback allows registering additional services or modifying the
    /// ReactiveUI configuration before the application is built.</remarks>
    /// <param name="builder">The application builder to configure. Cannot be null.</param>
    /// <param name="withReactiveUIBuilder">A callback that receives a ReactiveUI builder for further customization. Cannot be null.</param>
    /// <returns>The application builder instance, enabling further configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="builder"/> or <paramref name="withReactiveUIBuilder"/> is null.</exception>
    public static AppBuilder UseReactiveUI(this AppBuilder builder, Action<ReactiveUIBuilder> withReactiveUIBuilder)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (withReactiveUIBuilder is null)
        {
            throw new ArgumentNullException(nameof(withReactiveUIBuilder));
        }

        return builder.AfterPlatformServicesSetup(_ =>
        {
            var rxuiBuilder = AppLocator.CurrentMutable.CreateReactiveUIBuilder();
            rxuiBuilder
                .WithMainThreadScheduler(AvaloniaScheduler.Instance)
                .WithRegistration(splat =>
                {
                    splat.RegisterConstant<IActivationForViewFetcher>(new AvaloniaActivationForViewFetcher());
                    splat.RegisterConstant<IPropertyBindingHook>(new AutoDataTemplateBindingHook());
                    splat.RegisterConstant<ICreatesCommandBinding>(new AvaloniaCreatesCommandBinding());
                    splat.RegisterConstant<ICreatesObservableForProperty>(new AvaloniaObjectObservableForProperty());
                });

            withReactiveUIBuilder(rxuiBuilder);

            if (!Splat.Builder.AppBuilder.HasBeenBuilt)
            {
                rxuiBuilder.BuildApp();
            }
        });
    }

    /// <summary>
    /// Registers ReactiveUI view types from the specified assemblies with the application's dependency resolver during
    /// platform services setup.
    /// </summary>
    /// <remarks>This method should be called before the application is started to ensure that ReactiveUI
    /// views are available for dependency resolution. Views are registered after platform services have been set up,
    /// allowing for proper integration with the application's lifecycle.</remarks>
    /// <param name="builder">The application builder to configure. Cannot be null.</param>
    /// <param name="assemblies">An array of assemblies containing ReactiveUI view types to register. If null or empty, no views are registered.</param>
    /// <returns>The application builder instance, enabling further configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="builder"/> is null.</exception>
    public static AppBuilder RegisterReactiveUIViews(this AppBuilder builder, params Assembly[] assemblies)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.AfterPlatformServicesSetup(_ =>
        {
            var resolver = AppLocator.CurrentMutable;
            if (resolver is null)
            {
                return;
            }

            if (assemblies is null || assemblies.Length == 0)
            {
                return;
            }

            RegisterViewsInternal(resolver, assemblies);
        });
    }

    /// <summary>
    /// Registers all ReactiveUI view types found in the assembly containing the specified marker type with the
    /// application builder.
    /// </summary>
    /// <remarks>This method scans the assembly of <typeparamref name="TMarker"/> for ReactiveUI view types
    /// and registers them for use within the application. Use this method to simplify view registration when working
    /// with assemblies that contain ReactiveUI views.</remarks>
    /// <typeparam name="TMarker">The type used to identify the assembly from which ReactiveUI views will be registered.</typeparam>
    /// <param name="builder">The application builder to configure with ReactiveUI view registrations.</param>
    /// <returns>The same <see cref="AppBuilder"/> instance, enabling fluent configuration.</returns>
    public static AppBuilder RegisterReactiveUIViewsFromAssemblyOf<TMarker>(this AppBuilder builder)
        => RegisterReactiveUIViews(builder, typeof(TMarker).Assembly);

    /// <summary>
    /// Registers all ReactiveUI view types found in the application's entry assembly with the specified application
    /// builder.
    /// </summary>
    /// <remarks>This method is typically used during application startup to automatically discover and
    /// register ReactiveUI views from the entry assembly. If the entry assembly cannot be determined, no views are
    /// registered and the builder is returned unchanged.</remarks>
    /// <param name="builder">The application builder to configure with ReactiveUI view registrations.</param>
    /// <returns>The same application builder instance, with ReactiveUI views registered if the entry assembly is available;
    /// otherwise, the original builder.</returns>
    public static AppBuilder RegisterReactiveUIViewsFromEntryAssembly(this AppBuilder builder)
    {
        var entry = Assembly.GetEntryAssembly();
        return entry is null ? builder : RegisterReactiveUIViews(builder, entry);
    }

    /// <summary>
    /// Configures the application to use ReactiveUI with a custom dependency injection container and dependency
    /// resolver.
    /// </summary>
    /// <remarks>This method integrates ReactiveUI into the Avalonia application and sets up a custom
    /// dependency injection container for service registration and resolution. It should be called during application
    /// startup before platform services are initialized. The provided container and dependency resolver will be
    /// registered with the application's service locator.</remarks>
    /// <typeparam name="TContainer">The type of the dependency injection container to be used for service registration and resolution.</typeparam>
    /// <param name="builder">The application builder used to configure the Avalonia application.</param>
    /// <param name="containerFactory">A factory function that creates an instance of the dependency injection container.</param>
    /// <param name="containerConfig">An action that configures the dependency injection container after it has been created.</param>
    /// <param name="dependencyResolverFactory">A function that creates an IDependencyResolver from the dependency injection container.</param>
    /// <param name="configureReactiveUI">An action that configures ReactiveUI options and services.</param>
    /// <returns>The application builder instance, configured to use ReactiveUI with the specified dependency injection container
    /// and resolver.</returns>
    /// <exception cref="ArgumentNullException">Thrown if builder, containerFactory, containerConfig, or dependencyResolverFactory is null.</exception>
    public static AppBuilder UseReactiveUIWithDIContainer<TContainer>(
        this AppBuilder builder,
        Func<TContainer> containerFactory,
        Action<TContainer> containerConfig,
        Func<TContainer, IDependencyResolver> dependencyResolverFactory,
        Action<ReactiveUIBuilder> configureReactiveUI)
        where TContainer : class =>
            builder switch
            {
                null => throw new ArgumentNullException(nameof(builder)),
                _ => builder.UseReactiveUI(configureReactiveUI).AfterPlatformServicesSetup(_ =>
                {
                    if (AppLocator.CurrentMutable is null)
                    {
                        return;
                    }

#if NETSTANDARD
                    if (containerFactory is null)
                    {
                        throw new ArgumentNullException(nameof(containerFactory));
                    }

                    if (containerConfig is null)
                    {
                        throw new ArgumentNullException(nameof(containerConfig));
                    }

                    if (dependencyResolverFactory is null)
                    {
                        throw new ArgumentNullException(nameof(dependencyResolverFactory));
                    }
#else
                    ArgumentNullException.ThrowIfNull(containerFactory);
                    ArgumentNullException.ThrowIfNull(containerConfig);
                    ArgumentNullException.ThrowIfNull(dependencyResolverFactory);
#endif

                    var container = containerFactory();
                    AppLocator.CurrentMutable.RegisterConstant(container);
                    var dependencyResolver = dependencyResolverFactory(container);
                    AppLocator.SetLocator(dependencyResolver);
                    RxSchedulers.MainThreadScheduler = AvaloniaScheduler.Instance;
                    containerConfig(container);
                })
            };

    /// <summary>
    /// Registers all non-abstract, non-interface view types implementing IViewFor{T} from the specified assemblies with
    /// the provided dependency resolver.
    /// </summary>
    /// <remarks>If a view type is decorated with a ViewContract attribute, its contract value is used during
    /// registration. Each view type is registered with the resolver using either an existing service instance or a new
    /// instance created via Activator. Duplicate assemblies are ignored.</remarks>
    /// <param name="resolver">The dependency resolver in which the discovered view types will be registered.</param>
    /// <param name="assemblies">An array of assemblies to scan for view types implementing IViewFor{T}.</param>
    private static void RegisterViewsInternal(IMutableDependencyResolver resolver, Assembly[] assemblies)
    {
        foreach (var asm in assemblies.Distinct())
        {
            foreach (var viewType in asm.GetTypes().Where(t => !t.IsAbstract && !t.IsInterface))
            {
                var iViewFor = viewType.GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IViewFor<>));
                if (iViewFor is null)
                {
                    continue;
                }

                var viewModelType = iViewFor.GetGenericArguments()[0];

                // If ViewContract attribute is present, honor it.
                string? contract = null;
                var attr = viewType.GetCustomAttributes(true)
                    .FirstOrDefault(a => string.Equals(a.GetType().Name, "ViewContractAttribute", StringComparison.Ordinal));
                if (attr is not null)
                {
                    var prop = attr.GetType().GetProperty("Contract", BindingFlags.Public | BindingFlags.Instance);
                    if (prop is not null)
                    {
                        contract = prop.GetValue(attr) as string;
                    }
                }

                var serviceType = typeof(IViewFor<>).MakeGenericType(viewModelType);
                resolver.Register(
                    () =>
                    {
                        try
                        {
                            var resolved = AppLocator.Current.GetService(viewType);
                            if (resolved is not null)
                            {
                                return resolved;
                            }
                        }
                        catch
                        {
                            // Ignore and fall back to Activator.
                        }

                        return Activator.CreateInstance(viewType)!;
                    },
                    serviceType,
                    contract);
            }
        }
    }
}
