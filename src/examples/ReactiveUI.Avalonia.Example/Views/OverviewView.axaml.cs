// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using ReactiveUI;
using ReactiveUI.Avalonia.Example.ViewModels;
using ReactiveUI.Primitives.Disposables;

namespace ReactiveUI.Avalonia.Example.Views;

/// <summary>The routed overview view.</summary>
public sealed partial class OverviewView : ReactiveUserControl<OverviewViewModel>
{
    /// <summary>Initializes a new instance of the <see cref="OverviewView"/> class.</summary>
    [RequiresUnreferencedCode("ReactiveUserControl activation evaluates expression-based member chains via reflection.")]
    [RequiresDynamicCode("The feature list uses expression-based ReactiveUI binding.")]
    public OverviewView()
    {
        InitializeComponent();
        _ = this.WhenActivated(BindView);
    }

    /// <summary>Binds items while the overview is active, allowing the automatic template hook to resolve rows.</summary>
    /// <param name="disposables">The current activation lifetime.</param>
    [RequiresUnreferencedCode("The feature list uses reflection-based property binding.")]
    [RequiresDynamicCode("The feature list uses dynamic ReactiveUI binding.")]
    private void BindView(MultipleDisposable disposables) =>
        disposables.Add(this.OneWayBind(ViewModel, static model => model.Features, static view => view.FeatureItems.ItemsSource));
}
