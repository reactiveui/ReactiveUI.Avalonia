// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Avalonia.Example.Services;
using ReactiveUI.Primitives;

namespace ReactiveUI.Avalonia.Example.Tests;

/// <summary>Tests the browser-safe metrics sampler, which must not depend on <c>Process</c>.</summary>
public sealed class BrowserMachineMetricsServiceTests
{
    /// <summary>The test sample interval.</summary>
    private static readonly TimeSpan SampleInterval = TimeSpan.FromMilliseconds(20);

    /// <summary>The maximum wait for a periodic sample.</summary>
    private static readonly TimeSpan SampleTimeout = TimeSpan.FromSeconds(5);

    /// <summary>Verifies a snapshot holds positive memory, one thread and no processor reading.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReadSnapshot_Returns_Runtime_Only_Values()
    {
        var snapshot = new BrowserMachineMetricsService().ReadSnapshot();

        await Assert.That(snapshot.ManagedMegabytes).IsGreaterThan(0);
        await Assert.That(snapshot.WorkingSetMegabytes).IsGreaterThan(0);
        await Assert.That(snapshot.ThreadCount).IsEqualTo(1);
        await Assert.That(snapshot.ProcessorPercent).IsEqualTo(0);
        await Assert.That(snapshot.Status).StartsWith("Browser data:");
    }

    /// <summary>Verifies the watch stream keeps producing snapshots.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Watch_Produces_Periodic_Snapshots()
    {
        const int requiredSamples = 2;
        var snapshot = await new BrowserMachineMetricsService().Watch(SampleInterval)
            .Take(requiredSamples).ToTask().WaitAsync(SampleTimeout);

        await Assert.That(snapshot.ManagedMegabytes).IsGreaterThan(0);
    }
}
