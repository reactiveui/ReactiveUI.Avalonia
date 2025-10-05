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
            dependencyResolverFactory: _ => new DummyResolver());
        Assert.That(result, Is.SameAs(builder));
    }

    private sealed class DummyResolver : IDependencyResolver
    {
        public object? GetService(Type? serviceType, string? contract = null) => null;

        public IEnumerable<object> GetServices(Type? serviceType, string? contract = null) => [];

        public void Register(Func<object?> factory, Type? serviceType, string? contract = null)
        {
        }

        public void UnregisterCurrent(Type? serviceType, string? contract = null)
        {
        }

        public void UnregisterAll(Type? serviceType, string? contract = null)
        {
        }

        public IDisposable ServiceRegistrationCallback(Type serviceType, string? contract, Action<IDisposable> callback) => Disposable.Empty;

        public bool HasRegistration(Type? serviceType, string? contract = null) => false;

        public void Dispose()
        {
        }
    }
}
