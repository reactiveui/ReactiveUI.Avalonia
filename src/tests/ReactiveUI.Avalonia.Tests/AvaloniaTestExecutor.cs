// Copyright (c) 2019-2026 ReactiveUI and Avalonia Teams, and Contributors. All rights reserved.
// Licensed under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Builder;
using Splat;
using TUnit.Core.Interfaces;

namespace ReactiveUI.Avalonia.Tests;

/// <summary>
/// Test executor that provides AppBuilder lifecycle management for each test.
/// Resets and rebuilds ReactiveUI state before each test, and cleans up after.
/// </summary>
public class AvaloniaTestExecutor : ITestExecutor
{
    /// <inheritdoc/>
    public async ValueTask ExecuteTest(TestContext context, Func<ValueTask> testAction)
    {
        ArgumentNullException.ThrowIfNull(testAction);

        ReactiveUIBuilder.ResetBuilderStateForTests();

        AppLocator.CurrentMutable.CreateReactiveUIBuilder()
            .WithRegistration(splat =>
            {
                splat.RegisterConstant<IActivationForViewFetcher>(new AvaloniaActivationForViewFetcher());
                splat.RegisterConstant<IPropertyBindingHook>(new AutoDataTemplateBindingHook());
                splat.RegisterConstant<ICreatesCommandBinding>(new AvaloniaCreatesCommandBinding());
                splat.RegisterConstant<ICreatesObservableForProperty>(new AvaloniaObjectObservableForProperty());
            })
            .WithSuspensionHost()
            .WithCoreServices()
            .BuildApp();

        try
        {
            await testAction();
        }
        finally
        {
            ReactiveUIBuilder.ResetBuilderStateForTests();

            AppLocator.CurrentMutable.CreateReactiveUIBuilder()
                .WithCoreServices()
                .BuildApp();
        }
    }
}
