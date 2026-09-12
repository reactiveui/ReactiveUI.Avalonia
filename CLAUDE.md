# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Test Commands

This project uses **Microsoft Testing Platform (MTP)** with the **TUnit** testing framework. Test commands differ significantly from traditional VSTest.

See: https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-test?tabs=dotnet-test-with-mtp

### Prerequisites

```powershell
# Check .NET installation
dotnet --info

# Restore NuGet packages
dotnet restore ReactiveUI.Avalonia.slnx
```

**Note**: This repository uses **SLNX** (XML-based solution format) instead of the legacy SLN format.

### Build Commands

**CRITICAL:** The working folder must be `./src`. The `global.json` that configures MTP as the test runner lives there, so running from the repository root or any other directory will cause MTP to not be recognized and tests will fail or hang.

```powershell
# Build the solution
dotnet build ReactiveUI.Avalonia.slnx -c Release

# Build with warnings as errors (includes analyzer violations)
dotnet build ReactiveUI.Avalonia.slnx -c Release -warnaserror

# Clean the solution
dotnet clean ReactiveUI.Avalonia.slnx
```

### Test Commands (Microsoft Testing Platform)

**CRITICAL:** The working folder must be `./src`. TUnit/MTP arguments are passed directly — no `--` separator needed.

**IMPORTANT:**
- Do NOT use `--no-build` flag when running tests. Always build before testing to ensure all code changes (including test changes) are compiled. Using `--no-build` can cause tests to run against stale binaries and produce misleading results.

```powershell
# Run all tests in the solution
dotnet test --solution ReactiveUI.Avalonia.slnx -c Release

# Run all tests in a specific project
dotnet test --project tests/ReactiveUI.Avalonia.Tests/ReactiveUI.Avalonia.Tests.csproj -c Release

# Run a single test method using treenode-filter
# Syntax: /{AssemblyName}/{Namespace}/{ClassName}/{TestMethodName}
dotnet test --project tests/ReactiveUI.Avalonia.Tests/ReactiveUI.Avalonia.Tests.csproj --treenode-filter "/*/*/*/MyTestMethod"

# Run all tests in a specific class
dotnet test --project tests/ReactiveUI.Avalonia.Tests/ReactiveUI.Avalonia.Tests.csproj --treenode-filter "/*/*/MyClassName/*"

# Run tests in a specific namespace
dotnet test --project tests/ReactiveUI.Avalonia.Tests/ReactiveUI.Avalonia.Tests.csproj --treenode-filter "/*/MyNamespace/*/*"

# Filter by test property (e.g., Category)
dotnet test --solution ReactiveUI.Avalonia.slnx --treenode-filter "/*/*/*/*[Category=Integration]"

# Run tests with code coverage (Microsoft Code Coverage)
dotnet test --solution ReactiveUI.Avalonia.slnx --coverage --coverage-output-format cobertura

# Run tests with detailed output
dotnet test --solution ReactiveUI.Avalonia.slnx --output Detailed

# List all available tests without running them
dotnet test --project tests/ReactiveUI.Avalonia.Tests/ReactiveUI.Avalonia.Tests.csproj --list-tests

# Fail fast (stop on first failure)
dotnet test --solution ReactiveUI.Avalonia.slnx --fail-fast

# Control parallel test execution
dotnet test --solution ReactiveUI.Avalonia.slnx --maximum-parallel-tests 4

# Generate TRX report
dotnet test --solution ReactiveUI.Avalonia.slnx --report-trx

# Disable logo for cleaner output
dotnet test --project tests/ReactiveUI.Avalonia.Tests/ReactiveUI.Avalonia.Tests.csproj --disable-logo

# Combine options: coverage + TRX report + detailed output
dotnet test --solution ReactiveUI.Avalonia.slnx --coverage --coverage-output-format cobertura --report-trx --output Detailed
```

**Alternative: Using `dotnet run` for single project**
```powershell
# Run tests using dotnet run (easier for passing flags)
dotnet run --project tests/ReactiveUI.Avalonia.Tests/ReactiveUI.Avalonia.Tests.csproj -c Release -- --treenode-filter "/*/*/*/MyTest"

# Disable logo for cleaner output
dotnet run --project tests/ReactiveUI.Avalonia.Tests/ReactiveUI.Avalonia.Tests.csproj -- --disable-logo --treenode-filter "/*/*/*/Test1"
```

