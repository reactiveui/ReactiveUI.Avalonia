// Copyright (c) 2019-2026 ReactiveUI and Avalonia Teams, and Contributors. All rights reserved.
// Licensed under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI;

namespace ReactiveUIDemo.ViewModels;

/// <summary>
/// View model for the main window of the demo application.
/// </summary>
internal sealed class MainWindowViewModel : ReactiveObject
{
    /// <summary>
    /// Gets the routed view host page view model.
    /// </summary>
    public RoutedViewHostPageViewModel RoutedViewHost { get; } = new();
}
