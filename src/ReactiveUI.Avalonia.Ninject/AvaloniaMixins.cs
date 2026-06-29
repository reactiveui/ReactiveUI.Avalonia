// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
#if REACTIVE_SHIM
namespace ReactiveUI.Avalonia.Reactive.Splat;
#else
namespace ReactiveUI.Avalonia.Splat;
#endif

/// <summary>Provides extension methods for integrating ReactiveUI with Ninject in Avalonia applications.</summary>
/// <remarks>This static class contains mixin methods that enable the configuration of dependency injection using Ninject alongside ReactiveUI in Avalonia app builders.</remarks>
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

    /// <summary>Extends Avalonia application builders with Ninject ReactiveUI registration.</summary>
    /// <param name="builder">The Avalonia application builder to extend.</param>
    extension(AppBuilder builder)
    {
        /// <summary>Configures the application to use ReactiveUI with a Ninject dependency injection container.</summary>
        /// <remarks>This method integrates Ninject as the dependency injection container for ReactiveUI applications. The Ninject container is registered with the application's service locator, and the provided configuration delegate allows customization of container bindings.</remarks>
        /// <param name="containerConfig">A delegate that configures the Ninject container.</param>
        /// <param name="withReactiveUIBuilder">An optional delegate to further configure the ReactiveUI builder.</param>
        /// <returns>The application builder instance with ReactiveUI and Ninject configured.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the builder or <paramref name="containerConfig"/> is null.</exception>
        public AppBuilder UseReactiveUIWithNinject(
            Action<StandardKernel> containerConfig,
            Action<ReactiveUIBuilder>? withReactiveUIBuilder = null) =>
            builder switch
            {
                null => throw new ArgumentNullException(nameof(builder)),
                _ => builder.AfterPlatformServicesSetup(platformBuilder =>
                {
                    ArgumentNullException.ThrowIfNull(containerConfig);

                    var container = new StandardKernel();
                    var module = new NinjectSplatModule(container);
                    module.Configure(default!);
                    AppLocator.CurrentMutable.RegisterConstant(container);
                    containerConfig(container);

                    var rxuiBuilder = AppLocator.CurrentMutable.CreateReactiveUIBuilder();
                    _ = rxuiBuilder.WithAvalonia();
                    withReactiveUIBuilder?.Invoke(rxuiBuilder);
                    BuildAppIfNeeded(rxuiBuilder);
                })
            };
    }
}
