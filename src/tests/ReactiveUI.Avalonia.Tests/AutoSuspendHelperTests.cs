// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Reflection;
using System.Reflection.Emit;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace ReactiveUI.Avalonia.Tests;

/// <summary>Tests for the AutoSuspendHelper initialization and event wiring.</summary>
public class AutoSuspendHelperTests
{
    /// <summary>Verifies that the constructor throws on a null lifetime argument.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Ctor_With_Null_Lifetime_Throws()
    {
        await Assert.That(() => new AutoSuspendHelper(null!)).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies that the constructor throws for unsupported non-null lifetimes.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Ctor_With_Unsupported_Lifetime_Throws()
    {
        await Assert.That(() => new AutoSuspendHelper(CreateUnsupportedLifetime())).ThrowsExactly<NotSupportedException>();
    }

    /// <summary>Verifies that design mode bypasses lifetime-specific exit wiring.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Ctor_In_DesignMode_Uses_NeverPersistState()
    {
        var previous = Design.IsDesignMode;
        SetDesignMode(true);

        try
        {
            using var helper = new AutoSuspendHelper(CreateUnsupportedLifetime());

            var notified = false;
            using var sub = RxSuspension.SuspensionHost.ShouldPersistState.SubscribeSafe(
                d =>
                {
                    notified = true;
                    d.Dispose();
                },
                static error => throw error);

            await Assert.That(notified).IsFalse();
        }
        finally
        {
            SetDesignMode(previous);
        }
    }

    /// <summary>Verifies that the constructor with a desktop lifetime sets ShouldPersistState.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Ctor_With_DesktopLifetime_Sets_ShouldPersistState()
    {
        var lifetime = new ClassicDesktopStyleApplicationLifetime();
        using var helper = new AutoSuspendHelper(lifetime);

        var notified = false;
        var sub = RxSuspension.SuspensionHost.ShouldPersistState.SubscribeSafe(
            d =>
            {
                notified = true;
                d.Dispose();
            },
            static error => throw error);

        lifetime.Shutdown(0);

        await Assert.That(notified).IsTrue();
        sub.Dispose();
    }

    /// <summary>Verifies that OnFrameworkInitializationCompleted pushes an IsLaunchingNew notification.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OnFrameworkInitializationCompleted_Pushes_IsLaunchingNew()
    {
        var lifetime = new ClassicDesktopStyleApplicationLifetime();
        using var helper = new AutoSuspendHelper(lifetime);

        var count = 0;
        var sub = RxSuspension.SuspensionHost.IsLaunchingNew.SubscribeSafe(_ => count++, static error => throw error);

        helper.OnFrameworkInitializationCompleted();

        await Assert.That(count).IsEqualTo(1);
        sub.Dispose();
    }

    /// <summary>Verifies that unhandled exception notifications invalidate persisted state.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task OnUnhandledException_Pushes_ShouldInvalidateState()
    {
        var lifetime = new ClassicDesktopStyleApplicationLifetime();
        using var helper = new AutoSuspendHelper(lifetime);

        var count = 0;
        using var sub = RxSuspension.SuspensionHost.ShouldInvalidateState.SubscribeSafe(
            _ => count++,
            static error => throw error);

        helper.OnUnhandledException(this, new UnhandledExceptionEventArgs(new InvalidOperationException("expected"), isTerminating: false));

        await Assert.That(count).IsEqualTo(1);
    }

    /// <summary>Creates a runtime-only lifetime implementation to exercise unsupported lifetime behavior.</summary>
    /// <returns>An unsupported application lifetime.</returns>
    private static IApplicationLifetime CreateUnsupportedLifetime()
    {
        var assemblyName = new AssemblyName("ReactiveUI.Avalonia.Tests.DynamicLifetime");
        var assembly = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        var module = assembly.DefineDynamicModule("Main");
        var type = module.DefineType("UnsupportedLifetime", TypeAttributes.NotPublic | TypeAttributes.Sealed);
        type.AddInterfaceImplementation(typeof(IApplicationLifetime));

        var lifetimeType = type.CreateType();
        return (IApplicationLifetime)Activator.CreateInstance(lifetimeType)!;
    }

    /// <summary>Sets the runtime design-mode flag whose public reference metadata exposes only a getter.</summary>
    /// <param name="isDesignMode">The design-mode value.</param>
    private static void SetDesignMode(bool isDesignMode)
    {
        var property = typeof(Design).GetProperty(
            nameof(Design.IsDesignMode),
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

        property?.SetMethod?.Invoke(null, [isDesignMode]);
    }
}
