// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using ReactiveUI.Avalonia.Example.Tests;
using TUnit.Core.Executors;
using TUnit.Core.Interfaces;

[assembly: NotInParallel]
[assembly: TestExecutor<ExampleAvaloniaTestExecutor>]

namespace ReactiveUI.Avalonia.Example.Tests;

/// <summary>Runs every example test on the application's headless UI thread.</summary>
public sealed class ExampleAvaloniaTestExecutor : ITestExecutor
{
    /// <inheritdoc/>
    public async ValueTask ExecuteTest(TestContext context, Func<ValueTask> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        _ = await ExampleAvaloniaTestSession.Instance.Dispatch(
            async () =>
            {
                await action();
                return true;
            },
            CancellationToken.None);
    }
}
