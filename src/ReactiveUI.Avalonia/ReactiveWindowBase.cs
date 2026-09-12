// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
#if REACTIVE_SHIM
namespace ReactiveUI.Avalonia.Reactive;
#else
namespace ReactiveUI.Avalonia;
#endif

/// <summary>A non-generic ReactiveUI <see cref="Window"/> base that owns the Avalonia view model property.</summary>
[System.Diagnostics.DebuggerDisplay("ReactiveWindowBase: {ToString(),nq}")]
public class ReactiveWindowBase : Window, IViewFor
{
    /// <summary>Identifies the ViewModel dependency property for a ReactiveWindow.</summary>
    /// <remarks>This field is used to register and reference the ViewModel property within Avalonia's
    /// property system. It enables data binding and change notification for the ViewModel associated with the
    /// window.</remarks>
    public static readonly StyledProperty<object?> ViewModelProperty = ViewModelPropertySync.Register<ReactiveWindowBase>();

    /// <summary>Holds the view model value filter so the property-changed path allocates nothing.</summary>
    private readonly Func<object?, bool> _isValidViewModelValue;

    /// <summary>Initializes a new instance of the <see cref="ReactiveWindowBase"/> class.</summary>
    /// <remarks>This constructor configures the window to participate in activation lifecycle management. If
    /// the associated view model implements IActivatableViewModel, its activation logic will be invoked when the window
    /// is activated. This enables coordinated resource management and event handling between the view and its view
    /// model.</remarks>
    [RequiresUnreferencedCode("ReactiveUI activation evaluates expression-based member chains via reflection; members may be trimmed.")]
    protected ReactiveWindowBase()
    {
        _isValidViewModelValue = IsValidViewModelValue;
        ViewModelPropertySync.ForwardActivation(this);
    }

    /// <inheritdoc/>
    /// <remarks>Implemented explicitly so <c>ReactiveWindow&lt;TViewModel&gt;</c> can expose the only public
    /// <c>ViewModel</c> property; two public properties of that name and differing types would make ReactiveUI's
    /// resolution of it by name throw <see cref="System.Reflection.AmbiguousMatchException"/>.</remarks>
    object? IViewFor.ViewModel
    {
        get => GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        ArgumentNullException.ThrowIfNull(change);
        base.OnPropertyChanged(change);

        ViewModelPropertySync.Apply(this, change, ViewModelProperty, _isValidViewModelValue);
    }

    /// <summary>Determines whether the specified value is valid for the view model property.</summary>
    /// <param name="value">The value to validate.</param>
    /// <returns><see langword="true"/> when the value can be assigned to <see cref="ViewModelProperty"/>; otherwise, <see langword="false"/>.</returns>
    protected virtual bool IsValidViewModelValue(object? value) => true;
}
