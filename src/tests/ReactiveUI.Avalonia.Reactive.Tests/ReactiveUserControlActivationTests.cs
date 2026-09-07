// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Avalonia.Controls;
using Avalonia.Threading;
using ReactiveUI.Reactive;

namespace ReactiveUI.Avalonia.Reactive.Tests;

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
        model.WhenActivated((ReactiveUI.Primitives.Reactive.Disposables.ContainerDisposable disposables) => activationCount++);
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

    /// <summary>A view model that owns its activation lifetime.</summary>
    private sealed class ActivatableModel : ReactiveObject, IActivatableViewModel, IDisposable
    {
        /// <inheritdoc/>
        public ViewModelActivator Activator { get; } = new();

        /// <inheritdoc/>
        public void Dispose() => Activator.Dispose();
    }
}
