#pragma warning disable SA1201
using System.Reactive.Disposables;
using Avalonia;
using NUnit.Framework;
using ReactiveUI.Avalonia.Splat;
using Splat;

namespace ReactiveUI.Avalonia.Microsoft.Tests
{
    public class AvaloniaMixinsMicrosoftTests
    {
        [Test]
        public void UseReactiveUIWithMicrosoftDependencyResolver_ThrowsOnNullBuilder()
        {
            AppBuilder? builder = null;
            Assert.Throws<ArgumentNullException>(() =>
                AvaloniaMixins.UseReactiveUIWithMicrosoftDependencyResolver(
                    builder!,
                    _ => { }));
        }

        [Test]
        public void UseReactiveUIWithDIContainer_DoesNotThrow_WithValidArgs()
        {
            var builder = AppBuilder.Configure<Application>();
            var result = AvaloniaMixins.UseReactiveUIWithDIContainer(
                builder,
                containerFactory: () => new object(),
                containerConfig: _ => { },
                dependencyResolverFactory: _ => new DummyResolver());
            Assert.That(result, Is.SameAs(builder));
        }

        private sealed class DummyResolver : IDependencyResolver
        {
            public object? GetService(Type? serviceType, string? contract = null)
            {
                return null;
            }

            public IEnumerable<object> GetServices(Type? serviceType, string? contract = null)
            {
                return Array.Empty<object>();
            }

            public void Register(Func<object?> factory, Type? serviceType, string? contract = null)
            {
            }

            public void UnregisterCurrent(Type? serviceType, string? contract = null)
            {
            }

            public void UnregisterAll(Type? serviceType, string? contract = null)
            {
            }

            public IDisposable ServiceRegistrationCallback(Type serviceType, string? contract, Action<IDisposable> callback)
            {
                return Disposable.Empty;
            }

            public bool HasRegistration(Type? serviceType, string? contract = null)
            {
                return false;
            }

            public void Dispose()
            {
            }
        }
    }
}
