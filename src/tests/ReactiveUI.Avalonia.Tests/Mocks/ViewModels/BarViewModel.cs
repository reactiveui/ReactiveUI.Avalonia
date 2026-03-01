// Copyright (c) 2019-2026 ReactiveUI and Avalonia Teams, and Contributors. All rights reserved.
// Licensed under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI;

namespace ReactiveUIDemo.ViewModels;

/// <summary>
/// View model for the Bar page in the demo application.
/// </summary>
/// <param name="screen">The host screen for routing.</param>
internal sealed class BarViewModel(IScreen screen) : ReactiveObject, IRoutableViewModel
{
    /// <inheritdoc/>
    public string UrlPathSegment => "Bar";

    /// <inheritdoc/>
    public IScreen HostScreen { get; } = screen;
}
