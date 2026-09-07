// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using ReactiveUI.Avalonia.Example.Services;
using ReactiveUI.Primitives;

namespace ReactiveUI.Avalonia.Example.Tests;

/// <summary>Exercises the real local process sampler and its periodic observable.</summary>
public sealed class LocalMachineMetricsServiceTests
{
    /// <summary>The test sample interval.</summary>
    private static readonly TimeSpan SampleInterval = TimeSpan.FromMilliseconds(20);

    /// <summary>The maximum wait for a periodic sample.</summary>
    private static readonly TimeSpan SampleTimeout = TimeSpan.FromSeconds(5);

    /// <summary>Verifies the live service produces more than its initial snapshot.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Watch_Produces_Periodic_Process_Measurements()
    {
        const int requiredSamples = 2;
        const double maximumPercentage = 100;
        var service = new LocalMachineMetricsService();
        var snapshot = await service.Watch(SampleInterval)
            .Take(requiredSamples).ToTask().WaitAsync(SampleTimeout);
        await Assert.That(snapshot.WorkingSetMegabytes).IsGreaterThan(0);
        await Assert.That(snapshot.ManagedMegabytes).IsGreaterThan(0);
        await Assert.That(snapshot.ThreadCount).IsGreaterThan(0);
        await Assert.That(snapshot.ProcessorPercent).IsGreaterThanOrEqualTo(0);
        await Assert.That(snapshot.ProcessorPercent).IsLessThanOrEqualTo(maximumPercentage);
    }
}
