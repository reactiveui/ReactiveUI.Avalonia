// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI.Avalonia.Splat;
using ReactiveUI.Builder;
using Splat;

namespace ReactiveUI.Avalonia.Microsoft.Tests;

/// <summary>Additional tests for Microsoft dependency injection-based Avalonia mixin registration.</summary>
public class AvaloniaMixinsMicrosoftMoreTests
{
    /// <summary>Verifies that <c>UseReactiveUIWithMicrosoftDependencyResolver</c> with the overload accepting container config returns the same builder instance.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test.</returns>
    [Test]
    public async Task UseReactiveUIWithMicrosoftDependencyResolver_Overload_Returns_Builder()
    {
        var builder = AppBuilder.Configure<Application>();
        var result = AvaloniaMixins.UseReactiveUIWithMicrosoftDependencyResolver(
            builder,
            containerConfig: static sc => _ = sc.AddSingleton(new object()),
            withResolver: static _ => { },
            withReactiveUIBuilder: static _ => { });

        await Assert.That(result).IsSameReferenceAs(builder);
    }

    /// <summary>Verifies that the simple Microsoft dependency resolver overload returns the same builder instance.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test.</returns>
    [Test]
    public async Task UseReactiveUIWithMicrosoftDependencyResolver_SimpleOverload_Returns_Builder()
    {
        var builder = AppBuilder.Configure<Application>();
        var result = AvaloniaMixins.UseReactiveUIWithMicrosoftDependencyResolver(builder, static _ => { });

        await Assert.That(result).IsSameReferenceAs(builder);
    }

    /// <summary>Verifies that <c>UseReactiveUIWithMicrosoftDependencyResolver</c> returns the same builder instance without throwing.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test.</returns>
    [Test]
    public async Task UseReactiveUIWithMicrosoftDependencyResolver_Returns_Builder_NoThrow()
    {
        var builder = AppBuilder.Configure<Application>();
        var result = AvaloniaMixins.UseReactiveUIWithMicrosoftDependencyResolver(
            builder,
            static sc => _ = sc.AddSingleton(new object()),
            (Action<IServiceProvider?>)(static _ => { }));

        await Assert.That(result).IsSameReferenceAs(builder);
    }

    /// <summary>Verifies that null container configuration is validated by the deferred callback.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UseReactiveUIWithMicrosoftDependencyResolver_AfterPlatformCallback_ThrowsOnNullContainerConfig()
    {
        var builder = AppBuilder.Configure<Application>();
        _ = AvaloniaMixins.UseReactiveUIWithMicrosoftDependencyResolver(
            builder,
            null!,
            (Action<IServiceProvider?>?)null,
            null);

        await Assert.That(() => builder.AfterPlatformServicesSetupCallback!(builder))
            .ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies that Microsoft dependency resolver registration executes through the deferred platform setup callback.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UseReactiveUIWithMicrosoftDependencyResolver_AfterPlatformCallback_ConfiguresServicesAndResolver()
    {
        ReactiveUIBuilder.ResetBuilderStateForTests();
        var builder = AppBuilder.Configure<Application>();
        var configCalled = false;
        var resolverCalled = false;
        var reactiveBuilderCalled = false;
        var originalLocator = Locator.GetLocator();

        try
        {
            _ = AvaloniaMixins.UseReactiveUIWithMicrosoftDependencyResolver(
                builder,
                containerConfig: services =>
                {
                    configCalled = services is not null;
                    _ = services!.AddSingleton("configured");
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
            Locator.SetLocator(originalLocator);
            ReactiveUIBuilder.ResetBuilderStateForTests();
        }
    }

    /// <summary>Verifies that the deferred callback handles an already-built app and null optional callbacks.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UseReactiveUIWithMicrosoftDependencyResolver_AfterPlatformCallback_WhenAlreadyBuilt_AllowsNullCallbacks()
    {
        _ = AppLocator.CurrentMutable.CreateReactiveUIBuilder()
            .WithCoreServices()
            .BuildApp();
        var builder = AppBuilder.Configure<Application>();
        var configCalled = false;
        var originalLocator = Locator.GetLocator();

        try
        {
            _ = AvaloniaMixins.UseReactiveUIWithMicrosoftDependencyResolver(
                builder,
                containerConfig: services =>
                {
                    configCalled = services is not null;
                    _ = services!.AddSingleton("configured");
                },
                withResolver: null,
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
