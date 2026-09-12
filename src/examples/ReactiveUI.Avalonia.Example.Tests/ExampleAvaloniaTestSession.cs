// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Headless;

namespace ReactiveUI.Avalonia.Example.Tests;

/// <summary>Provides the shared headless Avalonia test session.</summary>
public static class ExampleAvaloniaTestSession
{
    /// <summary>The lazily-created headless session.</summary>
    private static readonly Lazy<HeadlessUnitTestSession> Session =
        new(static () => HeadlessUnitTestSession.StartNew(typeof(ExampleAvaloniaTestSession), AvaloniaTestIsolationLevel.PerAssembly), LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>Gets the shared headless session.</summary>
    internal static HeadlessUnitTestSession Instance => Session.Value;

    /// <summary>Uses the production application setup with a headless rendering backend.</summary>
    /// <returns>The configured application builder.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AppBuilder BuildAvaloniaApp() => Program.BuildAvaloniaApp().UseHeadless(new());
}
