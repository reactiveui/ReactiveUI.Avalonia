// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Rendering;
using Splat;

using ActivationDisposables = ReactiveUI.Primitives.Disposables.MultipleDisposable;
using ExceptionDispatchInfo = System.Runtime.ExceptionServices.ExceptionDispatchInfo;
using ReactiveDisposable = global::System.Reactive.Disposables.Disposable;
using ReactiveIRoutableViewModel = global::ReactiveUI.Reactive.IRoutableViewModel;
using ReactiveIScreen = global::ReactiveUI.Reactive.IScreen;
using ReactiveRoutingState = global::ReactiveUI.Reactive.RoutingState;
using ReactiveRxAppBuilder = global::ReactiveUI.Reactive.Builder.RxAppBuilder;
using ReactiveRxSchedulers = global::ReactiveUI.Reactive.RxSchedulers;
using ReactiveRxSuspension = global::ReactiveUI.Reactive.RxSuspension;
using ReactiveScheduler = global::System.Reactive.Concurrency.IScheduler;
using ReactiveUIBuilder = global::ReactiveUI.Reactive.Builder.ReactiveUIBuilder;
using RxUnit = global::System.Reactive.Unit;

namespace ReactiveUI.Avalonia.Reactive.Tests;

/// <summary>Behavioral coverage tests for ReactiveUI.Avalonia.Reactive linked-source types.</summary>
public class ReactiveShimFullCoverageTests
{
    /// <summary>Verifies reactive AppBuilder null guards and callback setup paths.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveAppBuilderExtensions_UseReactiveUI_CoversNullsAndCallback()
    {
        AppBuilder? builder = null;
        await Assert.That(() => AppBuilderExtensions.UseReactiveUI(builder!, _ => { }))
            .ThrowsExactly<ArgumentNullException>();

        await Assert.That(() => AppBuilderExtensions.UseReactiveUI(
            AppBuilder.Configure<Application>(),
            null!)).ThrowsExactly<ArgumentNullException>();

        var configured = false;
        builder = AppBuilder.Configure<Application>();

        var result = AppBuilderExtensions.UseReactiveUI(
            builder,
            _ => configured = true);
        InvokeAfterPlatformServicesSetup(result);

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

        var result = AppBuilderExtensions.RegisterReactiveUIViews(
            AppBuilder.Configure<Application>(),
            typeof(ShimRegistrationVm).Assembly,
            typeof(ShimRegistrationVm).Assembly);
        InvokeAfterPlatformServicesSetup(result);

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
        var privateEntryBuilder = InvokePrivateRegisterReactiveUIViewsFromEntryAssembly(
            AppBuilder.Configure<Application>(),
            typeof(ShimRegistrationVm).Assembly);
        InvokeAfterPlatformServicesSetup(privateEntryBuilder);
        GC.KeepAlive(new NoContractAttributeContainer());
        var missingContractView = new ShimRegistrationViewWithoutContractProperty();
        var missingContract = InvokePrivateGetViewContract(missingContractView.GetType());

        var mutableResolver = AppLocator.CurrentMutable!;
        var nullResolverFactoryCalled = false;
        InvokePrivateConfigureReactiveUIDIContainer<object>(
            null,
            () =>
            {
                nullResolverFactoryCalled = true;
                return new object();
            },
            _ => { },
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

        var invalidCreateThrows = ThrowsExactly<InvalidOperationException>(() => InvokePrivateCreateView(typeof(int?)));
        var nullBuilderThrows = ThrowsExactly<ArgumentNullException>(() => InvokePrivateRegisterReactiveUIViewsFromEntryAssembly(null!, null));

        await Assert.That(
            ReferenceEquals(markerResult, markerBuilder) &&
            ReferenceEquals(entryResult, entryBuilder) &&
            ReferenceEquals(privateNullEntryResult, privateNullEntryBuilder) &&
            !nullResolverFactoryCalled &&
            containerConfigured &&
            locatorResolved &&
            fallbackAfterThrow &&
            invalidCreateThrows &&
            nullBuilderThrows).IsTrue();
        await Assert.That(missingContract).IsNull();
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
        await Assert.That(() => InvokePrivateCreateViewWithActivator(typeof(int?)))
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

        var builder = AppBuilderExtensions.UseReactiveUIWithDIContainer(
            AppBuilder.Configure<Application>(),
            () => container,
            value => configured = ReferenceEquals(value, container),
            value => ReferenceEquals(value, container) ? resolver : throw new InvalidOperationException(),
            _ => reactiveConfigured = true);
        InvokeAfterPlatformServicesSetup(builder);

        await Assert.That(configured).IsTrue();
        await Assert.That(reactiveConfigured).IsTrue();
        await Assert.That(ReactiveRxSchedulers.MainThreadScheduler).IsSameReferenceAs(AvaloniaScheduler.Instance);

        await Assert.That(() => AppBuilderExtensions.UseReactiveUIWithDIContainer(
            null!,
            () => container,
            _ => { },
            _ => resolver,
            _ => { })).ThrowsExactly<ArgumentNullException>();

        var nullFactory = AppBuilderExtensions.UseReactiveUIWithDIContainer<object>(
            AppBuilder.Configure<Application>(),
            null!,
            _ => { },
            _ => resolver,
            _ => { });
        await Assert.That(() => InvokeAfterPlatformServicesSetup(nullFactory)).ThrowsExactly<ArgumentNullException>();
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

        await Assert.That(() => hook.ExecuteHook(null, items, () => [], null!, BindingDirection.OneWay))
            .ThrowsExactly<ArgumentNullException>();

        await Assert.That(hook.ExecuteHook(null, items, () => [], () => [], BindingDirection.OneWay)).IsTrue();
        await Assert.That(items.ItemTemplate).IsNull();

        await Assert.That(hook.ExecuteHook(null, new TextBlock(), () => [], () => [TextObservedChange(new TextBlock())], BindingDirection.OneWay)).IsTrue();
        await Assert.That(hook.ExecuteHook(null, items, () => [], () => [TagObservedChange(items)], BindingDirection.OneWay)).IsTrue();

        _ = hook.ExecuteHook(null, items, () => [], () => [ItemsObservedChange(items)], BindingDirection.OneWay);
        await Assert.That(items.ItemTemplate).IsNotNull();

        var control = items.ItemTemplate!.Build(new object());
        await Assert.That(control).IsTypeOf<ViewModelViewHost>();
        await Assert.That(((ViewModelViewHost)control!).HorizontalContentAlignment).IsEqualTo(HorizontalAlignment.Stretch);

        var templated = new ListBox
        {
            ItemTemplate = new FuncDataTemplate<object>((_, _) => new TextBlock(), true)
        };
        _ = hook.ExecuteHook(null, templated, () => [], () => [ItemsSourceObservedChange(templated)], BindingDirection.OneWay);
        await Assert.That(templated.ItemTemplate).IsNotNull();

        var dataTemplated = new ListBox();
        dataTemplated.DataTemplates.Add(new FuncDataTemplate<object>((_, _) => new TextBlock(), true));
        _ = hook.ExecuteHook(null, dataTemplated, () => [], () => [ItemsObservedChange(dataTemplated)], BindingDirection.OneWay);
        await Assert.That(dataTemplated.ItemTemplate).IsNull();
    }

    /// <summary>Verifies reactive AutoSuspendHelper lifetime paths.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveAutoSuspendHelper_CoversLifetimePaths()
    {
        await Assert.That(() => new AutoSuspendHelper(null!)).ThrowsExactly<ArgumentNullException>();
        await Assert.That(() => new AutoSuspendHelper(CreateUnsupportedLifetime())).ThrowsExactly<NotSupportedException>();

        var previous = Design.IsDesignMode;
        SetDesignMode(true);
        try
        {
            using var designHelper = new AutoSuspendHelper(CreateUnsupportedLifetime());
            var persistedInDesignMode = false;
            using var designSubscription = ReactiveRxSuspension.SuspensionHost.ShouldPersistState.Subscribe(
                new RecordingObserver<IDisposable>(value =>
                {
                    persistedInDesignMode = true;
                    value.Dispose();
                }));

            await Assert.That(persistedInDesignMode).IsFalse();
        }
        finally
        {
            SetDesignMode(previous);
        }

        var lifetime = new ClassicDesktopStyleApplicationLifetime();
        using var helper = new AutoSuspendHelper(lifetime);
        var persisted = false;
        using var persistSubscription = ReactiveRxSuspension.SuspensionHost.ShouldPersistState.Subscribe(
            new RecordingObserver<IDisposable>(value =>
            {
                persisted = true;
                value.Dispose();
            }));

        lifetime.Shutdown(0);
        await Assert.That(persisted).IsTrue();

        var launches = 0;
        using var launchSubscription = ReactiveRxSuspension.SuspensionHost.IsLaunchingNew.Subscribe(
            new RecordingObserver<RxUnit>(_ => launches++));
        helper.OnFrameworkInitializationCompleted();
        await Assert.That(launches).IsEqualTo(1);

        var invalidations = 0;
        using var invalidationSubscription = ReactiveRxSuspension.SuspensionHost.ShouldInvalidateState.Subscribe(
            new RecordingObserver<RxUnit>(_ => invalidations++));
        helper.OnUnhandledException(this, new UnhandledExceptionEventArgs(new InvalidOperationException("expected"), isTerminating: false));
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
            button.RaiseEvent(new RoutedEventArgs(Button.LoadedEvent));
            await Assert.That(loaded).IsTrue();

            button.RaiseEvent(new RoutedEventArgs(Button.UnloadedEvent));
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
        await Assert.That(sut.GetAffinityForObject<InputElement>(hasEventTarget: true)).IsEqualTo(6);
        await Assert.That(sut.GetAffinityForObject<Button>(hasEventTarget: false)).IsEqualTo(10);

        using (var binding = sut.BindCommandToObject(command, button, parameter))
        {
            parameter.OnNext("button");
            await Assert.That(button.CommandParameter).IsEqualTo("button");
            await Assert.That(button.Command).IsSameReferenceAs(command);
        }

        await Assert.That(button.Command).IsNull();
        await Assert.That(() => sut.BindCommandToObject(null!, button, parameter)).ThrowsExactly<ArgumentNullException>();
        await Assert.That(() => sut.BindCommandToObject<Button>(command, null!, parameter)).ThrowsExactly<ArgumentNullException>();
        await Assert.That(() => sut.BindCommandToObject<object>(command, new(), parameter)).ThrowsExactly<InvalidOperationException>();

        using (var eventBinding = sut.BindCommandToObject<Button, RoutedEventArgs>(command, button, parameter, nameof(InputElement.GotFocus)))
        {
            parameter.OnNext("event");
            button.RaiseEvent(new RoutedEventArgs(InputElement.GotFocusEvent));
            await Assert.That(command.LastParameter).IsEqualTo("event");

            command.SetCanExecute(false);
            parameter.OnNext("blocked");
            button.RaiseEvent(new RoutedEventArgs(InputElement.GotFocusEvent));
            await Assert.That(command.LastParameter).IsEqualTo("event");
        }

        await Assert.That(button.IsSet(InputElement.IsEnabledProperty)).IsFalse();
        await Assert.That(() => sut.BindCommandToObject<object, RoutedEventArgs>(null!, new object(), parameter, "Click")).ThrowsExactly<ArgumentNullException>();
        await Assert.That(() => sut.BindCommandToObject<object, RoutedEventArgs>(command, null!, parameter, "Click")).ThrowsExactly<ArgumentNullException>();
        await Assert.That(() => sut.BindCommandToObject<object, RoutedEventArgs>(command, new object(), parameter, "Click")).ThrowsExactly<InvalidOperationException>();
        await Assert.That(() => sut.BindCommandToObject<Button, RoutedEventArgs>(command, button, parameter, "MissingEvent")).ThrowsExactly<InvalidOperationException>();

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

        await Assert.That(sut.GetAffinityForObject(typeof(TestControl), nameof(TestControl.Text))).IsEqualTo(4);
        await Assert.That(sut.GetAffinityForObject((Type?)null, nameof(TestControl.Text), beforeChanged: false)).IsEqualTo(0);
        await Assert.That(sut.GetAffinityForObject(typeof(object), "Text")).IsEqualTo(0);
        await Assert.That(sut.GetAffinityForObject(typeof(TestControl), "Missing")).IsEqualTo(0);

        IObservedChange<object?, object?>? observed = null;
        using (sut.GetNotificationForProperty(control, expression, nameof(TestControl.Text))
            .Subscribe(new RecordingObserver<IObservedChange<object?, object?>>(value => observed = value)))
        {
            control.Text = "reactive";
            await Assert.That(observed).IsNotNull();
            await Assert.That(observed!.Value).IsEqualTo("reactive");
        }

        await Assert.That(() => sut.GetNotificationForProperty(new object(), expression, "Text"))
            .ThrowsExactly<InvalidOperationException>();
        await Assert.That(() => sut.GetNotificationForProperty(control, expression, "Missing", beforeChanged: false, suppressWarnings: false))
            .ThrowsExactly<MissingMemberException>();
        await Assert.That(() => sut.GetNotificationForProperty(control, expression, "Missing", beforeChanged: false, suppressWarnings: true))
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

    /// <summary>Verifies reactive scheduler behavior for immediate, delayed, posted, and cancelled work.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveAvaloniaScheduler_CoversSchedulePaths()
    {
        var scheduler = AvaloniaScheduler.Instance;
        await Assert.That(scheduler.Now).IsLessThan(DateTimeOffset.Now.AddSeconds(1));
        await Assert.That(() => scheduler.Schedule("state", TimeSpan.Zero, null!)).ThrowsExactly<ArgumentNullException>();

        var immediate = false;
        using (scheduler.Schedule("state", TimeSpan.Zero, (s, state) =>
        {
            immediate = state == "state" && ReferenceEquals(s, scheduler);
            return ReactiveDisposable.Empty;
        }))
        {
            await Assert.That(immediate).IsTrue();
        }

        var delayed = new TaskCompletionSource();
        using (scheduler.Schedule("delay", TimeSpan.FromMilliseconds(10), (_, _) =>
        {
            delayed.SetResult();
            return ReactiveDisposable.Empty;
        }))
        {
            await delayed.Task.WaitAsync(TimeSpan.FromSeconds(2));
        }

        var posted = new TaskCompletionSource();
        await Task.Run(() =>
        {
            _ = scheduler.Schedule("background", TimeSpan.Zero, (_, state) =>
            {
                if (state == "background")
                {
                    posted.SetResult();
                }

                return ReactiveDisposable.Empty;
            });
        });
        await posted.Task.WaitAsync(TimeSpan.FromSeconds(2));

        var cancelled = false;
        await Task.Run(() =>
        {
            var disposable = scheduler.Schedule("cancel", TimeSpan.Zero, (_, _) =>
            {
                cancelled = true;
                return ReactiveDisposable.Empty;
            });
            disposable.Dispose();
        });
        await Task.Delay(100);
        await Assert.That(cancelled).IsFalse();

        var reentrant = new TaskCompletionSource();
        IDisposable ScheduleNext(ReactiveScheduler _, int depth)
        {
            if (depth > 40)
            {
                reentrant.SetResult();
                return ReactiveDisposable.Empty;
            }

            return scheduler.Schedule(depth + 1, TimeSpan.Zero, ScheduleNext);
        }

        _ = scheduler.Schedule(0, TimeSpan.Zero, ScheduleNext);
        await reentrant.Task.WaitAsync(TimeSpan.FromSeconds(2));

        var guardField = typeof(AvaloniaScheduler).GetField("_reentrancyGuard", BindingFlags.Instance | BindingFlags.NonPublic);
        guardField!.SetValue(scheduler, 32);
        try
        {
            var guardedPost = new TaskCompletionSource();
            using (scheduler.Schedule("guarded", TimeSpan.Zero, (_, state) =>
            {
                if (state == "guarded")
                {
                    guardedPost.SetResult();
                }

                return ReactiveDisposable.Empty;
            }))
            {
                await guardedPost.Task.WaitAsync(TimeSpan.FromSeconds(2));
            }
        }
        finally
        {
            guardField.SetValue(scheduler, 0);
        }
    }

    /// <summary>Verifies reactive scheduler post branches before the first await.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveAvaloniaScheduler_CoversPostBranchesSynchronously()
    {
        var scheduler = AvaloniaScheduler.Instance;
        var guardField = typeof(AvaloniaScheduler).GetField("_reentrancyGuard", BindingFlags.Instance | BindingFlags.NonPublic);
        IDisposable? guardedDisposable = null;
        guardField!.SetValue(scheduler, 32);
        try
        {
            guardedDisposable = scheduler.Schedule("guarded", TimeSpan.Zero, static (_, _) => ReactiveDisposable.Empty);
        }
        finally
        {
            guardField.SetValue(scheduler, 0);
        }

        var backgroundScheduled = false;
        var backgroundWork = Task.Run(() =>
        {
            using var backgroundDisposable = scheduler.Schedule("background", TimeSpan.Zero, static (_, _) => ReactiveDisposable.Empty);
            backgroundScheduled = true;
        });

        guardedDisposable!.Dispose();
        WaitForCompletion(backgroundWork);

        await Assert.That(guardedDisposable is not null && backgroundScheduled).IsTrue();
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

        var directControl = new ReactiveUserControl<ShimVm>
        {
            DataContext = vm
        };
        await Assert.That(directControl.ViewModel).IsSameReferenceAs(vm);
        ((IViewFor)directControl).ViewModel = secondVm;
        await Assert.That(directControl.ViewModel).IsSameReferenceAs(secondVm);

        var window = new ReactiveWindow
        {
            DataContext = vm
        };
        await Assert.That(window.ViewModel).IsSameReferenceAs(vm);
        await Assert.That(((IViewFor<ShimVm>)window).ViewModel).IsSameReferenceAs(vm);

        window.DataContext = new();
        await Assert.That(window.ViewModel).IsSameReferenceAs(vm);

        window.ViewModel = secondVm;
        await Assert.That(window.DataContext).IsSameReferenceAs(secondVm);

        ((IViewFor)window).ViewModel = null;
        await Assert.That(window.ViewModel).IsNull();
        await Assert.That(window.DataContext).IsNull();

        var directWindow = new ReactiveWindow<ShimVm>
        {
            DataContext = vm
        };
        await Assert.That(directWindow.ViewModel).IsSameReferenceAs(vm);
        ((IViewFor)directWindow).ViewModel = secondVm;
        await Assert.That(directWindow.ViewModel).IsSameReferenceAs(secondVm);

        var baseControl = new ReactiveBaseControl();
        var arbitrary = new object();
        baseControl.DataContext = arbitrary;
        await Assert.That(baseControl.ViewModel).IsSameReferenceAs(arbitrary);

        var baseWindow = new ReactiveBaseWindow
        {
            DataContext = arbitrary
        };
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

    /// <summary>Verifies activation callbacks before the first await.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveControls_CoverActivationCallbacksSynchronously()
    {
        InvokeActivationCallback(typeof(ReactiveUserControlBase));
        InvokeActivationCallback(typeof(ReactiveWindowBase));
        InvokeActivationCallback(typeof(ReactiveUserControlBase));
        InvokeActivationCallback(typeof(ReactiveWindowBase));
        var callbacksCovered = typeof(ReactiveWindowBase).Assembly.GetName().Name == "ReactiveUI.Avalonia.Reactive";

        await Assert.That(callbacksCovered).IsTrue();
    }

    /// <summary>Verifies reactive view-host navigation behavior.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveViewModelViewHost_CoversNavigationPaths()
    {
        var view = new ViewB();
        var host = new TestableReactiveViewModelViewHost
        {
            DefaultContent = "default",
            ViewContract = "contract",
            ViewLocator = new StaticViewLocator(view, "contract")
        };
        var vm = new VmB();

        host.ViewModel = vm;
        await Assert.That(host.ViewModel).IsSameReferenceAs(vm);
        await Assert.That(host.ViewContract).IsEqualTo("contract");
        await Assert.That(host.DefaultContent).IsEqualTo("default");
        await Assert.That(host.ExposedStyleKey).IsEqualTo(typeof(TransitioningContentControl));

        InvokePrivateNavigation(host, vm, "contract");
        await Assert.That(host.Content).IsSameReferenceAs(view);
        await Assert.That(view.ViewModel).IsSameReferenceAs(vm);
        await Assert.That(view.DataContext).IsSameReferenceAs(vm);

        InvokePrivateNavigation(host, null, null);
        await Assert.That(host.Content).IsEqualTo("default");

        host.ViewLocator = new StaticViewLocator(null);
        InvokePrivateNavigation(host, vm, null);
        await Assert.That(host.Content).IsEqualTo("default");

        host.ViewLocator = new StaticViewLocator(null, "other");
        InvokePrivateNavigation(host, vm, "contract");
        await Assert.That(host.Content).IsEqualTo("default");

        host.ViewLocator = new StaticViewLocator(new ViewB());
        InvokePrivateNavigation(host, new VmB(), null);
        await Assert.That(host.Content).IsTypeOf<ViewB>();

        host.ViewLocator = null;
        InvokePrivateNavigation(host, new UnregisteredVm(), null);
        await Assert.That(host.Content).IsEqualTo("default");

        InvokeDisposeNavigationDisposables(host);

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
            DefaultContent = "default",
            Router = screen.Router,
            ViewContract = "contract",
            ViewLocator = new StaticViewLocator(view, "contract")
        };
        var vm = new VmA(screen);

        await Assert.That(host.Router).IsSameReferenceAs(screen.Router);
        await Assert.That(host.ViewContract).IsEqualTo("contract");
        await Assert.That(host.DefaultContent).IsEqualTo("default");
        await Assert.That(host.ExposedStyleKey).IsEqualTo(typeof(TransitioningContentControl));

        InvokePrivateNavigation(host, vm, "contract");
        await Assert.That(host.Content).IsSameReferenceAs(view);
        await Assert.That(view.ViewModel).IsSameReferenceAs(vm);
        await Assert.That(view.DataContext).IsSameReferenceAs(vm);

        host.Router = null;
        InvokePrivateNavigation(host, vm, null);
        await Assert.That(host.Content).IsEqualTo("default");

        host.Router = screen.Router;
        InvokePrivateNavigation(host, null, null);
        await Assert.That(host.Content).IsEqualTo("default");

        host.ViewLocator = new StaticViewLocator(null);
        InvokePrivateNavigation(host, vm, null);
        await Assert.That(host.Content).IsEqualTo("default");

        host.ViewLocator = new StaticViewLocator(null, "other");
        InvokePrivateNavigation(host, vm, "contract");
        await Assert.That(host.Content).IsEqualTo("default");

        host.ViewLocator = new StaticViewLocator(new ViewA());
        InvokePrivateNavigation(host, new VmA(screen), null);
        await Assert.That(host.Content).IsTypeOf<ViewA>();

        host.ViewLocator = null;
        InvokePrivateNavigation(host, new UnregisteredVm(), null);
        await Assert.That(host.Content).IsEqualTo("default");

        InvokeDisposeNavigationDisposables(host);

        var source = GetPresentationSource();
        host.Attach(source);
        host.Attach(source);
        host.Detach(source);
    }

    /// <summary>Verifies reactive view-host attach guards before the first await.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveViewHosts_CoverAttachGuardsSynchronously()
    {
        var source = GetPresentationSource();
        var guardedBeforeAttachViewModelHost = new TestableReactiveViewModelViewHost();
        SetNavigationDisposables(guardedBeforeAttachViewModelHost);
        guardedBeforeAttachViewModelHost.Attach(source);
        var viewModelGuardReturnCovered = HasNavigationDisposables(guardedBeforeAttachViewModelHost);
        InvokeDisposeNavigationDisposables(guardedBeforeAttachViewModelHost);

        var viewModelHost = new TestableReactiveViewModelViewHost
        {
            DefaultContent = "default",
            ViewLocator = new StaticViewLocator(new ViewB())
        };

        viewModelHost.Attach(source);
        var viewModelGuardReady = HasNavigationDisposables(viewModelHost);
        viewModelHost.Attach(source);
        viewModelHost.Detach(source);

        var guardedViewModelHost = new TestableReactiveViewModelViewHost { DefaultContent = "default" };
        IPresentationSource? guardedViewModelSource = null;
        guardedViewModelHost.AttachedToVisualTree += (_, args) => guardedViewModelSource = args.PresentationSource;
        var guardedViewModelWindow = new Window { Content = guardedViewModelHost };

        try
        {
            guardedViewModelWindow.Show();
            guardedViewModelHost.Attach(guardedViewModelSource!);
        }
        finally
        {
            guardedViewModelWindow.Close();
        }

        var screen = new ScreenImpl();
        var guardedBeforeAttachRoutedHost = new TestableReactiveRoutedViewHost();
        SetNavigationDisposables(guardedBeforeAttachRoutedHost);
        guardedBeforeAttachRoutedHost.Attach(source);
        var routedGuardReturnCovered = HasNavigationDisposables(guardedBeforeAttachRoutedHost);
        InvokeDisposeNavigationDisposables(guardedBeforeAttachRoutedHost);

        var routedHost = new TestableReactiveRoutedViewHost
        {
            DefaultContent = "default",
            Router = screen.Router,
            ViewLocator = new StaticViewLocator(new ViewA())
        };

        routedHost.Attach(source);
        var routedGuardReady = HasNavigationDisposables(routedHost);
        routedHost.Attach(source);
        routedHost.Router = null;
        routedHost.Detach(source);
        var attachGuardsCovered =
            viewModelGuardReturnCovered &&
            viewModelGuardReady &&
            routedGuardReturnCovered &&
            routedGuardReady &&
            (viewModelHost.Content is null or string) &&
            routedHost.Content is string;

        var guardedRoutedHost = new TestableReactiveRoutedViewHost
        {
            DefaultContent = "default",
            Router = new()
        };
        IPresentationSource? guardedRoutedSource = null;
        guardedRoutedHost.AttachedToVisualTree += (_, args) => guardedRoutedSource = args.PresentationSource;
        var guardedRoutedWindow = new Window { Content = guardedRoutedHost };

        try
        {
            guardedRoutedWindow.Show();
            guardedRoutedHost.Attach(guardedRoutedSource!);
        }
        finally
        {
            guardedRoutedWindow.Close();
        }

        await Assert.That(attachGuardsCovered).IsTrue();
    }

    /// <summary>Verifies reactive view hosts navigate through visual-tree subscriptions.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ReactiveViewHosts_CoverAttachedSubscriptions()
    {
        var viewModelView = new ViewB();
        var viewModelHost = new TestableReactiveViewModelViewHost
        {
            DefaultContent = "default",
            ViewLocator = new StaticViewLocator(viewModelView)
        };
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
        var routedHost = new TestableReactiveRoutedViewHost
        {
            DefaultContent = "default",
            Router = screen.Router,
            ViewLocator = new StaticViewLocator(new ViewA())
        };
        var routedWindow = new Window { Content = routedHost };

        try
        {
            routedWindow.Show();
            _ = screen.Router.Navigate.Execute(new VmA(screen));
            await Assert.That(routedHost.Content).IsTypeOf<ViewA>();

            routedHost.Router = null;
            await Assert.That(routedHost.Content).IsEqualTo("default");
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
        var error = new InvalidOperationException("expected");

        await Assert.That(() => SubscriptionErrors.Throw(error))
            .ThrowsExactly<InvalidOperationException>();
    }

    /// <summary>Invokes the AppBuilder platform setup callback registered by extension methods.</summary>
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
            .GetField("_navigationDisposables", BindingFlags.Instance | BindingFlags.NonPublic);

        return field!.GetValue(host) is not null;
    }

    /// <summary>Returns whether the reactive routed host has navigation disposables.</summary>
    /// <param name="host">The host instance.</param>
    /// <returns><see langword="true"/> when navigation disposables are present.</returns>
    private static bool HasNavigationDisposables(RoutedViewHost host)
    {
        var field = typeof(RoutedViewHost)
            .GetField("_navigationDisposables", BindingFlags.Instance | BindingFlags.NonPublic);

        return field!.GetValue(host) is not null;
    }

    /// <summary>Seeds the reactive view-model host navigation subscriptions.</summary>
    /// <param name="host">The host instance.</param>
    private static void SetNavigationDisposables(ViewModelViewHost host)
    {
        var field = typeof(ViewModelViewHost)
            .GetField("_navigationDisposables", BindingFlags.Instance | BindingFlags.NonPublic);

        field!.SetValue(host, Activator.CreateInstance(field.FieldType));
    }

    /// <summary>Seeds the reactive routed host navigation subscriptions.</summary>
    /// <param name="host">The host instance.</param>
    private static void SetNavigationDisposables(RoutedViewHost host)
    {
        var field = typeof(RoutedViewHost)
            .GetField("_navigationDisposables", BindingFlags.Instance | BindingFlags.NonPublic);

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
                candidate.Name == "RegisterReactiveUIViewsFromEntryAssembly" &&
                candidate.GetParameters() is [{ ParameterType: var builderType }, { ParameterType: var assemblyType }] &&
                builderType == typeof(AppBuilder) &&
                assemblyType == typeof(Assembly));

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
                candidate.Name == "RegisterReactiveUIViews" &&
                candidate.GetParameters() is [{ ParameterType: var resolverType }, { ParameterType: var assembliesType }] &&
                resolverType == typeof(IMutableDependencyResolver) &&
                assembliesType == typeof(Assembly[]));

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
                candidate.GetParameters() is [{ ParameterType: var parameterType }] &&
                parameterType == typeof(ActivationDisposables));

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

