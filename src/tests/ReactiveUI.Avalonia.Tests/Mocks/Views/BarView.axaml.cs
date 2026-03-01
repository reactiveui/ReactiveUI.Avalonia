// Copyright (c) 2019-2026 ReactiveUI and Avalonia Teams, and Contributors. All rights reserved.
// Licensed under the MIT license.
// See the LICENSE file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using ReactiveUIDemo.ViewModels;

namespace ReactiveUIDemo.Views;

/// <summary>
/// View for the <see cref="BarViewModel"/>.
/// </summary>
internal sealed partial class BarView : UserControl, IViewFor<BarViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BarView"/> class.
    /// </summary>
    public BarView() => InitializeComponent();

    /// <inheritdoc/>
    public BarViewModel? ViewModel { get; set; }

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (BarViewModel?)value;
    }

    /// <summary>
    /// Loads the XAML for this view.
    /// </summary>
    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
