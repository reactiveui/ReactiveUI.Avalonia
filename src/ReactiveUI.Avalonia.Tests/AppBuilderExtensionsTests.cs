using Avalonia;
using NUnit.Framework;

namespace ReactiveUI.Avalonia.Tests;

public class AppBuilderExtensionsTests
{
    [Test]
    public void UseReactiveUI_ThrowsOnNullBuilder()
    {
        // Test that UseReactiveUI throws ArgumentNullException for null builder
        AppBuilder? nullBuilder = null;
        Assert.Throws<ArgumentNullException>(() => nullBuilder!.UseReactiveUI());
    }

    [Test]
    public void AppBuilderExtensions_IsStaticClass()
    {
        // Test that AppBuilderExtensions is a static class
        var type = typeof(AppBuilderExtensions);
        Assert.That(type.IsClass, Is.True);
        Assert.That(type.IsAbstract, Is.True);
        Assert.That(type.IsSealed, Is.True);
    }
}
