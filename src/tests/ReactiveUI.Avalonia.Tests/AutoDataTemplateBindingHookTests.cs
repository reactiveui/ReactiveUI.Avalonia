// Copyright (c) 2019-2026 ReactiveUI and Avalonia Teams, and Contributors. All rights reserved.
// Licensed under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Linq.Expressions;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace ReactiveUI.Avalonia.Tests;

/// <summary>
/// Tests for the AutoDataTemplateBindingHook behavior.
/// </summary>
public class AutoDataTemplateBindingHookTests
{
    /// <summary>
    /// Verifies that ExecuteHook sets a default template when no ItemTemplate is set.
    /// </summary>
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

    /// <summary>
    /// Verifies that ExecuteHook does not override an already-set ItemTemplate.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExecuteHook_WhenItemTemplateAlreadySet_DoesNotOverride()
    {
        var hook = new AutoDataTemplateBindingHook();
        var items = new ListBox();
        items.ItemTemplate = new FuncDataTemplate<object>((_, _) => new TextBlock(), true);

        var res = hook.ExecuteHook(
            null,
            items,
            () => [],
            () => [ItemsObservedChange(items)],
            BindingDirection.OneWay);

        await Assert.That(res).IsTrue();
        await Assert.That(items.ItemTemplate).IsNotNull();
    }

    /// <summary>
    /// Verifies that ExecuteHook does not override when DataTemplates are already present.
    /// </summary>
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

    /// <summary>
    /// Creates an observed change for the Items property of an ItemsControl.
    /// </summary>
    /// <param name="items">The items control instance.</param>
    /// <returns>An observed change representing the Items property.</returns>
    private static IObservedChange<object, object> ItemsObservedChange(ItemsControl items)
    {
        var param = Expression.Parameter(typeof(ItemsControl), "x");
        var member = Expression.Property(param, nameof(ItemsControl.Items));
        return new ObservedChange<object, object>(items, member, items.Items!);
    }
}
