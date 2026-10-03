// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using Avalonia;
using Avalonia.Browser;

namespace ReactiveUI.Avalonia.Example.Browser;

/// <summary>The browser entry point for the example app.</summary>
[SupportedOSPlatform("browser")]
internal static class Program
{
    /// <summary>Starts the example app in the page element with the id <c>out</c>.</summary>
    /// <param name="args">The page URL.</param>
    /// <returns>A task that completes when the app has started.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Task Main(string[] args) => BuildAvaloniaApp().StartBrowserAppAsync("out");

    /// <summary>Builds the Avalonia app with the shared showcase setup.</summary>
    /// <returns>The configured app builder.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static AppBuilder BuildAvaloniaApp() => ExampleAppBuilderExtensions.Create();
}
