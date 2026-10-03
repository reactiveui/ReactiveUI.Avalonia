// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Runtime.CompilerServices;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI.Avalonia.Example.Services;
using ReactiveUI.Avalonia.Example.ViewModels;
using ReactiveUI.Primitives.Disposables;
using ReactiveUI.Primitives.Signals;
using Unit = ReactiveUI.Primitives.RxVoid;

namespace ReactiveUI.Avalonia.Example.Tests;

/// <summary>Tests suspension persistence and deferral release against isolated temporary session files.</summary>
public sealed class ShowcaseSessionTests
{
    /// <summary>The prefix used for recoverable session storage errors.</summary>
    private const string StorageErrorPrefix = "Session storage:";

    /// <summary>The file name each test saves its session to.</summary>
    private const string SessionFileName = "session.json";

    /// <summary>Verifies a persistence request saves settings and the next launch restores them.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Persistence_Saves_And_Next_Launch_Restores_Settings()
    {
        const string savedInput = "Inspect working set";
        const double savedThreshold = 63;
        var directory = Path.Combine(Path.GetTempPath(), $"AvaloniaShowcase-{Guid.NewGuid():N}");
        var path = Path.Combine(directory, SessionFileName);
        try
        {
            using (var shell = MainViewModel.Create(new LocalMachineMetricsService()))
            {
                var lifetime = new ClassicDesktopStyleApplicationLifetime();
                using var session = new ShowcaseSession(lifetime, shell, path);
                session.Start();
                shell.Commands.WorkItemText = savedInput;
                shell.Metrics.CpuWarningThreshold = savedThreshold;
                var released = new StrongBox<bool>();
                session.Persist(Scope.Create(released, static state => state.Value = true));
                await Assert.That(released.Value).IsTrue();
                await Assert.That(File.Exists(path)).IsTrue();
                await Assert.That(shell.SessionStatus).IsEqualTo("Session saved.");
            }

            using (var shell = MainViewModel.Create(new LocalMachineMetricsService()))
            {
                using var session = new ShowcaseSession(new ClassicDesktopStyleApplicationLifetime(), shell, path);
                session.Start();
                await Assert.That(shell.Commands.WorkItemText).IsEqualTo(savedInput);
                await Assert.That(shell.Metrics.CpuWarningThreshold).IsEqualTo(savedThreshold);
            }

            await File.WriteAllTextAsync(path, "Invalid JSON");
            using var recovered = MainViewModel.Create(new LocalMachineMetricsService());
            using var recovery = new ShowcaseSession(new ClassicDesktopStyleApplicationLifetime(), recovered, path);
            recovery.Start();
            await Assert.That(recovered.SessionStatus).StartsWith(StorageErrorPrefix);
            await Assert.That(recovered.Commands.WorkItemText).IsEqualTo(string.Empty);
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }

    /// <summary>Verifies a null path or a lifetime that cannot be controlled leaves the session unavailable.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Without_Storage_Or_Controlled_Lifetime_Session_Is_Unavailable()
    {
        var (singleView, _) = AppTests.SingleViewLifetimeProxy.Create();
        using var noPathShell = MainViewModel.Create(new LocalMachineMetricsService());
        using var noPath = new ShowcaseSession(new ClassicDesktopStyleApplicationLifetime(), noPathShell, null);
        noPath.Start();
        await Assert.That(noPathShell.SessionStatus).IsEqualTo(ShowcaseSession.UnavailableStatus);

        using var uncontrolledShell = MainViewModel.Create(new LocalMachineMetricsService());
        using var uncontrolled = new ShowcaseSession(singleView, uncontrolledShell, "unused.json");
        uncontrolled.Start();
        await Assert.That(uncontrolledShell.SessionStatus).IsEqualTo(ShowcaseSession.UnavailableStatus);
    }

    /// <summary>Verifies the default path is absent in a browser and a session file otherwise.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task DefaultPath_Depends_On_Platform()
    {
        await Assert.That(ShowcaseSession.DefaultPath(isBrowser: true)).IsNull();
        await Assert.That(ShowcaseSession.DefaultPath(isBrowser: false)).EndsWith(SessionFileName);
        await Assert.That(ShowcaseSession.DefaultPath()).IsEqualTo(ShowcaseSession.DefaultPath(OperatingSystem.IsBrowser()));
    }

    /// <summary>Verifies an unhandled application exception deletes the saved session.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Unhandled_Exception_Deletes_Saved_Session()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"AvaloniaShowcase-{Guid.NewGuid():N}");
        var path = Path.Combine(directory, SessionFileName);
        _ = Directory.CreateDirectory(directory);
        await File.WriteAllTextAsync(path, "{}");
        try
        {
            using var shell = MainViewModel.Create(new LocalMachineMetricsService());
            using var invalidation = new Signal<Unit>();
            using var session = new ShowcaseSession(new ClassicDesktopStyleApplicationLifetime(), shell, path, invalidation);
            invalidation.OnNext(Unit.Default);
            await Assert.That(File.Exists(path)).IsFalse();
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    /// <summary>Verifies a saved session holding JSON null is reported as a storage error rather than restored.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Empty_Saved_Session_Reports_Storage_Error()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"AvaloniaShowcase-{Guid.NewGuid():N}");
        var path = Path.Combine(directory, SessionFileName);
        _ = Directory.CreateDirectory(directory);
        try
        {
            await File.WriteAllTextAsync(path, "null");
            using var shell = MainViewModel.Create(new LocalMachineMetricsService());
            using var session = new ShowcaseSession(new ClassicDesktopStyleApplicationLifetime(), shell, path);
            session.Start();

            await Assert.That(shell.SessionStatus).StartsWith(StorageErrorPrefix);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    /// <summary>Verifies a saved session without input text restores an empty input and the saved threshold.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Saved_Session_Without_Input_Restores_Empty_Input()
    {
        const double savedThreshold = 42;
        var directory = Path.Combine(Path.GetTempPath(), $"AvaloniaShowcase-{Guid.NewGuid():N}");
        var path = Path.Combine(directory, SessionFileName);
        _ = Directory.CreateDirectory(directory);
        try
        {
            await File.WriteAllTextAsync(path, "{\"CpuWarningThreshold\":42}");
            using var shell = MainViewModel.Create(new LocalMachineMetricsService());
            shell.Commands.WorkItemText = "Replaced by the restore";
            using var session = new ShowcaseSession(new ClassicDesktopStyleApplicationLifetime(), shell, path);
            session.Start();

            await Assert.That(shell.Commands.WorkItemText).IsEqualTo(string.Empty);
            await Assert.That(shell.Metrics.CpuWarningThreshold).IsEqualTo(savedThreshold);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    /// <summary>Verifies a failed save still releases the platform shutdown deferral.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Storage_Failure_Still_Releases_Shutdown_Deferral()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"AvaloniaShowcase-{Guid.NewGuid():N}");
        _ = Directory.CreateDirectory(directory);
        var invalidFile = Path.Combine(directory, "directory-instead-of-file");
        _ = Directory.CreateDirectory(invalidFile);
        try
        {
            using var shell = MainViewModel.Create(new LocalMachineMetricsService());
            var lifetime = new ClassicDesktopStyleApplicationLifetime();
            using var session = new ShowcaseSession(lifetime, shell, invalidFile);
            session.Start();
            var released = new StrongBox<bool>();
            session.Persist(Scope.Create(released, static state => state.Value = true));
            await Assert.That(released.Value).IsTrue();
            await Assert.That(shell.SessionStatus).StartsWith(StorageErrorPrefix);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    /// <summary>Verifies crash invalidation deletes valid state and reports a failed deletion.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Invalidation_Deletes_State_And_Reports_Storage_Failure()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"AvaloniaShowcase-{Guid.NewGuid():N}");
        _ = Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, SessionFileName);
        var invalidPath = Path.Combine(directory, "directory-instead-of-file");
        _ = Directory.CreateDirectory(invalidPath);
        await File.WriteAllTextAsync(path, "{}");
        try
        {
            using (var shell = MainViewModel.Create(new LocalMachineMetricsService()))
            using (var session = new ShowcaseSession(new ClassicDesktopStyleApplicationLifetime(), shell, path))
            {
                session.Invalidate();
                await Assert.That(File.Exists(path)).IsFalse();
            }

            using var failedShell = MainViewModel.Create(new LocalMachineMetricsService());
            using var failedSession = new ShowcaseSession(new ClassicDesktopStyleApplicationLifetime(), failedShell, invalidPath);
            failedSession.Invalidate();
            await Assert.That(failedShell.SessionStatus).StartsWith(StorageErrorPrefix);
            await Assert.That(Directory.Exists(invalidPath)).IsTrue();
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
