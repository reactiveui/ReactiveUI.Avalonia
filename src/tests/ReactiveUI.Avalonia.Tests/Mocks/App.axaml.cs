// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using ReactiveUIDemo.ViewModels;
using ReactiveUIDemo.Views;
using Splat;

namespace ReactiveUIDemo;

/// <summary>Demo application for testing view registration and lifecycle.</summary>
public class App : Application
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        AppLocator.CurrentMutable.Register<IViewFor<FooViewModel>>(() => new FooView());
        AppLocator.CurrentMutable.Register<IViewFor<BarViewModel>>(() => new BarView());
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
