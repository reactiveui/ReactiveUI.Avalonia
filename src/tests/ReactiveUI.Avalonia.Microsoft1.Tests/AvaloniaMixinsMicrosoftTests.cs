// Copyright (c) 2019-2026 ReactiveUI and Avalonia Teams, and Contributors. All rights reserved.
// Licensed under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Disposables;
using Avalonia;
using ReactiveUI.Avalonia.Splat;
using Splat;

namespace ReactiveUI.Avalonia.Microsoft.Tests;

/// <summary>
/// Tests for Microsoft dependency injection-based Avalonia mixin registration.
/// </summary>
public class AvaloniaMixinsMicrosoftTests
{
    /// <summary>
    /// Verifies that <see cref="AvaloniaMixins.UseReactiveUIWithMicrosoftDependencyResolver"/>
    /// throws <see cref="ArgumentNullException"/> when the builder is null.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test.</returns>
    [Test]
    public async Task UseReactiveUIWithMicrosoftDependencyResolver_ThrowsOnNullBuilder()
    {
        AppBuilder? builder = null;
        await Assert.That(() =>
            AvaloniaMixins.UseReactiveUIWithMicrosoftDependencyResolver(
                builder!,
                _ => { },
                null)).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that <see cref="AppBuilderExtensions.UseReactiveUIWithDIContainer{TContainer}"/>
    /// does not throw and returns the same builder instance with valid arguments.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test.</returns>
    [Test]
    public async Task UseReactiveUIWithDIContainer_DoesNotThrow_WithValidArgs()
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
    /// A minimal <see cref="IDependencyResolver"/> implementation used for testing.
    /// </summary>
    private sealed class DummyResolver : IDependencyResolver
    {
        /// <inheritdoc/>
        public object? GetService(Type? serviceType) => null;

        /// <inheritdoc/>
        public object? GetService(Type? serviceType, string? contract) => null;

        /// <inheritdoc/>
        public T? GetService<T>() => default;

        /// <inheritdoc/>
        public T? GetService<T>(string? contract) => default;

        /// <inheritdoc/>
        public IEnumerable<object> GetServices(Type? serviceType) => Array.Empty<object>();

        /// <inheritdoc/>
        public IEnumerable<object> GetServices(Type? serviceType, string? contract) => Array.Empty<object>();

        /// <inheritdoc/>
        public IEnumerable<T> GetServices<T>() => Array.Empty<T>();

        /// <inheritdoc/>
        public IEnumerable<T> GetServices<T>(string? contract) => Array.Empty<T>();

        /// <inheritdoc/>
        public bool HasRegistration(Type? serviceType) => false;

        /// <inheritdoc/>
        public bool HasRegistration(Type? serviceType, string? contract) => false;

        /// <inheritdoc/>
        public bool HasRegistration<T>() => false;

        /// <inheritdoc/>
        public bool HasRegistration<T>(string? contract) => false;

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
        public IDisposable ServiceRegistrationCallback(Type serviceType, Action<IDisposable> callback) => Disposable.Empty;

        /// <inheritdoc/>
        public IDisposable ServiceRegistrationCallback(Type serviceType, string? contract, Action<IDisposable> callback) => Disposable.Empty;

        /// <inheritdoc/>
        public IDisposable ServiceRegistrationCallback<T>(Action<IDisposable> callback) => Disposable.Empty;

        /// <inheritdoc/>
        public IDisposable ServiceRegistrationCallback<T>(string? contract, Action<IDisposable> callback) => Disposable.Empty;

        /// <inheritdoc/>
        public void Dispose()
        {
        }
    }
}
