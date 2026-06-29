// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
#if REACTIVE_SHIM
namespace ReactiveUI.Avalonia.Reactive;
#else
namespace ReactiveUI.Avalonia;
#endif

/// <summary>Provides a binding hook that automatically assigns a default data template to an ItemsControl when no item template or data templates are defined.</summary>
/// <remarks>This class is typically used in scenarios where view models are bound to item controls and a data
/// template is required for proper rendering. If the ItemsControl does not have an ItemTemplate or any DataTemplates, a
/// default template is applied to ensure that items are displayed using a ViewModelViewHost. This behavior helps
/// prevent issues where items may not be rendered due to missing templates.</remarks>
public class AutoDataTemplateBindingHook : IPropertyBindingHook
{
    /// <summary>The default data template that wraps items in a <see cref="ViewModelViewHost"/>.</summary>
    private static readonly FuncDataTemplate DefaultItemTemplate = new FuncDataTemplate<object>(
     (_, _) =>
     {
         var control = new ViewModelViewHost();
         var context = control.GetObservable(StyledElement.DataContextProperty);
         _ = control.Bind(ViewModelViewHost.ViewModelProperty, context);
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
        ArgumentNullException.ThrowIfNull(getCurrentViewProperties);

        var viewProperties = getCurrentViewProperties();
        var lastViewProperty = viewProperties.Length == 0 ? null : viewProperties[^1];
        if (lastViewProperty?.Sender is not ItemsControl itemsControl)
        {
            return true;
        }

        var propertyName = lastViewProperty.GetPropertyName();
        if (propertyName != "Items" &&
            propertyName != "ItemsSource")
        {
            return true;
        }

        if (itemsControl.ItemTemplate is not null)
        {
            return true;
        }

        if (itemsControl.DataTemplates.Count > 0)
        {
            return true;
        }

        itemsControl.ItemTemplate = DefaultItemTemplate;
        return true;
    }
}
