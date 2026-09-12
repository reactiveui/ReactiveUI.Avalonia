// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI;
using ReactiveUI.Primitives.Disposables;

namespace ReactiveUI.Avalonia.Example.ViewModels;

/// <summary>Base view model with ReactiveUI activation support.</summary>
[System.Diagnostics.DebuggerDisplay("ViewModelBase: {Activator}")]
public class ViewModelBase : ReactiveObject, IActivatableViewModel, IDisposable
{
    /// <summary>The disposables owned by the view model.</summary>
    private readonly MultipleDisposable _disposables = new();

    /// <summary>A latch raised to 1 by the first caller to dispose this instance.</summary>
    private int _disposed;

    /// <summary>Initializes a new instance of the <see cref="ViewModelBase"/> class.</summary>
    protected ViewModelBase() => _disposables.Add(Activator);

    /// <inheritdoc/>
    public ViewModelActivator Activator { get; } = new();

    /// <inheritdoc/>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Disposes managed resources.</summary>
    /// <param name="disposing">A value indicating whether managed resources should be disposed.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        _disposables.Dispose();
    }

    /// <summary>Tracks a disposable for the view-model lifetime.</summary>
    /// <typeparam name="TDisposable">The disposable type.</typeparam>
    /// <param name="disposable">The disposable to track.</param>
    /// <returns>The tracked disposable.</returns>
    protected TDisposable Track<TDisposable>(TDisposable disposable)
        where TDisposable : IDisposable
    {
        _disposables.Add(disposable);
        return disposable;
    }
}
