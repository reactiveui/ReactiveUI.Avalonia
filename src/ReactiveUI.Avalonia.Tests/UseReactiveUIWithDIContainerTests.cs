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
            dependencyResolverFactory: _ => new DummyResolver()));
    }

    [Test]
    public void UseReactiveUIWithDIContainer_Returns_Builder_NoThrow()
    {
        var builder = AppBuilder.Configure<Application>();

        var result = builder.UseReactiveUIWithDIContainer(
            containerFactory: () => new object(),
            containerConfig: _ => { },
            dependencyResolverFactory: _ => new DummyResolver());

        Assert.That(result, Is.SameAs(builder));
    }

    private sealed class DummyResolver : IDependencyResolver
    {
        public void Dispose()
        {
        }

        public object? GetService(Type? serviceType, string? contract = null)
        {
            return null;
        }

        public IEnumerable<object> GetServices(Type? serviceType, string? contract = null)
        {
            return Array.Empty<object>();
        }

        public bool HasRegistration(Type? serviceType, string? contract = null)
        {
            return false;
        }

        public void Register(Func<object?> factory, Type? serviceType, string? contract = null)
        {
        }

        public IDisposable ServiceRegistrationCallback(Type serviceType, string? contract, Action<IDisposable> callback)
        {
            return System.Reactive.Disposables.Disposable.Empty;
        }

        public void UnregisterAll(Type? serviceType, string? contract = null)
        {
        }

        public void UnregisterCurrent(Type? serviceType, string? contract = null)
        {
        }
    }
}
