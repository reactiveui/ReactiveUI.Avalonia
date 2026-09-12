// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using ReactiveUI;
using ReactiveUI.Avalonia.Example.ViewModels;
using Unit = ReactiveUI.Primitives.RxVoid;

namespace ReactiveUI.Avalonia.Example.Views;

/// <summary>The routed command lab view.</summary>
[System.Diagnostics.DebuggerDisplay("CommandLabView: {ToString(),nq}")]
public sealed partial class CommandLabView : ReactiveUserControl<CommandLabViewModel>
{
    /// <summary>Initializes a new instance of the <see cref="CommandLabView"/> class.</summary>
    [RequiresUnreferencedCode("The view uses reflection-based ReactiveUI binding and activation.")]
    [RequiresDynamicCode("The view uses dynamic ReactiveUI property bindings.")]
    public CommandLabView()
    {
        InitializeComponent();
        _ = this.WhenActivated(BindView);
    }

    /// <summary>Creates bindings owned by this activation.</summary>
    /// <param name="disposables">The activation lifetime.</param>
    [RequiresUnreferencedCode("The view uses reflection-based ReactiveUI bindings.")]
    [RequiresDynamicCode("The view uses dynamic ReactiveUI property bindings.")]
    private void BindView(ReactiveUI.Primitives.Disposables.MultipleDisposable disposables)
    {
        disposables.Add(this.Bind(ViewModel, static viewModel => viewModel.WorkItemText, static view => view.WorkItemTextBox.Text));

        disposables.Add(this.OneWayBind(ViewModel, static viewModel => viewModel.LastResult, static view => view.LastResultText.Text));

        disposables.Add(this.OneWayBind(ViewModel, static viewModel => viewModel.LastError, static view => view.LastErrorText.Text));

        disposables.Add(this.OneWayBind(ViewModel, static viewModel => viewModel.InteractionStatus, static view => view.InteractionStatusText.Text));

        disposables.Add(this.BindCommand(ViewModel, static viewModel => viewModel.RunWork, static view => view.RunWorkButton));

        disposables.Add(this.BindCommand(ViewModel, static viewModel => viewModel.FailWork, static view => view.FailWorkButton));

        if (ViewModel is null)
        {
            return;
        }

        disposables.Add(ViewModel.ReportError.RegisterHandler(context =>
        {
            ViewModel.AcknowledgeError(context.Input);
            context.SetOutput(Unit.Default);
        }));
    }
}
