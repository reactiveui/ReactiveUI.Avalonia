// Copyright (c) 2019-2026 ReactiveUI and Avalonia Teams, and Contributors. All rights reserved.
// Licensed under the MIT license.
// See the LICENSE file in the project root for full license information.

using Avalonia;

namespace ReactiveUI.Avalonia.Tests;

/// <summary>
/// Tests for core AppBuilderExtensions behavior.
/// </summary>
public class AppBuilderExtensionsTests
{
    /// <summary>
    /// Verifies that UseReactiveUI throws ArgumentNullException for a null builder.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UseReactiveUI_ThrowsOnNullBuilder()
    {
        AppBuilder? nullBuilder = null;
        await Assert.That(() => nullBuilder!.UseReactiveUI(_ => { })).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that AppBuilderExtensions is a static class.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AppBuilderExtensions_IsStaticClass()
    {
        var type = typeof(AppBuilderExtensions);
        await Assert.That(type.IsClass).IsTrue();
        await Assert.That(type.IsAbstract).IsTrue();
        await Assert.That(type.IsSealed).IsTrue();
    }
}
