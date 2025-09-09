[![Build](https://github.com/reactiveui/ReactiveUI.Avalonia/actions/workflows/ci-build.yml/badge.svg)](https://github.com/reactiveui/ReactiveUI.Avalonia/actions/workflows/ci-build.yml)
[![Code Coverage](https://codecov.io/gh/reactiveui/ReactiveUI.Avalonia/branch/main/graph/badge.svg)](https://codecov.io/gh/reactiveui/ReactiveUI.Avalonia)
[![#yourfirstpr](https://img.shields.io/badge/first--timers--only-friendly-blue.svg)](https://reactiveui.net/contribute)
[![](https://img.shields.io/badge/chat-slack-blue.svg)](https://reactiveui.net/slack)
[![NuGet](https://img.shields.io/nuget/v/ReactiveUI.Avalonia.svg)](https://www.nuget.org/packages/ReactiveUI.Avalonia/)

<br>
<a href="https://github.com/reactiveui/reactiveui">
  <img width="160" heigth="160" src="https://raw.githubusercontent.com/reactiveui/styleguide/master/logo/main.png">
</a>
<br>

# ReactiveUI for Avalonia UI

This package provides [ReactiveUI](https://reactiveui.net/) bindings for the [Avalonia UI](https://avaloniaui.net/) framework, enabling you to build composable, cross-platform model-view-viewmodel (MVVM) applications for Windows, macOS, and Linux.

---
## NuGet Packages

To get started, install the following package into your Avalonia application project.

| Platform          | NuGet                  |
| ----------------- | ---------------------- |
| Avalonia          | [![NuGet](https://img.shields.io/nuget/v/ReactiveUI.Avalonia.svg)](https://www.nuget.org/packages/ReactiveUI.Avalonia/) |

-----

## Tutorial: Mastering ReactiveUI with Avalonia

Welcome to the `ReactiveUI.Avalonia` guide! This tutorial will walk you through setting up an Avalonia application with the power of ReactiveUI. We'll start from the basics and build up to a fully reactive application.

`ReactiveUI.Avalonia` provides the necessary bindings and helpers to seamlessly integrate the ReactiveUI MVVM framework with your Avalonia projects, enabling you to write elegant, testable, and maintainable code.

### Chapter 1: Getting Started - Your First Reactive View

Let's begin by setting up your project and creating your first reactive view and view model.

#### 1. Installation

Add the `ReactiveUI.Avalonia` package to your Avalonia application's project file.

```xml
<PackageReference Include="ReactiveUI.Avalonia" Version="21.0.1" />
```

#### 2. Initialization

Initialize ReactiveUI in your Avalonia app builder. Call `UseReactiveUI()` during app setup. You can also auto-register views for view models with the provided helpers.

```csharp
// Program.cs
using Avalonia;
using ReactiveUI.Avalonia;

public static class Program
{
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
        .UsePlatformDetect()
        .LogToTrace()
        .UseReactiveUI() // Initialize ReactiveUI for Avalonia
        .RegisterReactiveUIViewsFromEntryAssembly(); // Optional: scan & register IViewFor<> views
}
```

#### 3. Create a ViewModel

Create a simple view model. Notice how it inherits from `ReactiveObject` and uses `RaiseAndSetIfChanged` to notify the UI of property changes.

```csharp
// MyViewModel.cs
using ReactiveUI;

public class MyViewModel : ReactiveObject
{
    private string _greeting;

    public string Greeting
    {
        get => _greeting;
        set => this.RaiseAndSetIfChanged(ref _greeting, value);
    }

    public MyViewModel()
    {
        Greeting = "Hello, Reactive World!";
    }
}
```

#### 4. Create a Reactive View

Create a view that binds to this view model. `ReactiveUserControl<TViewModel>` (or `ReactiveWindow<TViewModel>`) makes this easy.

```xml
<!-- MainView.axaml -->
<UserControl x:Class="MyAvaloniaApp.Views.MainView"
             xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:local="clr-namespace:MyAvaloniaApp.Views"
             xmlns:rxui="using:ReactiveUI.Avalonia">
    <StackPanel HorizontalAlignment="Center" VerticalAlignment="Center">
        <TextBlock x:Name="GreetingTextBlock" FontSize="24"/>
    </StackPanel>
</UserControl>
```

In the code-behind, use `WhenActivated` to set up your bindings. This is the core of a reactive view.

```csharp
// MainView.axaml.cs
using ReactiveUI;
using ReactiveUI.Avalonia;
using System.Reactive.Disposables;

public partial class MainView : ReactiveUserControl<MyViewModel>
{
    public MainView()
    {
        InitializeComponent();
        ViewModel = new MyViewModel();

        this.WhenActivated(disposables =>
        {
            this.OneWayBind(ViewModel,
                vm => vm.Greeting,
                v => v.GreetingTextBlock.Text)
                .DisposeWith(disposables);
        });
    }
}
```

Congratulations! You've just created your first reactive UI with `ReactiveUI.Avalonia`. When you run the app, you'll see the greeting message displayed.

### Chapter 2: Handling User Interaction with ReactiveCommands

Static text is great, but apps need to respond to users. `ReactiveCommand` is the standard way to handle user actions like button clicks in a testable and composable way.

#### 1. Add a Command to the ViewModel

Let's add a command to our view model that generates a new greeting.

```csharp
// MyViewModel.cs
using ReactiveUI;
using System;
using System.Reactive;

public class MyViewModel : ReactiveObject
{
    // ... Greeting property from before ...

    public ReactiveCommand<Unit, Unit> GenerateGreetingCommand { get; }

    public MyViewModel()
    {
        Greeting = "Hello, Reactive World!";

        GenerateGreetingCommand = ReactiveCommand.Create(() =>
        {
            Greeting = $"Hello from Avalonia! The time is {DateTime.Now.ToLongTimeString()}";
        });
    }
}
```

#### 2. Bind the Command in the View

Add a button to your XAML and bind its `Command` property to the new command.

```xml
<!-- MainView.axaml -->
<StackPanel HorizontalAlignment="Center" VerticalAlignment="Center">
    <TextBlock x:Name="GreetingTextBlock" FontSize="24"/>
    <Button x:Name="GenerateGreetingButton" Content="Generate" Margin="0,20,0,0"/>
</StackPanel>
```

Update your `WhenActivated` block to bind the button to the command.

```csharp
// MainView.axaml.cs
this.WhenActivated(disposables =>
{
    // ... existing binding ...

    this.BindCommand(ViewModel,
        vm => vm.GenerateGreetingCommand,
        v => v.GenerateGreetingButton)
        .DisposeWith(disposables);
});
```

Now, when you click the button, the command will execute, the `Greeting` property will change, and the UI will automatically update.

### Chapter 3: Navigating with `RoutedViewHost`

For more complex applications, you'll need navigation. `RoutedViewHost` is a control that displays a view based on the current state of a `RoutingState` object.

#### 1. Set up a Router

In your main view model (or a dedicated routing service), create a `RoutingState`.

```csharp
// AppViewModel.cs
using ReactiveUI;

public class AppViewModel : ReactiveObject, IScreen
{
    public RoutingState Router { get; } = new RoutingState();

    public AppViewModel()
    {
        // Navigate to the initial view model
        Router.Navigate.Execute(new MyViewModel());
    }
}
```

#### 2. Use `RoutedViewHost` in your Main Window

In your main window's XAML, replace the content with a `RoutedViewHost` and bind its `Router` property.

```xml
<!-- MainWindow.axaml -->
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:rxui="using:ReactiveUI.Avalonia"
        x:Class="MyAvaloniaApp.MainWindow">
    <Grid>
        <rxui:RoutedViewHost Router="{Binding Router}" />
    </Grid>
</Window>
```

You'll also need a way to tell `RoutedViewHost` which view to create for a given view model. You can either register views manually or use the provided scan helpers.

```csharp
// App.axaml.cs (during app initialization)
using ReactiveUI;
using ReactiveUI.Avalonia;
using Splat;

Locator.CurrentMutable.Register(() => new MainView(), typeof(IViewFor<MyViewModel>));
// or use: AppBuilder.Configure<App>().UseReactiveUI().RegisterReactiveUIViewsFromEntryAssembly();
```

Now, as you call `Router.Navigate.Execute(...)`, the `RoutedViewHost` will automatically switch to the correct view.

This tutorial has covered the basics of getting started with `ReactiveUI.Avalonia`. You've learned how to set up your project, create reactive view models and views, handle user interaction with commands, and manage navigation. From here, you can explore more advanced ReactiveUI features like `WhenAnyValue`, `ObservableAsPropertyHelper`, and more complex command scenarios.

-----

## Thanks

We want to thank the following contributors and libraries that help make ReactiveUI.Avalonia possible:

### Core Libraries

  - **Avalonia UI**: [Avalonia](https://avaloniaui.net/) - The cross-platform UI framework.
  - **System.Reactive**: [Reactive Extensions for .NET](https://github.com/dotnet/reactive) - The foundation of ReactiveUI's asynchronous API.
  - **Splat**: [Splat](https://github.com/reactiveui/splat) - Cross-platform utilities and service location.
  - **ReactiveUI**: [ReactiveUI](https://github.com/reactiveui/reactiveui) - The core MVVM framework.

-----

## Sponsorship

The core team members, ReactiveUI contributors and contributors in the ecosystem do this open-source work in their free time. If you use ReactiveUI, a serious task, and you'd like us to invest more time on it, please donate. This project increases your income/productivity too. It makes development and applications faster and it reduces the required bandwidth.

[Become a sponsor](https://github.com/sponsors/reactivemarbles).

This is how we use the donations:

  * Allow the core team to work on ReactiveUI
  * Thank contributors if they invested a large amount of time in contributing
  * Support projects in the ecosystem

-----

## Support

If you have a question, please see if any discussions in our [GitHub Discussions](https://github.com/reactiveui/ReactiveUI.Avalonia/discussions) or [GitHub issues](https://github.com/reactiveui/ReactiveUI.Avalonia/issues) have already answered it.

If you want to discuss something or just need help, here is our [Slack room](https://reactiveui.net/slack), where there are always individuals looking to help out!

Please do not open GitHub issues for support requests.

-----

## Contribute

ReactiveUI.Avalonia is developed under an OSI-approved open source license, making it freely usable and distributable, even for commercial use.

If you want to submit pull requests please first open a [GitHub issue](https://github.com/reactiveui/ReactiveUI.Avalonia/issues/new/choose) to discuss. We are first time PR contributors friendly.

See [Contribution Guidelines](https://www.reactiveui.net/contribute/) for further information how to contribute changes.

-----

## License

ReactiveUI.Avalonia is licensed under the [MIT License](https://github.com/reactiveui/ReactiveUI.Avalonia/blob/main/LICENSE).
