// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Avalonia;
using ReactiveUI.Builder;
using Splat;

namespace ReactiveUI.Avalonia.Tests;

/// <summary>Tests for core AppBuilderExtensions behavior.</summary>
public class AppBuilderExtensionsTests
{
    /// <summary>Verifies that UseReactiveUI throws ArgumentNullException for a null builder.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UseReactiveUI_ThrowsOnNullBuilder()
    {
        AppBuilder? nullBuilder = null;
        await Assert.That(() => nullBuilder!.UseReactiveUI(_ => { })).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies that UseReactiveUI executes its platform setup callback and builds ReactiveUI when needed.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UseReactiveUI_AfterPlatformCallback_BuildsReactiveUI()
    {
        ReactiveUIBuilder.ResetBuilderStateForTests();
        var invoked = false;
        var builder = AppBuilder.Configure<Application>().UseReactiveUI(_ => invoked = true);

        InvokeAfterPlatformServicesSetup(builder);

        await Assert.That(invoked).IsTrue();
        await Assert.That(AppLocator.Current.GetService<IActivationForViewFetcher>()).IsTypeOf<AvaloniaActivationForViewFetcher>();
    }

    /// <summary>Verifies that UseReactiveUI executes the callback without rebuilding when ReactiveUI is already built.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task UseReactiveUI_AfterPlatformCallback_WhenAlreadyBuilt_ReturnsAfterUserCallback()
    {
        var invoked = false;
        var builder = AppBuilder.Configure<Application>().UseReactiveUI(_ => invoked = true);

        InvokeAfterPlatformServicesSetup(builder);

        await Assert.That(invoked).IsTrue();
    }

    /// <summary>Verifies that AppBuilderExtensions is a static class.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AppBuilderExtensions_IsStaticClass()
    {
        var type = typeof(AppBuilderExtensions);
        await Assert.That(type.IsClass).IsTrue();
        await Assert.That(type.IsAbstract).IsTrue();
        await Assert.That(type.IsSealed).IsTrue();
    }

    /// <summary>Verifies that WithAvalonia throws ArgumentNullException for a null builder.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithAvalonia_ThrowsOnNullBuilder()
    {
        IReactiveUIBuilder? builder = null;
        await Assert.That(() => builder!.WithAvalonia()).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies that WithAvalonia registers Avalonia-specific ReactiveUI services.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task WithAvalonia_Registers_Avalonia_Services()
    {
        var builder = AppLocator.CurrentMutable.CreateReactiveUIBuilder();
        _ = builder.WithAvalonia().BuildApp();

        await Assert.That(AppLocator.Current.GetService<IActivationForViewFetcher>()).IsTypeOf<AvaloniaActivationForViewFetcher>();
        await Assert.That(AppLocator.Current.GetService<IPropertyBindingHook>()).IsTypeOf<AutoDataTemplateBindingHook>();
        await Assert.That(AppLocator.Current.GetService<ICreatesCommandBinding>()).IsTypeOf<AvaloniaCreatesCommandBinding>();
        await Assert.That(AppLocator.Current.GetService<ICreatesObservableForProperty>()).IsTypeOf<AvaloniaObjectObservableForProperty>();
    }

    /// <summary>Invokes the AppBuilder platform setup callback registered by extension methods.</summary>
    /// <param name="builder">The application builder.</param>
    private static void InvokeAfterPlatformServicesSetup(AppBuilder builder)
    {
        var property = typeof(AppBuilder).GetProperty(
            "AfterPlatformServicesSetupCallback",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);

        var callback = (Action<AppBuilder>?)property?.GetValue(builder);
        callback?.Invoke(builder);
    }
}
