// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI.Avalonia.Example.Services;
using ReactiveUI.Avalonia.Example.Views;
using Splat;

namespace ReactiveUI.Avalonia.Example.Tests;

/// <summary>Tests how <see cref="App"/> composes the shell for each application lifetime.</summary>
public sealed class AppTests
{
    /// <summary>Verifies a single-view lifetime receives a <see cref="MainView"/> bound to the shell view model.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AttachShell_SingleViewLifetime_Creates_MainView_With_Shell_ViewModel()
    {
        var (lifetime, recorder) = SingleViewLifetimeProxy.Create();

        App.AttachShell(lifetime);

        await Assert.That(recorder.MainView).IsTypeOf<MainView>();
        var shell = (recorder.MainView as MainView)?.ViewModel;
        await Assert.That(shell).IsNotNull();
        await Assert.That(recorder.MainView!.DataContext).IsSameReferenceAs(shell);
        await Assert.That(shell!.SessionStatus).IsEqualTo(ShowcaseSession.UnavailableStatus);
        shell.Dispose();
    }

    /// <summary>Verifies a desktop lifetime receives a <see cref="MainWindow"/> bound to the shell and releases its session on exit.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AttachShell_DesktopLifetime_Creates_MainWindow_With_Shell_ViewModel()
    {
        const string DataHomeVariable = "XDG_DATA_HOME";
        var previousDataHome = Environment.GetEnvironmentVariable(DataHomeVariable);
        var directory = Path.Combine(Path.GetTempPath(), $"AvaloniaShowcase-{Guid.NewGuid():N}");
        Environment.SetEnvironmentVariable(DataHomeVariable, directory);
        try
        {
            var (lifetime, recorder) = DesktopLifetimeProxy.Create();

            App.AttachShell(lifetime);

            var window = recorder.MainWindow as MainWindow;
            await Assert.That(window).IsNotNull();
            var shell = window!.ViewModel;
            await Assert.That(shell).IsNotNull();
            await Assert.That(window.DataContext).IsSameReferenceAs(shell);
            await Assert.That(shell!.SessionStatus).IsNotEqualTo(ShowcaseSession.UnavailableStatus);

            recorder.RaiseExit();
        }
        finally
        {
            Environment.SetEnvironmentVariable(DataHomeVariable, previousDataHome);
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }

    /// <summary>Verifies attaching the shell fails clearly when the metrics service is not registered.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AttachShell_Without_Metrics_Service_Throws()
    {
        var resolver = new ModernDependencyResolver();
        await Assert.That(() => App.CreateShell(resolver)).ThrowsExactly<InvalidOperationException>();
    }

    /// <summary>Verifies the metrics service matches the platform.</summary>
    /// <param name="isBrowser">Whether the platform is a browser.</param>
    /// <param name="expected">The expected service type.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [Arguments(true, typeof(BrowserMachineMetricsService))]
    [Arguments(false, typeof(LocalMachineMetricsService))]
    public async Task CreateMetricsService_Matches_Platform(bool isBrowser, Type expected)
    {
        var service = App.CreateMetricsService(isBrowser);
        await Assert.That(service.GetType()).IsEqualTo(expected);
        (service as IDisposable)?.Dispose();
    }

    /// <summary>Verifies no lifetime leaves the app without a shell and without error.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AttachShell_NoLifetime_Does_Nothing() =>
        await Assert.That(static () => App.AttachShell(null)).ThrowsNothing();

    /// <summary>Stands in for the desktop lifetime so the test can raise exit without shutting down the shared dispatcher.</summary>
    [System.Diagnostics.DebuggerDisplay("DesktopLifetimeProxy: {MainWindow}")]
    public class DesktopLifetimeProxy : DispatchProxy
    {
        /// <summary>The method name that subscribes to the exit event.</summary>
        private const string AddExitName = "add_Exit";

        /// <summary>The method name that unsubscribes from the exit event.</summary>
        private const string RemoveExitName = "remove_Exit";

        /// <summary>The setter method name of <see cref="IClassicDesktopStyleApplicationLifetime.MainWindow"/>.</summary>
        private const string SetMainWindowName = "set_MainWindow";

        /// <summary>The getter method name of <see cref="IClassicDesktopStyleApplicationLifetime.MainWindow"/>.</summary>
        private const string GetMainWindowName = "get_MainWindow";

        /// <summary>The handlers subscribed to the exit event.</summary>
        private EventHandler<ControlledApplicationLifetimeExitEventArgs>? _exit;

        /// <summary>Gets or sets the window the app assigned.</summary>
        public Window? MainWindow { get; set; }

        /// <summary>Creates a proxy that implements the desktop lifetime.</summary>
        /// <returns>The lifetime and the proxy that records the main window.</returns>
        public static (IClassicDesktopStyleApplicationLifetime Lifetime, DesktopLifetimeProxy Recorder) Create()
        {
            var lifetime = Create<IClassicDesktopStyleApplicationLifetime, DesktopLifetimeProxy>();
            return (lifetime, (DesktopLifetimeProxy)(object)lifetime);
        }

        /// <summary>Raises the exit event the way a closing desktop lifetime does.</summary>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void RaiseExit() => _exit?.Invoke(this, new ControlledApplicationLifetimeExitEventArgs(0));

        /// <inheritdoc/>
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            switch (targetMethod?.Name)
            {
                case AddExitName:
                    {
                        _exit += (EventHandler<ControlledApplicationLifetimeExitEventArgs>?)args?[0];
                        return null;
                    }

                case RemoveExitName:
                    {
                        _exit -= (EventHandler<ControlledApplicationLifetimeExitEventArgs>?)args?[0];
                        return null;
                    }

                case SetMainWindowName:
                    {
                        MainWindow = (Window?)args?[0];
                        return null;
                    }

                case GetMainWindowName:
                    {
                        return MainWindow;
                    }

                default:
                    {
                        throw new NotSupportedException(targetMethod?.Name);
                    }
            }
        }
    }

    /// <summary>Stands in for the single-view lifetime used by browser, Android and iOS heads.</summary>
    /// <remarks>Avalonia seals the lifetime interface against user implementations, so a runtime proxy records the view.</remarks>
    [System.Diagnostics.DebuggerDisplay("SingleViewLifetimeProxy: {MainView}")]
    public class SingleViewLifetimeProxy : DispatchProxy
    {
        /// <summary>The setter method name of <see cref="ISingleViewApplicationLifetime.MainView"/>.</summary>
        private const string SetMainViewName = "set_MainView";

        /// <summary>The getter method name of <see cref="ISingleViewApplicationLifetime.MainView"/>.</summary>
        private const string GetMainViewName = "get_MainView";

        /// <summary>Gets or sets the view the app assigned.</summary>
        public Control? MainView { get; set; }

        /// <summary>Creates a proxy that implements the single-view lifetime.</summary>
        /// <returns>The lifetime and the proxy that records the main view.</returns>
        public static (ISingleViewApplicationLifetime Lifetime, SingleViewLifetimeProxy Recorder) Create()
        {
            var lifetime = Create<ISingleViewApplicationLifetime, SingleViewLifetimeProxy>();
            return (lifetime, (SingleViewLifetimeProxy)(object)lifetime);
        }

        /// <inheritdoc/>
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            switch (targetMethod?.Name)
            {
                case SetMainViewName:
                    {
                        MainView = (Control?)args?[0];
                        return null;
                    }

                case GetMainViewName:
                    {
                        return MainView;
                    }

                default:
                    {
                        throw new NotSupportedException(targetMethod?.Name);
                    }
            }
        }
    }
}
