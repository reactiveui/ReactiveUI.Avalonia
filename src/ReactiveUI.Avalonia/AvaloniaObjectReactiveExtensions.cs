// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
#if REACTIVE_SHIM
namespace ReactiveUI.Avalonia.Reactive;
#else
namespace ReactiveUI.Avalonia;
#endif

/// <summary>Provides extension methods for creating reactive property signals for Avalonia properties.</summary>
/// <remarks>These methods facilitate integration between Avalonia's property system and reactive programming paradigms by exposing property values as reactive subjects or signals. This allows developers to observe changes and push updates to properties using standard reactive interfaces. The extension methods are intended for use with Avalonia objects and properties, supporting both simple and binding-aware scenarios.</remarks>
public static class AvaloniaObjectReactiveExtensions
{
    /// <summary>Extends Avalonia objects with reactive property signals.</summary>
    /// <param name="o">The Avalonia object to extend.</param>
    extension(AvaloniaObject o)
    {
    /// <summary>Creates a reactive property signal for the specified Avalonia property.</summary>
    /// <remarks>The returned value can be used to observe changes to the property and set its value reactively. This is useful for integrating Avalonia properties with reactive programming patterns.</remarks>
    /// <param name="property">The Avalonia property to observe and set values for.</param>
    /// <param name="priority">The binding priority to use when setting the property value. Defaults to <see cref="BindingPriority.LocalValue"/>.</param>
    /// <returns>A reactive property signal that emits changes to the property value and allows property updates.</returns>
#if REACTIVE_SHIM
    public ISubject<object?> GetSubject(
            AvaloniaProperty property,
            BindingPriority priority = BindingPriority.LocalValue) =>
            Subject.Create<object?>(
                Observer.Create<object?>(x => _ = o.SetValue(property, x, priority)),
                o.GetObservable(property));
#else
    public ISignal<object?> GetSubject(
            AvaloniaProperty property,
            BindingPriority priority = BindingPriority.LocalValue) =>
            new AvaloniaPropertySignal<object?>(
                x => _ = o.SetValue(property, x, priority),
                o.GetObservable(property));
#endif

