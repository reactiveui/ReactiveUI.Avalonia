// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Avalonia;

namespace ReactiveUI.Avalonia.Example.Desktop;

/// <summary>The desktop entry point for the example app.</summary>
public static class Program
{
    /// <summary>Runs the example app.</summary>
    /// <param name="args">Command-line arguments.</param>
    [STAThread]
    public static void Main(string[] args) => _ = BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    /// <summary>Builds the Avalonia app with the shared showcase setup and the desktop platform backend.</summary>
    /// <returns>The configured app builder.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AppBuilder BuildAvaloniaApp() =>
        ExampleAppBuilderExtensions.Create()
            .UsePlatformDetect()
            .LogToTrace();
}
