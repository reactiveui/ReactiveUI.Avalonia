// Copyright (c) 2019-2026 ReactiveUI and Avalonia Teams, and Contributors. All rights reserved.
// Licensed under the MIT license.
// See the LICENSE file in the project root for full license information.

using Avalonia;
using ReactiveUI.Builder;
using Splat;

namespace ReactiveUI.Avalonia.Tests;

/// <summary>
/// Tests for core AppBuilderExtensions behavior.
/// </summary>
public class AppBuilderExtensionsTests
{
    /// <summary>
    /// Verifies that UseReactiveUI throws ArgumentNullException for a null builder.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UseReactiveUI_ThrowsOnNullBuilder()
    {
        AppBuilder? nullBuilder = null;
        await Assert.That(() => nullBuilder!.UseReactiveUI(_ => { })).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that AppBuilderExtensions is a static class.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AppBuilderExtensions_IsStaticClass()
    {
        var type = typeof(AppBuilderExtensions);
        await Assert.That(type.IsClass).IsTrue();
        await Assert.That(type.IsAbstract).IsTrue();
        await Assert.That(type.IsSealed).IsTrue();
    }

    /// <summary>
    /// Verifies that WithAvalonia throws ArgumentNullException for a null builder.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithAvalonia_ThrowsOnNullBuilder()
    {
        IReactiveUIBuilder? builder = null;
        await Assert.That(() => builder!.WithAvalonia()).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that WithAvalonia registers Avalonia-specific ReactiveUI services.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithAvalonia_Registers_Avalonia_Services()
    {
        var builder = AppLocator.CurrentMutable.CreateReactiveUIBuilder();
        builder.WithAvalonia().BuildApp();

        await Assert.That(AppLocator.Current.GetService<IActivationForViewFetcher>()).IsTypeOf<AvaloniaActivationForViewFetcher>();
        await Assert.That(AppLocator.Current.GetService<IPropertyBindingHook>()).IsTypeOf<AutoDataTemplateBindingHook>();
        await Assert.That(AppLocator.Current.GetService<ICreatesCommandBinding>()).IsTypeOf<AvaloniaCreatesCommandBinding>();
        await Assert.That(AppLocator.Current.GetService<ICreatesObservableForProperty>()).IsTypeOf<AvaloniaObjectObservableForProperty>();
    }
}
