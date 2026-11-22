// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
#if REACTIVE_SHIM
namespace ReactiveUI.Avalonia.Reactive;
#else
namespace ReactiveUI.Avalonia;
#endif

/// <summary>Declares the layout a <see cref="LayoutViewHost"/> wraps around a routed view.</summary>
/// <param name="layoutType">
/// The layout view type, which must derive from <see cref="ContentControl"/>, or the type of the layout's view model.
/// </param>
/// <remarks>
/// Apply the attribute to a view or to its view model. An attribute on the view takes precedence.
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
[System.Diagnostics.DebuggerDisplay("ViewLayout: {LayoutType}")]
public sealed class ViewLayoutAttribute(
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type layoutType) : Attribute
{
    /// <summary>Gets the layout view type or the layout view model type.</summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public Type LayoutType { get; } = layoutType ?? throw new ArgumentNullException(nameof(layoutType));
}
