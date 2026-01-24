// Copyright (c) 2025 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Disposables.Fluent;

namespace ReactiveUI.Avalonia;

/// <summary>
/// This content control will automatically load the View associated with
/// the ViewModel property and display it. This control is very useful
/// inside a DataTemplate to display the View associated with a ViewModel.
/// </summary>
public class ViewModelViewHost : TransitioningContentControl, IViewFor, IEnableLogger
{
    /// <summary>
    /// Identifies the ViewModel dependency property for the ViewModelViewHost control.
    /// </summary>
    /// <remarks>This field is used to reference the ViewModel property in property system operations, such as
    /// data binding or property change notifications. It is typically used when interacting with Avalonia's property
    /// system APIs.</remarks>
    public static readonly AvaloniaProperty<object?> ViewModelProperty =
        AvaloniaProperty.Register<ViewModelViewHost, object?>(nameof(ViewModel));

    /// <summary>
    /// Identifies the ViewContract dependency property for the ViewModelViewHost control.
    /// </summary>
    /// <remarks>This field is used to register and reference the ViewContract property within Avalonia's
    /// property system. It enables styling, binding, and change notification for the ViewContract property in XAML and
    /// code.</remarks>
    public static readonly StyledProperty<string?> ViewContractProperty =
        AvaloniaProperty.Register<ViewModelViewHost, string?>(nameof(ViewContract));

    /// <summary>
    /// Identifies the default content property for the ViewModelViewHost control.
    /// </summary>
    /// <remarks>This property can be used to set or retrieve the default content displayed by the
    /// ViewModelViewHost when no view model is present. It is typically used in XAML bindings or when customizing the
    /// control's appearance.</remarks>
    public static readonly StyledProperty<object?> DefaultContentProperty =
        AvaloniaProperty.Register<ViewModelViewHost, object?>(nameof(DefaultContent));

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewModelViewHost"/> class and sets up activation logic to automatically navigate
    /// to the appropriate view when the ViewModel or ViewContract changes.
    /// </summary>
    /// <remarks>This constructor subscribes to changes in the ViewModel and ViewContract properties, ensuring
    /// that the view is updated in response to these changes. The activation logic is disposed of when the host is
    /// deactivated, following the ReactiveUI activation pattern.</remarks>
    public ViewModelViewHost() =>
        this.WhenActivated(disposables => this.WhenAnyValue(x => x.ViewModel, x => x.ViewContract)
                                               .Subscribe(tuple => NavigateToViewModel(tuple.Item1, tuple.Item2))
                                               .DisposeWith(disposables));

    /// <summary>
    /// Gets or sets the data context for the control, typically used for data binding in MVVM scenarios.
    /// </summary>
    /// <remarks>Assigning a view model to this property enables the control to bind its UI elements to
    /// properties and commands exposed by the view model. Changing the value will update data bindings accordingly.
    /// This property is commonly used in frameworks that support the Model-View-ViewModel (MVVM) pattern.</remarks>
    public object? ViewModel
    {
        get => GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
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
    /// <remarks>The value can be any object, such as a string, UI element, or data model, depending on the
    /// context in which the property is used. If set to <see langword="null"/>, no default content will be
    /// shown.</remarks>
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
    /// resolved view.
    /// </summary>
    /// <remarks>If no view can be resolved for the provided view model and contract, the content falls back
    /// to the default. The resolved view's data context and view model are set to the provided instance, ensuring
    /// proper binding. Logging is performed to indicate navigation actions and fallback scenarios.</remarks>
    /// <param name="viewModel">The view model instance for which to resolve and display the corresponding view. If <paramref name="viewModel"/>
    /// is <see langword="null"/>, the default content is shown.</param>
    /// <param name="contract">An optional contract string used to distinguish between multiple views for the same view model type. If
    /// <paramref name="contract"/> is <see langword="null"/>, the default view is used.</param>
    private void NavigateToViewModel(object? viewModel, string? contract)
    {
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
        if (viewInstance is StyledElement styled)
        {
            styled.DataContext = viewModel;
        }

        Content = viewInstance;
    }
}
