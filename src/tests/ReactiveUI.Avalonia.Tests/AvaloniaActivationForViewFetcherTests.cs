// Copyright (c) 2019-2026 ReactiveUI and Avalonia Teams, and Contributors. All rights reserved.
// Licensed under the MIT license.
// See the LICENSE file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ReactiveUI.Avalonia.Tests;

/// <summary>
/// Tests for the AvaloniaActivationForViewFetcher activation behavior.
/// </summary>
public class AvaloniaActivationForViewFetcherTests
{
    /// <summary>
    /// Verifies that GetAffinityForView returns a positive value for Control types.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetAffinityForView_Control_ReturnsPositive()
    {
        var sut = new AvaloniaActivationForViewFetcher();
        await Assert.That(sut.GetAffinityForView(typeof(Button))).IsGreaterThan(0);
    }

    /// <summary>
    /// Verifies that GetActivationForView emits true on Loaded and false on Unloaded.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetActivationForView_Control_EmitsOnLoadedAndUnloaded()
    {
        var sut = new AvaloniaActivationForViewFetcher();
        var btn = new ActivatableButton();
        bool? last = null;

        using var sub = sut.GetActivationForView(btn).Subscribe(b => last = b);

        btn.RaiseEvent(new RoutedEventArgs(Button.LoadedEvent));
        await Assert.That(last).IsTrue();

        btn.RaiseEvent(new RoutedEventArgs(Button.UnloadedEvent));
        await Assert.That(last).IsFalse();
    }

    /// <summary>
    /// A button that implements IActivatableView for testing activation.
    /// </summary>
    private sealed class ActivatableButton : Button, IActivatableView
    {
        /// <summary>
        /// Gets the associated view. Always null for test purposes.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Required as instance member for IActivatableView pattern.")]
        public IViewFor? ViewFor => null;
    }
}
