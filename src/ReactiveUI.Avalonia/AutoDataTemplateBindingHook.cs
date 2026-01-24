// Copyright (c) 2025 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Avalonia;

/// <summary>
/// Provides a binding hook that automatically assigns a default data template to an ItemsControl when no item template
/// or data templates are defined.
/// </summary>
/// <remarks>This class is typically used in scenarios where view models are bound to item controls and a data
/// template is required for proper rendering. If the ItemsControl does not have an ItemTemplate or any DataTemplates, a
/// default template is applied to ensure that items are displayed using a ViewModelViewHost. This behavior helps
/// prevent issues where items may not be rendered due to missing templates.</remarks>
public class AutoDataTemplateBindingHook : IPropertyBindingHook
{
    private static readonly FuncDataTemplate DefaultItemTemplate = new FuncDataTemplate<object>(
     (_, _) =>
     {
         var control = new ViewModelViewHost();
         var context = control.GetObservable(StyledElement.DataContextProperty);
         control.Bind(ViewModelViewHost.ViewModelProperty, context);
         control.HorizontalContentAlignment = HorizontalAlignment.Stretch;
         control.VerticalContentAlignment = VerticalAlignment.Stretch;
         return control;
     },
     true);

    /// <inheritdoc/>
    public bool ExecuteHook(
        object? source,
        object target,
        Func<IObservedChange<object, object>[]> getCurrentViewModelProperties,
        Func<IObservedChange<object, object>[]> getCurrentViewProperties,
        BindingDirection direction)
    {
        if (getCurrentViewProperties is null)
        {
            throw new ArgumentNullException(nameof(getCurrentViewProperties));
        }

        var viewProperties = getCurrentViewProperties();
        var lastViewProperty = viewProperties.LastOrDefault();
        if (lastViewProperty?.Sender is not ItemsControl itemsControl)
        {
            return true;
        }

        var propertyName = viewProperties.Last().GetPropertyName();
        if (propertyName != "Items" &&
            propertyName != "ItemsSource")
        {
            return true;
        }

        if (itemsControl.ItemTemplate != null)
        {
            return true;
        }

        if (itemsControl.DataTemplates?.Count > 0)
        {
            return true;
        }

        itemsControl.ItemTemplate = DefaultItemTemplate;
        return true;
    }
}
