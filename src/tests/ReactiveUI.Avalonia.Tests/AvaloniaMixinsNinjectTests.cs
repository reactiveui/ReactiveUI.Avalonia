// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
extern alias ninject;

using Avalonia;
using ReactiveUI.Builder;
using TUnit.Core.Executors;
using AvaloniaMixins = ninject::ReactiveUI.Avalonia.Splat.AvaloniaMixins;

namespace ReactiveUI.Avalonia.Tests;

/// <summary>Tests for the Ninject-based AvaloniaMixins extension methods.</summary>
public class AvaloniaMixinsNinjectTests
{
    /// <summary>Verifies that UseReactiveUIWithNinject throws on a null builder.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UseReactiveUIWithNinject_Extension_ThrowsOnNullBuilder()
    {
        AppBuilder? builder = null;
        await Assert.That(() =>
            AvaloniaMixins.UseReactiveUIWithNinject(builder!, static _ => { }, null)).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies that the UseReactiveUIWithNinject overload throws on a null builder.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UseReactiveUIWithNinject_Overload_ThrowsOnNullBuilder()
    {
        AppBuilder? builder = null;
        await Assert.That(() =>
            AvaloniaMixins.UseReactiveUIWithNinject(builder!, static _ => { })).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies that UseReactiveUIWithNinject returns the builder without throwing.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UseReactiveUIWithNinject_ReturnsBuilder_NoThrow()
    {
        var builder = AppBuilder.Configure<Application>();
        var result = AvaloniaMixins.UseReactiveUIWithNinject(builder, static _ => { }, null);
        await Assert.That(result).IsSameReferenceAs(builder);
    }

    /// <summary>Verifies that the UseReactiveUIWithNinject overload returns the builder and invokes callbacks.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UseReactiveUIWithNinject_Overload_ReturnsBuilder_NoThrow()
    {
        var builder = AppBuilder.Configure<Application>();
        var result = AvaloniaMixins.UseReactiveUIWithNinject(
            builder,
            static _ => { },
            static rx => _ = rx is not null);
        await Assert.That(result).IsSameReferenceAs(builder);
    }

    /// <summary>Verifies that null container configuration is validated by the deferred callback.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [TestExecutor<AutofacIsolatedTestExecutor>]
    public async Task UseReactiveUIWithNinject_AfterPlatformCallback_ThrowsOnNullContainerConfig()
    {
        var builder = AppBuilder.Configure<Application>();
        _ = AvaloniaMixins.UseReactiveUIWithNinject(builder, null!, null);

        await Assert.That(() => builder.AfterPlatformServicesSetupCallback!(builder))
            .ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies that Ninject registration executes through the deferred platform setup callback.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [TestExecutor<AutofacIsolatedTestExecutor>]
    public async Task UseReactiveUIWithNinject_AfterPlatformCallback_ConfiguresContainer()
    {
        ReactiveUIBuilder.ResetBuilderStateForTests();
        var builder = AppBuilder.Configure<Application>();
        var configCalled = false;
        var reactiveBuilderCalled = false;

        try
        {
            _ = AvaloniaMixins.UseReactiveUIWithNinject(
                builder,
                containerConfig: kernel => configCalled = kernel is not null,
                withReactiveUIBuilder: _ => reactiveBuilderCalled = true);

            builder.AfterPlatformServicesSetupCallback!(builder);

            await Assert.That(configCalled).IsTrue();
            await Assert.That(reactiveBuilderCalled).IsTrue();
        }
        finally
        {
            ReactiveUIBuilder.ResetBuilderStateForTests();
        }
    }

    /// <summary>Verifies that the deferred callback handles an already-built app and a null ReactiveUI callback.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [TestExecutor<AutofacIsolatedTestExecutor>]
    public async Task UseReactiveUIWithNinject_AfterPlatformCallback_WhenAlreadyBuilt_AllowsNullCallback()
    {
        var builder = AppBuilder.Configure<Application>();
        var configCalled = false;

        _ = AvaloniaMixins.UseReactiveUIWithNinject(
            builder,
            containerConfig: kernel => configCalled = kernel is not null,
            withReactiveUIBuilder: null);

        builder.AfterPlatformServicesSetupCallback!(builder);

        await Assert.That(configCalled).IsTrue();
    }
}
