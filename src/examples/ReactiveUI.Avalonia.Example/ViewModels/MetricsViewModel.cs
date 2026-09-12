// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using ReactiveUI;
using ReactiveUI.Avalonia.Example.Models;
using ReactiveUI.Avalonia.Example.Services;
using ReactiveUI.Primitives;
using Unit = ReactiveUI.Primitives.RxVoid;

namespace ReactiveUI.Avalonia.Example.ViewModels;

/// <summary>Shows live local process measurements.</summary>
[System.Diagnostics.DebuggerDisplay("MetricsViewModel: {ProcessorCard}")]
public sealed class MetricsViewModel : PageViewModel
{
    /// <summary>The maximum percentage value.</summary>
    private const double MaximumPercentage = 100D;

    /// <summary>The local metrics service.</summary>
    private readonly ILocalMachineMetricsService _metricsService;

    /// <summary>The computed load label backing field.</summary>
    private readonly ObservableAsPropertyHelper<string> _loadLabel;

    /// <summary>Initializes a new instance of the <see cref="MetricsViewModel"/> class.</summary>
    /// <param name="hostScreen">The owning screen.</param>
    /// <param name="metricsService">The local metrics service.</param>
    [RequiresUnreferencedCode("This showcase uses WhenAnyValue and ToProperty to demonstrate ObservableAsPropertyHelper.")]
    public MetricsViewModel(IScreen hostScreen, ILocalMachineMetricsService metricsService)
        : base(hostScreen, "metrics", "Live local metrics", "Samples this process and updates the UI while the page is active.")
    {
        _metricsService = metricsService;
        ProcessorCard = new("Process CPU", "0.0%", "Estimated from process CPU time deltas");
        MemoryCard = new("Working set", "0.0 MB", "Current process resident memory");
        ManagedCard = new("Managed heap", "0.0 MB", "GC.GetTotalMemory without forcing collection");
        SampleOnce = Track(ReactiveCommand.CreateFromTask(SampleOnceAsync));
        _ = Track(SampleOnce.ThrownExceptions.SubscribeSafe(ApplyError, ApplyError));

        _loadLabel = this.WhenAnyValue(
            static x => x.Latest.ProcessorPercent,
            static x => x.CpuWarningThreshold,
            static (cpu, threshold) => cpu >= threshold ? "CPU is at or above the warning threshold." : "CPU is below the warning threshold.")
            .ToProperty(this, static x => x.LoadLabel, "Waiting for live data.");

        this.WhenActivated(disposables =>
        {
            _ = _metricsService
                .Watch(TimeSpan.FromSeconds(1))
                .ObserveOn(RxSchedulers.MainThreadScheduler)
                .SubscribeSafe(ApplySnapshot, ApplyError)
                .DisposeWith(disposables);
        });
    }

    /// <summary>Gets the CPU metric card.</summary>
    public MetricCardViewModel ProcessorCard { get; }

    /// <summary>Gets the memory metric card.</summary>
    public MetricCardViewModel MemoryCard { get; }

    /// <summary>Gets the managed heap metric card.</summary>
    public MetricCardViewModel ManagedCard { get; }

    /// <summary>Gets the command that reads one immediate sample.</summary>
    public ReactiveCommand<Unit, MachineSnapshot> SampleOnce { get; }

    /// <summary>Gets the computed load label.</summary>
    public string LoadLabel => _loadLabel.Value;

    /// <summary>Gets the latest sample.</summary>
    public MachineSnapshot Latest
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    } = MachineSnapshot.Empty;

    /// <summary>Gets or sets the CPU warning threshold.</summary>
    public double CpuWarningThreshold
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, Math.Clamp(value, 1, MaximumPercentage));
    } = 40;

    /// <summary>Gets the number of observed live samples.</summary>
    public int SampleCount
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>Applies a deterministic sample for tests and immediate UI refreshes.</summary>
    /// <param name="snapshot">The sample to apply.</param>
    public void ApplySnapshot(MachineSnapshot snapshot)
    {
        Latest = snapshot;
        SampleCount++;
        ProcessorCard.Value = $"{snapshot.ProcessorPercent:0.0}%";
        MemoryCard.Value = $"{snapshot.WorkingSetMegabytes:0.0} MB";
        ManagedCard.Value = $"{snapshot.ManagedMegabytes:0.0} MB";
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _loadLabel.Dispose();
        }

        base.Dispose(disposing);
    }

    /// <summary>Reads and applies one immediate sample.</summary>
    /// <returns>The applied sample.</returns>
    private Task<MachineSnapshot> SampleOnceAsync()
    {
        var snapshot = _metricsService.ReadSnapshot();
        ApplySnapshot(snapshot);
        return Task.FromResult(snapshot);
    }

    /// <summary>Applies an error sample when the live feed fails.</summary>
    /// <param name="error">The live feed error.</param>
    private void ApplyError(Exception error) => Latest = Latest with { Status = error.Message };
}