    /// <summary>Creates a typed reactive property signal for the specified Avalonia property.</summary>
    /// <remarks>The returned value allows two-way reactive binding to the specified property. Pushing a value to the signal updates the property, and changes to the property are emitted by the signal.</remarks>
    /// <typeparam name="T">The type of the value stored in the Avalonia property.</typeparam>
    /// <param name="property">The Avalonia property to bind to the signal. Cannot be null.</param>
    /// <param name="priority">The binding priority to use when setting the property value. Defaults to <see cref="BindingPriority.LocalValue"/>.</param>
    /// <returns>A typed reactive property signal that emits property changes and updates the property when new values are pushed.</returns>
#if REACTIVE_SHIM
    public ISubject<T> GetSubject<T>(
            AvaloniaProperty<T> property,
            BindingPriority priority = BindingPriority.LocalValue) =>
            Subject.Create<T>(
                Observer.Create<T>(x => _ = o.SetValue(property, x, priority)),
                o.GetObservable(property));
#else
    public ISignal<T> GetSubject<T>(
            AvaloniaProperty<T> property,
            BindingPriority priority = BindingPriority.LocalValue) =>
            new AvaloniaPropertySignal<T>(
                x => _ = o.SetValue(property, x, priority),
                o.GetObservable(property));
#endif

#if REACTIVE_SHIM
    /// <summary>Creates a reactive binding-value signal for the specified Avalonia property.</summary>
    /// <param name="property">The Avalonia property to bind to and observe for changes.</param>
    /// <param name="priority">The binding priority to use when setting the property value. Defaults to <see cref="BindingPriority.LocalValue"/>.</param>
    /// <returns>A reactive binding-value signal that emits property value changes and accepts new values to update the property.</returns>
    public ISubject<BindingValue<object?>> GetBindingSubject(
            AvaloniaProperty property,
            BindingPriority priority = BindingPriority.LocalValue) =>
            Subject.Create<BindingValue<object?>>(
                Observer.Create<BindingValue<object?>>(x =>
                {
                    if (!x.HasValue)
                    {
                        return;
                    }

                    _ = o.SetValue(property, x.Value, priority);
                }),
                o.GetBindingObservable(property));
#else
    /// <summary>Creates a reactive binding-value signal for the specified Avalonia property.</summary>
    /// <param name="property">The Avalonia property to bind to and observe for changes.</param>
    /// <param name="priority">The binding priority to use when setting the property value. Defaults to <see cref="BindingPriority.LocalValue"/>.</param>
    /// <returns>A reactive binding-value signal that emits property value changes and accepts new values to update the property.</returns>
    public ISignal<BindingValue<object?>> GetBindingSubject(
            AvaloniaProperty property,
            BindingPriority priority = BindingPriority.LocalValue) =>
            new AvaloniaPropertySignal<BindingValue<object?>>(
                x =>
                {
                    if (!x.HasValue)
                    {
                        return;
                    }

                    _ = o.SetValue(property, x.Value, priority);
                },
                o.GetBindingObservable(property));
#endif

#if REACTIVE_SHIM
    /// <summary>Creates a typed reactive binding-value signal for the specified Avalonia property.</summary>
    /// <typeparam name="T">The type of the value held by the Avalonia property.</typeparam>
    /// <param name="property">The Avalonia property to bind to and observe for value changes.</param>
    /// <param name="priority">The binding priority to use when setting the property's value. Defaults to <see cref="BindingPriority.LocalValue"/>.</param>
    /// <returns>A typed reactive binding-value signal that observes changes and allows values to be set reactively.</returns>
    public ISubject<BindingValue<T>> GetBindingSubject<T>(
            AvaloniaProperty<T> property,
            BindingPriority priority = BindingPriority.LocalValue) =>
            Subject.Create<BindingValue<T>>(
                Observer.Create<BindingValue<T>>(x =>
                {
                    if (!x.HasValue)
                    {
                        return;
                    }

                    _ = o.SetValue(property, x.Value, priority);
                }),
                o.GetBindingObservable(property));
#else
    /// <summary>Creates a typed reactive binding-value signal for the specified Avalonia property.</summary>
    /// <typeparam name="T">The type of the value held by the Avalonia property.</typeparam>
    /// <param name="property">The Avalonia property to bind to and observe for value changes.</param>
    /// <param name="priority">The binding priority to use when setting the property's value. Defaults to <see cref="BindingPriority.LocalValue"/>.</param>
    /// <returns>A typed reactive binding-value signal that observes changes and allows values to be set reactively.</returns>
    public ISignal<BindingValue<T>> GetBindingSubject<T>(
            AvaloniaProperty<T> property,
            BindingPriority priority = BindingPriority.LocalValue) =>
            new AvaloniaPropertySignal<BindingValue<T>>(
                x =>
                {
                    if (!x.HasValue)
                    {
                        return;
                    }

                    _ = o.SetValue(property, x.Value, priority);
                },
                o.GetBindingObservable(property));
#endif
    }

#if !REACTIVE_SHIM
    /// <summary>Bridges an Avalonia property observable with an observer action for the Primitives build.</summary>
    /// <typeparam name="T">The observed value type.</typeparam>
    private sealed class AvaloniaPropertySignal<T> : ISignal<T>
    {
        /// <summary>The action invoked when a value is pushed into the signal.</summary>
        private readonly Action<T> _onNext;

        /// <summary>The source observable for property changes.</summary>
        private readonly IObservable<T> _observable;

        /// <summary>The current number of active observers.</summary>
        private int _observerCount;

        /// <summary>Indicates whether this signal has been disposed.</summary>
        private bool _isDisposed;

        /// <summary>Initializes a new instance of the <see cref="AvaloniaPropertySignal{T}"/> class.</summary>
        /// <param name="onNext">The action invoked when a value is pushed.</param>
        /// <param name="observable">The source observable for property changes.</param>
        public AvaloniaPropertySignal(Action<T> onNext, IObservable<T> observable)
        {
            _onNext = onNext;
            _observable = observable;
        }

        /// <inheritdoc/>
        public bool IsDisposed => _isDisposed;

        /// <inheritdoc/>
        public bool HasObservers => _observerCount > 0;

        /// <inheritdoc/>
        public void OnCompleted() => _isDisposed = true;

        /// <inheritdoc/>
        public void OnError(Exception error)
        {
            _isDisposed = true;
            if (error is null)
            {
                return;
            }

            throw error;
        }

        /// <inheritdoc/>
        public void OnNext(T value)
        {
            if (_isDisposed)
            {
                return;
            }

            _onNext(value);
        }

        /// <inheritdoc/>
        public IDisposable Subscribe(IObserver<T> observer)
        {
            _ = Interlocked.Increment(ref _observerCount);

            try
            {
                var subscription = _observable.Subscribe(observer);
                return Disposable.Create((subscription, this), static state =>
                {
                    state.subscription.Dispose();
                    _ = Interlocked.Decrement(ref state.Item2._observerCount);
                });
            }
            catch
            {
                _ = Interlocked.Decrement(ref _observerCount);
                throw;
            }
        }

        /// <inheritdoc/>
        public void Dispose() => _isDisposed = true;
    }
#endif
}
