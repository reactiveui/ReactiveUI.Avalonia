// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Android.App;
using Android.Runtime;
using Avalonia;
using Avalonia.Android;

namespace ReactiveUI.Avalonia.Example.Android;

/// <summary>The Android application that starts the example app.</summary>
[Application]
public class ExampleApplication : AvaloniaAndroidApplication<App>
{
    /// <summary>Initializes a new instance of the <see cref="ExampleApplication"/> class.</summary>
    /// <param name="javaReference">The Java object reference.</param>
    /// <param name="transfer">How the reference is owned.</param>
    protected ExampleApplication(nint javaReference, JniHandleOwnership transfer)
        : base(javaReference, transfer)
    {
    }

    /// <inheritdoc/>
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder) =>
        base.CustomizeAppBuilder(builder).UseShowcase();
}
