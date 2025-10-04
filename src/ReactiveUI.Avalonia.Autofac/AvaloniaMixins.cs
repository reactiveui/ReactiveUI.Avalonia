// Copyright (c) 2025 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Autofac;
using ReactiveUI;
using ReactiveUI.Avalonia;
using ReactiveUI.Builder;
using Splat;
using Splat.Autofac;
using Splat.Builder;

namespace Avalonia.ReactiveUI.Splat
{
    /// <summary>
    /// Avalonia Mixins.
    /// </summary>
    public static class AvaloniaMixins
    {
        /// <summary>
        /// Uses the splat with dry ioc.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="containerConfig">The configure.</param>
        /// <param name="withResolver">The get resolver.</param>
        /// <returns>
        /// An App Builder.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">builder.</exception>
        public static AppBuilder UseReactiveUIWithAutofac(this AppBuilder builder, Action<ContainerBuilder> containerConfig, Action<AutofacDependencyResolver>? withResolver = null) =>
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
                    var containerBuilder = new ContainerBuilder();
                    var autofacResolver = containerBuilder.UseAutofacDependencyResolver();
                    containerBuilder.RegisterInstance(autofacResolver);
                    autofacResolver.InitializeReactiveUI(RegistrationNamespace.Avalonia);
                    RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;
                    containerConfig(containerBuilder);
                    var container = containerBuilder.Build();
                    autofacResolver.SetLifetimeScope(container);

                    if (withResolver is not null)
                    {
                        withResolver(autofacResolver);
                    }
                })
            };

        /// <summary>
        /// Uses the reactive UI with autofac.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="containerConfig">The container configuration.</param>
        /// <param name="withResolver">The with resolver.</param>
        /// <param name="withReactiveUIBuilder">The with reactive UI builder.</param>
        /// <returns>
        /// An App Builder.
        /// </returns>
        /// <exception cref="ArgumentNullException">builder.</exception>
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
}
