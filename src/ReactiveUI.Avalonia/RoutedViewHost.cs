// Copyright (c) 2025 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Disposables.Fluent;

namespace ReactiveUI.Avalonia;

/// <summary>
/// This control hosts the View associated with ReactiveUI RoutingState,
/// and will display the View and wire up the ViewModel whenever a new
/// ViewModel is navigated to. Nested routing is also supported.
/// </summary>
/// <remarks>
/// <para>
/// ReactiveUI routing consists of an IScreen that contains current
/// RoutingState, several IRoutableViewModels, and a platform-specific
/// XAML control called RoutedViewHost.
/// </para>
/// <para>
/// RoutingState manages the ViewModel navigation stack and allows
/// ViewModels to navigate to other ViewModels. IScreen is the root of
/// a navigation stack; despite the name, its views don't have to occupy
/// the whole screen. RoutedViewHost monitors an instance of RoutingState,
/// responding to any changes in the navigation stack by creating and
/// embedding the appropriate view.
/// </para>
/// <para>
/// Place this control to a view containing your ViewModel that implements
/// IScreen, and bind IScreen.Router property to RoutedViewHost.Router property.
/// <code>
/// <![CDATA[
/// <rxui:RoutedViewHost
///     HorizontalAlignment="Stretch"
///     VerticalAlignment="Stretch"
///     Router="{Binding Router}">
///     <rxui:RoutedViewHost.DefaultContent>
///         <TextBlock Text="Default Content"/>
///     </rxui:RoutedViewHost.DefaultContent>
/// </rxui:RoutedViewHost>
/// ]]>
/// </code>
/// </para>
/// <para>
/// See <see href="https://reactiveui.net/docs/handbook/routing/">
/// ReactiveUI routing documentation website</see> for more info.
/// </para>
/// </remarks>
public class RoutedViewHost : TransitioningContentControl, IActivatableView, IEnableLogger
{
    /// <summary>
    /// Identifies the Router styled property, which holds the current routing state for the associated RoutedViewHost
    /// control.
    /// </summary>
    /// <remarks>This property is typically used to bind or observe the navigation state within a
    /// RoutedViewHost. Changes to the routing state can trigger view transitions or updates in navigation-aware
    /// controls.</remarks>
    public static readonly StyledProperty<RoutingState?> RouterProperty =
        AvaloniaProperty.Register<RoutedViewHost, RoutingState?>(nameof(Router));

    /// <summary>
    /// Identifies the ViewContract styled property, which specifies the contract name used to resolve views in the
    /// routed view host.
    /// </summary>
    /// <remarks>Set this property to control which view implementation is selected when navigating between
    /// views. The contract name is typically used to distinguish between multiple views registered for the same view
    /// model type. If the property is null, the default view will be used.</remarks>
    public static readonly StyledProperty<string?> ViewContractProperty =
        AvaloniaProperty.Register<RoutedViewHost, string?>(nameof(ViewContract));

    /// <summary>
    /// Identifies the default content property for the <see cref="RoutedViewHost"/> control.
    /// </summary>
    /// <remarks>This property enables styling and binding of the content displayed by <see
    /// cref="RoutedViewHost"/>. It is typically used in XAML to set or bind the content shown when no view is available
    /// for the current view model.</remarks>
    public static readonly StyledProperty<object?> DefaultContentProperty =
        ViewModelViewHost.DefaultContentProperty.AddOwner<RoutedViewHost>();

