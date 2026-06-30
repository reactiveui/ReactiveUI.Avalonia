// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Avalonia;
using Avalonia.Headless;
using ReactiveUI.Avalonia.Reactive;
using ReactiveUI.Reactive.Builder;
using Splat;
using TUnit.Core.Interfaces;

namespace ReactiveUI.Avalonia.Reactive.Tests;

/// <summary>Runs reactive Avalonia tests on a shared headless UI dispatcher.</summary>
public class ReactiveAvaloniaTestExecutor : ITestExecutor
{
    /// <summary>The lazily-created headless Avalonia session.</summary>
    private static readonly Lazy<HeadlessUnitTestSession> Session =
        new(() => HeadlessUnitTestSession.StartNew(typeof(Application)), LazyThreadSafetyMode.ExecutionAndPublication);

    /// <inheritdoc/>
    public async ValueTask ExecuteTest(TestContext context, Func<ValueTask> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        await Session.Value.Dispatch(
            async () =>
            {
                ReactiveUIBuilder.ResetBuilderStateForTests();
                _ = RxAppBuilder.CreateReactiveUIBuilder()
                    .WithAvalonia()
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
