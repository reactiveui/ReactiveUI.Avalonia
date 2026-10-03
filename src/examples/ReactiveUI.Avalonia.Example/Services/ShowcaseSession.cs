// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Runtime.CompilerServices;
using System.Text.Json;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using ReactiveUI.Avalonia.Example.Models;
using ReactiveUI.Avalonia.Example.ViewModels;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Disposables;
using Unit = ReactiveUI.Primitives.RxVoid;

namespace ReactiveUI.Avalonia.Example.Services;

/// <summary>Composes Avalonia suspension notifications with a small local JSON settings store.</summary>
[System.Diagnostics.DebuggerDisplay("ShowcaseSession: {_shell}")]
public sealed class ShowcaseSession : IDisposable
{
    /// <summary>The status shown when the platform has no session storage.</summary>
    public static readonly string UnavailableStatus ="Session storage is not available on this platform. Input and threshold reset on each launch.";

    /// <summary>The view model whose settings are persisted.</summary>
    private readonly MainViewModel _shell;

    /// <summary>The settings file path. It is empty, and never used, when the platform has no session storage.</summary>
    private readonly string _path = string.Empty;

    /// <summary>The platform lifetime adapter, or <see langword="null"/> when the session is not persisted.</summary>
    private readonly AutoSuspendHelper? _suspension;

    /// <summary>The lifetime subscriptions.</summary>
    private readonly MultipleDisposable _subscriptions = new();

    /// <summary>Initializes a new instance of the <see cref="ShowcaseSession"/> class.</summary>
    /// <param name="lifetime">The running application lifetime. Only a controlled lifetime can persist a session.</param>
    /// <param name="shell">The shell to persist.</param>
    /// <param name="path">The settings file path, or <see langword="null"/> when the platform has no session storage.</param>
    public ShowcaseSession(IApplicationLifetime lifetime, MainViewModel shell, string? path)
        : this(lifetime, shell, path, null)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="ShowcaseSession"/> class.</summary>
    /// <param name="lifetime">The running application lifetime. Only a controlled lifetime can persist a session.</param>
    /// <param name="shell">The shell to persist.</param>
    /// <param name="path">The settings file path, or <see langword="null"/> when the platform has no session storage.</param>
    /// <param name="invalidation">Signals that saved state is invalid, or <see langword="null"/> to use the suspension host.</param>
    internal ShowcaseSession(IApplicationLifetime lifetime, MainViewModel shell, string? path, IObservable<Unit>? invalidation)
    {
        _shell = shell;
        if (path is null || lifetime is not IControlledApplicationLifetime)
        {
            return;
        }

        _path = path;
        _suspension = new(lifetime);
        _subscriptions.Add(RxSuspension.SuspensionHost.IsLaunchingNew.SubscribeSafe(_ => Restore(), ReportFailure));
        _subscriptions.Add(RxSuspension.SuspensionHost.ShouldPersistState.SubscribeSafe(Persist, ReportFailure));
        _subscriptions.Add((invalidation ?? RxSuspension.SuspensionHost.ShouldInvalidateState).SubscribeSafe(_ => Invalidate(), ReportFailure));
    }

    /// <summary>Gets the default settings file path, or <see langword="null"/> in a browser, which has no file storage.</summary>
    /// <returns>The settings file path, or <see langword="null"/> when the platform has no session storage.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? DefaultPath() => DefaultPath(OperatingSystem.IsBrowser());

    /// <summary>Signals startup after the shell and its services have been composed.</summary>
    public void Start()
    {
        if (_suspension is null)
        {
            _shell.SessionStatus = UnavailableStatus;
            return;
        }

        _suspension.OnFrameworkInitializationCompleted();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _subscriptions.Dispose();
        _suspension?.Dispose();
    }

    /// <summary>Gets the default settings file path for a platform.</summary>
    /// <param name="isBrowser">Whether the app runs in a browser.</param>
    /// <returns>The settings file path, or <see langword="null"/> in a browser.</returns>
    internal static string? DefaultPath(bool isBrowser) => isBrowser
        ? null
        : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ReactiveUI.Avalonia.Example", "session.json");

    /// <summary>Saves state and always releases the shutdown deferral.</summary>
    /// <param name="deferral">The token allowing platform shutdown to finish.</param>
    internal void Persist(IDisposable deferral)
    {
        using (deferral)
        {
            try
            {
                var state = new ShowcaseState(_shell.Commands.WorkItemText, _shell.Metrics.CpuWarningThreshold);
                _ = Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(_path))!);
                var temporaryPath = $"{_path}.tmp";
                File.WriteAllText(temporaryPath, JsonSerializer.Serialize(state, ShowcaseJsonContext.Default.ShowcaseState));
                File.Move(temporaryPath, _path, overwrite: true);
                _shell.SessionStatus = "Session saved.";
            }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException)
            {
                ReportFailure(error);
            }
        }
    }

    /// <summary>Removes settings after an unhandled application exception.</summary>
    internal void Invalidate()
    {
        try
        {
            File.Delete(_path);
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            ReportFailure(error);
        }
    }

    /// <summary>Restores the last session or reports a recoverable storage error.</summary>
    /// <exception cref="JsonException">Thrown when <c>JsonSerializer.Deserialize(File.ReadAllText(_path), ShowcaseJsonContext.Default.ShowcaseState)</c> is <see langword="null"/>.</exception>
    private void Restore()
    {
        try
        {
            if (!File.Exists(_path))
            {
                _shell.SessionStatus = "New session. Input and threshold save on exit.";
                return;
            }

            var state = JsonSerializer.Deserialize(File.ReadAllText(_path), ShowcaseJsonContext.Default.ShowcaseState)
                ?? throw new JsonException("The saved session is empty.");
            _shell.Commands.WorkItemText = state.WorkItemText ?? string.Empty;
            _shell.Metrics.CpuWarningThreshold = state.CpuWarningThreshold;
            _shell.SessionStatus = "Previous input and threshold restored.";
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or JsonException)
        {
            ReportFailure(error);
        }
    }

    /// <summary>Surfaces storage failures in the shell.</summary>
    /// <param name="error">The storage failure.</param>
    private void ReportFailure(Exception error) => _shell.SessionStatus = $"Session storage: {error.Message}";
}
