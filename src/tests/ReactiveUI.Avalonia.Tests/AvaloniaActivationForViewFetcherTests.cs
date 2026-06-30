// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ReactiveUI.Avalonia.Tests;

/// <summary>Tests for the AvaloniaActivationForViewFetcher activation behavior.</summary>
public class AvaloniaActivationForViewFetcherTests
{
    /// <summary>Verifies that GetAffinityForView returns a positive value for Control types.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetAffinityForView_Control_ReturnsPositive()
    {
        var sut = new AvaloniaActivationForViewFetcher();
        await Assert.That(sut.GetAffinityForView(typeof(Button))).IsGreaterThan(0);
    }

    /// <summary>Verifies that GetAffinityForView returns zero for non-visual types.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetAffinityForView_NonVisual_ReturnsZero()
    {
        var sut = new AvaloniaActivationForViewFetcher();
        await Assert.That(sut.GetAffinityForView(typeof(object))).IsEqualTo(0);
    }

    /// <summary>Verifies that non-visual activatable views start inactive.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetActivationForView_NonVisual_ReturnsInactive()
    {
        var sut = new AvaloniaActivationForViewFetcher();
        var view = new ActivatableOnly();
        bool? last = null;

        using var sub = sut.GetActivationForView(view).SubscribeSafe(b => last = b, static error => throw error);

        await Assert.That(last).IsFalse();
    }

    /// <summary>Verifies that GetActivationForView emits true on Loaded and false on Unloaded.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetActivationForView_Control_EmitsOnLoadedAndUnloaded()
    {
        var sut = new AvaloniaActivationForViewFetcher();
        var btn = new ActivatableButton();
        bool? last = null;

        using var sub = sut.GetActivationForView(btn).SubscribeSafe(b => last = b, static error => throw error);

        btn.RaiseEvent(new RoutedEventArgs(Button.LoadedEvent));
        await Assert.That(last).IsTrue();

        btn.RaiseEvent(new RoutedEventArgs(Button.UnloadedEvent));
        await Assert.That(last).IsFalse();
    }

    /// <summary>Verifies that non-control visuals emit activation changes for visual tree attachment.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetActivationForView_Visual_EmitsOnAttachAndDetach()
    {
        var sut = new AvaloniaActivationForViewFetcher();
        var host = new VisualHost();
        var visual = new ActivatableVisual();
        var window = new Window { Content = host };
        bool? last = null;

        using var sub = sut.GetActivationForView(visual).SubscribeSafe(b => last = b, static error => throw error);

        try
        {
            host.AddChild(visual);
            window.Show();

            await Assert.That(last).IsTrue();

            host.RemoveChild(visual);
            await Assert.That(last).IsFalse();
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>A button that implements IActivatableView for testing activation.</summary>
    private sealed class ActivatableButton : Button, IActivatableView;

    /// <summary>An activatable view that is not an Avalonia visual.</summary>
    private sealed class ActivatableOnly : IActivatableView;

    /// <summary>A control that can host raw visual children.</summary>
    private sealed class VisualHost : Control
    {
        /// <summary>Adds a raw visual child.</summary>
        /// <param name="visual">The visual to add.</param>
        public void AddChild(Visual visual) =>
            VisualChildren.Add(visual);

        /// <summary>Removes a raw visual child.</summary>
        /// <param name="visual">The visual to remove.</param>
        public void RemoveChild(Visual visual) =>
            _ = VisualChildren.Remove(visual);
    }

    /// <summary>An activatable non-control visual.</summary>
    private sealed class ActivatableVisual : Visual, IActivatableView;
}
