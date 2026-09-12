// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Runtime.CompilerServices;
using ReactiveUI.Avalonia.Example.Models;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Signals;

namespace ReactiveUI.Avalonia.Example.Services;

/// <summary>Samples process-local measurements from the current machine.</summary>
[System.Diagnostics.DebuggerDisplay("LocalMachineMetricsService: {_gate}")]
public sealed class LocalMachineMetricsService : ILocalMachineMetricsService
{
    /// <summary>The number of bytes in one megabyte.</summary>
    private const double BytesPerMegabyte = 1024D * 1024D;

    /// <summary>The maximum percentage value.</summary>
    private const double MaximumPercentage = 100D;

    /// <summary>The gate protecting previous CPU sample values.</summary>
    private readonly Lock _gate = new();

    /// <summary>The time provider.</summary>
    private readonly TimeProvider _timeProvider;

    /// <summary>The previous total processor time.</summary>
    private TimeSpan _lastProcessorTime;

    /// <summary>The previous sample time.</summary>
    private DateTimeOffset _lastSampleTime;

    /// <summary>Initializes a new instance of the <see cref="LocalMachineMetricsService"/> class.</summary>
    public LocalMachineMetricsService()
        : this(TimeProvider.System)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="LocalMachineMetricsService"/> class.</summary>
    /// <param name="timeProvider">The time provider.</param>
    public LocalMachineMetricsService(TimeProvider timeProvider) => _timeProvider = timeProvider;

    /// <inheritdoc/>
    public MachineSnapshot ReadSnapshot()
    {
        using var process = Process.GetCurrentProcess();
        var now = _timeProvider.GetLocalNow();
        var cpuPercent = CalculateProcessorPercent(process.TotalProcessorTime, now);

        return new(
            now,
            cpuPercent,
            process.WorkingSet64 / BytesPerMegabyte,
            GC.GetTotalMemory(false) / BytesPerMegabyte,
            process.Threads.Count,
            "Live local process data");
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IObservable<MachineSnapshot> Watch(TimeSpan interval) =>
        Signal.Interval(interval)
            .StartWith(0L)
            .Select(_ => ReadSnapshot());

    /// <summary>Calculates process CPU percentage from two local samples.</summary>
    /// <param name="processorTime">The current total processor time.</param>
    /// <param name="sampleTime">The current sample time.</param>
    /// <returns>The estimated CPU percentage.</returns>
    private double CalculateProcessorPercent(TimeSpan processorTime, DateTimeOffset sampleTime)
    {
        lock (_gate)
        {
            if (_lastSampleTime == default)
            {
                _lastProcessorTime = processorTime;
                _lastSampleTime = sampleTime;
                return 0;
            }

            var elapsedProcessor = processorTime - _lastProcessorTime;
            var elapsedWall = sampleTime - _lastSampleTime;
            _lastProcessorTime = processorTime;
            _lastSampleTime = sampleTime;

            if (elapsedWall <= TimeSpan.Zero)
            {
                return 0;
            }

            var processorCount = Math.Max(1, Environment.ProcessorCount);
            var percentage = elapsedProcessor.TotalMilliseconds / elapsedWall.TotalMilliseconds / processorCount * MaximumPercentage;
            return Math.Clamp(percentage, 0, MaximumPercentage);
        }
    }
}
