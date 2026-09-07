// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
extern alias autofac;

using Autofac;
using Avalonia;
using ReactiveUI.Builder;
using Splat;
using Splat.Autofac;
using TUnit.Core.Executors;
using AutofacSplatModule = Splat.Builder.AutofacSplatModule;
using AvaloniaMixins = autofac::ReactiveUI.Avalonia.Splat.AvaloniaMixins;
using SplatAppBuilder = Splat.Builder.AppBuilder;

namespace ReactiveUI.Avalonia.Tests;

/// <summary>Tests for the Autofac-based AvaloniaMixins extension methods.</summary>
public class AvaloniaMixinsAutofacTests
{
    /// <summary>Verifies that UseReactiveUIWithAutofac throws on a null builder.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UseReactiveUIWithAutofac_Extension_ThrowsOnNullBuilder()
    {
        AppBuilder? builder = null;
        await Assert.That(() =>
            AvaloniaMixins.UseReactiveUIWithAutofac(builder!, static _ => { }, null, null)).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies that the UseReactiveUIWithAutofac overload throws on a null builder.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UseReactiveUIWithAutofac_Overload_ThrowsOnNullBuilder()
    {
        AppBuilder? builder = null;
        await Assert.That(() =>
            AvaloniaMixins.UseReactiveUIWithAutofac(builder!, static _ => { }, static _ => { })).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies that UseReactiveUIWithAutofac returns the builder without throwing.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UseReactiveUIWithAutofac_ReturnsBuilder_NoThrow()
    {
        var builder = AppBuilder.Configure<Application>();
        var result = AvaloniaMixins.UseReactiveUIWithAutofac(builder, static _ => { }, null, null);
        await Assert.That(result).IsSameReferenceAs(builder);
    }

    /// <summary>Verifies that the UseReactiveUIWithAutofac overload returns the builder and invokes callbacks.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UseReactiveUIWithAutofac_Overload_ReturnsBuilder_NoThrow()
    {
        var builder = AppBuilder.Configure<Application>();
        var result = AvaloniaMixins.UseReactiveUIWithAutofac(
            builder,
            static _ => { },
            static r => _ = r is AutofacDependencyResolver,
            static rx => _ = rx is not null);

        await Assert.That(result).IsSameReferenceAs(builder);
    }

    /// <summary>Verifies that the simple overload defers to the full overload.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UseReactiveUIWithAutofac_SimpleOverload_ReturnsBuilder()
    {
        var builder = AppBuilder.Configure<Application>();
        var result = AvaloniaMixins.UseReactiveUIWithAutofac(builder, static _ => { });

        await Assert.That(result).IsSameReferenceAs(builder);
    }

    /// <summary>Verifies that null container configuration is validated immediately.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UseReactiveUIWithAutofac_ThrowsOnNullContainerConfig()
    {
        var builder = AppBuilder.Configure<Application>();

        await Assert.That(() => AvaloniaMixins.UseReactiveUIWithAutofac(builder, null!, null, null))
            .ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies that Autofac registration executes through the deferred platform setup callback.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [TestExecutor<AutofacIsolatedTestExecutor>]
    public async Task UseReactiveUIWithAutofac_AfterPlatformCallback_ConfiguresContainerAndResolver()
    {
        ReactiveUIBuilder.ResetBuilderStateForTests();
        var builder = AppBuilder.Configure<Application>();
        var configCalled = false;
        var resolverCalled = false;
        var reactiveBuilderCalled = false;

        try
        {
            _ = AvaloniaMixins.UseReactiveUIWithAutofac(
                builder,
                containerConfig: containerBuilder =>
                {
                    configCalled = true;
                    _ = containerBuilder.RegisterInstance("configured");
                },
                withResolver: resolver => resolverCalled = resolver is not null,
                withReactiveUIBuilder: _ => reactiveBuilderCalled = true);

            builder.AfterPlatformServicesSetupCallback!(builder);

            await Assert.That(configCalled).IsTrue();
            await Assert.That(resolverCalled).IsTrue();
            await Assert.That(reactiveBuilderCalled).IsTrue();
        }
        finally
        {
            ReactiveUIBuilder.ResetBuilderStateForTests();
        }
    }

    /// <summary>Verifies that the deferred callback handles an already-built app and null optional callbacks.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [TestExecutor<AutofacIsolatedTestExecutor>]
    public async Task UseReactiveUIWithAutofac_AfterPlatformCallback_WhenAlreadyBuilt_AllowsNullCallbacks()
    {
        var builder = AppBuilder.Configure<Application>();
        var configCalled = false;

        _ = AvaloniaMixins.UseReactiveUIWithAutofac(
            builder,
            containerConfig: _ => configCalled = true,
            withResolver: null,
            withReactiveUIBuilder: null);

        builder.AfterPlatformServicesSetupCallback!(builder);

        await Assert.That(configCalled).IsTrue();
    }

    /// <summary>
    /// Verifies that AutofacSplatModule.Configure registers AutofacDependencyResolver in the
    /// ContainerBuilder so it can be resolved after Build(). This is the fix for issue #64 where
    /// UsingSplatModule deferred Configure() until after the Autofac container was already built.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [TestExecutor<AutofacIsolatedTestExecutor>]
    public async Task AutofacSplatModule_Configure_Registers_Resolver_Before_ContainerBuild()
    {
        var containerBuilder = new ContainerBuilder();
        var module = new AutofacSplatModule(containerBuilder);

        // Eagerly configure the module (what the fix does instead of deferring via UsingSplatModule)
        module.Configure(AppLocator.CurrentMutable);

        // Build the container - this must succeed and the resolver must be resolvable
        var container = containerBuilder.Build();
        var resolver = container.Resolve<AutofacDependencyResolver>();

        await Assert.That(resolver).IsNotNull();
    }

    /// <summary>
    /// Verifies that deferring AutofacSplatModule.Configure via UsingModule (as UsingSplatModule does)
    /// causes the Autofac container to fail resolution because Configure has not yet been invoked.
    /// This reproduces the root cause of issue #64.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [TestExecutor<AutofacIsolatedTestExecutor>]
    public async Task AutofacSplatModule_Deferred_Via_UsingModule_Fails_To_Resolve()
    {
        var containerBuilder = new ContainerBuilder();
        var module = new AutofacSplatModule(containerBuilder);

        // Simulate what UsingSplatModule does: enqueue Configure but don't invoke it
        var splatBuilder = new SplatAppBuilder(AppLocator.CurrentMutable, AppLocator.Current);
        _ = splatBuilder.UsingModule(module);

        // Build the Autofac container without having invoked Configure
        var container = containerBuilder.Build();

        // This should fail because AutofacDependencyResolver was never registered
        await Assert.That(() => container.Resolve<AutofacDependencyResolver>())
            .ThrowsExactly<Autofac.Core.Registration.ComponentNotRegisteredException>();
    }
}
