// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using ReactiveUI;
using ReactiveUI.Primitives;
using Unit = ReactiveUI.Primitives.RxVoid;

namespace ReactiveUI.Avalonia.Example.ViewModels;

/// <summary>Demonstrates command can-execute, async execution, failure handling, and interactions.</summary>
public sealed class CommandLabViewModel : PageViewModel
{
    /// <summary>The simulated command work delay.</summary>
    private static readonly TimeSpan WorkDelay = TimeSpan.FromMilliseconds(250);

    /// <summary>The simulated failure delay.</summary>
    private static readonly TimeSpan FailureDelay = TimeSpan.FromMilliseconds(100);

    /// <summary>The time provider.</summary>
    private readonly TimeProvider _timeProvider;

    /// <summary>Initializes a new instance of the <see cref="CommandLabViewModel"/> class.</summary>
    /// <param name="hostScreen">The owning screen.</param>
    [RequiresUnreferencedCode("This showcase uses WhenAnyValue to demonstrate command can-execute.")]
    public CommandLabViewModel(IScreen hostScreen)
        : this(hostScreen, TimeProvider.System)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="CommandLabViewModel"/> class.</summary>
    /// <param name="hostScreen">The owning screen.</param>
    /// <param name="timeProvider">The time provider.</param>
    [RequiresUnreferencedCode("This showcase uses WhenAnyValue to demonstrate command can-execute.")]
    public CommandLabViewModel(IScreen hostScreen, TimeProvider timeProvider)
        : base(hostScreen, "commands", "Command lab", "Exercises ReactiveCommand can-execute, async work, thrown exceptions, and interactions.")
    {
        _timeProvider = timeProvider;
        var canRun = this.WhenAnyValue(static x => x.WorkItemText, static text => !string.IsNullOrWhiteSpace(text));
        RunWork = Track(ReactiveCommand.CreateFromTask(RunWorkAsync, canRun));
        FailWork = Track(ReactiveCommand.CreateFromTask(FailWorkAsync));

        _ = Track(RunWork.SubscribeSafe(result => LastResult = result, HandleError));
        _ = Track(RunWork.ThrownExceptions.SubscribeSafe(HandleError, HandleError));
        _ = Track(FailWork.ThrownExceptions.SubscribeSafe(HandleError, HandleError));
    }

    /// <summary>Gets the command that runs async work.</summary>
    public ReactiveCommand<Unit, string> RunWork { get; }

    /// <summary>Gets the command that intentionally fails.</summary>
    public ReactiveCommand<Unit, Unit> FailWork { get; }

    /// <summary>Gets the interaction used by the view to present command errors.</summary>
    public Interaction<string, Unit> ReportError { get; } = new();

    /// <summary>Gets or sets the command text input.</summary>
    public string WorkItemText
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = string.Empty;

    /// <summary>Gets or sets the last result text.</summary>
    public string LastResult
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = "No command has run yet.";

    /// <summary>Gets or sets the last error text.</summary>
    public string LastError
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = "No errors reported.";

    /// <summary>Gets or sets the interaction acknowledgement text.</summary>
    public string InteractionStatus
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = "The view has not handled an interaction yet.";

    /// <summary>Gets or sets the number of errors acknowledged by the view.</summary>
    public int AcknowledgedErrors
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>Records that the view handled an error interaction.</summary>
    /// <param name="message">The handled message.</param>
    public void AcknowledgeError(string message)
    {
        AcknowledgedErrors++;
        InteractionStatus = $"View acknowledged: {message}";
    }

    /// <summary>Intentionally fails to demonstrate command error flow.</summary>
    /// <returns>A task representing the failing work.</returns>
    private static async Task FailWorkAsync()
    {
        await Task.Delay(FailureDelay).ConfigureAwait(false);
        throw new InvalidOperationException("The sample command failed by design.");
    }

    /// <summary>Runs the simulated async command work.</summary>
    /// <returns>The command result text.</returns>
    private async Task<string> RunWorkAsync()
    {
        var text = WorkItemText.Trim();
        await Task.Delay(WorkDelay).ConfigureAwait(false);
        return $"Processed '{text}' at {_timeProvider.GetLocalNow():T}.";
    }

    /// <summary>Handles command errors and notifies the interaction.</summary>
    /// <param name="error">The command error.</param>
    private void HandleError(Exception error)
    {
        LastError = error.Message;
        _ = ReportError.Handle(error.Message).SubscribeSafe(
            static _ => { },
            interactionError => InteractionStatus = $"No active view handled the interaction: {interactionError.Message}");
    }
}
