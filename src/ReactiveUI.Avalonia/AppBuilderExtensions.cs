// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Runtime.CompilerServices;

#if REACTIVE_SHIM
namespace ReactiveUI.Avalonia.Reactive;
#else
namespace ReactiveUI.Avalonia;
#endif

/// <summary>Provides extension methods for configuring Avalonia applications to use ReactiveUI.</summary>
/// <remarks>
/// These extensions configure view registration, dependency injection integration, and ReactiveUI services for an
/// Avalonia application. Call them during application startup, before platform services finish initialization.
/// </remarks>
public static class AppBuilderExtensions
{
    /// <summary>Extends Avalonia application builders.</summary>
    /// <param name="builder">The Avalonia application builder to extend.</param>
    extension(AppBuilder builder)
    {
        /// <summary>Configures the application to use ReactiveUI with Avalonia.</summary>
        /// <param name="withReactiveUIBuilder">
        /// A callback that receives a ReactiveUI builder for further customization.
        /// </param>
        /// <returns>The application builder instance, enabling further configuration.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the builder or <paramref name="withReactiveUIBuilder"/> is null.</exception>
        /// <remarks>
        /// This method sets up activation, property binding, and command binding. The callback can register additional
        /// services or modify the ReactiveUI configuration before the application is built.
        /// </remarks>
        public AppBuilder UseReactiveUI(Action<ReactiveUIBuilder> withReactiveUIBuilder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(withReactiveUIBuilder);

            return builder.AfterPlatformServicesSetup(_ => StartupConfiguration.ConfigureReactiveUI(withReactiveUIBuilder));
        }

        /// <summary>Registers ReactiveUI view types from the specified assemblies.</summary>
        /// <param name="assemblies">Assemblies containing ReactiveUI view types to register.</param>
        /// <returns>The application builder instance, enabling further configuration.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the builder is null.</exception>
        /// <remarks>
        /// Call this before the application starts so ReactiveUI views are available for dependency resolution.
        /// Registration runs after platform services have been set up.
        /// </remarks>
        [RequiresUnreferencedCode("Scans assemblies and reflects over view types and attributes during ReactiveUI view registration.")]
        [RequiresDynamicCode("Creates closed generic view service types at runtime during ReactiveUI view registration.")]
        public AppBuilder RegisterReactiveUIViews(params Assembly[] assemblies)
        {
            ArgumentNullException.ThrowIfNull(builder);

            return builder.AfterPlatformServicesSetup(platformBuilder =>
                ViewRegistrar.RegisterViews(AppLocator.CurrentMutable, assemblies));
        }

        /// <summary>Registers views found in the assembly containing the specified marker type.</summary>
        /// <typeparam name="TMarker">The type identifying the assembly to scan.</typeparam>
        /// <param name="markers">Optional marker values retained for source-compatible generic calls.</param>
        /// <returns>The same <see cref="AppBuilder"/> instance, enabling fluent configuration.</returns>
        /// <remarks>
        /// This method scans the assembly of <typeparamref name="TMarker"/> and registers its ReactiveUI views.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [RequiresUnreferencedCode("Scans assemblies and reflects over view types and attributes during ReactiveUI view registration.")]
        [RequiresDynamicCode("Creates closed generic view service types at runtime during ReactiveUI view registration.")]
        public AppBuilder RegisterReactiveUIViewsFromAssemblyOf<TMarker>(params TMarker[] markers) =>
            builder.RegisterReactiveUIViews(typeof(TMarker).Assembly);

        /// <summary>Registers all ReactiveUI view types found in the application's entry assembly.</summary>
        /// <returns>The same builder, with entry-assembly views registered when an entry assembly is available.</returns>
        /// <remarks>
        /// The entry assembly is discovered during application startup. When it is unavailable, no views are registered.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [RequiresUnreferencedCode("Scans assemblies and reflects over view types and attributes during ReactiveUI view registration.")]
        [RequiresDynamicCode("Creates closed generic view service types at runtime during ReactiveUI view registration.")]
        public AppBuilder RegisterReactiveUIViewsFromEntryAssembly() =>
            builder.RegisterReactiveUIViewsFromEntryAssembly(Assembly.GetEntryAssembly());

        /// <summary>Configures ReactiveUI with a custom dependency injection container and resolver.</summary>
        /// <typeparam name="TContainer">The dependency injection container type.</typeparam>
        /// <param name="containerFactory">Creates the dependency injection container.</param>
        /// <param name="containerConfig">Configures the created container.</param>
        /// <param name="dependencyResolverFactory">Creates a resolver from the container.</param>
        /// <param name="configureReactiveUI">An action that configures ReactiveUI options and services.</param>
        /// <returns>The application builder configured with the requested container.</returns>
        /// <exception cref="ArgumentNullException">A required argument is <see langword="null"/>.</exception>
        /// <remarks>
        /// Call this during startup before platform services initialize. The container and dependency resolver are
        /// registered with the application's service locator.
        /// </remarks>
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
                    StartupConfiguration.ConfigureReactiveUIDIContainer(
                        AppLocator.CurrentMutable,
                        containerFactory,
                        containerConfig,
                        dependencyResolverFactory);
                });
        }

        /// <summary>Registers views from the supplied entry assembly when one is available.</summary>
        /// <param name="entryAssembly">The entry assembly to scan, or null when no entry assembly is available.</param>
        /// <returns>The same application builder instance.</returns>
        [RequiresUnreferencedCode("Scans assemblies and reflects over view types and attributes during ReactiveUI view registration.")]
        [RequiresDynamicCode("Creates closed generic view service types at runtime during ReactiveUI view registration.")]
        internal AppBuilder RegisterReactiveUIViewsFromEntryAssembly(Assembly? entryAssembly)
        {
            ArgumentNullException.ThrowIfNull(builder);

            return entryAssembly is null ? builder : builder.RegisterReactiveUIViews(entryAssembly);
        }
    }

    /// <summary>Extends ReactiveUI builders with Avalonia registrations.</summary>
    /// <param name="builder">The ReactiveUI builder to extend.</param>
    extension(IReactiveUIBuilder builder)
    {
        /// <summary>Configures the specified ReactiveUI builder to use Avalonia-specific implementations.</summary>
        /// <returns>The configured IReactiveUIBuilder instance with Avalonia support enabled.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the builder parameter is null.</exception>
        /// <remarks>
        /// This sets the main-thread and task-pool schedulers and registers Avalonia command-binding and property
        /// observation services.
        /// </remarks>
        public IReactiveUIBuilder WithAvalonia()
        {
            ArgumentNullException.ThrowIfNull(builder);

            return builder
                .WithMainThreadScheduler(AvaloniaScheduler.Instance)
                .WithTaskPoolScheduler(TaskPoolScheduler.Default)
                .WithRegistration(static splat =>
                {
                    splat.RegisterConstant<IActivationForViewFetcher>(new AvaloniaActivationForViewFetcher());
                    splat.RegisterConstant<IPropertyBindingHook>(new AutoDataTemplateBindingHook());
                    splat.RegisterConstant<ICreatesCommandBinding>(new AvaloniaCreatesCommandBinding());
                    splat.RegisterConstant<ICreatesObservableForProperty>(new AvaloniaObjectObservableForProperty());
                }).WithSuspensionHost<Unit>();
        }
    }
}
