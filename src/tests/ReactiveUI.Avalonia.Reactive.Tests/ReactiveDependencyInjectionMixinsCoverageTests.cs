// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
extern alias reactiveautofac;
extern alias reactivedryioc;
extern alias reactivemicrosoft;
extern alias reactiveninject;

using Autofac;
using Avalonia;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI.Avalonia.Reactive;
using ReactiveUI.Reactive.Builder;
using Splat;
using ReactiveAutofacMixins = reactiveautofac::ReactiveUI.Avalonia.Reactive.Splat.AvaloniaMixins;
using ReactiveDryIocMixins = reactivedryioc::ReactiveUI.Avalonia.Reactive.Splat.AvaloniaMixins;
using ReactiveMicrosoftMixins = reactivemicrosoft::ReactiveUI.Avalonia.Reactive.Splat.AvaloniaMixins;
using ReactiveNinjectMixins = reactiveninject::ReactiveUI.Avalonia.Reactive.Splat.AvaloniaMixins;

namespace ReactiveUI.Avalonia.Reactive.Tests;

/// <summary>Coverage tests for reactive dependency-injection Avalonia mixins.</summary>
public class ReactiveDependencyInjectionMixinsCoverageTests
{
    /// <summary>The service value registered by container callbacks.</summary>
    private const string ConfiguredService = "configured";

    /// <summary>Verifies linked reactive core AppBuilder callbacks execute through the platform setup hook.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveCoreAppBuilderExtensions_AfterPlatformCallbacks_ExecuteDeferredWork()
    {
        _ = new TestViewModel();
        _ = new TestView();
        var originalLocator = Locator.GetLocator();
        var registerBuilder = AppBuilder.Configure<Application>()
            .RegisterReactiveUIViews(typeof(ReactiveDependencyInjectionMixinsCoverageTests).Assembly);
        var container = new object();
        var factoryCalled = false;
        var resolverFactoryCalled = false;
        var configCalled = false;
        var containerBuilder = AppBuilder.Configure<Application>();

        try
        {
            registerBuilder.AfterPlatformServicesSetupCallback!(registerBuilder);

            _ = containerBuilder.UseReactiveUIWithDIContainer(
                containerFactory: () =>
                {
                    factoryCalled = true;
                    return container;
                },
                containerConfig: value => configCalled = ReferenceEquals(value, container),
                dependencyResolverFactory: value =>
                {
                    resolverFactoryCalled = ReferenceEquals(value, container);
                    return (IDependencyResolver)AppLocator.CurrentMutable;
                },
                static _ => { });

            containerBuilder.AfterPlatformServicesSetupCallback!(containerBuilder);

            await Assert.That(AppLocator.Current.GetService<IViewFor<TestViewModel>>()).IsNotNull();
            await Assert.That(factoryCalled).IsTrue();
            await Assert.That(resolverFactoryCalled).IsTrue();
            await Assert.That(configCalled).IsTrue();
        }
        finally
        {
            Locator.SetLocator(originalLocator);
            ReactiveUIBuilder.ResetBuilderStateForTests();
        }
    }

    /// <summary>Verifies null command binding ignores targets that cannot source commands.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindCommandToObject_NullCommand_IgnoresNonCommandSource()
    {
        var sut = new AvaloniaCreatesCommandBinding();
        var parameter = new Signal<object?>();

        await Assert.That(sut.BindCommandToObject(null, new TextBlock(), parameter)).IsNull();
    }

