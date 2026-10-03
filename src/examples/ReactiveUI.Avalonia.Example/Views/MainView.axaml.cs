// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using ReactiveUI.Avalonia.Example.ViewModels;

namespace ReactiveUI.Avalonia.Example.Views;

/// <summary>The example shell content, shared by the desktop window and single-view platforms.</summary>
[System.Diagnostics.DebuggerDisplay("MainView: {ToString(),nq}")]
public sealed partial class MainView : ReactiveUserControl<MainViewModel>
{
    /// <summary>Initializes a new instance of the <see cref="MainView"/> class.</summary>
    [RequiresUnreferencedCode("ReactiveUserControl activation evaluates expression-based member chains via reflection.")]
    [RequiresDynamicCode("ReactiveUserControl activation may require dynamic binding invocation.")]
    public MainView() => InitializeComponent();
}
