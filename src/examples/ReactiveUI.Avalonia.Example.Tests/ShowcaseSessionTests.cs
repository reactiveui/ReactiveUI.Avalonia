// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Runtime.CompilerServices;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI.Avalonia.Example.Services;
using ReactiveUI.Avalonia.Example.ViewModels;
using ReactiveUI.Primitives.Disposables;

namespace ReactiveUI.Avalonia.Example.Tests;

/// <summary>Tests suspension persistence and deferral release against isolated temporary session files.</summary>
public sealed class ShowcaseSessionTests
{
    /// <summary>Verifies a persistence request saves settings and the next launch restores them.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Persistence_Saves_And_Next_Launch_Restores_Settings()
    {
        const string savedInput = "Inspect working set";
        const double savedThreshold = 63;
        var directory = Path.Combine(Path.GetTempPath(), $"AvaloniaShowcase-{Guid.NewGuid():N}");
        var path = Path.Combine(directory, "session.json");
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
            await Assert.That(recovered.SessionStatus).StartsWith("Session storage:");
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
            await Assert.That(shell.SessionStatus).StartsWith("Session storage:");
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
