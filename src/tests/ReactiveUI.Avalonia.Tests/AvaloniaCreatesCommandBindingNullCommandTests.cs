// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Avalonia.Controls;

namespace ReactiveUI.Avalonia.Tests;

/// <summary>Regression tests for null command updates in Avalonia command bindings.</summary>
public class AvaloniaCreatesCommandBindingNullCommandTests
{
    /// <summary>Verifies that a null command removes a previous command and remains removed when its earlier binding is disposed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindCommandToObject_NullCommand_ClearsPreviousCommandAndSurvivesPriorBindingDisposal()
    {
        var sut = new AvaloniaCreatesCommandBinding();
        var button = new Button();
        var parameter = new Signal<object?>();
        var command = new TestCommand();

        using var emptyBinding = sut.BindCommandToObject(null, button, parameter);
        await Assert.That(emptyBinding).IsNotNull();
        await Assert.That(button.Command).IsNull();

        var binding = sut.BindCommandToObject(command, button, parameter)!;
        await Assert.That(button.Command).IsSameReferenceAs(command);

        using var clearedBinding = sut.BindCommandToObject(null, button, parameter);
        await Assert.That(clearedBinding).IsNotNull();
        await Assert.That(button.Command).IsNull();

        binding.Dispose();
        await Assert.That(button.Command).IsNull();
    }

    /// <summary>Verifies that disposing a binding preserves a command assigned after that binding was created.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindCommandToObject_Disposal_PreservesReplacementCommand()
    {
        var sut = new AvaloniaCreatesCommandBinding();
        var button = new Button();
        var parameter = new Signal<object?>();
        var initialCommand = new TestCommand();
        var replacementCommand = new TestCommand();

        var binding = sut.BindCommandToObject(initialCommand, button, parameter)!;
        button.Command = replacementCommand;

        binding.Dispose();

        await Assert.That(button.Command).IsSameReferenceAs(replacementCommand);
    }

    /// <summary>Verifies the public BindCommand pipeline updates a button when a nullable view-model command changes.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task BindCommand_NullableViewModelCommand_TransitionsFromNullToCommandAndBack()
    {
        var viewModel = new CommandViewModel();
        var view = new CommandView { ViewModel = viewModel };
        var command = new TestCommand();

        using (view.BindCommand(viewModel, static x => x.Command, static x => x.Button))
        {
            await Assert.That(view.Button.Command).IsNull();

            viewModel.Command = command;
            await Assert.That(view.Button.Command).IsSameReferenceAs(command);

            view.Button.Command!.Execute(null);
            await Assert.That(command.ExecutionCount).IsEqualTo(1);

            viewModel.Command = null;
            await Assert.That(view.Button.Command).IsNull();
        }
    }

    /// <summary>A view model with the nullable command property used by the public binding regression.</summary>
    private sealed class CommandViewModel : ReactiveObject
    {
        /// <summary>Gets or sets the command bound to the test button.</summary>
        public System.Windows.Input.ICommand? Command
        {
            get;
            set => this.RaiseAndSetIfChanged(ref field, value);
        }
    }

    /// <summary>A view exposing the test button to the public command-binding extension.</summary>
    private sealed class CommandView : ReactiveUserControl<CommandViewModel>
    {
        /// <summary>Gets the button bound by the test.</summary>
        public Button Button { get; } = new();
    }

    /// <summary>A minimal command implementation for command property assertions.</summary>
    private sealed class TestCommand : System.Windows.Input.ICommand
    {
        /// <inheritdoc/>
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        /// <summary>Gets the number of times the command has executed.</summary>
        public int ExecutionCount { get; private set; }

        /// <inheritdoc/>
        public bool CanExecute(object? parameter) => true;

        /// <inheritdoc/>
        public void Execute(object? parameter) => ExecutionCount++;
    }
}