### TUnit Treenode-Filter Syntax

The `--treenode-filter` follows the pattern: `/{AssemblyName}/{Namespace}/{ClassName}/{TestMethodName}`

**Examples:**
- Single test: `--treenode-filter "/*/*/*/MyTestMethod"`
- All tests in class: `--treenode-filter "/*/*/MyClassName/*"`
- All tests in namespace: `--treenode-filter "/*/MyNamespace/*/*"`
- Filter by property: `--treenode-filter "/*/*/*/*[Category=Integration]"`
- Multiple wildcards: `--treenode-filter "/*/*/MyTests*/*"`

**Note:** Use single asterisks (`*`) to match segments. Double asterisks (`/**`) are not supported in treenode-filter.

### Key TUnit Command-Line Flags

- `--treenode-filter` - Filter tests by path pattern or properties (syntax: `/{Assembly}/{Namespace}/{Class}/{Method}`)
- `--list-tests` - Display available tests without running
- `--fail-fast` - Stop after first failure
- `--maximum-parallel-tests` - Limit concurrent execution (default: processor count)
- `--coverage` - Enable Microsoft Code Coverage
- `--coverage-output-format` - Set coverage format (cobertura, xml, coverage)
- `--report-trx` - Generate TRX format reports
- `--output` - Control verbosity (Normal or Detailed)
- `--no-progress` - Suppress progress reporting
- `--disable-logo` - Remove TUnit logo display
- `--diagnostic` - Enable diagnostic logging (Trace level)
- `--timeout` - Set global test timeout

See https://tunit.dev/docs/reference/command-line-flags for complete TUnit flag reference.

### Code Coverage Reports

To generate and view a code coverage report:

```powershell
# 1. Clean bin/obj to ensure fresh instrumentation
find . -type d \( -name bin -o -name obj \) -exec rm -rf {} + 2>/dev/null

# 2. Run tests with coverage
dotnet test --project tests/ReactiveUI.Avalonia.Tests/ReactiveUI.Avalonia.Tests.csproj -c Release --coverage --coverage-output-format cobertura --disable-logo --no-progress

# 3. Generate a text summary (requires dotnet-reportgenerator-globaltool)
reportgenerator -reports:"tests/ReactiveUI.Avalonia.Tests/bin/Release/net10.0/TestResults/*.cobertura.xml" -targetdir:/tmp/coverage-report -reporttypes:TextSummary
cat /tmp/coverage-report/Summary.txt

# 4. Generate an HTML report for detailed per-file analysis
reportgenerator -reports:"tests/ReactiveUI.Avalonia.Tests/bin/Release/net10.0/TestResults/*.cobertura.xml" -targetdir:/tmp/coverage-report -reporttypes:Html
```

### Public API Baselines

Every shipping project checks in its public surface as `PublicAPI/{TargetFramework}/PublicAPI.txt`, one baseline per target framework, enforced by `PublicApiSharp.Analyzers`. The baseline is ordinary C# that reads like source, so a reviewer can tell from the diff alone whether an API change is additive.

The build fails when the surface and the baseline disagree:

- `PAS0001` - a public member is missing from the baseline (reported on the declaration)
- `PAS0002` - a baseline entry no longer exists in source (reported on the baseline line)
- `PAS0003` - a member's signature differs from the baseline (reported on the declaration)

There is no shipped/unshipped split. An intentional API change is accepted by regenerating the baseline in the same commit that makes the change.

```powershell
# Regenerate a project's baseline. dotnet format writes ONE baseline per run
# (it fixes a single inner build at a time), so on a multi-targeting project
# repeat the command until the build is clean - once per target framework.
dotnet format analyzers ReactiveUI.Avalonia/ReactiveUI.Avalonia.csproj --diagnostics PAS0001 PAS0003 --severity info

# Adding a new target framework? Create the empty baseline first - the analyzer
# stays silent until the file exists, so a missing one tracks nothing at all.
mkdir -p ReactiveUI.Avalonia/PublicAPI/net12.0 && touch ReactiveUI.Avalonia/PublicAPI/net12.0/PublicAPI.txt
```

