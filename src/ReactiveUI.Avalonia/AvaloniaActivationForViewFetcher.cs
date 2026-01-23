// Copyright (c) 2025 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Avalonia;

/// <summary>
/// Provides activation support for Avalonia views by determining affinity and supplying activation observables based on
/// Avalonia's visual and control types.
/// </summary>
/// <remarks>This class is intended for use with Avalonia UI elements that implement the IActivatableView
/// interface. It assigns affinity to types derived from Visual and supplies activation observables that reflect the
/// loaded and unloaded state of controls or the attachment and detachment of visuals from the visual tree. This enables
/// reactive activation and deactivation handling in Avalonia-based applications.</remarks>
public class AvaloniaActivationForViewFetcher : IActivationForViewFetcher
{
    /// <inheritdoc/>
    public int GetAffinityForView(Type view) => typeof(Visual).IsAssignableFrom(view) ? 10 : 0;

    /// <inheritdoc/>
    public IObservable<bool> GetActivationForView(IActivatableView view)
    {
        if (view is not Visual visual)
        {
            return Observable.Return(false);
        }

        if (view is Control control)
        {
            return GetActivationForControl(control);
        }

        return GetActivationForVisual(visual);
    }

    /// <summary>
    /// Returns an observable sequence that indicates whether the specified control is currently loaded in the visual
    /// tree.
    /// </summary>
    /// <remarks>This observable can be used to react to the control's activation and deactivation events,
    /// such as initializing resources when the control is loaded and cleaning up when it is unloaded. The sequence
    /// emits distinct values and does not repeat the same state consecutively.</remarks>
    /// <param name="control">The control to monitor for loaded and unloaded state changes. Cannot be null.</param>
    /// <returns>An observable sequence of Boolean values. Emits <see langword="true"/> when the control is loaded, and <see
    /// langword="false"/> when it is unloaded. The sequence only emits values when the state changes.</returns>
    private static IObservable<bool> GetActivationForControl(Control control)
    {
        var controlLoaded = Observable
                            .FromEventPattern<RoutedEventArgs>(
                                                               x => control.Loaded += x,
                                                               x => control.Loaded -= x)
                            .Select(args => true);
        var controlUnloaded = Observable
                              .FromEventPattern<RoutedEventArgs>(
                                                                 x => control.Unloaded += x,
                                                                 x => control.Unloaded -= x)
                              .Select(args => false);
        return controlLoaded
               .Merge(controlUnloaded)
               .DistinctUntilChanged();
    }

    /// <summary>
    /// Creates an observable sequence that emits activation state changes for the specified visual based on its
    /// attachment to the visual tree.
    /// </summary>
    /// <remarks>This method is useful for tracking when a visual becomes active or inactive within the visual
    /// tree, such as for resource management or UI updates. The returned observable emits distinct state changes and
    /// does not repeat the same value consecutively.</remarks>
    /// <param name="visual">The visual whose activation state will be monitored. Cannot be null.</param>
    /// <returns>An observable sequence of boolean values indicating the activation state of the visual. Emits <see
    /// langword="true"/> when the visual is attached to the visual tree, and <see langword="false"/> when it is
    /// detached. The sequence only emits when the state changes.</returns>
    private static IObservable<bool> GetActivationForVisual(Visual visual)
    {
        var visualLoaded = Observable
                           .FromEventPattern<VisualTreeAttachmentEventArgs>(
                                                                            x => visual.AttachedToVisualTree += x,
                                                                            x => visual.AttachedToVisualTree -= x)
                           .Select(args => true);
        var visualUnloaded = Observable
                             .FromEventPattern<VisualTreeAttachmentEventArgs>(
                                                                              x => visual.DetachedFromVisualTree += x,
                                                                              x => visual.DetachedFromVisualTree -= x)
                             .Select(args => false);
        return visualLoaded
               .Merge(visualUnloaded)
               .DistinctUntilChanged();
    }
}
