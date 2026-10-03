// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using ReactiveUI.Avalonia.Example.Models;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Signals;

namespace ReactiveUI.Avalonia.Example.Services;

/// <summary>Samples the few runtime measurements available in a browser, where <c>Process</c> is not supported.</summary>
[System.Diagnostics.DebuggerDisplay("BrowserMachineMetricsService: {_timeProvider}")]
public sealed class BrowserMachineMetricsService : ILocalMachineMetricsService
{
    /// <summary>The number of bytes in one megabyte.</summary>
    private const double BytesPerMegabyte = 1024D * 1024D;

    /// <summary>The number of milliseconds in one second.</summary>
    private const long MillisecondsPerSecond = 1000;

    /// <summary>The time provider.</summary>
    private readonly TimeProvider _timeProvider;

    /// <summary>Initializes a new instance of the <see cref="BrowserMachineMetricsService"/> class.</summary>
    public BrowserMachineMetricsService()
        : this(TimeProvider.System)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="BrowserMachineMetricsService"/> class.</summary>
    /// <param name="timeProvider">The time provider.</param>
    public BrowserMachineMetricsService(TimeProvider timeProvider) => _timeProvider = timeProvider;

    /// <inheritdoc/>
    public MachineSnapshot ReadSnapshot()
    {
        var managedMegabytes = GC.GetTotalMemory(false) / BytesPerMegabyte;
        var uptimeSeconds = Environment.TickCount64 / MillisecondsPerSecond;
        return new(
            _timeProvider.GetLocalNow(),
            0,
            managedMegabytes,
            managedMegabytes,
            1,
            $"Browser data: CPU and threads are not measured. {GC.CollectionCount(0)} gen0 collections, up {uptimeSeconds} s.");
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IObservable<MachineSnapshot> Watch(TimeSpan interval) =>
        Signal.Interval(interval)
            .StartWith(0L)
            .Select(_ => ReadSnapshot());
}
