// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
#if REACTIVE_SHIM
namespace ReactiveUI.Avalonia.Reactive;
#else
namespace ReactiveUI.Avalonia;
#endif

/// <summary>Holds what a typed reactive view needs to keep its view model property and data context in step.</summary>
/// <typeparam name="TViewModel">The view model type.</typeparam>
/// <param name="Property">The view's typed view model property.</param>
/// <param name="IsValidViewModelValue">
/// Decides whether an incoming data context may become the view model. It must only accept <see langword="null"/> or a
/// <typeparamref name="TViewModel"/>. The view holds it for its whole life, so property changes allocate nothing.
/// </param>
internal readonly record struct ViewModelPropertyState<TViewModel>(
    StyledProperty<TViewModel?> Property,
    Func<object?, bool> IsValidViewModelValue)
    where TViewModel : class;
