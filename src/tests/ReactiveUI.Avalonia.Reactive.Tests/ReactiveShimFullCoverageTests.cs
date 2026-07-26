// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Linq.Expressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Rendering;
using Splat;

using ReactiveRxAppBuilder = global::ReactiveUI.Reactive.Builder.RxAppBuilder;
using ReactiveRxSchedulers = global::ReactiveUI.Reactive.RxSchedulers;
using ReactiveRxSuspension = global::ReactiveUI.Reactive.RxSuspension;
using ReactiveUIBuilder = global::ReactiveUI.Reactive.Builder.ReactiveUIBuilder;
using RxUnit = global::System.Reactive.Unit;

namespace ReactiveUI.Avalonia.Reactive.Tests;

/// <summary>Behavioral coverage tests for ReactiveUI.Avalonia.Reactive linked-source types.</summary>
public partial class ReactiveShimFullCoverageTests
{
    /// <summary>The expected exception message used by error-path tests.</summary>
    private const string ExpectedErrorMessage = "expected";

    /// <summary>The default content used by view-host tests.</summary>
    private const string DefaultContentValue = "default";

    /// <summary>The matching view contract used by view-host tests.</summary>
    private const string ViewContractValue = "contract";

    /// <summary>The event parameter used by command-binding tests.</summary>
    private const string EventParameter = "event";

    /// <summary>The command event name used by command-binding tests.</summary>
    private const string ClickEventName = "Click";

    /// <summary>The missing property name used by notification tests.</summary>
    private const string MissingPropertyName = "Missing";

    /// <summary>The affinity assigned to generic input elements with event targets.</summary>
    private const int InputElementEventAffinity = 6;

    /// <summary>The affinity assigned to button command bindings.</summary>
    private const int ButtonCommandBindingAffinity = 10;

    /// <summary>The affinity assigned to Avalonia styled properties.</summary>
    private const int StyledPropertyAffinity = 4;

    /// <summary>Verifies reactive AppBuilder null guards and callback setup paths.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveAppBuilderExtensions_UseReactiveUI_CoversNullsAndCallback()
    {
        AppBuilder? builder = null;
        await Assert.That(() => AppBuilderExtensions.UseReactiveUI(builder!, static _ => { }))
            .ThrowsExactly<ArgumentNullException>();

        await Assert.That(static () => AppBuilderExtensions.UseReactiveUI(
            AppBuilder.Configure<Application>(),
            null!)).ThrowsExactly<ArgumentNullException>();

        var configured = false;
        builder = AppBuilder.Configure<Application>();

        var result = AppBuilderExtensions.UseReactiveUI(
            builder,
            _ => configured = true);
        AppBuilderExtensions.ConfigureReactiveUI(_ => configured = true);

        await Assert.That(result).IsSameReferenceAs(builder);
        await Assert.That(configured).IsTrue();
    }