**Note:** `PAS0004` (no baseline for this target framework) is reported at `CSC` with no source location, so Roslyn only honours its severity from a *global* analyzer config. The `.editorconfig` entry under `[*.cs]` has no effect, which is why the empty-file step above is manual.

Test projects are excluded via `TrackPublicApi` in `src/Directory.Build.props`.

### Key Configuration Files

- `global.json` - Specifies `"Microsoft.Testing.Platform"` as the test runner
- `testconfig.json` - Configures test execution (`"parallel": false`) and code coverage (Cobertura format)
- `Directory.Build.props` - Enables `TestingPlatformDotnetTestSupport` for test projects, and sets `TrackPublicApi` for public API baselines
- `Directory.Packages.props` - Central package versions; `RoslynCommonAnalyzersVersion` pins StyleSharp, PerformanceSharp and SecuritySharp to a single shared version

## Architecture Overview

ReactiveUI.Avalonia provides platform-specific extensions that integrate ReactiveUI with the Avalonia UI framework, including reactive controls, view activation, command binding, and dependency injection container support.

### Project Structure

- `ReactiveUI.Avalonia/` - Core library with reactive controls and Avalonia integrations; scheduler/sequencer implementations come from ReactiveUI.Primitives.Avalonia
- `ReactiveUI.Avalonia.Autofac/` - Autofac DI container integration
- `ReactiveUI.Avalonia.DryIoc/` - DryIoc DI container integration
- `ReactiveUI.Avalonia.Microsoft.Extensions.DependencyInjection/` - Microsoft DI container integration
- `ReactiveUI.Avalonia.Ninject/` - Ninject DI container integration
- `tests/ReactiveUI.Avalonia.Tests/` - Main unit tests
- `tests/ReactiveUI.Avalonia.DryIoc1.Tests/` - DryIoc integration tests
- `tests/ReactiveUI.Avalonia.Microsoft1.Tests/` - Microsoft DI integration tests

### Key Types

- **`ReactiveUI.Primitives.Avalonia.*` `AvaloniaScheduler`** - Package-owned Avalonia UI-thread implementation; it provides `ISequencer` in the lean build and `IScheduler` in the reactive build
- **`ReactiveUserControl<TViewModel>`** - Base user control with ViewModel/DataContext sync
- **`ReactiveWindow<TViewModel>`** - Base window with ViewModel/DataContext sync
- **`ViewModelViewHost`** - Content control that resolves and displays views for a given ViewModel
- **`RoutedViewHost`** - Content control for routing-based navigation
- **`AppBuilderExtensions`** - Extension methods for Avalonia AppBuilder to register ReactiveUI
- **`AvaloniaObjectReactiveExtensions`** - GetSubject/GetBindingSubject for AvaloniaProperties
- **`AutoSuspendHelper`** - Wires Avalonia application lifetime events to ReactiveUI suspension
- **`AvaloniaMixins`** - Per-DI-container extension methods (UseReactiveUIWithDryIoc, etc.)

## Code Style & Quality Requirements

**CRITICAL:** All code must comply with ReactiveUI contribution guidelines: https://www.reactiveui.net/contribute/index.html

### Style Enforcement

- EditorConfig rules (`.editorconfig`) - comprehensive C# formatting and naming conventions
- StyleSharp (SST), PerformanceSharp (PSH) and SecuritySharp (SES) analyzers - builds fail on violations
- **All elements require XML documentation comments** (public, internal, and private)
- **No `#pragma` directives** — use `[SuppressMessage]` attributes when suppression is truly needed

### C# Style Rules

- **Braces:** Allman style (each brace on new line)
- **Indentation:** 4 spaces, no tabs
- **Fields:** `_camelCase` for private/internal, `readonly` where possible
- **Visibility:** Always explicit, visibility first modifier
- **Namespaces:** File-scoped preferred, imports outside namespace
- **Modern C#:** Use nullable reference types, pattern matching, file-scoped namespaces

## Testing Guidelines

- Unit tests use **TUnit** framework with **Microsoft Testing Platform**
- Test projects detected via `IsTestProject` MSBuild property
- Coverage configured in `testconfig.json` (Cobertura format, skip auto-properties)
- Test execution is serial (`"parallel": false` in testconfig.json) due to shared Avalonia state
- Always write unit tests for new features or bug fixes
- Follow existing test patterns in `tests/ReactiveUI.Avalonia.Tests/`
