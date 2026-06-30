// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
#if REACTIVE_SHIM
namespace ReactiveUI.Avalonia.Reactive.Splat;
#else
namespace ReactiveUI.Avalonia.Splat;
#endif

/// <summary>Provides extension methods for configuring Avalonia applications to use ReactiveUI with Autofac.</summary>
/// <remarks>This class contains extension methods that integrate Autofac with ReactiveUI in Avalonia applications. These methods are intended to be used during application startup to set up dependency resolution and enable further customization of the application's composition and ReactiveUI configuration.</remarks>
public static class AvaloniaMixins
{
    /// <summary>Builds the ReactiveUI Splat application when it has not already been built.</summary>
    /// <param name="rxuiBuilder">The ReactiveUI builder.</param>
    private static void BuildAppIfNeeded(IReactiveUIBuilder rxuiBuilder)
    {
        if (SplatBuilder.HasBeenBuilt)
        {
            return;
        }

        _ = rxuiBuilder.BuildApp();
    }

    /// <summary>Extends Avalonia application builders with Autofac ReactiveUI registration.</summary>
    /// <param name="builder">The Avalonia application builder to extend.</param>
    extension(AppBuilder builder)
    {
        /// <summary>Configures the application to use ReactiveUI with Autofac as the dependency injection container.</summary>
        /// <remarks>This method integrates Autofac with ReactiveUI by registering the Autofac dependency resolver and allowing custom container configuration. Additional customization of the resolver and ReactiveUI builder can be performed using the optional delegates.</remarks>
        /// <param name="containerConfig">A delegate that configures the Autofac container by registering services and components. Cannot be null.</param>
        /// <param name="withResolver">An optional delegate that allows further customization of the Autofac dependency resolver after it is created.</param>
        /// <param name="withReactiveUIBuilder">An optional delegate that allows additional configuration of the ReactiveUI builder.</param>
        /// <returns>The application builder instance, enabling further configuration or chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the builder or <paramref name="containerConfig"/> is null.</exception>
        public AppBuilder UseReactiveUIWithAutofac(
            Action<ContainerBuilder> containerConfig,
            Action<AutofacDependencyResolver>? withResolver = null,
            Action<ReactiveUIBuilder>? withReactiveUIBuilder = null)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(containerConfig);

            return builder.AfterPlatformServicesSetup(platformBuilder =>
            {
                var containerBuilder = new ContainerBuilder();
                var module = new AutofacSplatModule(containerBuilder);
                module.Configure(default!);
                containerConfig(containerBuilder);

                var rxuiBuilder = AppLocator.CurrentMutable.CreateReactiveUIBuilder();
                _ = rxuiBuilder.WithAvalonia();
                withReactiveUIBuilder?.Invoke(rxuiBuilder);
                BuildAppIfNeeded(rxuiBuilder);

                var container = containerBuilder.Build();
                var autofacResolver = container.Resolve<AutofacDependencyResolver>();
                autofacResolver.SetLifetimeScope(container);
                withResolver?.Invoke(autofacResolver);
            });
        }
    }
}
