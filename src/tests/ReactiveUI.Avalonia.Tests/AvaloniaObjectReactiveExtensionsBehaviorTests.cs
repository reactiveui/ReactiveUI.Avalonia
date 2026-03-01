// Copyright (c) 2019-2026 ReactiveUI and Avalonia Teams, and Contributors. All rights reserved.
// Licensed under the MIT license.
// See the LICENSE file in the project root for full license information.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace ReactiveUI.Avalonia.Tests;

/// <summary>
/// Tests for AvaloniaObjectReactiveExtensions GetSubject and GetBindingSubject behavior.
/// </summary>
public class AvaloniaObjectReactiveExtensionsBehaviorTests
{
    /// <summary>
    /// Verifies that GetSubject with an untyped AvaloniaProperty writes and observes values.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetSubject_ObjectRoundtrip_WritesAndObserves()
    {
        var ctrl = new TestControl();
        var subject = ctrl.GetSubject((AvaloniaProperty)TestControl.IntPropProperty);
        object? observed = null;
        using var sub = subject.Subscribe(v => observed = v);

        subject.OnNext(42);

        await Assert.That(ctrl.IntProp).IsEqualTo(42);
        await Assert.That(observed).IsEqualTo(42);
    }

    /// <summary>
    /// Verifies that GetSubject with a generic StyledProperty writes and observes values.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetSubject_GenericRoundtrip_WritesAndObserves()
    {
        var ctrl = new TestControl();
        var subject = ctrl.GetSubject(TestControl.IntPropProperty);
        int? observed = null;
        using var sub = subject.Subscribe(v => observed = v);

        subject.OnNext(7);

        await Assert.That(ctrl.IntProp).IsEqualTo(7);
        await Assert.That(observed).IsEqualTo(7);
    }

    /// <summary>
    /// Verifies that GetBindingSubject with an untyped AvaloniaProperty only writes on HasValue.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetBindingSubject_ObjectRoundtrip_WritesOnlyOnHasValue()
    {
        var ctrl = new TestControl();
        var subject = ctrl.GetBindingSubject((AvaloniaProperty)TestControl.IntPropProperty);
        BindingValue<object?>? observed = null;
        using var sub = subject.Subscribe(v => observed = v);

        subject.OnNext(BindingValue<object?>.DoNothing);
        await Assert.That(ctrl.IntProp).IsEqualTo(default(int));

        subject.OnNext(new BindingValue<object?>(99));
        await Assert.That(ctrl.IntProp).IsEqualTo(99);
        await Assert.That(observed!.HasValue).IsTrue();
    }

    /// <summary>
    /// Verifies that GetBindingSubject with a generic StyledProperty only writes on HasValue.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetBindingSubject_GenericRoundtrip_WritesOnlyOnHasValue()
    {
        var ctrl = new TestControl();
        var subject = ctrl.GetBindingSubject(TestControl.IntPropProperty);
        BindingValue<int>? observed = null;
        using var sub = subject.Subscribe(v => observed = v);

        subject.OnNext(BindingValue<int>.DoNothing);
        await Assert.That(ctrl.IntProp).IsEqualTo(default(int));

        subject.OnNext(new BindingValue<int>(5));
        await Assert.That(ctrl.IntProp).IsEqualTo(5);
        await Assert.That(observed!.HasValue).IsTrue();
    }

    /// <summary>
    /// A test control with an integer styled property.
    /// </summary>
    private class TestControl : Control
    {
        /// <summary>
        /// The styled property for <see cref="IntProp"/>.
        /// </summary>
        public static readonly StyledProperty<int> IntPropProperty =
            AvaloniaProperty.Register<TestControl, int>(nameof(IntProp));

        /// <summary>
        /// Gets or sets the integer property value.
        /// </summary>
        public int IntProp
        {
            get => GetValue(IntPropProperty);
            set => SetValue(IntPropProperty, value);
        }
    }
}
