// Copyright (c) 2025 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using ReactiveUI.Builder;
using Splat;
using Splat.Builder;
using Splat.Microsoft.Extensions.DependencyInjection;
using AppBuilder = Avalonia.AppBuilder;

namespace ReactiveUI.Avalonia.Splat;

/// <summary>
/// Provides extension methods for integrating ReactiveUI with Avalonia applications using the Microsoft dependency
/// resolver.
/// </summary>
/// <remarks>This static class contains mixin methods that enable the use of ReactiveUI in Avalonia applications
/// with dependency injection support via Microsoft's IServiceCollection and IServiceProvider. These methods facilitate
/// the configuration of services and the setup of the dependency resolver, streamlining the integration process for
/// applications that leverage both Avalonia and ReactiveUI.</remarks>
public static class AvaloniaMixins
{
    /// <summary>
    /// Configures the application to use ReactiveUI with the Microsoft dependency injection system, allowing services
    /// to be registered and resolved via IServiceCollection and IServiceProvider.
    /// </summary>
    /// <remarks>This method integrates ReactiveUI with Microsoft's dependency injection by registering
    /// services in an IServiceCollection and building an IServiceProvider. It is typically used during application
    /// startup to enable service resolution throughout the app. The optional delegates allow for advanced customization
    /// of both the dependency resolver and ReactiveUI configuration.</remarks>
    /// <param name="builder">The application builder used to configure the app. Cannot be null.</param>
    /// <param name="containerConfig">A delegate that configures the IServiceCollection for dependency injection. This is used to register services
    /// required by the application. Cannot be null.</param>
    /// <param name="withResolver">An optional delegate invoked with the IServiceProvider after it has been built, allowing additional
    /// configuration or initialization using the resolved services.</param>
    /// <param name="withReactiveUIBuilder">An optional delegate invoked with the ReactiveUIBuilder to allow further customization of ReactiveUI
    /// configuration.</param>
    /// <returns>The application builder instance, configured to use ReactiveUI with Microsoft dependency injection.</returns>
    /// <exception cref="ArgumentNullException">Thrown if builder or containerConfig is null.</exception>
    public static AppBuilder UseReactiveUIWithMicrosoftDependencyResolver(this AppBuilder builder, Action<IServiceCollection> containerConfig, Action<IServiceProvider?>? withResolver = null, Action<ReactiveUIBuilder>? withReactiveUIBuilder = null) =>
        builder switch
        {
            null => throw new ArgumentNullException(nameof(builder)),
            _ => builder.UseReactiveUI(rxuiBuilder =>
            {
                if (AppLocator.CurrentMutable is null)
                {
                    return;
                }

#if NETSTANDARD
                if (containerConfig is null)
                {
                    throw new ArgumentNullException(nameof(containerConfig));
                }
#else
                ArgumentNullException.ThrowIfNull(containerConfig);
#endif

                IServiceCollection serviceCollection = new ServiceCollection();
                rxuiBuilder.UsingSplatModule(new MicrosoftDependencyResolverModule(serviceCollection));
                AppLocator.CurrentMutable.RegisterConstant(serviceCollection);
                containerConfig(serviceCollection);
                var serviceProvider = serviceCollection.BuildServiceProvider();
                serviceProvider.UseMicrosoftDependencyResolver();

                if (withResolver is not null)
                {
                    withResolver(serviceProvider);
                }

                if (withReactiveUIBuilder is not null)
                {
                    withReactiveUIBuilder(rxuiBuilder);
                }
            })
        };
}
