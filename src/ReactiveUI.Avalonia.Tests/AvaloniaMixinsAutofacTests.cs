extern alias autofac;

using Avalonia;
using NUnit.Framework;
using Splat.Autofac;
using AvaloniaMixins = autofac::ReactiveUI.Avalonia.Splat.AvaloniaMixins;

namespace ReactiveUI.Avalonia.Tests;

public class AvaloniaMixinsAutofacTests
{
    [Test]
    public void UseReactiveUIWithAutofac_Extension_ThrowsOnNullBuilder()
    {
        AppBuilder? builder = null;
        Assert.Throws<ArgumentNullException>(() =>
            AvaloniaMixins.UseReactiveUIWithAutofac(builder!, _ => { }, null, null));
    }

    [Test]
    public void UseReactiveUIWithAutofac_Overload_ThrowsOnNullBuilder()
    {
        AppBuilder? builder = null;
        Assert.Throws<ArgumentNullException>(() =>
            AvaloniaMixins.UseReactiveUIWithAutofac(builder!, _ => { }, null, null));
    }

    [Test]
    public void UseReactiveUIWithAutofac_ReturnsBuilder_NoThrow()
    {
        var builder = AppBuilder.Configure<Application>();
        var result = AvaloniaMixins.UseReactiveUIWithAutofac(builder, _ => { }, null, null);
        Assert.That(result, Is.SameAs(builder));
    }

    [Test]
    public void UseReactiveUIWithAutofac_Overload_ReturnsBuilder_NoThrow()
    {
        var builder = AppBuilder.Configure<Application>();
        var result = AvaloniaMixins.UseReactiveUIWithAutofac(
            builder,
            _ => { },
            r => { Assert.That(r, Is.InstanceOf<AutofacDependencyResolver>()); },
            rx => { Assert.That(rx, Is.Not.Null); });

        Assert.That(result, Is.SameAs(builder));
    }
}
