// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Avalonia.Example.Models;

/// <summary>Represents one local machine measurement sample.</summary>
[System.Diagnostics.DebuggerDisplay("MachineSnapshot: {CapturedAt}")]
public sealed record MachineSnapshot
{
    /// <summary>Initializes a new instance of the <see cref="MachineSnapshot"/> class.</summary>
    /// <param name="capturedAt">The local time when the sample was captured.</param>
    /// <param name="processorPercent">The estimated process CPU percentage.</param>
    /// <param name="workingSetMegabytes">The process working-set size in megabytes.</param>
    /// <param name="managedMegabytes">The managed heap size in megabytes.</param>
    /// <param name="threadCount">The process thread count.</param>
    /// <param name="status">The feed status text.</param>
    public MachineSnapshot(
        DateTimeOffset capturedAt,
        double processorPercent,
        double workingSetMegabytes,
        double managedMegabytes,
        int threadCount,
        string status)
    {
        CapturedAt = capturedAt;
        ProcessorPercent = processorPercent;
        WorkingSetMegabytes = workingSetMegabytes;
        ManagedMegabytes = managedMegabytes;
        ThreadCount = threadCount;
        Status = status;
    }

    /// <summary>Gets an empty sample used before the first live update arrives.</summary>
    public static MachineSnapshot Empty { get; } = new(DateTimeOffset.UnixEpoch, 0, 0, 0, 0, "Waiting for the first local sample.");

    /// <summary>Gets the local time when the sample was captured.</summary>
    public DateTimeOffset CapturedAt { get; init; }

    /// <summary>Gets the estimated process CPU percentage.</summary>
    public double ProcessorPercent { get; init; }

    /// <summary>Gets the process working-set size in megabytes.</summary>
    public double WorkingSetMegabytes { get; init; }

    /// <summary>Gets the managed heap size in megabytes.</summary>
    public double ManagedMegabytes { get; init; }

    /// <summary>Gets the process thread count.</summary>
    public int ThreadCount { get; init; }

    /// <summary>Gets the feed status text.</summary>
    public string Status { get; init; }
}
