// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI.Avalonia.Example.Services;
using ReactiveUI.Avalonia.Example.ViewModels;
using ReactiveUI.Avalonia.Example.Views;
using ReactiveUI.Avalonia.Splat;
using Splat;

namespace ReactiveUI.Avalonia.Example;

/// <summary>The executable entry point for the example app.</summary>
public static class Program
{
    /// <summary>Runs the example app.</summary>
    /// <param name="args">Command-line arguments.</param>
    [STAThread]
    public static void Main(string[] args) => _ = BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    /// <summary>Builds the Avalonia app with ReactiveUI and Microsoft dependency resolver integration.</summary>
    /// <returns>The configured app builder.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [RequiresUnreferencedCode("The desktop showcase uses expression-based ReactiveUI bindings.")]
    [RequiresDynamicCode("The desktop showcase uses dynamic ReactiveUI bindings.")]
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure(static () => new App { MainWindowFactory = CreateMainWindow })
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUIWithMicrosoftDependencyResolver(
                static _ => App.RegisterViews(AppLocator.CurrentMutable),
                null,
                static builder => builder.WithCoreServices());

    /// <summary>Creates the main window after application services have been registered.</summary>
    /// <param name="lifetime">The desktop lifetime.</param>
    /// <returns>The initialized desktop window.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <c>AppLocator.Current.GetService&lt;ILocalMachineMetricsService&gt;()</c> is <see langword="null"/>.</exception>
    [RequiresUnreferencedCode("The window demonstrates expression-based ReactiveUI bindings.")]
    [RequiresDynamicCode("The window demonstrates dynamic ReactiveUI bindings.")]
    private static MainWindow CreateMainWindow(IClassicDesktopStyleApplicationLifetime lifetime)
    {
        var shell = MainViewModel.Create(AppLocator.Current.GetService<ILocalMachineMetricsService>()
            ?? throw new InvalidOperationException("The local metrics service has not been registered."));
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ReactiveUI.Avalonia.Example", "session.json");
        var session = new ShowcaseSession(lifetime, shell, path);
        lifetime.Exit += (_, _) => session.Dispose();
        session.Start();
        return new MainWindow { ViewModel = shell };
    }
}
