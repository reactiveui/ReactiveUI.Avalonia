// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI;

namespace ReactiveUI.Avalonia.Example.ViewModels;

/// <summary>Introduces the showcase features.</summary>
public sealed class OverviewViewModel : PageViewModel
{
    /// <summary>Initializes a new instance of the <see cref="OverviewViewModel"/> class.</summary>
    /// <param name="hostScreen">The owning screen.</param>
    public OverviewViewModel(IScreen hostScreen)
        : base(hostScreen, "overview", "ReactiveUI.Avalonia showcase", "A compact tour through routing, binding, activation, interactions, and live local data.")
    {
    }

    /// <summary>Gets feature labels rendered by the overview page.</summary>
    public IReadOnlyList<FeatureViewModel> Features { get; } =
    [
        new("RoutedViewHost shell navigation"),
        new("ViewModelViewHost contracts and automatic item templates"),
        new("ReactiveUI Bind, OneWayBind, and Avalonia compiled bindings"),
        new("ReactiveCommand can-execute, async execution, and failures"),
        new("ObservableAsPropertyHelper computed state"),
        new("WhenActivated disposal for live subscriptions"),
        new("GetSubject and GetBindingSubject property bridges"),
        new("AutoSuspendHelper restores input and threshold between sessions")
    ];
}
