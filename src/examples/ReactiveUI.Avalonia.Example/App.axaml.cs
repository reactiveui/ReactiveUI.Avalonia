// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ReactiveUI.Avalonia.Example.Services;
using ReactiveUI.Avalonia.Example.ViewModels;
using ReactiveUI.Avalonia.Example.Views;
using Splat;

namespace ReactiveUI.Avalonia.Example;

/// <summary>The example Avalonia application.</summary>
[System.Diagnostics.DebuggerDisplay("App: {ApplicationLifetime}")]
public sealed class App : Application
{
    /// <summary>Registers view and service dependencies for the example.</summary>
    /// <param name="resolver">The resolver to update.</param>
    [RequiresUnreferencedCode("The registered views demonstrate reflection-based ReactiveUI binding and activation.")]
    [RequiresDynamicCode("The registered views demonstrate dynamic ReactiveUI bindings.")]
    public static void RegisterViews(IMutableDependencyResolver resolver)
    {
        resolver.RegisterLazySingleton(static () => CreateMetricsService(OperatingSystem.IsBrowser()));
        resolver.Register<IViewFor<OverviewViewModel>>(static () => new OverviewView());
        resolver.Register<IViewFor<MetricsViewModel>>(static () => new MetricsView());
        resolver.Register<IViewFor<CommandLabViewModel>>(static () => new CommandLabView());
        resolver.Register<IViewFor<FeatureViewModel>>(static () => new FeatureView());
        resolver.Register(static () => new PerformanceMetricCardView(), typeof(IViewFor<MetricCardViewModel>), "performance");
        resolver.Register(static () => new MemoryMetricCardView(), typeof(IViewFor<MetricCardViewModel>), "memory");
    }

    /// <inheritdoc/>
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    /// <inheritdoc/>
    public override void OnFrameworkInitializationCompleted()
    {
        AttachShell(ApplicationLifetime);
        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>Creates the metrics service for the platform.</summary>
    /// <param name="isBrowser">Whether the app runs in a browser, which cannot read local machine metrics.</param>
    /// <returns>The browser service in a browser, otherwise the local service.</returns>
    internal static ILocalMachineMetricsService CreateMetricsService(bool isBrowser) => isBrowser
        ? new BrowserMachineMetricsService()
        : new LocalMachineMetricsService();

    /// <summary>Creates the shell and gives it to the lifetime: a window on desktop, a single view elsewhere.</summary>
    /// <param name="lifetime">The application lifetime, or <see langword="null"/> when none is running.</param>
    /// <exception cref="InvalidOperationException">Thrown when the local metrics service has not been registered.</exception>
    internal static void AttachShell(IApplicationLifetime? lifetime)
    {
        switch (lifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
                {
                    var shell = CreateShell(AppLocator.Current);
                    var session = new ShowcaseSession(desktop, shell, ShowcaseSession.DefaultPath());
                    desktop.Exit += (_, _) => session.Dispose();
                    session.Start();
                    desktop.MainWindow = new MainWindow { ViewModel = shell };
                    break;
                }

            case ISingleViewApplicationLifetime singleView:
                {
                    var shell = CreateShell(AppLocator.Current);

                    // A single-view lifetime has no exit event, so the session is not persisted.
                    shell.SessionStatus = ShowcaseSession.UnavailableStatus;
                    singleView.MainView = new MainView { ViewModel = shell };
                    break;
                }
        }
    }

    /// <summary>Creates the shell view model from the registered metrics service.</summary>
    /// <param name="resolver">The resolver that holds the metrics service.</param>
    /// <returns>The initialized shell view model.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the local metrics service has not been registered.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [RequiresUnreferencedCode("The shell creates showcase pages that intentionally use ReactiveUI expression-based helpers.")]
    internal static MainViewModel CreateShell(IReadonlyDependencyResolver resolver) =>
        MainViewModel.Create(resolver.GetService<ILocalMachineMetricsService>()
            ?? throw new InvalidOperationException("The local metrics service has not been registered."));
}
