// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI;

namespace ReactiveUI.Avalonia.Example.ViewModels;

/// <summary>View model for a reusable metric card resolved by contract.</summary>
[System.Diagnostics.DebuggerDisplay("MetricCardViewModel: {Title}")]
public sealed class MetricCardViewModel : ReactiveObject
{
    /// <summary>The value text.</summary>
    private string _value;

    /// <summary>Initializes a new instance of the <see cref="MetricCardViewModel"/> class.</summary>
    /// <param name="title">The card title.</param>
    /// <param name="value">The value text.</param>
    /// <param name="caption">The caption text.</param>
    public MetricCardViewModel(string title, string value, string caption)
    {
        Title = title;
        _value = value;
        Caption = caption;
    }

    /// <summary>Gets the card title.</summary>
    public string Title { get; }

    /// <summary>Gets or sets the value text.</summary>
    public string Value
    {
        get => _value;
        set => this.RaiseAndSetIfChanged(ref _value, value);
    }

    /// <summary>Gets the caption text.</summary>
    public string Caption { get; }
}
