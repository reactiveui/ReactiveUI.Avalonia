// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
#if REACTIVE_SHIM
namespace ReactiveUI.Avalonia.Reactive;
#else
namespace ReactiveUI.Avalonia;
#endif

/// <summary>Represents a pending navigation target.</summary>
/// <param name="ViewModel">The view model to display.</param>
/// <param name="Contract">The optional view contract.</param>
internal readonly record struct NavigationTarget(object? ViewModel, string? Contract);
