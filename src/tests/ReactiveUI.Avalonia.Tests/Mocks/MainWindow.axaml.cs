// Copyright (c) 2019-2026 ReactiveUI and Avalonia Teams, and Contributors. All rights reserved.
// Licensed under the MIT license.
// See the LICENSE file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUIDemo.ViewModels;

namespace ReactiveUIDemo;

/// <summary>
/// Main window for the demo application.
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }

    /// <summary>
    /// Loads the XAML for this window.
    /// </summary>
    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
