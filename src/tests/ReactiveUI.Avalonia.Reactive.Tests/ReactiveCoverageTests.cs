// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Rendering;
using ReactiveUI.Avalonia.Reactive;
using ReactiveUI.Reactive;
using ReactiveUI.Reactive.Builder;
using Splat;

using ReactiveAvaloniaScheduler = ReactiveUI.Primitives.Reactive.Concurrency.AvaloniaScheduler;
using RxDisposable = System.Reactive.Disposables.Disposable;

namespace ReactiveUI.Avalonia.Reactive.Tests;

/// <summary>Coverage tests for the normally referenced ReactiveUI.Avalonia.Reactive assembly.</summary>
public class ReactiveCoverageTests
{
    /// <summary>The affinity assigned to Avalonia property notifications.</summary>
    private const int AvaloniaPropertyAffinity = 4;

    /// <summary>The default host content used by navigation tests.</summary>
    private const string DefaultContentValue = "default";

    /// <summary>The missing property name used by notification tests.</summary>
    private const string Missing = nameof(Missing);

    /// <summary>The view contract used by registration tests.</summary>
    private const string ReactiveContract = "reactive";

    /// <summary>The view contract used by navigation tests.</summary>
    private const string ViewContractValue = "contract";

    /// <summary>The maximum time allowed for scheduled test work.</summary>
    private static readonly TimeSpan SchedulerTimeout = TimeSpan.FromSeconds(2);

    /// <summary>Covers AppBuilder extension guard, setup, and private Activator fallback paths.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public Task AppBuilderExtensions_CoverReactivePaths() => VerifyAppBuilderExtensionsAsync();

    /// <summary>Covers reactive AppBuilder paths before async assertion continuations run.</summary>
    [Test]
    public void AppBuilderExtensions_CoverReactivePathsSynchronously() =>
        Ensure(CoverViewCreationSynchronously() && CoverViewRegistrationSynchronously());

    /// <summary>Covers all reactive auto-template hook branches and default template creation.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AutoDataTemplateBindingHook_CoversReactivePaths()
    {
        var hook = new AutoDataTemplateBindingHook();
        var items = new ListBox();

        await Assert.That(() => hook.ExecuteHook(null, items, () => [], null!, BindingDirection.OneWay))
            .ThrowsExactly<ArgumentNullException>();

        await Assert.That(hook.ExecuteHook(null, items, () => [], () => [], BindingDirection.OneWay)).IsTrue();
        await Assert.That(hook.ExecuteHook(null, new TextBlock(), () => [], () => [TextObservedChange(new())], BindingDirection.OneWay)).IsTrue();
        await Assert.That(hook.ExecuteHook(null, items, () => [], () => [TagObservedChange(items)], BindingDirection.OneWay)).IsTrue();

        _ = hook.ExecuteHook(null, items, () => [], () => [ItemsObservedChange(items)], BindingDirection.OneWay);
        await Assert.That(items.ItemTemplate).IsNotNull();
        var control = items.ItemTemplate!.Build(new());
        await Assert.That(control).IsTypeOf<ViewModelViewHost>();
        await Assert.That(((ViewModelViewHost)control!).HorizontalContentAlignment).IsEqualTo(HorizontalAlignment.Stretch);
        await Assert.That(((ViewModelViewHost)control).VerticalContentAlignment).IsEqualTo(VerticalAlignment.Stretch);

        var templated = new ListBox { ItemTemplate = new FuncDataTemplate<object>((_, _) => new TextBlock(), true) };
        _ = hook.ExecuteHook(null, templated, () => [], () => [ItemsSourceObservedChange(templated)], BindingDirection.OneWay);
        await Assert.That(templated.ItemTemplate).IsNotNull();

        var dataTemplated = new ListBox();
        dataTemplated.DataTemplates.Add(new FuncDataTemplate<object>((_, _) => new TextBlock(), true));
        _ = hook.ExecuteHook(null, dataTemplated, () => [], () => [ItemsObservedChange(dataTemplated)], BindingDirection.OneWay);
        await Assert.That(dataTemplated.ItemTemplate).IsNull();

        var lastChangeWins = new ListBox();
        _ = hook.ExecuteHook(
            null,
            lastChangeWins,
            () => [],
            () => [TagObservedChange(lastChangeWins), ItemsSourceObservedChange(lastChangeWins)],
            BindingDirection.OneWay);
        await Assert.That(lastChangeWins.ItemTemplate).IsNotNull();

        var ignoredLastChange = new ListBox();
        _ = hook.ExecuteHook(
            null,
            ignoredLastChange,
            () => [],
            () => [ItemsObservedChange(ignoredLastChange), TagObservedChange(ignoredLastChange)],
            BindingDirection.OneWay);
        await Assert.That(ignoredLastChange.ItemTemplate).IsNull();
    }

    /// <summary>Verifies the public ReactiveUI.Primitives Avalonia scheduler contract.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AvaloniaScheduler_UsesReactivePrimitivesPackage()
    {
        var scheduler = ReactiveAvaloniaScheduler.Instance;
        await Assert.That(typeof(ReactiveAvaloniaScheduler).Assembly.GetName().Name)
            .IsEqualTo("ReactiveUI.Primitives.Avalonia.Reactive");
        await Assert.That(ReactiveAvaloniaScheduler.Instance).IsSameReferenceAs(scheduler);
        await Assert.That(scheduler.Now).IsGreaterThan(DateTimeOffset.MinValue);
        await Assert.That(() => scheduler.Schedule("state", TimeSpan.Zero, null!)).ThrowsExactly<ArgumentNullException>();

        var scheduled = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var backgroundHasUiAccess = true;
        await Task.Run(() =>
        {
            backgroundHasUiAccess = global::Avalonia.Threading.Dispatcher.UIThread.CheckAccess();
            _ = scheduler.Schedule("background", TimeSpan.Zero, (_, state) =>
            {
                if (state == "background")
                {
                    scheduled.SetResult();
                }

                return RxDisposable.Empty;
            });
        });
        await Assert.That(backgroundHasUiAccess).IsFalse();
        await scheduled.Task.WaitAsync(SchedulerTimeout);
    }

    /// <summary>Covers reactive user control and window ViewModel synchronization.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveControls_CoverReactivePaths()
    {
        await Assert.That(typeof(ReactiveWindow<TestViewModel>).Assembly.GetName().Name).IsEqualTo("ReactiveUI.Avalonia.Reactive");
        await Assert.That(typeof(ReactiveWindowBase).Assembly.GetName().Name).IsEqualTo("ReactiveUI.Avalonia.Reactive");

        var vm = new TestViewModel();
        var second = new TestViewModel();
        var control = new ReactiveUserControl<TestViewModel> { DataContext = vm };
        var window = new ReactiveWindow<TestViewModel> { DataContext = vm };

        await VerifyUserControlAsync(control, vm, second);
        await VerifyWindowAsync(window, vm, second);
        var reflectedWindow = await VerifyReflectedControlsAsync(vm, second);
        var baseWindow = await VerifyBaseControlsAsync(vm, second);
        VerifyControlActivation(control, window, reflectedWindow, baseWindow);
    }

