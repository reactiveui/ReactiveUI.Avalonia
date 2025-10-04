using Avalonia;
using Avalonia.ReactiveUI.Splat;
using DryIoc;
using NUnit.Framework;
using ReactiveUI.Avalonia;
using Splat.DryIoc;

namespace ReactiveUI.Avalonia.DryIoc1.Tests
{
    public class AvaloniaMixinsDryIocTests
    {
        [Test]
        public void UseReactiveUIWithDryIoc_ThrowsOnNullBuilder()
        {
            AppBuilder? builder = null;
            Assert.Throws<ArgumentNullException>(() =>
                AvaloniaMixins.UseReactiveUIWithDryIoc(builder!, _ => { }));
        }

        [Test]
        public void UseReactiveUIWithDryIoc_ReturnsBuilder_NoThrow()
        {
            var builder = AppBuilder.Configure<Application>();
            var result = builder.UseReactiveUIWithDryIoc(_ => { });
            Assert.That(result, Is.SameAs(builder));
        }

        [Test]
        public void UseReactiveUIWithDIContainer_ThrowsOnNullBuilder()
        {
            AppBuilder? builder = null;
            Assert.Throws<ArgumentNullException>(() =>
                AppBuilderExtensions.UseReactiveUIWithDIContainer(
                    builder!,
                    () => new Container(),
                    _ => { },
                    c => new DryIocDependencyResolver(c)));
        }

        [Test]
        public void UseReactiveUIWithDIContainer_ReturnsBuilder_NoThrow()
        {
            var builder = AppBuilder.Configure<Application>();
            var container = new Container();

            var result = builder.UseReactiveUIWithDIContainer(
                containerFactory: () => container,
                containerConfig: _ => { },
                dependencyResolverFactory: c => new DryIocDependencyResolver(c));

            Assert.That(result, Is.SameAs(builder));
        }

        [Test]
        public void DryIocDependencyResolver_Register_And_Resolve_WithAndWithoutContract()
        {
            var container = new Container();
            var resolver = new DryIocDependencyResolver(container);

            resolver.Register(() => "a", typeof(string));
            resolver.Register(() => "b", typeof(string), "x");

            var last = resolver.GetService(typeof(string));
            Assert.That(last, Is.EqualTo("b"));

            var all = resolver.GetServices(typeof(string)).ToArray();
            Assert.That(all, Does.Contain("a"));
            Assert.That(all, Does.Contain("b"));
        }
    }
}
