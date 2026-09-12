// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
#if REACTIVE_SHIM
namespace ReactiveUI.Avalonia.Reactive.Splat;
#else
namespace ReactiveUI.Avalonia.Splat;
#endif

/// <summary>Configures Avalonia support on a ReactiveUI builder and builds the Splat application once.</summary>
/// <remarks>
/// Every dependency injection integration reaches this from Avalonia's post-platform-setup callback. The callback can
/// run more than once in a process that configures several builders, and a second build would throw, so the build is
/// guarded.
/// </remarks>
internal static class SplatApp
{
    /// <summary>Creates the ReactiveUI builder for Avalonia, applies the caller's configuration and builds it.</summary>
    /// <param name="withReactiveUIBuilder">Customizes the ReactiveUI builder, or null.</param>
    internal static void BuildWithAvalonia(Action<ReactiveUIBuilder>? withReactiveUIBuilder)
    {
        var rxuiBuilder = AppLocator.CurrentMutable.CreateReactiveUIBuilder();
        _ = rxuiBuilder.WithAvalonia();
        withReactiveUIBuilder?.Invoke(rxuiBuilder);

        if (SplatBuilder.HasBeenBuilt)
        {
            return;
        }

        _ = rxuiBuilder.BuildApp();
    }
}
