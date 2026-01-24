// Copyright (c) 2025 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Ninject;
using ReactiveUI.Builder;
using Splat;
using Splat.Builder;
using Splat.Ninject;
using AppBuilder = Avalonia.AppBuilder;

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
            _ => builder.UseReactiveUI(rxuiBuilder =>
            {
#if NETSTANDARD
                if (containerConfig is null)
                {
                    throw new ArgumentNullException(nameof(containerConfig));
                }
#else
                ArgumentNullException.ThrowIfNull(containerConfig);
#endif

                var container = new StandardKernel();
                rxuiBuilder.UsingSplatModule(new NinjectSplatModule(container));
                AppLocator.CurrentMutable.RegisterConstant(container);
                containerConfig(container);

                if (withReactiveUIBuilder is not null)
                {
                    withReactiveUIBuilder(rxuiBuilder);
                }
            })
        };
}
