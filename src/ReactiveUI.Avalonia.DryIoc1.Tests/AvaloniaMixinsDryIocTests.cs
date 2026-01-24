using Avalonia;
using DryIoc;
using NUnit.Framework;
using ReactiveUI.Avalonia.Splat;
using Splat.DryIoc;

namespace ReactiveUI.Avalonia.DryIoc.Tests;

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
                c => new DryIocDependencyResolver(c),
                _ => { }));
    }

    [Test]
    public void UseReactiveUIWithDIContainer_ReturnsBuilder_NoThrow()
    {
        var builder = AppBuilder.Configure<Application>();
        var container = new Container();

        var result = builder.UseReactiveUIWithDIContainer(
            containerFactory: () => container,
            containerConfig: _ => { },
            dependencyResolverFactory: c => new DryIocDependencyResolver(c),
            _ => { });

        Assert.That(result, Is.SameAs(builder));
    }

    [Test]
    public void DryIocDependencyResolver_Register_And_Resolve_WithAndWithoutContract()
    {
        var container = new Container();
        var resolver = new DryIocDependencyResolver(container);

        resolver.Register<string>(() => "a");
        resolver.Register<string>(() => "b");
        resolver.Register<string>(() => "c", "x");

        var noContract = resolver.GetService(typeof(string));
        Assert.That(noContract, Is.EqualTo("b"));

        var withContract = resolver.GetService(typeof(string), "x");
        Assert.That(withContract, Is.EqualTo("c"));

        var all = resolver.GetServices(typeof(string)).ToArray();
        Assert.That(all, Does.Contain("a"));
        Assert.That(all, Does.Contain("b"));
    }
}
