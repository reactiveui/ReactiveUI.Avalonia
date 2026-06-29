// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Reflection;
using ReflectionAssembly = System.Reflection.Assembly;

namespace ReactiveUI.Avalonia.Tests;

/// <summary>Tests for verifying the ReactiveUI.Avalonia assembly metadata and type discovery.</summary>
public class DependencyInjectionTests
{
    /// <summary>Verifies that the ReactiveUI.Avalonia assembly is loaded.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveUIAvaloniaAssembly_IsLoaded()
    {
        var assembly = ReflectionAssembly.GetAssembly(typeof(AvaloniaScheduler));
        await Assert.That(assembly).IsNotNull();
        await Assert.That(assembly!.FullName!).Contains("ReactiveUI.Avalonia");
    }

    /// <summary>Verifies that the assembly contains the expected public types.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveUIAvaloniaAssembly_HasExpectedTypes()
    {
        var assembly = ReflectionAssembly.GetAssembly(typeof(AvaloniaScheduler));
        await Assert.That(assembly).IsNotNull();
        var types = assembly!.GetTypes();

        var hasAvaloniaScheduler = false;
        var hasReactiveUserControl = false;
        var hasReactiveWindow = false;
        var hasViewModelViewHost = false;

        foreach (var type in types)
        {
            if (type.Name == "AvaloniaScheduler")
            {
                hasAvaloniaScheduler = true;
            }
            else if (type.Name.StartsWith("ReactiveUserControl"))
            {
                hasReactiveUserControl = true;
            }
            else if (type.Name.StartsWith("ReactiveWindow"))
            {
                hasReactiveWindow = true;
            }
            else if (type.Name == "ViewModelViewHost")
            {
                hasViewModelViewHost = true;
            }
        }

        await Assert.That(hasAvaloniaScheduler).IsTrue();
        await Assert.That(hasReactiveUserControl).IsTrue();
        await Assert.That(hasReactiveWindow).IsTrue();
        await Assert.That(hasViewModelViewHost).IsTrue();
    }

    /// <summary>Verifies that types are in the correct namespace.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveUIAvaloniaNamespace_IsCorrect()
    {
        var scheduler = typeof(AvaloniaScheduler);
        await Assert.That(scheduler.Namespace).IsEqualTo("ReactiveUI.Avalonia");

        var userControl = typeof(ReactiveUserControl<>);
        await Assert.That(userControl.Namespace).IsEqualTo("ReactiveUI.Avalonia");

        var window = typeof(ReactiveWindow<>);
        await Assert.That(window.Namespace).IsEqualTo("ReactiveUI.Avalonia");
    }

    /// <summary>Verifies that the assembly has expected metadata.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AssemblyInfo_HasExpectedMetadata()
    {
        var assembly = ReflectionAssembly.GetAssembly(typeof(AvaloniaScheduler));
        var assemblyName = assembly!.GetName();

        await Assert.That(assemblyName.Name).IsEqualTo("ReactiveUI.Avalonia");
        await Assert.That(assemblyName.Version).IsNotNull();
    }

    /// <summary>Verifies that main types are public.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task PublicTypes_ArePublic()
    {
        var scheduler = typeof(AvaloniaScheduler);
        await Assert.That(scheduler.IsPublic).IsTrue();

        var userControl = typeof(ReactiveUserControl<>);
        await Assert.That(userControl.IsPublic).IsTrue();

        var window = typeof(ReactiveWindow<>);
        await Assert.That(window.IsPublic).IsTrue();

        var viewHost = typeof(ViewModelViewHost);
        await Assert.That(viewHost.IsPublic).IsTrue();
    }

    /// <summary>Verifies that extension method types are public, sealed, and abstract (static).</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExtensionMethods_ArePublicAndStatic()
    {
        var appBuilderExts = typeof(AppBuilderExtensions);
        await Assert.That(appBuilderExts.IsPublic).IsTrue();
        await Assert.That(appBuilderExts.IsSealed).IsTrue();
        await Assert.That(appBuilderExts.IsAbstract).IsTrue();

        var avaloniaExts = typeof(AvaloniaObjectReactiveExtensions);
        await Assert.That(avaloniaExts.IsPublic).IsTrue();
        await Assert.That(avaloniaExts.IsSealed).IsTrue();
        await Assert.That(avaloniaExts.IsAbstract).IsTrue();
    }

    /// <summary>Verifies that the assembly references expected dependencies.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AssemblyReferences_ContainExpectedDependencies()
    {
        var assembly = ReflectionAssembly.GetAssembly(typeof(AvaloniaScheduler));
        var referencedAssemblies = assembly!.GetReferencedAssemblies();

        var hasAvaloniaBase = false;
        var hasReactiveUI = false;
        var hasSystemReactive = false;

        foreach (var refAssembly in referencedAssemblies)
        {
            if (refAssembly.Name!.StartsWith("Avalonia.Base") || refAssembly.Name.StartsWith("Avalonia"))
            {
                hasAvaloniaBase = true;
            }
            else if (refAssembly.Name.StartsWith("ReactiveUI"))
            {
                hasReactiveUI = true;
            }
            else if (refAssembly.Name.Contains("System.Reactive"))
            {
                hasSystemReactive = true;
            }
        }

        await Assert.That(hasAvaloniaBase).IsTrue();
        await Assert.That(hasReactiveUI).IsTrue();
        await Assert.That(hasSystemReactive).IsFalse();
    }
}
