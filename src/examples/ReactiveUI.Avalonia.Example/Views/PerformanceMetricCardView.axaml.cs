// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using ReactiveUI.Avalonia.Example.ViewModels;

namespace ReactiveUI.Avalonia.Example.Views;

/// <summary>A contracted metric card view for performance metrics.</summary>
public sealed partial class PerformanceMetricCardView : ReactiveUserControl<MetricCardViewModel>
{
    /// <summary>Initializes a new instance of the <see cref="PerformanceMetricCardView"/> class.</summary>
    [RequiresUnreferencedCode("ReactiveUserControl activation evaluates expression-based member chains via reflection.")]
    public PerformanceMetricCardView() => InitializeComponent();
}
