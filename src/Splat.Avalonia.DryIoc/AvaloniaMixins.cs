// Copyright (c) 2025 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using DryIoc;
using ReactiveUI;
using ReactiveUI.Avalonia;
using Splat;
using Splat.DryIoc;

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
        /// <returns>An App Builder.</returns>
        public static AppBuilder UseReactiveUIWithDryIoc(this AppBuilder builder, Action<Container> containerConfig) =>
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

                    var container = new Container();
                    container.UseDryIocDependencyResolver();
                    AppLocator.CurrentMutable.RegisterConstant(container);
                    RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;
                    containerConfig(container);
                })
            };
    }
}
