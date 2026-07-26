// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Avalonia;
using ReactiveUI.Avalonia.Splat;
using ReactiveUI.Builder;
using Splat;

namespace ReactiveUI.Avalonia.Microsoft.Tests;

/// <summary>Tests for Microsoft dependency injection-based Avalonia mixin registration.</summary>
public class AvaloniaMixinsMicrosoftTests
{
    /// <summary>Verifies that <c>UseReactiveUIWithMicrosoftDependencyResolver</c> throws <see cref="ArgumentNullException"/> when the builder is null.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test.</returns>
    [Test]
    public async Task UseReactiveUIWithMicrosoftDependencyResolver_ThrowsOnNullBuilder()
    {
        AppBuilder? builder = null;
        await Assert.That(() =>
            AvaloniaMixins.UseReactiveUIWithMicrosoftDependencyResolver(
                builder!,
                static _ => { },
                (Action<IServiceProvider?>?)null,
                (Action<ReactiveUIBuilder>?)null)).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies that <see cref="AppBuilderExtensions.UseReactiveUIWithDIContainer{TContainer}"/> does not throw and returns the same builder instance with valid arguments.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test.</returns>
    [Test]
    public async Task UseReactiveUIWithDIContainer_DoesNotThrow_WithValidArgs()
    {
        var builder = AppBuilder.Configure<Application>();
        var result = builder.UseReactiveUIWithDIContainer(
            containerFactory: static () => new object(),
            containerConfig: static _ => { },
            dependencyResolverFactory: static _ => new DummyResolver(),
            static _ => { });
        await Assert.That(result).IsSameReferenceAs(builder);
    }

    /// <summary>A minimal <see cref="IDependencyResolver"/> implementation used for testing.</summary>
    private sealed class DummyResolver : IDependencyResolver
    {
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
        public void Dispose()
        {
        }
    }
}
