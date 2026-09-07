// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Avalonia;
using DryIoc;
using ReactiveUI.Avalonia.Splat;
using ReactiveUI.Builder;
using Splat;
using Splat.DryIoc;

namespace ReactiveUI.Avalonia.DryIoc.Tests;

/// <summary>Additional tests for DryIoc-based Avalonia mixin registration.</summary>
public class AvaloniaMixinsDryIocMoreTests
{
    /// <summary>Verifies that the <c>UseReactiveUIWithDryIoc</c> builder overload returns the same builder instance.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test.</returns>
    [Test]
    public async Task UseReactiveUIWithDryIoc_WithBuilderOverload_Returns_Builder()
    {
        var builder = AppBuilder.Configure<Application>();
        var result = AvaloniaMixins.UseReactiveUIWithDryIoc(
            builder,
            containerConfig: static c => c.RegisterInstance(new object()),
            withReactiveUIBuilder: static _ => { });

        await Assert.That(result).IsSameReferenceAs(builder);
    }

    /// <summary>Verifies that the generic <see cref="AppBuilderExtensions.UseReactiveUIWithDIContainer{TContainer}"/> returns the same builder instance.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test.</returns>
    [Test]
    public async Task UseReactiveUIWithDIContainer_Generic_Returns_Builder()
    {
        var builder = AppBuilder.Configure<Application>();
        var result = AppBuilderExtensions.UseReactiveUIWithDIContainer(
            builder,
            containerFactory: static () => new Container(),
            containerConfig: static _ => { },
            dependencyResolverFactory: static c => new DryIocDependencyResolver(c),
            static _ => { });

        await Assert.That(result).IsSameReferenceAs(builder);
    }

    /// <summary>Verifies that null container configuration is validated by the deferred callback.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UseReactiveUIWithDryIoc_AfterPlatformCallback_ThrowsOnNullContainerConfig()
    {
        var builder = AppBuilder.Configure<Application>();
        _ = AvaloniaMixins.UseReactiveUIWithDryIoc(builder, null!, null);

        await Assert.That(() => builder.AfterPlatformServicesSetupCallback!(builder))
            .ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies that DryIoc registration executes through the deferred platform setup callback.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UseReactiveUIWithDryIoc_AfterPlatformCallback_ConfiguresContainer()
    {
        ReactiveUIBuilder.ResetBuilderStateForTests();
        var builder = AppBuilder.Configure<Application>();
        var configCalled = false;
        var reactiveBuilderCalled = false;
        var originalLocator = Locator.GetLocator();

        try
        {
            _ = AvaloniaMixins.UseReactiveUIWithDryIoc(
                builder,
                containerConfig: container => configCalled = container is not null,
                withReactiveUIBuilder: _ => reactiveBuilderCalled = true);

            builder.AfterPlatformServicesSetupCallback!(builder);

            await Assert.That(configCalled).IsTrue();
            await Assert.That(reactiveBuilderCalled).IsTrue();
        }
        finally
        {
            Locator.SetLocator(originalLocator);
            ReactiveUIBuilder.ResetBuilderStateForTests();
        }
    }

    /// <summary>Verifies that the deferred callback handles an already-built app and a null ReactiveUI callback.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UseReactiveUIWithDryIoc_AfterPlatformCallback_WhenAlreadyBuilt_AllowsNullCallback()
    {
        _ = AppLocator.CurrentMutable.CreateReactiveUIBuilder()
            .WithCoreServices()
            .BuildApp();
        var builder = AppBuilder.Configure<Application>();
        var configCalled = false;
        var originalLocator = Locator.GetLocator();

        try
        {
            _ = AvaloniaMixins.UseReactiveUIWithDryIoc(
                builder,
                containerConfig: container => configCalled = container is not null,
                withReactiveUIBuilder: null);

            builder.AfterPlatformServicesSetupCallback!(builder);

            await Assert.That(configCalled).IsTrue();
        }
        finally
        {
            Locator.SetLocator(originalLocator);
            ReactiveUIBuilder.ResetBuilderStateForTests();
        }
    }
}
