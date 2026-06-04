// Copyright (c) 2019-2026 ReactiveUI and Avalonia Teams, and Contributors. All rights reserved.
// Licensed under the MIT license.
// See the LICENSE file in the project root for full license information.

using Avalonia;
using Avalonia.Headless;

namespace ReactiveUI.Avalonia.Tests;

/// <summary>
/// Provides a single, process-wide Avalonia headless session whose dispatcher runs on a
/// dedicated, continuously pumped UI thread.
/// </summary>
/// <remarks>
/// Tests that exercise <see cref="AvaloniaScheduler"/> (and any other code that posts work to
/// <c>Dispatcher.UIThread</c>) need a real, pumped UI thread to behave deterministically. Without
/// one, whether a test ran on the bound UI thread was incidental to how the test framework happened
/// to schedule it, which made dispatcher-dependent assertions intermittently fail on CI. Routing
/// every test body through <see cref="HeadlessUnitTestSession"/>'s <c>Dispatch</c> method guarantees
/// the body executes on the session's UI thread, so <c>Dispatcher.UIThread.CheckAccess()</c> is
/// always <see langword="true"/> and posted jobs are pumped.
/// <para>
/// The session is created lazily and never disposed: Avalonia can only initialise its platform once
/// per process, so a single shared instance is reused for the lifetime of the test run.
/// </para>
/// </remarks>
internal static class AvaloniaTestSession
{
    /// <summary>
    /// The lazily-created, process-wide headless session backing <see cref="Instance"/>.
    /// </summary>
    private static readonly Lazy<HeadlessUnitTestSession> _session =
        new(() => HeadlessUnitTestSession.StartNew(typeof(Application)), LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>
    /// Gets the shared headless session, creating it on first access.
    /// </summary>
    public static HeadlessUnitTestSession Instance => _session.Value;
}