    /// <summary>Verifies reactive AppBuilder view registration paths.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveAppBuilderExtensions_RegisterViews_CoversRegistrationPaths()
    {
        AppBuilder? builder = null;
        await Assert.That(() => AppBuilderExtensions.RegisterReactiveUIViews(builder!))
            .ThrowsExactly<ArgumentNullException>();

        _ = AppBuilderExtensions.RegisterReactiveUIViews(
            AppBuilder.Configure<Application>(),
            typeof(ShimRegistrationVm).Assembly,
            typeof(ShimRegistrationVm).Assembly);
        AppBuilderExtensions.RegisterReactiveUIViews(
            AppLocator.CurrentMutable,
            [typeof(ShimRegistrationVm).Assembly, typeof(ShimRegistrationVm).Assembly]);

        var serviceType = typeof(IViewFor<>).MakeGenericType(typeof(ShimRegistrationVm));
        _ = new ShimRegistrationVm();
        _ = new ShimRegistrationView();
        _ = new ContractedShimRegistrationView();
        var resolved = AppLocator.Current.GetService(serviceType);
        var contracted = AppLocator.Current.GetService(serviceType, "shim");

        await Assert.That(resolved).IsNotNull();
        await Assert.That(contracted).IsTypeOf<ContractedShimRegistrationView>();

        _ = new ActivatorCreatedShimRegistrationView();
        var fallbackView = InvokePrivateCreateView(typeof(ActivatorCreatedShimRegistrationView));
        await Assert.That(fallbackView).IsTypeOf<ActivatorCreatedShimRegistrationView>();

        var originalLocator = AppLocator.GetLocator();
        try
        {
            AppLocator.SetLocator(new ThrowingResolver());
            var fallbackAfterResolverFailure = InvokePrivateCreateView(typeof(ActivatorCreatedShimRegistrationView));
            await Assert.That(fallbackAfterResolverFailure).IsTypeOf<ActivatorCreatedShimRegistrationView>();
        }
        finally
        {
            AppLocator.SetLocator(originalLocator);
        }

        var markerBuilder = AppBuilder.Configure<Application>();
        var markerResult = AppBuilderExtensions.RegisterReactiveUIViewsFromAssemblyOf<ShimRegistrationVm>(markerBuilder);
        await Assert.That(markerResult).IsSameReferenceAs(markerBuilder);

        var entryBuilder = AppBuilder.Configure<Application>();
        var entryResult = AppBuilderExtensions.RegisterReactiveUIViewsFromEntryAssembly(entryBuilder);
        await Assert.That(entryResult).IsSameReferenceAs(entryBuilder);
    }

    /// <summary>Verifies reactive AppBuilder wrapper and private helper paths before the first await.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveAppBuilderExtensions_CoversWrapperAndPrivateHelperPathsSynchronously()
    {
        var markerBuilder = AppBuilder.Configure<Application>();
        var markerResult = AppBuilderExtensions.RegisterReactiveUIViewsFromAssemblyOf<ShimRegistrationVm>(markerBuilder);
        var entryBuilder = AppBuilder.Configure<Application>();
        var entryResult = AppBuilderExtensions.RegisterReactiveUIViewsFromEntryAssembly(entryBuilder);

        var privateNullEntryBuilder = AppBuilder.Configure<Application>();
        var privateNullEntryResult = InvokePrivateRegisterReactiveUIViewsFromEntryAssembly(privateNullEntryBuilder, null);
        _ = InvokePrivateRegisterReactiveUIViewsFromEntryAssembly(
            AppBuilder.Configure<Application>(),
            typeof(ShimRegistrationVm).Assembly);
        AppBuilderExtensions.RegisterReactiveUIViews(
            AppLocator.CurrentMutable,
            [typeof(ShimRegistrationVm).Assembly]);
        GC.KeepAlive(typeof(NoContractAttributeContainer));
        var missingContractView = new ShimRegistrationViewWithoutContractProperty();
        var missingContract = InvokePrivateGetViewContract(missingContractView.GetType());

        await Assert.That(ReferenceEquals(markerResult, markerBuilder)).IsTrue();
        await Assert.That(ReferenceEquals(entryResult, entryBuilder)).IsTrue();
        await Assert.That(ReferenceEquals(privateNullEntryResult, privateNullEntryBuilder)).IsTrue();
        await Assert.That(missingContract).IsNull();
    }

