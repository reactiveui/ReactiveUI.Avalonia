// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Diagnostics;

#if REACTIVE_SHIM
namespace ReactiveUI.Avalonia.Reactive;
#else
namespace ReactiveUI.Avalonia;
#endif

/// <summary>Provides a scheduler that executes actions on the Avalonia UI thread.</summary>
/// <remarks>Use <see cref="AvaloniaScheduler.Instance"/> to access the singleton instance. This scheduler is
/// typically used to marshal work onto the Avalonia UI thread, ensuring thread-safe interaction with UI components.
/// Actions scheduled with zero delay may be executed immediately if already on the dispatcher thread, but excessive
/// immediate scheduling is limited to prevent stack overflows.</remarks>
public sealed class AvaloniaScheduler :
#if REACTIVE_SHIM
    LocalScheduler
#else
    ISequencer
#endif
{
    /// <summary>Gets the singleton instance of the AvaloniaScheduler.</summary>
    /// <remarks>Use this property to access the default scheduler for Avalonia operations. The instance is
    /// thread-safe and intended for global use throughout the application.</remarks>
    public static readonly AvaloniaScheduler Instance = new();

    /// <summary>
    /// Users can schedule actions on the dispatcher thread while being on the correct thread already.
    /// We are optimizing this case by invoking user callback immediately which can lead to stack overflows in certain cases.
    /// To prevent this we are limiting amount of reentrant calls to <see cref="Schedule{TState}"/> before we will
    /// schedule on a dispatcher anyway.
    /// </summary>
    private const int MaxReentrantSchedules = 32;

    /// <summary>Tracks the current depth of reentrant schedule calls.</summary>
    private int _reentrancyGuard;

    /// <summary>Initializes a new instance of the <see cref="AvaloniaScheduler"/> class.</summary>
    private AvaloniaScheduler()
    {
    }

#if !REACTIVE_SHIM
    /// <inheritdoc/>
    public DateTimeOffset Now => DateTimeOffset.Now;

    /// <inheritdoc/>
    public long Timestamp => Stopwatch.GetTimestamp();

    /// <inheritdoc/>
    public void Schedule(IWorkItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        _ = Schedule(item, TimeSpan.Zero, static (_, workItem) =>
        {
            workItem.Execute();
            return EmptyDisposable.Instance;
        });
    }

    /// <inheritdoc/>
    public void Schedule(IWorkItem item, long dueTimestamp)
    {
        ArgumentNullException.ThrowIfNull(item);

        var remainingTicks = dueTimestamp - Timestamp;
        var dueTime = remainingTicks <= 0
            ? TimeSpan.Zero
            : TimeSpan.FromSeconds((double)remainingTicks / Stopwatch.Frequency);

        _ = Schedule(item, dueTime, static (_, workItem) =>
        {
            workItem.Execute();
            return EmptyDisposable.Instance;
        });
    }
#endif
    /// <summary>Schedules an action for execution on the Avalonia UI thread.</summary>
    /// <typeparam name="TState">The scheduled state type.</typeparam>
    /// <param name="state">The state passed to the scheduled action.</param>
    /// <param name="dueTime">The relative due time.</param>
    /// <param name="action">The action to execute.</param>
    /// <returns>A disposable that cancels the scheduled work when possible.</returns>
#if REACTIVE_SHIM
    public override IDisposable Schedule<TState>(
        TState state,
        TimeSpan dueTime,
        Func<IScheduler, TState, IDisposable> action)
#else
    public IDisposable Schedule<TState>(
        TState state,
        TimeSpan dueTime,
        Func<IScheduler, TState, IDisposable> action)
#endif
    {
        ArgumentNullException.ThrowIfNull(action);

        if (dueTime == TimeSpan.Zero)
        {
            if (!Dispatcher.UIThread.CheckAccess())
            {
                return PostOnDispatcher(state, action);
            }

            if (_reentrancyGuard >= MaxReentrantSchedules)
            {
                return PostOnDispatcher(state, action);
            }

            try
            {
                _reentrancyGuard++;

                return action(this, state);
            }
            finally
            {
                _reentrancyGuard--;
            }
        }

        var scheduledWork = new ScheduledWork<TState>(this, state, action);
        scheduledWork.Disposables.Add(Disposable.Empty);
        scheduledWork.Disposables.Add(RunOnce(scheduledWork.Execute, dueTime));

        return scheduledWork.Disposables;
    }

    /// <summary>Posts scheduled work to the Avalonia dispatcher.</summary>
    /// <typeparam name="TState">The scheduled state type.</typeparam>
    /// <param name="state">The state passed to the scheduled action.</param>
    /// <param name="action">The action to execute.</param>
    /// <returns>A disposable that cancels the posted work when possible.</returns>
    internal CompositeDisposable PostOnDispatcher<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
    {
        var scheduledWork = new ScheduledWork<TState>(this, state, action);

        Dispatcher.UIThread.Post(
                                 scheduledWork.ExecuteUnlessCancelled,
                                 DispatcherPriority.Background);

        scheduledWork.Disposables.Add(scheduledWork.Cancellation);

        return scheduledWork.Disposables;
    }

    /// <summary>Runs a callback once after the specified interval on the Avalonia UI thread.</summary>
    /// <remarks>This mirrors <see cref="DispatcherTimer.RunOnce(Action, TimeSpan, DispatcherPriority)"/> but
    /// explicitly binds the timer to <see cref="Dispatcher.UIThread"/>. Avalonia's built-in helper creates the
    /// timer against <see cref="Dispatcher.CurrentDispatcher"/>, so delayed work scheduled from a background
    /// thread would otherwise tick on that thread and break the scheduler's UI-thread guarantee.</remarks>
    /// <param name="action">The callback to invoke after the interval elapses.</param>
    /// <param name="interval">The interval after which to invoke the callback.</param>
    /// <param name="priority">The dispatcher priority to use.</param>
    /// <returns>A disposable that cancels the timer.</returns>
    private static IDisposable RunOnce(
        Action action,
        TimeSpan interval,
        DispatcherPriority priority = default)
    {
        interval = (interval != TimeSpan.Zero) ? interval : TimeSpan.FromTicks(1);

        var timer = new DispatcherTimer(priority, Dispatcher.UIThread) { Interval = interval };

        timer.Tick += (_, _) =>
        {
            action();
            timer.Stop();
        };

        timer.Start();

        return Disposable.Create(() => timer.Stop());
    }

    /// <summary>Stores scheduled dispatcher work without compiler-generated callback bodies.</summary>
    /// <typeparam name="TState">The scheduled state type.</typeparam>
    internal sealed class ScheduledWork<TState>
    {
        /// <summary>The scheduler executing the callback.</summary>
        private readonly AvaloniaScheduler _scheduler;

        /// <summary>The state passed to the scheduled callback.</summary>
        private readonly TState _state;

        /// <summary>The scheduled callback.</summary>
        private readonly Func<IScheduler, TState, IDisposable> _action;

        /// <summary>Initializes a new instance of the <see cref="ScheduledWork{TState}"/> class.</summary>
        /// <param name="scheduler">The scheduler executing the callback.</param>
        /// <param name="state">The scheduled state.</param>
        /// <param name="action">The scheduled callback.</param>
        public ScheduledWork(AvaloniaScheduler scheduler, TState state, Func<IScheduler, TState, IDisposable> action)
        {
            _scheduler = scheduler;
            _state = state;
            _action = action;
        }

        /// <summary>Gets the disposable collection for scheduled work.</summary>
        public CompositeDisposable Disposables { get; } = new();

        /// <summary>Gets the cancellation source for posted dispatcher work.</summary>
        public CancellationDisposable Cancellation { get; } = new();

        /// <summary>Executes the scheduled callback.</summary>
        public void Execute() =>
            Disposables.Add(_action(_scheduler, _state));

        /// <summary>Executes the scheduled callback when posted work has not been cancelled.</summary>
        public void ExecuteUnlessCancelled()
        {
            if (Cancellation.Token.IsCancellationRequested)
            {
                return;
            }

            Execute();
        }
    }
}
