// Copyright (c) 2019-2026 ReactiveUI and Avalonia Teams, and Contributors. All rights reserved.
// Licensed under the MIT license.
// See the LICENSE file in the project root for full license information.

using Avalonia;
using DryIoc;
using ReactiveUI.Avalonia.Splat;
using Splat.DryIoc;

namespace ReactiveUI.Avalonia.DryIoc.Tests;

/// <summary>
/// Tests for DryIoc-based Avalonia mixin registration and resolution.
/// </summary>
public class AvaloniaMixinsDryIocTests
{
    /// <summary>
    /// Verifies that <see cref="AvaloniaMixins.UseReactiveUIWithDryIoc"/> throws
    /// <see cref="ArgumentNullException"/> when the builder is null.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test.</returns>
    [Test]
    public async Task UseReactiveUIWithDryIoc_ThrowsOnNullBuilder()
    {
        AppBuilder? builder = null;
        await Assert.That(() =>
            AvaloniaMixins.UseReactiveUIWithDryIoc(builder!, _ => { })).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that <see cref="AvaloniaMixins.UseReactiveUIWithDryIoc"/> returns the same builder instance.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test.</returns>
    [Test]
    public async Task UseReactiveUIWithDryIoc_ReturnsBuilder_NoThrow()
    {
        var builder = AppBuilder.Configure<Application>();
        var result = builder.UseReactiveUIWithDryIoc(_ => { });
        await Assert.That(result).IsSameReferenceAs(builder);
    }

    /// <summary>
    /// Verifies that <see cref="AppBuilderExtensions.UseReactiveUIWithDIContainer{TContainer}"/> throws
    /// <see cref="ArgumentNullException"/> when the builder is null.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test.</returns>
    [Test]
    public async Task UseReactiveUIWithDIContainer_ThrowsOnNullBuilder()
    {
        AppBuilder? builder = null;
        await Assert.That(() =>
            AppBuilderExtensions.UseReactiveUIWithDIContainer(
                builder!,
                () => new Container(),
                _ => { },
                c => new DryIocDependencyResolver(c),
                _ => { })).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that <see cref="AppBuilderExtensions.UseReactiveUIWithDIContainer{TContainer}"/> returns
    /// the same builder instance with valid arguments.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test.</returns>
    [Test]
    public async Task UseReactiveUIWithDIContainer_ReturnsBuilder_NoThrow()
    {
        var builder = AppBuilder.Configure<Application>();
        var container = new Container();

        var result = builder.UseReactiveUIWithDIContainer(
            containerFactory: () => container,
            containerConfig: _ => { },
            dependencyResolverFactory: c => new DryIocDependencyResolver(c),
            _ => { });

        await Assert.That(result).IsSameReferenceAs(builder);
    }

    /// <summary>
    /// Verifies that <see cref="DryIocDependencyResolver"/> can register and resolve services
    /// both with and without contracts.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test.</returns>
    [Test]
    public async Task DryIocDependencyResolver_Register_And_Resolve_WithAndWithoutContract()
    {
        var container = new Container();
        var resolver = new DryIocDependencyResolver(container);

        resolver.Register<string>(() => "a");
        resolver.Register<string>(() => "b");
        resolver.Register<string>(() => "c", "x");

        var noContract = resolver.GetService(typeof(string));
        await Assert.That(noContract).IsEqualTo("b");

        var withContract = resolver.GetService(typeof(string), "x");
        await Assert.That(withContract).IsEqualTo("c");

        var all = resolver.GetServices(typeof(string)).ToArray();
        await Assert.That(all).Contains("a");
        await Assert.That(all).Contains("b");
    }
}
