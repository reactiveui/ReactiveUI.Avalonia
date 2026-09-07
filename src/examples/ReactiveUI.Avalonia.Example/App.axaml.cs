// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using ReactiveUI.Avalonia.Example.Services;
using ReactiveUI.Avalonia.Example.ViewModels;
using ReactiveUI.Avalonia.Example.Views;
using Splat;

namespace ReactiveUI.Avalonia.Example;

/// <summary>The example Avalonia application.</summary>
public sealed class App : Application
{
    /// <summary>Gets the window factory composed by the desktop entry point.</summary>
    public Func<IClassicDesktopStyleApplicationLifetime, Window>? MainWindowFactory { get; init; }

    /// <summary>Registers view and service dependencies for the example.</summary>
    /// <param name="resolver">The resolver to update.</param>
    [RequiresUnreferencedCode("The registered views demonstrate reflection-based ReactiveUI binding and activation.")]
    [RequiresDynamicCode("The registered views demonstrate dynamic ReactiveUI bindings.")]
    public static void RegisterViews(IMutableDependencyResolver resolver)
    {
        resolver.RegisterLazySingleton<ILocalMachineMetricsService>(static () => new LocalMachineMetricsService());
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
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = MainWindowFactory?.Invoke(desktop)
                ?? throw new InvalidOperationException("The desktop window factory has not been configured.");
        }

        base.OnFrameworkInitializationCompleted();
    }
}
