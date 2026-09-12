// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using ReactiveUI.Avalonia.Example.Models;
using ReactiveUI.Avalonia.Example.Services;
using ReactiveUI.Avalonia.Example.ViewModels;
using ReactiveUI.Avalonia.Example.Views;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Concurrency;
using ReactiveUI.Primitives.Disposables;
using ReactiveUI.Primitives.Signals;
using Splat;

namespace ReactiveUI.Avalonia.Example.Tests;

/// <summary>Tests the production application setup and rendered view lifetimes.</summary>
public sealed class ShowcaseHeadlessTests
{
    /// <summary>The width of a test window.</summary>
    private const int WindowWidth = 1000;

    /// <summary>The height of a test window.</summary>
    private const int WindowHeight = 700;

    /// <summary>The maximum wait for a dispatched measurement.</summary>
    private static readonly TimeSpan DispatchTimeout = TimeSpan.FromSeconds(5);

    /// <summary>Verifies the explicit light palette remains legible when Windows uses a dark theme.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Showcase_Uses_Explicit_Light_Theme() =>
        await Assert.That(Application.Current!.RequestedThemeVariant).IsEqualTo(ThemeVariant.Light);

    /// <summary>Verifies button ICommand execution routes asynchronous errors to the view interaction.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Failure_Button_Executes_And_Reports_Error()
    {
        await Assert.That(AvaloniaScheduler.Instance.Dispatcher).IsSameReferenceAs(Dispatcher.UIThread);
        using var service = new CountingMetricsService();
        using var shell = MainViewModel.Create(service);
        var window = new MainWindow { ViewModel = shell };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            shell.Navigate(shell.Commands);
            Dispatcher.UIThread.RunJobs();
            var view = (CommandLabView)window.FindControl<RoutedViewHost>("RouterHost")!.Content!;
            var button = view.FindControl<Button>("FailWorkButton")!;
            await Assert.That(button.Command).IsSameReferenceAs(shell.Commands.FailWork);
            await Assert.That(button.IsEffectivelyEnabled).IsTrue();
            await Assert.That(button.Command!.CanExecute(null)).IsTrue();
            var acknowledged = shell.Commands.WhenAnyValue(static model => model.AcknowledgedErrors)
                .Where(static count => count > 0).Take(1).ToTask();
            var center = button.TranslatePoint(new Rect(button.Bounds.Size).Center, window)!.Value;
            window.MouseDown(center, MouseButton.Left);
            window.MouseUp(center, MouseButton.Left);
            _ = await acknowledged.WaitAsync(DispatchTimeout);
            await Assert.That(shell.Commands.LastError).IsEqualTo("The sample command failed by design.");
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>Verifies real view bindings, command availability, and interaction disposal across activation.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Command_View_Binds_Input_And_Handles_Interactions_Only_While_Active()
    {
        const string initialInput = "Review measurements";
        const int expectedAcknowledgements = 2;
        using var service = new CountingMetricsService();
        using var shell = MainViewModel.Create(service);
        var view = new CommandLabView { ViewModel = shell.Commands };
        var window = new Window { Content = view, Width = WindowWidth, Height = WindowHeight };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var input = view.FindControl<TextBox>("WorkItemTextBox")!;
            var button = view.FindControl<Button>("RunWorkButton")!;
            await Assert.That(button.IsEffectivelyEnabled).IsFalse();
            input.Text = initialInput;
            Dispatcher.UIThread.RunJobs();
            await Assert.That(shell.Commands.WorkItemText).IsEqualTo(initialInput);
            await Assert.That(button.IsEffectivelyEnabled).IsTrue();
            await Assert.That(button.Command).IsSameReferenceAs(shell.Commands.RunWork);
            _ = await shell.Commands.ReportError.Handle("Expected test interaction").ToTask();
            await Assert.That(shell.Commands.AcknowledgedErrors).IsEqualTo(1);
            window.Content = null;
            Dispatcher.UIThread.RunJobs();
            await Assert.That(button.Command).IsNull();
            shell.Commands.WorkItemText = "Changed while inactive";
            await Assert.That(input.Text).IsEqualTo(initialInput);
            window.Content = view;
            Dispatcher.UIThread.RunJobs();
            await Assert.That(input.Text).IsEqualTo("Changed while inactive");
            _ = await shell.Commands.ReportError.Handle("Reactivated interaction").ToTask();
            await Assert.That(shell.Commands.AcknowledgedErrors).IsEqualTo(expectedAcknowledgements);
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>Verifies activation starts, stops, and resumes the live subscription.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Metrics_View_Activation_Stops_And_Resumes_Live_Subscription()
    {
        using var service = new CountingMetricsService();
        using var shell = MainViewModel.Create(service);
        var view = new MetricsView { ViewModel = shell.Metrics };
        var window = new Window { Content = view, Width = WindowWidth, Height = WindowHeight };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            await Assert.That(service.ActiveSubscriptions).IsEqualTo(1);

            window.Content = null;
            Dispatcher.UIThread.RunJobs();
            await Assert.That(service.ActiveSubscriptions).IsEqualTo(0);
            var sampleCount = shell.Metrics.SampleCount;
            service.Publish();
            Dispatcher.UIThread.RunJobs();
            await Assert.That(shell.Metrics.SampleCount).IsEqualTo(sampleCount);

            window.Content = view;
            Dispatcher.UIThread.RunJobs();
            await Assert.That(service.ActiveSubscriptions).IsEqualTo(1);
        }
        finally
        {
            window.Close();
        }

        await Assert.That(service.ActiveSubscriptions).IsEqualTo(0);
    }

    /// <summary>Verifies worker-thread measurements are dispatched before changing UI-bound state.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Metrics_View_Marshals_Background_Samples_To_UI_Thread()
    {
        using var service = new CountingMetricsService();
        using var shell = MainViewModel.Create(service);
        var window = new Window { Content = new MetricsView { ViewModel = shell.Metrics }, Width = WindowWidth, Height = WindowHeight };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var updated = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            using var changes = shell.Metrics.Changed
                .Where(static change => change.PropertyName == nameof(MetricsViewModel.SampleCount))
                .SubscribeSafe(_ => updated.TrySetResult(Dispatcher.UIThread.CheckAccess()), error => updated.TrySetException(error));
            await Task.Run(service.Publish);
            await Assert.That(await updated.Task.WaitAsync(DispatchTimeout)).IsTrue();
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>Verifies startup resolves the initial route and all contracted metric cards through real DI setup.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Production_Startup_Resolves_Initial_Route_And_Contracted_Cards()
    {
        using var service = new CountingMetricsService();
        using var shell = MainViewModel.Create(service);
        var window = new MainWindow { ViewModel = shell };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            await Assert.That(AppLocator.Current.GetService<IViewFor<MetricCardViewModel>>("performance")).IsTypeOf<PerformanceMetricCardView>();
            await Assert.That(AppLocator.Current.GetService<IViewFor<MetricCardViewModel>>("memory")).IsTypeOf<MemoryMetricCardView>();
            var host = window.FindControl<RoutedViewHost>("RouterHost")!;
            await Assert.That(host.Content).IsTypeOf<OverviewView>();
            var overview = (OverviewView)host.Content!;
            var items = overview.FindControl<ItemsControl>("FeatureItems")!;
            await Assert.That(items.ItemTemplate).IsNotNull();
            var renderedFeatures = 0;
            foreach (var descendant in items.GetVisualDescendants())
            {
                if (descendant is FeatureView)
                {
                    renderedFeatures++;
                }
            }

            await Assert.That(renderedFeatures).IsEqualTo(overview.ViewModel!.Features.Count);
            shell.Navigate(shell.Metrics);
            Dispatcher.UIThread.RunJobs();
            await Assert.That(host.Content).IsTypeOf<MetricsView>();
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>A controllable measurement source that records subscription ownership.</summary>
    private sealed class CountingMetricsService : ILocalMachineMetricsService, IDisposable
    {
        /// <summary>The source of subsequent measurements.</summary>
        private readonly Signal<MachineSnapshot> _samples = new();

        /// <summary>Gets the active subscription count.</summary>
        internal int ActiveSubscriptions { get; private set; }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MachineSnapshot ReadSnapshot() => MachineSnapshot.Empty;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IObservable<MachineSnapshot> Watch(TimeSpan interval) => Signal.Create<MachineSnapshot>(observer =>
        {
            ActiveSubscriptions++;
            observer.OnNext(ReadSnapshot());
            var subscription = _samples.Subscribe(observer);
            return Scope.Create((Subscription: subscription, Service: this), static state =>
            {
                state.Subscription.Dispose();
                state.Service.ActiveSubscriptions--;
            });
        });

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => _samples.Dispose();

        /// <summary>Pushes another measurement on the calling thread.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Publish() => _samples.OnNext(ReadSnapshot());
    }
}