    /// <summary>Covers reactive control paths before async assertion continuations run.</summary>
    [Test]
    public void ReactiveControls_CoverReactivePathsSynchronously()
    {
        var vm = new TestViewModel();
        var second = new TestViewModel();
        var control = new ReactiveUserControl<TestViewModel>();

        var controlsCovered = CoverReactiveUserControlsBeforeFirstAwait(control, vm, second);
        var windowsCovered = CoverReactiveWindowsBeforeFirstAwait(control, vm, second);

        Ensure(controlsCovered && windowsCovered);
    }

    /// <summary>Covers reactive property observation and missing-property paths.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AvaloniaObjectObservableForProperty_CoversReactivePaths()
    {
        var sut = new AvaloniaObjectObservableForProperty();
        var control = new TestControl();
        Expression<Func<string?>> expression = () => control.Text;

        await Assert.That(sut.GetAffinityForObject(typeof(TestControl), nameof(TestControl.Text))).IsEqualTo(AvaloniaPropertyAffinity);
        await Assert.That(sut.GetAffinityForObject((Type?)null, nameof(TestControl.Text), beforeChanged: false)).IsEqualTo(0);
        await Assert.That(sut.GetAffinityForObject(typeof(object), "Text")).IsEqualTo(0);
        await Assert.That(sut.GetAffinityForObject(typeof(TestControl), Missing)).IsEqualTo(0);

        IObservedChange<object?, object?>? observed = null;
        using (sut.GetNotificationForProperty(control, expression, nameof(TestControl.Text), beforeChanged: false)
            .Subscribe(new RecordingObserver<IObservedChange<object?, object?>>(value => observed = value)))
        {
            control.Text = ReactiveContract;
            await Assert.That(observed).IsNotNull();
            await Assert.That(observed!.Value).IsEqualTo(ReactiveContract);
        }

        await Assert.That(() => sut.GetNotificationForProperty(new(), expression, "Text"))
            .ThrowsExactly<InvalidOperationException>();
        await Assert.That(() => sut.GetNotificationForProperty(control, expression, Missing, beforeChanged: false, suppressWarnings: false))
            .ThrowsExactly<MissingMemberException>();
        await Assert.That(() => sut.GetNotificationForProperty(null!, expression, "Text"))
            .ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Covers reactive view-host navigation and visual-tree subscriptions.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public Task ViewHosts_CoverReactivePaths() => VerifyViewHostsAsync();

    /// <summary>Covers reactive view-host attach paths before async assertion continuations run.</summary>
    [Test]
    public void ViewHosts_CoverReactiveAttachPathsSynchronously()
    {
        var source = GetPresentationSource();
        var viewModelView = new ViewB();
        var viewModelHost = new TestableViewModelViewHost { DefaultContent = DefaultContentValue, ViewLocator = new StaticViewLocator(viewModelView) };

        viewModelHost.Attach(source);
        viewModelHost.Attach(source);
        viewModelHost.ViewContract = ViewContractValue;
        viewModelHost.ViewModel = new VmB();
        viewModelHost.Detach(source);

        var screen = new ScreenImpl();
        var routedView = new ViewA();
        var routedHost = new TestableRoutedViewHost { DefaultContent = DefaultContentValue, Router = screen.Router, ViewLocator = new StaticViewLocator(routedView) };

        routedHost.Attach(source);
        routedHost.Attach(source);
        _ = screen.Router.Navigate.Execute(new VmA(screen));
        routedHost.Router = null;
        routedHost.Detach(source);

        Ensure(viewModelHost.Content is ViewB && routedHost.Content is string);
    }

    /// <summary>Covers subscription error forwarding.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task SubscriptionErrors_CoversReactiveThrow() =>
        await Assert.That(() => SubscriptionErrors.Throw(new InvalidOperationException("expected")))
            .ThrowsExactly<InvalidOperationException>();

    /// <summary>Verifies all reactive AppBuilder extension paths.</summary>
    /// <returns>A task representing the asynchronous verification.</returns>
    private static async Task VerifyAppBuilderExtensionsAsync()
    {
        await VerifyUseReactiveUIConfigurationAsync();
        await VerifyViewCreationAsync();
        await VerifyViewRegistrationAsync();
        await VerifyDependencyInjectionRegistrationAsync();
    }

    /// <summary>Verifies all reactive view-host paths.</summary>
    /// <returns>A task representing the asynchronous verification.</returns>
    private static async Task VerifyViewHostsAsync()
    {
        var view = new ViewB();
        var host = new TestableViewModelViewHost { DefaultContent = DefaultContentValue, ViewContract = ViewContractValue, ViewLocator = new StaticViewLocator(view, ViewContractValue) };
        var source = GetPresentationSource();
        await VerifyViewModelViewHostAsync(host, new(), view, source);
        await VerifyRoutedViewHostAsync(source);
    }

    /// <summary>Verifies reactive AppBuilder configuration guards and callbacks.</summary>
    /// <returns>A task representing the asynchronous verification.</returns>
    private static async Task VerifyUseReactiveUIConfigurationAsync()
    {
        AppBuilder? builder = null;
        await Assert.That(() => builder!.UseReactiveUI(_ => { })).ThrowsExactly<ArgumentNullException>();
        await Assert.That(() => AppBuilder.Configure<Application>().UseReactiveUI(null!))
            .ThrowsExactly<ArgumentNullException>();

        var configured = false;
        builder = AppBuilder.Configure<Application>().UseReactiveUI(_ => configured = true);
        InvokeAfterPlatformServicesSetup(builder);
        await Assert.That(configured).IsTrue();
    }

    /// <summary>Verifies view creation through the locator and Activator fallback paths.</summary>
    /// <returns>A task representing the asynchronous verification.</returns>
    private static async Task VerifyViewCreationAsync()
    {
        _ = new ActivatorCreatedView();
        await Assert.That(InvokeCreateView(typeof(ActivatorCreatedView))).IsTypeOf<ActivatorCreatedView>();
        await Assert.That(() => InvokeCreateView(typeof(int?))).ThrowsExactly<InvalidOperationException>();

        var originalLocator = Locator.GetLocator();
        try
        {
            AppLocator.SetLocator(new ThrowingResolver());
            await Assert.That(InvokeCreateView(typeof(ActivatorCreatedView))).IsTypeOf<ActivatorCreatedView>();

            var resolvedByLocator = new LocatorCreatedView();
            AppLocator.SetLocator(new ConstantResolver(resolvedByLocator));
            await Assert.That(InvokeCreateView(typeof(LocatorCreatedView))).IsSameReferenceAs(resolvedByLocator);
        }
        finally
        {
            AppLocator.SetLocator(originalLocator);
        }
    }

    /// <summary>Verifies view-registration overloads and their guarded private implementation.</summary>
    /// <returns>A task representing the asynchronous verification.</returns>
    private static async Task VerifyViewRegistrationAsync()
    {
        var registrationBuilder = AppBuilder.Configure<Application>()
            .RegisterReactiveUIViews(typeof(RegistrationViewModel).Assembly, typeof(RegistrationViewModel).Assembly);
        InvokeAfterPlatformServicesSetup(registrationBuilder);

        var serviceType = typeof(IViewFor<>).MakeGenericType(typeof(RegistrationViewModel));
        _ = new RegistrationViewModel();
        _ = new RegistrationView();
        _ = new ContractedRegistrationView();
        await Assert.That(AppLocator.Current.GetService(serviceType)).IsNotNull();
        await Assert.That(AppLocator.Current.GetService(serviceType, ReactiveContract)).IsTypeOf<ContractedRegistrationView>();

        InvokeRegistrationGuardPaths();
        InvokeMarkerRegistration();
        await Assert.That(AppLocator.Current.GetService(serviceType)).IsNotNull();
        InvokeEntryAssemblyRegistration();
    }

    /// <summary>Verifies reactive builder and dependency-injection registration paths.</summary>
    /// <returns>A task representing the asynchronous verification.</returns>
    private static async Task VerifyDependencyInjectionRegistrationAsync()
    {
        ReactiveUIBuilder? reactiveBuilder = null;
        await Assert.That(() => reactiveBuilder!.WithAvalonia()).ThrowsExactly<ArgumentNullException>();
        reactiveBuilder = RxAppBuilder.CreateReactiveUIBuilder();
        await Assert.That(reactiveBuilder.WithAvalonia()).IsSameReferenceAs(reactiveBuilder);

        var containerFactoryCalled = false;
        var containerConfigCalled = false;
        var dependencyResolverFactoryCalled = false;
        var originalLocator = Locator.GetLocator();
        try
        {
            Locator.SetLocator(new ThrowingResolver());
            var containerBuilder = AppBuilder.Configure<Application>().UseReactiveUIWithDIContainer(
                () => MarkFactoryCalled(ref containerFactoryCalled),
                _ => containerConfigCalled = true,
                _ =>
                {
                    dependencyResolverFactoryCalled = true;
                    return new ThrowingResolver();
                },
                _ => { });
            InvokeAfterPlatformServicesSetup(containerBuilder);
        }
        finally
        {
            Locator.SetLocator(originalLocator);
        }

        await Assert.That(containerFactoryCalled).IsFalse();
        await Assert.That(containerConfigCalled).IsFalse();
        await Assert.That(dependencyResolverFactoryCalled).IsFalse();
        await Assert.That(InvokeDependencyInjectionHelper()).IsFalse();
    }

    /// <summary>Exercises null and empty view-registration guard paths.</summary>
    private static void InvokeRegistrationGuardPaths()
    {
        InvokeAfterPlatformServicesSetup(AppBuilder.Configure<Application>().RegisterReactiveUIViews((Assembly[]?)null!));
        InvokeAfterPlatformServicesSetup(AppBuilder.Configure<Application>().RegisterReactiveUIViews());
        InvokeRegisterReactiveUIViews(null, [typeof(RegistrationViewModel).Assembly]);
        InvokeRegisterReactiveUIViews(AppLocator.CurrentMutable!, null);
        InvokeRegisterReactiveUIViews(AppLocator.CurrentMutable!, []);
    }

    /// <summary>Exercises marker-based view registration paths.</summary>
    private static void InvokeMarkerRegistration()
    {
        var markerBuilder = AppBuilder.Configure<Application>().RegisterReactiveUIViewsFromAssemblyOf<RegistrationViewModel>();
        var reflectedMarkerBuilder = InvokeRegisterReactiveUIViewsFromAssemblyOf<RegistrationViewModel>(AppBuilder.Configure<Application>());
        InvokeAfterPlatformServicesSetup(markerBuilder);
        InvokeAfterPlatformServicesSetup(reflectedMarkerBuilder);
    }

    /// <summary>Exercises entry-assembly view registration paths.</summary>
    private static void InvokeEntryAssemblyRegistration()
    {
        InvokeAfterPlatformServicesSetup(AppBuilder.Configure<Application>().RegisterReactiveUIViewsFromEntryAssembly());
        _ = InvokeRegisterReactiveUIViewsFromEntryAssembly(AppBuilder.Configure<Application>(), null);
        var reflectedBuilder = InvokeRegisterReactiveUIViewsFromEntryAssembly(
            AppBuilder.Configure<Application>(),
            typeof(RegistrationViewModel).Assembly);
        InvokeAfterPlatformServicesSetup(reflectedBuilder);
    }

    /// <summary>Verifies view creation paths without asynchronous assertions.</summary>
    /// <returns><see langword="true"/> when all covered paths succeed.</returns>
    private static bool CoverViewCreationSynchronously()
    {
        var originalLocator = Locator.GetLocator();
        try
        {
            AppLocator.SetLocator(new ThrowingResolver());
            return InvokeCreateView(typeof(ActivatorCreatedView)) is ActivatorCreatedView
                && ThrowsExactly<InvalidOperationException>(() => InvokeCreateView(typeof(int?)));
        }
        finally
        {
            AppLocator.SetLocator(originalLocator);
        }
    }

    /// <summary>Verifies view registration paths without asynchronous assertions.</summary>
    /// <returns><see langword="true"/> when all covered paths succeed.</returns>
    private static bool CoverViewRegistrationSynchronously()
    {
        var registrationBuilder = AppBuilder.Configure<Application>()
            .RegisterReactiveUIViews(typeof(RegistrationViewModel).Assembly, typeof(RegistrationViewModel).Assembly);
        InvokeAfterPlatformServicesSetup(registrationBuilder);
        var serviceType = typeof(IViewFor<>).MakeGenericType(typeof(RegistrationViewModel));
        var registrationWorks = AppLocator.Current.GetService(serviceType) is not null
            && AppLocator.Current.GetService(serviceType, ReactiveContract) is ContractedRegistrationView;

        InvokeRegistrationGuardPaths();
        InvokeMarkerRegistration();
        var noEntryBuilder = AppBuilder.Configure<Application>();
        var noEntryWorks = ReferenceEquals(InvokeRegisterReactiveUIViewsFromEntryAssembly(noEntryBuilder, null), noEntryBuilder);
        InvokeEntryAssemblyRegistration();
        return registrationWorks && noEntryWorks && InvokeDependencyInjectionHelper() is false;
    }

    /// <summary>Invokes the private dependency-injection helper and reports whether it created a container.</summary>
    /// <returns><see langword="true"/> if the container factory was invoked.</returns>
    private static bool InvokeDependencyInjectionHelper()
    {
        var factoryCalled = false;
        InvokeConfigureReactiveUIDIContainer(
            null,
            () => MarkFactoryCalled(ref factoryCalled),
            _ => { },
            _ => new ThrowingResolver());
        return factoryCalled;
    }

    /// <summary>Marks a container factory invocation and returns a new container.</summary>
    /// <param name="factoryCalled">The factory invocation flag.</param>
    /// <returns>A new container object.</returns>
    private static object MarkFactoryCalled(ref bool factoryCalled)
    {
        factoryCalled = true;
        return new();
    }

    /// <summary>Verifies generic reactive user-control synchronization.</summary>
    /// <param name="control">The control under test.</param>
    /// <param name="viewModel">The initial view model.</param>
    /// <param name="secondViewModel">The replacement view model.</param>
    /// <returns>A task representing the asynchronous verification.</returns>
    private static async Task VerifyUserControlAsync(
        ReactiveUserControl<TestViewModel> control,
        TestViewModel viewModel,
        TestViewModel secondViewModel)
    {
        await Assert.That(control.ViewModel).IsSameReferenceAs(viewModel);
        control.DataContext = new();
        await Assert.That(control.ViewModel).IsSameReferenceAs(viewModel);
        ((IViewFor)control).ViewModel = secondViewModel;
        await Assert.That(control.DataContext).IsSameReferenceAs(secondViewModel);
        await Assert.That(((IViewFor)control).ViewModel).IsSameReferenceAs(secondViewModel);
        ((IViewFor)control).ViewModel = null;
        await Assert.That(control.DataContext).IsNull();
        await Assert.That(() => SetInvalidViewModel(control)).ThrowsExactly<InvalidCastException>();
    }

    /// <summary>Verifies generic reactive window synchronization.</summary>
    /// <param name="window">The window under test.</param>
    /// <param name="viewModel">The initial view model.</param>
    /// <param name="secondViewModel">The replacement view model.</param>
    /// <returns>A task representing the asynchronous verification.</returns>
    private static async Task VerifyWindowAsync(
        ReactiveWindow<TestViewModel> window,
        TestViewModel viewModel,
        TestViewModel secondViewModel)
    {
        await Assert.That(window.ViewModel).IsSameReferenceAs(viewModel);
        window.DataContext = new();
        await Assert.That(window.ViewModel).IsSameReferenceAs(viewModel);
        ((IViewFor)window).ViewModel = secondViewModel;
        await Assert.That(window.DataContext).IsSameReferenceAs(secondViewModel);
        ((IViewFor)window).ViewModel = null;
        await Assert.That(window.DataContext).IsNull();
        await Assert.That(() => SetInvalidViewModel(window)).ThrowsExactly<InvalidCastException>();
    }

    /// <summary>Verifies reflection-based control and window paths.</summary>
    /// <param name="viewModel">The initial view model.</param>
    /// <param name="secondViewModel">The replacement view model.</param>
    /// <returns>The reflected reactive window.</returns>
    private static async Task<ReactiveWindow<TestViewModel>> VerifyReflectedControlsAsync(
        TestViewModel viewModel,
        TestViewModel secondViewModel)
    {
        RuntimeHelpers.RunClassConstructor(typeof(ReactiveWindowBase).TypeHandle);
        var reflectedWindowType = typeof(ReactiveWindow<TestViewModel>);
        var reflectedWindow = (ReactiveWindow<TestViewModel>)Activator.CreateInstance(reflectedWindowType)!;
        var reflectedWindowProperty = reflectedWindowType.GetProperty(nameof(ReactiveWindow<>.ViewModel))!;
        reflectedWindowProperty.SetValue(reflectedWindow, viewModel);
        await Assert.That(reflectedWindowProperty.GetValue(reflectedWindow)).IsSameReferenceAs(viewModel);
        typeof(IViewFor).GetProperty(nameof(IViewFor.ViewModel))!.SetValue(reflectedWindow, secondViewModel);
        await Assert.That(reflectedWindow.ViewModel).IsSameReferenceAs(secondViewModel);

        var reflectedControlType = typeof(ReactiveUserControl<TestViewModel>);
        var reflectedControl = (ReactiveUserControl<TestViewModel>)Activator.CreateInstance(reflectedControlType)!;
        var reflectedControlProperty = reflectedControlType.GetProperty(nameof(ReactiveUserControl<>.ViewModel))!;
        reflectedControlProperty.SetValue(reflectedControl, viewModel);
        await Assert.That(reflectedControlProperty.GetValue(reflectedControl)).IsSameReferenceAs(viewModel);
        typeof(IViewFor).GetProperty(nameof(IViewFor.ViewModel))!.SetValue(reflectedControl, secondViewModel);
        await Assert.That(reflectedControl.ViewModel).IsSameReferenceAs(secondViewModel);

        var validViewModel = typeof(ReactiveWindowBase)
            .GetMethod("IsValidViewModelValue", BindingFlags.Instance | BindingFlags.NonPublic)!;
        await Assert.That((bool)validViewModel.Invoke(reflectedWindow, [viewModel])!).IsTrue();
        await Assert.That((bool)validViewModel.Invoke(reflectedWindow, [new object()])!).IsFalse();
        return reflectedWindow;
    }

    /// <summary>Verifies the non-generic reactive control bases.</summary>
    /// <param name="viewModel">The initial view model.</param>
    /// <param name="secondViewModel">The replacement view model.</param>
    /// <returns>The base reactive window.</returns>
    private static async Task<TestableWindowBase> VerifyBaseControlsAsync(TestViewModel viewModel, TestViewModel secondViewModel)
    {
        var directControlBase = new TestableUserControlBase { DataContext = viewModel };
        await Assert.That(directControlBase.ViewModel).IsSameReferenceAs(viewModel);
        directControlBase.ViewModel = secondViewModel;
        await Assert.That(directControlBase.DataContext).IsSameReferenceAs(secondViewModel);
        await Assert.That(directControlBase.IsValidViewModel(new())).IsTrue();

        var baseWindow = new TestableWindowBase { DataContext = viewModel };
        await Assert.That(baseWindow.ViewModel).IsSameReferenceAs(viewModel);
        baseWindow.DataContext = new();
        await Assert.That(baseWindow.ViewModel).IsSameReferenceAs(baseWindow.DataContext);
        baseWindow.ViewModel = secondViewModel;
        await Assert.That(baseWindow.DataContext).IsSameReferenceAs(secondViewModel);
        return baseWindow;
    }

    /// <summary>Verifies visual activation of the reactive controls.</summary>
    /// <param name="control">The generic control.</param>
    /// <param name="window">The generic window.</param>
    /// <param name="reflectedWindow">The reflected window.</param>
    /// <param name="baseWindow">The non-generic window base.</param>
    private static void VerifyControlActivation(
        ReactiveUserControl<TestViewModel> control,
        ReactiveWindow<TestViewModel> window,
        ReactiveWindow<TestViewModel> reflectedWindow,
        TestableWindowBase baseWindow)
    {
        var activationWindow = new Window { Content = control };
        var directActivationWindow = new ReactiveWindow<TestViewModel> { ViewModel = new() };
        try
        {
            activationWindow.Show();
            directActivationWindow.Show();
            window.Show();
            reflectedWindow.Show();
            baseWindow.Show();
        }
        finally
        {
            baseWindow.Close();
            reflectedWindow.Close();
            window.Close();
            directActivationWindow.Close();
            activationWindow.Close();
        }
    }

    /// <summary>Verifies ViewModelViewHost navigation and attachment paths.</summary>
    /// <param name="host">The host under test.</param>
    /// <param name="viewModel">The view model to navigate to.</param>
    /// <param name="view">The expected resolved view.</param>
    /// <param name="source">The presentation source.</param>
    /// <returns>A task representing the asynchronous verification.</returns>
    private static async Task VerifyViewModelViewHostAsync(
        TestableViewModelViewHost host,
        VmB viewModel,
        ViewB view,
        IPresentationSource source)
    {
        host.ViewModel = viewModel;
        await Assert.That(host.ViewModel).IsSameReferenceAs(viewModel);
        InvokeNavigation(host, viewModel, ViewContractValue);
        await Assert.That(host.Content).IsSameReferenceAs(view);
        InvokeNavigation(host, null, null);
        await Assert.That(host.Content).IsEqualTo(DefaultContentValue);
        host.ViewLocator = new StaticViewLocator(null, ViewContractValue);
        InvokeNavigation(host, viewModel, ViewContractValue);
        await Assert.That(host.Content).IsEqualTo(DefaultContentValue);
        host.ViewLocator = new StaticViewLocator(new ViewB());
        InvokeNavigation(host, new VmB(), null);
        await Assert.That(host.Content).IsTypeOf<ViewB>();
        var plainVm = new VmB();
        var plainView = new PlainViewB();
        host.ViewLocator = new StaticViewLocator(plainView);
        InvokeNavigation(host, plainVm, null);
        await Assert.That(host.Content).IsSameReferenceAs(plainView);
        await Assert.That(plainView.ViewModel).IsSameReferenceAs(plainVm);
        host.ViewLocator = null;
        InvokeNavigation(host, new UnregisteredVm(), null);
        await Assert.That(host.Content).IsEqualTo(DefaultContentValue);

        host.Attach(source);
        host.ViewContract = "other";
        host.ViewModel = new VmB();
        host.Attach(source);
        host.Detach(source);
        SetNavigationDisposables(host, new());
        host.Attach(source);
        InvokeDisposeNavigationDisposables(host);
    }

    /// <summary>Verifies RoutedViewHost navigation and attachment paths.</summary>
    /// <param name="source">The presentation source.</param>
    /// <returns>A task representing the asynchronous verification.</returns>
    private static async Task VerifyRoutedViewHostAsync(IPresentationSource source)
    {
        var screen = new ScreenImpl();
        var routedView = new ViewA();
        var routed = new TestableRoutedViewHost { DefaultContent = DefaultContentValue };
        routed.Router = screen.Router;
        routed.ViewContract = ViewContractValue;
        routed.ViewLocator = new StaticViewLocator(routedView, ViewContractValue);
        var route = new VmA(screen);

        InvokeNavigation(routed, route, ViewContractValue);
        await Assert.That(routed.Content).IsSameReferenceAs(routedView);
        routed.Router = null;
        InvokeNavigation(routed, route, null);
        await Assert.That(routed.Content).IsEqualTo(DefaultContentValue);
        routed.Router = screen.Router;
        InvokeNavigation(routed, null, null);
        await Assert.That(routed.Content).IsEqualTo(DefaultContentValue);
        routed.ViewLocator = new StaticViewLocator(null, ViewContractValue);
        InvokeNavigation(routed, route, ViewContractValue);
        await Assert.That(routed.Content).IsEqualTo(DefaultContentValue);
        routed.ViewLocator = new StaticViewLocator(new ViewA());
        InvokeNavigation(routed, new VmA(screen), null);
        await Assert.That(routed.Content).IsTypeOf<ViewA>();
        var plainRoute = new VmA(screen);
        var plainRoutedView = new PlainViewA();
        routed.ViewLocator = new StaticViewLocator(plainRoutedView);
        InvokeNavigation(routed, plainRoute, null);
        await Assert.That(routed.Content).IsSameReferenceAs(plainRoutedView);
        await Assert.That(plainRoutedView.ViewModel).IsSameReferenceAs(plainRoute);
        routed.ViewLocator = null;
        InvokeNavigation(routed, new UnregisteredVm(), null);
        await Assert.That(routed.Content).IsEqualTo(DefaultContentValue);

        routed.Attach(source);
        routed.Router = null;
        routed.Attach(source);
        routed.Detach(source);
        SetNavigationDisposables(routed, new());
        routed.Attach(source);
        InvokeDisposeNavigationDisposables(routed);
    }

    /// <summary>Invokes AppBuilder platform setup callback.</summary>
    /// <param name="builder">The app builder.</param>
    private static void InvokeAfterPlatformServicesSetup(AppBuilder builder) =>
        GetAfterPlatformServicesSetupCallback(builder)?.Invoke(builder);

    /// <summary>Gets the AppBuilder platform setup callback.</summary>
    /// <param name="builder">The app builder.</param>
    /// <returns>The configured callback, if any.</returns>
    private static Action<AppBuilder>? GetAfterPlatformServicesSetupCallback(AppBuilder builder) =>
        (Action<AppBuilder>?)typeof(AppBuilder).GetProperty(
            "AfterPlatformServicesSetupCallback",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(builder);

    /// <summary>Invokes the private CreateView fallback path.</summary>
    /// <param name="viewType">The view type.</param>
    /// <returns>The created view.</returns>
    private static object InvokeCreateView(Type viewType)
    {
        var method = typeof(AppBuilderExtensions).GetMethod("CreateView", BindingFlags.Static | BindingFlags.NonPublic);

        try
        {
            return method!.Invoke(null, [viewType])!;
        }
        catch (TargetInvocationException error) when (error.InnerException is not null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(error.InnerException).Throw();
            throw;
        }
    }

    /// <summary>Invokes the private guarded view registration helper.</summary>
    /// <param name="resolver">The resolver to register with.</param>
    /// <param name="assemblies">The assemblies to scan.</param>
    private static void InvokeRegisterReactiveUIViews(IMutableDependencyResolver? resolver, Assembly[]? assemblies)
    {
        var method = typeof(AppBuilderExtensions)
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .Single(candidate =>
                candidate.Name == "RegisterReactiveUIViews"
                && candidate.GetParameters() is [{ ParameterType: var resolverType }, { ParameterType: var assembliesType }]
                && resolverType == typeof(IMutableDependencyResolver)
                && assembliesType == typeof(Assembly[]));

        _ = method.Invoke(null, [resolver, assemblies]);
    }

    /// <summary>Invokes the public generic assembly marker registration method through reflection.</summary>
    /// <typeparam name="TMarker">The marker type.</typeparam>
    /// <param name="builder">The app builder.</param>
    /// <returns>The app builder returned by the method.</returns>
    private static AppBuilder InvokeRegisterReactiveUIViewsFromAssemblyOf<TMarker>(AppBuilder builder)
    {
        var method = typeof(AppBuilderExtensions)
            .GetMethods(BindingFlags.Static | BindingFlags.Public)
            .Single(candidate => candidate.Name == "RegisterReactiveUIViewsFromAssemblyOf" && candidate.IsGenericMethodDefinition);

        return (AppBuilder)method.MakeGenericMethod(typeof(TMarker)).Invoke(null, [builder])!;
    }

    /// <summary>Invokes the private entry assembly registration helper.</summary>
    /// <param name="builder">The app builder.</param>
    /// <param name="entryAssembly">The entry assembly.</param>
    /// <returns>The app builder returned by the method.</returns>
    private static AppBuilder InvokeRegisterReactiveUIViewsFromEntryAssembly(AppBuilder builder, Assembly? entryAssembly)
    {
        var method = typeof(AppBuilderExtensions)
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .Single(candidate =>
                candidate.Name == "RegisterReactiveUIViewsFromEntryAssembly"
                && candidate.GetParameters() is [{ ParameterType: var builderType }, { ParameterType: var assemblyType }]
                && builderType == typeof(AppBuilder)
                && assemblyType == typeof(Assembly));

        return (AppBuilder)method.Invoke(null, [builder, entryAssembly])!;
    }

    /// <summary>Invokes the private dependency injection configuration helper.</summary>
    /// <typeparam name="TContainer">The container type.</typeparam>
    /// <param name="resolver">The mutable resolver.</param>
    /// <param name="containerFactory">The container factory.</param>
    /// <param name="containerConfig">The container configuration action.</param>
    /// <param name="dependencyResolverFactory">The dependency resolver factory.</param>
    private static void InvokeConfigureReactiveUIDIContainer<TContainer>(
        IMutableDependencyResolver? resolver,
        Func<TContainer> containerFactory,
        Action<TContainer> containerConfig,
        Func<TContainer, IDependencyResolver> dependencyResolverFactory)
        where TContainer : class
    {
        var method = typeof(AppBuilderExtensions)
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .Single(candidate => candidate.Name == "ConfigureReactiveUIDIContainer" && candidate.IsGenericMethodDefinition);

        _ = method.MakeGenericMethod(typeof(TContainer)).Invoke(
            null,
            [resolver, containerFactory, containerConfig, dependencyResolverFactory]);
    }

    /// <summary>Covers reactive user-control synchronization paths before the first test await.</summary>
    /// <param name="control">The generic reactive user control.</param>
    /// <param name="viewModel">The initial view model.</param>
    /// <param name="secondViewModel">The replacement view model.</param>
    /// <returns><see langword="true"/> when all user-control paths behaved as expected.</returns>
    private static bool CoverReactiveUserControlsBeforeFirstAwait(
        ReactiveUserControl<TestViewModel> control,
        TestViewModel viewModel,
        TestViewModel secondViewModel)
    {
        control.DataContext = viewModel;
        var controlInitial = ReferenceEquals(control.ViewModel, viewModel);
        control.DataContext = CreateObject();
        var controlIgnoresInvalidDataContext = ReferenceEquals(control.ViewModel, viewModel);
        ((IViewFor)control).ViewModel = secondViewModel;
        var controlInterfaceGet = ReferenceEquals(((IViewFor)control).ViewModel, secondViewModel);
        var controlUpdatesDataContext = ReferenceEquals(control.DataContext, secondViewModel);
        ((IViewFor)control).ViewModel = null;
        var controlClearsDataContext = control.DataContext is null;
        var controlInvalidThrows = ThrowsExactly<InvalidCastException>(() => SetInvalidViewModel(control));

        var baseControl = new TestableUserControlBase { DataContext = viewModel };
        var baseControlInitial = ReferenceEquals(baseControl.ViewModel, viewModel);
        baseControl.ViewModel = secondViewModel;
        var baseControlUpdatesDataContext = ReferenceEquals(baseControl.DataContext, secondViewModel);
        var baseControlValidatesAnyValue = baseControl.IsValidViewModel(CreateObject());

        return All(
            controlInitial,
            controlIgnoresInvalidDataContext,
            controlInterfaceGet,
            controlUpdatesDataContext,
            controlClearsDataContext,
            controlInvalidThrows,
            baseControlInitial,
            baseControlUpdatesDataContext,
            baseControlValidatesAnyValue);
    }

    /// <summary>Covers reactive window synchronization paths before the first test await.</summary>
    /// <param name="activationContent">The control hosted by the activation window.</param>
    /// <param name="viewModel">The initial view model.</param>
    /// <param name="secondViewModel">The replacement view model.</param>
    /// <returns><see langword="true"/> when all window paths behaved as expected.</returns>
    private static bool CoverReactiveWindowsBeforeFirstAwait(
        Control activationContent,
        TestViewModel viewModel,
        TestViewModel secondViewModel)
    {
        var window = new ReactiveWindow<TestViewModel> { DataContext = viewModel };
        var windowInitial = ReferenceEquals(window.ViewModel, viewModel);
        window.DataContext = CreateObject();
        var windowIgnoresInvalidDataContext = ReferenceEquals(window.ViewModel, viewModel);
        ((IViewFor)window).ViewModel = secondViewModel;
        var windowInterfaceGet = ReferenceEquals(((IViewFor)window).ViewModel, secondViewModel);
        var windowUpdatesDataContext = ReferenceEquals(window.DataContext, secondViewModel);
        ((IViewFor)window).ViewModel = null;
        var windowClearsDataContext = window.DataContext is null;
        var windowInvalidThrows = ThrowsExactly<InvalidCastException>(() => SetInvalidViewModel(window));

        var baseWindow = new TestableWindowBase { DataContext = viewModel };
        var baseWindowInitial = ReferenceEquals(baseWindow.ViewModel, viewModel);
        baseWindow.ViewModel = secondViewModel;
        var baseWindowUpdatesDataContext = ReferenceEquals(baseWindow.DataContext, secondViewModel);
        var baseWindowAcceptsAnyValue = (bool)typeof(ReactiveWindowBase)
            .GetMethod("IsValidViewModelValue", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(baseWindow, [CreateObject()])!;

        var activationWindow = new Window { Content = activationContent };
        var directActivationWindow = new ReactiveWindow<TestViewModel> { ViewModel = new() };
        try
        {
            activationWindow.Show();
            directActivationWindow.Show();
            window.Show();
            baseWindow.Show();
        }
        finally
        {
            baseWindow.Close();
            window.Close();
            directActivationWindow.Close();
            activationWindow.Close();
        }

        return All(
            windowInitial,
            windowIgnoresInvalidDataContext,
            windowInterfaceGet,
            windowUpdatesDataContext,
            windowClearsDataContext,
            windowInvalidThrows,
            baseWindowInitial,
            baseWindowUpdatesDataContext,
            baseWindowAcceptsAnyValue);
    }

    /// <summary>Returns whether all values are true.</summary>
    /// <param name="values">The values to inspect.</param>
    /// <returns><see langword="true"/> when all supplied values are true.</returns>
    private static bool All(params bool[] values) =>
        values.All(static value => value);

    /// <summary>Creates a fresh object for invalid-value coverage paths.</summary>
    /// <returns>A new object instance.</returns>
    private static object CreateObject() => new();

    /// <summary>Ensures a condition is true for synchronous coverage tests.</summary>
    /// <param name="condition">The condition to inspect.</param>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="condition"/> is false.</exception>
    private static void Ensure(bool condition)
    {
        if (condition)
        {
            return;
        }

        throw new InvalidOperationException("The synchronous coverage check failed.");
    }

    /// <summary>Returns whether an action throws exactly the specified exception type.</summary>
    /// <typeparam name="TException">The expected exception type.</typeparam>
    /// <param name="action">The action to execute.</param>
    /// <returns><see langword="true"/> when the action throws exactly <typeparamref name="TException"/>; otherwise, <see langword="false"/>.</returns>
    private static bool ThrowsExactly<TException>(Action action)
        where TException : Exception
    {
        try
        {
            action();
            return false;
        }
        catch (Exception error) when (error.GetType() == typeof(TException))
        {
            return true;
        }
    }

    /// <summary>Assigns an invalid view model value through the non-generic interface.</summary>
    /// <param name="view">The view.</param>
    private static void SetInvalidViewModel(IViewFor view) =>
        view.ViewModel = new();

    /// <summary>Invokes ViewModelViewHost private navigation.</summary>
    /// <param name="host">The host.</param>
    /// <param name="viewModel">The view model.</param>
    /// <param name="contract">The view contract.</param>
    private static void InvokeNavigation(ViewModelViewHost host, object? viewModel, string? contract)
    {
        var method = typeof(ViewModelViewHost).GetMethod("NavigateToViewModel", BindingFlags.Instance | BindingFlags.NonPublic);
        _ = method!.Invoke(host, [viewModel, contract]);
    }

    /// <summary>Invokes RoutedViewHost private navigation.</summary>
    /// <param name="host">The host.</param>
    /// <param name="viewModel">The view model.</param>
    /// <param name="contract">The view contract.</param>
    private static void InvokeNavigation(RoutedViewHost host, object? viewModel, string? contract)
    {
        var method = typeof(RoutedViewHost).GetMethod("NavigateToViewModel", BindingFlags.Instance | BindingFlags.NonPublic);
        _ = method!.Invoke(host, [viewModel, contract]);
    }

    /// <summary>Sets ViewModelViewHost navigation disposables through reflection.</summary>
    /// <param name="host">The host.</param>
    /// <param name="disposables">The disposables.</param>
    private static void SetNavigationDisposables(ViewModelViewHost host, System.Reactive.Disposables.CompositeDisposable disposables)
    {
        var field = typeof(ViewModelViewHost).GetField("_navigationDisposables", BindingFlags.Instance | BindingFlags.NonPublic);
        field!.SetValue(host, disposables);
    }

    /// <summary>Sets RoutedViewHost navigation disposables through reflection.</summary>
    /// <param name="host">The host.</param>
    /// <param name="disposables">The disposables.</param>
    private static void SetNavigationDisposables(RoutedViewHost host, System.Reactive.Disposables.CompositeDisposable disposables)
    {
        var field = typeof(RoutedViewHost).GetField("_navigationDisposables", BindingFlags.Instance | BindingFlags.NonPublic);
        field!.SetValue(host, disposables);
    }

    /// <summary>Invokes ViewModelViewHost navigation disposal through reflection.</summary>
    /// <param name="host">The host.</param>
    private static void InvokeDisposeNavigationDisposables(ViewModelViewHost host)
    {
        var method = typeof(ViewModelViewHost).GetMethod("DisposeNavigationDisposables", BindingFlags.Instance | BindingFlags.NonPublic);
        _ = method!.Invoke(host, null);
    }

    /// <summary>Invokes RoutedViewHost navigation disposal through reflection.</summary>
    /// <param name="host">The host.</param>
    private static void InvokeDisposeNavigationDisposables(RoutedViewHost host)
    {
        var method = typeof(RoutedViewHost).GetMethod("DisposeNavigationDisposables", BindingFlags.Instance | BindingFlags.NonPublic);
        _ = method!.Invoke(host, null);
    }

    /// <summary>Creates an observed change for ItemsControl.Items.</summary>
    /// <param name="items">The items control.</param>
    /// <returns>The observed change.</returns>
    private static ObservedChange<object, object> ItemsObservedChange(ItemsControl items)
    {
        var param = Expression.Parameter(typeof(ItemsControl), "x");
        var member = Expression.Property(param, nameof(ItemsControl.Items));
        return new(items, member, items.Items!);
    }

    /// <summary>Creates an observed change for ItemsControl.ItemsSource.</summary>
    /// <param name="items">The items control.</param>
    /// <returns>The observed change.</returns>
    private static ObservedChange<object, object> ItemsSourceObservedChange(ItemsControl items)
    {
        var param = Expression.Parameter(typeof(ItemsControl), "x");
        var member = Expression.Property(param, nameof(ItemsControl.ItemsSource));
        return new(items, member, items.ItemsSource!);
    }

    /// <summary>Creates an observed change for Control.Tag.</summary>
    /// <param name="control">The control.</param>
    /// <returns>The observed change.</returns>
    private static ObservedChange<object, object> TagObservedChange(Control control)
    {
        var param = Expression.Parameter(typeof(Control), "x");
        var member = Expression.Property(param, nameof(Control.Tag));
        return new(control, member, control.Tag!);
    }

    /// <summary>Creates an observed change for TextBlock.Text.</summary>
    /// <param name="text">The text block.</param>
    /// <returns>The observed change.</returns>
    private static ObservedChange<object, object> TextObservedChange(TextBlock text)
    {
        var param = Expression.Parameter(typeof(TextBlock), "x");
        var member = Expression.Property(param, nameof(TextBlock.Text));
        return new(text, member, text.Text!);
    }

    /// <summary>Gets a live presentation source.</summary>
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

    /// <summary>A view contract attribute for registration tests.</summary>
    /// <param name="contract">The contract.</param>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ViewContractAttribute(string contract) : Attribute
    {
        /// <summary>Gets the contract.</summary>
        public string Contract { get; } = contract;
    }

    /// <summary>A resolver that throws during direct concrete lookup.</summary>
    protected class ThrowingResolver : IDependencyResolver
    {
        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public virtual object? GetService(Type? serviceType) =>
            throw new InvalidOperationException($"Cannot resolve {serviceType}.");

        /// <inheritdoc/>
        public virtual object? GetService(Type? serviceType, string? contract) =>
            throw new InvalidOperationException($"Cannot resolve {serviceType} for {contract}.");

        /// <inheritdoc/>
        public T? GetService<T>() =>
            default;

        /// <inheritdoc/>
        public T? GetService<T>(string? contract) =>
            default;

        /// <inheritdoc/>
        public IEnumerable<object> GetServices(Type? serviceType) =>
            GetServices(serviceType, contract: null);

        /// <inheritdoc/>
        public IEnumerable<object> GetServices(Type? serviceType, string? contract) =>
            [];

        /// <inheritdoc/>
        public IEnumerable<T> GetServices<T>() =>
            GetServices<T>(contract: null);

        /// <inheritdoc/>
        public IEnumerable<T> GetServices<T>(string? contract)
        {
            _ = typeof(T);
            return [];
        }

        /// <inheritdoc/>
        public bool HasRegistration(Type? serviceType) =>
            false;

        /// <inheritdoc/>
        public bool HasRegistration(Type? serviceType, string? contract) =>
            false;

        /// <inheritdoc/>
        public bool HasRegistration<T>() =>
            HasRegistration<T>(contract: null);

        /// <inheritdoc/>
        public bool HasRegistration<T>(string? contract)
        {
            _ = typeof(T);
            return false;
        }

        /// <inheritdoc/>
        public void Register(Func<object?> factory, Type? serviceType)
        {
        }

        /// <inheritdoc/>
        public void Register(Func<object?> factory, Type? serviceType, string? contract)
        {
        }

        /// <inheritdoc/>
        public void Register<T>(Func<T?> factory)
        {
        }

        /// <inheritdoc/>
        public void Register<T>(Func<T?> factory, string? contract)
        {
        }

        /// <inheritdoc/>
        public void Register<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService, new() =>
            Register<TService, TImplementation>(contract: null);

        /// <inheritdoc/>
        public void Register<TService, TImplementation>(string? contract)
            where TService : class
            where TImplementation : class, TService, new()
        {
            _ = typeof(TService);
            _ = typeof(TImplementation);
        }

        /// <inheritdoc/>
        public void UnregisterCurrent(Type? serviceType)
        {
        }

        /// <inheritdoc/>
        public void UnregisterCurrent(Type? serviceType, string? contract)
        {
        }

        /// <inheritdoc/>
        public void UnregisterCurrent<T>() => UnregisterCurrent<T>(contract: null);

        /// <inheritdoc/>
        public void UnregisterCurrent<T>(string? contract) => _ = typeof(T);

        /// <inheritdoc/>
        public void UnregisterAll(Type? serviceType)
        {
        }

        /// <inheritdoc/>
        public void UnregisterAll(Type? serviceType, string? contract)
        {
        }

        /// <inheritdoc/>
        public void UnregisterAll<T>() => UnregisterAll<T>(contract: null);

        /// <inheritdoc/>
        public void UnregisterAll<T>(string? contract) => UnregisterCurrent<T>(contract);

        /// <inheritdoc/>
        public IDisposable ServiceRegistrationCallback(Type serviceType, Action<IDisposable> callback) =>
            RxDisposable.Empty;

        /// <inheritdoc/>
        public IDisposable ServiceRegistrationCallback(Type serviceType, string? contract, Action<IDisposable> callback) =>
            RxDisposable.Empty;

        /// <inheritdoc/>
        public IDisposable ServiceRegistrationCallback<T>(Action<IDisposable> callback) =>
            ServiceRegistrationCallback<T>(contract: null, callback);

        /// <inheritdoc/>
        public IDisposable ServiceRegistrationCallback<T>(string? contract, Action<IDisposable> callback)
        {
            _ = typeof(T);
            return RxDisposable.Empty;
        }

        /// <inheritdoc/>
        public void RegisterConstant<T>(T? value)
            where T : class
        {
        }

        /// <inheritdoc/>
        public void RegisterConstant<T>(T? value, string? contract)
            where T : class
        {
        }

        /// <inheritdoc/>
        public void RegisterLazySingleton<T>(Func<T?> valueFactory)
            where T : class
        {
        }

        /// <inheritdoc/>
        public void RegisterLazySingleton<T>(Func<T?> valueFactory, string? contract)
            where T : class
        {
        }

        /// <summary>Disposes this resolver. It owns no managed or unmanaged resources.</summary>
        /// <param name="disposing">Whether the caller is disposing managed resources.</param>
        protected virtual void Dispose(bool disposing) => _ = disposing;
    }

    /// <summary>A recording observer.</summary>
    /// <typeparam name="T">The observed type.</typeparam>
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

    /// <summary>A test control with a styled text property.</summary>
    private sealed class TestControl : Control
    {
        /// <summary>The text property.</summary>
        private static readonly StyledProperty<string?> TextProperty =
            AvaloniaProperty.Register<TestControl, string?>(nameof(Text));

        /// <summary>Gets or sets text.</summary>
        public string? Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
    }

    /// <summary>A test view model.</summary>
    private sealed class TestViewModel : ReactiveObject;

    /// <summary>A concrete non-generic reactive window base.</summary>
    private sealed class TestableWindowBase : ReactiveWindowBase;

    /// <summary>A concrete non-generic reactive user control base.</summary>
    private sealed class TestableUserControlBase : ReactiveUserControlBase
    {
        /// <summary>Exposes base view model validation.</summary>
        /// <param name="value">The value to validate.</param>
        /// <returns><see langword="true"/> when the base class accepts the value.</returns>
        public bool IsValidViewModel(object? value) => IsValidViewModelValue(value);
    }

    /// <summary>A view model without registrations.</summary>
    private sealed class UnregisteredVm : ReactiveObject;

    /// <summary>A routable view model.</summary>
    private sealed class VmA : ReactiveObject, IRoutableViewModel
    {
        /// <summary>Initializes a new instance of the <see cref="VmA"/> class.</summary>
        /// <param name="screen">The host screen.</param>
        public VmA(IScreen screen)
        {
            HostScreen = screen;
        }

        /// <summary>Gets the route path.</summary>
        public string? UrlPathSegment => "a";

        /// <summary>Gets the host screen.</summary>
        public IScreen HostScreen { get; }
    }

    /// <summary>A simple view model.</summary>
    private sealed class VmB : ReactiveObject;

    /// <summary>A test screen.</summary>
    private sealed class ScreenImpl : ReactiveObject, IScreen
    {
        /// <summary>Gets the router.</summary>
        public RoutingState Router { get; } = new();
    }

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
    }

    /// <summary>A plain non-Avalonia view for VmA.</summary>
    private sealed class PlainViewA : IViewFor<VmA>
    {
        /// <summary>Gets or sets the view model.</summary>
        public VmA? ViewModel { get; set; }

        /// <inheritdoc/>
        object? IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (VmA?)value;
        }
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
    }

    /// <summary>A plain non-Avalonia view for VmB.</summary>
    private sealed class PlainViewB : IViewFor<VmB>
    {
        /// <summary>Gets or sets the view model.</summary>
        public VmB? ViewModel { get; set; }

        /// <inheritdoc/>
        object? IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (VmB?)value;
        }
    }

