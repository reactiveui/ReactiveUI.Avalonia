extern alias ninject;

using Avalonia;
using NUnit.Framework;
using AvaloniaMixins = ninject::ReactiveUI.Avalonia.Splat.AvaloniaMixins;

namespace ReactiveUI.Avalonia.Tests;

public class AvaloniaMixinsNinjectTests
{
    [Test]
    public void UseReactiveUIWithNinject_Extension_ThrowsOnNullBuilder()
    {
        AppBuilder? builder = null;
        Assert.Throws<ArgumentNullException>(() =>
            AvaloniaMixins.UseReactiveUIWithNinject(builder!, _ => { }, null));
    }

    [Test]
    public void UseReactiveUIWithNinject_Overload_ThrowsOnNullBuilder()
    {
        AppBuilder? builder = null;
        Assert.Throws<ArgumentNullException>(() =>
            AvaloniaMixins.UseReactiveUIWithNinject(builder!, _ => { }, null));
    }

    [Test]
    public void UseReactiveUIWithNinject_ReturnsBuilder_NoThrow()
    {
        var builder = AppBuilder.Configure<Application>();
        var result = AvaloniaMixins.UseReactiveUIWithNinject(builder, _ => { }, null);
        Assert.That(result, Is.SameAs(builder));
    }

    [Test]
    public void UseReactiveUIWithNinject_Overload_ReturnsBuilder_NoThrow()
    {
        var builder = AppBuilder.Configure<Application>();
        var result = AvaloniaMixins.UseReactiveUIWithNinject(
            builder,
            _ => { },
            rx => { Assert.That(rx, Is.Not.Null); });
        Assert.That(result, Is.SameAs(builder));
    }
}
