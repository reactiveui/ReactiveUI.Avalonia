// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Avalonia.Threading;
using ReactiveUI.Avalonia.Example.Models;
using ReactiveUI.Avalonia.Example.Services;
using ReactiveUI.Avalonia.Example.ViewModels;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Signals;
using Unit = ReactiveUI.Primitives.RxVoid;

namespace ReactiveUI.Avalonia.Example.Tests;

/// <summary>Tests the showcase's commands, routing, and computed state.</summary>
public sealed class ShowcaseViewModelTests
{
    /// <summary>The sample CPU utilization.</summary>
    private const double ProcessorPercent = 12.5;

    /// <summary>The warning threshold below the sample utilization.</summary>
    private const double WarningThreshold = 10;

    /// <summary>Verifies navigation, back availability, and reset through the real routing commands.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Shell_Navigates_Back_And_Resets_With_Router_Commands()
    {
        using var viewModel = MainViewModel.Create(new FixedMetricsService());
        await Assert.That(viewModel.Router.NavigationStack).Count().IsEqualTo(1);
        await Assert.That(viewModel.Router.NavigationStack[0]).IsTypeOf<OverviewViewModel>();
        await Assert.That(((ICommand)viewModel.GoBack).CanExecute(null)).IsFalse();

        viewModel.Navigate(viewModel.Metrics);
        await Assert.That(((ICommand)viewModel.GoBack).CanExecute(null)).IsTrue();
        await viewModel.GoBack.Execute(Unit.Default).ToTask();
        await Assert.That(viewModel.Router.NavigationStack[0]).IsTypeOf<OverviewViewModel>();

        viewModel.Navigate(viewModel.Commands);
        viewModel.ResetNavigation();
        await Assert.That(viewModel.Router.NavigationStack).Count().IsEqualTo(1);
        await Assert.That(viewModel.Router.NavigationStack[0]).IsTypeOf<OverviewViewModel>();
        await Assert.That(((ICommand)viewModel.GoBack).CanExecute(null)).IsFalse();
    }

    /// <summary>Verifies can-execute, input capture during async work, and error interaction acknowledgement.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Command_Lab_Uses_CanExecute_Async_And_Interaction_Acknowledgement()
    {
        using var shell = MainViewModel.Create(new FixedMetricsService());
        var viewModel = shell.Commands;
        await Assert.That(((ICommand)viewModel.RunWork).CanExecute(null)).IsFalse();

        viewModel.WorkItemText = "sample";
        await Assert.That(((ICommand)viewModel.RunWork).CanExecute(null)).IsTrue();
        var operation = viewModel.RunWork.Execute(Unit.Default).ToTask();
        viewModel.WorkItemText = "changed during execution";
        var result = await operation;
        Dispatcher.UIThread.RunJobs();
        await Assert.That(result).Contains("'sample'");
        await Assert.That(viewModel.LastResult).IsEqualTo(result);

        using var handler = viewModel.ReportError.RegisterHandler(context =>
        {
            viewModel.AcknowledgeError(context.Input);
            context.SetOutput(Unit.Default);
        });
        await Assert.That(() => viewModel.FailWork.Execute(Unit.Default).ToTask()).ThrowsExactly<InvalidOperationException>();
        Dispatcher.UIThread.RunJobs();
        await Assert.That(viewModel.LastError).IsEqualTo("The sample command failed by design.");
        await Assert.That(viewModel.AcknowledgedErrors).IsEqualTo(1);
        await Assert.That(viewModel.InteractionStatus).Contains("acknowledged");
    }

    /// <summary>Verifies computed warning state follows both incoming measurements and user input.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Metrics_Page_Applies_Samples_And_Computes_Load_Label()
    {
        using var shell = MainViewModel.Create(new FixedMetricsService());
        var viewModel = shell.Metrics;
        viewModel.CpuWarningThreshold = WarningThreshold;
        viewModel.ApplySnapshot(FixedMetricsService.Snapshot);

        await Assert.That(viewModel.ProcessorCard.Value).IsEqualTo("12.5%");
        await Assert.That(viewModel.MemoryCard.Value).IsEqualTo("128.0 MB");
        await Assert.That(viewModel.ManagedCard.Value).IsEqualTo("32.0 MB");
        await Assert.That(viewModel.SampleCount).IsEqualTo(1);
        await Assert.That(viewModel.LoadLabel).Contains("at or above");
        viewModel.CpuWarningThreshold = ProcessorPercent + 1;
        await Assert.That(viewModel.LoadLabel).Contains("below");
    }

    /// <summary>A deterministic measurement source for view-model tests.</summary>
    private sealed class FixedMetricsService : ILocalMachineMetricsService
    {
        /// <summary>The sample working set in megabytes.</summary>
        private const double WorkingSet = 128;

        /// <summary>The sample managed heap in megabytes.</summary>
        private const double ManagedHeap = 32;

        /// <summary>The sample thread count.</summary>
        private const int ThreadCount = 8;

        /// <summary>Gets the fixed sample.</summary>
        internal static MachineSnapshot Snapshot => new(DateTimeOffset.UnixEpoch, ProcessorPercent, WorkingSet, ManagedHeap, ThreadCount, "fixed");

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MachineSnapshot ReadSnapshot() => Snapshot;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IObservable<MachineSnapshot> Watch(TimeSpan interval) => Signal.Return(Snapshot);
    }
}
