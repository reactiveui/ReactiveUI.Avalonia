// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Runtime.CompilerServices;

#if REACTIVE_SHIM
namespace ReactiveUI.Avalonia.Reactive;
#else
namespace ReactiveUI.Avalonia;
#endif

/// <summary>Keeps a view's view model property and its data context in step.</summary>
/// <remarks>
/// A view model reaches a view either through the view model property or through the data context, and each has to
/// follow the other. The two reactive bases cannot share this by inheritance because one extends
/// <see cref="UserControl"/> and the other <see cref="Window"/>.
/// </remarks>
internal static class ViewModelPropertySync
{
    /// <summary>Registers the Avalonia view model property for a view type.</summary>
    /// <typeparam name="TOwner">The view type that owns the property.</typeparam>
    /// <returns>The registered property.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static StyledProperty<object?> Register<TOwner>()
        where TOwner : AvaloniaObject =>
        AvaloniaProperty.Register<TOwner, object?>(nameof(IViewFor.ViewModel));

    /// <summary>Forwards the view's activation to its view model when the view model takes part in activation.</summary>
    /// <typeparam name="TView">The view type.</typeparam>
    /// <param name="view">The view to activate with.</param>
    [RequiresUnreferencedCode("ReactiveUI activation evaluates expression-based member chains via reflection; members may be trimmed.")]
    internal static void ForwardActivation<TView>(TView view)
        where TView : IActivatableView
    {
        // The empty block is the point: WhenActivated runs the view model's own activation.
        _ = view.WhenActivated(static (ActivationDisposables disposables) => { });
    }

    /// <summary>Propagates a change between the view model property and the data context.</summary>
    /// <param name="view">The view whose property changed.</param>
    /// <param name="change">The property change to propagate.</param>
    /// <param name="viewModelProperty">The view's view model property.</param>
    /// <param name="isValidViewModelValue">
    /// Decides whether an incoming data context may become the view model. Callers pass a delegate held for the life of
    /// the view, because this runs for every property change.
    /// </param>
    internal static void Apply(
        StyledElement view,
        AvaloniaPropertyChangedEventArgs change,
        StyledProperty<object?> viewModelProperty,
        Func<object?, bool> isValidViewModelValue)
    {
        if (change.Property == StyledElement.DataContextProperty
            && ReferenceEquals(change.OldValue, view.GetValue(viewModelProperty))
            && isValidViewModelValue(change.NewValue))
        {
            view.SetCurrentValue(viewModelProperty, change.NewValue);
        }
        else if (change.Property == viewModelProperty
                 && ReferenceEquals(change.OldValue, view.DataContext))
        {
            view.SetCurrentValue(StyledElement.DataContextProperty, change.NewValue);
        }
    }
}
