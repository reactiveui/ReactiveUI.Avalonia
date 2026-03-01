// Copyright (c) 2019-2026 ReactiveUI and Avalonia Teams, and Contributors. All rights reserved.
// Licensed under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Avalonia.Tests;

/// <summary>
/// Tests for ReactiveUserControl DataContext and ViewModel bidirectional binding.
/// </summary>
public class ReactiveWindowUserControlBindingTests
{
    /// <summary>
    /// Verifies that DataContext and ViewModel stay in sync.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveUserControl_DataContext_And_ViewModel_Sync()
    {
        var c = new C();
        var vm = new VM();

        c.DataContext = vm;
        await Assert.That(c.ViewModel).IsSameReferenceAs(vm);

        var vm2 = new VM();
        c.ViewModel = vm2;
        await Assert.That(c.DataContext).IsSameReferenceAs(vm2);
    }

    /// <summary>
    /// Verifies that setting ViewModel to null also sets DataContext to null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveUserControl_ViewModel_Set_To_Null_Syncs_DataContext()
    {
        var c = new C();
        var vm = new VM();
        c.DataContext = vm;
        await Assert.That(c.ViewModel).IsSameReferenceAs(vm);

        c.ViewModel = null;
        await Assert.That(c.DataContext).IsNull();
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
    /// A test ReactiveUserControl for VM.
    /// </summary>
    private sealed class C : ReactiveUserControl<VM>
    {
    }
}
