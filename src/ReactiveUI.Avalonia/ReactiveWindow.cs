// Copyright (c) 2025 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Avalonia;

/// <summary>
/// A ReactiveUI <see cref="Window"/> that implements the <see cref="IViewFor{TViewModel}"/> interface and will
/// activate your ViewModel automatically if the view model implements <see cref="IActivatableViewModel"/>. When
/// the DataContext property changes, this class will update the ViewModel property with the new DataContext value,
/// and vice versa.
/// </summary>
/// <typeparam name="TViewModel">ViewModel type.</typeparam>
public class ReactiveWindow<TViewModel> : Window, IViewFor<TViewModel>
    where TViewModel : class
{
    /// <summary>
    /// Identifies the ViewModel dependency property for a ReactiveWindow.
    /// </summary>
    /// <remarks>This field is used to register and reference the ViewModel property within Avalonia's
    /// property system. It enables data binding and change notification for the ViewModel associated with the
    /// window.</remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("AvaloniaProperty", "AVP1002", Justification = "Generic avalonia property is expected here.")]
    public static readonly StyledProperty<TViewModel?> ViewModelProperty = AvaloniaProperty
        .Register<ReactiveWindow<TViewModel>, TViewModel?>(nameof(ViewModel));

    /// <summary>
    /// Initializes a new instance of the <see cref="ReactiveWindow{TViewModel}"/> class and sets up activation handling for the window and its.
    /// view model.
    /// </summary>
    /// <remarks>This constructor configures the window to participate in activation lifecycle management. If
    /// the associated view model implements IActivatableViewModel, its activation logic will be invoked when the window
    /// is activated. This enables coordinated resource management and event handling between the view and its view
    /// model.</remarks>
    public ReactiveWindow()
    {
        // This WhenActivated block calls ViewModel's WhenActivated
        // block if the ViewModel implements IActivatableViewModel.
        this.WhenActivated(disposables => { });
    }

    /// <inheritdoc cref="IViewFor{TViewModel}.ViewModel"/>
    public TViewModel? ViewModel
    {
        get => GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    /// <inheritdoc cref="IViewFor{TViewModel}.ViewModel"/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (TViewModel?)value;
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

#pragma warning disable CA1062
        if (change.Property == DataContextProperty)
#pragma warning restore CA1062
        {
            if (ReferenceEquals(change.OldValue, ViewModel)
                && change.NewValue is null or TViewModel)
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
}
