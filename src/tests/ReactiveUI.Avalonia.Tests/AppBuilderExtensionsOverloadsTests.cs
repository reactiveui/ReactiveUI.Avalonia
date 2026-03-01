// Copyright (c) 2019-2026 ReactiveUI and Avalonia Teams, and Contributors. All rights reserved.
// Licensed under the MIT license.
// See the LICENSE file in the project root for full license information.

using Avalonia;

namespace ReactiveUI.Avalonia.Tests;

/// <summary>
/// Tests for AppBuilder extension method overloads and null-guard behavior.
/// </summary>
public class AppBuilderExtensionsOverloadsTests
{
    /// <summary>
    /// Verifies that UseReactiveUI with builder overload throws on null arguments.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UseReactiveUI_WithBuilderOverload_ThrowsOnNulls()
    {
        AppBuilder? builder = null;
        await Assert.That(() => builder!.UseReactiveUI(_ => { })).ThrowsExactly<ArgumentNullException>();

        var b = AppBuilder.Configure<Application>();
        await Assert.That(() => b.UseReactiveUI(null!)).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that RegisterReactiveUIViews throws on a null builder.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RegisterReactiveUIViews_ThrowsOnNullBuilder()
    {
        AppBuilder? builder = null;
        await Assert.That(() => builder!.RegisterReactiveUIViews()).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that RegisterReactiveUIViews returns the builder without throwing when no assemblies are provided.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RegisterReactiveUIViews_ReturnsBuilder_NoAssemblies_NoThrow()
    {
        var builder = AppBuilder.Configure<Application>();
        var result = builder.RegisterReactiveUIViews();
        await Assert.That(result).IsSameReferenceAs(builder);
    }
}
