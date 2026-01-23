// Copyright (c) 2025 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Autofac;
using ReactiveUI.Builder;
using Splat.Autofac;
using Splat.Builder;
using AppBuilder = Avalonia.AppBuilder;

namespace ReactiveUI.Avalonia.Splat;

/// <summary>
/// Provides extension methods for configuring Avalonia applications to use ReactiveUI with Autofac as the dependency
/// injection container.
/// </summary>
/// <remarks>This class contains extension methods that integrate Autofac with ReactiveUI in Avalonia
/// applications. These methods are intended to be used during application startup to set up dependency resolution and
/// enable further customization of the application's composition and ReactiveUI configuration.</remarks>
public static class AvaloniaMixins
{
    /// <summary>
    /// Configures the application to use ReactiveUI with Autofac as the dependency injection container.
    /// </summary>
    /// <remarks>This method integrates Autofac with ReactiveUI by registering the Autofac dependency resolver
    /// and allowing custom container configuration. Additional customization of the resolver and ReactiveUI builder can
    /// be performed using the optional delegates. This extension should be called during application startup before
    /// registering views and view models.</remarks>
    /// <param name="builder">The application builder used to configure the app. Cannot be null.</param>
    /// <param name="containerConfig">A delegate that configures the Autofac container by registering services and components. Cannot be null.</param>
    /// <param name="withResolver">An optional delegate that allows further customization of the Autofac dependency resolver after it is created.</param>
    /// <param name="withReactiveUIBuilder">An optional delegate that allows additional configuration of the ReactiveUI builder.</param>
    /// <returns>The application builder instance, enabling further configuration or chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="builder"/> or <paramref name="containerConfig"/> is null.</exception>
    public static AppBuilder UseReactiveUIWithAutofac(this AppBuilder builder, Action<ContainerBuilder> containerConfig, Action<AutofacDependencyResolver>? withResolver = null, Action<ReactiveUIBuilder>? withReactiveUIBuilder = null)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (containerConfig is null)
        {
            throw new ArgumentNullException(nameof(containerConfig));
        }

        return builder.UseReactiveUI(rxuiBuilder =>
        {
            var containerBuilder = new ContainerBuilder();
            rxuiBuilder.UsingSplatModule(new AutofacSplatModule(containerBuilder));
            containerConfig(containerBuilder);
            var container = containerBuilder.Build();
            var autofacResolver = container.Resolve<AutofacDependencyResolver>();
            autofacResolver.SetLifetimeScope(container);
            if (withResolver is not null)
            {
                withResolver(autofacResolver);
            }

            if (withReactiveUIBuilder is not null)
            {
                withReactiveUIBuilder(rxuiBuilder);
            }
        });
    }
}
