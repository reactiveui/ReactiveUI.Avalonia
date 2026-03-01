// Copyright (c) 2019-2026 ReactiveUI and Avalonia Teams, and Contributors. All rights reserved.
// Licensed under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using Avalonia.Controls;
using Splat;

namespace ReactiveUI.Avalonia.Tests;

/// <summary>
/// Tests for ViewModelViewHost and RoutedViewHost navigation behavior.
/// </summary>
public class ViewHostsNavigationTests
{
    /// <summary>
    /// Verifies that ViewModelViewHost navigates to a resolved view and sets its ViewModel.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ViewModelViewHost_Navigates_To_Resolved_View()
    {
        RegisterViews();
        var host = new ViewModelViewHost { DefaultContent = "default" };
        var vm = new VmB();

        var m = typeof(ViewModelViewHost).GetMethod("NavigateToViewModel", BindingFlags.Instance | BindingFlags.NonPublic);
        m!.Invoke(host, new object?[] { vm, null });

        await Assert.That(host.Content).IsTypeOf<ViewB>();
        await Assert.That(((IViewFor)host.Content!).ViewModel).IsSameReferenceAs(vm);
    }

    /// <summary>
    /// Verifies that ViewModelViewHost falls back to default content when no view is found.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ViewModelViewHost_When_No_View_Falls_Back_To_Default()
    {
        var host = new ViewModelViewHost { DefaultContent = "default" };

        var m = typeof(ViewModelViewHost).GetMethod("NavigateToViewModel", BindingFlags.Instance | BindingFlags.NonPublic);
        m!.Invoke(host, new object?[] { new object(), null });

        await Assert.That(host.Content).IsEqualTo("default");
    }

    /// <summary>
    /// Verifies that RoutedViewHost navigates to a resolved view when a Router is set.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RoutedViewHost_Navigates_To_Resolved_View_When_Router_Set()
    {
        RegisterViews();
        var screen = new ScreenImpl();
        var host = new RoutedViewHost { DefaultContent = "def", Router = screen.Router };
        var vm = new VmA(screen);

        var m = typeof(RoutedViewHost).GetMethod("NavigateToViewModel", BindingFlags.Instance | BindingFlags.NonPublic);
        m!.Invoke(host, new object?[] { vm, null });

        await Assert.That(host.Content).IsTypeOf<ViewA>();
        await Assert.That(((IViewFor)host.Content!).ViewModel).IsSameReferenceAs(vm);
    }

    /// <summary>
    /// Verifies that RoutedViewHost falls back to default content when no Router is set.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RoutedViewHost_No_Router_Falls_Back_To_Default()
    {
        var host = new RoutedViewHost { DefaultContent = "def" };

        var m = typeof(RoutedViewHost).GetMethod("NavigateToViewModel", BindingFlags.Instance | BindingFlags.NonPublic);
        m!.Invoke(host, new object?[] { new object(), null });

        await Assert.That(host.Content).IsEqualTo("def");
    }

    /// <summary>
    /// Registers test views into the locator.
    /// </summary>
    private static void RegisterViews()
    {
        Locator.CurrentMutable.Register(() => new ViewA(), typeof(IViewFor<VmA>));
        Locator.CurrentMutable.Register(() => new ViewB(), typeof(IViewFor<VmB>));
    }

    /// <summary>
    /// A routable view model for testing.
    /// </summary>
    private sealed class VmA : ReactiveObject, IRoutableViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VmA"/> class.
        /// </summary>
        /// <param name="screen">The host screen.</param>
        public VmA(IScreen screen)
        {
            HostScreen = screen;
        }

        /// <summary>
        /// Gets the URL path segment.
        /// </summary>
        public string? UrlPathSegment => "A";

        /// <summary>
        /// Gets the host screen.
        /// </summary>
        public IScreen HostScreen { get; }
    }

    /// <summary>
    /// A simple view model for testing non-routable navigation.
    /// </summary>
    private sealed class VmB : ReactiveObject
    {
    }

    /// <summary>
    /// A view for VmA.
    /// </summary>
    private sealed class ViewA : UserControl, IViewFor<VmA>
    {
        /// <summary>
        /// Gets or sets the view model.
        /// </summary>
        public VmA? ViewModel { get; set; }

        /// <inheritdoc/>
        object? IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (VmA?)value;
        }

        /// <inheritdoc/>
        public override string ToString() => nameof(ViewA);
    }

    /// <summary>
    /// A view for VmB.
    /// </summary>
    private sealed class ViewB : UserControl, IViewFor<VmB>
    {
        /// <summary>
        /// Gets or sets the view model.
        /// </summary>
        public VmB? ViewModel { get; set; }

        /// <inheritdoc/>
        object? IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (VmB?)value;
        }

        /// <inheritdoc/>
        public override string ToString() => nameof(ViewB);
    }

    /// <summary>
    /// A screen implementation for testing routing.
    /// </summary>
    private sealed class ScreenImpl : ReactiveObject, IScreen
    {
        /// <summary>
        /// Gets the routing state.
        /// </summary>
        public RoutingState Router { get; } = new();
    }
}
