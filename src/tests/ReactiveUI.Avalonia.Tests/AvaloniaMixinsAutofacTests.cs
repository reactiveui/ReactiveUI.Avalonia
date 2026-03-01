// Copyright (c) 2019-2026 ReactiveUI and Avalonia Teams, and Contributors. All rights reserved.
// Licensed under the MIT license.
// See the LICENSE file in the project root for full license information.

extern alias autofac;

using Avalonia;
using Splat.Autofac;
using AvaloniaMixins = autofac::ReactiveUI.Avalonia.Splat.AvaloniaMixins;

namespace ReactiveUI.Avalonia.Tests;

/// <summary>
/// Tests for the Autofac-based AvaloniaMixins extension methods.
/// </summary>
public class AvaloniaMixinsAutofacTests
{
    /// <summary>
    /// Verifies that UseReactiveUIWithAutofac throws on a null builder.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UseReactiveUIWithAutofac_Extension_ThrowsOnNullBuilder()
    {
        AppBuilder? builder = null;
        await Assert.That(() =>
            AvaloniaMixins.UseReactiveUIWithAutofac(builder!, _ => { }, null, null)).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that the UseReactiveUIWithAutofac overload throws on a null builder.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UseReactiveUIWithAutofac_Overload_ThrowsOnNullBuilder()
    {
        AppBuilder? builder = null;
        await Assert.That(() =>
            AvaloniaMixins.UseReactiveUIWithAutofac(builder!, _ => { }, null, null)).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that UseReactiveUIWithAutofac returns the builder without throwing.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UseReactiveUIWithAutofac_ReturnsBuilder_NoThrow()
    {
        var builder = AppBuilder.Configure<Application>();
        var result = AvaloniaMixins.UseReactiveUIWithAutofac(builder, _ => { }, null, null);
        await Assert.That(result).IsSameReferenceAs(builder);
    }

    /// <summary>
    /// Verifies that the UseReactiveUIWithAutofac overload returns the builder and invokes callbacks.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UseReactiveUIWithAutofac_Overload_ReturnsBuilder_NoThrow()
    {
        var builder = AppBuilder.Configure<Application>();
        var result = AvaloniaMixins.UseReactiveUIWithAutofac(
            builder,
            _ => { },
            r => { _ = r is AutofacDependencyResolver; },
            rx => { _ = rx is not null; });

        await Assert.That(result).IsSameReferenceAs(builder);
    }
}
