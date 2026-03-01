// Copyright (c) 2019-2026 ReactiveUI and Avalonia Teams, and Contributors. All rights reserved.
// Licensed under the MIT license.
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
    public async ValueTask ExecuteTest(TestContext context, Func<ValueTask> testAction)
    {
        ArgumentNullException.ThrowIfNull(testAction);

        var originalLocator = Locator.GetLocator();

        try
        {
            await testAction();
        }
        finally
        {
            Locator.SetLocator(originalLocator);

            ReactiveUIBuilder.ResetBuilderStateForTests();

            AppLocator.CurrentMutable.CreateReactiveUIBuilder()
                .WithCoreServices()
                .BuildApp();
        }
    }
}
