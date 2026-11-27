// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
#if REACTIVE_SHIM
namespace ReactiveUI.Avalonia.Reactive;
#else
namespace ReactiveUI.Avalonia;
#endif

/// <summary>A ReactiveUI <see cref="UserControl"/> that implements <see cref="IViewFor{TViewModel}"/>.</summary>
/// <typeparam name="TViewModel">ViewModel type.</typeparam>
[System.Diagnostics.DebuggerDisplay("ReactiveUserControl: {ViewModel}")]
public class ReactiveUserControl<TViewModel> : UserControl, IViewFor<TViewModel>
    where TViewModel : class
{
    /// <summary>Identifies the <see cref="ViewModel"/> styled property.</summary>
    /// <remarks>The property is typed as <typeparamref name="TViewModel"/>, so bindings to it need no cast.</remarks>
    public static readonly StyledProperty<TViewModel?> ViewModelProperty =
        AvaloniaProperty.Register<ReactiveUserControl<TViewModel>, TViewModel?>(nameof(ViewModel));

    /// <summary>The view model property and data context filter, held so property changes allocate nothing.</summary>
    private readonly ViewModelPropertyState<TViewModel> _state;

    /// <summary>Initializes a new instance of the <see cref="ReactiveUserControl{TViewModel}"/> class.</summary>
    /// <remarks>When the control is activated, any activation logic in a view model that implements
    /// <c>IActivatableViewModel</c> runs too.</remarks>
    public ReactiveUserControl() => _state = ViewModelPropertySync.Attach(this, ViewModelProperty, IsValidViewModelValue);

    /// <inheritdoc cref="IViewFor{TViewModel}.ViewModel"/>
    public TViewModel? ViewModel
    {
        get => ViewModelPropertySync.GetViewModel(this, _state);
        set => ViewModelPropertySync.SetViewModel(this, _state, value);
    }

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModelPropertySync.SetViewModel(this, _state, value);
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        ArgumentNullException.ThrowIfNull(change);
        base.OnPropertyChanged(change);
        ViewModelPropertySync.Apply(this, change, _state);
    }

    /// <summary>Determines whether a data context value may become the view model.</summary>
    /// <param name="value">The value to validate.</param>
    /// <returns><see langword="true"/> when the value is <see langword="null"/> or a <typeparamref name="TViewModel"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>An override must still return <see langword="false"/> for any value that is not a <typeparamref name="TViewModel"/>.</remarks>
    protected virtual bool IsValidViewModelValue(object? value) => ViewModelPropertySync.IsViewModelOrNull<TViewModel>(value);
}
