// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
#if REACTIVE_SHIM
namespace ReactiveUI.Avalonia.Reactive.Splat;
#else
namespace ReactiveUI.Avalonia.Splat;
#endif

/// <summary>Guards the one-time build of the ReactiveUI Splat application.</summary>
/// <remarks>
/// Each dependency injection integration reaches this during Avalonia's post-platform-setup callback. The callback can
/// run more than once in a process that configures several builders, and the second build would throw, so the guard
/// makes the call idempotent.
/// </remarks>
internal static class SplatApp
{
    /// <summary>Builds the ReactiveUI Splat application when it has not already been built.</summary>
    /// <param name="rxuiBuilder">The ReactiveUI builder.</param>
    internal static void BuildIfNeeded(IReactiveUIBuilder rxuiBuilder)
    {
        if (SplatBuilder.HasBeenBuilt)
        {
            return;
        }

        _ = rxuiBuilder.BuildApp();
    }
}
