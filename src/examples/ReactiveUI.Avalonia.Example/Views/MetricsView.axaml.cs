// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using ReactiveUI.Avalonia.Example.ViewModels;
using ReactiveUI.Primitives;

namespace ReactiveUI.Avalonia.Example.Views;

/// <summary>The routed metrics view.</summary>
[System.Diagnostics.DebuggerDisplay("MetricsView: {ToString(),nq}")]
public sealed partial class MetricsView : ReactiveUserControl<MetricsViewModel>
{
    /// <summary>The minimum preview opacity.</summary>
    private const double MinimumPreviewOpacity = 0.35D;

    /// <summary>The preview opacity range.</summary>
    private const double PreviewOpacityRange = 0.65D;

    /// <summary>The maximum percentage value.</summary>
    private const double MaximumPercentage = 100D;

    /// <summary>Initializes a new instance of the <see cref="MetricsView"/> class.</summary>
    [RequiresUnreferencedCode("The view uses reflection-based ReactiveUI activation.")]
    public MetricsView()
    {
        InitializeComponent();
        _ = this.WhenActivated(disposables =>
        {
            var preview = ThresholdPreview.GetBindingSubject(Visual.OpacityProperty);
            disposables.Add(ThresholdSlider
                .GetSubject(RangeBase.ValueProperty)
                .Select(static value => new BindingValue<double>(MinimumPreviewOpacity + (value / MaximumPercentage * PreviewOpacityRange)))
                .SubscribeSafe(preview.OnNext, static error => throw error));
        });
    }
}
