// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace ReactiveUI.Avalonia.Reactive.Tests;

/// <summary>Behavioral coverage tests for the ReactiveUI.Avalonia.Reactive shim assembly.</summary>
public class ReactiveShimBehaviorCoverageTests
{
    /// <summary>Verifies that the reactive shim GetSubject overloads write and observe values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveShim_GetSubject_RoundtripsValues()
    {
        var control = new TestControl();
        var untyped = control.GetSubject((AvaloniaProperty)TestControl.IntPropProperty);
        var typed = control.GetSubject(TestControl.IntPropProperty);
        object? untypedObserved = null;
        int typedObserved = 0;

        using var untypedSubscription = untyped.Subscribe(value => untypedObserved = value);
        using var typedSubscription = typed.Subscribe(value => typedObserved = value);

        untyped.OnNext(11);
        typed.OnNext(12);

        await Assert.That(untypedObserved).IsEqualTo(11);
        await Assert.That(typedObserved).IsEqualTo(12);
        await Assert.That(control.IntProp).IsEqualTo(12);
    }

    /// <summary>Verifies that the reactive shim GetBindingSubject overloads write only values with HasValue.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveShim_GetBindingSubject_RoundtripsValues()
    {
        var control = new TestControl();
        var untyped = control.GetBindingSubject((AvaloniaProperty)TestControl.IntPropProperty);
        var typed = control.GetBindingSubject(TestControl.IntPropProperty);
        BindingValue<object?>? untypedObserved = null;
        BindingValue<int>? typedObserved = null;

        using var untypedSubscription = untyped.Subscribe(value => untypedObserved = value);
        using var typedSubscription = typed.Subscribe(value => typedObserved = value);

        untyped.OnNext(BindingValue<object?>.DoNothing);
        typed.OnNext(BindingValue<int>.DoNothing);

        await Assert.That(control.IntProp).IsEqualTo(0);

        untyped.OnNext(new BindingValue<object?>(13));
        typed.OnNext(new BindingValue<int>(14));

        await Assert.That(untypedObserved!.Value.Value).IsEqualTo(13);
        await Assert.That(typedObserved!.Value.Value).IsEqualTo(14);
        await Assert.That(control.IntProp).IsEqualTo(14);
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
