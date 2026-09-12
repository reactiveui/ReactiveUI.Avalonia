// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
#if REACTIVE_SHIM
namespace ReactiveUI.Avalonia.Reactive;
#else
namespace ReactiveUI.Avalonia;
#endif

/// <summary>Builds ReactiveUI once the Avalonia platform services are ready.</summary>
/// <remarks>
/// Avalonia defers these calls until after platform services are set up, so the service locator and the main-thread
/// scheduler are both available by the time they run.
/// </remarks>
internal static class StartupConfiguration
{
    /// <summary>Configures and builds ReactiveUI after Avalonia platform services are ready.</summary>
    /// <param name="configureReactiveUI">The application-specific ReactiveUI configuration.</param>
    internal static void ConfigureReactiveUI(Action<ReactiveUIBuilder> configureReactiveUI)
    {
        var rxuiBuilder = RxAppBuilder.CreateReactiveUIBuilder();
        _ = rxuiBuilder.WithAvalonia();

        configureReactiveUI(rxuiBuilder);

        if (Splat.Builder.AppBuilder.HasBeenBuilt)
        {
            return;
        }

        _ = rxuiBuilder.BuildApp();
    }

    /// <summary>Configures ReactiveUI with a dependency injection container when a mutable resolver is available.</summary>
    /// <typeparam name="TContainer">The dependency injection container type.</typeparam>
    /// <param name="resolver">The mutable resolver used for container registration.</param>
    /// <param name="containerFactory">The factory used to create the container.</param>
    /// <param name="containerConfig">The configuration action for the container.</param>
    /// <param name="dependencyResolverFactory">The factory used to create the dependency resolver.</param>
    internal static void ConfigureReactiveUIDIContainer<TContainer>(
        IMutableDependencyResolver? resolver,
        Func<TContainer> containerFactory,
        Action<TContainer> containerConfig,
        Func<TContainer, IDependencyResolver> dependencyResolverFactory)
        where TContainer : class
    {
        if (resolver is null)
        {
            return;
        }

        ArgumentNullException.ThrowIfNull(containerFactory);
        ArgumentNullException.ThrowIfNull(containerConfig);
        ArgumentNullException.ThrowIfNull(dependencyResolverFactory);

        var container = containerFactory();
        resolver.RegisterConstant(container);
        var dependencyResolver = dependencyResolverFactory(container);
        AppLocator.SetLocator(dependencyResolver);
        RxSchedulers.MainThreadScheduler = AvaloniaScheduler.Instance;
        containerConfig(container);
    }
}
