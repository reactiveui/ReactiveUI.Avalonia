// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
extern alias reactiveautofac;
extern alias reactivedryioc;
extern alias reactivemicrosoft;
extern alias reactiveninject;

using System.Reflection;
using Avalonia;
using Splat;
using ReactiveAutofacMixins = reactiveautofac::ReactiveUI.Avalonia.Reactive.Splat.AvaloniaMixins;
using ReactiveDryIocMixins = reactivedryioc::ReactiveUI.Avalonia.Reactive.Splat.AvaloniaMixins;
using ReactiveMicrosoftMixins = reactivemicrosoft::ReactiveUI.Avalonia.Reactive.Splat.AvaloniaMixins;
using ReactiveNinjectMixins = reactiveninject::ReactiveUI.Avalonia.Reactive.Splat.AvaloniaMixins;

namespace ReactiveUI.Avalonia.Reactive.Tests;

/// <summary>Coverage tests for reactive dependency-injection mixin setup callbacks.</summary>
public class ReactiveAvaloniaMixinsCoverageTests
{
    /// <summary>Covers reactive Autofac setup callback execution.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveAutofacMixins_InvokePlatformSetup_CoversContainerWiring()
    {
        await RunWithLocatorRestore(async () =>
        {
            AppBuilder? builder = null;
            await Assert.That(() =>
                ReactiveAutofacMixins.UseReactiveUIWithAutofac(builder!, _ => { }, null, null)).ThrowsExactly<ArgumentNullException>();

            builder = AppBuilder.Configure<Application>();
            await Assert.That(() =>
                ReactiveAutofacMixins.UseReactiveUIWithAutofac(builder, null!, null, null)).ThrowsExactly<ArgumentNullException>();

            ResetReactiveBuilderState();

            var containerConfigured = false;
            var resolverConfigured = false;
            var reactiveBuilderConfigured = false;
            builder = AppBuilder.Configure<Application>();
            var result = ReactiveAutofacMixins.UseReactiveUIWithAutofac(
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
            _ = ReactiveAutofacMixins.UseReactiveUIWithAutofac(builder, _ => alreadyBuiltContainerConfigured = true, null, null);
            InvokeAfterPlatformServicesSetup(builder);
            await Assert.That(alreadyBuiltContainerConfigured).IsTrue();
        });
    }

    /// <summary>Covers reactive DryIoc setup callback execution.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveDryIocMixins_InvokePlatformSetup_CoversContainerWiring()
    {
        await RunWithLocatorRestore(async () =>
        {
            AppBuilder? builder = null;
            await Assert.That(() =>
                ReactiveDryIocMixins.UseReactiveUIWithDryIoc(builder!, _ => { }, null)).ThrowsExactly<ArgumentNullException>();

            builder = AppBuilder.Configure<Application>();
            _ = ReactiveDryIocMixins.UseReactiveUIWithDryIoc(builder, null!, null);
            await Assert.That(() => InvokeAfterPlatformServicesSetup(builder)).ThrowsExactly<ArgumentNullException>();

            ResetReactiveBuilderState();

            var containerConfigured = false;
            var reactiveBuilderConfigured = false;
            builder = AppBuilder.Configure<Application>();
            var result = ReactiveDryIocMixins.UseReactiveUIWithDryIoc(
                builder,
                _ => containerConfigured = true,
                rxuiBuilder => reactiveBuilderConfigured = rxuiBuilder is not null);

            InvokeAfterPlatformServicesSetup(builder);

            await Assert.That(result).IsSameReferenceAs(builder);
            await Assert.That(containerConfigured).IsTrue();
            await Assert.That(reactiveBuilderConfigured).IsTrue();

            builder = AppBuilder.Configure<Application>();
            var alreadyBuiltContainerConfigured = false;
            _ = ReactiveDryIocMixins.UseReactiveUIWithDryIoc(builder, _ => alreadyBuiltContainerConfigured = true, null);
            InvokeAfterPlatformServicesSetup(builder);
            await Assert.That(alreadyBuiltContainerConfigured).IsTrue();
        });
    }

    /// <summary>Covers reactive Microsoft dependency-injection setup callback execution.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveMicrosoftMixins_InvokePlatformSetup_CoversContainerWiring()
    {
        await RunWithLocatorRestore(async () =>
        {
            AppBuilder? builder = null;
            await Assert.That(() =>
                ReactiveMicrosoftMixins.UseReactiveUIWithMicrosoftDependencyResolver(
                    builder!,
                    _ => { },
                    null,
                    null)).ThrowsExactly<ArgumentNullException>();

            builder = AppBuilder.Configure<Application>();
            _ = ReactiveMicrosoftMixins.UseReactiveUIWithMicrosoftDependencyResolver(builder, null!, null, null);
            await Assert.That(() => InvokeAfterPlatformServicesSetup(builder)).ThrowsExactly<ArgumentNullException>();

            ResetReactiveBuilderState();

            var containerConfigured = false;
            var resolverConfigured = false;
            var reactiveBuilderConfigured = false;
            builder = AppBuilder.Configure<Application>();
            var result = ReactiveMicrosoftMixins.UseReactiveUIWithMicrosoftDependencyResolver(
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
            _ = ReactiveMicrosoftMixins.UseReactiveUIWithMicrosoftDependencyResolver(
                builder,
                _ => alreadyBuiltContainerConfigured = true,
                null,
                null);
            InvokeAfterPlatformServicesSetup(builder);
            await Assert.That(alreadyBuiltContainerConfigured).IsTrue();
        });
    }

    /// <summary>Covers reactive Ninject setup callback execution.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveNinjectMixins_InvokePlatformSetup_CoversContainerWiring()
    {
        await RunWithLocatorRestore(async () =>
        {
            AppBuilder? builder = null;
            await Assert.That(() =>
                ReactiveNinjectMixins.UseReactiveUIWithNinject(builder!, _ => { }, null)).ThrowsExactly<ArgumentNullException>();

            builder = AppBuilder.Configure<Application>();
            _ = ReactiveNinjectMixins.UseReactiveUIWithNinject(builder, null!, null);
            await Assert.That(() => InvokeAfterPlatformServicesSetup(builder)).ThrowsExactly<ArgumentNullException>();

            ResetReactiveBuilderState();

            var containerConfigured = false;
            var reactiveBuilderConfigured = false;
            builder = AppBuilder.Configure<Application>();
            var result = ReactiveNinjectMixins.UseReactiveUIWithNinject(
                builder,
                _ => containerConfigured = true,
                rxuiBuilder => reactiveBuilderConfigured = rxuiBuilder is not null);

            InvokeAfterPlatformServicesSetup(builder);

            await Assert.That(result).IsSameReferenceAs(builder);
            await Assert.That(containerConfigured).IsTrue();
            await Assert.That(reactiveBuilderConfigured).IsTrue();

            builder = AppBuilder.Configure<Application>();
            var alreadyBuiltContainerConfigured = false;
            _ = ReactiveNinjectMixins.UseReactiveUIWithNinject(builder, _ => alreadyBuiltContainerConfigured = true, null);
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

    /// <summary>Resets the reactive ReactiveUI builder state.</summary>
    private static void ResetReactiveBuilderState() =>
        ReactiveUI.Reactive.Builder.ReactiveUIBuilder.ResetBuilderStateForTests();

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
            ResetReactiveBuilderState();
        }
    }
}
