// Copyright (c) 2019-2026 ReactiveUI and Avalonia Teams, and Contributors. All rights reserved.
// Licensed under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Subjects;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace ReactiveUI.Avalonia.Tests;

/// <summary>
/// Tests for the AvaloniaCreatesCommandBinding command wiring behavior.
/// </summary>
public class AvaloniaCreatesCommandBindingTests
{
    /// <summary>
    /// Verifies that GetAffinityForObject returns expected values for various types.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetAffinityForObject_Returns_Expected()
    {
        var sut = new AvaloniaCreatesCommandBinding();

        await Assert.That(sut.GetAffinityForObject<object>(hasEventTarget: false)).IsEqualTo(0);
        await Assert.That(sut.GetAffinityForObject<InputElement>(hasEventTarget: false)).IsEqualTo(0);
        await Assert.That(sut.GetAffinityForObject<InputElement>(hasEventTarget: true)).IsGreaterThan(0);
        await Assert.That(sut.GetAffinityForObject<Button>(hasEventTarget: false)).IsEqualTo(10);
    }

    /// <summary>
    /// Verifies that BindCommandToObject wires a button's command and parameter correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindCommandToObject_Wires_Button_Command_And_Parameter()
    {
        var sut = new AvaloniaCreatesCommandBinding();
        var cmd = new TestCommand();
        var btn = new Button();
        var param = new BehaviorSubject<object?>("p1");

        using var disp = sut.BindCommandToObject(cmd, btn, param)!;

        await Assert.That(btn.CommandParameter).IsEqualTo("p1");

        param.OnNext("p2");
        await Assert.That(btn.CommandParameter).IsEqualTo("p2");

        await Assert.That(btn.Command).IsNotNull();

        disp.Dispose();
        await Assert.That(btn.Command).IsNull();
    }

    /// <summary>
    /// Verifies that BindCommandToObject with an event name binds to GotFocus on an InputElement.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindCommandToObject_With_EventName_Binds_InputElement_GotFocus()
    {
        var sut = new AvaloniaCreatesCommandBinding();
        var cmd = new TestCommand();
        var btn = new Button();
        var param = new BehaviorSubject<object?>("evt");

        using var disp = sut.BindCommandToObject<Button, RoutedEventArgs>(cmd, btn, param, nameof(InputElement.GotFocus));

        await Assert.That(btn.IsEnabled).IsTrue();

        cmd.SetCanExecute(false);
        param.OnNext("evt2");
        await Assert.That(btn.IsEnabled).IsFalse();

        cmd.SetCanExecute(true);
        param.OnNext("evt3");
        await Assert.That(btn.IsEnabled).IsTrue();

        btn.RaiseEvent(new RoutedEventArgs(InputElement.GotFocusEvent));
        await Assert.That(cmd.ExecutedCount).IsEqualTo(1);
        await Assert.That(cmd.LastParameter).IsEqualTo("evt3");

        disp?.Dispose();
        await Assert.That(btn.IsSet(InputElement.IsEnabledProperty)).IsFalse();
    }

    /// <summary>
    /// Verifies that BindCommandToObject throws on invalid targets or null arguments.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindCommandToObject_Throws_On_Invalid_Targets_Or_Nulls()
    {
        var sut = new AvaloniaCreatesCommandBinding();
        var cmd = new TestCommand();
        var param = new BehaviorSubject<object?>(null);

        await Assert.That((Action)(() => sut.BindCommandToObject(null!, new object(), param))).ThrowsExactly<ArgumentNullException>();
        await Assert.That((Action)(() => sut.BindCommandToObject<object>(cmd, null!, param))).ThrowsExactly<ArgumentNullException>();
        await Assert.That((Action)(() => sut.BindCommandToObject(cmd, new object(), param))).ThrowsExactly<InvalidOperationException>();

        await Assert.That((Action)(() => sut.BindCommandToObject<object, RoutedEventArgs>(null!, new object(), param, "Click"))).ThrowsExactly<ArgumentNullException>();
        await Assert.That((Action)(() => sut.BindCommandToObject<object, RoutedEventArgs>(cmd, null!, param, "Click"))).ThrowsExactly<ArgumentNullException>();
        await Assert.That((Action)(() => sut.BindCommandToObject<object, RoutedEventArgs>(cmd, new object(), param, "Click"))).ThrowsExactly<InvalidOperationException>();
        await Assert.That((Action)(() => sut.BindCommandToObject<object, RoutedEventArgs>(cmd, new Button(), param, "MissingEvent"))).ThrowsExactly<InvalidOperationException>();
    }

    /// <summary>
    /// A test command implementation for verifying command binding.
    /// </summary>
    private sealed class TestCommand : System.Windows.Input.ICommand
    {
        /// <summary>
        /// Whether the command can currently execute.
        /// </summary>
        private bool _canExecute = true;

        /// <inheritdoc/>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Gets the number of times Execute has been called.
        /// </summary>
        public int ExecutedCount { get; private set; }

        /// <summary>
        /// Gets the last parameter passed to Execute.
        /// </summary>
        public object? LastParameter { get; private set; }

        /// <summary>
        /// Sets whether the command can execute and raises CanExecuteChanged.
        /// </summary>
        /// <param name="can">Whether the command can execute.</param>
        public void SetCanExecute(bool can)
        {
            _canExecute = can;
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc/>
        public bool CanExecute(object? parameter) => _canExecute;

        /// <inheritdoc/>
        public void Execute(object? parameter)
        {
            ExecutedCount++;
            LastParameter = parameter;
        }
    }
}
