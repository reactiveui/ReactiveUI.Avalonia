// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
#if REACTIVE_SHIM
namespace ReactiveUI.Avalonia.Reactive.Splat;
#else
namespace ReactiveUI.Avalonia.Splat;
#endif

/// <summary>Provides extension methods for configuring Avalonia applications to use ReactiveUI with DryIoc.</summary>
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

    /// <summary>Extends Avalonia application builders with DryIoc ReactiveUI registration.</summary>
    /// <param name="builder">The Avalonia application builder to extend.</param>
    extension(AppBuilder builder)
    {
        /// <summary>Configures the application to use ReactiveUI with DryIoc as the dependency injection container.</summary>
        /// <remarks>This method integrates DryIoc with ReactiveUI, allowing services and dependencies to be registered using DryIoc. The provided <paramref name="containerConfig"/> delegate can be used to register application-specific services.</remarks>
        /// <param name="containerConfig">A delegate that configures the DryIoc container.</param>
        /// <param name="withReactiveUIBuilder">An optional delegate to further configure the ReactiveUI builder.</param>
        /// <returns>The application builder instance, configured to use ReactiveUI with DryIoc.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the builder or <paramref name="containerConfig"/> is null.</exception>
        public AppBuilder UseReactiveUIWithDryIoc(
            Action<Container> containerConfig,
            Action<ReactiveUIBuilder>? withReactiveUIBuilder = null) =>
            builder switch
            {
                null => throw new ArgumentNullException(nameof(builder)),
                _ => builder.AfterPlatformServicesSetup(platformBuilder =>
                {
                    ArgumentNullException.ThrowIfNull(containerConfig);

                    var container = new Container();
                    var module = new DryIocSplatModule(container);
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
