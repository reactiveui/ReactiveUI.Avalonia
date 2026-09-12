// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Runtime.CompilerServices;

#if REACTIVE_SHIM
namespace ReactiveUI.Avalonia.Reactive;
#else
namespace ReactiveUI.Avalonia;
#endif

/// <summary>Resolves and wires the view a content host displays for a view model.</summary>
/// <remarks>
/// The hosts differ only in where the view model comes from, so view resolution, its logging and the subscription
/// bookkeeping live here rather than once per host.
/// </remarks>
internal static class ViewHostNavigation
{
    /// <summary>Resolves the view for a view model and hands it the view model.</summary>
    /// <typeparam name="THost">The host type, which supplies the logging context.</typeparam>
    /// <param name="host">The host requesting the view.</param>
    /// <param name="viewModel">The view model to display.</param>
    /// <param name="contract">The optional contract used to distinguish registered views.</param>
    /// <param name="viewLocator">The locator to use, or null to use the current one.</param>
    /// <returns>The wired view, or null when no view is registered for the view model.</returns>
    internal static IViewFor? ResolveView<THost>(THost host, object viewModel, string? contract, IViewLocator? viewLocator)
        where THost : class, IEnableLogger
    {
        var locator = viewLocator ?? CurrentViewLocator.Current;
        var viewInstance = locator.ResolveView(viewModel, contract);
        if (viewInstance is null)
        {
            LogMissingView(host, viewModel, contract);
            return null;
        }

        host.Log().Info(contract is null
            ? $"Ready to show {viewInstance} with autowired {viewModel}."
            : $"Ready to show {viewInstance} with autowired {viewModel} and contract '{contract}'.");

        viewInstance.ViewModel = viewModel;
        if (viewInstance is IDataContextProvider provider)
        {
            provider.DataContext = viewModel;
        }

        return viewInstance;
    }

    /// <summary>Disposes the supplied subscriptions and clears the reference.</summary>
    /// <param name="disposables">The subscriptions to release.</param>
    /// <remarks>The reference is cleared before disposal so a re-entrant detach cannot dispose the same set twice.</remarks>
    internal static void Release(ref CompositeDisposable? disposables)
    {
        var pending = disposables;
        disposables = null;
        pending?.Dispose();
    }

    /// <summary>Logs that no view is registered for a view model.</summary>
    /// <typeparam name="THost">The host type, which supplies the logging context.</typeparam>
    /// <param name="host">The host that attempted the resolution.</param>
    /// <param name="viewModel">The view model that could not be resolved.</param>
    /// <param name="contract">The optional view contract.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void LogMissingView<THost>(THost host, object viewModel, string? contract)
        where THost : class, IEnableLogger =>
        host.Log().Warn(contract is null
            ? $"Couldn't find view for '{viewModel}'. Is it registered? Falling back to default content."
            : $"Couldn't find view with contract '{contract}' for '{viewModel}'. Is it registered? Falling back to default content.");
}
