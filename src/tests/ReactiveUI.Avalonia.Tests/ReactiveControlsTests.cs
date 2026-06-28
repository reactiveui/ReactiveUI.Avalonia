// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Reflection;
using Avalonia.Controls;

using ActivationDisposables = ReactiveUI.Primitives.Disposables.MultipleDisposable;

namespace ReactiveUI.Avalonia.Tests;

/// <summary>Tests for reactive control types and their type hierarchy.</summary>
public class ReactiveControlsTests
{
    /// <summary>Verifies that ReactiveUserControl type can be referenced without setup.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveUserControl_CanBeCreatedWithoutSetup()
    {
        var controlType = typeof(ReactiveUserControl<object>);
        await Assert.That(controlType).IsNotNull();
        await Assert.That(controlType.IsClass).IsTrue();
    }

    /// <summary>Verifies that the non-generic user control activation callback can execute.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveUserControlBase_ActivationCallback_CanExecute()
    {
        var invoked = InvokeActivationCallback(typeof(ReactiveUserControlBase));

        await Assert.That(invoked).IsTrue();
    }

    /// <summary>Verifies that ReactiveWindow type can be referenced without setup.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveWindow_CanBeCreatedWithoutSetup()
    {
        var windowType = typeof(ReactiveWindow<object>);
        await Assert.That(windowType).IsNotNull();
        await Assert.That(windowType.IsClass).IsTrue();
    }

    /// <summary>Verifies that ViewModelViewHost type exists.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ViewModelViewHost_TypeExists()
    {
        var hostType = typeof(ViewModelViewHost);
        await Assert.That(hostType).IsNotNull();
        await Assert.That(hostType.IsClass).IsTrue();
    }

    /// <summary>Verifies that RoutedViewHost type exists.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RoutedViewHost_TypeExists()
    {
        var hostType = typeof(RoutedViewHost);
        await Assert.That(hostType).IsNotNull();
        await Assert.That(hostType.IsClass).IsTrue();
    }

    /// <summary>Verifies that ReactiveUserControl inherits from UserControl.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveUserControl_InheritsFromUserControl()
    {
        var controlType = typeof(ReactiveUserControl<object>);
        await Assert.That(controlType.BaseType).IsNotNull();

        var currentType = controlType.BaseType;
        var inheritsFromUserControl = false;
        while (currentType is not null && currentType != typeof(object))
        {
            if (currentType == typeof(UserControl) ||
                currentType.Name.Contains("UserControl"))
            {
                inheritsFromUserControl = true;
                break;
            }

            currentType = currentType.BaseType;
        }

        await Assert.That(inheritsFromUserControl).IsTrue();
    }

    /// <summary>Verifies that ReactiveWindow inherits from Window.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveWindow_InheritsFromWindow()
    {
        var windowType = typeof(ReactiveWindow<object>);
        await Assert.That(windowType.BaseType).IsNotNull();

        var currentType = windowType.BaseType;
        var inheritsFromWindow = false;
        while (currentType is not null && currentType != typeof(object))
        {
            if (currentType == typeof(Window) ||
                currentType.Name.Contains("Window"))
            {
                inheritsFromWindow = true;
                break;
            }

            currentType = currentType.BaseType;
        }

        await Assert.That(inheritsFromWindow).IsTrue();
    }

    /// <summary>Verifies that ViewModelViewHost inherits from ContentControl.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ViewModelViewHost_InheritsFromContentControl()
    {
        var hostType = typeof(ViewModelViewHost);
        await Assert.That(hostType.BaseType).IsNotNull();

        var currentType = hostType.BaseType;
        var inheritsFromContentControl = false;
        while (currentType is not null && currentType != typeof(object))
        {
            if (currentType == typeof(ContentControl) ||
                currentType.Name.Contains("ContentControl"))
            {
                inheritsFromContentControl = true;
                break;
            }

            currentType = currentType.BaseType;
        }

        await Assert.That(inheritsFromContentControl).IsTrue();
    }

    /// <summary>Verifies that RoutedViewHost inherits from ContentControl.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RoutedViewHost_InheritsFromContentControl()
    {
        var hostType = typeof(RoutedViewHost);
        await Assert.That(hostType.BaseType).IsNotNull();

        var currentType = hostType.BaseType;
        var inheritsFromContentControl = false;
        while (currentType is not null && currentType != typeof(object))
        {
            if (currentType == typeof(ContentControl) ||
                currentType.Name.Contains("ContentControl"))
            {
                inheritsFromContentControl = true;
                break;
            }

            currentType = currentType.BaseType;
        }

        await Assert.That(inheritsFromContentControl).IsTrue();
    }

    /// <summary>Verifies that ReactiveUserControl is a generic type definition with one type argument.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveUserControl_IsGeneric()
    {
        var controlType = typeof(ReactiveUserControl<>);
        await Assert.That(controlType.IsGenericTypeDefinition).IsTrue();

        var genericArguments = controlType.GetGenericArguments();
        await Assert.That(genericArguments.Length).IsEqualTo(1);
    }

    /// <summary>Verifies that ReactiveWindow is a generic type definition with one type argument.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveWindow_IsGeneric()
    {
        var windowType = typeof(ReactiveWindow<>);
        await Assert.That(windowType.IsGenericTypeDefinition).IsTrue();

        var genericArguments = windowType.GetGenericArguments();
        await Assert.That(genericArguments.Length).IsEqualTo(1);
    }

    /// <summary>Verifies that ViewModelViewHost has the expected public properties.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ViewModelViewHost_HasExpectedProperties()
    {
        var hostType = typeof(ViewModelViewHost);

        var viewModelProp = hostType.GetProperty("ViewModel");
        await Assert.That(viewModelProp).IsNotNull();

        var defaultContentProp = hostType.GetProperty("DefaultContent");
        await Assert.That(defaultContentProp).IsNotNull();

        var viewContractProp = hostType.GetProperty("ViewContract");
        await Assert.That(viewContractProp).IsNotNull();
    }

    /// <summary>Verifies that RoutedViewHost has the expected public properties.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RoutedViewHost_HasExpectedProperties()
    {
        var hostType = typeof(RoutedViewHost);

        var defaultContentProp = hostType.GetProperty("DefaultContent");
        await Assert.That(defaultContentProp).IsNotNull();

        var viewContractProp = hostType.GetProperty("ViewContract");
        await Assert.That(viewContractProp).IsNotNull();

        var routerProp = hostType.GetProperty("Router");
        await Assert.That(routerProp).IsNotNull();
    }

    /// <summary>Invokes a compiler-generated activation callback.</summary>
    /// <param name="viewType">The view base type that owns the callback.</param>
    /// <returns><see langword="true"/> after the callback has been invoked.</returns>
    private static bool InvokeActivationCallback(Type viewType)
    {
        var closureType = viewType.GetNestedTypes(BindingFlags.NonPublic)
            .Single(type => type.Name.Contains("<>c", StringComparison.Ordinal));
        var instance = closureType.GetField("<>9", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
            ?.GetValue(null);
        var method = closureType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
            .Single(candidate =>
                candidate.GetParameters() is [{ ParameterType: var parameterType }] &&
                parameterType == typeof(ActivationDisposables));

        using var disposables = new ActivationDisposables();
        _ = method.Invoke(instance, [disposables]);

        return true;
    }
}
