// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Avalonia;
using DryIoc;
using ReactiveUI.Avalonia.Splat;
using Splat.DryIoc;

namespace ReactiveUI.Avalonia.DryIoc.Tests;

/// <summary>Tests for DryIoc-based Avalonia mixin registration and resolution.</summary>
public class AvaloniaMixinsDryIocTests
{
    /// <summary>Verifies that <c>UseReactiveUIWithDryIoc</c> throws <see cref="ArgumentNullException"/> when the builder is null.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test.</returns>
    [Test]
    public async Task UseReactiveUIWithDryIoc_ThrowsOnNullBuilder()
    {
        AppBuilder? builder = null;
        await Assert.That(() =>
            AvaloniaMixins.UseReactiveUIWithDryIoc(builder!, static _ => { })).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies that <c>UseReactiveUIWithDryIoc</c> returns the same builder instance.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test.</returns>
    [Test]
    public async Task UseReactiveUIWithDryIoc_ReturnsBuilder_NoThrow()
    {
        var builder = AppBuilder.Configure<Application>();
        var result = builder.UseReactiveUIWithDryIoc(static _ => { });
        await Assert.That(result).IsSameReferenceAs(builder);
    }

    /// <summary>Verifies that <see cref="AppBuilderExtensions.UseReactiveUIWithDIContainer{TContainer}"/> throws <see cref="ArgumentNullException"/> when the builder is null.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test.</returns>
    [Test]
    public async Task UseReactiveUIWithDIContainer_ThrowsOnNullBuilder()
    {
        AppBuilder? builder = null;
        await Assert.That(() =>
            AppBuilderExtensions.UseReactiveUIWithDIContainer(
                builder!,
                static () => new Container(),
                static _ => { },
                static c => new DryIocDependencyResolver(c),
                static _ => { })).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies that <see cref="AppBuilderExtensions.UseReactiveUIWithDIContainer{TContainer}"/> returns the same builder instance with valid arguments.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test.</returns>
    [Test]
    public async Task UseReactiveUIWithDIContainer_ReturnsBuilder_NoThrow()
    {
        var builder = AppBuilder.Configure<Application>();
        using var container = new Container();

        var result = builder.UseReactiveUIWithDIContainer(
            containerFactory: () => container,
            containerConfig: static _ => { },
            dependencyResolverFactory: static c => new DryIocDependencyResolver(c),
            static _ => { });

        await Assert.That(result).IsSameReferenceAs(builder);
    }

    /// <summary>Verifies that <see cref="DryIocDependencyResolver"/> can register and resolve services both with and without contracts.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test.</returns>
    [Test]
    public async Task DryIocDependencyResolver_Register_And_Resolve_WithAndWithoutContract()
    {
        var container = new Container();
        using var resolver = new DryIocDependencyResolver(container);

        resolver.Register(static () => "a");
        resolver.Register(static () => "b");
        resolver.Register(static () => "c", "x");

        var noContract = resolver.GetService<string>();
        await Assert.That(noContract).IsEqualTo("b");

        var withContract = resolver.GetService<string>("x");
        await Assert.That(withContract).IsEqualTo("c");

        var hasA = false;
        var hasB = false;
        foreach (var service in resolver.GetServices<string>())
        {
            hasA |= service == "a";
            hasB |= service == "b";
        }

        await Assert.That(hasA).IsTrue();
        await Assert.That(hasB).IsTrue();
    }
}
