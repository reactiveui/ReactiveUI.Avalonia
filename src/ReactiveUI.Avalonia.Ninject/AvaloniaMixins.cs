// Copyright (c) 2019-2026 ReactiveUI and Avalonia Teams, and Contributors. All rights reserved.
// Licensed under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive;
using System.Reactive.Concurrency;
using Ninject;
using ReactiveUI.Builder;
using Splat;
using Splat.Builder;
using AppBuilder = Avalonia.AppBuilder;
using SplatBuilder = Splat.Builder.AppBuilder;

namespace ReactiveUI.Avalonia.Splat;

/// <summary>
/// Provides extension methods for integrating ReactiveUI with Ninject in Avalonia applications.
/// </summary>
/// <remarks>This static class contains mixin methods that enable the configuration of dependency injection using
/// Ninject alongside ReactiveUI in Avalonia app builders. These methods are intended to simplify setup and promote
/// modular application architecture.</remarks>
public static class AvaloniaMixins
{
    /// <summary>
    /// Configures the application to use ReactiveUI with a Ninject dependency injection container, allowing custom
    /// container setup and optional ReactiveUI builder configuration.
    /// </summary>
    /// <remarks>This method integrates Ninject as the dependency injection container for ReactiveUI
    /// applications. The Ninject container is registered with the application's service locator, and the provided
    /// configuration delegate allows customization of container bindings. Additional ReactiveUI builder configuration
    /// can be performed via the optional delegate.</remarks>
    /// <param name="builder">The application builder used to configure the app pipeline. Cannot be null.</param>
    /// <param name="containerConfig">A delegate that configures the Ninject container. This is called after the container is created and registered.</param>
    /// <param name="withReactiveUIBuilder">An optional delegate to further configure the ReactiveUI builder. If provided, it is invoked after the Ninject
    /// container is set up.</param>
    /// <returns>The application builder instance with ReactiveUI and Ninject configured.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="builder"/> is null, or if <paramref name="containerConfig"/> is null.</exception>
    public static AppBuilder UseReactiveUIWithNinject(this AppBuilder builder, Action<StandardKernel> containerConfig, Action<ReactiveUIBuilder>? withReactiveUIBuilder = null) =>
        builder switch
        {
            null => throw new ArgumentNullException(nameof(builder)),
            _ => builder.AfterPlatformServicesSetup(_ =>
            {
                ArgumentNullException.ThrowIfNull(containerConfig);

                var container = new StandardKernel();
                var module = new NinjectSplatModule(container);
                module.Configure(default!);
                AppLocator.CurrentMutable.RegisterConstant(container);
                containerConfig(container);

                // Create the ReactiveUI builder and register Avalonia-specific services for view activation, property binding, command binding, and observable properties using AutoFac.
                // This ensures that ReactiveUI can properly interact with Avalonia's view lifecycle and data binding mechanisms.
                var rxuiBuilder = AppLocator.CurrentMutable.CreateReactiveUIBuilder();

                // Configure the default schedulers for ReactiveUI and register Avalonia-specific implementations for view activation, property binding hooks, command binding, and observable properties.
                // This setup allows ReactiveUI to work seamlessly with Avalonia's UI framework and ensures that ReactiveUI's features are properly integrated into the Avalonia application.
                rxuiBuilder
                    .WithMainThreadScheduler(AvaloniaScheduler.Instance)
                    .WithTaskPoolScheduler(TaskPoolScheduler.Default)
                    .WithRegistration(splat =>
                    {
                        splat.RegisterConstant<IActivationForViewFetcher>(new AvaloniaActivationForViewFetcher());
                        splat.RegisterConstant<IPropertyBindingHook>(new AutoDataTemplateBindingHook());
                        splat.RegisterConstant<ICreatesCommandBinding>(new AvaloniaCreatesCommandBinding());
                        splat.RegisterConstant<ICreatesObservableForProperty>(new AvaloniaObjectObservableForProperty());
                    }).WithSuspensionHost<Unit>();

                // Allow additional configuration of the ReactiveUI builder through the optional delegate, enabling further customization of ReactiveUI's behavior and integration with Avalonia.
                if (withReactiveUIBuilder is not null)
                {
                    withReactiveUIBuilder(rxuiBuilder);
                }

                if (!SplatBuilder.HasBeenBuilt)
                {
                    rxuiBuilder.BuildApp();
                }
            })
        };
}
