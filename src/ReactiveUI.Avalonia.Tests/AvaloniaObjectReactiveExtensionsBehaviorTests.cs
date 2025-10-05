#pragma warning disable SA1201
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using NUnit.Framework;

namespace ReactiveUI.Avalonia.Tests;

public class AvaloniaObjectReactiveExtensionsBehaviorTests
{
    private class TestControl : Control
    {
        public static readonly StyledProperty<int> IntPropProperty =
            AvaloniaProperty.Register<TestControl, int>(nameof(IntProp));

        public int IntProp
        {
            get => GetValue(IntPropProperty);
            set => SetValue(IntPropProperty, value);
        }
    }

    [Test]
    public void GetSubject_ObjectRoundtrip_WritesAndObserves()
    {
        var ctrl = new TestControl();
        var subject = ctrl.GetSubject((AvaloniaProperty)TestControl.IntPropProperty);
        object? observed = null;
        using var sub = subject.Subscribe(v => observed = v);

        subject.OnNext(42);

        Assert.That(ctrl.IntProp, Is.EqualTo(42));
        Assert.That(observed, Is.EqualTo(42));
    }

    [Test]
    public void GetSubject_GenericRoundtrip_WritesAndObserves()
    {
        var ctrl = new TestControl();
        var subject = ctrl.GetSubject(TestControl.IntPropProperty);
        int? observed = null;
        using var sub = subject.Subscribe(v => observed = v);

        subject.OnNext(7);

        Assert.That(ctrl.IntProp, Is.EqualTo(7));
        Assert.That(observed, Is.EqualTo(7));
    }

    [Test]
    public void GetBindingSubject_ObjectRoundtrip_WritesOnlyOnHasValue()
    {
        var ctrl = new TestControl();
        var subject = ctrl.GetBindingSubject((AvaloniaProperty)TestControl.IntPropProperty);
        BindingValue<object?>? observed = null;
        using var sub = subject.Subscribe(v => observed = v);

        subject.OnNext(BindingValue<object?>.DoNothing);
        Assert.That(ctrl.IntProp, Is.EqualTo(default(int)));

        subject.OnNext(new BindingValue<object?>(99));
        Assert.That(ctrl.IntProp, Is.EqualTo(99));
        Assert.That(observed!.HasValue, Is.True);
    }

    [Test]
    public void GetBindingSubject_GenericRoundtrip_WritesOnlyOnHasValue()
    {
        var ctrl = new TestControl();
        var subject = ctrl.GetBindingSubject(TestControl.IntPropProperty);
        BindingValue<int>? observed = null;
        using var sub = subject.Subscribe(v => observed = v);

        subject.OnNext(BindingValue<int>.DoNothing);
        Assert.That(ctrl.IntProp, Is.EqualTo(default(int)));

        subject.OnNext(new BindingValue<int>(5));
        Assert.That(ctrl.IntProp, Is.EqualTo(5));
        Assert.That(observed!.HasValue, Is.True);
    }
}
