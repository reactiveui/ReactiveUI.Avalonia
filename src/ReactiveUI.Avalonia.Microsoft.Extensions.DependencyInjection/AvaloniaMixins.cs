// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
#if REACTIVE_SHIM
namespace ReactiveUI.Avalonia.Reactive.Splat;
#else
namespace ReactiveUI.Avalonia.Splat;
#endif

/// <summary>Provides extension methods for integrating ReactiveUI with Microsoft dependency injection.</summary>
/// <remarks>This static class contains mixin methods that enable the use of ReactiveUI in Avalonia applications with dependency injection support via Microsoft's IServiceCollection and IServiceProvider.</remarks>
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
        /// <remarks>This method integrates ReactiveUI with Microsoft's dependency injection by registering services in an IServiceCollection and building an IServiceProvider.</remarks>
        /// <param name="containerConfig">A delegate that configures the IServiceCollection for dependency injection. Cannot be null.</param>
        /// <param name="withResolver">An optional delegate invoked with the IServiceProvider after it has been built.</param>
        /// <param name="withReactiveUIBuilder">An optional delegate invoked with the ReactiveUIBuilder.</param>
        /// <returns>The application builder instance, configured to use ReactiveUI with Microsoft dependency injection.</returns>
        /// <exception cref="ArgumentNullException">Thrown if builder or containerConfig is null.</exception>
        public AppBuilder UseReactiveUIWithMicrosoftDependencyResolver(
            Action<IServiceCollection> containerConfig,
            Action<IServiceProvider?>? withResolver = null,
            Action<ReactiveUIBuilder>? withReactiveUIBuilder = null) =>
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
