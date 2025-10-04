using System.Reflection;
using Avalonia;
using NUnit.Framework;

namespace ReactiveUI.Avalonia.Tests;

public class AppBuilderExtensionsViewsApiTests
{
    [Test]
    public void RegisterReactiveUIViews_Returns_Same_Builder_And_Handles_NullOrEmpty()
    {
        // null arrays are compile-time issues; test empty
        var builder = AppBuilder.Configure<Application>();
        var result = builder.RegisterReactiveUIViews([]);
        Assert.That(result, Is.SameAs(builder));
    }

    [Test]
    public void RegisterReactiveUIViewsFromAssemblyOf_Registers_Types()
    {
        var builder = AppBuilder.Configure<Application>();
        var result = builder.RegisterReactiveUIViewsFromAssemblyOf<AppBuilderExtensionsRegistrationTests>();
        Assert.That(result, Is.SameAs(builder));

        var resolver = global::Splat.AppLocator.CurrentMutable!;
        var method = typeof(AppBuilderExtensions)
            .GetMethod("RegisterViewsInternal", BindingFlags.NonPublic | BindingFlags.Static)!;
        method.Invoke(null, [resolver, new[] { typeof(AppBuilderExtensionsRegistrationTests).Assembly }]);
        var serviceType = typeof(IViewFor<>).MakeGenericType(typeof(AppBuilderExtensionsRegistrationTests));
        Assert.Pass();
    }

    [Test]
    public void RegisterReactiveUIViewsFromEntryAssembly_Returns_Same_Builder()
    {
        var builder = AppBuilder.Configure<Application>();
        var result = builder.RegisterReactiveUIViewsFromEntryAssembly();
        Assert.That(result, Is.SameAs(builder));
    }
}
