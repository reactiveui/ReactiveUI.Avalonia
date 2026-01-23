#pragma warning disable SA1201
using System.Reactive.Disposables;
using Avalonia;
using NUnit.Framework;
using ReactiveUI.Avalonia.Splat;
using Splat;

namespace ReactiveUI.Avalonia.Microsoft.Tests;

public class AvaloniaMixinsMicrosoftTests
{
    [Test]
    public void UseReactiveUIWithMicrosoftDependencyResolver_ThrowsOnNullBuilder()
    {
        AppBuilder? builder = null;
        Assert.Throws<ArgumentNullException>(() =>
            AvaloniaMixins.UseReactiveUIWithMicrosoftDependencyResolver(
                builder!,
                _ => { },
                null));
    }

    [Test]
    public void UseReactiveUIWithDIContainer_DoesNotThrow_WithValidArgs()
    {
        var builder = AppBuilder.Configure<Application>();
        var result = builder.UseReactiveUIWithDIContainer(
            containerFactory: () => new object(),
            containerConfig: _ => { },
            dependencyResolverFactory: _ => new DummyResolver(),
            _ => { });
        Assert.That(result, Is.SameAs(builder));
    }

    private sealed class DummyResolver : IDependencyResolver
    {
        public object? GetService(Type? serviceType) => null;

        public object? GetService(Type? serviceType, string? contract) => null;

        public T? GetService<T>() => default;

        public T? GetService<T>(string? contract) => default;

        public IEnumerable<object> GetServices(Type? serviceType) => Array.Empty<object>();

        public IEnumerable<object> GetServices(Type? serviceType, string? contract) => Array.Empty<object>();

        public IEnumerable<T> GetServices<T>() => Array.Empty<T>();

        public IEnumerable<T> GetServices<T>(string? contract) => Array.Empty<T>();

        public bool HasRegistration(Type? serviceType) => false;

        public bool HasRegistration(Type? serviceType, string? contract) => false;

        public bool HasRegistration<T>() => false;

        public bool HasRegistration<T>(string? contract) => false;

        public void Register(Func<object?> factory, Type? serviceType)
        {
        }

        public void Register(Func<object?> factory, Type? serviceType, string? contract)
        {
        }

        public void Register<T>(Func<T?> factory)
        {
        }

        public void Register<T>(Func<T?> factory, string? contract)
        {
        }

        public void Register<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService, new()
        {
        }

        public void Register<TService, TImplementation>(string? contract)
            where TService : class
            where TImplementation : class, TService, new()
        {
        }

        public void RegisterConstant<T>(T? value)
            where T : class
        {
        }

        public void RegisterConstant<T>(T? value, string? contract)
            where T : class
        {
        }

        public void RegisterLazySingleton<T>(Func<T?> valueFactory)
            where T : class
        {
        }

        public void RegisterLazySingleton<T>(Func<T?> valueFactory, string? contract)
            where T : class
        {
        }

        public void UnregisterCurrent(Type? serviceType)
        {
        }

        public void UnregisterCurrent(Type? serviceType, string? contract)
        {
        }

        public void UnregisterCurrent<T>()
        {
        }

        public void UnregisterCurrent<T>(string? contract)
        {
        }

        public void UnregisterAll(Type? serviceType)
        {
        }

        public void UnregisterAll(Type? serviceType, string? contract)
        {
        }

        public void UnregisterAll<T>()
        {
        }

        public void UnregisterAll<T>(string? contract)
        {
        }

        public IDisposable ServiceRegistrationCallback(Type serviceType, Action<IDisposable> callback) => Disposable.Empty;

        public IDisposable ServiceRegistrationCallback(Type serviceType, string? contract, Action<IDisposable> callback) => Disposable.Empty;

        public IDisposable ServiceRegistrationCallback<T>(Action<IDisposable> callback) => Disposable.Empty;

        public IDisposable ServiceRegistrationCallback<T>(string? contract, Action<IDisposable> callback) => Disposable.Empty;

        public void Dispose()
        {
        }
    }
}
