// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI.Avalonia.Splat;

namespace ReactiveUI.Avalonia.Microsoft.Tests;

/// <summary>Additional tests for Microsoft dependency injection-based Avalonia mixin registration.</summary>
public class AvaloniaMixinsMicrosoftMoreTests
{
    /// <summary>Verifies that <see cref="AvaloniaMixins.UseReactiveUIWithMicrosoftDependencyResolver"/> with the overload accepting container config returns the same builder instance.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test.</returns>
    [Test]
    public async Task UseReactiveUIWithMicrosoftDependencyResolver_Overload_Returns_Builder()
    {
        var builder = AppBuilder.Configure<Application>();
        var result = AvaloniaMixins.UseReactiveUIWithMicrosoftDependencyResolver(
            builder,
            containerConfig: sc => _ = sc.AddSingleton(new object()),
            withResolver: _ => { },
            withReactiveUIBuilder: _ => { });

        await Assert.That(result).IsSameReferenceAs(builder);
    }

    /// <summary>Verifies that <see cref="AvaloniaMixins.UseReactiveUIWithMicrosoftDependencyResolver"/> returns the same builder instance without throwing.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test.</returns>
    [Test]
    public async Task UseReactiveUIWithMicrosoftDependencyResolver_Returns_Builder_NoThrow()
    {
        var builder = AppBuilder.Configure<Application>();
        var result = AvaloniaMixins.UseReactiveUIWithMicrosoftDependencyResolver(
            builder,
            sc => _ = sc.AddSingleton(new object()),
            _ => { });

        await Assert.That(result).IsSameReferenceAs(builder);
    }
}
