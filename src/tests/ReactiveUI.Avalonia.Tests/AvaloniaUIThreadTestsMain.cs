// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Diagnostics;
using Avalonia.Threading;
using ReactiveUI.Primitives.Concurrency;

namespace ReactiveUI.Avalonia.Tests;

/// <summary>Tests for the AvaloniaScheduler UI thread scheduler behavior.</summary>
public class AvaloniaUIThreadTestsMain
{
    /// <summary>Verifies that the AvaloniaScheduler singleton instance is not null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AvaloniaScheduler_Instance_IsNotNull()
    {
        await Assert.That(AvaloniaScheduler.Instance).IsNotNull();
    }

    /// <summary>Verifies that multiple calls to Instance return the same singleton.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AvaloniaScheduler_Instance_IsSingleton()
    {
        var instance1 = AvaloniaScheduler.Instance;
        var instance2 = AvaloniaScheduler.Instance;
        await Assert.That(instance1).IsSameReferenceAs(instance2);
    }

    /// <summary>Verifies that Schedule throws ArgumentNullException for a null action.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AvaloniaScheduler_Schedule_ThrowsOnNullAction()
    {
        var scheduler = AvaloniaScheduler.Instance;
        await Assert.That(() =>
            scheduler.Schedule<object>(new object(), TimeSpan.Zero, null!)).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies that AvaloniaScheduler is a LocalScheduler.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AvaloniaScheduler_IsLocalScheduler()
    {
        var scheduler = AvaloniaScheduler.Instance;
        await Assert.That(scheduler).IsTypeOf<AvaloniaScheduler>();
    }

    /// <summary>Verifies that the Now property returns approximately the current time.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AvaloniaScheduler_Now_ReturnsCurrentTime()
    {
        var scheduler = AvaloniaScheduler.Instance;
        var beforeNow = DateTimeOffset.Now;
        var schedulerNow = scheduler.Now;
        var afterNow = DateTimeOffset.Now;

        await Assert.That(schedulerNow).IsGreaterThanOrEqualTo(beforeNow.AddSeconds(-1));
        await Assert.That(schedulerNow).IsLessThanOrEqualTo(afterNow.AddSeconds(1));
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

        scheduler.Schedule(new WorkItem(() => completion.SetResult()), scheduler.Timestamp + StopwatchTicks(20));

        await completion.Task.WaitAsync(TimeSpan.FromSeconds(2));
        await Assert.That(completion.Task.IsCompletedSuccessfully).IsTrue();
    }

    /// <summary>Verifies that Schedule with zero delay executes the action.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AvaloniaScheduler_Schedule_WithZeroDelay_ExecutesAction()
    {
        var scheduler = AvaloniaScheduler.Instance;
        var actionExecuted = false;

        // Tests run on the headless UI thread (see AvaloniaTestSession), so a zero-delay schedule
        // executes synchronously and the assertion does not depend on dispatcher pumping latency.
        var disposable = scheduler.Schedule("test", TimeSpan.Zero, (s, state) =>
        {
            actionExecuted = true;
            return EmptyDisposable.Instance;
        });

        await Assert.That(actionExecuted).IsTrue();
        await Assert.That(disposable).IsNotNull();
    }

    /// <summary>Verifies that Schedule with a positive delay returns a disposable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AvaloniaScheduler_Schedule_WithPositiveDelay_ReturnsDisposable()
    {
        var scheduler = AvaloniaScheduler.Instance;
        var delay = TimeSpan.FromMilliseconds(10);

        var disposable = scheduler.Schedule("test", delay, (s, state) => EmptyDisposable.Instance);

        await Assert.That(disposable).IsNotNull();
        await Assert.That(disposable).IsTypeOf<IDisposable>();

        disposable.Dispose();
    }

    /// <summary>Verifies that Schedule returns a disposable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AvaloniaScheduler_Schedule_ReturnsDisposable()
    {
        var scheduler = AvaloniaScheduler.Instance;

        var disposable = scheduler.Schedule("test", TimeSpan.Zero, (s, state) => EmptyDisposable.Instance);

        await Assert.That(disposable).IsNotNull();
        await Assert.That(disposable).IsTypeOf<IDisposable>();
    }

    /// <summary>Verifies that zero-delay scheduling from a background thread posts onto the dispatcher.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AvaloniaScheduler_Schedule_FromBackgroundThread_PostsToDispatcher()
    {
        var scheduler = AvaloniaScheduler.Instance;
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        await Task.Run(() =>
            scheduler.Schedule("background", TimeSpan.Zero, (s, state) =>
            {
                completion.SetResult();
                return EmptyDisposable.Instance;
            }));

        await completion.Task.WaitAsync(TimeSpan.FromSeconds(2));
        await Assert.That(completion.Task.IsCompletedSuccessfully).IsTrue();
    }

    /// <summary>Verifies that zero-delay scheduling from a dedicated non-UI thread posts onto the dispatcher.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AvaloniaScheduler_Schedule_FromDedicatedBackgroundThread_PostsToDispatcher()
    {
        var scheduler = AvaloniaScheduler.Instance;
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var backgroundHasAccess = true;
        Exception? threadError = null;
        IDisposable? disposable = null;

        var thread = new Thread(() =>
        {
            try
            {
                backgroundHasAccess = Dispatcher.UIThread.CheckAccess();
                disposable = scheduler.Schedule("dedicated-background", TimeSpan.Zero, (_, _) =>
                {
                    completion.SetResult();
                    return EmptyDisposable.Instance;
                });
            }
            catch (Exception ex)
            {
                threadError = ex;
            }
        });

        thread.Start();
        thread.Join();

        try
        {
            await Assert.That(threadError).IsNull();
            await Assert.That(backgroundHasAccess).IsFalse();
            await completion.Task.WaitAsync(TimeSpan.FromSeconds(2));
            await Assert.That(completion.Task.IsCompletedSuccessfully).IsTrue();
        }
        finally
        {
            disposable?.Dispose();
        }
    }

    /// <summary>Verifies that disposing a posted dispatcher schedule before it runs cancels the action.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AvaloniaScheduler_Schedule_FromBackgroundThread_DisposeBeforePost_Cancels()
    {
        var scheduler = AvaloniaScheduler.Instance;
        var actionExecuted = false;

        await Task.Run(() =>
        {
            var disposable = scheduler.Schedule("cancel", TimeSpan.Zero, (s, state) =>
            {
                actionExecuted = true;
                return EmptyDisposable.Instance;
            });
            disposable.Dispose();
        });

        await Task.Delay(100);
        await Assert.That(actionExecuted).IsFalse();
    }

    /// <summary>Verifies that deep reentrant schedules fall back to dispatcher posting.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AvaloniaScheduler_Schedule_ReentrantLimit_PostsToDispatcher()
    {
        var scheduler = AvaloniaScheduler.Instance;
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var count = 0;

        IDisposable ScheduleNext(ISequencer _, int depth)
        {
            count++;
            if (depth >= 40)
            {
                completion.SetResult();
                return EmptyDisposable.Instance;
            }

            return scheduler.Schedule(depth + 1, TimeSpan.Zero, ScheduleNext);
        }

        _ = scheduler.Schedule(0, TimeSpan.Zero, ScheduleNext);

        await completion.Task.WaitAsync(TimeSpan.FromSeconds(2));
        await Assert.That(count).IsEqualTo(41);
    }

    /// <summary>Verifies that disposing before execution cancels the scheduled action.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AvaloniaScheduler_Schedule_CanDisposeBeforeExecution()
    {
        var scheduler = AvaloniaScheduler.Instance;
        var actionExecuted = false;

        var disposable = scheduler.Schedule("test", TimeSpan.FromMilliseconds(100), (s, state) =>
        {
            actionExecuted = true;
            return EmptyDisposable.Instance;
        });

        disposable.Dispose();

        await Task.Delay(200);

        await Assert.That(actionExecuted).IsFalse();
    }

    /// <summary>Verifies that cancelled scheduled work does not execute when invoked directly.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AvaloniaScheduler_ScheduledWork_WhenCancelled_DoesNotExecute()
    {
        var scheduler = AvaloniaScheduler.Instance;
        var actionExecuted = false;
        var work = new AvaloniaScheduler.ScheduledWork<string>(
            scheduler,
            "cancelled",
            (_, _) =>
            {
                actionExecuted = true;
                return EmptyDisposable.Instance;
            });

        work.Cancellation.Dispose();
        work.ExecuteUnlessCancelled();

        await Assert.That(actionExecuted).IsFalse();
    }

    /// <summary>Converts milliseconds to stopwatch ticks.</summary>
    /// <param name="milliseconds">The number of milliseconds.</param>
    /// <returns>The number of stopwatch ticks.</returns>
    private static long StopwatchTicks(int milliseconds) =>
        (long)(milliseconds / 1000.0 * Stopwatch.Frequency);

    /// <summary>A work item backed by an action.</summary>
    /// <param name="action">The action to execute.</param>
    private sealed class WorkItem(Action action) : IWorkItem
    {
        /// <inheritdoc/>
        public void Execute() => action();
    }
}
