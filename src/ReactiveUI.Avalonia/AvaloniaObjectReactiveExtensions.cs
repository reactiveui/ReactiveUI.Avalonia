// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
#if REACTIVE_SHIM
namespace ReactiveUI.Avalonia.Reactive;
#else
namespace ReactiveUI.Avalonia;
#endif

/// <summary>Provides extension methods for creating reactive property signals for Avalonia properties.</summary>
/// <remarks>
/// These methods integrate Avalonia's property system with reactive programming by exposing property values as
/// reactive subjects or signals. Consumers can observe changes and push updates through standard reactive interfaces.
/// The extensions support both simple and binding-aware property scenarios.
/// </remarks>
public static class AvaloniaObjectReactiveExtensions
{
    /// <summary>Extends Avalonia objects with reactive property signals.</summary>
    /// <param name="o">The Avalonia object to extend.</param>
    extension(AvaloniaObject o)
    {
#if REACTIVE_SHIM
        /// <summary>Creates a reactive property signal for the specified Avalonia property.</summary>
        /// <param name="property">The Avalonia property to observe and update.</param>
        /// <returns>A reactive property signal that emits changes and allows property updates.</returns>
        public ISubject<object?> GetSubject(AvaloniaProperty property) =>
            o.GetSubject(property, BindingPriority.LocalValue);

        /// <summary>Creates a reactive property signal with the specified binding priority.</summary>
        /// <param name="property">The Avalonia property to observe and set values for.</param>
        /// <param name="priority">The binding priority to use when setting the property value.</param>
        /// <returns>A reactive property signal that emits changes and allows updates.</returns>
        public ISubject<object?> GetSubject(
            AvaloniaProperty property,
            BindingPriority priority) =>
            Subject.Create<object?>(
                Observer.Create<object?>(x => _ = o.SetValue(property, x, priority)),
                o.GetObservable(property));
#else
        /// <summary>Creates a reactive property signal for the specified Avalonia property.</summary>
        /// <param name="property">The Avalonia property to observe and update.</param>
        /// <returns>A reactive property signal that emits changes and allows property updates.</returns>
        public ISignal<object?> GetSubject(AvaloniaProperty property) =>
            o.GetSubject(property, BindingPriority.LocalValue);

        /// <summary>Creates a reactive property signal with the specified binding priority.</summary>
        /// <param name="property">The Avalonia property to observe and set values for.</param>
        /// <param name="priority">The binding priority to use when setting the property value.</param>
        /// <returns>A reactive property signal that emits changes and allows updates.</returns>
        public ISignal<object?> GetSubject(
                AvaloniaProperty property,
                BindingPriority priority) =>
                new AvaloniaPropertySignal<object?>(
                    x => _ = o.SetValue(property, x, priority),
                    o.GetObservable(property));
#endif

#if REACTIVE_SHIM
        /// <summary>Creates a typed reactive property signal for the specified Avalonia property.</summary>
        /// <typeparam name="T">The type of value stored in the Avalonia property.</typeparam>
        /// <param name="property">The Avalonia property to observe and update.</param>
        /// <returns>A typed reactive property signal that emits changes and accepts updates.</returns>
        public ISubject<T> GetSubject<T>(AvaloniaProperty<T> property) =>
            o.GetSubject(property, BindingPriority.LocalValue);

        /// <summary>Creates a typed reactive property signal with the specified binding priority.</summary>
        /// <typeparam name="T">The value stored in the Avalonia property.</typeparam>
        /// <param name="property">The Avalonia property to bind to the signal.</param>
        /// <param name="priority">The binding priority to use when setting the property value.</param>
        /// <returns>A typed reactive property signal that emits changes and accepts updates.</returns>
        public ISubject<T> GetSubject<T>(
            AvaloniaProperty<T> property,
            BindingPriority priority) =>
            Subject.Create<T>(
                Observer.Create<T>(x => _ = o.SetValue(property, x, priority)),
                o.GetObservable(property));
#else
        /// <summary>Creates a typed reactive property signal for the specified Avalonia property.</summary>
        /// <typeparam name="T">The type of value stored in the Avalonia property.</typeparam>
        /// <param name="property">The Avalonia property to observe and update.</param>
        /// <returns>A typed reactive property signal that emits changes and accepts updates.</returns>
        public ISignal<T> GetSubject<T>(AvaloniaProperty<T> property) =>
            o.GetSubject(property, BindingPriority.LocalValue);