    /// <summary>Verifies the private registration and resolver helper paths.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveAppBuilderExtensions_CoversPrivateRegistrationHelperPaths()
    {
        var mutableResolver = AppLocator.CurrentMutable!;
        var nullResolverFactoryCalled = false;
        InvokePrivateConfigureReactiveUIDIContainer(
            null,
            () =>
            {
                nullResolverFactoryCalled = true;
                return new object();
            },
            static _ => { },
            _ => (IDependencyResolver)mutableResolver);

        InvokePrivateRegisterReactiveUIViews(null, [typeof(ShimRegistrationVm).Assembly]);
        InvokePrivateRegisterReactiveUIViews(mutableResolver, null);
        InvokePrivateRegisterReactiveUIViews(mutableResolver, []);
        InvokePrivateRegisterReactiveUIViews(mutableResolver, [typeof(ShimRegistrationVm).Assembly]);

        var container = new object();
        var containerConfigured = false;
        InvokePrivateConfigureReactiveUIDIContainer(
            mutableResolver,
            () => container,
            value => containerConfigured = ReferenceEquals(value, container),
            value => ReferenceEquals(value, container) ? (IDependencyResolver)mutableResolver : throw new InvalidOperationException());

        var resolvedView = new LocatorCreatedShimRegistrationView();
        mutableResolver.RegisterConstant(resolvedView);
        var originalLocator = AppLocator.GetLocator();
        var locatorResolved = false;
        var fallbackAfterThrow = false;
        try
        {
            locatorResolved = ReferenceEquals(InvokePrivateCreateView(typeof(LocatorCreatedShimRegistrationView)), resolvedView);

            AppLocator.SetLocator(new ThrowingResolver());
            fallbackAfterThrow = InvokePrivateCreateView(typeof(ActivatorCreatedShimRegistrationView)) is ActivatorCreatedShimRegistrationView;
        }
        finally
        {
            AppLocator.SetLocator(originalLocator);
        }

        var invalidCreateThrows = ThrowsExactly<InvalidOperationException>(static () => InvokePrivateCreateView(typeof(int?)));
        var nullBuilderThrows = ThrowsExactly<ArgumentNullException>(static () => InvokePrivateRegisterReactiveUIViewsFromEntryAssembly(null!, null));

        await Assert.That(IsPrivateHelperSetupValid(
            nullResolverFactoryCalled,
            containerConfigured,
            locatorResolved,
            fallbackAfterThrow,
            invalidCreateThrows,
            nullBuilderThrows)).IsTrue();
    }

    /// <summary>Verifies the reactive AppBuilder Activator helper paths.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveAppBuilderExtensions_CoversActivatorHelperPathsSynchronously()
    {
        await Assert.That(InvokePrivateCreateViewAfterResolutionFailure(typeof(ActivatorCreatedShimRegistrationView)))
            .IsTypeOf<ActivatorCreatedShimRegistrationView>();
        await Assert.That(InvokePrivateCreateViewWithActivator(typeof(ActivatorCreatedShimRegistrationView)))
            .IsTypeOf<ActivatorCreatedShimRegistrationView>();
        await Assert.That(static () => InvokePrivateCreateViewWithActivator(typeof(int?)))
            .ThrowsExactly<InvalidOperationException>();
    }

