// Copyright (c) 2019-2026 ReactiveUI and Avalonia Teams, and Contributors. All rights reserved.
// Licensed under the MIT license.
// See the LICENSE file in the project root for full license information.

extern alias ninject;

using Avalonia;
using AvaloniaMixins = ninject::ReactiveUI.Avalonia.Splat.AvaloniaMixins;

namespace ReactiveUI.Avalonia.Tests;

/// <summary>
/// Tests for the Ninject-based AvaloniaMixins extension methods.
/// </summary>
public class AvaloniaMixinsNinjectTests
{
    /// <summary>
    /// Verifies that UseReactiveUIWithNinject throws on a null builder.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UseReactiveUIWithNinject_Extension_ThrowsOnNullBuilder()
    {
        AppBuilder? builder = null;
        await Assert.That(() =>
            AvaloniaMixins.UseReactiveUIWithNinject(builder!, _ => { }, null)).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that the UseReactiveUIWithNinject overload throws on a null builder.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UseReactiveUIWithNinject_Overload_ThrowsOnNullBuilder()
    {
        AppBuilder? builder = null;
        await Assert.That(() =>
            AvaloniaMixins.UseReactiveUIWithNinject(builder!, _ => { }, null)).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that UseReactiveUIWithNinject returns the builder without throwing.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UseReactiveUIWithNinject_ReturnsBuilder_NoThrow()
    {
        var builder = AppBuilder.Configure<Application>();
        var result = AvaloniaMixins.UseReactiveUIWithNinject(builder, _ => { }, null);
        await Assert.That(result).IsSameReferenceAs(builder);
    }

    /// <summary>
    /// Verifies that the UseReactiveUIWithNinject overload returns the builder and invokes callbacks.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UseReactiveUIWithNinject_Overload_ReturnsBuilder_NoThrow()
    {
        var builder = AppBuilder.Configure<Application>();
        var result = AvaloniaMixins.UseReactiveUIWithNinject(
            builder,
            _ => { },
            rx => { _ = rx is not null; });
        await Assert.That(result).IsSameReferenceAs(builder);
    }
}
