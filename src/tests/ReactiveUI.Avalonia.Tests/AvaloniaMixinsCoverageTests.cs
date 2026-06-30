// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
extern alias autofac;
extern alias dryioc;
extern alias ninject;

using System.Reflection;
using Avalonia;
using Splat;
using AutofacMixins = autofac::ReactiveUI.Avalonia.Splat.AvaloniaMixins;
using DryIocMixins = dryioc::ReactiveUI.Avalonia.Splat.AvaloniaMixins;
using NinjectMixins = ninject::ReactiveUI.Avalonia.Splat.AvaloniaMixins;

namespace ReactiveUI.Avalonia.Tests;

/// <summary>Coverage tests for dependency-injection mixin setup callbacks.</summary>
public class AvaloniaMixinsCoverageTests
{
    /// <summary>Covers Autofac setup callback execution.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AutofacMixins_InvokePlatformSetup_CoversContainerWiring()
    {
        await RunWithLocatorRestore(async () =>
        {
            AppBuilder? builder = null;
            await Assert.That(() =>
                AutofacMixins.UseReactiveUIWithAutofac(builder!, _ => { }, null, null)).ThrowsExactly<ArgumentNullException>();

            builder = AppBuilder.Configure<Application>();
            await Assert.That(() =>
                AutofacMixins.UseReactiveUIWithAutofac(builder, null!, null, null)).ThrowsExactly<ArgumentNullException>();

            ResetNormalBuilderState();

            var containerConfigured = false;
            var resolverConfigured = false;
            var reactiveBuilderConfigured = false;
            builder = AppBuilder.Configure<Application>();
            var result = AutofacMixins.UseReactiveUIWithAutofac(
                builder,
                _ => containerConfigured = true,
                resolver => resolverConfigured = resolver is not null,
                rxuiBuilder => reactiveBuilderConfigured = rxuiBuilder is not null);

            InvokeAfterPlatformServicesSetup(builder);

            await Assert.That(result).IsSameReferenceAs(builder);
            await Assert.That(containerConfigured).IsTrue();
            await Assert.That(resolverConfigured).IsTrue();
            await Assert.That(reactiveBuilderConfigured).IsTrue();

            builder = AppBuilder.Configure<Application>();
            var alreadyBuiltContainerConfigured = false;
            _ = AutofacMixins.UseReactiveUIWithAutofac(builder, _ => alreadyBuiltContainerConfigured = true, null, null);
            InvokeAfterPlatformServicesSetup(builder);
            await Assert.That(alreadyBuiltContainerConfigured).IsTrue();
        });
    }

    /// <summary>Covers DryIoc setup callback execution.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DryIocMixins_InvokePlatformSetup_CoversContainerWiring()
    {
        await RunWithLocatorRestore(async () =>
        {
            AppBuilder? builder = null;
            await Assert.That(() =>
                DryIocMixins.UseReactiveUIWithDryIoc(builder!, _ => { }, null)).ThrowsExactly<ArgumentNullException>();

            builder = AppBuilder.Configure<Application>();
            _ = DryIocMixins.UseReactiveUIWithDryIoc(builder, null!, null);
            await Assert.That(() => InvokeAfterPlatformServicesSetup(builder)).ThrowsExactly<ArgumentNullException>();

            ResetNormalBuilderState();

            var containerConfigured = false;
            var reactiveBuilderConfigured = false;
            builder = AppBuilder.Configure<Application>();
            var result = DryIocMixins.UseReactiveUIWithDryIoc(
                builder,
                _ => containerConfigured = true,
                rxuiBuilder => reactiveBuilderConfigured = rxuiBuilder is not null);

            InvokeAfterPlatformServicesSetup(builder);

            await Assert.That(result).IsSameReferenceAs(builder);
            await Assert.That(containerConfigured).IsTrue();
            await Assert.That(reactiveBuilderConfigured).IsTrue();

            builder = AppBuilder.Configure<Application>();
            var alreadyBuiltContainerConfigured = false;
            _ = DryIocMixins.UseReactiveUIWithDryIoc(builder, _ => alreadyBuiltContainerConfigured = true, null);
            InvokeAfterPlatformServicesSetup(builder);
            await Assert.That(alreadyBuiltContainerConfigured).IsTrue();
        });
    }

