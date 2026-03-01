// Copyright (c) 2019-2026 ReactiveUI and Avalonia Teams, and Contributors. All rights reserved.
// Licensed under the MIT license.
// See the LICENSE file in the project root for full license information.

using Avalonia.Controls.ApplicationLifetimes;

namespace ReactiveUI.Avalonia.Tests;

/// <summary>
/// Tests for the AutoSuspendHelper initialization and event wiring.
/// </summary>
public class AutoSuspendHelperTests
{
    /// <summary>
    /// Verifies that the constructor throws on a null lifetime argument.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Ctor_With_Null_Lifetime_Throws()
    {
        await Assert.That(() => new AutoSuspendHelper(null!)).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that the constructor with a desktop lifetime sets ShouldPersistState.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Ctor_With_DesktopLifetime_Sets_ShouldPersistState()
    {
        var lifetime = new ClassicDesktopStyleApplicationLifetime();
        using var helper = new AutoSuspendHelper(lifetime);

        var notified = false;
        var sub = RxSuspension.SuspensionHost.ShouldPersistState.Subscribe(d =>
        {
            notified = true;
            d.Dispose();
        });

        lifetime.Shutdown(0);

        await Assert.That(notified).IsTrue();
        sub.Dispose();
    }

    /// <summary>
    /// Verifies that OnFrameworkInitializationCompleted pushes an IsLaunchingNew notification.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OnFrameworkInitializationCompleted_Pushes_IsLaunchingNew()
    {
        var lifetime = new ClassicDesktopStyleApplicationLifetime();
        using var helper = new AutoSuspendHelper(lifetime);

        var count = 0;
        var sub = RxSuspension.SuspensionHost.IsLaunchingNew.Subscribe(_ => count++);

        helper.OnFrameworkInitializationCompleted();

        await Assert.That(count).IsEqualTo(1);
        sub.Dispose();
    }
}
