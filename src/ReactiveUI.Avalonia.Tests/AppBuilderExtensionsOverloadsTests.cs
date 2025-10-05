using Avalonia;
using NUnit.Framework;

namespace ReactiveUI.Avalonia.Tests;

public class AppBuilderExtensionsOverloadsTests
{
    [Test]
    public void UseReactiveUI_WithBuilderOverload_ThrowsOnNulls()
    {
        AppBuilder? builder = null;
        Assert.Throws<ArgumentNullException>(() => builder!.UseReactiveUI(_ => { }));

        var b = AppBuilder.Configure<Application>();
        Assert.Throws<ArgumentNullException>(() => b.UseReactiveUI(null!));
    }

    [Test]
    public void RegisterReactiveUIViews_ThrowsOnNullBuilder()
    {
        AppBuilder? builder = null;
        Assert.Throws<ArgumentNullException>(() => builder!.RegisterReactiveUIViews());
    }

    [Test]
    public void RegisterReactiveUIViews_ReturnsBuilder_NoAssemblies_NoThrow()
    {
        var builder = AppBuilder.Configure<Application>();
        var result = builder.RegisterReactiveUIViews();
        Assert.That(result, Is.SameAs(builder));
    }
}
