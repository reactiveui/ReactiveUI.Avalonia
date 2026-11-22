// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
#if REACTIVE_SHIM
namespace ReactiveUI.Avalonia.Reactive;
#else
namespace ReactiveUI.Avalonia;
#endif

/// <summary>Hosts the current routed view inside the layout its <see cref="ViewLayoutAttribute"/> names.</summary>
/// <remarks>
/// <para>
/// A layout is a <see cref="ContentControl"/> that draws shared chrome around the routed view, which the host assigns
/// to the layout's content. The layout places that content through its template or content template.
/// </para>
/// <para>
/// Consecutive routed views that name the same layout type share one layout instance, so the chrome and the layout's
/// view model survive the navigation. A view without a layout is shown directly, as <see cref="RoutedViewHost"/> does.
/// </para>
/// <para>
/// A layout instance comes from the current dependency resolver when the layout type is registered there, and
/// otherwise from the type's public parameterless constructor. When the instance is not a
/// <see cref="ContentControl"/> it is treated as the layout's view model and its view is resolved through the view
/// locator.
/// </para>
/// </remarks>
[System.Diagnostics.DebuggerDisplay("LayoutViewHost: {Router}")]
public class LayoutViewHost : RoutedViewHost
{
    /// <summary>The layout type the current layout was created for.</summary>
    private Type? _layoutType;

    /// <summary>The layout wrapping the current routed view, or null when the view has no layout.</summary>
    private ContentControl? _layout;

    /// <inheritdoc/>
    private protected override void ShowView(IViewFor? view)
    {
        var layout = view is null ? null : GetLayout(view);
        if (layout is null)
        {
            _layout = null;
            _layoutType = null;
            DetachFromOtherParent(view, this);
            base.ShowView(view);
            return;
        }

        DetachFromOtherParent(view, layout);
        layout.Content = view;
        Content = layout;
    }

    /// <summary>Finds the layout declared for a view, checking the view before its view model.</summary>
    /// <param name="view">The resolved view.</param>
    /// <returns>The layout declaration, or null when neither the view nor its view model has one.</returns>
    private static ViewLayoutAttribute? FindLayoutAttribute(IViewFor view) =>
        view.GetType().GetCustomAttribute<ViewLayoutAttribute>()
        ?? view.ViewModel?.GetType().GetCustomAttribute<ViewLayoutAttribute>();

    /// <summary>Removes a view from the content control that still holds it from an earlier navigation.</summary>
    /// <param name="view">The view about to be shown.</param>
    /// <param name="newParent">The control the view is about to be assigned to.</param>
    /// <remarks>
    /// A view locator may hand back the same view instance for consecutive navigations. Avalonia rejects a control
    /// that already has a parent, so the instance has to leave its previous layout or the host first.
    /// </remarks>
    private static void DetachFromOtherParent(IViewFor? view, ContentControl newParent)
    {
        if (view is StyledElement { Parent: ContentControl parent }
            && !ReferenceEquals(parent, newParent)
            && ReferenceEquals(parent.Content, view))
        {
            parent.Content = null;
        }
    }

    /// <summary>Gets the layout for a view, reusing the current layout when the view names the same layout type.</summary>
    /// <param name="view">The resolved view.</param>
    /// <returns>The layout to wrap the view in, or null when the view is shown without one.</returns>
    private ContentControl? GetLayout(IViewFor view)
    {
        var attribute = FindLayoutAttribute(view);
        if (attribute is null)
        {
            return null;
        }

        if (_layout is not null && attribute.LayoutType == _layoutType)
        {
            return _layout;
        }

        _layoutType = attribute.LayoutType;
        return _layout = CreateLayout(attribute.LayoutType);
    }

    /// <summary>Creates the layout for a layout type.</summary>
    /// <param name="layoutType">The layout view type or layout view model type.</param>
    /// <returns>The layout, or null when the type does not lead to a <see cref="ContentControl"/>.</returns>
    private ContentControl? CreateLayout(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type layoutType)
    {
        var instance = AppLocator.Current.GetService(layoutType) ?? Activator.CreateInstance(layoutType)!;
        if (instance is ContentControl layout)
        {
            return layout;
        }

        if (ViewHostNavigation.ResolveView(this, instance, null, ViewLocator) is ContentControl layoutView)
        {
            return layoutView;
        }

        this.Log().Warn($"Layout '{layoutType}' did not resolve to a ContentControl. Showing the view without a layout.");
        return null;
    }
}
