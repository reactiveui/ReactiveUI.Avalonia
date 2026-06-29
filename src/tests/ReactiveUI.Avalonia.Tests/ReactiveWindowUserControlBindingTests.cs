// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
namespace ReactiveUI.Avalonia.Tests;

/// <summary>Tests for ReactiveUserControl DataContext and ViewModel bidirectional binding.</summary>
public class ReactiveWindowUserControlBindingTests
{
    /// <summary>Verifies that DataContext and ViewModel stay in sync.</summary>
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

    /// <summary>Verifies that setting ViewModel to null also sets DataContext to null.</summary>
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

    /// <summary>Verifies that the explicit IViewFor implementation delegates to the typed ViewModel.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveUserControl_IViewFor_ViewModel_Delegates_To_Typed_ViewModel()
    {
        var c = new C();
        var vm = new VM();
        var view = (IViewFor)c;

        view.ViewModel = vm;

        await Assert.That(view.ViewModel).IsSameReferenceAs(vm);
        await Assert.That(c.ViewModel).IsSameReferenceAs(vm);
    }

    /// <summary>Verifies that an incompatible DataContext does not replace the typed ViewModel.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveUserControl_Incompatible_DataContext_Does_Not_Replace_ViewModel()
    {
        var c = new C();
        var vm = new VM();
        c.DataContext = vm;

        c.DataContext = new();

        await Assert.That(c.ViewModel).IsSameReferenceAs(vm);
    }

    /// <summary>Verifies that ReactiveWindow keeps DataContext and ViewModel in sync.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveWindow_DataContext_And_ViewModel_Sync()
    {
        var window = new W();
        var vm = new VM();

        window.DataContext = vm;
        await Assert.That(window.ViewModel).IsSameReferenceAs(vm);

        var vm2 = new VM();
        window.ViewModel = vm2;
        await Assert.That(window.DataContext).IsSameReferenceAs(vm2);
    }

    /// <summary>Verifies that setting a ReactiveWindow ViewModel to null also sets DataContext to null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveWindow_ViewModel_Set_To_Null_Syncs_DataContext()
    {
        var window = new W();
        var vm = new VM();
        window.DataContext = vm;
        await Assert.That(window.ViewModel).IsSameReferenceAs(vm);

        window.ViewModel = null;
        await Assert.That(window.DataContext).IsNull();
    }

    /// <summary>Verifies that the ReactiveWindow explicit IViewFor implementation delegates to the typed ViewModel.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveWindow_IViewFor_ViewModel_Delegates_To_Typed_ViewModel()
    {
        var window = new W();
        var vm = new VM();
        var view = (IViewFor)window;

        view.ViewModel = vm;

        await Assert.That(view.ViewModel).IsSameReferenceAs(vm);
        await Assert.That(window.ViewModel).IsSameReferenceAs(vm);
    }

    /// <summary>Verifies that an incompatible ReactiveWindow DataContext does not replace the typed ViewModel.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveWindow_Incompatible_DataContext_Does_Not_Replace_ViewModel()
    {
        var window = new W();
        var vm = new VM();
        window.DataContext = vm;

        window.DataContext = new();

        await Assert.That(window.ViewModel).IsSameReferenceAs(vm);
    }

    /// <summary>Verifies that the non-generic ReactiveUserControlBase synchronizes arbitrary view model values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveUserControlBase_DataContext_And_ViewModel_Sync()
    {
        var control = new BaseC();
        var vm = new object();

        control.DataContext = vm;

        await Assert.That(control.ViewModel).IsSameReferenceAs(vm);
    }

    /// <summary>Verifies that the non-generic ReactiveWindowBase synchronizes arbitrary view model values.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveWindowBase_DataContext_And_ViewModel_Sync()
    {
        var window = new BaseW();
        var vm = new object();

        window.DataContext = vm;

        await Assert.That(window.ViewModel).IsSameReferenceAs(vm);
    }

    /// <summary>A test view model with a Name property.</summary>
    private sealed class VM : ReactiveObject;

    /// <summary>A test ReactiveUserControl for VM.</summary>
    private sealed class C : ReactiveUserControl<VM>;

    /// <summary>A test ReactiveWindow for VM.</summary>
    private sealed class W : ReactiveWindow<VM>;

    /// <summary>A test non-generic ReactiveUserControlBase.</summary>
    private sealed class BaseC : ReactiveUserControlBase;

    /// <summary>A test non-generic ReactiveWindowBase.</summary>
    private sealed class BaseW : ReactiveWindowBase;
}
