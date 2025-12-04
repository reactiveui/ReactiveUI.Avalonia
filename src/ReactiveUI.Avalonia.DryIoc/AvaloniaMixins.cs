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
                RxSchedulers.MainThreadScheduler = AvaloniaScheduler.Instance;
                containerConfig(container);
            })
        };

    /// <summary>
    /// Uses the reactive UI with dry ioc.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="containerConfig">The container configuration.</param>
    /// <param name="withReactiveUIBuilder">The with reactive UI builder.</param>
    /// <returns>An App Builder.</returns>
    /// <exception cref="ArgumentNullException">builder.</exception>
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
