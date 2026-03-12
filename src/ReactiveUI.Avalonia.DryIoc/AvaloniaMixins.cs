// Copyright (c) 2019-2026 ReactiveUI and Avalonia Teams, and Contributors. All rights reserved.
// Licensed under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive;
using System.Reactive.Concurrency;
using DryIoc;
using ReactiveUI.Builder;
using Splat;
using Splat.Builder;
using AppBuilder = Avalonia.AppBuilder;
using SplatBuilder = Splat.Builder.AppBuilder;

namespace ReactiveUI.Avalonia.Splat;

/// <summary>
/// Provides extension methods for configuring Avalonia applications to use ReactiveUI with DryIoc as the dependency
/// injection container.
/// </summary>
public static class AvaloniaMixins
{
    /// <summary>
    /// Configures the application to use ReactiveUI with DryIoc as the dependency injection container.
    /// </summary>
    /// <remarks>This method integrates DryIoc with ReactiveUI, allowing services and dependencies to be
    /// registered using DryIoc. The provided <paramref name="containerConfig"/> delegate can be used to register
    /// application-specific services. If additional ReactiveUI configuration is required, supply the <paramref
    /// name="withReactiveUIBuilder"/> delegate.</remarks>
    /// <param name="builder">The application builder used to configure the app pipeline. Cannot be null.</param>
    /// <param name="containerConfig">A delegate that configures the DryIoc container. This is called after the container is created and registered.</param>
    /// <param name="withReactiveUIBuilder">An optional delegate to further configure the ReactiveUI builder. If provided, it is invoked after DryIoc
    /// integration.</param>
    /// <returns>The application builder instance, configured to use ReactiveUI with DryIoc.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="builder"/> or <paramref name="containerConfig"/> is null.</exception>
    public static AppBuilder UseReactiveUIWithDryIoc(this AppBuilder builder, Action<Container> containerConfig, Action<ReactiveUIBuilder>? withReactiveUIBuilder = null) =>
        builder switch
        {
            null => throw new ArgumentNullException(nameof(builder)),
            _ => builder.AfterPlatformServicesSetup(_ =>
            {
                ArgumentNullException.ThrowIfNull(containerConfig);

                var container = new Container();
                var module = new DryIocSplatModule(container);
                module.Configure(default!);
                AppLocator.CurrentMutable.RegisterConstant(container);
                containerConfig(container);

                // Create the ReactiveUI builder and register Avalonia-specific services for view activation, property binding, command binding, and observable properties using AutoFac.
                // This ensures that ReactiveUI can properly interact with Avalonia's view lifecycle and data binding mechanisms.
                var rxuiBuilder = AppLocator.CurrentMutable.CreateReactiveUIBuilder();

                // Configure the default schedulers for ReactiveUI and register Avalonia-specific implementations for view activation, property binding hooks, command binding, and observable properties.
                // This setup allows ReactiveUI to work seamlessly with Avalonia's UI framework and ensures that ReactiveUI's features are properly integrated into the Avalonia application.
                rxuiBuilder.WithAvalonia();

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
