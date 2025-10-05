using Avalonia;
using DryIoc;
using NUnit.Framework;
using ReactiveUI.Avalonia.Splat;
using Splat.DryIoc;

namespace ReactiveUI.Avalonia.DryIoc.Tests;

public class AvaloniaMixinsDryIocMoreTests
{
    [Test]
    public void UseReactiveUIWithDryIoc_WithBuilderOverload_Returns_Builder()
    {
        var builder = AppBuilder.Configure<Application>();
        var result = AvaloniaMixins.UseReactiveUIWithDryIoc(
            builder,
            containerConfig: c =>
            {
                c.RegisterInstance(new object());
            },
            withReactiveUIBuilder: _ => { });

        Assert.That(result, Is.SameAs(builder));
    }

    [Test]
    public void UseReactiveUIWithDIContainer_Generic_Returns_Builder()
    {
        var builder = AppBuilder.Configure<Application>();
        var result = AppBuilderExtensions.UseReactiveUIWithDIContainer(
            builder,
            containerFactory: () => new Container(),
            containerConfig: _ => { },
            dependencyResolverFactory: c => new DryIocDependencyResolver(c));

        Assert.That(result, Is.SameAs(builder));
    }
}
