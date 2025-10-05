#pragma warning disable SA1201
using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using ReactiveUI.Avalonia.Splat;

namespace ReactiveUI.Avalonia.Microsoft.Tests;

public class AvaloniaMixinsMicrosoftMoreTests
{
    [Test]
    public void UseReactiveUIWithMicrosoftDependencyResolver_Overload_Returns_Builder()
    {
        var builder = AppBuilder.Configure<Application>();
        var result = AvaloniaMixins.UseReactiveUIWithMicrosoftDependencyResolver(
            builder,
            containerConfig: sc =>
            {
                sc.AddSingleton(new object());
            },
            withResolver: _ => { },
            withReactiveUIBuilder: _ => { });

        Assert.That(result, Is.SameAs(builder));
    }

    [Test]
    public void UseReactiveUIWithMicrosoftDependencyResolver_Returns_Builder_NoThrow()
    {
        var builder = AppBuilder.Configure<Application>();
        var result = AvaloniaMixins.UseReactiveUIWithMicrosoftDependencyResolver(
            builder,
            sc => { sc.AddSingleton(new object()); },
            _ => { });

        Assert.That(result, Is.SameAs(builder));
    }
}