        /// <summary>Creates a typed reactive property signal with the specified binding priority.</summary>
        /// <typeparam name="T">The value stored in the Avalonia property.</typeparam>
        /// <param name="property">The Avalonia property to bind to the signal.</param>
        /// <param name="priority">The binding priority to use when setting the property value.</param>
        /// <returns>A typed reactive property signal that emits changes and accepts updates.</returns>
        public ISignal<T> GetSubject<T>(
                AvaloniaProperty<T> property,
                BindingPriority priority) =>
                new AvaloniaPropertySignal<T>(
                    x => _ = o.SetValue(property, x, priority),
                    o.GetObservable(property));
#endif

#if REACTIVE_SHIM
    /// <summary>Creates a reactive binding-value signal for the specified Avalonia property.</summary>
    /// <param name="property">The Avalonia property to bind to and observe for changes.</param>
        /// <returns>A reactive binding-value signal that emits changes and accepts property updates.</returns>
        public ISubject<BindingValue<object?>> GetBindingSubject(AvaloniaProperty property) =>
            o.GetBindingSubject(property, BindingPriority.LocalValue);

        /// <summary>Creates a reactive binding-value signal with the specified binding priority.</summary>
        /// <param name="property">The Avalonia property to bind to and observe.</param>
        /// <param name="priority">The binding priority to use when setting the property value.</param>
        /// <returns>A reactive binding-value signal that emits changes and accepts updates.</returns>
        public ISubject<BindingValue<object?>> GetBindingSubject(
            AvaloniaProperty property,
            BindingPriority priority) =>
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
        /// <returns>A reactive binding-value signal that emits changes and accepts property updates.</returns>
        public ISignal<BindingValue<object?>> GetBindingSubject(AvaloniaProperty property) =>
            o.GetBindingSubject(property, BindingPriority.LocalValue);

        /// <summary>Creates a reactive binding-value signal with the specified binding priority.</summary>
        /// <param name="property">The Avalonia property to bind to and observe.</param>
        /// <param name="priority">The binding priority to use when setting the property value.</param>
        /// <returns>A reactive binding-value signal that emits changes and accepts updates.</returns>
        public ISignal<BindingValue<object?>> GetBindingSubject(
                AvaloniaProperty property,
                BindingPriority priority) =>
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
        /// <returns>A typed reactive binding-value signal that observes changes and accepts updates.</returns>
        public ISubject<BindingValue<T>> GetBindingSubject<T>(AvaloniaProperty<T> property) =>
            o.GetBindingSubject(property, BindingPriority.LocalValue);

        /// <summary>Creates a typed reactive binding-value signal with the specified binding priority.</summary>
        /// <typeparam name="T">The value held by the Avalonia property.</typeparam>
        /// <param name="property">The Avalonia property to bind to and observe.</param>
        /// <param name="priority">The binding priority to use when setting the property value.</param>
        /// <returns>A typed reactive binding-value signal that observes changes and accepts updates.</returns>
        public ISubject<BindingValue<T>> GetBindingSubject<T>(
            AvaloniaProperty<T> property,
            BindingPriority priority) =>
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
        /// <returns>A typed reactive binding-value signal that observes changes and accepts updates.</returns>
        public ISignal<BindingValue<T>> GetBindingSubject<T>(AvaloniaProperty<T> property) =>
            o.GetBindingSubject(property, BindingPriority.LocalValue);

        /// <summary>Creates a typed reactive binding-value signal with the specified binding priority.</summary>
        /// <typeparam name="T">The value held by the Avalonia property.</typeparam>
        /// <param name="property">The Avalonia property to bind to and observe.</param>
        /// <param name="priority">The binding priority to use when setting the property value.</param>
        /// <returns>A typed reactive binding-value signal that observes changes and accepts updates.</returns>
        public ISignal<BindingValue<T>> GetBindingSubject<T>(
                AvaloniaProperty<T> property,
                BindingPriority priority) =>
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
    /// <param name="onNext">The action invoked when a value is pushed.</param>
    /// <param name="observable">The source observable for property changes.</param>
    internal sealed class AvaloniaPropertySignal<T>(
        Action<T> onNext,
        IObservable<T> observable) : ISignal<T>
    {
        /// <summary>The action invoked when a value is pushed into the signal.</summary>
        private readonly Action<T> _onNext = onNext;

        /// <summary>The source observable for property changes.</summary>
        private readonly IObservable<T> _observable = observable;

        /// <summary>The current number of active observers.</summary>
        private int _observerCount;

        /// <summary>Indicates whether this signal has been disposed.</summary>
        private bool _isDisposed;

        /// <inheritdoc/>
        public bool IsDisposed => _isDisposed;

        /// <inheritdoc/>
        public bool HasObservers => Volatile.Read(ref _observerCount) > 0;

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
        public void Dispose() => OnCompleted();
    }
#endif
}
