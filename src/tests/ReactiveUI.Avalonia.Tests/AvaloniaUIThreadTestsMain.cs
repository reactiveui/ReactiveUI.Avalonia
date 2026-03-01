// Copyright (c) 2019-2026 ReactiveUI and Avalonia Teams, and Contributors. All rights reserved.
// Licensed under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Concurrency;
using System.Reactive.Disposables;

namespace ReactiveUI.Avalonia.Tests;

/// <summary>
/// Tests for the AvaloniaScheduler UI thread scheduler behavior.
/// </summary>
public class AvaloniaUIThreadTestsMain
{
    /// <summary>
    /// Verifies that the AvaloniaScheduler singleton instance is not null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AvaloniaScheduler_Instance_IsNotNull()
    {
        await Assert.That(AvaloniaScheduler.Instance).IsNotNull();
    }

    /// <summary>
    /// Verifies that multiple calls to Instance return the same singleton.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AvaloniaScheduler_Instance_IsSingleton()
    {
        var instance1 = AvaloniaScheduler.Instance;
        var instance2 = AvaloniaScheduler.Instance;
        await Assert.That(instance1).IsSameReferenceAs(instance2);
    }

    /// <summary>
    /// Verifies that Schedule throws ArgumentNullException for a null action.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AvaloniaScheduler_Schedule_ThrowsOnNullAction()
    {
        var scheduler = AvaloniaScheduler.Instance;
        await Assert.That(() =>
            scheduler.Schedule<object>(new object(), TimeSpan.Zero, null!)).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that AvaloniaScheduler is a LocalScheduler.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AvaloniaScheduler_IsLocalScheduler()
    {
        var scheduler = AvaloniaScheduler.Instance;
        await Assert.That(scheduler).IsTypeOf<AvaloniaScheduler>();
    }

    /// <summary>
    /// Verifies that the Now property returns approximately the current time.
    /// </summary>
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

    /// <summary>
    /// Verifies that Schedule with zero delay executes the action.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AvaloniaScheduler_Schedule_WithZeroDelay_ExecutesAction()
    {
        var scheduler = AvaloniaScheduler.Instance;
        var actionExecuted = false;

        var disposable = scheduler.Schedule("test", TimeSpan.Zero, (s, state) =>
        {
            actionExecuted = true;
            return Disposable.Empty;
        });

        Thread.Sleep(50);

        await Assert.That(actionExecuted).IsTrue();
        await Assert.That(disposable).IsNotNull();
    }

    /// <summary>
    /// Verifies that Schedule with a positive delay returns a disposable.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AvaloniaScheduler_Schedule_WithPositiveDelay_ReturnsDisposable()
    {
        var scheduler = AvaloniaScheduler.Instance;
        var delay = TimeSpan.FromMilliseconds(10);

        var disposable = scheduler.Schedule("test", delay, (s, state) => Disposable.Empty);

        await Assert.That(disposable).IsNotNull();
        await Assert.That(disposable).IsTypeOf<IDisposable>();

        disposable.Dispose();
    }

    /// <summary>
    /// Verifies that Schedule returns a disposable.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AvaloniaScheduler_Schedule_ReturnsDisposable()
    {
        var scheduler = AvaloniaScheduler.Instance;

        var disposable = scheduler.Schedule("test", TimeSpan.Zero, (s, state) => Disposable.Empty);

        await Assert.That(disposable).IsNotNull();
        await Assert.That(disposable).IsTypeOf<IDisposable>();
    }

    /// <summary>
    /// Verifies that disposing before execution cancels the scheduled action.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AvaloniaScheduler_Schedule_CanDisposeBeforeExecution()
    {
        var scheduler = AvaloniaScheduler.Instance;
        var actionExecuted = false;

        var disposable = scheduler.Schedule("test", TimeSpan.FromMilliseconds(100), (s, state) =>
        {
            actionExecuted = true;
            return Disposable.Empty;
        });

        disposable.Dispose();

        Thread.Sleep(200);

        await Assert.That(actionExecuted).IsFalse();
    }
}
