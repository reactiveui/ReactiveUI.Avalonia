// Copyright (c) 2019-2026 ReactiveUI and Avalonia Teams, and Contributors. All rights reserved.
// Licensed under the MIT license.
// See the LICENSE file in the project root for full license information.

using Avalonia;
using DryIoc;
using ReactiveUI.Avalonia.Splat;
using Splat.DryIoc;

namespace ReactiveUI.Avalonia.DryIoc.Tests;

/// <summary>
/// Additional tests for DryIoc-based Avalonia mixin registration.
/// </summary>
public class AvaloniaMixinsDryIocMoreTests
{
    /// <summary>
    /// Verifies that <see cref="AvaloniaMixins.UseReactiveUIWithDryIoc"/> with the builder overload
    /// returns the same builder instance.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test.</returns>
    [Test]
    public async Task UseReactiveUIWithDryIoc_WithBuilderOverload_Returns_Builder()
    {
        var builder = AppBuilder.Configure<Application>();
        var result = AvaloniaMixins.UseReactiveUIWithDryIoc(
            builder,
            containerConfig: c =>
            {
                c.RegisterInstance(new object());
            },
            withReactiveUIBuilder: _ => { });

        await Assert.That(result).IsSameReferenceAs(builder);
    }

    /// <summary>
    /// Verifies that the generic <see cref="AppBuilderExtensions.UseReactiveUIWithDIContainer{TContainer}"/>
    /// returns the same builder instance.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test.</returns>
    [Test]
    public async Task UseReactiveUIWithDIContainer_Generic_Returns_Builder()
    {
        var builder = AppBuilder.Configure<Application>();
        var result = AppBuilderExtensions.UseReactiveUIWithDIContainer(
            builder,
            containerFactory: () => new Container(),
            containerConfig: _ => { },
            dependencyResolverFactory: c => new DryIocDependencyResolver(c),
            _ => { });

        await Assert.That(result).IsSameReferenceAs(builder);
    }
}
