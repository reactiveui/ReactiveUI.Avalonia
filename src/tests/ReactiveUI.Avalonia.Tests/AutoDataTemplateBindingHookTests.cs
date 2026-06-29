// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Linq.Expressions;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Layout;

namespace ReactiveUI.Avalonia.Tests;

/// <summary>Tests for the AutoDataTemplateBindingHook behavior.</summary>
public class AutoDataTemplateBindingHookTests
{
    /// <summary>Verifies that ExecuteHook sets a default template when no ItemTemplate is set.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExecuteHook_ItemsControlWithoutTemplate_SetsDefaultTemplate()
    {
        var hook = new AutoDataTemplateBindingHook();
        var items = new ListBox();

        var result = hook.ExecuteHook(
            source: null,
            target: items,
            getCurrentViewModelProperties: () => [],
            getCurrentViewProperties: () => [ItemsObservedChange(items)],
            direction: BindingDirection.TwoWay);

        await Assert.That(result).IsTrue();
        await Assert.That(items.ItemTemplate).IsNotNull();
    }

    /// <summary>Verifies that the default template creates a ViewModelViewHost with stretch alignment.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExecuteHook_DefaultTemplate_Builds_ViewModelViewHost()
    {
        var hook = new AutoDataTemplateBindingHook();
        var items = new ListBox();

        _ = hook.ExecuteHook(
            source: null,
            target: items,
            getCurrentViewModelProperties: () => [],
            getCurrentViewProperties: () => [ItemsSourceObservedChange(items)],
            direction: BindingDirection.OneWay);

        var control = items.ItemTemplate!.Build(new object());

        await Assert.That(control).IsTypeOf<ViewModelViewHost>();

        var host = (ViewModelViewHost)control!;
        await Assert.That(host.HorizontalContentAlignment).IsEqualTo(HorizontalAlignment.Stretch);
        await Assert.That(host.VerticalContentAlignment).IsEqualTo(VerticalAlignment.Stretch);
    }

    /// <summary>Verifies that ExecuteHook validates the view properties delegate.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExecuteHook_NullGetCurrentViewProperties_Throws()
    {
        var hook = new AutoDataTemplateBindingHook();

        await Assert.That(() => hook.ExecuteHook(null, new ListBox(), () => [], null!, BindingDirection.OneWay))
            .ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies that ExecuteHook returns without changes when no view properties are available.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExecuteHook_NoViewProperties_ReturnsTrue()
    {
        var hook = new AutoDataTemplateBindingHook();
        var items = new ListBox();

        var result = hook.ExecuteHook(null, items, () => [], () => [], BindingDirection.OneWay);

        await Assert.That(result).IsTrue();
        await Assert.That(items.ItemTemplate).IsNull();
    }

    /// <summary>Verifies that ExecuteHook ignores changes whose sender is not an ItemsControl.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExecuteHook_NonItemsControlSender_ReturnsTrue()
    {
        var hook = new AutoDataTemplateBindingHook();
        var text = new TextBlock();

        var result = hook.ExecuteHook(null, text, () => [], () => [TextObservedChange(text)], BindingDirection.OneWay);

        await Assert.That(result).IsTrue();
    }

    /// <summary>Verifies that ExecuteHook ignores ItemsControl properties other than Items and ItemsSource.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExecuteHook_NonItemsProperty_ReturnsTrue()
    {
        var hook = new AutoDataTemplateBindingHook();
        var items = new ListBox();

        var result = hook.ExecuteHook(null, items, () => [], () => [TagObservedChange(items)], BindingDirection.OneWay);

        await Assert.That(result).IsTrue();
        await Assert.That(items.ItemTemplate).IsNull();
    }

    /// <summary>Verifies that ExecuteHook does not override an already-set ItemTemplate.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExecuteHook_WhenItemTemplateAlreadySet_DoesNotOverride()
    {
        var hook = new AutoDataTemplateBindingHook();
        var items = new ListBox
        {
            ItemTemplate = new FuncDataTemplate<object>((_, _) => new TextBlock(), true)
        };

        var res = hook.ExecuteHook(
            null,
            items,
            () => [],
            () => [ItemsObservedChange(items)],
            BindingDirection.OneWay);

        await Assert.That(res).IsTrue();
        await Assert.That(items.ItemTemplate).IsNotNull();
    }

    /// <summary>Verifies that ExecuteHook does not override when DataTemplates are already present.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExecuteHook_WhenDataTemplatesPresent_DoesNotOverride()
    {
        var hook = new AutoDataTemplateBindingHook();
        var items = new ListBox();
        items.DataTemplates.Add(new FuncDataTemplate<object>((_, _) => new TextBlock(), true));

        var res = hook.ExecuteHook(
            null,
            items,
            () => [],
            () => [ItemsObservedChange(items)],
            BindingDirection.OneWay);

        await Assert.That(res).IsTrue();
        await Assert.That(items.ItemTemplate).IsNull();
    }

    /// <summary>Creates an observed change for the Items property of an ItemsControl.</summary>
    /// <param name="items">The items control instance.</param>
    /// <returns>An observed change representing the Items property.</returns>
    private static ObservedChange<object, object> ItemsObservedChange(ItemsControl items)
    {
        var param = Expression.Parameter(typeof(ItemsControl), "x");
        var member = Expression.Property(param, nameof(ItemsControl.Items));
        return new ObservedChange<object, object>(items, member, items.Items!);
    }

    /// <summary>Creates an observed change for the ItemsSource property of an ItemsControl.</summary>
    /// <param name="items">The items control instance.</param>
    /// <returns>An observed change representing the ItemsSource property.</returns>
    private static ObservedChange<object, object> ItemsSourceObservedChange(ItemsControl items)
    {
        var param = Expression.Parameter(typeof(ItemsControl), "x");
        var member = Expression.Property(param, nameof(ItemsControl.ItemsSource));
        return new ObservedChange<object, object>(items, member, items.ItemsSource!);
    }

    /// <summary>Creates an observed change for the Tag property of a control.</summary>
    /// <param name="control">The control instance.</param>
    /// <returns>An observed change representing the Tag property.</returns>
    private static ObservedChange<object, object> TagObservedChange(Control control)
    {
        var param = Expression.Parameter(typeof(Control), "x");
        var member = Expression.Property(param, nameof(Control.Tag));
        return new ObservedChange<object, object>(control, member, control.Tag!);
    }

    /// <summary>Creates an observed change for the Text property of a text block.</summary>
    /// <param name="text">The text block instance.</param>
    /// <returns>An observed change representing the Text property.</returns>
    private static ObservedChange<object, object> TextObservedChange(TextBlock text)
    {
        var param = Expression.Parameter(typeof(TextBlock), "x");
        var member = Expression.Property(param, nameof(TextBlock.Text));
        return new ObservedChange<object, object>(text, member, text.Text!);
    }
}
