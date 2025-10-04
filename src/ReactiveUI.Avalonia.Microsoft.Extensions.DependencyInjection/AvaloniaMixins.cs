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

namespace ReactiveUI.Avalonia.Splat
{
    /// <summary>
    /// Avalonia Mixins.
    /// </summary>
    public static class AvaloniaMixins
    {
        /// <summary>
        /// Uses the splat with microsoft dependency resolver.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="containerConfig">The configure.</param>
        /// <param name="withResolver">The get service provider.</param>
        /// <returns>An App Builder.</returns>
        public static AppBuilder UseReactiveUIWithMicrosoftDependencyResolver(this AppBuilder builder, Action<IServiceCollection> containerConfig, Action<IServiceProvider?>? withResolver = null) =>
            builder switch
            {
                null => throw new ArgumentNullException(nameof(builder)),
                _ => builder.UseReactiveUI().AfterPlatformServicesSetup(_ =>
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
                    serviceCollection.UseMicrosoftDependencyResolver();
                    AppLocator.CurrentMutable.RegisterConstant(serviceCollection);
                    RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;
                    containerConfig(serviceCollection);
                    var serviceProvider = serviceCollection.BuildServiceProvider();
                    serviceProvider.UseMicrosoftDependencyResolver();

                    if (withResolver is not null)
                    {
                        withResolver(serviceProvider);
                    }
                })
            };

        /// <summary>
        /// Uses the splat with microsoft dependency resolver.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="containerConfig">The configure.</param>
        /// <param name="withResolver">The get service provider.</param>
        /// <param name="withReactiveUIBuilder">The with reactive UI builder.</param>
        /// <returns>
        /// An App Builder.
        /// </returns>
        /// <exception cref="ArgumentNullException">builder.</exception>
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
}