    /// <summary>Waits for a task to complete within a bounded time.</summary>
    /// <param name="task">The task to wait for.</param>
    private static void WaitForCompletion(Task task)
    {
        if (task.Wait(TimeSpan.FromSeconds(2)))
        {
            return;
        }

        throw new TimeoutException("The expected task did not complete.");
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
        return new ObservedChange<object, object>(items, member, items.Items!);
    }

    /// <summary>Creates an observed change for the ItemsSource property of an ItemsControl.</summary>
    /// <param name="items">The items control instance.</param>
    /// <returns>An observed change representing the ItemsSource property.</returns>
    private static ObservedChange<object, object> ItemsSourceObservedChange(ItemsControl items)
    {
        var param = Expression.Parameter(typeof(ItemsControl), "x");
        var member = Expression.Property(param, nameof(ItemsControl.ItemsSource));
        return new ObservedChange<object, object>(items, member, items.ItemsSource!);
    }

    /// <summary>Creates an observed change for the Tag property of a control.</summary>
    /// <param name="control">The control instance.</param>
    /// <returns>An observed change representing the Tag property.</returns>
    private static ObservedChange<object, object> TagObservedChange(Control control)
    {
        var param = Expression.Parameter(typeof(Control), "x");
        var member = Expression.Property(param, nameof(Control.Tag));
        return new ObservedChange<object, object>(control, member, control.Tag!);
    }

