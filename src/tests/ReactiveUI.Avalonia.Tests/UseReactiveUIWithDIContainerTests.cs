// Copyright (c) 2019-2026 ReactiveUI and Avalonia Teams, and Contributors. All rights reserved.
// Licensed under the MIT license.
// See the LICENSE file in the project root for full license information.

using Avalonia;
using Splat;

namespace ReactiveUI.Avalonia.Tests;

/// <summary>
/// Tests for the UseReactiveUIWithDIContainer extension method.
/// </summary>
public class UseReactiveUIWithDIContainerTests
{
    /// <summary>
    /// Verifies that UseReactiveUIWithDIContainer throws on a null builder.
    /// </summary>
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

    /// <summary>
    /// Verifies that UseReactiveUIWithDIContainer returns the builder without throwing.
    /// </summary>
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

    /// <summary>
    /// A dummy dependency resolver implementation for testing.
    /// </summary>
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
            return Array.Empty<object>();
        }

        /// <inheritdoc/>
        public IEnumerable<object> GetServices(Type? serviceType, string? contract)
        {
            return Array.Empty<object>();
        }

        /// <inheritdoc/>
        public IEnumerable<T> GetServices<T>()
        {
            return Array.Empty<T>();
        }

        /// <inheritdoc/>
        public IEnumerable<T> GetServices<T>(string? contract)
        {
            return Array.Empty<T>();
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
            return System.Reactive.Disposables.Disposable.Empty;
        }

        /// <inheritdoc/>
        public IDisposable ServiceRegistrationCallback(Type serviceType, string? contract, Action<IDisposable> callback)
        {
            return System.Reactive.Disposables.Disposable.Empty;
        }

        /// <inheritdoc/>
        public IDisposable ServiceRegistrationCallback<T>(Action<IDisposable> callback)
        {
            return System.Reactive.Disposables.Disposable.Empty;
        }

        /// <inheritdoc/>
        public IDisposable ServiceRegistrationCallback<T>(string? contract, Action<IDisposable> callback)
        {
            return System.Reactive.Disposables.Disposable.Empty;
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
