// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using ReactiveUI;

namespace ReactiveUIDemo.ViewModels;

/// <summary>View model for the routed view host page in the demo application.</summary>
internal sealed class RoutedViewHostPageViewModel : ReactiveObject, IScreen
{
    /// <summary>Initializes a new instance of the <see cref="RoutedViewHostPageViewModel"/> class.</summary>
    private RoutedViewHostPageViewModel()
    {
    }

    /// <inheritdoc/>
    public RoutingState Router { get; } = new();

    /// <summary>Gets the Foo view model.</summary>
    internal FooViewModel Foo { get; private set; } = null!;

    /// <summary>Gets the Bar view model.</summary>
    internal BarViewModel Bar { get; private set; } = null!;

    /// <summary>Creates an initialized routed view-host page view model.</summary>
    /// <returns>The initialized view model.</returns>
    internal static RoutedViewHostPageViewModel Create()
    {
        var viewModel = new RoutedViewHostPageViewModel();
        viewModel.Initialize();
        return viewModel;
    }

    /// <summary>Navigates to the Foo view.</summary>
    internal void ShowFoo() => Router.Navigate.Execute(Foo);

    /// <summary>Navigates to the Bar view.</summary>
    internal void ShowBar() => Router.Navigate.Execute(Bar);

    /// <summary>Initializes routing after construction has completed.</summary>
    private void Initialize()
    {
        Foo = new(this);
        Bar = new(this);
        _ = Router.Navigate.Execute(Foo);
    }
}
