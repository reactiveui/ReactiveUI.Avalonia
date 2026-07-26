// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Rendering;
using Splat;

using ActivationDisposables = ReactiveUI.Primitives.Disposables.MultipleDisposable;
using ExceptionDispatchInfo = System.Runtime.ExceptionServices.ExceptionDispatchInfo;
using ReactiveIRoutableViewModel = global::ReactiveUI.Reactive.IRoutableViewModel;
using ReactiveIScreen = global::ReactiveUI.Reactive.IScreen;
using ReactiveRoutingState = global::ReactiveUI.Reactive.RoutingState;

namespace ReactiveUI.Avalonia.Reactive.Tests;

/// <summary>Test infrastructure for <see cref="ReactiveShimFullCoverageTests"/>.</summary>
public partial class ReactiveShimFullCoverageTests
{
    /// <summary>The private navigation-disposables field name on reactive view hosts.</summary>
    private const string NavigationDisposablesFieldName = "_navigationDisposables";

    /// <summary>Returns whether private registration helpers produced their expected outcomes.</summary>
    /// <param name="nullResolverFactoryCalled">Whether the null resolver factory ran.</param>
    /// <param name="containerConfigured">Whether the container configuration ran.</param>
    /// <param name="locatorResolved">Whether the locator resolved the registered view.</param>
    /// <param name="fallbackAfterThrow">Whether the view factory used its fallback after resolution failed.</param>
    /// <param name="invalidCreateThrows">Whether invalid view creation throws.</param>
    /// <param name="nullBuilderThrows">Whether a null builder throws.</param>
    /// <returns><see langword="true"/> when all helper outcomes are valid; otherwise, <see langword="false"/>.</returns>
    private static bool IsPrivateHelperSetupValid(
        bool nullResolverFactoryCalled,
        bool containerConfigured,
        bool locatorResolved,
        bool fallbackAfterThrow,
        bool invalidCreateThrows,
        bool nullBuilderThrows) =>
        !nullResolverFactoryCalled
        && containerConfigured
        && locatorResolved
        && fallbackAfterThrow
        && invalidCreateThrows
        && nullBuilderThrows;

