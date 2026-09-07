# ReactiveUI.Avalonia Example

This example is a small desktop showcase for the lean ReactiveUI.Avalonia package graph. It uses `ReactiveUI`, `ReactiveUI.Primitives`, project-referenced `ReactiveUI.Avalonia`, and the Microsoft dependency resolver integration.

It targets .NET 10 and runs as a normal, untrimmed desktop application. Traditional expression-based ReactiveUI binding and activation use reflection; this example does not claim Native AOT compatibility. Package projects retain their declared trimming/AOT analysis.

Run it from the `src` folder:

```powershell
dotnet run --project examples/ReactiveUI.Avalonia.Example/ReactiveUI.Avalonia.Example.csproj -c Release
```

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
4. Close and reopen the application. Your input and threshold return. The small JSON file lives at `Environment.SpecialFolder.LocalApplicationData/ReactiveUI.Avalonia.Example/session.json`. Storage errors are reported in the sidebar, and the shutdown deferral is always released.

`Program` composes the Microsoft DI provider and registers all views before it is built. The core, Autofac, DryIoc, Microsoft DI, and Ninject integrations are covered by the package tests; this single application chooses Microsoft DI to keep the walkthrough compact. The executable has no `ReactiveUI.Primitives.Reactive` or System.Reactive dependency.

The matching headless TUnit project tests the production app configuration, routing, automatically resolved controls, actual button input, activation cleanup, UI-thread delivery, async errors, and session storage:

```powershell
dotnet test --project examples/ReactiveUI.Avalonia.Example.Tests/ReactiveUI.Avalonia.Example.Tests.csproj -c Release
```

The test executor awaits `Dispatch<bool>(Func<Task<bool>>)` and explicitly shares the application's dispatcher. Using an async delegate with the synchronous headless dispatch overload can return before assertions finish; recreating the dispatcher also invalidates cached UI schedulers. Session storage tests exercise persistence deferrals without shutting down the shared test dispatcher.
