// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
#if REACTIVE_SHIM
namespace ReactiveUI.Avalonia.Reactive;
#else
namespace ReactiveUI.Avalonia;
#endif

/// <summary>A non-generic ReactiveUI <see cref="UserControl"/> base that owns the Avalonia view model property.</summary>
public class ReactiveUserControlBase : UserControl, IViewFor
{
    /// <summary>Identifies the ViewModel dependency property for a ReactiveUserControl.</summary>
    /// <remarks>This property enables data binding of a view model to a ReactiveUserControl instance in
    /// Avalonia applications. It is typically used to associate a view model with the control for reactive UI
    /// scenarios.</remarks>
    public static readonly StyledProperty<object?> ViewModelProperty = AvaloniaProperty
        .Register<ReactiveUserControlBase, object?>(nameof(ViewModel));

    /// <summary>Initializes a new instance of the <see cref="ReactiveUserControlBase"/> class.</summary>
    /// <remarks>When the control is activated, this constructor ensures that any activation logic defined in
    /// the associated ViewModel is also executed, provided the ViewModel implements IActivatableViewModel. This enables
    /// coordinated activation and deactivation between the view and its ViewModel, which is useful for managing
    /// resources and subscriptions in reactive UI scenarios.</remarks>
    [RequiresUnreferencedCode("ReactiveUI activation evaluates expression-based member chains via reflection; members may be trimmed.")]
    protected ReactiveUserControlBase()
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
