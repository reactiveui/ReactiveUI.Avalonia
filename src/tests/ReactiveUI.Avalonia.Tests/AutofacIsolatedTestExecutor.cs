// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using ReactiveUI.Builder;
using Splat;
using TUnit.Core.Interfaces;

namespace ReactiveUI.Avalonia.Tests;

/// <summary>
/// Test executor for tests that mutate the global Splat dependency resolver
/// (e.g. replacing it with an Autofac-backed resolver). Saves and restores
/// the original locator so subsequent tests are not poisoned.
/// </summary>
public class AutofacIsolatedTestExecutor : ITestExecutor
{
    /// <inheritdoc/>
    public async ValueTask ExecuteTest(TestContext context, Func<ValueTask> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        // Run on the shared headless UI thread for consistency with AvaloniaTestExecutor.
        await AvaloniaTestSession.Instance.Dispatch(
            async () =>
            {
                var originalLocator = Locator.GetLocator();

                try
                {
                    await action();
                }
                finally
                {
                    Locator.SetLocator(originalLocator);

                    ReactiveUIBuilder.ResetBuilderStateForTests();

                    _ = AppLocator.CurrentMutable.CreateReactiveUIBuilder()
                        .WithCoreServices()
                        .BuildApp();
                }
            },
            CancellationToken.None);
    }
}
