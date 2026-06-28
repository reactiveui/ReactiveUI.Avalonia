// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Linq.Expressions;
using Avalonia;
using Avalonia.Controls;

namespace ReactiveUI.Avalonia.Tests;

/// <summary>Tests for the AvaloniaObjectObservableForProperty notification behavior.</summary>
public class AvaloniaObjectObservableForPropertyTests
{
    /// <summary>Verifies that GetAffinity returns a positive value for an AvaloniaObject with a known property.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetAffinity_AvaloniaObjectWithProperty_ReturnsPositive()
    {
        var sut = new AvaloniaObjectObservableForProperty();
        var affinity = ((ICreatesObservableForProperty)sut).GetAffinityForObject(typeof(TestControl), nameof(TestControl.Text));
        await Assert.That(affinity).IsGreaterThan(0);
    }

    /// <summary>Verifies that GetAffinity returns zero for a non-AvaloniaObject type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetAffinity_NonAvaloniaObject_ReturnsZero()
    {
        var sut = new AvaloniaObjectObservableForProperty();
        var affinity = ((ICreatesObservableForProperty)sut).GetAffinityForObject(typeof(object), "Foo");
        await Assert.That(affinity).IsEqualTo(0);
    }

    /// <summary>Verifies that GetAffinity returns zero for a null type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetAffinity_NullType_ReturnsZero()
    {
        var sut = new AvaloniaObjectObservableForProperty();
        var affinity = sut.GetAffinityForObject(null, "Foo", beforeChanged: false);
        await Assert.That(affinity).IsEqualTo(0);
    }

    /// <summary>Verifies that GetAffinity returns zero for a missing Avalonia property.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetAffinity_AvaloniaObjectMissingProperty_ReturnsZero()
    {
        var sut = new AvaloniaObjectObservableForProperty();
        var affinity = ((ICreatesObservableForProperty)sut).GetAffinityForObject(typeof(TestControl), "Missing");
        await Assert.That(affinity).IsEqualTo(0);
    }

    /// <summary>Verifies that GetNotification emits changes when a property value changes.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetNotification_HappyPath_EmitsOnChange()
    {
        var ctrl = new TestControl();
        var sut = new AvaloniaObjectObservableForProperty();
        Expression<Func<string?>> expr = () => ctrl.Text;
        var changes = ((ICreatesObservableForProperty)sut).GetNotificationForProperty(ctrl, expr, nameof(TestControl.Text));

        IObservedChange<object?, object?>? last = null;
        using var sub = changes.SubscribeSafe(c => last = c, static error => throw error);

        ctrl.Text = "hello";

        await Assert.That(last).IsNotNull();
        await Assert.That(last!.Sender).IsEqualTo(ctrl);
        await Assert.That(last.Value).IsEqualTo("hello");
    }

    /// <summary>Verifies that the beforeChanged overload delegates to the property notification path.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetNotification_BeforeChangedOverload_EmitsOnChange()
    {
        var ctrl = new TestControl();
        var sut = new AvaloniaObjectObservableForProperty();
        Expression<Func<string?>> expr = () => ctrl.Text;
        var changes = ((ICreatesObservableForProperty)sut).GetNotificationForProperty(ctrl, expr, nameof(TestControl.Text), beforeChanged: true);

        IObservedChange<object?, object?>? last = null;
        using var sub = changes.SubscribeSafe(c => last = c, static error => throw error);

        ctrl.Text = "before";

        await Assert.That(last).IsNotNull();
        await Assert.That(last!.Value).IsEqualTo("before");
    }

    /// <summary>Verifies that GetNotification throws when a missing property name is specified.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetNotification_MissingProperty_ThrowsImmediately()
    {
        var ctrl = new TestControl();
        var sut = new AvaloniaObjectObservableForProperty();
        Expression<Func<string?>> expr = () => ctrl.Text;

        await Assert.That(() =>
            ((ICreatesObservableForProperty)sut).GetNotificationForProperty(ctrl, expr, "Missing", beforeChanged: false, suppressWarnings: true)).ThrowsExactly<MissingMemberException>();
    }

    /// <summary>Verifies that missing property notifications throw when warning logging is enabled.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetNotification_MissingPropertyWithWarning_ThrowsImmediately()
    {
        var ctrl = new TestControl();
        var sut = new AvaloniaObjectObservableForProperty();
        Expression<Func<string?>> expr = () => ctrl.Text;

        await Assert.That(() =>
            ((ICreatesObservableForProperty)sut).GetNotificationForProperty(ctrl, expr, "Missing", beforeChanged: false, suppressWarnings: false)).ThrowsExactly<MissingMemberException>();
    }

    /// <summary>Verifies that GetNotification throws when the sender is not an AvaloniaObject.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetNotification_NonAvaloniaSender_Throws()
    {
        var sut = new AvaloniaObjectObservableForProperty();
        Expression<Func<object?>> expr = () => new object();
        await Assert.That(() =>
            ((ICreatesObservableForProperty)sut).GetNotificationForProperty(new object(), expr, "Foo")).ThrowsExactly<InvalidOperationException>();
    }

    /// <summary>A test control with a styled Text property for testing property notifications.</summary>
    private sealed class TestControl : Control
    {
        /// <summary>The styled property for <see cref="Text"/>.</summary>
        public static readonly StyledProperty<string?> TextProperty =
            AvaloniaProperty.Register<TestControl, string?>(nameof(Text));

        /// <summary>Gets or sets the text value.</summary>
        public string? Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
    }
}
