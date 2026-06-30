// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace ReactiveUI.Avalonia.Tests;

/// <summary>Tests for AvaloniaObjectReactiveExtensions GetSubject and GetBindingSubject behavior.</summary>
public class AvaloniaObjectReactiveExtensionsBehaviorTests
{
    /// <summary>Verifies that GetSubject with an untyped AvaloniaProperty writes and observes values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetSubject_ObjectRoundtrip_WritesAndObserves()
    {
        var ctrl = new TestControl();
        var subject = ctrl.GetSubject((AvaloniaProperty)TestControl.IntPropProperty);
        object? observed = null;
        using var sub = subject.SubscribeSafe(v => observed = v, static error => throw error);

        subject.OnNext(42);

        await Assert.That(ctrl.IntProp).IsEqualTo(42);
        await Assert.That(observed).IsEqualTo(42);
    }

    /// <summary>Verifies that GetSubject with a generic StyledProperty writes and observes values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetSubject_GenericRoundtrip_WritesAndObserves()
    {
        var ctrl = new TestControl();
        var subject = ctrl.GetSubject(TestControl.IntPropProperty);
        int? observed = null;
        using var sub = subject.SubscribeSafe(v => observed = v, static error => throw error);

        subject.OnNext(7);

        await Assert.That(ctrl.IntProp).IsEqualTo(7);
        await Assert.That(observed).IsEqualTo(7);
    }

    /// <summary>Verifies that GetBindingSubject with an untyped AvaloniaProperty only writes on HasValue.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetBindingSubject_ObjectRoundtrip_WritesOnlyOnHasValue()
    {
        var ctrl = new TestControl();
        var subject = ctrl.GetBindingSubject((AvaloniaProperty)TestControl.IntPropProperty);
        BindingValue<object?>? observed = null;
        using var sub = subject.SubscribeSafe(v => observed = v, static error => throw error);

        subject.OnNext(BindingValue<object?>.DoNothing);
        await Assert.That(ctrl.IntProp).IsEqualTo(default(int));

        subject.OnNext(new BindingValue<object?>(99));
        await Assert.That(ctrl.IntProp).IsEqualTo(99);
        await Assert.That(observed!.HasValue).IsTrue();
    }

    /// <summary>Verifies that GetBindingSubject with a generic StyledProperty only writes on HasValue.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetBindingSubject_GenericRoundtrip_WritesOnlyOnHasValue()
    {
        var ctrl = new TestControl();
        var subject = ctrl.GetBindingSubject(TestControl.IntPropProperty);
        BindingValue<int>? observed = null;
        using var sub = subject.SubscribeSafe(v => observed = v, static error => throw error);

        subject.OnNext(BindingValue<int>.DoNothing);
        await Assert.That(ctrl.IntProp).IsEqualTo(default(int));

        subject.OnNext(new BindingValue<int>(5));
        await Assert.That(ctrl.IntProp).IsEqualTo(5);
        await Assert.That(observed!.HasValue).IsTrue();
    }

    /// <summary>Verifies signal observer count and disposal state transitions.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetSubject_SignalTracksObserversAndDisposedState()
    {
        var ctrl = new TestControl();
        var subject = ctrl.GetSubject(TestControl.IntPropProperty);

        await Assert.That(subject.HasObservers).IsFalse();
        await Assert.That(subject.IsDisposed).IsFalse();

        using (subject.SubscribeSafe(_ => { }, static error => throw error))
        {
            await Assert.That(subject.HasObservers).IsTrue();
        }

        await Assert.That(subject.HasObservers).IsFalse();

        subject.OnCompleted();
        subject.OnNext(123);

        await Assert.That(subject.IsDisposed).IsTrue();
        await Assert.That(ctrl.IntProp).IsEqualTo(default(int));
    }

    /// <summary>Verifies that disposing the signal marks it as disposed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetSubject_SignalDispose_SetsDisposed()
    {
        var ctrl = new TestControl();
        var subject = ctrl.GetSubject(TestControl.IntPropProperty);

        subject.Dispose();

        await Assert.That(subject.IsDisposed).IsTrue();
    }

    /// <summary>Verifies signal error handling for null and non-null errors.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetSubject_SignalOnErrorSetsDisposedAndThrowsNonNullError()
    {
        var ctrl = new TestControl();
        var nullErrorSubject = ctrl.GetSubject(TestControl.IntPropProperty);

        nullErrorSubject.OnError(null!);

        await Assert.That(nullErrorSubject.IsDisposed).IsTrue();

        var errorSubject = ctrl.GetSubject(TestControl.IntPropProperty);

        await Assert.That(() => errorSubject.OnError(new InvalidOperationException("expected"))).ThrowsExactly<InvalidOperationException>();
        await Assert.That(errorSubject.IsDisposed).IsTrue();
    }

    /// <summary>Verifies observer count is restored when subscribing to the backing observable throws.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetSubject_WhenSubscribeThrows_RestoresObserverCount()
    {
        var subject = CreateThrowingSignal();

        await Assert.That(() => subject.SubscribeSafe(_ => { }, static error => throw error)).ThrowsExactly<InvalidOperationException>();
        await Assert.That(subject.HasObservers).IsFalse();
    }

    /// <summary>Creates an AvaloniaPropertySignal backed by a throwing observable.</summary>
    /// <returns>The created signal.</returns>
    private static ISignal<int> CreateThrowingSignal()
    {
        var signalType = typeof(AvaloniaObjectReactiveExtensions)
            .GetNestedType("AvaloniaPropertySignal`1", BindingFlags.NonPublic)!
            .MakeGenericType(typeof(int));

        return (ISignal<int>)Activator.CreateInstance(signalType, new Action<int>(_ => { }), new ThrowingObservable())!;
    }

    /// <summary>An observable that throws when subscribed.</summary>
    private sealed class ThrowingObservable : IObservable<int>
    {
        /// <inheritdoc/>
        public IDisposable Subscribe(IObserver<int> observer) =>
            throw new InvalidOperationException("expected");
    }

    /// <summary>A test control with an integer styled property.</summary>
    private sealed class TestControl : Control
    {
        /// <summary>The styled property for <see cref="IntProp"/>.</summary>
        public static readonly StyledProperty<int> IntPropProperty =
            AvaloniaProperty.Register<TestControl, int>(nameof(IntProp));

        /// <summary>Gets or sets the integer property value.</summary>
        public int IntProp
        {
            get => GetValue(IntPropProperty);
            set => SetValue(IntPropProperty, value);
        }
    }
}
