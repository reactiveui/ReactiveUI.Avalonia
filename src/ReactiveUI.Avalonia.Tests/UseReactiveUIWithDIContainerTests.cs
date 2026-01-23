using Avalonia;
using NUnit.Framework;
using Splat;

namespace ReactiveUI.Avalonia.Tests;

public class UseReactiveUIWithDIContainerTests
{
    [Test]
    public void UseReactiveUIWithDIContainer_Throws_On_Null_Builder()
    {
        AppBuilder? builder = null;
        Assert.Throws<ArgumentNullException>(() => builder!.UseReactiveUIWithDIContainer(
            containerFactory: () => new object(),
            containerConfig: _ => { },
            dependencyResolverFactory: _ => new DummyResolver(),
            _ => { }));
    }

    [Test]
    public void UseReactiveUIWithDIContainer_Returns_Builder_NoThrow()
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
        public void Dispose()
        {
        }

        public object? GetService(Type? serviceType)
        {
            return null;
        }

        public object? GetService(Type? serviceType, string? contract)
        {
            return null;
        }

        public T? GetService<T>()
        {
            return default;
        }

        public T? GetService<T>(string? contract)
        {
            return default;
        }

        public IEnumerable<object> GetServices(Type? serviceType)
        {
            return Array.Empty<object>();
        }

        public IEnumerable<object> GetServices(Type? serviceType, string? contract)
        {
            return Array.Empty<object>();
        }

        public IEnumerable<T> GetServices<T>()
        {
            return Array.Empty<T>();
        }

        public IEnumerable<T> GetServices<T>(string? contract)
        {
            return Array.Empty<T>();
        }

        public bool HasRegistration(Type? serviceType)
        {
            return false;
        }

        public bool HasRegistration(Type? serviceType, string? contract)
        {
            return false;
        }

        public bool HasRegistration<T>()
        {
            return false;
        }

        public bool HasRegistration<T>(string? contract)
        {
            return false;
        }

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

        public IDisposable ServiceRegistrationCallback(Type serviceType, Action<IDisposable> callback)
        {
            return System.Reactive.Disposables.Disposable.Empty;
        }

        public IDisposable ServiceRegistrationCallback(Type serviceType, string? contract, Action<IDisposable> callback)
        {
            return System.Reactive.Disposables.Disposable.Empty;
        }

        public IDisposable ServiceRegistrationCallback<T>(Action<IDisposable> callback)
        {
            return System.Reactive.Disposables.Disposable.Empty;
        }

        public IDisposable ServiceRegistrationCallback<T>(string? contract, Action<IDisposable> callback)
        {
            return System.Reactive.Disposables.Disposable.Empty;
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
    }
}
