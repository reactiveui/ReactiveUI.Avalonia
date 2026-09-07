// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
namespace ReactiveUI.Avalonia.Example.Models;

/// <summary>The small, serializable state restored between desktop sessions.</summary>
/// <param name="WorkItemText">The command input.</param>
/// <param name="CpuWarningThreshold">The CPU warning threshold.</param>
public sealed record ShowcaseState(string WorkItemText, double CpuWarningThreshold);