    /// <summary>
    /// Initializes a new instance of the <see cref="RoutedViewHost"/> class and sets up reactive navigation between view models and.
    /// views.
    /// </summary>
    /// <remarks>This constructor activates the view host and subscribes to changes in the router and view
    /// contract properties. When the router changes or is removed, the view host automatically navigates to the
    /// appropriate view model. This enables dynamic view navigation in response to application state changes. The
    /// constructor is typically used in reactive UI scenarios where view navigation is managed by a routing
    /// mechanism.</remarks>
    public RoutedViewHost() =>
        this.WhenActivated(disposables =>
        {
            var routerRemoved = this
                                .WhenAnyValue(x => x.Router)
                                .Where(router => router == null)!
                                .Cast<object?>();

            var viewContract = this.WhenAnyValue(x => x.ViewContract);

            this.WhenAnyValue(x => x.Router)
                .Where(router => router != null)
                .SelectMany(router => router!.CurrentViewModel)
                .Merge(routerRemoved)
                .CombineLatest(viewContract)
                .Subscribe(tuple => NavigateToViewModel(tuple.First, tuple.Second))
                .DisposeWith(disposables);
        });

    /// <summary>
    /// Gets or sets the current routing state for the router, if available.
    /// </summary>
    public RoutingState? Router
    {
        get => GetValue(RouterProperty);
        set => SetValue(RouterProperty, value);
    }

    /// <summary>
    /// Gets or sets the name of the view contract associated with this element.
    /// </summary>
    public string? ViewContract
    {
        get => GetValue(ViewContractProperty);
        set => SetValue(ViewContractProperty, value);
    }

    /// <summary>
    /// Gets or sets the default content to display when no explicit content is provided.
    /// </summary>
    public object? DefaultContent
    {
        get => GetValue(DefaultContentProperty);
        set => SetValue(DefaultContentProperty, value);
    }

    /// <summary>
    /// Gets or sets the view locator used to resolve views for view models.
    /// </summary>
    /// <remarks>Assign an implementation of <see cref="IViewLocator"/> to customize how views are located and
    /// instantiated. If <see langword="null"/>, the default view resolution strategy will be used.</remarks>
    public IViewLocator? ViewLocator { get; set; }

    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(TransitioningContentControl);

    /// <summary>
    /// Navigates to the view associated with the specified view model and contract, updating the content to display the
    /// resolved view or a default fallback if no view is found.
    /// </summary>
    /// <remarks>If the router or view model is null, or if no view can be resolved for the given view model
    /// and contract, the method falls back to displaying the default content. The resolved view's ViewModel and
    /// DataContext (if applicable) are set to the provided view model.</remarks>
    /// <param name="viewModel">The view model instance for which to resolve and display a corresponding view. If null, the default content is
    /// shown.</param>
    /// <param name="contract">An optional contract string used to distinguish between multiple views for the same view model type. If null,
    /// the default view resolution is used.</param>
    private void NavigateToViewModel(object? viewModel, string? contract)
    {
        if (Router == null)
        {
            this.Log().Warn("Router property is null. Falling back to default content.");
            Content = DefaultContent;
            return;
        }

        if (viewModel == null)
        {
            this.Log().Info("ViewModel is null. Falling back to default content.");
            Content = DefaultContent;
            return;
        }

        var viewLocator = ViewLocator ?? global::ReactiveUI.ViewLocator.Current;
        var viewInstance = viewLocator.ResolveView(viewModel, contract);
        if (viewInstance == null)
        {
            if (contract == null)
            {
                this.Log()
                    .Warn($"Couldn't find view for '{viewModel}'. Is it registered? Falling back to default content.");
            }
            else
            {
                this.Log()
                    .Warn($"Couldn't find view with contract '{contract}' for '{viewModel}'. Is it registered? Falling back to default content.");
            }

            Content = DefaultContent;
            return;
        }

        if (contract == null)
        {
            this.Log().Info($"Ready to show {viewInstance} with autowired {viewModel}.");
        }
        else
        {
            this.Log().Info($"Ready to show {viewInstance} with autowired {viewModel} and contract '{contract}'.");
        }

        viewInstance.ViewModel = viewModel;
        if (viewInstance is IDataContextProvider provider)
        {
            provider.DataContext = viewModel;
        }

        Content = viewInstance;
    }
}
