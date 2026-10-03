// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Avalonia;
using Avalonia.iOS;
using Foundation;

namespace ReactiveUI.Avalonia.Example.iOS;

/// <summary>Launches the example app and forwards iOS application events to Avalonia.</summary>
[Register("AppDelegate")]
public sealed class AppDelegate : AvaloniaAppDelegate<App>
{
    /// <inheritdoc/>
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder) =>
        base.CustomizeAppBuilder(builder).UseShowcase();
}
