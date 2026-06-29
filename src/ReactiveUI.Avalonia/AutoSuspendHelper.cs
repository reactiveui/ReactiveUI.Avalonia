// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
#if REACTIVE_SHIM
namespace ReactiveUI.Avalonia.Reactive;
#else
namespace ReactiveUI.Avalonia;
#endif

/// <summary>Provides automatic suspension and state persistence support for Avalonia applications.</summary>
/// <remarks>Use this helper to enable automatic state persistence and restoration in Avalonia applications. Pass
/// the application's lifetime object to the constructor, and call OnFrameworkInitializationCompleted in your
/// App.OnFrameworkInitializationCompleted method to signal application launch. This class integrates with ReactiveUI's
/// RxSuspension system and handles application exit and unhandled exceptions to manage state persistence. It is not
/// intended for use in design mode, where state persistence is disabled.</remarks>
public sealed class AutoSuspendHelper : IEnableLogger, IDisposable
{
    /// <summary>Signals when application state should be persisted.</summary>
    private readonly Signal<IDisposable> _shouldPersistState = new();

    /// <summary>Signals when the application is launching for the first time.</summary>
    private readonly Signal<Unit> _isLaunchingNew = new();

    /// <summary>Signals when process state should be invalidated after an unhandled exception.</summary>
    private readonly Signal<Unit> _shouldInvalidateState = new();

    /// <summary>Initializes a new instance of the <see cref="AutoSuspendHelper"/> class.</summary>
    /// <remarks>If the application is running in design mode, state persistence is disabled. For supported
    /// lifetimes, application exit events are wired to enable state persistence. This constructor should be called
    /// after Avalonia application initialization is completed.</remarks>
    /// <param name="lifetime">The application lifetime object used to determine how application exit and state persistence events are handled.
    /// Must not be null.</param>
    /// <exception cref="NotSupportedException">Thrown if the specified application lifetime type is not supported for detecting application exit events.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="lifetime"/> is null.</exception>
    public AutoSuspendHelper(IApplicationLifetime lifetime)
    {
        RxSuspension.SuspensionHost.IsResuming = Observable.Never<Unit>();
        RxSuspension.SuspensionHost.IsLaunchingNew = _isLaunchingNew;

        if (Design.IsDesignMode)
        {
            this.Log().Debug("Design mode detected. AutoSuspendHelper won't persist app state.");
            RxSuspension.SuspensionHost.ShouldPersistState = Observable.Never<IDisposable>();
        }
        else if (lifetime is IControlledApplicationLifetime controlled)
        {
            this.Log().Debug("Using IControlledApplicationLifetime events to handle app exit.");
            controlled.Exit += (sender, args) => OnControlledApplicationLifetimeExit();
            RxSuspension.SuspensionHost.ShouldPersistState = _shouldPersistState;
        }
        else if (lifetime is not null)
        {
            var type = lifetime.GetType().FullName;
            var message = $"Don't know how to detect app exit event for {type}.";
            throw new NotSupportedException(message);
        }
        else
        {
            const string message = "ApplicationLifetime is null. "
                          + "Ensure you are initializing AutoSuspendHelper "
                          + "after Avalonia application initialization is completed.";
            throw new ArgumentNullException(nameof(lifetime), message);
        }

        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        RxSuspension.SuspensionHost.ShouldInvalidateState = _shouldInvalidateState;
    }

    /// <summary>Signals that the framework initialization process has completed.</summary>
    /// <remarks>This method should be called once all necessary framework setup is finished and the
    /// application is ready to proceed. It notifies observers that initialization is complete, which may trigger
    /// subsequent application logic. Typically used in application startup routines.</remarks>
    public void OnFrameworkInitializationCompleted() => _isLaunchingNew.OnNext(Unit.Default);

    /// <summary>Releases all resources used by the current instance.</summary>
    /// <remarks>Call this method when you are finished using the instance to ensure that all unmanaged and
    /// managed resources are properly released. After calling Dispose, the instance should not be used.</remarks>
    public void Dispose()
    {
        AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
        _shouldPersistState.Dispose();
        _isLaunchingNew.Dispose();
        _shouldInvalidateState.Dispose();
    }

    /// <summary>Handles unhandled process exceptions by invalidating persisted state.</summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="args">The unhandled exception event data.</param>
    internal void OnUnhandledException(object? sender, UnhandledExceptionEventArgs args) =>
        _shouldInvalidateState.OnNext(Unit.Default);

    /// <summary>Handles the exit event for a controlled application lifetime, ensuring that any required state persistence actions are completed before shutdown.</summary>
    /// <remarks>This method blocks until all registered state persistence actions have finished executing. It
    /// should be called during application shutdown to guarantee that state is saved reliably. Calling this method from
    /// a non-shutdown context may result in the application waiting indefinitely.</remarks>
    private void OnControlledApplicationLifetimeExit()
    {
        this.Log().Debug("Received IControlledApplicationLifetime exit event.");
        var manual = new ManualResetEvent(false);
        _shouldPersistState.OnNext(Disposable.Create(() => _ = manual.Set()));

        _ = manual.WaitOne();
        this.Log().Debug("Completed actions on IControlledApplicationLifetime exit event.");
    }
}