    /// <summary>Verifies reactive AppBuilder DI container setup paths.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveAppBuilderExtensions_DIContainer_CoversCallback()
    {
        ReactiveUIBuilder.ResetBuilderStateForTests();
        var container = new object();
        var configured = false;
        var reactiveConfigured = false;
        var resolver = (IDependencyResolver)AppLocator.CurrentMutable!;

        _ = AppBuilderExtensions.UseReactiveUIWithDIContainer(
            AppBuilder.Configure<Application>(),
            () => container,
            value => configured = ReferenceEquals(value, container),
            value => ReferenceEquals(value, container) ? resolver : throw new InvalidOperationException(),
            _ => reactiveConfigured = true);
        AppBuilderExtensions.ConfigureReactiveUI(_ => reactiveConfigured = true);
        AppBuilderExtensions.ConfigureReactiveUIDIContainer(
            AppLocator.CurrentMutable,
            () => container,
            value => configured = ReferenceEquals(value, container),
            value => ReferenceEquals(value, container) ? resolver : throw new InvalidOperationException());

        await Assert.That(configured).IsTrue();
        await Assert.That(reactiveConfigured).IsTrue();
        await Assert.That(ReactiveRxSchedulers.MainThreadScheduler).IsSameReferenceAs(AvaloniaScheduler.Instance);

        await Assert.That(() => AppBuilderExtensions.UseReactiveUIWithDIContainer(
            null!,
            () => container,
            static _ => { },
            _ => resolver,
            static _ => { })).ThrowsExactly<ArgumentNullException>();

        _ = AppBuilderExtensions.UseReactiveUIWithDIContainer<object>(
            AppBuilder.Configure<Application>(),
            null!,
            static _ => { },
            _ => resolver,
            static _ => { });
        await Assert.That(() => AppBuilderExtensions.ConfigureReactiveUIDIContainer<object>(
            AppLocator.CurrentMutable,
            null!,
            static _ => { },
            _ => resolver)).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies reactive WithAvalonia registrations and null guard.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveAppBuilderExtensions_WithAvalonia_CoversRegistrations()
    {
        ReactiveUIBuilder? builder = null;
        await Assert.That(() => AppBuilderExtensions.WithAvalonia(builder!))
            .ThrowsExactly<ArgumentNullException>();

        builder = ReactiveRxAppBuilder.CreateReactiveUIBuilder();
        var result = AppBuilderExtensions.WithAvalonia(builder);

        await Assert.That(result).IsSameReferenceAs(builder);
    }

    /// <summary>Verifies reactive auto data-template binding behavior.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveAutoDataTemplateBindingHook_CoversBranches()
    {
        var hook = new AutoDataTemplateBindingHook();
        var items = new ListBox();

        await Assert.That(() => hook.ExecuteHook(null, items, static () => [], null!, BindingDirection.OneWay))
            .ThrowsExactly<ArgumentNullException>();

        await Assert.That(hook.ExecuteHook(null, items, static () => [], static () => [], BindingDirection.OneWay)).IsTrue();
        await Assert.That(items.ItemTemplate).IsNull();

        await Assert.That(hook.ExecuteHook(null, new TextBlock(), static () => [], static () => [TextObservedChange(new())], BindingDirection.OneWay)).IsTrue();
        await Assert.That(hook.ExecuteHook(null, items, static () => [], () => [TagObservedChange(items)], BindingDirection.OneWay)).IsTrue();

        _ = hook.ExecuteHook(null, items, static () => [], () => [ItemsObservedChange(items)], BindingDirection.OneWay);
        await Assert.That(items.ItemTemplate).IsNotNull();

        var control = items.ItemTemplate!.Build(new());
        await Assert.That(control).IsTypeOf<ViewModelViewHost>();
        await Assert.That(((ViewModelViewHost)control!).HorizontalContentAlignment).IsEqualTo(HorizontalAlignment.Stretch);

        var templated = new ListBox { ItemTemplate = new FuncDataTemplate<object>(static (_, _) => new TextBlock(), true) };
        _ = hook.ExecuteHook(null, templated, static () => [], () => [ItemsSourceObservedChange(templated)], BindingDirection.OneWay);
        await Assert.That(templated.ItemTemplate).IsNotNull();

        var dataTemplated = new ListBox();
        dataTemplated.DataTemplates.Add(new FuncDataTemplate<object>(static (_, _) => new TextBlock(), true));
        _ = hook.ExecuteHook(null, dataTemplated, static () => [], () => [ItemsObservedChange(dataTemplated)], BindingDirection.OneWay);
        await Assert.That(dataTemplated.ItemTemplate).IsNull();
    }

    /// <summary>Verifies reactive AutoSuspendHelper lifetime paths.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveAutoSuspendHelper_CoversLifetimePaths()
    {
        await Assert.That(static () => new AutoSuspendHelper(null!)).ThrowsExactly<ArgumentNullException>();
        await Assert.That(static () => new AutoSuspendHelper(CreateUnsupportedLifetime())).ThrowsExactly<NotSupportedException>();

        var lifetime = new ClassicDesktopStyleApplicationLifetime();
        using var helper = new AutoSuspendHelper(lifetime);
        var persisted = false;
        using var persistSubscription = ReactiveRxSuspension.SuspensionHost.ShouldPersistState.Subscribe(
            new RecordingObserver<IDisposable>(value =>
            {
                persisted = true;
                value.Dispose();
            }));

        lifetime.Shutdown();
        await Assert.That(persisted).IsTrue();

        var launches = 0;
        using var launchSubscription = ReactiveRxSuspension.SuspensionHost.IsLaunchingNew.Subscribe(
            new RecordingObserver<RxUnit>(_ => launches++));
        helper.OnFrameworkInitializationCompleted();
        await Assert.That(launches).IsEqualTo(1);

        var invalidations = 0;
        using var invalidationSubscription = ReactiveRxSuspension.SuspensionHost.ShouldInvalidateState.Subscribe(
            new RecordingObserver<RxUnit>(_ => invalidations++));
        helper.OnUnhandledException(this, new(new InvalidOperationException(ExpectedErrorMessage), isTerminating: false));
        await Assert.That(invalidations).IsEqualTo(1);
    }

    /// <summary>Verifies reactive activation fetcher behavior.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveAvaloniaActivationForViewFetcher_CoversActivationPaths()
    {
        var sut = new AvaloniaActivationForViewFetcher();

        await Assert.That(sut.GetAffinityForView(typeof(Button))).IsGreaterThan(0);
        await Assert.That(sut.GetAffinityForView(typeof(object))).IsEqualTo(0);

        bool? nonVisual = null;
        using (sut.GetActivationForView(new ActivatableOnly()).Subscribe(new RecordingObserver<bool>(value => nonVisual = value)))
        {
            await Assert.That(nonVisual).IsFalse();
        }

        var button = new ActivatableButton();
        bool? loaded = null;
        using (sut.GetActivationForView(button).Subscribe(new RecordingObserver<bool>(value => loaded = value)))
        {
            button.RaiseEvent(new(Button.LoadedEvent));
            await Assert.That(loaded).IsTrue();

            button.RaiseEvent(new(Button.UnloadedEvent));
            await Assert.That(loaded).IsFalse();
        }

        var host = new VisualHost();
        var visual = new ActivatableVisual();
        var window = new Window { Content = host };
        bool? attached = null;
        using var visualSubscription = sut.GetActivationForView(visual).Subscribe(new RecordingObserver<bool>(value => attached = value));
        try
        {
            host.AddChild(visual);
            window.Show();
            await Assert.That(attached).IsTrue();

            host.RemoveChild(visual);
            await Assert.That(attached).IsFalse();
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>Verifies reactive command binding behavior.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveAvaloniaCreatesCommandBinding_CoversCommandPaths()
    {
        var sut = new AvaloniaCreatesCommandBinding();
        var command = new TestCommand();
        var parameter = new Signal<object?>();
        var button = new Button();

        await Assert.That(sut.GetAffinityForObject<object>(hasEventTarget: false)).IsEqualTo(0);
        await Assert.That(sut.GetAffinityForObject<InputElement>(hasEventTarget: false)).IsEqualTo(0);
        await Assert.That(sut.GetAffinityForObject<InputElement>(hasEventTarget: true)).IsEqualTo(InputElementEventAffinity);
        await Assert.That(sut.GetAffinityForObject<Button>(hasEventTarget: false)).IsEqualTo(ButtonCommandBindingAffinity);

        using (var binding = sut.BindCommandToObject(command, button, parameter))
        {
            parameter.OnNext("button");
            await Assert.That(button.CommandParameter).IsEqualTo("button");
            await Assert.That(button.Command).IsSameReferenceAs(command);
        }

        await Assert.That(button.Command).IsNull();
        await Assert.That(sut.BindCommandToObject(null, button, parameter)).IsNull();
        await Assert.That(sut.BindCommandToObject<Button>(command, null, parameter)).IsNull();
        await Assert.That(CaptureInvalidOperation(() => sut.BindCommandToObject<object>(command, new(), parameter))).IsNotNull();
        await Assert.That(CaptureInvalidOperation(() => sut.BindCommandToObject(command, new TextBox(), parameter))).IsNotNull();

        using (var eventBinding = sut.BindCommandToObject<Button, RoutedEventArgs>(command, button, parameter, nameof(InputElement.GotFocus)))
        {
            parameter.OnNext(EventParameter);
            button.RaiseEvent(new(InputElement.GotFocusEvent));
            await Assert.That(command.LastParameter).IsEqualTo(EventParameter);

            command.SetCanExecute(false);
            parameter.OnNext("blocked");
            button.RaiseEvent(new(InputElement.GotFocusEvent));
            await Assert.That(command.LastParameter).IsEqualTo(EventParameter);
        }

        await Assert.That(button.IsSet(InputElement.IsEnabledProperty)).IsFalse();
        await Assert.That(sut.BindCommandToObject<object, RoutedEventArgs>(null, new(), parameter, ClickEventName)).IsNull();
        await Assert.That(sut.BindCommandToObject<object, RoutedEventArgs>(command, null, parameter, ClickEventName)).IsNull();
        await Assert.That(CaptureInvalidOperation(() => sut.BindCommandToObject<object, RoutedEventArgs>(command, new(), parameter, ClickEventName))).IsNotNull();
        await Assert.That(CaptureInvalidOperation(() => sut.BindCommandToObject<Button, RoutedEventArgs>(command, button, parameter, "MissingEvent"))).IsNotNull();

        using var addRemove = sut.BindCommandToObject<Button, EventArgs>(
            command,
            button,
            parameter,
            static _ => { },
            static _ => { });
        await Assert.That(addRemove).IsNotNull();
    }

    /// <summary>Verifies reactive property notification behavior.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveAvaloniaObjectObservableForProperty_CoversNotificationPaths()
    {
        var sut = new AvaloniaObjectObservableForProperty();
        var control = new TestControl();
        Expression<Func<string?>> expression = () => control.Text;

        await Assert.That(sut.GetAffinityForObject(typeof(TestControl), nameof(TestControl.Text))).IsEqualTo(StyledPropertyAffinity);
        await Assert.That(sut.GetAffinityForObject((Type?)null, nameof(TestControl.Text), beforeChanged: false)).IsEqualTo(0);
        await Assert.That(sut.GetAffinityForObject(typeof(object), "Text")).IsEqualTo(0);
        await Assert.That(sut.GetAffinityForObject(typeof(TestControl), MissingPropertyName)).IsEqualTo(0);

        IObservedChange<object?, object?>? observed = null;
        using (sut.GetNotificationForProperty(control, expression, nameof(TestControl.Text))
            .Subscribe(new RecordingObserver<IObservedChange<object?, object?>>(value => observed = value)))
        {
            control.Text = "reactive";
            await Assert.That(observed).IsNotNull();
            await Assert.That(observed!.Value).IsEqualTo("reactive");
        }

        await Assert.That(() => sut.GetNotificationForProperty(new(), expression, "Text"))
            .ThrowsExactly<InvalidOperationException>();
        await Assert.That(() => sut.GetNotificationForProperty(control, expression, MissingPropertyName, beforeChanged: false, suppressWarnings: false))
            .ThrowsExactly<MissingMemberException>();
        await Assert.That(() => sut.GetNotificationForProperty(control, expression, MissingPropertyName, beforeChanged: false, suppressWarnings: true))
            .ThrowsExactly<MissingMemberException>();
        await Assert.That(() => sut.GetNotificationForProperty(null!, expression, "Text"))
            .ThrowsExactly<ArgumentNullException>();

        IObservedChange<object?, object?>? observedFromOverload = null;
        using (sut.GetNotificationForProperty(control, expression, nameof(TestControl.Text), beforeChanged: false)
            .Subscribe(new RecordingObserver<IObservedChange<object?, object?>>(value => observedFromOverload = value)))
        {
            control.Text = "overload";
            await Assert.That(observedFromOverload).IsNotNull();
            await Assert.That(observedFromOverload!.Value).IsEqualTo("overload");
        }
    }

    /// <summary>Verifies reactive control and window ViewModel synchronization.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveControls_CoverViewModelSynchronization()
    {
        var control = new ReactiveControl();
        var vm = new ShimVm();
        control.DataContext = vm;
        await Assert.That(control.ViewModel).IsSameReferenceAs(vm);
        await Assert.That(((IViewFor<ShimVm>)control).ViewModel).IsSameReferenceAs(vm);

        control.DataContext = new();
        await Assert.That(control.ViewModel).IsSameReferenceAs(vm);

        var secondVm = new ShimVm();
        control.ViewModel = secondVm;
        await Assert.That(control.DataContext).IsSameReferenceAs(secondVm);

        ((IViewFor)control).ViewModel = null;
        await Assert.That(control.ViewModel).IsNull();
        await Assert.That(control.DataContext).IsNull();

        var directControl = new ReactiveUserControl<ShimVm> { DataContext = vm };
        await Assert.That(directControl.ViewModel).IsSameReferenceAs(vm);
        ((IViewFor)directControl).ViewModel = secondVm;
        await Assert.That(directControl.ViewModel).IsSameReferenceAs(secondVm);

        var window = new ReactiveWindow { DataContext = vm };
        await Assert.That(window.ViewModel).IsSameReferenceAs(vm);
        await Assert.That(((IViewFor<ShimVm>)window).ViewModel).IsSameReferenceAs(vm);

        window.DataContext = new();
        await Assert.That(window.ViewModel).IsSameReferenceAs(vm);

        window.ViewModel = secondVm;
        await Assert.That(window.DataContext).IsSameReferenceAs(secondVm);

        ((IViewFor)window).ViewModel = null;
        await Assert.That(window.ViewModel).IsNull();
        await Assert.That(window.DataContext).IsNull();

        var directWindow = new ReactiveWindow<ShimVm> { DataContext = vm };
        await Assert.That(directWindow.ViewModel).IsSameReferenceAs(vm);
        ((IViewFor)directWindow).ViewModel = secondVm;
        await Assert.That(directWindow.ViewModel).IsSameReferenceAs(secondVm);

        var baseControl = new ReactiveBaseControl();
        var arbitrary = new object();
        baseControl.DataContext = arbitrary;
        await Assert.That(baseControl.ViewModel).IsSameReferenceAs(arbitrary);

        var baseWindow = new ReactiveBaseWindow { DataContext = arbitrary };
        await Assert.That(baseWindow.ViewModel).IsSameReferenceAs(arbitrary);

        var activationWindow = new Window { Content = control };
        try
        {
            activationWindow.Show();
        }
        finally
        {
            activationWindow.Close();
        }

        try
        {
            directWindow.Show();
        }
        finally
        {
            directWindow.Close();
        }
    }

    /// <summary>Verifies reactive view-host navigation behavior.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveViewModelViewHost_CoversNavigationPaths()
    {
        var view = new ViewB();
        var host = new TestableReactiveViewModelViewHost { DefaultContent = DefaultContentValue, ViewContract = ViewContractValue, ViewLocator = new StaticViewLocator(view, ViewContractValue) };
        var vm = new VmB();

        host.ViewModel = vm;
        await Assert.That(host.ViewModel).IsSameReferenceAs(vm);
        await Assert.That(host.ViewContract).IsEqualTo(ViewContractValue);
        await Assert.That(host.DefaultContent).IsEqualTo(DefaultContentValue);
        await Assert.That(host.ExposedStyleKey).IsEqualTo(typeof(TransitioningContentControl));

        host.NavigateToViewModel(vm, ViewContractValue);
        await Assert.That(host.Content).IsSameReferenceAs(view);
        await Assert.That(view.ViewModel).IsSameReferenceAs(vm);
        await Assert.That(view.DataContext).IsSameReferenceAs(vm);

        host.NavigateToViewModel(null, null);
        await Assert.That(host.Content).IsEqualTo(DefaultContentValue);

        host.ViewLocator = new StaticViewLocator(null);
        host.NavigateToViewModel(vm, null);
        await Assert.That(host.Content).IsEqualTo(DefaultContentValue);

        host.ViewLocator = new StaticViewLocator(null, "other");
        host.NavigateToViewModel(vm, ViewContractValue);
        await Assert.That(host.Content).IsEqualTo(DefaultContentValue);

        host.ViewLocator = new StaticViewLocator(new ViewB());
        host.NavigateToViewModel(new VmB(), null);
        await Assert.That(host.Content).IsTypeOf<ViewB>();

        host.ViewLocator = null;
        host.NavigateToViewModel(new UnregisteredVm(), null);
        await Assert.That(host.Content).IsEqualTo(DefaultContentValue);

        host.DisposeNavigationDisposables();

        var source = GetPresentationSource();
        host.Attach(source);
        host.Attach(source);
        host.Detach(source);
    }

    /// <summary>Verifies reactive routed view-host navigation behavior.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveRoutedViewHost_CoversNavigationPaths()
    {
        var screen = new ScreenImpl();
        var view = new ViewA();
        var host = new TestableReactiveRoutedViewHost
        {
            DefaultContent = DefaultContentValue,
            Router = screen.Router,
            ViewContract = ViewContractValue,
            ViewLocator = new StaticViewLocator(view, ViewContractValue)
        };
        var vm = new VmA(screen);

        await Assert.That(host.Router).IsSameReferenceAs(screen.Router);
        await Assert.That(host.ViewContract).IsEqualTo(ViewContractValue);
        await Assert.That(host.DefaultContent).IsEqualTo(DefaultContentValue);
        await Assert.That(host.ExposedStyleKey).IsEqualTo(typeof(TransitioningContentControl));

        host.NavigateToViewModel(vm, ViewContractValue);
        await Assert.That(host.Content).IsSameReferenceAs(view);
        await Assert.That(view.ViewModel).IsSameReferenceAs(vm);
        await Assert.That(view.DataContext).IsSameReferenceAs(vm);

        host.Router = null;
        host.NavigateToViewModel(vm, null);
        await Assert.That(host.Content).IsEqualTo(DefaultContentValue);

        host.Router = screen.Router;
        host.NavigateToViewModel(null, null);
        await Assert.That(host.Content).IsEqualTo(DefaultContentValue);

        host.ViewLocator = new StaticViewLocator(null);
        host.NavigateToViewModel(vm, null);
        await Assert.That(host.Content).IsEqualTo(DefaultContentValue);

        host.ViewLocator = new StaticViewLocator(null, "other");
        host.NavigateToViewModel(vm, ViewContractValue);
        await Assert.That(host.Content).IsEqualTo(DefaultContentValue);

        host.ViewLocator = new StaticViewLocator(new ViewA());
        host.NavigateToViewModel(new VmA(screen), null);
        await Assert.That(host.Content).IsTypeOf<ViewA>();

        host.ViewLocator = null;
        host.NavigateToViewModel(new UnregisteredVm(), null);
        await Assert.That(host.Content).IsEqualTo(DefaultContentValue);

        host.DisposeNavigationDisposables();

        var source = GetPresentationSource();
        host.Attach(source);
        host.Attach(source);
        host.Detach(source);
    }

    /// <summary>Verifies reactive view hosts navigate through visual-tree subscriptions.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveViewHosts_CoverAttachedSubscriptions()
    {
        var viewModelView = new ViewB();
        var viewModelHost = new TestableReactiveViewModelViewHost { DefaultContent = DefaultContentValue, ViewLocator = new StaticViewLocator(viewModelView) };
        var viewModelWindow = new Window { Content = viewModelHost };

        try
        {
            viewModelWindow.Show();
            viewModelHost.ViewModel = new VmB();
            await Assert.That(viewModelHost.Content).IsSameReferenceAs(viewModelView);
        }
        finally
        {
            viewModelWindow.Close();
        }

        var screen = new ScreenImpl();
        var routedHost = new TestableReactiveRoutedViewHost { DefaultContent = DefaultContentValue, Router = screen.Router, ViewLocator = new StaticViewLocator(new ViewA()) };
        var routedWindow = new Window { Content = routedHost };

        try
        {
            routedWindow.Show();
            _ = screen.Router.Navigate.Execute(new VmA(screen));
            await Assert.That(routedHost.Content).IsTypeOf<ViewA>();

            routedHost.Router = null;
            await Assert.That(routedHost.Content).IsEqualTo(DefaultContentValue);
        }
        finally
        {
            routedWindow.Close();
        }
    }

    /// <summary>Verifies reactive subscription error forwarding preserves exception identity.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveSubscriptionErrors_CoversThrow()
    {
        var error = new InvalidOperationException(ExpectedErrorMessage);

        await Assert.That(() => SubscriptionErrors.Throw(error))
            .ThrowsExactly<InvalidOperationException>();
    }
}
