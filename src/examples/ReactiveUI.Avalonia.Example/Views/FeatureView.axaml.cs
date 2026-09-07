// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Diagnostics.CodeAnalysis;
using ReactiveUI.Avalonia.Example.ViewModels;

namespace ReactiveUI.Avalonia.Example.Views;

/// <summary>A reusable feature row resolved by the automatic data-template binding hook.</summary>
public sealed partial class FeatureView : ReactiveUserControl<FeatureViewModel>
{
    /// <summary>Initializes a new instance of the <see cref="FeatureView"/> class.</summary>
    [RequiresUnreferencedCode("The reactive control wires reflection-based activation.")]
    public FeatureView() => InitializeComponent();
}