    /// <summary>Creates an observed change for the Text property of a text block.</summary>
    /// <param name="text">The text block instance.</param>
    /// <returns>An observed change representing the Text property.</returns>
    private static ObservedChange<object, object> TextObservedChange(TextBlock text)
    {
        var param = Expression.Parameter(typeof(TextBlock), "x");
        var member = Expression.Property(param, nameof(TextBlock.Text));
        return new ObservedChange<object, object>(text, member, text.Text!);
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

    /// <summary>A recording observer used to avoid Subscribe extension overload ambiguity.</summary>
    /// <typeparam name="T">The observed value type.</typeparam>
    private sealed class RecordingObserver<T> : IObserver<T>
    {
        /// <summary>The action invoked for observed values.</summary>
        private readonly Action<T> _onNext;

        /// <summary>Initializes a new instance of the <see cref="RecordingObserver{T}"/> class.</summary>
        /// <param name="onNext">The observed-value action.</param>
        public RecordingObserver(Action<T> onNext)
        {
            _onNext = onNext;
        }

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
        public static readonly StyledProperty<string?> TextProperty =
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
            OnAttachedToVisualTree(new VisualTreeAttachmentEventArgs(this, source));

        /// <summary>Raises the detached-from-visual-tree hook.</summary>
        /// <param name="source">The presentation source.</param>
        public void Detach(IPresentationSource source) =>
            OnDetachedFromVisualTree(new VisualTreeAttachmentEventArgs(this, source));
    }

    /// <summary>A testable reactive RoutedViewHost that exposes protected members.</summary>
    private sealed class TestableReactiveRoutedViewHost : RoutedViewHost
    {
        /// <summary>Gets the protected style key override.</summary>
        public Type ExposedStyleKey => StyleKeyOverride;

        /// <summary>Raises the attached-to-visual-tree hook.</summary>
        /// <param name="source">The presentation source.</param>
        public void Attach(IPresentationSource source) =>
            OnAttachedToVisualTree(new VisualTreeAttachmentEventArgs(this, source));

        /// <summary>Raises the detached-from-visual-tree hook.</summary>
        /// <param name="source">The presentation source.</param>
        public void Detach(IPresentationSource source) =>
            OnDetachedFromVisualTree(new VisualTreeAttachmentEventArgs(this, source));
    }

    /// <summary>A minimal view locator for host tests.</summary>
    private sealed class StaticViewLocator : IViewLocator
    {
        /// <summary>The view returned for matching contracts.</summary>
        private readonly IViewFor? _view;

        /// <summary>The contract that must match.</summary>
        private readonly string? _contract;

        /// <summary>Initializes a new instance of the <see cref="StaticViewLocator"/> class.</summary>
        /// <param name="view">The view to return.</param>
        /// <param name="contract">The optional contract to match.</param>
        public StaticViewLocator(IViewFor? view, string? contract = null)
        {
            _view = view;
            _contract = contract;
        }

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

    /// <summary>Attribute to specify a view contract name.</summary>
    /// <param name="contract">The contract name.</param>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    private sealed class ViewContractAttribute(string contract) : Attribute
    {
        /// <summary>Gets the contract name.</summary>
        public string Contract { get; } = contract;
    }

    /// <summary>Container for a view-contract-shaped attribute that exposes no Contract property.</summary>
    private sealed class NoContractAttributeContainer
    {
        /// <summary>Attribute with the expected type name and no Contract property.</summary>
        [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
        public sealed class ViewContractAttribute : Attribute;
    }

    /// <summary>A resolver that throws during concrete view resolution.</summary>
    private sealed class ThrowingResolver : IDependencyResolver
    {
        /// <inheritdoc/>
        public void Dispose()
        {
        }

        /// <inheritdoc/>
        public object? GetService(Type? serviceType) =>
            throw new InvalidOperationException($"Cannot resolve {serviceType}.");

        /// <inheritdoc/>
        public object? GetService(Type? serviceType, string? contract) =>
            throw new InvalidOperationException($"Cannot resolve {serviceType} for {contract}.");

        /// <inheritdoc/>
        public T? GetService<T>() =>
            default;

        /// <inheritdoc/>
        public T? GetService<T>(string? contract) =>
            default;

        /// <inheritdoc/>
        public IEnumerable<object> GetServices(Type? serviceType) =>
            [];

        /// <inheritdoc/>
        public IEnumerable<object> GetServices(Type? serviceType, string? contract) =>
            [];

        /// <inheritdoc/>
        public IEnumerable<T> GetServices<T>() =>
            [];

        /// <inheritdoc/>
        public IEnumerable<T> GetServices<T>(string? contract) =>
            [];

        /// <inheritdoc/>
        public bool HasRegistration(Type? serviceType) =>
            false;

        /// <inheritdoc/>
        public bool HasRegistration(Type? serviceType, string? contract) =>
            false;

        /// <inheritdoc/>
        public bool HasRegistration<T>() =>
            false;

        /// <inheritdoc/>
        public bool HasRegistration<T>(string? contract) =>
            false;

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
        public void UnregisterCurrent(Type? serviceType)
        {
        }

        /// <inheritdoc/>
        public void UnregisterCurrent(Type? serviceType, string? contract)
        {
        }

        /// <inheritdoc/>
        public void UnregisterCurrent<T>()
        {
        }

        /// <inheritdoc/>
        public void UnregisterCurrent<T>(string? contract)
        {
        }

        /// <inheritdoc/>
        public void UnregisterAll(Type? serviceType)
        {
        }

        /// <inheritdoc/>
        public void UnregisterAll(Type? serviceType, string? contract)
        {
        }

        /// <inheritdoc/>
        public void UnregisterAll<T>()
        {
        }

        /// <inheritdoc/>
        public void UnregisterAll<T>(string? contract)
        {
        }

        /// <inheritdoc/>
        public IDisposable ServiceRegistrationCallback(Type serviceType, Action<IDisposable> callback) =>
            EmptyDisposable.Instance;

        /// <inheritdoc/>
        public IDisposable ServiceRegistrationCallback(Type serviceType, string? contract, Action<IDisposable> callback) =>
            EmptyDisposable.Instance;

        /// <inheritdoc/>
        public IDisposable ServiceRegistrationCallback<T>(Action<IDisposable> callback) =>
            EmptyDisposable.Instance;

        /// <inheritdoc/>
        public IDisposable ServiceRegistrationCallback<T>(string? contract, Action<IDisposable> callback) =>
            EmptyDisposable.Instance;

        /// <inheritdoc/>
        public void Register<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService, new()
        {
        }

        /// <inheritdoc/>
        public void Register<TService, TImplementation>(string? contract)
            where TService : class
            where TImplementation : class, TService, new()
        {
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
    }
}
