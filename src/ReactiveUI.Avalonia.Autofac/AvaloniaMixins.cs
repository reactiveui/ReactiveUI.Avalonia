// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
#if REACTIVE_SHIM
namespace ReactiveUI.Avalonia.Reactive.Splat;
#else
namespace ReactiveUI.Avalonia.Splat;
#endif

/// <summary>Provides extension methods for configuring Avalonia applications to use ReactiveUI with Autofac.</summary>
/// <remarks>
/// Use these methods during application startup to configure dependency resolution and customize the Autofac and
/// ReactiveUI builders.
/// </remarks>
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
        /// <remarks>
        /// This method registers the Autofac resolver and allows the container, resolver, and ReactiveUI builder to be
        /// configured.
        /// </remarks>
        /// <param name="containerConfig">Configures the Autofac container.</param>
        /// <returns>The application builder instance, enabling further configuration or chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the builder or <paramref name="containerConfig"/> is null.</exception>
        public AppBuilder UseReactiveUIWithAutofac(Action<ContainerBuilder> containerConfig) =>
            builder.UseReactiveUIWithAutofac(containerConfig, null, null);

        /// <summary>Configures ReactiveUI with Autofac and customizes the created resolver.</summary>
        /// <param name="containerConfig">Configures the Autofac container.</param>
        /// <param name="withResolver">Customizes the Autofac resolver.</param>
        /// <returns>The application builder instance.</returns>
        public AppBuilder UseReactiveUIWithAutofac(
            Action<ContainerBuilder> containerConfig,
            Action<AutofacDependencyResolver> withResolver) =>
            builder.UseReactiveUIWithAutofac(containerConfig, withResolver, null);

        /// <summary>Configures ReactiveUI with Autofac and customizes its resolver and ReactiveUI builder.</summary>
        /// <param name="containerConfig">Configures the Autofac container.</param>
        /// <param name="withResolver">Customizes the Autofac resolver, or null.</param>
        /// <param name="withReactiveUIBuilder">Customizes the ReactiveUI builder, or null.</param>
        /// <returns>The application builder instance.</returns>
        public AppBuilder UseReactiveUIWithAutofac(
            Action<ContainerBuilder> containerConfig,
            Action<AutofacDependencyResolver>? withResolver,
            Action<ReactiveUIBuilder>? withReactiveUIBuilder)
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
