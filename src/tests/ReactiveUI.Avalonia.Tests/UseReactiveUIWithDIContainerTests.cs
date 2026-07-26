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
            containerFactory: static () => new object(),
            containerConfig: static _ => { },
            dependencyResolverFactory: static _ => new DummyResolver(),
            static _ => { })).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies that UseReactiveUIWithDIContainer returns the builder without throwing.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UseReactiveUIWithDIContainer_Returns_Builder_NoThrow()
    {
        var builder = AppBuilder.Configure<Application>();

        var result = builder.UseReactiveUIWithDIContainer(
            containerFactory: static () => new object(),
            containerConfig: static _ => { },
            dependencyResolverFactory: static _ => new DummyResolver(),
            static _ => { });

        await Assert.That(result).IsSameReferenceAs(builder);
    }

    /// <summary>Verifies that the deferred callback validates a null container factory.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UseReactiveUIWithDIContainer_AfterPlatformCallback_Throws_On_Null_ContainerFactory() =>
        await Assert.That(static () => AppBuilderExtensions.ConfigureReactiveUIDIContainer<object>(
                AppLocator.CurrentMutable,
                containerFactory: null!,
                containerConfig: static _ => { },
                dependencyResolverFactory: static _ => new DummyResolver()))
            .ThrowsExactly<ArgumentNullException>();

    /// <summary>Verifies that the deferred callback validates a null container config action.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UseReactiveUIWithDIContainer_AfterPlatformCallback_Throws_On_Null_ContainerConfig() => await Assert.That(static () => AppBuilderExtensions.ConfigureReactiveUIDIContainer(
            AppLocator.CurrentMutable,
            containerFactory: static () => new object(),
            containerConfig: null!,
            dependencyResolverFactory: static _ => new DummyResolver())).ThrowsExactly<ArgumentNullException>();

    /// <summary>Verifies that the deferred callback validates a null dependency resolver factory.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UseReactiveUIWithDIContainer_AfterPlatformCallback_Throws_On_Null_DependencyResolverFactory() =>
        await Assert.That(static () => AppBuilderExtensions.ConfigureReactiveUIDIContainer(
                AppLocator.CurrentMutable,
                containerFactory: static () => new object(),
                containerConfig: static _ => { },
                dependencyResolverFactory: null!))
            .ThrowsExactly<ArgumentNullException>();

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

        AppBuilderExtensions.ConfigureReactiveUI(_ => reactiveConfigured = true);
        AppBuilderExtensions.ConfigureReactiveUIDIContainer(
            resolver,
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
            });

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

        AppBuilderExtensions.ConfigureReactiveUIDIContainer(
            resolver: null,
            containerFactory: () =>
            {
                factoryCalled = true;
                return new object();
            },
            containerConfig: static _ => { },
            dependencyResolverFactory: static _ => new DummyResolver());

        await Assert.That(factoryCalled).IsFalse();
    }

    /// <summary>A dummy dependency resolver implementation for testing.</summary>
    private sealed class DummyResolver : IDependencyResolver
    {
        /// <inheritdoc/>
        public void Dispose()
        {
        }

        /// <inheritdoc/>
        public object? GetService(Type? serviceType) => null;

        /// <inheritdoc/>
        public object? GetService(Type? serviceType, string? contract) => null;

        /// <inheritdoc/>
        public T? GetService<T>() => (T?)GetService(typeof(T));

        /// <inheritdoc/>
        public T? GetService<T>(string? contract) => (T?)GetService(typeof(T), contract);

        /// <inheritdoc/>
        public IEnumerable<object> GetServices(Type? serviceType) => [];

        /// <inheritdoc/>
        public IEnumerable<object> GetServices(Type? serviceType, string? contract) => GetServices(serviceType);

        /// <inheritdoc/>
        public IEnumerable<T> GetServices<T>()
        {
            foreach (var service in GetServices(typeof(T)))
            {
                if (service is T typedService)
                {
                    yield return typedService;
                }
            }
        }

        /// <inheritdoc/>
        public IEnumerable<T> GetServices<T>(string? contract)
        {
            foreach (var service in GetServices(typeof(T), contract))
            {
                if (service is T typedService)
                {
                    yield return typedService;
                }
            }
        }

        /// <inheritdoc/>
        public bool HasRegistration(Type? serviceType) => false;

        /// <inheritdoc/>
        public bool HasRegistration(Type? serviceType, string? contract) => false;

        /// <inheritdoc/>
        public bool HasRegistration<T>() => HasRegistration(typeof(T));

        /// <inheritdoc/>
        public bool HasRegistration<T>(string? contract) => HasRegistration(typeof(T), contract);

        /// <inheritdoc/>
        public void Register(Func<object?> factory, Type? serviceType)
        {
        }

        /// <inheritdoc/>
        public void Register(Func<object?> factory, Type? serviceType, string? contract)
        {
        }

        /// <inheritdoc/>
        public void Register<T>(Func<T?> factory) => Register(() => factory(), typeof(T));

        /// <inheritdoc/>
        public void Register<T>(Func<T?> factory, string? contract) => Register(() => factory(), typeof(T), contract);

        /// <inheritdoc/>
        public void Register<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService, new() =>
            Register(static () => new TImplementation(), typeof(TService));

        /// <inheritdoc/>
        public void Register<TService, TImplementation>(string? contract)
            where TService : class
            where TImplementation : class, TService, new() =>
            Register(static () => new TImplementation(), typeof(TService), contract);

        /// <inheritdoc/>
        public void UnregisterCurrent(Type? serviceType)
        {
        }

        /// <inheritdoc/>
        public void UnregisterCurrent(Type? serviceType, string? contract)
        {
        }

        /// <inheritdoc/>
        public void UnregisterCurrent<T>() => UnregisterCurrent(typeof(T));

        /// <inheritdoc/>
        public void UnregisterCurrent<T>(string? contract) => UnregisterCurrent(typeof(T), contract);

        /// <inheritdoc/>
        public void UnregisterAll(Type? serviceType)
        {
        }

        /// <inheritdoc/>
        public void UnregisterAll(Type? serviceType, string? contract)
        {
        }

        /// <inheritdoc/>
        public void UnregisterAll<T>() => UnregisterAll(typeof(T));

        /// <inheritdoc/>
        public void UnregisterAll<T>(string? contract) => UnregisterAll(typeof(T), contract);

        /// <inheritdoc/>
        public IDisposable ServiceRegistrationCallback(Type serviceType, Action<IDisposable> callback) => EmptyDisposable.Instance;

        /// <inheritdoc/>
        public IDisposable ServiceRegistrationCallback(Type serviceType, string? contract, Action<IDisposable> callback) => EmptyDisposable.Instance;

        /// <inheritdoc/>
        public IDisposable ServiceRegistrationCallback<T>(Action<IDisposable> callback) => ServiceRegistrationCallback(typeof(T), callback);

        /// <inheritdoc/>
        public IDisposable ServiceRegistrationCallback<T>(string? contract, Action<IDisposable> callback) => ServiceRegistrationCallback(typeof(T), contract, callback);

        /// <inheritdoc/>
        public void RegisterConstant<T>(T? value)
            where T : class =>
            Register(() => value, typeof(T));

        /// <inheritdoc/>
        public void RegisterConstant<T>(T? value, string? contract)
            where T : class =>
            Register(() => value, typeof(T), contract);

        /// <inheritdoc/>
        public void RegisterLazySingleton<T>(Func<T?> valueFactory)
            where T : class =>
            Register(() => valueFactory(), typeof(T));

        /// <inheritdoc/>
        public void RegisterLazySingleton<T>(Func<T?> valueFactory, string? contract)
            where T : class =>
            Register(() => valueFactory(), typeof(T), contract);
    }
}
