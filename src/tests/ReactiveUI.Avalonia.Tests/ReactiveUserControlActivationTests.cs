// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Avalonia.Controls;
using Avalonia.Threading;
using ReactiveUI;

namespace ReactiveUI.Avalonia.Tests;

/// <summary>Tests view-model activation without a custom control activation block.</summary>
public sealed class ReactiveUserControlActivationTests
{
    /// <summary>Verifies the base control activates its view model when it becomes visible.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Base_Control_Activates_ViewModel_Without_Custom_View_Block()
    {
        using var model = new ActivatableModel();
        var activationCount = 0;
        model.WhenActivated((MultipleDisposable disposables) => activationCount++);
        var control = new ReactiveUserControl<ActivatableModel> { ViewModel = model };
        var window = new Window { Content = control };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            await Assert.That(activationCount).IsEqualTo(1);
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>Verifies a view model swapped in while the control is shown takes over activation from the old one.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Swapping_ViewModel_While_Shown_Moves_Activation_To_New_ViewModel()
    {
        using var first = new ActivatableModel();
        using var second = new ActivatableModel();
        var firstDeactivation = new DisposeCounter();
        var secondActivations = 0;
        first.WhenActivated(disposables => disposables.Add(firstDeactivation));
        second.WhenActivated((MultipleDisposable disposables) => secondActivations++);
        var control = new ReactiveUserControl<ActivatableModel> { ViewModel = first };
        var window = new Window { Content = control };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            control.DataContext = second;
            Dispatcher.UIThread.RunJobs();

            await Assert.That(control.ViewModel).IsSameReferenceAs(second);
            await Assert.That(firstDeactivation.Count).IsEqualTo(1);
            await Assert.That(secondActivations).IsEqualTo(1);
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>Counts how many times it is disposed.</summary>
    private sealed class DisposeCounter : IDisposable
    {
        /// <summary>Gets the number of times <see cref="Dispose"/> ran.</summary>
        public int Count { get; private set; }

        /// <inheritdoc/>
        public void Dispose() => Count++;
    }

    /// <summary>A view model that owns its activation lifetime.</summary>
    private sealed class ActivatableModel : ReactiveObject, IActivatableViewModel, IDisposable
    {
        /// <inheritdoc/>
        public ViewModelActivator Activator { get; } = new();

        /// <inheritdoc/>
        public void Dispose() => Activator.Dispose();
    }
}
