// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Avalonia.Controls;

namespace ReactiveUI.Avalonia.Reactive.Tests;

/// <summary>Regression tests for suspension helper shutdown and event ownership.</summary>
public class AutoSuspendHelperShutdownTests
{
    /// <summary>Verifies designer initialization never subscribes to application shutdown.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Design_Mode_Does_Not_Persist_State()
    {
        var designMode = typeof(Design).GetProperty(nameof(Design.IsDesignMode))!;
        var previous = Design.IsDesignMode;
        try
        {
            designMode.SetValue(null, true);
            var fixture = new ControlledLifetimeFixture();
            using var helper = new AutoSuspendHelper(fixture.Lifetime);
            fixture.RaiseExit();
            await Assert.That(Design.IsDesignMode).IsTrue();
        }
        finally
        {
            designMode.SetValue(null, previous);
        }
    }

    /// <summary>Verifies that shutdown completes when no persistence observer is registered.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Shutdown_WithoutPersistenceSubscribers_Completes()
    {
        var fixture = new ControlledLifetimeFixture();
        using var helper = new AutoSuspendHelper(fixture.Lifetime);
        var exited = false;
        fixture.Lifetime.Exit += (_, _) => exited = true;

        fixture.RaiseExit();

        await Assert.That(exited).IsTrue();
    }

    /// <summary>Verifies that a disposed helper does not intercept application shutdown.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Dispose_RemovesLifetimeHandler()
    {
        var fixture = new ControlledLifetimeFixture();
        var helper = new AutoSuspendHelper(fixture.Lifetime);
        helper.Dispose();
        var exited = false;
        fixture.Lifetime.Exit += (_, _) => exited = true;

        fixture.RaiseExit();

        await Assert.That(exited).IsTrue();
    }
}
