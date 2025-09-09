// Copyright (c) 2025 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Reflection;
using Avalonia;
using Splat;

namespace ReactiveUI.Avalonia;

/// <summary>
/// Avalonia AppBuilder setup extensions.
/// </summary>
public static class AppBuilderExtensions
{
    /// <summary>
    /// Initializes ReactiveUI framework to use with Avalonia. Registers Avalonia scheduler,
    /// an activation for view fetcher, a template binding hook.
    /// Remember to call this method if you are using ReactiveUI in your application.
    /// </summary>
    /// <param name="builder">This builder.</param>
    /// <returns>The builder.</returns>
    public static AppBuilder UseReactiveUI(this AppBuilder builder)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.AfterPlatformServicesSetup(_ => Locator.RegisterResolverCallbackChanged(() =>
        {
            if (AppLocator.CurrentMutable is null)
            {
                return;
            }

            PlatformRegistrationManager.SetRegistrationNamespaces(RegistrationNamespace.Avalonia);
            RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;
            AppLocator.CurrentMutable.RegisterConstant<IActivationForViewFetcher>(new AvaloniaActivationForViewFetcher());
            AppLocator.CurrentMutable.RegisterConstant<IPropertyBindingHook>(new AutoDataTemplateBindingHook());
            AppLocator.CurrentMutable.RegisterConstant<ICreatesCommandBinding>(new AvaloniaCreatesCommandBinding());
            AppLocator.CurrentMutable.RegisterConstant<ICreatesObservableForProperty>(new AvaloniaObjectObservableForProperty());
        }));
    }

    /// <summary>
    /// Scan and register IViewFor&lt;TViewModel&gt; view types into the current resolver for the provided assemblies.
    /// Useful to avoid manual view registrations in App.Initialize.
    /// Registration prefers resolving the concrete view from the current resolver (to allow DI),
    /// and falls back to Activator.CreateInstance.
    /// </summary>
    /// <param name="builder">This builder.</param>
    /// <param name="assemblies">Assemblies to scan for view registrations.</param>
    /// <returns>The builder.</returns>
    public static AppBuilder RegisterReactiveUIViews(this AppBuilder builder, params Assembly[] assemblies)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.AfterPlatformServicesSetup(_ =>
        {
            var resolver = AppLocator.CurrentMutable;
            if (resolver is null)
            {
                return;
            }

            if (assemblies is null || assemblies.Length == 0)
            {
                return;
            }

            RegisterViewsInternal(resolver, assemblies);
        });
    }

    /// <summary>
    /// Convenience overload to scan and register views from the assembly containing the specified marker type.
    /// </summary>
    /// <typeparam name="TMarker">A type from the assembly to scan.</typeparam>
    /// <param name="builder">This builder.</param>
    /// <returns>The builder.</returns>
    public static AppBuilder RegisterReactiveUIViewsFromAssemblyOf<TMarker>(this AppBuilder builder)
        => RegisterReactiveUIViews(builder, typeof(TMarker).Assembly);

    /// <summary>
    /// Convenience overload to scan and register views from the entry assembly when available.
    /// </summary>
    /// <param name="builder">This builder.</param>
    /// <returns>The builder.</returns>
    public static AppBuilder RegisterReactiveUIViewsFromEntryAssembly(this AppBuilder builder)
    {
        var entry = Assembly.GetEntryAssembly();
        return entry is null ? builder : RegisterReactiveUIViews(builder, entry);
    }

    private static void RegisterViewsInternal(IMutableDependencyResolver resolver, Assembly[] assemblies)
    {
        foreach (var asm in assemblies.Distinct())
        {
            foreach (var viewType in asm.GetTypes().Where(t => !t.IsAbstract && !t.IsInterface))
            {
                var iViewFor = viewType.GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IViewFor<>));
                if (iViewFor is null)
                {
                    continue;
                }

                var viewModelType = iViewFor.GetGenericArguments()[0];

                // If ViewContract attribute is present, honor it.
                string? contract = null;
                var attr = viewType.GetCustomAttributes(true)
                    .FirstOrDefault(a => string.Equals(a.GetType().Name, "ViewContractAttribute", StringComparison.Ordinal));
                if (attr is not null)
                {
                    var prop = attr.GetType().GetProperty("Contract", BindingFlags.Public | BindingFlags.Instance);
                    if (prop is not null)
                    {
                        contract = prop.GetValue(attr) as string;
                    }
                }

                var serviceType = typeof(IViewFor<>).MakeGenericType(viewModelType);
                resolver.Register(
                    () =>
                    {
                        try
                        {
                            var resolved = AppLocator.Current.GetService(viewType);
                            if (resolved is not null)
                            {
                                return resolved;
                            }
                        }
                        catch
                        {
                            // Ignore and fall back to Activator.
                        }

                        return Activator.CreateInstance(viewType)!;
                    },
                    serviceType,
                    contract);
            }
        }
    }
}
