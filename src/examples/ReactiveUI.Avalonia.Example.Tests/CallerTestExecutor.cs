// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using TUnit.Core.Interfaces;

namespace ReactiveUI.Avalonia.Example.Tests;

/// <summary>Runs executor regression tests outside the headless dispatch queue to avoid nested dispatch.</summary>
public sealed class CallerTestExecutor : ITestExecutor
{
    /// <inheritdoc/>
    public ValueTask ExecuteTest(TestContext context, Func<ValueTask> action) => action();
}
