// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
extern alias reactiveautofac;
extern alias reactivedryioc;
extern alias reactivemicrosoft;
extern alias reactiveninject;

namespace ReactiveUI.Avalonia.Reactive.Tests;

/// <summary>Tests for the ReactiveUI.Avalonia.*.Reactive sibling assemblies.</summary>
public class ReactiveShimProjectsTests
{
    /// <summary>Verifies that the core reactive assembly uses the Reactive namespace.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveCoreAssembly_UsesReactiveNamespace()
    {
        var schedulerType = typeof(AvaloniaScheduler);

        await Assert.That(schedulerType.Assembly.GetName().Name).IsEqualTo("ReactiveUI.Avalonia.Reactive");
        await Assert.That(schedulerType.Namespace).IsEqualTo("ReactiveUI.Avalonia.Reactive");
    }

    /// <summary>Verifies that each dependency injection reactive assembly uses the Reactive.Splat namespace.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveDependencyInjectionAssemblies_UseReactiveSplatNamespace()
    {
        var autofacType = typeof(reactiveautofac::ReactiveUI.Avalonia.Reactive.Splat.AvaloniaMixins);
        var dryIocType = typeof(reactivedryioc::ReactiveUI.Avalonia.Reactive.Splat.AvaloniaMixins);
        var microsoftType = typeof(reactivemicrosoft::ReactiveUI.Avalonia.Reactive.Splat.AvaloniaMixins);
        var ninjectType = typeof(reactiveninject::ReactiveUI.Avalonia.Reactive.Splat.AvaloniaMixins);

        await Assert.That(autofacType.Assembly.GetName().Name).IsEqualTo("ReactiveUI.Avalonia.Autofac.Reactive");
        await Assert.That(dryIocType.Assembly.GetName().Name).IsEqualTo("ReactiveUI.Avalonia.DryIoc.Reactive");
        await Assert.That(microsoftType.Assembly.GetName().Name).IsEqualTo("ReactiveUI.Avalonia.Microsoft.Extensions.DependencyInjection.Reactive");
        await Assert.That(ninjectType.Assembly.GetName().Name).IsEqualTo("ReactiveUI.Avalonia.Ninject.Reactive");

        await Assert.That(autofacType.Namespace).IsEqualTo("ReactiveUI.Avalonia.Reactive.Splat");
        await Assert.That(dryIocType.Namespace).IsEqualTo("ReactiveUI.Avalonia.Reactive.Splat");
        await Assert.That(microsoftType.Namespace).IsEqualTo("ReactiveUI.Avalonia.Reactive.Splat");
        await Assert.That(ninjectType.Namespace).IsEqualTo("ReactiveUI.Avalonia.Reactive.Splat");
    }
}
