// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Reflection;

namespace ReactiveUI.Avalonia.Tests;

/// <summary>
/// Regression tests for issue #157: a duplicate public <c>ViewModel</c> property on
/// <c>ReactiveWindow{TViewModel}</c> / <c>ReactiveWindowBase</c> caused an
/// <see cref="AmbiguousMatchException"/> when ReactiveUI resolved the property by name, which broke
/// view model activation from the view.
/// </summary>
public class ReactiveWindowActivationTests
{
    /// <summary>Verifies that resolving <c>ViewModel</c> by name (as ReactiveUI does during activation) does not throw and returns the single typed property.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveWindow_Resolves_Single_Typed_ViewModel_Property()
    {
        // Mirrors ReactiveUI's reflection lookup that previously threw AmbiguousMatchException
        // because both ReactiveWindowBase and ReactiveWindow<T> exposed a public ViewModel property.
        var property = typeof(ActivatableWindow)
            .GetProperty("ViewModel", BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

        await Assert.That(property).IsNotNull();
        await Assert.That(property!.PropertyType).IsEqualTo(typeof(ActivatableViewModel));
    }

    /// <summary>Verifies that activating the view activates its <see cref="IActivatableViewModel"/> when no explicit view model change observable is supplied.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task View_Activation_Activates_ViewModel()
    {
        var window = new ActivatableWindow();
        using var viewModel = new ActivatableViewModel();
        var activationCount = 0;
        viewModel.WhenActivated((MultipleDisposable _) => activationCount++);
        window.ViewModel = viewModel;

        try
        {
            window.Show();

            await Assert.That(activationCount).IsGreaterThan(0);
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>An activatable view model used to observe activation from the view.</summary>
    private sealed class ActivatableViewModel : ReactiveObject, IActivatableViewModel, IDisposable
    {
        /// <inheritdoc/>
        public ViewModelActivator Activator { get; } = new();

        /// <inheritdoc/>
        public void Dispose() => Activator.Dispose();
    }

    /// <summary>A strongly typed reactive window under test.</summary>
    private sealed class ActivatableWindow : ReactiveWindow<ActivatableViewModel>;
}
