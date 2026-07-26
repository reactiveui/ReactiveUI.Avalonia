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
    /// <remarks>
    /// Use this property identifier for Avalonia data binding and property-change operations.
    /// </remarks>
    public static readonly AvaloniaProperty<object?> ViewModelProperty =
        AvaloniaProperty.Register<ViewModelViewHost, object?>(nameof(ViewModel));

    /// <summary>Identifies the ViewContract dependency property for the ViewModelViewHost control.</summary>
    /// <remarks>
    /// This property identifier enables ViewContract styling, binding, and change notification.
    /// </remarks>
    public static readonly StyledProperty<string?> ViewContractProperty =
        AvaloniaProperty.Register<ViewModelViewHost, string?>(nameof(ViewContract));

    /// <summary>Identifies the default content property for the ViewModelViewHost control.</summary>
    /// <remarks>
    /// Use this property identifier to bind the content displayed when no view model is present.
    /// </remarks>
    public static readonly StyledProperty<object?> DefaultContentProperty =
        AvaloniaProperty.Register<ViewModelViewHost, object?>(nameof(DefaultContent));

    /// <summary>Stores the active navigation subscriptions while the host is attached to the visual tree.</summary>
    private CompositeDisposable? _navigationDisposables;

    /// <summary>Gets or sets the data context for the control.</summary>
    /// <remarks>
    /// Assigning a view model updates the view selected by this host and the bindings within that view.
    /// </remarks>
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
    /// <remarks>A null value means that no default content is shown.</remarks>
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
    /// Missing views display the default content. A resolved view receives the supplied view model through ViewModel
    /// and DataContext.
    /// </remarks>
    /// <param name="viewModel">The view model to display, or null to display the default content.</param>
    /// <param name="contract">The optional contract used to distinguish registered views.</param>
    internal void NavigateToViewModel(object? viewModel, string? contract)
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
        var viewModel = this.GetObservable(ViewModelProperty)
            .CombineLatest(
                this.GetObservable(ViewContractProperty),
                static (viewModel, contract) => new NavigationTarget(viewModel, contract));

        var subscription = PrimitivesLinqExtensions.SubscribeSafe(
            viewModel,
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
