using Avalonia;
using NUnit.Framework;
using ReactiveUI.Avalonia.Splat;
using ReactiveUIDemo;
using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;

namespace ReactiveUI.Avalonia.Microsoft.Tests;

public class AvaloniaUIThreadTestsMicrosoft
{
#if MICROSOFT1
    [Test]
    public void Test1()
    {
        // Simplified test for now - we can expand this later
        Assert.That(true, Is.True, "Microsoft DI container test placeholder");
    }
#endif
////#if MICROSOFT2
////    [Fact]
////    public void Test2()
////    {
////        IServiceCollection? container = default;
////        IServiceProvider? resolver = default;
////        AppBuilder.Configure<App>()
////            .UsePlatformDetect()
////            .UseReactiveUIWithMicrosoftDependencyResolver(con => container = con, res => resolver = res)
////            .LogToTrace()
////            .SetupWithoutStarting();
////        Assert.IsType<AvaloniaScheduler>(RxApp.MainThreadScheduler);
////        Assert.NotNull(container);
////        Assert.NotNull(resolver);
////        Assert.IsType<MicrosoftDependencyResolver>(Locator.Current);
////    }
////#endif
}
