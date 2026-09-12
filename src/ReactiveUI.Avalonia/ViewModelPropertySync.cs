// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
#if REACTIVE_SHIM
namespace ReactiveUI.Avalonia.Reactive;
#else
namespace ReactiveUI.Avalonia;
#endif

/// <summary>Decides which way a view model value travels when a view's property changes.</summary>
/// <remarks>
/// A view model reaches a view either through the view model property or through the data context, and each has to
/// follow the other. The two reactive bases cannot share this by inheritance because one extends
/// <see cref="UserControl"/> and the other <see cref="Window"/>. The decision is returned rather than applied so the
/// caller keeps its own value filter on the branch that needs it, without a delegate on a path that runs for every
/// property change.
/// </remarks>
internal static class ViewModelPropertySync
{
    /// <summary>Determines which way the changed value has to travel.</summary>
    /// <param name="view">The view whose property changed.</param>
    /// <param name="change">The property change to classify.</param>
    /// <param name="viewModelProperty">The view's view model property.</param>
    /// <returns>The direction the value travels, or <see cref="ViewModelSyncStep.None"/> when it stays put.</returns>
    internal static ViewModelSyncStep Classify(StyledElement view, AvaloniaPropertyChangedEventArgs change, StyledProperty<object?> viewModelProperty)
    {
        if (change.Property == StyledElement.DataContextProperty
            && ReferenceEquals(change.OldValue, view.GetValue(viewModelProperty)))
        {
            return ViewModelSyncStep.AdoptDataContext;
        }

        return change.Property == viewModelProperty
            && ReferenceEquals(change.OldValue, view.DataContext) ? ViewModelSyncStep.PushToDataContext : ViewModelSyncStep.None;
    }
}
