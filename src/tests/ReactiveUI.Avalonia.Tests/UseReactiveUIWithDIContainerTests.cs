// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Avalonia;
using ReactiveUI.Builder;
using Splat;

namespace ReactiveUI.Avalonia.Tests;

/// <summary>Tests for the UseReactiveUIWithDIContainer extension method.</summary>
public class UseReactiveUIWithDIContainerTests
{
    /// <summary>Verifies that UseReactiveUIWithDIContainer throws on a null builder.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UseReactiveUIWithDIContainer_Throws_On_Null_Builder()
    {
        AppBuilder? builder = null;
        await Assert.That(() => builder!.UseReactiveUIWithDIContainer(
            containerFactory: () => new object(),
            containerConfig: _ => { },
            dependencyResolverFactory: _ => new DummyResolver(),
            _ => { })).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies that UseReactiveUIWithDIContainer returns the builder without throwing.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UseReactiveUIWithDIContainer_Returns_Builder_NoThrow()
    {
        var builder = AppBuilder.Configure<Application>();

        var result = builder.UseReactiveUIWithDIContainer(
            containerFactory: () => new object(),
            containerConfig: _ => { },
            dependencyResolverFactory: _ => new DummyResolver(),
            _ => { });

        await Assert.That(result).IsSameReferenceAs(builder);
    }

    /// <summary>Verifies that the deferred callback validates a null container factory.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UseReactiveUIWithDIContainer_AfterPlatformCallback_Throws_On_Null_ContainerFactory()
    {
        var builder = AppBuilder.Configure<Application>().UseReactiveUIWithDIContainer<object>(
            containerFactory: null!,
            containerConfig: _ => { },
            dependencyResolverFactory: _ => new DummyResolver(),
            _ => { });

        await Assert.That(() => InvokeAfterPlatformServicesSetup(builder)).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies that the deferred callback validates a null container config action.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UseReactiveUIWithDIContainer_AfterPlatformCallback_Throws_On_Null_ContainerConfig()
    {
        var builder = AppBuilder.Configure<Application>().UseReactiveUIWithDIContainer(
            containerFactory: () => new object(),
            containerConfig: null!,
            dependencyResolverFactory: _ => new DummyResolver(),
            _ => { });

        await Assert.That(() => InvokeAfterPlatformServicesSetup(builder)).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies that the deferred callback validates a null dependency resolver factory.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UseReactiveUIWithDIContainer_AfterPlatformCallback_Throws_On_Null_DependencyResolverFactory()
    {
        var builder = AppBuilder.Configure<Application>().UseReactiveUIWithDIContainer(
            containerFactory: () => new object(),
            containerConfig: _ => { },
            dependencyResolverFactory: null!,
            _ => { });

        await Assert.That(() => InvokeAfterPlatformServicesSetup(builder)).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies that the deferred callback creates, registers, and configures the container.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UseReactiveUIWithDIContainer_AfterPlatformCallback_ConfiguresContainer()
    {
        ReactiveUIBuilder.ResetBuilderStateForTests();
        var container = new object();
        var resolver = AppLocator.CurrentMutable!;
        var factoryCalled = false;
        var resolverFactoryCalled = false;
        var configCalled = false;
        var reactiveConfigured = false;

        var builder = AppBuilder.Configure<Application>().UseReactiveUIWithDIContainer(
            containerFactory: () =>
            {
                factoryCalled = true;
                return container;
            },
            containerConfig: value => configCalled = ReferenceEquals(value, container),
            dependencyResolverFactory: value =>
            {
                resolverFactoryCalled = ReferenceEquals(value, container);
                return (IDependencyResolver)resolver;
            },
            _ => reactiveConfigured = true);

        InvokeAfterPlatformServicesSetup(builder);

        await Assert.That(factoryCalled).IsTrue();
        await Assert.That(resolverFactoryCalled).IsTrue();
        await Assert.That(configCalled).IsTrue();
        await Assert.That(reactiveConfigured).IsTrue();
        await Assert.That(AppLocator.Current).IsSameReferenceAs(resolver);
        await Assert.That(RxSchedulers.MainThreadScheduler).IsSameReferenceAs(AvaloniaScheduler.Instance);
    }

    /// <summary>Verifies that dependency injection configuration returns when no mutable resolver is available.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UseReactiveUIWithDIContainer_WhenResolverNull_Returns()
    {
        var factoryCalled = false;

        InvokeConfigureReactiveUIDIContainer<object>(
            resolver: null,
            containerFactory: () =>
            {
                factoryCalled = true;
                return new object();
            },
            containerConfig: _ => { },
            dependencyResolverFactory: _ => new DummyResolver());

        await Assert.That(factoryCalled).IsFalse();
    }

    /// <summary>Invokes the AppBuilder platform setup callback registered by extension methods.</summary>
    /// <param name="builder">The application builder.</param>
    private static void InvokeAfterPlatformServicesSetup(AppBuilder builder)
    {
        var property = typeof(AppBuilder).GetProperty(
            "AfterPlatformServicesSetupCallback",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);

        var callback = (Action<AppBuilder>?)property?.GetValue(builder);
        callback?.Invoke(builder);
    }

    /// <summary>Invokes the private dependency injection container helper.</summary>
    /// <typeparam name="TContainer">The container type.</typeparam>
    /// <param name="resolver">The mutable dependency resolver.</param>
    /// <param name="containerFactory">The container factory.</param>
    /// <param name="containerConfig">The container configuration callback.</param>
    /// <param name="dependencyResolverFactory">The dependency resolver factory.</param>
    private static void InvokeConfigureReactiveUIDIContainer<TContainer>(
        IMutableDependencyResolver? resolver,
        Func<TContainer> containerFactory,
        Action<TContainer> containerConfig,
        Func<TContainer, IDependencyResolver> dependencyResolverFactory)
        where TContainer : class
    {
        var method = typeof(AppBuilderExtensions).GetMethod(
            "ConfigureReactiveUIDIContainer",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

        try
        {
            _ = method!.MakeGenericMethod(typeof(TContainer)).Invoke(
                null,
                [resolver, containerFactory, containerConfig, dependencyResolverFactory]);
        }
        catch (System.Reflection.TargetInvocationException error) when (error.InnerException is not null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(error.InnerException).Throw();
        }
    }

    /// <summary>A dummy dependency resolver implementation for testing.</summary>
    private sealed class DummyResolver : IDependencyResolver
    {
        /// <inheritdoc/>
        public void Dispose()
        {
        }

        /// <inheritdoc/>
        public object? GetService(Type? serviceType)
        {
            return null;
        }

        /// <inheritdoc/>
        public object? GetService(Type? serviceType, string? contract)
        {
            return null;
        }

        /// <inheritdoc/>
        public T? GetService<T>()
        {
            return default;
        }

        /// <inheritdoc/>
        public T? GetService<T>(string? contract)
        {
            return default;
        }

        /// <inheritdoc/>
        public IEnumerable<object> GetServices(Type? serviceType)
        {
            return [];
        }

        /// <inheritdoc/>
        public IEnumerable<object> GetServices(Type? serviceType, string? contract)
        {
            return [];
        }

        /// <inheritdoc/>
        public IEnumerable<T> GetServices<T>()
        {
            return [];
        }

        /// <inheritdoc/>
        public IEnumerable<T> GetServices<T>(string? contract)
        {
            return [];
        }

        /// <inheritdoc/>
        public bool HasRegistration(Type? serviceType)
        {
            return false;
        }

        /// <inheritdoc/>
        public bool HasRegistration(Type? serviceType, string? contract)
        {
            return false;
        }

        /// <inheritdoc/>
        public bool HasRegistration<T>()
        {
            return false;
        }

        /// <inheritdoc/>
        public bool HasRegistration<T>(string? contract)
        {
            return false;
        }

        /// <inheritdoc/>
        public void Register(Func<object?> factory, Type? serviceType)
        {
        }

        /// <inheritdoc/>
        public void Register(Func<object?> factory, Type? serviceType, string? contract)
        {
        }

        /// <inheritdoc/>
        public void Register<T>(Func<T?> factory)
        {
        }

        /// <inheritdoc/>
        public void Register<T>(Func<T?> factory, string? contract)
        {
        }

        /// <inheritdoc/>
        public void UnregisterCurrent(Type? serviceType)
        {
        }

        /// <inheritdoc/>
        public void UnregisterCurrent(Type? serviceType, string? contract)
        {
        }

        /// <inheritdoc/>
        public void UnregisterCurrent<T>()
        {
        }

        /// <inheritdoc/>
        public void UnregisterCurrent<T>(string? contract)
        {
        }

        /// <inheritdoc/>
        public void UnregisterAll(Type? serviceType)
        {
        }

        /// <inheritdoc/>
        public void UnregisterAll(Type? serviceType, string? contract)
        {
        }

        /// <inheritdoc/>
        public void UnregisterAll<T>()
        {
        }

        /// <inheritdoc/>
        public void UnregisterAll<T>(string? contract)
        {
        }

        /// <inheritdoc/>
        public IDisposable ServiceRegistrationCallback(Type serviceType, Action<IDisposable> callback)
        {
            return EmptyDisposable.Instance;
        }

        /// <inheritdoc/>
        public IDisposable ServiceRegistrationCallback(Type serviceType, string? contract, Action<IDisposable> callback)
        {
            return EmptyDisposable.Instance;
        }

        /// <inheritdoc/>
        public IDisposable ServiceRegistrationCallback<T>(Action<IDisposable> callback)
        {
            return EmptyDisposable.Instance;
        }

        /// <inheritdoc/>
        public IDisposable ServiceRegistrationCallback<T>(string? contract, Action<IDisposable> callback)
        {
            return EmptyDisposable.Instance;
        }

        /// <inheritdoc/>
        public void Register<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService, new()
        {
        }

        /// <inheritdoc/>
        public void Register<TService, TImplementation>(string? contract)
            where TService : class
            where TImplementation : class, TService, new()
        {
        }

        /// <inheritdoc/>
        public void RegisterConstant<T>(T? value)
            where T : class
        {
        }

        /// <inheritdoc/>
        public void RegisterConstant<T>(T? value, string? contract)
            where T : class
        {
        }

        /// <inheritdoc/>
        public void RegisterLazySingleton<T>(Func<T?> valueFactory)
            where T : class
        {
        }

        /// <inheritdoc/>
        public void RegisterLazySingleton<T>(Func<T?> valueFactory, string? contract)
            where T : class
        {
        }
    }
}
