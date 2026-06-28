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
/// <para>ReactiveUI routing consists of an IScreen that contains current RoutingState, several IRoutableViewModels, and a platform-specific XAML control called RoutedViewHost.</para>
/// <para>RoutingState manages the ViewModel navigation stack and allows ViewModels to navigate to other ViewModels. IScreen is the root of a navigation stack; despite the name, its views do not have to occupy the whole screen. RoutedViewHost monitors an instance of RoutingState, responding to any changes in the navigation stack by creating and embedding the appropriate view.</para>
/// <para>Place this control in a view containing a ViewModel that implements IScreen, and bind IScreen.Router to RoutedViewHost.Router.</para>
/// <para>See <see href="https://reactiveui.net/docs/handbook/routing/">ReactiveUI routing documentation website</see> for more info.</para>
/// </remarks>
public class RoutedViewHost : TransitioningContentControl, IActivatableView, IEnableLogger
{
    /// <summary>Identifies the Router styled property for the associated RoutedViewHost control.</summary>
    /// <remarks>This property is typically used to bind or observe the navigation state within a RoutedViewHost. Changes to the routing state can trigger view transitions or updates in navigation-aware controls.</remarks>
    public static readonly StyledProperty<RoutingState?> RouterProperty =
        AvaloniaProperty.Register<RoutedViewHost, RoutingState?>(nameof(Router));

    /// <summary>Identifies the ViewContract styled property used to resolve views in the routed view host.</summary>
    /// <remarks>Set this property to control which view implementation is selected when navigating between views. The contract name is typically used to distinguish between multiple views registered for the same view model type. If the property is null, the default view will be used.</remarks>
    public static readonly StyledProperty<string?> ViewContractProperty =
        AvaloniaProperty.Register<RoutedViewHost, string?>(nameof(ViewContract));

    /// <summary>Identifies the default content property for the <see cref="RoutedViewHost"/> control.</summary>
    /// <remarks>This property enables styling and binding of the content displayed by <see cref="RoutedViewHost"/>. It is typically used in XAML to set or bind the content shown when no view is available for the current view model.</remarks>
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
    /// <remarks>Assign an implementation of <see cref="IViewLocator"/> to customize how views are located and instantiated. If <see langword="null"/>, the default view resolution strategy will be used.</remarks>
    public IViewLocator? ViewLocator { get; set; }

    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(TransitioningContentControl);

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _ = LazyInitializer.EnsureInitialized(ref _navigationDisposables, () => CreateNavigationDisposables(e));
    }

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
            .Where(router => router is null);

        var viewContract = this.GetObservable(ViewContractProperty);

        var navigation = this.GetObservable(RouterProperty)
            .Where(router => router is not null)
            .SelectMany(router => router!.CurrentViewModel)
            .Merge(routerRemoved)
            .CombineLatest(viewContract, static (viewModel, contract) => new NavigationTarget(viewModel, contract));

        var subscription = PrimitivesLinqExtensions.SubscribeSafe(navigation, target => NavigateToViewModel(target.ViewModel, target.Contract), SubscriptionErrors.Throw);

        disposables.Add(subscription);
        return disposables;
    }

    /// <summary>Navigates to the view associated with the specified view model and contract.</summary>
    /// <remarks>If the router or view model is null, or if no view can be resolved for the given view model and contract, the method falls back to displaying the default content. The resolved view's ViewModel and DataContext, if applicable, are set to the provided view model.</remarks>
    /// <param name="viewModel">The view model instance for which to resolve and display a corresponding view. If null, the default content is shown.</param>
    /// <param name="contract">An optional contract string used to distinguish between multiple views for the same view model type. If null, the default view resolution is used.</param>
    private void NavigateToViewModel(object? viewModel, string? contract)
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

    /// <summary>Disposes the active navigation subscriptions when they exist.</summary>
    private void DisposeNavigationDisposables()
    {
        var disposables = _navigationDisposables;
        _navigationDisposables = null;

        if (disposables is null)
        {
            return;
        }

        disposables.Dispose();
    }

    /// <summary>Represents a pending navigation target.</summary>
    /// <param name="ViewModel">The view model to display.</param>
    /// <param name="Contract">The optional view contract.</param>
    private readonly record struct NavigationTarget(object? ViewModel, string? Contract);
}