    /// <summary>Invokes the application builder callback that completes platform service setup.</summary>
    /// <param name="builder">The application builder.</param>
    private static void InvokeAfterPlatformServicesSetup(AppBuilder builder)
    {
        var property = typeof(AppBuilder).GetProperty(
            "AfterPlatformServicesSetupCallback",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        var callback = (Action<AppBuilder>?)property?.GetValue(builder);
        callback?.Invoke(builder);
    }

    /// <summary>Invokes a ViewModelViewHost private navigation method.</summary>
    /// <param name="host">The host to invoke.</param>
    /// <param name="viewModel">The view model.</param>
    /// <param name="contract">The view contract.</param>
    private static void InvokePrivateNavigation(ViewModelViewHost host, object? viewModel, string? contract)
    {
        var method = typeof(ViewModelViewHost)
            .GetMethod("NavigateToViewModel", BindingFlags.Instance | BindingFlags.NonPublic);
        _ = method!.Invoke(host, [viewModel, contract]);
    }

    /// <summary>Invokes a RoutedViewHost private navigation method.</summary>
    /// <param name="host">The host to invoke.</param>
    /// <param name="viewModel">The view model.</param>
    /// <param name="contract">The view contract.</param>
    private static void InvokePrivateNavigation(RoutedViewHost host, object? viewModel, string? contract)
    {
        var method = typeof(RoutedViewHost)
            .GetMethod("NavigateToViewModel", BindingFlags.Instance | BindingFlags.NonPublic);
        _ = method!.Invoke(host, [viewModel, contract]);
    }

    /// <summary>Invokes the private reactive ViewModelViewHost navigation disposal helper.</summary>
    /// <param name="host">The host instance.</param>
    private static void InvokeDisposeNavigationDisposables(ViewModelViewHost host)
    {
        var method = typeof(ViewModelViewHost)
            .GetMethod("DisposeNavigationDisposables", BindingFlags.Instance | BindingFlags.NonPublic);

        _ = method!.Invoke(host, null);
    }

    /// <summary>Invokes the private reactive RoutedViewHost navigation disposal helper.</summary>
    /// <param name="host">The host instance.</param>
    private static void InvokeDisposeNavigationDisposables(RoutedViewHost host)
    {
        var method = typeof(RoutedViewHost)
            .GetMethod("DisposeNavigationDisposables", BindingFlags.Instance | BindingFlags.NonPublic);

        _ = method!.Invoke(host, null);
    }

    /// <summary>Returns whether the reactive view-model host has navigation disposables.</summary>
    /// <param name="host">The host instance.</param>
    /// <returns><see langword="true"/> when navigation disposables are present.</returns>
    private static bool HasNavigationDisposables(ViewModelViewHost host)
    {
        var field = typeof(ViewModelViewHost)
            .GetField(NavigationDisposablesFieldName, BindingFlags.Instance | BindingFlags.NonPublic);

        return field!.GetValue(host) is not null;
    }

    /// <summary>Returns whether the reactive routed host has navigation disposables.</summary>
    /// <param name="host">The host instance.</param>
    /// <returns><see langword="true"/> when navigation disposables are present.</returns>
    private static bool HasNavigationDisposables(RoutedViewHost host)
    {
        var field = typeof(RoutedViewHost)
            .GetField(NavigationDisposablesFieldName, BindingFlags.Instance | BindingFlags.NonPublic);

        return field!.GetValue(host) is not null;
    }

    /// <summary>Seeds the reactive view-model host navigation subscriptions.</summary>
    /// <param name="host">The host instance.</param>
    private static void SetNavigationDisposables(ViewModelViewHost host)
    {
        var field = typeof(ViewModelViewHost)
            .GetField(NavigationDisposablesFieldName, BindingFlags.Instance | BindingFlags.NonPublic);

        field!.SetValue(host, Activator.CreateInstance(field.FieldType));
    }

    /// <summary>Seeds the reactive routed host navigation subscriptions.</summary>
    /// <param name="host">The host instance.</param>
    private static void SetNavigationDisposables(RoutedViewHost host)
    {
        var field = typeof(RoutedViewHost)
            .GetField(NavigationDisposablesFieldName, BindingFlags.Instance | BindingFlags.NonPublic);

        field!.SetValue(host, Activator.CreateInstance(field.FieldType));
    }

    /// <summary>Invokes the private view factory used by assembly view registration.</summary>
    /// <param name="viewType">The view type to create.</param>
    /// <returns>The created view instance.</returns>
    private static object InvokePrivateCreateView(Type viewType)
    {
        var method = typeof(AppBuilderExtensions)
            .GetMethod("CreateView", BindingFlags.Static | BindingFlags.NonPublic);
        return InvokeReflectedMethod(method!, null, [viewType])!;
    }

    /// <summary>Invokes the private Activator-based view factory.</summary>
    /// <param name="viewType">The view type to create.</param>
    /// <returns>The created view instance.</returns>
    private static object InvokePrivateCreateViewWithActivator(Type viewType)
    {
        var method = typeof(AppBuilderExtensions)
            .GetMethod("CreateViewWithActivator", BindingFlags.Static | BindingFlags.NonPublic);
        return InvokeReflectedMethod(method!, null, [viewType])!;
    }

    /// <summary>Invokes the private resolver-failure fallback view factory.</summary>
    /// <param name="viewType">The view type to create.</param>
    /// <returns>The created view instance.</returns>
    private static object InvokePrivateCreateViewAfterResolutionFailure(Type viewType)
    {
        var method = typeof(AppBuilderExtensions)
            .GetMethod("CreateViewAfterResolutionFailure", BindingFlags.Static | BindingFlags.NonPublic);
        return InvokeReflectedMethod(method!, null, [viewType, new InvalidOperationException("expected")])!;
    }

    /// <summary>Invokes the private view-contract attribute helper.</summary>
    /// <param name="viewType">The view type to inspect.</param>
    /// <returns>The reflected contract value.</returns>
    private static string? InvokePrivateGetViewContract(Type viewType)
    {
        var method = typeof(AppBuilderExtensions)
            .GetMethod("GetViewContract", BindingFlags.Static | BindingFlags.NonPublic);
        return (string?)InvokeReflectedMethod(method!, null, [viewType]);
    }

    /// <summary>Invokes the private entry-assembly view registration helper.</summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="entryAssembly">The entry assembly.</param>
    /// <returns>The returned builder.</returns>
    private static AppBuilder InvokePrivateRegisterReactiveUIViewsFromEntryAssembly(AppBuilder builder, Assembly? entryAssembly)
    {
        var method = typeof(AppBuilderExtensions)
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .Single(candidate =>
                candidate.Name == "RegisterReactiveUIViewsFromEntryAssembly"
                && candidate.GetParameters() is [{ ParameterType: var builderType }, { ParameterType: var assemblyType }]
                && builderType == typeof(AppBuilder)
                && assemblyType == typeof(Assembly));

        return (AppBuilder)InvokeReflectedMethod(method, null, [builder, entryAssembly])!;
    }

    /// <summary>Invokes the private guarded view registration helper.</summary>
    /// <param name="resolver">The resolver instance.</param>
    /// <param name="assemblies">The assemblies to scan.</param>
    private static void InvokePrivateRegisterReactiveUIViews(IMutableDependencyResolver? resolver, Assembly[]? assemblies)
    {
        var method = typeof(AppBuilderExtensions)
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .Single(candidate =>
                candidate.Name == "RegisterReactiveUIViews"
                && candidate.GetParameters() is [{ ParameterType: var resolverType }, { ParameterType: var assembliesType }]
                && resolverType == typeof(IMutableDependencyResolver)
                && assembliesType == typeof(Assembly[]));

        _ = InvokeReflectedMethod(method, null, [resolver, assemblies]);
    }

    /// <summary>Invokes the private dependency-injection container helper.</summary>
    /// <typeparam name="TContainer">The container type.</typeparam>
    /// <param name="resolver">The mutable resolver.</param>
    /// <param name="containerFactory">The container factory.</param>
    /// <param name="containerConfig">The container configuration action.</param>
    /// <param name="dependencyResolverFactory">The dependency resolver factory.</param>
    private static void InvokePrivateConfigureReactiveUIDIContainer<TContainer>(
        IMutableDependencyResolver? resolver,
        Func<TContainer> containerFactory,
        Action<TContainer> containerConfig,
        Func<TContainer, IDependencyResolver> dependencyResolverFactory)
        where TContainer : class
    {
        var method = typeof(AppBuilderExtensions)
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .Single(candidate => candidate.Name == "ConfigureReactiveUIDIContainer" && candidate.IsGenericMethodDefinition);

        _ = InvokeReflectedMethod(
            method.MakeGenericMethod(typeof(TContainer)),
            null,
            [resolver, containerFactory, containerConfig, dependencyResolverFactory]);
    }

    /// <summary>Invokes a compiler-generated activation callback.</summary>
    /// <param name="viewType">The view base type that owns the callback.</param>
    private static void InvokeActivationCallback(Type viewType)
    {
        var closureType = viewType.GetNestedTypes(BindingFlags.NonPublic)
            .Single(type => type.Name.Contains("<>c", StringComparison.Ordinal));
        var instance = closureType.GetField("<>9", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
            ?.GetValue(null);
        var method = closureType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
            .Single(candidate =>
                candidate.GetParameters() is [{ ParameterType: var parameterType }]
                && parameterType == typeof(ActivationDisposables));

        using var disposables = new ActivationDisposables();
        _ = InvokeReflectedMethod(method, instance, [disposables]);
    }

    /// <summary>Returns whether the action throws exactly the requested exception type.</summary>
    /// <typeparam name="TException">The expected exception type.</typeparam>
    /// <param name="action">The action to invoke.</param>
    /// <returns><see langword="true"/> when the exact exception type is thrown; otherwise, <see langword="false"/>.</returns>
    private static bool ThrowsExactly<TException>(Action action)
        where TException : Exception
    {
        try
        {
            action();
        }
        catch (Exception error)
        {
            return UnwrapReflectionException(error).GetType() == typeof(TException);
        }

        return false;
    }

    /// <summary>Invokes a reflected method and rethrows inner target invocation exceptions.</summary>
    /// <param name="method">The reflected method.</param>
    /// <param name="instance">The instance for instance methods.</param>
    /// <param name="arguments">The method arguments.</param>
    /// <returns>The reflected method result.</returns>
    private static object? InvokeReflectedMethod(MethodInfo method, object? instance, object?[] arguments)
    {
        try
        {
            return method.Invoke(instance, arguments);
        }
        catch (TargetInvocationException error) when (error.InnerException is not null)
        {
            ExceptionDispatchInfo.Capture(error.InnerException).Throw();
            throw;
        }
    }

    /// <summary>Unwraps reflection invocation exceptions.</summary>
    /// <param name="error">The thrown exception.</param>
    /// <returns>The underlying exception when reflection wrapped it.</returns>
    private static Exception UnwrapReflectionException(Exception error) =>
        error is TargetInvocationException { InnerException: { } innerException } ? innerException : error;

    /// <summary>Creates an observed change for the Items property of an ItemsControl.</summary>
    /// <param name="items">The items control instance.</param>
    /// <returns>An observed change representing the Items property.</returns>
    private static ObservedChange<object, object> ItemsObservedChange(ItemsControl items)
    {
        var param = Expression.Parameter(typeof(ItemsControl), "x");
        var member = Expression.Property(param, nameof(ItemsControl.Items));
        return new(items, member, items.Items!);
    }

    /// <summary>Creates an observed change for the ItemsSource property of an ItemsControl.</summary>
    /// <param name="items">The items control instance.</param>
    /// <returns>An observed change representing the ItemsSource property.</returns>
    private static ObservedChange<object, object> ItemsSourceObservedChange(ItemsControl items)
    {
        var param = Expression.Parameter(typeof(ItemsControl), "x");
        var member = Expression.Property(param, nameof(ItemsControl.ItemsSource));
        return new(items, member, items.ItemsSource!);
    }

    /// <summary>Creates an observed change for the Tag property of a control.</summary>
    /// <param name="control">The control instance.</param>
    /// <returns>An observed change representing the Tag property.</returns>
    private static ObservedChange<object, object> TagObservedChange(Control control)
    {
        var param = Expression.Parameter(typeof(Control), "x");
        var member = Expression.Property(param, nameof(Control.Tag));
        return new(control, member, control.Tag!);
    }

    /// <summary>Creates an observed change for the Text property of a text block.</summary>
    /// <param name="text">The text block instance.</param>
    /// <returns>An observed change representing the Text property.</returns>
    private static ObservedChange<object, object> TextObservedChange(TextBlock text)
    {
        var param = Expression.Parameter(typeof(TextBlock), "x");
        var member = Expression.Property(param, nameof(TextBlock.Text));
        return new(text, member, text.Text!);
    }

    /// <summary>Creates a runtime-only lifetime implementation to exercise unsupported lifetime behavior.</summary>
    /// <returns>An unsupported application lifetime.</returns>
    private static IApplicationLifetime CreateUnsupportedLifetime()
    {
        var assemblyName = new AssemblyName("ReactiveUI.Avalonia.Tests.ReactiveDynamicLifetime");
        var assembly = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        var module = assembly.DefineDynamicModule("Main");
        var type = module.DefineType("ReactiveUnsupportedLifetime", TypeAttributes.NotPublic | TypeAttributes.Sealed);
        type.AddInterfaceImplementation(typeof(IApplicationLifetime));

        var lifetimeType = type.CreateType();
        return (IApplicationLifetime)Activator.CreateInstance(lifetimeType)!;
    }

    /// <summary>Sets the runtime design-mode flag whose public reference metadata exposes only a getter.</summary>
    /// <param name="isDesignMode">The design-mode value.</param>
    private static void SetDesignMode(bool isDesignMode)
    {
        var property = typeof(Design).GetProperty(
            nameof(Design.IsDesignMode),
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

        property?.SetMethod?.Invoke(null, [isDesignMode]);
    }

    /// <summary>Gets a real presentation source from a headless window.</summary>
    /// <returns>The presentation source.</returns>
    private static IPresentationSource GetPresentationSource()
    {
        IPresentationSource? source = null;
        var control = new Control();
        control.AttachedToVisualTree += (_, args) => source = args.PresentationSource;
        var window = new Window { Content = control };

        try
        {
            window.Show();
            return source!;
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>Executes an action and returns the expected invalid-operation exception.</summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>The captured exception, or null when the action did not throw.</returns>
    private static InvalidOperationException? CaptureInvalidOperation(Action action)
    {
        try
        {
            action();
            return null;
        }
        catch (InvalidOperationException exception)
        {
            return exception;
        }
    }

    /// <summary>Attribute to specify a view contract name.</summary>
    /// <param name="contract">The contract name.</param>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ViewContractAttribute(string contract) : Attribute
    {
        /// <summary>Gets the contract name.</summary>
        public string Contract { get; } = contract;
    }

    /// <summary>Container for a view-contract-shaped attribute that exposes no Contract property.</summary>
    public sealed class NoContractAttributeContainer
    {
        /// <summary>Gets an instance marker that keeps this shape non-static.</summary>
        public object? InstanceMarker => null;

        /// <summary>Attribute with the expected type name and no Contract property.</summary>
        [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
        public sealed class ViewContractAttribute : Attribute;
    }

    /// <summary>A recording observer used to avoid Subscribe extension overload ambiguity.</summary>
    /// <typeparam name="T">The observed value type.</typeparam>
    /// <param name="onNext">The observed-value action.</param>
    private sealed class RecordingObserver<T>(Action<T> onNext) : IObserver<T>
    {
        /// <summary>The action invoked for observed values.</summary>
        private readonly Action<T> _onNext = onNext;

        /// <inheritdoc/>
        public void OnCompleted()
        {
        }

        /// <inheritdoc/>
        public void OnError(Exception error) => throw error;

        /// <inheritdoc/>
        public void OnNext(T value) => _onNext(value);
    }

    /// <summary>A test control with styled properties.</summary>
    private sealed class TestControl : Control
    {
        /// <summary>The styled text property.</summary>
        private static readonly StyledProperty<string?> TextProperty =
            AvaloniaProperty.Register<TestControl, string?>(nameof(Text));

        /// <summary>Gets or sets the text value.</summary>
        public string? Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
    }

    /// <summary>A button that implements IActivatableView for testing activation.</summary>
    private sealed class ActivatableButton : Button, IActivatableView;

    /// <summary>An activatable view that is not an Avalonia visual.</summary>
    private sealed class ActivatableOnly : IActivatableView;

    /// <summary>A control that can host raw visual children.</summary>
    private sealed class VisualHost : Control
    {
        /// <summary>Adds a raw visual child.</summary>
        /// <param name="visual">The visual to add.</param>
        public void AddChild(Visual visual) =>
            VisualChildren.Add(visual);

        /// <summary>Removes a raw visual child.</summary>
        /// <param name="visual">The visual to remove.</param>
        public void RemoveChild(Visual visual) =>
            _ = VisualChildren.Remove(visual);
    }

    /// <summary>An activatable non-control visual.</summary>
    private sealed class ActivatableVisual : Visual, IActivatableView;

    /// <summary>A test command implementation for verifying command binding.</summary>
    private sealed class TestCommand : System.Windows.Input.ICommand
    {
        /// <summary>Whether the command can currently execute.</summary>
        private bool _canExecute = true;

        /// <inheritdoc/>
        public event EventHandler? CanExecuteChanged;

        /// <summary>Gets the last parameter passed to Execute.</summary>
        public object? LastParameter { get; private set; }

        /// <summary>Sets whether the command can execute and raises CanExecuteChanged.</summary>
        /// <param name="can">Whether the command can execute.</param>
        public void SetCanExecute(bool can)
        {
            _canExecute = can;
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc/>
        public bool CanExecute(object? parameter) => _canExecute;

        /// <inheritdoc/>
        public void Execute(object? parameter) => LastParameter = parameter;
    }

    /// <summary>A test view model.</summary>
    private sealed class ShimVm : ReactiveObject;

    /// <summary>A reactive shim user control for testing.</summary>
    private sealed class ReactiveControl : ReactiveUserControl<ShimVm>;

    /// <summary>A reactive shim window for testing.</summary>
    private sealed class ReactiveWindow : ReactiveWindow<ShimVm>;

    /// <summary>A reactive shim base user control for testing.</summary>
    private sealed class ReactiveBaseControl : ReactiveUserControlBase;

    /// <summary>A reactive shim base window for testing.</summary>
    private sealed class ReactiveBaseWindow : ReactiveWindowBase;

    /// <summary>A testable reactive ViewModelViewHost that exposes protected members.</summary>
    private sealed class TestableReactiveViewModelViewHost : ViewModelViewHost
    {
        /// <summary>Gets the protected style key override.</summary>
        public Type ExposedStyleKey => StyleKeyOverride;

        /// <summary>Raises the attached-to-visual-tree hook.</summary>
        /// <param name="source">The presentation source.</param>
        public void Attach(IPresentationSource source) =>
            OnAttachedToVisualTree(new(this, source));

        /// <summary>Raises the detached-from-visual-tree hook.</summary>
        /// <param name="source">The presentation source.</param>
        public void Detach(IPresentationSource source) =>
            OnDetachedFromVisualTree(new(this, source));
    }

    /// <summary>A testable reactive RoutedViewHost that exposes protected members.</summary>
    private sealed class TestableReactiveRoutedViewHost : RoutedViewHost
    {
        /// <summary>Gets the protected style key override.</summary>
        public Type ExposedStyleKey => StyleKeyOverride;

        /// <summary>Raises the attached-to-visual-tree hook.</summary>
        /// <param name="source">The presentation source.</param>
        public void Attach(IPresentationSource source) =>
            OnAttachedToVisualTree(new(this, source));

        /// <summary>Raises the detached-from-visual-tree hook.</summary>
        /// <param name="source">The presentation source.</param>
        public void Detach(IPresentationSource source) =>
            OnDetachedFromVisualTree(new(this, source));
    }

    /// <summary>A minimal view locator for host tests.</summary>
    /// <param name="view">The view to return.</param>
    /// <param name="contract">The optional contract to match.</param>
    private sealed class StaticViewLocator(IViewFor? view, string? contract = null) : IViewLocator
    {
        /// <summary>The view returned for matching contracts.</summary>
        private readonly IViewFor? _view = view;

        /// <summary>The contract that must match.</summary>
        private readonly string? _contract = contract;

        /// <inheritdoc/>
        public IViewFor<TViewModel>? ResolveView<TViewModel>()
            where TViewModel : class =>
            ResolveView<TViewModel>(contract: null);

        /// <inheritdoc/>
        public IViewFor<TViewModel>? ResolveView<TViewModel>(string? contract)
            where TViewModel : class =>
            IsMatch(contract) ? _view as IViewFor<TViewModel> : null;

        /// <inheritdoc/>
        public IViewFor? ResolveView(object? instance) =>
            ResolveView(instance, contract: null);

        /// <inheritdoc/>
        public IViewFor? ResolveView(object? instance, string? contract) =>
            IsMatch(contract) ? _view : null;

        /// <summary>Returns whether the requested contract matches this locator.</summary>
        /// <param name="contract">The requested contract.</param>
        /// <returns><see langword="true"/> when the contract matches; otherwise, <see langword="false"/>.</returns>
        private bool IsMatch(string? contract) =>
            string.Equals(_contract, contract, StringComparison.Ordinal);
    }

    /// <summary>A routable view model for testing.</summary>
    private sealed class VmA : ReactiveObject, ReactiveIRoutableViewModel
    {
        /// <summary>Initializes a new instance of the <see cref="VmA"/> class.</summary>
        /// <param name="screen">The host screen.</param>
        public VmA(ReactiveIScreen screen)
        {
            HostScreen = screen;
        }

        /// <summary>Gets the URL path segment.</summary>
        public string? UrlPathSegment => "A";

        /// <summary>Gets the host screen.</summary>
        public ReactiveIScreen HostScreen { get; }
    }

    /// <summary>A simple view model for testing non-routable navigation.</summary>
    private sealed class VmB : ReactiveObject;

    /// <summary>An unregistered view model for default locator fallbacks.</summary>
    private sealed class UnregisteredVm : ReactiveObject;

    /// <summary>A view for VmA.</summary>
    private sealed class ViewA : UserControl, IViewFor<VmA>
    {
        /// <summary>Gets or sets the view model.</summary>
        public VmA? ViewModel { get; set; }

        /// <inheritdoc/>
        object? IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (VmA?)value;
        }

        /// <inheritdoc/>
        public override string ToString() => nameof(ViewA);
    }

    /// <summary>A view for VmB.</summary>
    private sealed class ViewB : UserControl, IViewFor<VmB>
    {
        /// <summary>Gets or sets the view model.</summary>
        public VmB? ViewModel { get; set; }

        /// <inheritdoc/>
        object? IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (VmB?)value;
        }

        /// <inheritdoc/>
        public override string ToString() => nameof(ViewB);
    }

    /// <summary>A screen implementation for testing routing.</summary>
    private sealed class ScreenImpl : ReactiveObject, ReactiveIScreen
    {
        /// <summary>Gets the routing state.</summary>
        public ReactiveRoutingState Router { get; } = new();
    }

    /// <summary>A shim registration view model.</summary>
    private sealed class ShimRegistrationVm : ReactiveObject;

    /// <summary>A default shim registration view.</summary>
    private sealed class ShimRegistrationView : UserControl, IViewFor<ShimRegistrationVm>
    {
        /// <summary>Gets or sets the view model.</summary>
        public ShimRegistrationVm? ViewModel { get; set; }

        /// <inheritdoc/>
        object? IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (ShimRegistrationVm?)value;
        }
    }

    /// <summary>A shim registration view created through Activator fallback.</summary>
    private sealed class ActivatorCreatedShimRegistrationView : UserControl, IViewFor<ShimRegistrationVm>
    {
        /// <summary>Gets or sets the view model.</summary>
        public ShimRegistrationVm? ViewModel { get; set; }

        /// <inheritdoc/>
        object? IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (ShimRegistrationVm?)value;
        }
    }

    /// <summary>A shim registration view returned by the service locator.</summary>
    private sealed class LocatorCreatedShimRegistrationView : UserControl, IViewFor<ShimRegistrationVm>
    {
        /// <summary>Gets or sets the view model.</summary>
        public ShimRegistrationVm? ViewModel { get; set; }

        /// <inheritdoc/>
        object? IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (ShimRegistrationVm?)value;
        }
    }

    /// <summary>A contracted shim registration view.</summary>
    [ViewContract("shim")]
    private sealed class ContractedShimRegistrationView : UserControl, IViewFor<ShimRegistrationVm>
    {
        /// <summary>Gets or sets the view model.</summary>
        public ShimRegistrationVm? ViewModel { get; set; }

        /// <inheritdoc/>
        object? IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (ShimRegistrationVm?)value;
        }
    }

    /// <summary>A shim registration view with a matching attribute name but no Contract property.</summary>
    [NoContractAttributeContainer.ViewContract]
    private sealed class ShimRegistrationViewWithoutContractProperty : UserControl;

    /// <summary>A resolver that throws during concrete view resolution.</summary>
    private sealed class ThrowingResolver : IDependencyResolver
    {
        /// <inheritdoc/>
        public void Dispose() => GC.SuppressFinalize(this);

        /// <inheritdoc/>
        public object? GetService(Type? serviceType) =>
            throw new InvalidOperationException($"Cannot resolve {serviceType}.");

        /// <inheritdoc/>
        public object? GetService(Type? serviceType, string? contract) =>
            throw new InvalidOperationException($"Cannot resolve {serviceType} for {contract}.");

        /// <inheritdoc/>
        public T? GetService<T>()
        {
            _ = typeof(T);
            return default;
        }

        /// <inheritdoc/>
        public T? GetService<T>(string? contract) =>
            GetService<T>();

        /// <inheritdoc/>
        public IEnumerable<object> GetServices(Type? serviceType) =>
            GetServices<object>();

        /// <inheritdoc/>
        public IEnumerable<object> GetServices(Type? serviceType, string? contract) =>
            GetServices(serviceType);

        /// <inheritdoc/>
        public IEnumerable<T> GetServices<T>() =>
            [];

        /// <inheritdoc/>
        public IEnumerable<T> GetServices<T>(string? contract) =>
            GetServices<T>();

        /// <inheritdoc/>
        public bool HasRegistration(Type? serviceType) =>
            false;

        /// <inheritdoc/>
        public bool HasRegistration(Type? serviceType, string? contract) =>
            false;

        /// <inheritdoc/>
        public bool HasRegistration<T>() =>
            HasRegistration(typeof(T));

        /// <inheritdoc/>
        public bool HasRegistration<T>(string? contract) =>
            HasRegistration(typeof(T), contract);

        /// <inheritdoc/>
        public void Register(Func<object?> factory, Type? serviceType) =>
            _ = factory;

        /// <inheritdoc/>
        public void Register(Func<object?> factory, Type? serviceType, string? contract) =>
            Register(factory, serviceType);

        /// <inheritdoc/>
        public void Register<T>(Func<T?> factory) =>
            Register(() => factory(), typeof(T));

        /// <inheritdoc/>
        public void Register<T>(Func<T?> factory, string? contract) =>
            Register(() => factory(), typeof(T), contract);

        /// <inheritdoc/>
        public void Register<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService, new() =>
            Register<TImplementation>(() => new());

        /// <inheritdoc/>
        public void Register<TService, TImplementation>(string? contract)
            where TService : class
            where TImplementation : class, TService, new() =>
            Register<TImplementation>(() => new(), contract);

        /// <inheritdoc/>
        public void UnregisterCurrent(Type? serviceType) =>
            _ = serviceType;

        /// <inheritdoc/>
        public void UnregisterCurrent(Type? serviceType, string? contract) =>
            UnregisterCurrent(serviceType);

        /// <inheritdoc/>
        public void UnregisterCurrent<T>() =>
            UnregisterCurrent(typeof(T));

        /// <inheritdoc/>
        public void UnregisterCurrent<T>(string? contract) =>
            UnregisterCurrent(typeof(T), contract);

        /// <inheritdoc/>
        public void UnregisterAll(Type? serviceType)
        {
            if (serviceType is null)
            {
                return;
            }

            UnregisterCurrent(serviceType);
        }

        /// <inheritdoc/>
        public void UnregisterAll(Type? serviceType, string? contract) =>
            UnregisterCurrent(serviceType, contract);

        /// <inheritdoc/>
        public void UnregisterAll<T>() =>
            UnregisterCurrent<T>();

        /// <inheritdoc/>
        public void UnregisterAll<T>(string? contract) =>
            UnregisterCurrent<T>(contract);

        /// <inheritdoc/>
        public IDisposable ServiceRegistrationCallback(Type serviceType, Action<IDisposable> callback) =>
            EmptyDisposable.Instance;

        /// <inheritdoc/>
        public IDisposable ServiceRegistrationCallback(Type serviceType, string? contract, Action<IDisposable> callback) =>
            EmptyDisposable.Instance;

        /// <inheritdoc/>
        public IDisposable ServiceRegistrationCallback<T>(Action<IDisposable> callback) =>
            ServiceRegistrationCallback(typeof(T), callback);

        /// <inheritdoc/>
        public IDisposable ServiceRegistrationCallback<T>(string? contract, Action<IDisposable> callback) =>
            ServiceRegistrationCallback(typeof(T), contract, callback);

        /// <inheritdoc/>
        public void RegisterConstant<T>(T? value)
            where T : class =>
            Register(() => value, typeof(T));

        /// <inheritdoc/>
        public void RegisterConstant<T>(T? value, string? contract)
            where T : class =>
            Register(() => value, typeof(T), contract);

        /// <inheritdoc/>
        public void RegisterLazySingleton<T>(Func<T?> valueFactory)
            where T : class =>
            Register(() => valueFactory(), typeof(T));

        /// <inheritdoc/>
        public void RegisterLazySingleton<T>(Func<T?> valueFactory, string? contract)
            where T : class =>
            Register(() => valueFactory(), typeof(T), contract);
    }
}
