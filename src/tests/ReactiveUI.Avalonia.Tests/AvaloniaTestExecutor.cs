// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using ReactiveUI.Builder;
using Splat;
using TUnit.Core.Interfaces;

namespace ReactiveUI.Avalonia.Tests;

/// <summary>Test executor that provides AppBuilder lifecycle management for each test. Resets and rebuilds ReactiveUI state before each test, and cleans up after.</summary>
public class AvaloniaTestExecutor : ITestExecutor
{
    /// <inheritdoc/>
    public async ValueTask ExecuteTest(TestContext context, Func<ValueTask> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        // Run the test body on the shared headless UI thread so dispatcher-dependent code
        // (e.g. AvaloniaScheduler) behaves deterministically. See AvaloniaTestSession.
        await AvaloniaTestSession.Instance.Dispatch(
            async () =>
            {
                ReactiveUIBuilder.ResetBuilderStateForTests();

                _ = AppLocator.CurrentMutable.CreateReactiveUIBuilder()
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
                    await action();
                }
                finally
                {
                    ReactiveUIBuilder.ResetBuilderStateForTests();

                    _ = AppLocator.CurrentMutable.CreateReactiveUIBuilder()
                        .WithCoreServices()
                        .BuildApp();
                }
            },
            CancellationToken.None);
    }
}
