[![Build](https://github.com/reactiveui/ReactiveUI.Avalonia/actions/workflows/ci-build.yml/badge.svg)](https://github.com/reactiveui/ReactiveUI.Avalonia/actions/workflows/ci-build.yml)
[![Code Coverage](https://codecov.io/gh/reactiveui/ReactiveUI.Avalonia/branch/main/graph/badge.svg)](https://codecov.io/gh/reactiveui/ReactiveUI.Avalonia)
[![#yourfirstpr](https://img.shields.io/badge/first--timers--only-friendly-blue.svg)](https://reactiveui.net/contribute)
[![](https://img.shields.io/badge/chat-slack-blue.svg)](https://reactiveui.net/slack)
[![NuGet](https://img.shields.io/nuget/v/ReactiveUI.Avalonia.svg)](https://www.nuget.org/packages/ReactiveUI.Avalonia/)

<br>
<a href="https://github.com/reactiveui/reactiveui">
  <img width="160" height="160" src="https://raw.githubusercontent.com/reactiveui/styleguide/master/logo/main.png">
</a>
<br>

# ReactiveUI for Avalonia UI

This package provides [ReactiveUI](https://reactiveui.net/) bindings and helpers for the [Avalonia UI](https://avaloniaui.net/) framework, enabling you to build composable, cross-platform model-view-viewmodel (MVVM) applications for Windows, macOS, and Linux.

---
## Packages

Install the packages that match your preferred dependency injection container. The core package is always required.

- Core
  - [![ReactiveUI.Avalonia](https://img.shields.io/nuget/v/ReactiveUI.Avalonia.svg)](https://www.nuget.org/packages/ReactiveUI.Avalonia/)
- DI integrations
  - [![ReactiveUI.Avalonia.Autofac](https://img.shields.io/nuget/v/ReactiveUI.Avalonia.Autofac.svg)](https://www.nuget.org/packages/ReactiveUI.Avalonia.Autofac)
  - [![ReactiveUI.Avalonia.DryIoc](https://img.shields.io/nuget/v/ReactiveUI.Avalonia.DryIoc.svg)](https://www.nuget.org/packages/ReactiveUI.Avalonia.DryIoc)
  - [![ReactiveUI.Avalonia.Microsoft.Extensions.DependencyInjection](https://img.shields.io/nuget/v/ReactiveUI.Avalonia.Microsoft.Extensions.DependencyInjection.svg)](https://www.nuget.org/packages/ReactiveUI.Avalonia.Microsoft.Extensions.DependencyInjection)
  - [![ReactiveUI.Avalonia.Ninject](https://img.shields.io/nuget/v/ReactiveUI.Avalonia.Ninject.svg)](https://www.nuget.org/packages/ReactiveUI.Avalonia.Ninject)

All libraries target multiple frameworks including .NET Standard 2.0 and modern .NET (.NET 8/9/10) for broad compatibility.

---
## Recommended setup (ReactiveUIBuilder)

The recommended approach for new projects is to use the `ReactiveUIBuilder` via the `UseReactiveUI` and `UseReactiveUIWith...` extensions. This ensures consistent registration of schedulers, activation/binding hooks, and view discovery.

Namespaces to import in your startup:

```csharp
using Avalonia; // AppBuilder
using ReactiveUI.Avalonia; // UseReactiveUI, RegisterReactiveUIViews* (core)
using ReactiveUI.Avalonia.Splat; // Autofac, DryIoc, Ninject, Microsoft.Extensions.DependencyInjection integrations
```

Minimal setup (no external DI container):

```csharp
public static class Program
{
    public static void Main(string[] args) => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp() => AppBuilder
        .Configure<App>()
        .UsePlatformDetect()
        .UseReactiveUI(rxui =>
        {
            // Optional: add custom registration here via rxui.WithRegistration(...)
        })
        .RegisterReactiveUIViewsFromEntryAssembly();
}
```

With Autofac:

```csharp
public static AppBuilder BuildAvaloniaApp() => AppBuilder
    .Configure<App>()
    .UsePlatformDetect()
    .UseReactiveUIWithAutofac(
        container =>
        {
            // Register your services/view models
            // container.RegisterType<MainViewModel>();
        },
        withResolver: resolver =>
        {
            // Optional: access the Autofac resolver/lifetime scope
        },
        withReactiveUIBuilder: rxui =>
        {
            // Optional: add ReactiveUI customizations
        })
    .RegisterReactiveUIViewsFromEntryAssembly();
```

With DryIoc:

```csharp
public static AppBuilder BuildAvaloniaApp() => AppBuilder
    .Configure<App>()
    .UsePlatformDetect()
    .UseReactiveUIWithDryIoc(
        container =>
        {
            // container.Register<MainViewModel>(Reuse.Singleton);
        },
        withReactiveUIBuilder: rxui =>
        {
            // Optional ReactiveUI customizations
        })
    .RegisterReactiveUIViewsFromEntryAssembly();
```

With Microsoft.Extensions.DependencyInjection:

```csharp
public static AppBuilder BuildAvaloniaApp() => AppBuilder
    .Configure<App>()
    .UsePlatformDetect()
    .UseReactiveUIWithMicrosoftDependencyResolver(
        services =>
        {
            // services.AddSingleton<MainViewModel>();
        },
        withResolver: sp =>
        {
            // Optional: access ServiceProvider
        },
        withReactiveUIBuilder: rxui =>
        {
            // Optional ReactiveUI customizations
        })
    .RegisterReactiveUIViewsFromEntryAssembly();
```

With Ninject:

```csharp
public static AppBuilder BuildAvaloniaApp() => AppBuilder
    .Configure<App>()
    .UsePlatformDetect()
    .UseReactiveUIWithNinject(
        kernel =>
        {
            // kernel.Bind<MainViewModel>().ToSelf().InSingletonScope();
        },
        withReactiveUIBuilder: rxui =>
        {
            // Optional ReactiveUI customizations
        })
    .RegisterReactiveUIViewsFromEntryAssembly();
```

Notes
- `UseReactiveUI` sets `RxApp.MainThreadScheduler` to `AvaloniaScheduler.Instance` and registers the Avalonia-specific activation and binding services.
- `RegisterReactiveUIViewsFromEntryAssembly()` scans your entry assembly and registers any types implementing `IViewFor<TViewModel>` for view location/navigation.
- For existing apps, you can keep using `UseReactiveUI()` without a DI container and register services into `Splat` directly if you prefer.

---
## Manual setup (without container mixins)

You can configure a custom container using the generic `UseReactiveUIWithDIContainer` if you don’t use one of the provided integrations:

```csharp
AppBuilder
    .Configure<App>()
    .UseReactiveUIWithDIContainer(
        containerFactory: () => new MyContainer(),
        containerConfig: container =>
        {
            // configure container
        },
        dependencyResolverFactory: container => new MySplatResolver(container))
    .RegisterReactiveUIViewsFromEntryAssembly();
```

---
## Quick example: first reactive view

```csharp
// View model
using ReactiveUI;

public class MyViewModel : ReactiveObject
{
    private string _greeting = "Hello, Reactive World!";
    public string Greeting
    {
        get => _greeting;
        set => this.RaiseAndSetIfChanged(ref _greeting, value);
    }
}
```

```xml
<!-- MainView.axaml -->
<UserControl x:Class="MyAvaloniaApp.Views.MainView"
             xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:rxui="using:ReactiveUI.Avalonia">
    <StackPanel HorizontalAlignment="Center" VerticalAlignment="Center">
        <TextBlock x:Name="GreetingTextBlock" FontSize="24"/>
    </StackPanel>
</UserControl>
```

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
            this.OneWayBind(ViewModel, vm => vm.Greeting, v => v.GreetingTextBlock.Text)
                .DisposeWith(disposables);
        });
    }
}
```

---
## API reference

### ReactiveUI.Avalonia (core)

Key extension methods on `AppBuilder`:
- `UseReactiveUI()` — initialize ReactiveUI for Avalonia (scheduler, activation, bindings)
- `UseReactiveUI(Action<ReactiveUIBuilder>)` — initialize with the `ReactiveUIBuilder` for additional configuration
- `RegisterReactiveUIViews(params Assembly[])` — scan and register views implementing `IViewFor<T>`
- `RegisterReactiveUIViewsFromEntryAssembly()` — convenience overload to scan the entry assembly
- `RegisterReactiveUIViewsFromAssemblyOf<TMarker>()` — scan a specific assembly
- `UseReactiveUIWithDIContainer<TContainer>(...)` — bring-your-own container integration via an `IDependencyResolver`

Important types registered by default:
- `IActivationForViewFetcher` ? `AvaloniaActivationForViewFetcher`
- `IPropertyBindingHook` ? `AutoDataTemplateBindingHook`
- `ICreatesCommandBinding` ? `AvaloniaCreatesCommandBinding`
- `ICreatesObservableForProperty` ? `AvaloniaObjectObservableForProperty`
- `RxApp.MainThreadScheduler` set to `AvaloniaScheduler.Instance`

Controls and helpers:
- `RoutedViewHost` — view host that displays the view for the current `RoutingState`
- `ReactiveUserControl<TViewModel>`, `ReactiveWindow<TViewModel>` — base classes for reactive views

### ReactiveUI.Avalonia.Autofac

Extension methods on `AppBuilder` (namespace `Avalonia.ReactiveUI.Splat`):
- `UseReactiveUIWithAutofac(Action<ContainerBuilder> containerConfig, Action<AutofacDependencyResolver>? withResolver = null)`
- `UseReactiveUIWithAutofac(Action<ContainerBuilder> containerConfig, Action<AutofacDependencyResolver>? withResolver = null, Action<ReactiveUIBuilder>? withReactiveUIBuilder = null)`

What it does:
- Sets up `Splat` with Autofac, initializes ReactiveUI for Avalonia, builds your container, and optionally exposes the Autofac resolver.

### ReactiveUI.Avalonia.DryIoc

Extension methods on `AppBuilder` (namespace `ReactiveUI.Avalonia.Splat`):
- `UseReactiveUIWithDryIoc(Action<Container> containerConfig)`
- `UseReactiveUIWithDryIoc(Action<Container> containerConfig, Action<ReactiveUIBuilder>? withReactiveUIBuilder = null)`

What it does:
- Wires `Splat` to DryIoc, initializes ReactiveUI for Avalonia, and lets you register services on the container.

### ReactiveUI.Avalonia.Microsoft.Extensions.DependencyInjection

Extension methods on `AppBuilder` (namespace `ReactiveUI.Avalonia.Splat`):
- `UseReactiveUIWithMicrosoftDependencyResolver(Action<IServiceCollection> containerConfig, Action<IServiceProvider?>? withResolver = null)`
- `UseReactiveUIWithMicrosoftDependencyResolver(Action<IServiceCollection> containerConfig, Action<IServiceProvider?>? withResolver = null, Action<ReactiveUIBuilder>? withReactiveUIBuilder = null)`

What it does:
- Sets up `Splat` using `IServiceCollection`/`ServiceProvider`, initializes ReactiveUI for Avalonia, and exposes the built provider if you need it.

### ReactiveUI.Avalonia.Ninject

Extension methods on `AppBuilder` (namespace `ReactiveUI.Avalonia.Splat`):
- `UseReactiveUIWithNinject(Action<StandardKernel> containerConfig)`
- `UseReactiveUIWithNinject(Action<StandardKernel> containerConfig, Action<ReactiveUIBuilder>? withReactiveUIBuilder = null)`

What it does:
- Wires `Splat` to Ninject, initializes ReactiveUI for Avalonia, and lets you configure bindings on the kernel.

---
## Tutorial: Mastering ReactiveUI with Avalonia

Welcome to the `ReactiveUI.Avalonia` guide! This tutorial walks you through setting up an Avalonia app with ReactiveUI. We start with the basics and build up to a reactive application.

`ReactiveUI.Avalonia` provides the necessary bindings and helpers to seamlessly integrate the ReactiveUI MVVM framework with your Avalonia projects, enabling elegant, testable, and maintainable code.

### Chapter 1: Getting Started - Your First Reactive View

#### 1. Installation

Add the `ReactiveUI.Avalonia` package to your Avalonia application project file.

```xml
<PackageReference Include="ReactiveUI.Avalonia" Version="11.3.0" />
```

#### 2. Initialization (recommended)

Use the builder-based setup shown above (see "Recommended setup"). For a minimal variant:

```csharp
AppBuilder.Configure<App>()
    .UsePlatformDetect()
    .UseReactiveUI()
    .RegisterReactiveUIViewsFromEntryAssembly();
```

#### 3. Create a ViewModel

```csharp
using ReactiveUI;

public class MyViewModel : ReactiveObject
{
    private string _greeting;

    public string Greeting
    {
        get => _greeting;
        set => this.RaiseAndSetIfChanged(ref _greeting, value);
    }

    public MyViewModel() => Greeting = "Hello, Reactive World!";
}
```

#### 4. Create a Reactive View

```xml
<!-- MainView.axaml -->
<UserControl x:Class="MyAvaloniaApp.Views.MainView"
             xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:rxui="using:ReactiveUI.Avalonia">
    <StackPanel HorizontalAlignment="Center" VerticalAlignment="Center">
        <TextBlock x:Name="GreetingTextBlock" FontSize="24"/>
    </StackPanel>
</UserControl>
```

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
            this.OneWayBind(ViewModel, vm => vm.Greeting, v => v.GreetingTextBlock.Text)
                .DisposeWith(disposables);
        });
    }
}
```

### Chapter 2: Handling User Interaction with ReactiveCommands

Add a command to the view model and bind it in the view.

```csharp
using ReactiveUI;
using System;
using System.Reactive;

public class MyViewModel : ReactiveObject
{
    public ReactiveCommand<Unit, Unit> GenerateGreetingCommand { get; }

    private string _greeting = "Hello, Reactive World!";
    public string Greeting
    {
        get => _greeting;
        set => this.RaiseAndSetIfChanged(ref _greeting, value);
    }

    public MyViewModel()
    {
        GenerateGreetingCommand = ReactiveCommand.Create(() =>
        {
            Greeting = $"Hello from Avalonia! The time is {DateTime.Now.ToLongTimeString()}";
        });
    }
}
```

```xml
<StackPanel HorizontalAlignment="Center" VerticalAlignment="Center">
    <TextBlock x:Name="GreetingTextBlock" FontSize="24"/>
    <Button x:Name="GenerateGreetingButton" Content="Generate" Margin="0,20,0,0"/>
</StackPanel>
```

```csharp
this.WhenActivated(disposables =>
{
    this.OneWayBind(ViewModel, vm => vm.Greeting, v => v.GreetingTextBlock.Text)
        .DisposeWith(disposables);

    this.BindCommand(ViewModel, vm => vm.GenerateGreetingCommand, v => v.GenerateGreetingButton)
        .DisposeWith(disposables);
});
```

### Chapter 3: Navigating with `RoutedViewHost`

Set up a router and display views based on navigation state.

```csharp
using ReactiveUI;

public class AppViewModel : ReactiveObject, IScreen
{
    public RoutingState Router { get; } = new RoutingState();

    public AppViewModel() => Router.Navigate.Execute(new MyViewModel());
}
```

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

Register views manually or scan assemblies:

```csharp
using ReactiveUI;
using ReactiveUI.Avalonia;
using Splat;

Locator.CurrentMutable.Register(() => new MainView(), typeof(IViewFor<MyViewModel>));
// or:
AppBuilder.Configure<App>().UseReactiveUI().RegisterReactiveUIViewsFromEntryAssembly();
```

---
## Thanks

We want to thank the following contributors and libraries that help make ReactiveUI.Avalonia possible:

### Core Libraries

  - **Avalonia UI**: [Avalonia](https://avaloniaui.net/) - The cross-platform UI framework.
  - **System.Reactive**: [Reactive Extensions for .NET](https://github.com/dotnet/reactive) - The foundation of ReactiveUI's asynchronous API.
  - **Splat**: [Splat](https://github.com/reactiveui/splat) - Cross-platform utilities and service location.
  - **ReactiveUI**: [ReactiveUI](https://github.com/reactiveui/reactiveui) - The core MVVM framework.

---

## Sponsorship

The core team members, ReactiveUI contributors and contributors in the ecosystem do this open-source work in their free time. If you use ReactiveUI, a serious task, and you'd like us to invest more time on it, please donate. This project increases your income/productivity too. It makes development and applications faster and it reduces the required bandwidth.

[Become a sponsor](https://github.com/sponsors/reactivemarbles).

This is how we use the donations:

  * Allow the core team to work on ReactiveUI
  * Thank contributors if they invested a large amount of time in contributing
  * Support projects in the ecosystem

---
## Support

If you have a question, please see if any discussions in our [GitHub Discussions](https://github.com/reactiveui/ReactiveUI.Avalonia/discussions) or [GitHub issues](https://github.com/reactiveui/ReactiveUI.Avalonia/issues) have already answered it.

If you want to discuss something or just need help, here is our [Slack room](https://reactiveui.net/slack), where there are always individuals looking to help out!

Please do not open GitHub issues for support requests.

Please do not open GitHub issues for general support requests.

---
## Contribute

ReactiveUI.Avalonia is developed under an OSI-approved open source license, making it freely usable and distributable, even for commercial use.

If you want to submit pull requests please first open a [GitHub issue](https://github.com/reactiveui/ReactiveUI.Avalonia/issues/new/choose) to discuss. We are first time PR contributors friendly.

See [Contribution Guidelines](https://www.reactiveui.net/contribute/) for further information how to contribute changes.

---
## License

ReactiveUI.Avalonia is licensed under the [MIT License](https://github.com/reactiveui/ReactiveUI.Avalonia/blob/main/LICENSE).
