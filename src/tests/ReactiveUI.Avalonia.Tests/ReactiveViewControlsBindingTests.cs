// Copyright (c) 2019-2026 ReactiveUI and Avalonia Teams, and Contributors. All rights reserved.
// Licensed under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Avalonia.Tests;

/// <summary>
/// Tests for ReactiveUserControl ViewModel and DataContext synchronization.
/// </summary>
public class ReactiveViewControlsBindingTests
{
    /// <summary>
    /// Verifies that setting DataContext syncs to ViewModel and vice versa.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveUserControl_ViewModel_DataContext_Syncs()
    {
        var v = new View();
        var vm = new VM();

        v.DataContext = vm;
        await Assert.That(v.ViewModel).IsSameReferenceAs(vm);

        var vm2 = new VM();
        v.ViewModel = vm2;
        await Assert.That(v.DataContext).IsSameReferenceAs(vm2);
    }

    /// <summary>
    /// A test view model with a Name property.
    /// </summary>
    private sealed class VM : ReactiveObject
    {
        /// <summary>
        /// The backing field for <see cref="Name"/>.
        /// </summary>
        private string? _name;

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string? Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }
    }

    /// <summary>
    /// A test view for VM.
    /// </summary>
    private sealed class View : ReactiveUserControl<VM>;
}
