// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using UIKit;

namespace ReactiveUI.Avalonia.Example.iOS;

/// <summary>The iOS entry point for the example app.</summary>
public static class Application
{
    /// <summary>Starts the iOS application.</summary>
    /// <param name="args">Command-line arguments.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Main(string[] args) => UIApplication.Main(args, null, typeof(AppDelegate));
}
