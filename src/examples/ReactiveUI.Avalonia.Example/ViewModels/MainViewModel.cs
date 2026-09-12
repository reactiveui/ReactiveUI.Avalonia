// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using ReactiveUI;
using ReactiveUI.Avalonia.Example.Services;
using ReactiveUI.Primitives;
using Unit = ReactiveUI.Primitives.RxVoid;

namespace ReactiveUI.Avalonia.Example.ViewModels;

/// <summary>Application shell view model.</summary>
[System.Diagnostics.DebuggerDisplay("MainViewModel: {Router}")]
public sealed class MainViewModel : ViewModelBase, IScreen
{
    /// <summary>The overview page.</summary>
    private OverviewViewModel _overview = null!;

    /// <summary>Initializes a new instance of the <see cref="MainViewModel"/> class.</summary>
    private MainViewModel()
    {
        ShowOverview = Track(ReactiveCommand.Create(() => Navigate(_overview)));
        ShowMetrics = Track(ReactiveCommand.Create(() => Navigate(Metrics)));
        ShowCommands = Track(ReactiveCommand.Create(() => Navigate(Commands)));
        GoBack = Router.NavigateBack;
        Reset = Track(ReactiveCommand.Create(ResetNavigation));
    }

    /// <inheritdoc/>
    public RoutingState Router { get; } = new();

    /// <summary>Gets the metrics page view model.</summary>
    public MetricsViewModel Metrics { get; private set; } = null!;

    /// <summary>Gets the commands page view model.</summary>
    public CommandLabViewModel Commands { get; private set; } = null!;

    /// <summary>Gets the command that navigates to the overview.</summary>
    public ReactiveCommand<Unit, Unit> ShowOverview { get; }

    /// <summary>Gets the command that navigates to the metrics page.</summary>
    public ReactiveCommand<Unit, Unit> ShowMetrics { get; }

    /// <summary>Gets the command that navigates to the command lab.</summary>
    public ReactiveCommand<Unit, Unit> ShowCommands { get; }

    /// <summary>Gets the command that navigates back.</summary>
    public ReactiveCommand<Unit, IRoutableViewModel> GoBack { get; }

    /// <summary>Gets the command that resets navigation.</summary>
    public ReactiveCommand<Unit, Unit> Reset { get; }

    /// <summary>Gets or sets the navigation status.</summary>
    public string NavigationStatus
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = "Navigation is ready.";

    /// <summary>Gets or sets the current persistence status.</summary>
    public string SessionStatus
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = "Session persistence starts with the desktop lifetime.";

    /// <summary>Creates an initialized shell view model.</summary>
    /// <param name="metricsService">The local metrics service.</param>
    /// <returns>The initialized shell view model.</returns>
    [RequiresUnreferencedCode("The shell creates showcase pages that intentionally use ReactiveUI expression-based helpers.")]
    public static MainViewModel Create(ILocalMachineMetricsService metricsService)
    {
        var viewModel = new MainViewModel();
        viewModel.Initialize(metricsService);
        return viewModel;
    }

    /// <summary>Navigates to a page.</summary>
    /// <param name="page">The page to navigate to.</param>
    public void Navigate(IRoutableViewModel page) => _ = Router.Navigate.Execute(page).SubscribeSafe(
        routed => NavigationStatus = $"Navigated to {routed.UrlPathSegment}.",
        HandleNavigationError);

    /// <summary>Resets the navigation stack to the overview page.</summary>
    public void ResetNavigation() => _ = Router.NavigateAndReset.Execute(_overview).SubscribeSafe(
        routed => NavigationStatus = $"Reset to {routed.UrlPathSegment}.",
        HandleNavigationError);

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _overview.Dispose();
        }

        base.Dispose(disposing);
    }

    /// <summary>Handles navigation errors.</summary>
    /// <param name="error">The navigation error.</param>
    private void HandleNavigationError(Exception error) => NavigationStatus = error.Message;

    /// <summary>Initializes routed pages after construction.</summary>
    /// <param name="metricsService">The local metrics service.</param>
    [RequiresUnreferencedCode("The shell creates showcase pages that intentionally use ReactiveUI expression-based helpers.")]
    private void Initialize(ILocalMachineMetricsService metricsService)
    {
        _overview = new(this);
        Metrics = Track(new MetricsViewModel(this, metricsService));
        Commands = Track(new CommandLabViewModel(this));
        ResetNavigation();
    }
}