    /// <summary>A view model used for view registration.</summary>
    private sealed class RegistrationViewModel : ReactiveObject;

    /// <summary>A default registration view.</summary>
    private sealed class RegistrationView : UserControl, IViewFor<RegistrationViewModel>
    {
        /// <summary>Gets or sets the view model.</summary>
        public RegistrationViewModel? ViewModel { get; set; }

        /// <inheritdoc/>
        object? IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (RegistrationViewModel?)value;
        }
    }

    /// <summary>A contracted registration view.</summary>
    [ViewContract("reactive")]
    private sealed class ContractedRegistrationView : UserControl, IViewFor<RegistrationViewModel>
    {
        /// <summary>Gets or sets the view model.</summary>
        public RegistrationViewModel? ViewModel { get; set; }

        /// <inheritdoc/>
        object? IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (RegistrationViewModel?)value;
        }
    }

    /// <summary>A view created through Activator fallback.</summary>
    private sealed class ActivatorCreatedView : UserControl;

    /// <summary>A view returned by the test dependency resolver.</summary>
    private sealed class LocatorCreatedView : UserControl;

    /// <summary>A static view locator for host tests.</summary>
    /// <param name="view">The view to return.</param>
    /// <param name="contract">The matching contract.</param>
    private sealed class StaticViewLocator(IViewFor? view, string? contract = null) : IViewLocator
    {
        /// <summary>The view to return.</summary>
        private readonly IViewFor? _view = view;

        /// <summary>The matching contract.</summary>
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

        /// <summary>Returns whether the contract matches.</summary>
        /// <param name="contract">The requested contract.</param>
        /// <returns><see langword="true"/> if the contract matches.</returns>
        private bool IsMatch(string? contract) =>
            string.Equals(_contract, contract, StringComparison.Ordinal);
    }

    /// <summary>A testable ViewModelViewHost.</summary>
    private sealed class TestableViewModelViewHost : ViewModelViewHost
    {
        /// <summary>Raises attached-to-visual-tree.</summary>
        /// <param name="source">The presentation source.</param>
        public void Attach(IPresentationSource source) =>
            OnAttachedToVisualTree(new(this, source));

        /// <summary>Raises detached-from-visual-tree.</summary>
        /// <param name="source">The presentation source.</param>
        public void Detach(IPresentationSource source) =>
            OnDetachedFromVisualTree(new(this, source));
    }

    /// <summary>A testable RoutedViewHost.</summary>
    private sealed class TestableRoutedViewHost : RoutedViewHost
    {
        /// <summary>Raises attached-to-visual-tree.</summary>
        /// <param name="source">The presentation source.</param>
        public void Attach(IPresentationSource source) =>
            OnAttachedToVisualTree(new(this, source));

        /// <summary>Raises detached-from-visual-tree.</summary>
        /// <param name="source">The presentation source.</param>
        public void Detach(IPresentationSource source) =>
            OnDetachedFromVisualTree(new(this, source));
    }

    /// <summary>A resolver that returns a single constant value for matching concrete types.</summary>
    /// <param name="value">The service value.</param>
    private sealed class ConstantResolver(object value) : ThrowingResolver
    {
        /// <summary>The service value.</summary>
        private readonly object _value = value;

        /// <inheritdoc/>
        public override object? GetService(Type? serviceType) =>
            serviceType == _value.GetType() ? _value : null;

        /// <inheritdoc/>
        public override object? GetService(Type? serviceType, string? contract) =>
            serviceType == _value.GetType() && contract is null ? _value : null;
    }
}
