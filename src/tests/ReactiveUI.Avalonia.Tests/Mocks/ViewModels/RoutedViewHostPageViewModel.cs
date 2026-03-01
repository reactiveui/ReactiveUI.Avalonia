// Copyright (c) 2019-2026 ReactiveUI and Avalonia Teams, and Contributors. All rights reserved.
// Licensed under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI;

namespace ReactiveUIDemo.ViewModels;

/// <summary>
/// View model for the routed view host page in the demo application.
/// </summary>
internal sealed class RoutedViewHostPageViewModel : ReactiveObject, IScreen
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RoutedViewHostPageViewModel"/> class.
    /// </summary>
    public RoutedViewHostPageViewModel()
    {
        Foo = new(this);
        Bar = new(this);
        Router.Navigate.Execute(Foo);
    }

    /// <inheritdoc/>
    public RoutingState Router { get; } = new();

    /// <summary>
    /// Gets the Foo view model.
    /// </summary>
    public FooViewModel Foo { get; }

    /// <summary>
    /// Gets the Bar view model.
    /// </summary>
    public BarViewModel Bar { get; }

    /// <summary>
    /// Navigates to the Foo view.
    /// </summary>
    public void ShowFoo() => Router.Navigate.Execute(Foo);

    /// <summary>
    /// Navigates to the Bar view.
    /// </summary>
    public void ShowBar() => Router.Navigate.Execute(Bar);
}
