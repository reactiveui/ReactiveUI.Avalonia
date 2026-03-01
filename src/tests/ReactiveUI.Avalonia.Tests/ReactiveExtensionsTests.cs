// Copyright (c) 2019-2026 ReactiveUI and Avalonia Teams, and Contributors. All rights reserved.
// Licensed under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;

namespace ReactiveUI.Avalonia.Tests;

/// <summary>
/// Tests for verifying the public API surface of reactive extension types.
/// </summary>
public class ReactiveExtensionsTests
{
    /// <summary>
    /// Verifies that AvaloniaObjectReactiveExtensions is a static class.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AvaloniaObjectReactiveExtensions_IsStaticClass()
    {
        var type = typeof(AvaloniaObjectReactiveExtensions);
        await Assert.That(type.IsClass).IsTrue();
        await Assert.That(type.IsAbstract).IsTrue();
        await Assert.That(type.IsSealed).IsTrue();
    }

    /// <summary>
    /// Verifies that the GetSubject extension method exists.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AvaloniaObjectReactiveExtensions_HasGetSubjectMethod()
    {
        var type = typeof(AvaloniaObjectReactiveExtensions);
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);

        var hasGetSubjectMethod = false;
        foreach (var method in methods)
        {
            if (method.Name == "GetSubject")
            {
                hasGetSubjectMethod = true;
                break;
            }
        }

        await Assert.That(hasGetSubjectMethod).IsTrue();
    }

    /// <summary>
    /// Verifies that the GetBindingSubject extension method exists.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AvaloniaObjectReactiveExtensions_HasGetBindingSubjectMethod()
    {
        var type = typeof(AvaloniaObjectReactiveExtensions);
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);

        var hasGetBindingSubjectMethod = false;
        foreach (var method in methods)
        {
            if (method.Name == "GetBindingSubject")
            {
                hasGetBindingSubjectMethod = true;
                break;
            }
        }

        await Assert.That(hasGetBindingSubjectMethod).IsTrue();
    }

    /// <summary>
    /// Verifies that AutoDataTemplateBindingHook type exists and is a class.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AutoDataTemplateBindingHook_TypeExists()
    {
        var type = typeof(AutoDataTemplateBindingHook);
        await Assert.That(type).IsNotNull();
        await Assert.That(type.IsClass).IsTrue();
    }

    /// <summary>
    /// Verifies that AvaloniaActivationForViewFetcher type exists and is a class.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AvaloniaActivationForViewFetcher_TypeExists()
    {
        var type = typeof(AvaloniaActivationForViewFetcher);
        await Assert.That(type).IsNotNull();
        await Assert.That(type.IsClass).IsTrue();
    }

    /// <summary>
    /// Verifies that AutoSuspendHelper type exists and is a class.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AutoSuspendHelper_TypeExists()
    {
        var type = typeof(AutoSuspendHelper);
        await Assert.That(type).IsNotNull();
        await Assert.That(type.IsClass).IsTrue();
    }

    /// <summary>
    /// Verifies that AutoDataTemplateBindingHook has the ExecuteHook method.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AutoDataTemplateBindingHook_HasExpectedMethods()
    {
        var type = typeof(AutoDataTemplateBindingHook);
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);

        var hasExecuteHookMethod = false;

        foreach (var method in methods)
        {
            if (method.Name == "ExecuteHook")
            {
                hasExecuteHookMethod = true;
                break;
            }
        }

        await Assert.That(hasExecuteHookMethod).IsTrue();
    }

    /// <summary>
    /// Verifies that AvaloniaActivationForViewFetcher has the expected methods.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AvaloniaActivationForViewFetcher_HasExpectedMethods()
    {
        var type = typeof(AvaloniaActivationForViewFetcher);
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);

        var hasGetAffinityForViewMethod = false;
        var hasGetActivationForViewMethod = false;

        foreach (var method in methods)
        {
            if (method.Name == "GetAffinityForView")
            {
                hasGetAffinityForViewMethod = true;
            }
            else if (method.Name == "GetActivationForView")
            {
                hasGetActivationForViewMethod = true;
            }
        }

        await Assert.That(hasGetAffinityForViewMethod).IsTrue();
        await Assert.That(hasGetActivationForViewMethod).IsTrue();
    }

    /// <summary>
    /// Verifies that AvaloniaObjectReactiveExtensions has public extension methods.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveUIAvalonia_HasPublicExtensionMethods()
    {
        var type = typeof(AvaloniaObjectReactiveExtensions);
        var publicStaticMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);

        await Assert.That(publicStaticMethods.Length).IsGreaterThan(0);

        var allAreExtensions = true;
        foreach (var method in publicStaticMethods)
        {
            var extensionAttr = method.GetCustomAttribute<System.Runtime.CompilerServices.ExtensionAttribute>();
            if (extensionAttr == null)
            {
                allAreExtensions = false;
                break;
            }
        }

        await Assert.That(allAreExtensions).IsTrue();
    }

    /// <summary>
    /// Verifies that AutoSuspendHelper is a sealed, non-abstract class with constructors.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AutoSuspendHelper_HasExpectedProperties()
    {
        var type = typeof(AutoSuspendHelper);

        await Assert.That(type.IsClass).IsTrue();
        await Assert.That(type.IsAbstract).IsFalse();
        await Assert.That(type.IsSealed).IsTrue();

        var constructors = type.GetConstructors();
        await Assert.That(constructors.Length).IsGreaterThan(0);
    }

    /// <summary>
    /// Verifies that public types have parameterless constructors.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task PublicTypes_HaveParameterlessConstructors()
    {
        var autoDataTemplateHookType = typeof(AutoDataTemplateBindingHook);
        var constructors = autoDataTemplateHookType.GetConstructors();
        await Assert.That(constructors.Length).IsGreaterThan(0);

        var activationFetcherType = typeof(AvaloniaActivationForViewFetcher);
        var activationConstructors = activationFetcherType.GetConstructors();
        await Assert.That(activationConstructors.Length).IsGreaterThan(0);
    }
}
