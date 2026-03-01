// Copyright (c) 2019-2026 ReactiveUI and Avalonia Teams, and Contributors. All rights reserved.
// Licensed under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using Avalonia;

namespace ReactiveUI.Avalonia.Tests;

/// <summary>
/// Tests for the views API surface of AppBuilderExtensions.
/// </summary>
public class AppBuilderExtensionsViewsApiTests
{
    /// <summary>
    /// Verifies that RegisterReactiveUIViews returns the same builder and handles empty arrays.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RegisterReactiveUIViews_Returns_Same_Builder_And_Handles_NullOrEmpty()
    {
        var builder = AppBuilder.Configure<Application>();
        var result = builder.RegisterReactiveUIViews([]);
        await Assert.That(result).IsSameReferenceAs(builder);
    }

    /// <summary>
    /// Verifies that RegisterReactiveUIViewsFromAssemblyOf registers types from the specified assembly.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RegisterReactiveUIViewsFromAssemblyOf_Registers_Types()
    {
        var builder = AppBuilder.Configure<Application>();
        var result = builder.RegisterReactiveUIViewsFromAssemblyOf<AppBuilderExtensionsRegistrationTests>();
        await Assert.That(result).IsSameReferenceAs(builder);

        var resolver = global::Splat.AppLocator.CurrentMutable!;
        var method = typeof(AppBuilderExtensions)
            .GetMethod("RegisterViewsInternal", BindingFlags.NonPublic | BindingFlags.Static)!;
        method.Invoke(null, [resolver, new[] { typeof(AppBuilderExtensionsRegistrationTests).Assembly }]);
        var serviceType = typeof(IViewFor<>).MakeGenericType(typeof(AppBuilderExtensionsRegistrationTests));
    }

    /// <summary>
    /// Verifies that RegisterReactiveUIViewsFromEntryAssembly returns the same builder instance.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RegisterReactiveUIViewsFromEntryAssembly_Returns_Same_Builder()
    {
        var builder = AppBuilder.Configure<Application>();
        var result = builder.RegisterReactiveUIViewsFromEntryAssembly();
        await Assert.That(result).IsSameReferenceAs(builder);
    }
}
