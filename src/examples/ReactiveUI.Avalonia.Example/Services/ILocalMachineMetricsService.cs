// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Avalonia.Example.Models;

namespace ReactiveUI.Avalonia.Example.Services;

/// <summary>Provides live local machine measurements.</summary>
public interface ILocalMachineMetricsService
{
    /// <summary>Reads a single local machine measurement.</summary>
    /// <returns>The current local machine measurement.</returns>
    MachineSnapshot ReadSnapshot();

    /// <summary>Watches local machine measurements at the requested interval.</summary>
    /// <param name="interval">The polling interval.</param>
    /// <returns>An observable stream of local machine measurements.</returns>
    IObservable<MachineSnapshot> Watch(TimeSpan interval);
}
