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
        /// <remarks>
        /// The Ninject container is registered with the service locator before the ReactiveUI builder is built.
        /// </remarks>
        /// <param name="containerConfig">Configures the Ninject container.</param>
        /// <returns>The application builder instance with ReactiveUI and Ninject configured.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the builder or <paramref name="containerConfig"/> is null.</exception>
        public AppBuilder UseReactiveUIWithNinject(Action<StandardKernel> containerConfig) =>
            builder.UseReactiveUIWithNinject(containerConfig, null);

        /// <summary>Configures ReactiveUI with Ninject and customizes the ReactiveUI builder.</summary>
        /// <param name="containerConfig">Configures the Ninject container.</param>
        /// <param name="withReactiveUIBuilder">Customizes the ReactiveUI builder, or null.</param>
        /// <returns>The application builder instance.</returns>
        public AppBuilder UseReactiveUIWithNinject(
            Action<StandardKernel> containerConfig,
            Action<ReactiveUIBuilder>? withReactiveUIBuilder) =>
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
