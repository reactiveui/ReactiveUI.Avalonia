// Copyright (c) 2025 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using DryIoc;
using ReactiveUI.Builder;
using Splat;
using Splat.Builder;
using Splat.DryIoc;
using AppBuilder = Avalonia.AppBuilder;

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

                var container = new Container();
                rxuiBuilder.UsingSplatModule(new DryIocSplatModule(container));
                AppLocator.CurrentMutable.RegisterConstant(container);
                containerConfig(container);

                if (withReactiveUIBuilder is not null)
                {
                    withReactiveUIBuilder(rxuiBuilder);
                }
            })
        };
}