    /// <summary>Covers Microsoft dependency-injection setup callback execution.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task MicrosoftMixins_InvokePlatformSetup_CoversContainerWiring()
    {
        await RunWithLocatorRestore(async () =>
        {
            AppBuilder? builder = null;
            await Assert.That(() =>
                ReactiveUI.Avalonia.Splat.AvaloniaMixins.UseReactiveUIWithMicrosoftDependencyResolver(
                    builder!,
                    _ => { },
                    null,
                    null)).ThrowsExactly<ArgumentNullException>();

            builder = AppBuilder.Configure<Application>();
            _ = ReactiveUI.Avalonia.Splat.AvaloniaMixins.UseReactiveUIWithMicrosoftDependencyResolver(builder, null!, null, null);
            await Assert.That(() => InvokeAfterPlatformServicesSetup(builder)).ThrowsExactly<ArgumentNullException>();

            ResetNormalBuilderState();

            var containerConfigured = false;
            var resolverConfigured = false;
            var reactiveBuilderConfigured = false;
            builder = AppBuilder.Configure<Application>();
            var result = ReactiveUI.Avalonia.Splat.AvaloniaMixins.UseReactiveUIWithMicrosoftDependencyResolver(
                builder,
                _ => containerConfigured = true,
                provider => resolverConfigured = provider is not null,
                rxuiBuilder => reactiveBuilderConfigured = rxuiBuilder is not null);

            InvokeAfterPlatformServicesSetup(builder);

            await Assert.That(result).IsSameReferenceAs(builder);
            await Assert.That(containerConfigured).IsTrue();
            await Assert.That(resolverConfigured).IsTrue();
            await Assert.That(reactiveBuilderConfigured).IsTrue();

            builder = AppBuilder.Configure<Application>();
            var alreadyBuiltContainerConfigured = false;
            _ = ReactiveUI.Avalonia.Splat.AvaloniaMixins.UseReactiveUIWithMicrosoftDependencyResolver(
                builder,
                _ => alreadyBuiltContainerConfigured = true,
                null,
                null);
            InvokeAfterPlatformServicesSetup(builder);
            await Assert.That(alreadyBuiltContainerConfigured).IsTrue();
        });
    }

    /// <summary>Covers Ninject setup callback execution.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task NinjectMixins_InvokePlatformSetup_CoversContainerWiring()
    {
        await RunWithLocatorRestore(async () =>
        {
            AppBuilder? builder = null;
            await Assert.That(() =>
                NinjectMixins.UseReactiveUIWithNinject(builder!, _ => { }, null)).ThrowsExactly<ArgumentNullException>();

            builder = AppBuilder.Configure<Application>();
            _ = NinjectMixins.UseReactiveUIWithNinject(builder, null!, null);
            await Assert.That(() => InvokeAfterPlatformServicesSetup(builder)).ThrowsExactly<ArgumentNullException>();

            ResetNormalBuilderState();

            var containerConfigured = false;
            var reactiveBuilderConfigured = false;
            builder = AppBuilder.Configure<Application>();
            var result = NinjectMixins.UseReactiveUIWithNinject(
                builder,
                _ => containerConfigured = true,
                rxuiBuilder => reactiveBuilderConfigured = rxuiBuilder is not null);

            InvokeAfterPlatformServicesSetup(builder);

            await Assert.That(result).IsSameReferenceAs(builder);
            await Assert.That(containerConfigured).IsTrue();
            await Assert.That(reactiveBuilderConfigured).IsTrue();

            builder = AppBuilder.Configure<Application>();
            var alreadyBuiltContainerConfigured = false;
            _ = NinjectMixins.UseReactiveUIWithNinject(builder, _ => alreadyBuiltContainerConfigured = true, null);
            InvokeAfterPlatformServicesSetup(builder);
            await Assert.That(alreadyBuiltContainerConfigured).IsTrue();
        });
    }

    /// <summary>Invokes AppBuilder platform setup callback.</summary>
    /// <param name="builder">The app builder.</param>
    private static void InvokeAfterPlatformServicesSetup(AppBuilder builder)
    {
        var property = typeof(AppBuilder).GetProperty(
            "AfterPlatformServicesSetupCallback",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var callback = (Action<AppBuilder>?)property?.GetValue(builder);
        callback?.Invoke(builder);
    }

    /// <summary>Resets the normal ReactiveUI builder state.</summary>
    private static void ResetNormalBuilderState() =>
        ReactiveUI.Builder.ReactiveUIBuilder.ResetBuilderStateForTests();

    /// <summary>Runs a test action and restores the global locator afterwards.</summary>
    /// <param name="action">The action to run.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static async Task RunWithLocatorRestore(Func<Task> action)
    {
        var originalLocator = Locator.GetLocator();

        try
        {
            await action();
        }
        finally
        {
            Locator.SetLocator(originalLocator);
            ResetNormalBuilderState();
        }
    }
}