    /// <summary>Verifies reactive DI overload forwarding and null argument validation.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveDependencyInjectionMixins_CoverOverloadsAndNullGuards()
    {
        var autofacBuilder = AppBuilder.Configure<Application>();
        var dryIocBuilder = AppBuilder.Configure<Application>();
        var microsoftBuilder = AppBuilder.Configure<Application>();
        var ninjectBuilder = AppBuilder.Configure<Application>();
        AppBuilder? nullBuilder = null;

        await Assert.That(ReactiveAutofacMixins.UseReactiveUIWithAutofac(autofacBuilder, static _ => { }))
            .IsSameReferenceAs(autofacBuilder);
        await Assert.That(ReactiveAutofacMixins.UseReactiveUIWithAutofac(autofacBuilder, static _ => { }, static _ => { }))
            .IsSameReferenceAs(autofacBuilder);
        await Assert.That(() => ReactiveAutofacMixins.UseReactiveUIWithAutofac(autofacBuilder, null!, null, null))
            .ThrowsExactly<ArgumentNullException>();

        await Assert.That(ReactiveDryIocMixins.UseReactiveUIWithDryIoc(dryIocBuilder, static _ => { }))
            .IsSameReferenceAs(dryIocBuilder);
        await Assert.That(() => ReactiveDryIocMixins.UseReactiveUIWithDryIoc(nullBuilder!, static _ => { }, null))
            .ThrowsExactly<ArgumentNullException>();
        _ = ReactiveDryIocMixins.UseReactiveUIWithDryIoc(dryIocBuilder, null!, null);
        await Assert.That(() => dryIocBuilder.AfterPlatformServicesSetupCallback!(dryIocBuilder))
            .ThrowsExactly<ArgumentNullException>();

        await Assert.That(ReactiveMicrosoftMixins.UseReactiveUIWithMicrosoftDependencyResolver(microsoftBuilder, static _ => { }))
            .IsSameReferenceAs(microsoftBuilder);
        await Assert.That(ReactiveMicrosoftMixins.UseReactiveUIWithMicrosoftDependencyResolver(microsoftBuilder, static _ => { }, static _ => { }))
            .IsSameReferenceAs(microsoftBuilder);
        await Assert.That(() => ReactiveMicrosoftMixins.UseReactiveUIWithMicrosoftDependencyResolver(
                nullBuilder!,
                static _ => { },
                (Action<IServiceProvider?>?)null,
                null))
            .ThrowsExactly<ArgumentNullException>();
        _ = ReactiveMicrosoftMixins.UseReactiveUIWithMicrosoftDependencyResolver(
            microsoftBuilder,
            null!,
            (Action<IServiceProvider?>?)null,
            null);
        await Assert.That(() => microsoftBuilder.AfterPlatformServicesSetupCallback!(microsoftBuilder))
            .ThrowsExactly<ArgumentNullException>();

        await Assert.That(ReactiveNinjectMixins.UseReactiveUIWithNinject(ninjectBuilder, static _ => { }))
            .IsSameReferenceAs(ninjectBuilder);
        await Assert.That(() => ReactiveNinjectMixins.UseReactiveUIWithNinject(nullBuilder!, static _ => { }, null))
            .ThrowsExactly<ArgumentNullException>();
        _ = ReactiveNinjectMixins.UseReactiveUIWithNinject(ninjectBuilder, null!, null);
        await Assert.That(() => ninjectBuilder.AfterPlatformServicesSetupCallback!(ninjectBuilder))
            .ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies that reactive Autofac registration executes through the deferred platform setup callback.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UseReactiveUIWithAutofac_AfterPlatformCallback_ConfiguresContainerAndResolver()
    {
        ReactiveUIBuilder.ResetBuilderStateForTests();
        var builder = AppBuilder.Configure<Application>();
        var configCalled = false;
        var resolverCalled = false;
        var reactiveBuilderCalled = false;
        var originalLocator = Locator.GetLocator();

        try
        {
            _ = ReactiveAutofacMixins.UseReactiveUIWithAutofac(
                builder,
                containerConfig: containerBuilder =>
                {
                    configCalled = true;
                    _ = containerBuilder.RegisterInstance(ConfiguredService);
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

    /// <summary>Verifies that reactive Autofac handles an already-built app and null optional callbacks.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UseReactiveUIWithAutofac_AfterPlatformCallback_WhenAlreadyBuilt_AllowsNullCallbacks()
    {
        _ = AppLocator.CurrentMutable.CreateReactiveUIBuilder()
            .WithCoreServices()
            .BuildApp();
        var builder = AppBuilder.Configure<Application>();
        var configCalled = false;
        var originalLocator = Locator.GetLocator();

        try
        {
            _ = ReactiveAutofacMixins.UseReactiveUIWithAutofac(
                builder,
                containerConfig: containerBuilder =>
                {
                    configCalled = true;
                    _ = containerBuilder.RegisterInstance(ConfiguredService);
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

    /// <summary>Verifies that reactive DryIoc registration executes through the deferred platform setup callback.</summary>
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
            _ = ReactiveDryIocMixins.UseReactiveUIWithDryIoc(
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

    /// <summary>Verifies that reactive DryIoc handles an already-built app and a null ReactiveUI callback.</summary>
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
            _ = ReactiveDryIocMixins.UseReactiveUIWithDryIoc(
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

    /// <summary>Verifies that reactive Microsoft dependency resolver registration executes through the deferred platform setup callback.</summary>
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
            _ = ReactiveMicrosoftMixins.UseReactiveUIWithMicrosoftDependencyResolver(
                builder,
                containerConfig: services =>
                {
                    configCalled = services is not null;
                    _ = services!.AddSingleton(ConfiguredService);
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

    /// <summary>Verifies that reactive Microsoft dependency resolver handles an already-built app and null optional callbacks.</summary>
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
            _ = ReactiveMicrosoftMixins.UseReactiveUIWithMicrosoftDependencyResolver(
                builder,
                containerConfig: services =>
                {
                    configCalled = services is not null;
                    _ = services!.AddSingleton(ConfiguredService);
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

    /// <summary>Verifies that reactive Ninject registration executes through the deferred platform setup callback.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UseReactiveUIWithNinject_AfterPlatformCallback_ConfiguresContainer()
    {
        ReactiveUIBuilder.ResetBuilderStateForTests();
        var builder = AppBuilder.Configure<Application>();
        var configCalled = false;
        var reactiveBuilderCalled = false;
        var originalLocator = Locator.GetLocator();

        try
        {
            _ = ReactiveNinjectMixins.UseReactiveUIWithNinject(
                builder,
                containerConfig: kernel => configCalled = kernel is not null,
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

    /// <summary>Verifies that reactive Ninject handles an already-built app and a null ReactiveUI callback.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UseReactiveUIWithNinject_AfterPlatformCallback_WhenAlreadyBuilt_AllowsNullCallback()
    {
        _ = AppLocator.CurrentMutable.CreateReactiveUIBuilder()
            .WithCoreServices()
            .BuildApp();
        var builder = AppBuilder.Configure<Application>();
        var configCalled = false;
        var originalLocator = Locator.GetLocator();

        try
        {
            _ = ReactiveNinjectMixins.UseReactiveUIWithNinject(
                builder,
                containerConfig: kernel => configCalled = kernel is not null,
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

    /// <summary>A view model used by deferred view registration tests.</summary>
    private sealed class TestViewModel : ReactiveObject;

    /// <summary>A view used by deferred view registration tests.</summary>
    private sealed class TestView : IViewFor<TestViewModel>
    {
        /// <summary>Gets or sets the view model.</summary>
        public TestViewModel? ViewModel { get; set; }

        /// <inheritdoc/>
        object? IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (TestViewModel?)value;
        }
    }
}
