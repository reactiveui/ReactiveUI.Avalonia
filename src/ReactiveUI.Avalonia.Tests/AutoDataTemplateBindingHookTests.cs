using System.Linq.Expressions;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using NUnit.Framework;
using ReactiveUI.Builder;
using Splat;

namespace ReactiveUI.Avalonia.Tests;

public class AutoDataTemplateBindingHookTests
{
    [Test]
    public void ExecuteHook_ItemsControlWithoutTemplate_SetsDefaultTemplate()
    {
        ReactiveUIBuilder.ResetBuilderStateForTests();
        AppLocator.CurrentMutable.CreateReactiveUIBuilder().WithCoreServices().BuildApp();
        var hook = new AutoDataTemplateBindingHook();
        var items = new ListBox();

        var result = hook.ExecuteHook(
            source: null,
            target: items,
            getCurrentViewModelProperties: () => [],
            getCurrentViewProperties: () => [ItemsObservedChange(items)],
            direction: BindingDirection.TwoWay);

        Assert.That(result, Is.True);
        Assert.That(items.ItemTemplate, Is.Not.Null);
    }

    [Test]
    public void ExecuteHook_WhenItemTemplateAlreadySet_DoesNotOverride()
    {
        ReactiveUIBuilder.ResetBuilderStateForTests();
        AppLocator.CurrentMutable.CreateReactiveUIBuilder().WithCoreServices().BuildApp();
        var hook = new AutoDataTemplateBindingHook();
        var items = new ListBox();
        items.ItemTemplate = new FuncDataTemplate<object>((_, _) => new TextBlock(), true);

        var res = hook.ExecuteHook(
            null,
            items,
            () => [],
            () => [ItemsObservedChange(items)],
            BindingDirection.OneWay);

        Assert.That(res, Is.True);
        Assert.That(items.ItemTemplate, Is.Not.Null);
    }

    [Test]
    public void ExecuteHook_WhenDataTemplatesPresent_DoesNotOverride()
    {
        ReactiveUIBuilder.ResetBuilderStateForTests();
        AppLocator.CurrentMutable.CreateReactiveUIBuilder().WithCoreServices().BuildApp();
        var hook = new AutoDataTemplateBindingHook();
        var items = new ListBox();
        items.DataTemplates.Add(new FuncDataTemplate<object>((_, _) => new TextBlock(), true));

        var res = hook.ExecuteHook(
            null,
            items,
            () => [],
            () => [ItemsObservedChange(items)],
            BindingDirection.OneWay);

        Assert.That(res, Is.True);
        Assert.That(items.ItemTemplate, Is.Null);
    }

    private static IObservedChange<object, object> ItemsObservedChange(ItemsControl items)
    {
        var param = Expression.Parameter(typeof(ItemsControl), "x");
        var member = Expression.Property(param, nameof(ItemsControl.Items));
        return new ObservedChange<object, object>(items, member, items.Items!);
    }
}
