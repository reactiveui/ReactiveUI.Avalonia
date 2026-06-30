// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
#if REACTIVE_SHIM
namespace ReactiveUI.Avalonia.Reactive;
#else
namespace ReactiveUI.Avalonia;
#endif

/// <summary>Automatically loads and displays the view associated with the ViewModel property.</summary>
public class ViewModelViewHost : TransitioningContentControl, IViewFor, IEnableLogger
{
    /// <summary>Identifies the ViewModel dependency property for the ViewModelViewHost control.</summary>
    /// <remarks>This field is used to reference the ViewModel property in property system operations, such as data binding or property change notifications. It is typically used when interacting with Avalonia's property system APIs.</remarks>
    public static readonly AvaloniaProperty<object?> ViewModelProperty =
        AvaloniaProperty.Register<ViewModelViewHost, object?>(nameof(ViewModel));

    /// <summary>Identifies the ViewContract dependency property for the ViewModelViewHost control.</summary>
    /// <remarks>This field is used to register and reference the ViewContract property within Avalonia's property system. It enables styling, binding, and change notification for the ViewContract property in XAML and code.</remarks>
    public static readonly StyledProperty<string?> ViewContractProperty =
        AvaloniaProperty.Register<ViewModelViewHost, string?>(nameof(ViewContract));

    /// <summary>Identifies the default content property for the ViewModelViewHost control.</summary>
    /// <remarks>This property can be used to set or retrieve the default content displayed by the ViewModelViewHost when no view model is present. It is typically used in XAML bindings or when customizing the control's appearance.</remarks>
    public static readonly StyledProperty<object?> DefaultContentProperty =
        AvaloniaProperty.Register<ViewModelViewHost, object?>(nameof(DefaultContent));

    /// <summary>Stores the active navigation subscriptions while the host is attached to the visual tree.</summary>
    private CompositeDisposable? _navigationDisposables;

    /// <summary>Gets or sets the data context for the control.</summary>
    /// <remarks>Assigning a view model to this property enables the control to bind its UI elements to properties and commands exposed by the view model. Changing the value will update data bindings accordingly. This property is commonly used in frameworks that support the Model-View-ViewModel pattern.</remarks>
    public object? ViewModel
    {
        get => GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    /// <summary>Gets or sets the name of the view contract associated with this element.</summary>
    public string? ViewContract
    {
        get => GetValue(ViewContractProperty);
        set => SetValue(ViewContractProperty, value);
    }

    /// <summary>Gets or sets the default content to display when no explicit content is provided.</summary>
    /// <remarks>The value can be any object, such as a string, UI element, or data model, depending on the context in which the property is used. If set to <see langword="null"/>, no default content will be shown.</remarks>
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
        var viewModel = this.GetObservable(ViewModelProperty)
            .CombineLatest(
                this.GetObservable(ViewContractProperty),
                static (viewModel, contract) => new NavigationTarget(viewModel, contract));

        var subscription = PrimitivesLinqExtensions.SubscribeSafe(viewModel, target => NavigateToViewModel(target.ViewModel, target.Contract), SubscriptionErrors.Throw);

        disposables.Add(subscription);
        return disposables;
    }

    /// <summary>Navigates to the view associated with the specified view model and contract.</summary>
    /// <remarks>If no view can be resolved for the provided view model and contract, the content falls back to the default. The resolved view's data context and view model are set to the provided instance, ensuring proper binding. Logging is performed to indicate navigation actions and fallback scenarios.</remarks>
    /// <param name="viewModel">The view model instance for which to resolve and display the corresponding view. If <paramref name="viewModel"/> is <see langword="null"/>, the default content is shown.</param>
    /// <param name="contract">An optional contract string used to distinguish between multiple views for the same view model type. If <paramref name="contract"/> is <see langword="null"/>, the default view is used.</param>
    private void NavigateToViewModel(object? viewModel, string? contract)
    {
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
        if (viewInstance is StyledElement styled)
        {
            styled.DataContext = viewModel;
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
