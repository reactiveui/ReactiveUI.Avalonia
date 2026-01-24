using System.Reactive.Subjects;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using NUnit.Framework;

namespace ReactiveUI.Avalonia.Tests;

public class AvaloniaCreatesCommandBindingTests
{
    [Test]
    public void GetAffinityForObject_Returns_Expected()
    {
        var sut = new AvaloniaCreatesCommandBinding();

        Assert.That(sut.GetAffinityForObject<object>(hasEventTarget: false), Is.EqualTo(0));
        Assert.That(sut.GetAffinityForObject<InputElement>(hasEventTarget: false), Is.EqualTo(0));
        Assert.That(sut.GetAffinityForObject<InputElement>(hasEventTarget: true), Is.GreaterThan(0));
        Assert.That(sut.GetAffinityForObject<Button>(hasEventTarget: false), Is.EqualTo(10));
    }

    [Test]
    public void BindCommandToObject_Wires_Button_Command_And_Parameter()
    {
        var sut = new AvaloniaCreatesCommandBinding();
        var cmd = new TestCommand();
        var btn = new Button();
        var param = new BehaviorSubject<object?>("p1");

        using var disp = sut.BindCommandToObject(cmd, btn, param)!;

        Assert.That(btn.CommandParameter, Is.EqualTo("p1"));

        param.OnNext("p2");
        Assert.That(btn.CommandParameter, Is.EqualTo("p2"));

        // Sanity check that binding exists
        Assert.That(btn.Command, Is.Not.Null);

        disp.Dispose();
        Assert.That(btn.Command, Is.Null);
    }

    [Test]
    public void BindCommandToObject_With_EventName_Binds_InputElement_GotFocus()
    {
        var sut = new AvaloniaCreatesCommandBinding();
        var cmd = new TestCommand();
        var btn = new Button();
        var param = new BehaviorSubject<object?>("evt");

        using var disp = sut.BindCommandToObject<Button, RoutedEventArgs>(cmd, btn, param, nameof(InputElement.GotFocus));

        Assert.That(btn.IsEnabled, Is.True);

        cmd.SetCanExecute(false);
        param.OnNext("evt2");
        Assert.That(btn.IsEnabled, Is.False);

        cmd.SetCanExecute(true);
        param.OnNext("evt3");
        Assert.That(btn.IsEnabled, Is.True);

        btn.RaiseEvent(new RoutedEventArgs(InputElement.GotFocusEvent));
        Assert.That(cmd.ExecutedCount, Is.EqualTo(1));
        Assert.That(cmd.LastParameter, Is.EqualTo("evt3"));

        disp?.Dispose();
        Assert.That(btn.IsSet(InputElement.IsEnabledProperty), Is.False);
    }

    [Test]
    public void BindCommandToObject_Throws_On_Invalid_Targets_Or_Nulls()
    {
        var sut = new AvaloniaCreatesCommandBinding();
        var cmd = new TestCommand();
        var param = new BehaviorSubject<object?>(null);

        Assert.Throws<ArgumentNullException>(() => sut.BindCommandToObject(null!, new object(), param));
        Assert.Throws<ArgumentNullException>(() => sut.BindCommandToObject<object>(cmd, null!, param));
        Assert.Throws<InvalidOperationException>(() => sut.BindCommandToObject(cmd, new object(), param));

        Assert.Throws<ArgumentNullException>(() => sut.BindCommandToObject<object, RoutedEventArgs>(null!, new object(), param, "Click"));
        Assert.Throws<ArgumentNullException>(() => sut.BindCommandToObject<object, RoutedEventArgs>(cmd, null!, param, "Click"));
        Assert.Throws<InvalidOperationException>(() => sut.BindCommandToObject<object, RoutedEventArgs>(cmd, new object(), param, "Click"));
        Assert.Throws<InvalidOperationException>(() => sut.BindCommandToObject<object, RoutedEventArgs>(cmd, new Button(), param, "MissingEvent"));
    }

    private sealed class TestCommand : System.Windows.Input.ICommand
    {
        private bool _canExecute = true;

        public event EventHandler? CanExecuteChanged;

        public int ExecutedCount { get; private set; }

        public object? LastParameter { get; private set; }

        public void SetCanExecute(bool can)
        {
            _canExecute = can;
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool CanExecute(object? parameter) => _canExecute;

        public void Execute(object? parameter)
        {
            ExecutedCount++;
            LastParameter = parameter;
        }
    }
}
