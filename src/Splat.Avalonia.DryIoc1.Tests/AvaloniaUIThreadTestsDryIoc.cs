using Avalonia;
using Avalonia.ReactiveUI.Splat;
using DryIoc;
using NUnit.Framework;
using ReactiveUIDemo;
using Splat;
using Splat.DryIoc;

namespace ReactiveUI.Avalonia.DryIoc1.Tests
{
    public class AvaloniaUIThreadTestsDryIoc
    {
#if DRYIOC1
        [Test]
        public void Test1()
        {
            DryIocDependencyResolver? container = default;
            DryIocDependencyResolver? resolver = default;
            AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .UseReactiveUIWithDIContainer<DryIocDependencyResolver>(() => new(), con => container = con, res => resolver = res)
                .LogToTrace()
                .SetupWithoutStarting();
            Assert.That(RxApp.MainThreadScheduler, Is.TypeOf<AvaloniaScheduler>());
            Assert.That(container, Is.Not.Null);
            Assert.That(resolver, Is.Not.Null);
            Assert.That(Locator.Current, Is.TypeOf<DryIocDependencyResolver>());
        }
#endif
#if DRYIOC2
        [Test]
        public void Test2()
        {
            Container? container = default;
            AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .UseReactiveUIWithDryIoc(con => container = con)
                .LogToTrace()
                .SetupWithoutStarting();
            Assert.That(RxApp.MainThreadScheduler, Is.TypeOf<AvaloniaScheduler>());
            Assert.That(container, Is.Not.Null);
            Assert.That(Locator.Current, Is.TypeOf<DryIocDependencyResolver>());
        }
#endif
    }
}
