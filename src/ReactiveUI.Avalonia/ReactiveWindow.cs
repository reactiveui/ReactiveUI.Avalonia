// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
#if REACTIVE_SHIM
namespace ReactiveUI.Avalonia.Reactive;
#else
namespace ReactiveUI.Avalonia;
#endif

/// <summary>A ReactiveUI <see cref="Window"/> that implements <see cref="IViewFor{TViewModel}"/>.</summary>
/// <typeparam name="TViewModel">ViewModel type.</typeparam>
public class ReactiveWindow<TViewModel> : ReactiveWindowBase, IViewFor<TViewModel>, IViewFor
    where TViewModel : class
{
    /// <summary>Initializes a new instance of the <see cref="ReactiveWindow{TViewModel}"/> class.</summary>
    [RequiresUnreferencedCode("ReactiveUI activation evaluates expression-based member chains via reflection; members may be trimmed.")]
    public ReactiveWindow()
    {
    }

    /// <inheritdoc cref="IViewFor{TViewModel}.ViewModel"/>
    public new TViewModel? ViewModel
    {
        get => (TViewModel?)base.ViewModel;
        set => base.ViewModel = value;
    }

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (TViewModel?)value;
    }

    /// <inheritdoc/>
    protected override bool IsValidViewModelValue(object? value) => value is null or TViewModel;
}
