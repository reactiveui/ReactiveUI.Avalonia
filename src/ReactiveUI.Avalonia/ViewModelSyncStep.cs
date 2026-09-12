// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
#if REACTIVE_SHIM
namespace ReactiveUI.Avalonia.Reactive;
#else
namespace ReactiveUI.Avalonia;
#endif

/// <summary>Identifies which way a view model value has to travel after a property change.</summary>
internal enum ViewModelSyncStep
{
    /// <summary>The change affects neither the view model property nor the data context.</summary>
    None = 0,

    /// <summary>The data context changed, so the view model property follows it once the value is accepted.</summary>
    AdoptDataContext = 1,

    /// <summary>The view model property changed, so the data context follows it.</summary>
    PushToDataContext = 2,
}
