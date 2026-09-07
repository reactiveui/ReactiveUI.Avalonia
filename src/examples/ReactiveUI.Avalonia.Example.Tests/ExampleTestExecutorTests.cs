// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using TUnit.Core.Executors;

namespace ReactiveUI.Avalonia.Example.Tests;

/// <summary>Guards against reporting success before a dispatched asynchronous assertion completes.</summary>
[TestExecutor<CallerTestExecutor>]
public sealed class ExampleTestExecutorTests
{
    /// <summary>Verifies asynchronous failures propagate out of the headless executor.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Headless_Executor_Propagates_Failure_After_Async_Yield()
    {
        var executor = new ExampleAvaloniaTestExecutor();
        await Assert.That(() => executor.ExecuteTest(TestContext.Current!, static async () =>
        {
            await Task.Yield();
            throw new InvalidOperationException("Expected asynchronous test failure.");
        }).AsTask()).ThrowsExactly<InvalidOperationException>();
    }
}
