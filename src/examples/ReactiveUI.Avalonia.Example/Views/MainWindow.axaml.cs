// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using ReactiveUI.Avalonia.Example.ViewModels;

namespace ReactiveUI.Avalonia.Example.Views;

/// <summary>The main example shell window.</summary>
[System.Diagnostics.DebuggerDisplay("MainWindow: {ToString(),nq}")]
public sealed partial class MainWindow : ReactiveWindow<MainViewModel>
{
    /// <summary>Initializes a new instance of the <see cref="MainWindow"/> class.</summary>
    [RequiresUnreferencedCode("ReactiveWindow activation evaluates expression-based member chains via reflection.")]
    [RequiresDynamicCode("ReactiveWindow activation may require dynamic binding invocation.")]
    public MainWindow()
    {
        InitializeComponent();
        Closed += OnClosed;
    }

    /// <summary>Disposes the view model when the shell closes.</summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event data.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void OnClosed(object? sender, EventArgs e) => ViewModel?.Dispose();
}
