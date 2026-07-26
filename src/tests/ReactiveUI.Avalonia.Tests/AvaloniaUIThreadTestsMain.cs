// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Diagnostics;
using ReactiveUI.Primitives.Concurrency;

namespace ReactiveUI.Avalonia.Tests;

/// <summary>Tests for the AvaloniaScheduler UI thread scheduler behavior.</summary>
public class AvaloniaUIThreadTestsMain
{
    /// <summary>The delay before scheduled work is due.</summary>
    private const int FutureScheduleDelayMilliseconds = 20;

    /// <summary>The timeout used when awaiting scheduled work.</summary>
    private const int ScheduleTimeoutSeconds = 2;

    /// <summary>The number of milliseconds in a second.</summary>
    private const double MillisecondsPerSecond = 1000.0;

    /// <summary>Verifies that the AvaloniaScheduler singleton instance is not null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AvaloniaScheduler_Instance_IsNotNull() => await Assert.That(AvaloniaScheduler.Instance).IsNotNull();

    /// <summary>Verifies that multiple calls to Instance return the same singleton.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AvaloniaScheduler_Instance_IsSingleton()
    {
        var instance1 = AvaloniaScheduler.Instance;
        var instance2 = AvaloniaScheduler.Instance;
        await Assert.That(instance1).IsSameReferenceAs(instance2);
    }

    /// <summary>Verifies that AvaloniaScheduler provides the sequencer abstraction used by ReactiveUI.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AvaloniaScheduler_ImplementsISequencer()
    {
        var scheduler = AvaloniaScheduler.Instance;
        await Assert.That(scheduler).IsAssignableTo<ISequencer>();
    }

    /// <summary>Verifies that the Timestamp property returns a monotonic timestamp.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AvaloniaScheduler_Timestamp_ReturnsPositiveValue()
    {
        var scheduler = AvaloniaScheduler.Instance;

        await Assert.That(scheduler.Timestamp).IsGreaterThan(0);
    }

    /// <summary>Verifies that Schedule throws ArgumentNullException for a null work item.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AvaloniaScheduler_ScheduleWorkItem_ThrowsOnNull()
    {
        var scheduler = AvaloniaScheduler.Instance;

        await Assert.That(() => scheduler.Schedule((IWorkItem)null!)).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies that Schedule with a work item executes it.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AvaloniaScheduler_ScheduleWorkItem_Executes()
    {
        var scheduler = AvaloniaScheduler.Instance;
        var executed = false;

        scheduler.Schedule(new WorkItem(() => executed = true));

        await Assert.That(executed).IsTrue();
    }

    /// <summary>Verifies that Schedule with a timestamp throws ArgumentNullException for a null work item.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AvaloniaScheduler_ScheduleWorkItemWithTimestamp_ThrowsOnNull()
    {
        var scheduler = AvaloniaScheduler.Instance;

        await Assert.That(() => scheduler.Schedule(null!, scheduler.Timestamp)).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies that Schedule with a past timestamp executes the work item immediately.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AvaloniaScheduler_ScheduleWorkItemWithPastTimestamp_Executes()
    {
        var scheduler = AvaloniaScheduler.Instance;
        var executed = false;

        scheduler.Schedule(new WorkItem(() => executed = true), scheduler.Timestamp - 1);

        await Assert.That(executed).IsTrue();
    }

    /// <summary>Verifies that Schedule with a future timestamp executes after the delay.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AvaloniaScheduler_ScheduleWorkItemWithFutureTimestamp_Executes()
    {
        var scheduler = AvaloniaScheduler.Instance;
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        scheduler.Schedule(new WorkItem(() => completion.SetResult()), scheduler.Timestamp + StopwatchTicks(FutureScheduleDelayMilliseconds));

        await completion.Task.WaitAsync(TimeSpan.FromSeconds(ScheduleTimeoutSeconds));
        await Assert.That(completion.Task.IsCompletedSuccessfully).IsTrue();
    }

    /// <summary>Converts milliseconds to stopwatch ticks.</summary>
    /// <param name="milliseconds">The number of milliseconds.</param>
    /// <returns>The number of stopwatch ticks.</returns>
    private static long StopwatchTicks(int milliseconds) =>
        (long)(milliseconds / MillisecondsPerSecond * Stopwatch.Frequency);

    /// <summary>A work item backed by an action.</summary>
    /// <param name="action">The action to execute.</param>
    private sealed class WorkItem(Action action) : IWorkItem
    {
        /// <inheritdoc/>
        public void Execute() => action();
    }
}
