# ReactiveUI.Avalonia Example

This example shows the lean ReactiveUI.Avalonia package graph on every platform that Avalonia ships an official head for. It uses `ReactiveUI`, `ReactiveUI.Primitives`, project-referenced `ReactiveUI.Avalonia`, and the Microsoft dependency resolver integration.

It targets .NET 10 and runs untrimmed. Traditional expression-based ReactiveUI binding and activation use reflection. This example does not claim trim or Native AOT compatibility. Package projects keep their own trimming and AOT analysis.

## Projects

| Project | Target | Role |
| --- | --- | --- |
| `ReactiveUI.Avalonia.Example` | `net10.0` | Shared library: `App`, views, view models, models and services. The root view is the `MainView` user control. |
| `ReactiveUI.Avalonia.Example.Desktop` | `net10.0` | Windows, macOS and Linux head. `MainWindow` hosts `MainView`. |
| `ReactiveUI.Avalonia.Example.Browser` | `net10.0-browser` | WebAssembly head. |
| `ReactiveUI.Avalonia.Example.Android` | `net10.0-android` | Android head. |
| `ReactiveUI.Avalonia.Example.iOS` | `net10.0-ios` | iOS head. It builds only on macOS. |
| `ReactiveUI.Avalonia.Example.Tests` | `net10.0` | Headless TUnit tests for the shared library. |

`App.OnFrameworkInitializationCompleted` handles both lifetimes. A desktop lifetime gets a `MainWindow`. A single-view lifetime (browser, Android, iOS) gets a `MainView`.

`ExampleAppBuilderExtensions` holds the registration every head shares: the Inter font, the Microsoft dependency resolver, the core ReactiveUI services and the view registrations. A head adds only its platform backend:

```csharp
ExampleAppBuilderExtensions.Create().UsePlatformDetect().LogToTrace();   // Desktop
ExampleAppBuilderExtensions.Create();                                    // Browser
base.CustomizeAppBuilder(builder).UseShowcase();                         // Android and iOS
```

## Run it

Run every command from the `src` folder.

```powershell
# Desktop
dotnet run --project examples/ReactiveUI.Avalonia.Example.Desktop/ReactiveUI.Avalonia.Example.Desktop.csproj -c Release

# Browser. This needs the wasm-tools workload: dotnet workload restore ReactiveUI.Avalonia.slnx
dotnet run --project examples/ReactiveUI.Avalonia.Example.Browser/ReactiveUI.Avalonia.Example.Browser.csproj -c Release

# Android. This needs the android workload and a running emulator or a connected device.
dotnet run --project examples/ReactiveUI.Avalonia.Example.Android/ReactiveUI.Avalonia.Example.Android.csproj -f net10.0-android

# iOS. This needs macOS, Xcode and the ios workload. It runs on a simulator.
dotnet build examples/ReactiveUI.Avalonia.Example.iOS/ReactiveUI.Avalonia.Example.iOS.csproj -f net10.0-ios -t:Run
```

`dotnet workload restore ReactiveUI.Avalonia.slnx` installs the workloads the solution needs on the current OS.

## Platform notes

- **Per-OS builds.** The iOS project sets `TargetFrameworks` to `net10.0-ios` only on macOS. On Windows and Linux it compiles a small placeholder for `net10.0`, so `dotnet build ReactiveUI.Avalonia.slnx` works on every OS. The pattern comes from ReactiveUI's `platform-apple` example.
- **Running tests.** Use `dotnet test --project` for each test project, or a solution filter. `dotnet test --solution ReactiveUI.Avalonia.slnx` asks the Android head for a device and fails when none is attached. CI passes `testProjects` to select the test projects.

## Browser limitations

- **Metrics.** `Process` is not supported in a browser. `App.RegisterViews` registers `BrowserMachineMetricsService` there. It reports managed memory (`GC.GetTotalMemory`), the gen0 collection count (`GC.CollectionCount`) and uptime (`Environment.TickCount64`). The CPU percentage is always 0. The thread count is always 1. The working-set card shows managed memory.
- **Session storage.** A browser has no `LocalApplicationData` folder. `ShowcaseSession.DefaultPath()` returns `null` there, so the session does nothing. The input and threshold reset on each launch. The sidebar says so.
- **Single-view platforms.** Android and iOS also use a single-view lifetime. That lifetime has no exit event for `AutoSuspendHelper`, so they do not persist the session either. Only the desktop lifetime does.
- **Trimming.** The browser head sets `PublishTrimmed` to `false`. Trimming the showcase gives 12 IL2026 warnings, all from its reflection-based bindings.

## What it shows

The app demonstrates:

- `RoutedViewHost` navigation with home, metrics, commands, back, and reset flows.
- `ViewModelViewHost` resolution with explicit view contracts for reusable metric cards.
- Live local machine measurements from `Process`, `GC`, and `Environment` without network or fake feed data.
- One-way and two-way bindings, `ReactiveCommand` can-execute, async execution, error reporting, and interactions.
- Computed state with `ObservableAsPropertyHelper`.
- `WhenActivated` lifetime wiring in views and view models.
- Avalonia property subjects through `GetSubject` and `GetBindingSubject` on a color intensity preview.
- `AutoDataTemplateBindingHook` resolves the overview's feature rows from an `ItemsControl` with no explicit item template.
- `AutoSuspendHelper` restores the command input and CPU threshold on launch, persists them on exit, and invalidates the saved state after an unhandled exception.

Try these flows:

1. Open **Live metrics**. The CPU, working-set, and managed-heap cards show measurements from this application process. The sample count advances every second while this page is active. Move the threshold slider and watch the computed warning label and property-subject preview.
2. Open **Command lab**. Clear the input to disable **Run async command**, then enter a work item and run it. The command captures its input before awaiting work. **Trigger failure** deliberately throws; `ThrownExceptions` delivers the error as a value, and the active view handles an `Interaction` to acknowledge it inline.
3. Use **Back** and **Reset**. Navigation starts before the host attaches, exercising the initial-route regression. Leaving a page disposes its activation subscriptions and bindings; returning reconnects them.
4. On desktop, close and reopen the application. Your input and threshold return. The small JSON file lives at `Environment.SpecialFolder.LocalApplicationData/ReactiveUI.Avalonia.Example/session.json`. Storage errors are reported in the sidebar, and the shutdown deferral is always released.

`ExampleAppBuilderExtensions` composes the Microsoft DI provider and registers all views before the app is built. The core, Autofac, DryIoc, Microsoft DI, and Ninject integrations are covered by the package tests; this single application chooses Microsoft DI to keep the walkthrough compact. The executable has no `ReactiveUI.Primitives.Reactive` or System.Reactive dependency.

The matching headless TUnit project tests the production app configuration, the single-view lifetime path, the browser metrics service, routing, automatically resolved controls, actual button input, activation cleanup, UI-thread delivery, async errors, and session storage:

```powershell
dotnet test --project examples/ReactiveUI.Avalonia.Example.Tests/ReactiveUI.Avalonia.Example.Tests.csproj -c Release
```

The test executor awaits `Dispatch<bool>(Func<Task<bool>>)` and explicitly shares the application's dispatcher. Using an async delegate with the synchronous headless dispatch overload can return before assertions finish; recreating the dispatcher also invalidates cached UI schedulers. Session storage tests exercise persistence deferrals without shutting down the shared test dispatcher.
