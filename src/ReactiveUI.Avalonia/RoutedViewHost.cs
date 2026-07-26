// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
#if REACTIVE_SHIM
namespace ReactiveUI.Avalonia.Reactive;
#else
namespace ReactiveUI.Avalonia;
#endif

/// <summary>Hosts the view associated with the current ReactiveUI routing state.</summary>
/// <remarks>
/// <para>
/// ReactiveUI routing consists of an IScreen containing a RoutingState, several IRoutableViewModels, and a
/// platform-specific RoutedViewHost control.
/// </para>
/// <para>
/// RoutingState manages the view-model navigation stack. RoutedViewHost monitors that state and embeds the view for the
/// current view model.
/// </para>
/// <para>Bind IScreen.Router to RoutedViewHost.Router in a view whose view model implements IScreen.</para>
/// <para>
/// See <see href="https://reactiveui.net/docs/handbook/routing/">ReactiveUI routing documentation</see>.
/// </para>
/// </remarks>
public class RoutedViewHost : TransitioningContentControl, IActivatableView, IEnableLogger
{
    /// <summary>Identifies the Router styled property for the associated RoutedViewHost control.</summary>
    /// <remarks>
    /// Bind or observe this property to update navigation-aware controls when the routing state changes.
    /// </remarks>
    public static readonly StyledProperty<RoutingState?> RouterProperty =
        AvaloniaProperty.Register<RoutedViewHost, RoutingState?>(nameof(Router));

    /// <summary>Identifies the ViewContract styled property used to resolve views in the routed view host.</summary>
    /// <remarks>
    /// The contract distinguishes multiple views registered for the same view-model type. A null contract selects the
    /// default view.
    /// </remarks>
    public static readonly StyledProperty<string?> ViewContractProperty =
        AvaloniaProperty.Register<RoutedViewHost, string?>(nameof(ViewContract));

    /// <summary>Identifies the default content property for the <see cref="RoutedViewHost"/> control.</summary>
    /// <remarks>
    /// Use this in XAML to set or bind the content shown when no view is available for the current view model.
    /// </remarks>
    public static readonly StyledProperty<object?> DefaultContentProperty =
        ViewModelViewHost.DefaultContentProperty.AddOwner<RoutedViewHost>();

    /// <summary>Stores the active navigation subscriptions while the host is attached to the visual tree.</summary>
    private CompositeDisposable? _navigationDisposables;

    /// <summary>Gets or sets the current routing state for the router, if available.</summary>
    public RoutingState? Router
    {
        get => GetValue(RouterProperty);
        set => SetValue(RouterProperty, value);
    }

    /// <summary>Gets or sets the name of the view contract associated with this element.</summary>
    public string? ViewContract
    {
        get => GetValue(ViewContractProperty);
        set => SetValue(ViewContractProperty, value);
    }

    /// <summary>Gets or sets the default content to display when no explicit content is provided.</summary>
    public object? DefaultContent
    {
        get => GetValue(DefaultContentProperty);
        set => SetValue(DefaultContentProperty, value);
    }

    /// <summary>Gets or sets the view locator used to resolve views for view models.</summary>
    /// <remarks>
    /// Assign an <see cref="IViewLocator"/> to customize view resolution. A null value uses the default locator.
    /// </remarks>
    public IViewLocator? ViewLocator { get; set; }

    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(TransitioningContentControl);

    /// <summary>Navigates to the view associated with the specified view model and contract.</summary>
    /// <remarks>
    /// Missing routers, view models, or views display the default content. A resolved view receives the supplied view
    /// model through both ViewModel and DataContext when supported.
    /// </remarks>
    /// <param name="viewModel">The view model to display, or null to display the default content.</param>
    /// <param name="contract">The optional view contract used during resolution.</param>
    internal void NavigateToViewModel(object? viewModel, string? contract)
    {
        if (Router is null)
        {
            this.Log().Warn("Router property is null. Falling back to default content.");
            Content = DefaultContent;
            return;
        }

        if (viewModel is null)
        {
            this.Log().Info("ViewModel is null. Falling back to default content.");
            Content = DefaultContent;
            return;
        }

        var viewLocator = ViewLocator ?? CurrentViewLocator.Current;
        var viewInstance = viewLocator.ResolveView(viewModel, contract);
        if (viewInstance is null)
        {
            LogMissingView(viewModel, contract);
            Content = DefaultContent;
            return;
        }

        var resolvedMessage = contract is null
            ? $"Ready to show {viewInstance} with autowired {viewModel}."
            : $"Ready to show {viewInstance} with autowired {viewModel} and contract '{contract}'.";
        this.Log().Info(resolvedMessage);

        viewInstance.ViewModel = viewModel;
        if (viewInstance is IDataContextProvider provider)
        {
            provider.DataContext = viewModel;
        }

        Content = viewInstance;
    }

    /// <summary>Disposes the active navigation subscriptions when they exist.</summary>
    internal void DisposeNavigationDisposables()
    {
        var disposables = _navigationDisposables;
        _navigationDisposables = null;

        if (disposables is null)
        {
            return;
        }

        disposables.Dispose();
    }

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) =>
        _navigationDisposables ??= CreateNavigationDisposables(e);

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        DisposeNavigationDisposables();
    }

    /// <summary>Creates the active navigation subscriptions for an attached host.</summary>
    /// <param name="e">The visual tree attachment event arguments.</param>
    /// <returns>The created navigation subscriptions.</returns>
    private CompositeDisposable CreateNavigationDisposables(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        var disposables = new CompositeDisposable();
        IObservable<object?> routerChanges = this.GetObservable(RouterProperty);
        var routerRemoved = routerChanges
            .Where(static router => router is null);

        var viewContract = this.GetObservable(ViewContractProperty);

        var navigation = this.GetObservable(RouterProperty)
            .Where(static router => router is not null)
            .SelectMany(static router => router!.CurrentViewModel)
            .Merge(routerRemoved)
            .CombineLatest(viewContract, static (viewModel, contract) => new NavigationTarget(viewModel, contract));

        var subscription = PrimitivesLinqExtensions.SubscribeSafe(
            navigation,
            target => NavigateToViewModel(target.ViewModel, target.Contract),
            SubscriptionErrors.Throw);

        disposables.Add(subscription);
        return disposables;
    }

    /// <summary>Logs a missing view resolution result.</summary>
    /// <param name="viewModel">The view model that could not be resolved.</param>
    /// <param name="contract">The optional view contract.</param>
    private void LogMissingView(object viewModel, string? contract)
    {
        if (contract is null)
        {
            this.Log().Warn($"Couldn't find view for '{viewModel}'. Is it registered? Falling back to default content.");
            return;
        }

        this.Log().Warn($"Couldn't find view with contract '{contract}' for '{viewModel}'. Is it registered? Falling back to default content.");
    }

    /// <summary>Represents a pending navigation target.</summary>
    /// <param name="ViewModel">The view model to display.</param>
    /// <param name="Contract">The optional view contract.</param>
    private readonly record struct NavigationTarget(object? ViewModel, string? Contract);
}
