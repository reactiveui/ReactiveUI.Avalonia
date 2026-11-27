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
    /// <summary>Forwards the view's activation to its view model when the view model takes part in activation.</summary>
    /// <typeparam name="TView">The view type.</typeparam>
    /// <typeparam name="TValue">The view model property's value type.</typeparam>
    /// <param name="view">The view to activate with.</param>
    /// <param name="viewModelProperty">The view's view model property.</param>
    internal static void ForwardActivation<TView, TValue>(TView view, StyledProperty<TValue?> viewModelProperty)
        where TView : AvaloniaObject, IActivatableView
        where TValue : class
    {
        // The Avalonia property observable tells ReactiveUI when the view model changes.
        // Without it, ReactiveUI finds the view model by reflection, which trimming can break.
        _ = view.WhenActivated(view.GetObservable(viewModelProperty));
    }

    /// <summary>Sets up a typed reactive view: forwards its activation, keeps its data context in step, and returns its state.</summary>
    /// <typeparam name="TView">The view type.</typeparam>
    /// <typeparam name="TViewModel">The view model type.</typeparam>
    /// <param name="view">The view being constructed.</param>
    /// <param name="property">The view's typed view model property.</param>
    /// <param name="isValidViewModelValue">Decides whether an incoming data context may become the view model.</param>
    /// <returns>The state to hold for the life of the view.</returns>
    internal static ViewModelPropertyState<TViewModel> Attach<TView, TViewModel>(
        TView view,
        StyledProperty<TViewModel?> property,
        Func<object?, bool> isValidViewModelValue)
        where TView : StyledElement, IActivatableView
        where TViewModel : class
    {
        ForwardActivation(view, property);
        ViewModelPropertyState<TViewModel> state = new(property, isValidViewModelValue);

        // The view raises this for its own changes, so the handler lives exactly as long as the view.
        view.PropertyChanged += state.OnPropertyChanged;
        return state;
    }

    /// <summary>Reads a typed view's view model.</summary>
    /// <typeparam name="TViewModel">The view model type.</typeparam>
    /// <param name="view">The view.</param>
    /// <param name="state">The view's state.</param>
    /// <returns>The view model, or <see langword="null"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static TViewModel? GetViewModel<TViewModel>(AvaloniaObject view, ViewModelPropertyState<TViewModel> state)
        where TViewModel : class => view.GetValue(state.Property);

    /// <summary>Sets a typed view's view model.</summary>
    /// <typeparam name="TViewModel">The view model type.</typeparam>
    /// <param name="view">The view.</param>
    /// <param name="state">The view's state.</param>
    /// <param name="value">The view model to set, or <see langword="null"/>; any other type throws.</param>
    /// <exception cref="InvalidCastException"><paramref name="value"/> is not a <typeparamref name="TViewModel"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void SetViewModel<TViewModel>(AvaloniaObject view, ViewModelPropertyState<TViewModel> state, object? value)
        where TViewModel : class => _ = view.SetValue(state.Property, (TViewModel?)value);

    /// <summary>Returns whether a value may become a <typeparamref name="TViewModel"/> view model.</summary>
    /// <typeparam name="TViewModel">The view model type.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <returns><see langword="true"/> when the value is <see langword="null"/> or a <typeparamref name="TViewModel"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsViewModelOrNull<TViewModel>(object? value)
        where TViewModel : class => value is null or TViewModel;

    /// <summary>Propagates a change between the view model property and the data context.</summary>
    /// <typeparam name="TValue">The view model property's value type.</typeparam>
    /// <param name="view">The view whose property changed.</param>
    /// <param name="change">The property change to propagate.</param>
    /// <param name="viewModelProperty">The view's view model property.</param>
    /// <param name="isValidViewModelValue">
    /// Decides whether an incoming data context may become the view model. It must only accept values of
    /// <typeparamref name="TValue"/>. Callers pass a delegate held for the life of the view, because this runs for every
    /// property change.
    /// </param>
    internal static void Apply<TValue>(
        StyledElement view,
        AvaloniaPropertyChangedEventArgs change,
        StyledProperty<TValue?> viewModelProperty,
        Func<object?, bool> isValidViewModelValue)
        where TValue : class
    {
        if (change.Property == StyledElement.DataContextProperty
            && ReferenceEquals(change.OldValue, view.GetValue(viewModelProperty))
            && isValidViewModelValue(change.NewValue))
        {
            view.SetCurrentValue(viewModelProperty, (TValue?)change.NewValue);
        }
        else if (change.Property == viewModelProperty
                 && ReferenceEquals(change.OldValue, view.DataContext))
        {
            view.SetCurrentValue(StyledElement.DataContextProperty, change.NewValue);
        }
    }
}
