// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Runtime.ExceptionServices;

#if REACTIVE_SHIM
namespace ReactiveUI.Avalonia.Reactive;
#else
namespace ReactiveUI.Avalonia;
#endif

/// <summary>Handles exceptions from observable subscriptions.</summary>
internal static class SubscriptionErrors
{
    /// <summary>Rethrows the supplied exception without losing the original stack trace.</summary>
    /// <param name="error">The subscription exception.</param>
    public static void Throw(Exception error) => ExceptionDispatchInfo.Capture(error).Throw();
}
