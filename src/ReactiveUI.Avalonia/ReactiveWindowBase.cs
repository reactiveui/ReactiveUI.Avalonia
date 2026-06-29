// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
#if REACTIVE_SHIM
namespace ReactiveUI.Avalonia.Reactive;
#else
namespace ReactiveUI.Avalonia;
#endif

/// <summary>A non-generic ReactiveUI <see cref="Window"/> base that owns the Avalonia view model property.</summary>
public class ReactiveWindowBase : Window, IViewFor
{
    /// <summary>Identifies the ViewModel dependency property for a ReactiveWindow.</summary>
    /// <remarks>This field is used to register and reference the ViewModel property within Avalonia's
    /// property system. It enables data binding and change notification for the ViewModel associated with the
    /// window.</remarks>
    public static readonly StyledProperty<object?> ViewModelProperty = AvaloniaProperty
        .Register<ReactiveWindowBase, object?>(nameof(ViewModel));

    /// <summary>Initializes a new instance of the <see cref="ReactiveWindowBase"/> class.</summary>
    /// <remarks>This constructor configures the window to participate in activation lifecycle management. If
    /// the associated view model implements IActivatableViewModel, its activation logic will be invoked when the window
    /// is activated. This enables coordinated resource management and event handling between the view and its view
    /// model.</remarks>
    [RequiresUnreferencedCode("ReactiveUI activation evaluates expression-based member chains via reflection; members may be trimmed.")]
    protected ReactiveWindowBase()
    {
        // This WhenActivated block calls ViewModel's WhenActivated
        // block if the ViewModel implements IActivatableViewModel.
        _ = this.WhenActivated((ActivationDisposables disposables) => { });
    }

    /// <inheritdoc cref="IViewFor.ViewModel"/>
    public virtual object? ViewModel
    {
        get => GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        ArgumentNullException.ThrowIfNull(change);
        base.OnPropertyChanged(change);

        if (change.Property == DataContextProperty)
        {
            if (ReferenceEquals(change.OldValue, ViewModel)
                && IsValidViewModelValue(change.NewValue))
            {
                SetCurrentValue(ViewModelProperty, change.NewValue);
            }
        }
        else if (change.Property == ViewModelProperty)
        {
            if (ReferenceEquals(change.OldValue, DataContext))
            {
                SetCurrentValue(DataContextProperty, change.NewValue);
            }
        }
    }

    /// <summary>Determines whether the specified value is valid for the view model property.</summary>
    /// <param name="value">The value to validate.</param>
    /// <returns><see langword="true"/> when the value can be assigned to <see cref="ViewModel"/>; otherwise, <see langword="false"/>.</returns>
    protected virtual bool IsValidViewModelValue(object? value) => true;
}
