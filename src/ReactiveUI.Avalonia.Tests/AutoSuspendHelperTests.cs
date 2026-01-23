using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Avalonia.Controls.ApplicationLifetimes;
using NUnit.Framework;

namespace ReactiveUI.Avalonia.Tests;

public class AutoSuspendHelperTests
{
    [SetUp]
    public void Setup()
    {
        RxSuspension.SuspensionHost.IsResuming = Observable.Never<Unit>();
        RxSuspension.SuspensionHost.IsLaunchingNew = new Subject<Unit>();
        RxSuspension.SuspensionHost.ShouldPersistState = Observable.Never<IDisposable>();
        RxSuspension.SuspensionHost.ShouldInvalidateState = Observable.Never<Unit>();
    }

    [Test]
    public void Ctor_With_Null_Lifetime_Throws() => Assert.Throws<ArgumentNullException>(() => new AutoSuspendHelper(null!));

    [Test]
    public void Ctor_With_DesktopLifetime_Sets_ShouldPersistState()
    {
        var lifetime = new ClassicDesktopStyleApplicationLifetime();
        using var helper = new AutoSuspendHelper(lifetime);

        var notified = false;
        var sub = RxSuspension.SuspensionHost.ShouldPersistState.Subscribe(d =>
        {
            notified = true;
            d.Dispose();
        });

        lifetime.Shutdown(0);

        Assert.That(notified, Is.True);
        sub.Dispose();
    }

    [Test]
    public void OnFrameworkInitializationCompleted_Pushes_IsLaunchingNew()
    {
        var lifetime = new ClassicDesktopStyleApplicationLifetime();
        using var helper = new AutoSuspendHelper(lifetime);

        var count = 0;
        var sub = RxSuspension.SuspensionHost.IsLaunchingNew.Subscribe(_ => count++);

        helper.OnFrameworkInitializationCompleted();

        Assert.That(count, Is.EqualTo(1));
        sub.Dispose();
    }
}
