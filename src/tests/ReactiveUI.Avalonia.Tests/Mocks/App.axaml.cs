// Copyright (c) 2019-2026 ReactiveUI and Avalonia Teams, and Contributors. All rights reserved.
// Licensed under the MIT license.
// See the LICENSE file in the project root for full license information.

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using ReactiveUIDemo.ViewModels;
using ReactiveUIDemo.Views;
using Splat;

namespace ReactiveUIDemo;

/// <summary>
/// Demo application for testing view registration and lifecycle.
/// </summary>
public class App : Application
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        AppLocator.CurrentMutable.Register(() => new FooView(), typeof(IViewFor<FooViewModel>));
        AppLocator.CurrentMutable.Register(() => new BarView(), typeof(IViewFor<BarViewModel>));
    }

    /// <inheritdoc/>
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
