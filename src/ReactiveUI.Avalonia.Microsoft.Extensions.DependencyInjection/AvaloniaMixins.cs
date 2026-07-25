// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
#if REACTIVE_SHIM
namespace ReactiveUI.Avalonia.Reactive.Splat;
#else
namespace ReactiveUI.Avalonia.Splat;
#endif

/// <summary>Provides extension methods for integrating ReactiveUI with Microsoft dependency injection.</summary>
/// <remarks>
/// These methods configure Avalonia applications through Microsoft's IServiceCollection and IServiceProvider.
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

    /// <summary>Extends Avalonia application builders with Microsoft dependency injection registration.</summary>
    /// <param name="builder">The Avalonia application builder to extend.</param>
    extension(AppBuilder builder)
    {
        /// <summary>Configures the application to use ReactiveUI with Microsoft dependency injection.</summary>
        /// <remarks>
        /// Services are registered in an IServiceCollection before the method builds the IServiceProvider.
        /// </remarks>
        /// <param name="containerConfig">Configures the IServiceCollection.</param>
        /// <returns>The application builder instance, configured to use ReactiveUI with Microsoft dependency injection.</returns>
        /// <exception cref="ArgumentNullException">Thrown if builder or containerConfig is null.</exception>
        public AppBuilder UseReactiveUIWithMicrosoftDependencyResolver(
            Action<IServiceCollection> containerConfig) =>
            builder.UseReactiveUIWithMicrosoftDependencyResolver(containerConfig, null, null);

        /// <summary>Configures Microsoft dependency injection and customizes the created provider.</summary>
        /// <param name="containerConfig">Configures the IServiceCollection.</param>
        /// <param name="withResolver">Customizes the built service provider.</param>
        /// <returns>The application builder instance.</returns>
        public AppBuilder UseReactiveUIWithMicrosoftDependencyResolver(
            Action<IServiceCollection> containerConfig,
            Action<IServiceProvider?> withResolver) =>
            builder.UseReactiveUIWithMicrosoftDependencyResolver(containerConfig, withResolver, null);

        /// <summary>Configures Microsoft dependency injection with provider and ReactiveUI customization.</summary>
        /// <param name="containerConfig">Configures the IServiceCollection.</param>
        /// <param name="withResolver">Customizes the service provider, or null.</param>
        /// <param name="withReactiveUIBuilder">Customizes the ReactiveUI builder, or null.</param>
        /// <returns>The application builder instance.</returns>
        public AppBuilder UseReactiveUIWithMicrosoftDependencyResolver(
            Action<IServiceCollection> containerConfig,
            Action<IServiceProvider?>? withResolver,
            Action<ReactiveUIBuilder>? withReactiveUIBuilder) =>
            builder switch
            {
                null => throw new ArgumentNullException(nameof(builder)),
                _ => builder.AfterPlatformServicesSetup(platformBuilder =>
                {
                    ArgumentNullException.ThrowIfNull(containerConfig);

                    IServiceCollection serviceCollection = new ServiceCollection();
                    var module = new MicrosoftDependencyResolverModule(serviceCollection);
                    module.Configure(default!);
                    AppLocator.CurrentMutable.RegisterConstant(serviceCollection);
                    containerConfig(serviceCollection);

                    var rxuiBuilder = AppLocator.CurrentMutable.CreateReactiveUIBuilder();
                    _ = rxuiBuilder.WithAvalonia();
                    withReactiveUIBuilder?.Invoke(rxuiBuilder);
                    BuildAppIfNeeded(rxuiBuilder);

                    var serviceProvider = serviceCollection.BuildServiceProvider();
                    serviceProvider.UseMicrosoftDependencyResolver();
                    withResolver?.Invoke(serviceProvider);
                })
            };
    }
}
