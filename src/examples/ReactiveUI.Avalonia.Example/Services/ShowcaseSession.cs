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

namespace ReactiveUI.Avalonia.Example.Services;

/// <summary>Composes Avalonia suspension notifications with a small local JSON settings store.</summary>
[System.Diagnostics.DebuggerDisplay("ShowcaseSession: {_shell}")]
public sealed class ShowcaseSession : IDisposable
{
    /// <summary>The view model whose settings are persisted.</summary>
    private readonly MainViewModel _shell;

    /// <summary>The settings file path.</summary>
    private readonly string _path;

    /// <summary>The platform lifetime adapter.</summary>
    private readonly AutoSuspendHelper _suspension;

    /// <summary>The lifetime subscriptions.</summary>
    private readonly MultipleDisposable _subscriptions = new();

    /// <summary>Initializes a new instance of the <see cref="ShowcaseSession"/> class.</summary>
    /// <param name="lifetime">The running desktop lifetime.</param>
    /// <param name="shell">The shell to persist.</param>
    /// <param name="path">The settings file path.</param>
    public ShowcaseSession(IApplicationLifetime lifetime, MainViewModel shell, string path)
    {
        _shell = shell;
        _path = path;
        _suspension = new(lifetime);
        _subscriptions.Add(RxSuspension.SuspensionHost.IsLaunchingNew.SubscribeSafe(_ => Restore(), ReportFailure));
        _subscriptions.Add(RxSuspension.SuspensionHost.ShouldPersistState.SubscribeSafe(Persist, ReportFailure));
        _subscriptions.Add(RxSuspension.SuspensionHost.ShouldInvalidateState.SubscribeSafe(_ => Invalidate(), ReportFailure));
    }

    /// <summary>Signals startup after the shell and its services have been composed.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Start() => _suspension.OnFrameworkInitializationCompleted();

    /// <inheritdoc/>
    public void Dispose()
    {
        _subscriptions.Dispose();
        _suspension.Dispose();
    }

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

    /// <summary>Removes settings after an unhandled application exception.</summary>
    private void Invalidate()
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

    /// <summary>Surfaces storage failures in the shell.</summary>
    /// <param name="error">The storage failure.</param>
    private void ReportFailure(Exception error) => _shell.SessionStatus = $"Session storage: {error.Message}";
}
