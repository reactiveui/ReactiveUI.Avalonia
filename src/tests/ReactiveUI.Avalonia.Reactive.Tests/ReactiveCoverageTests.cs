// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Rendering;
using Avalonia.Threading;
using ReactiveUI.Avalonia.Reactive;
using ReactiveUI.Reactive;
using ReactiveUI.Reactive.Builder;
using Splat;

using RxDisposable = System.Reactive.Disposables.Disposable;
using RxScheduler = System.Reactive.Concurrency.IScheduler;
using RxUnit = System.Reactive.Unit;

namespace ReactiveUI.Avalonia.Reactive.Tests;

/// <summary>Coverage tests for the normally referenced ReactiveUI.Avalonia.Reactive assembly.</summary>
public class ReactiveCoverageTests
{
    /// <summary>Covers AppBuilder extension guard, setup, and private Activator fallback paths.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AppBuilderExtensions_CoverReactivePaths()
    {
        AppBuilder? builder = null;
        await Assert.That(() => builder!.UseReactiveUI(_ => { })).ThrowsExactly<ArgumentNullException>();
        await Assert.That(() => AppBuilder.Configure<Application>().UseReactiveUI(null!)).ThrowsExactly<ArgumentNullException>();

        var configured = false;
        builder = AppBuilder.Configure<Application>().UseReactiveUI(_ => configured = true);
        InvokeAfterPlatformServicesSetup(builder);
        await Assert.That(configured).IsTrue();

        _ = new ActivatorCreatedView();
        await Assert.That(InvokeCreateView(typeof(ActivatorCreatedView))).IsTypeOf<ActivatorCreatedView>();
        await Assert.That(() => InvokeCreateView(typeof(int?))).ThrowsExactly<InvalidOperationException>();

        var originalLocator = Locator.GetLocator();
        try
        {
            AppLocator.SetLocator(new ThrowingResolver());
            await Assert.That(InvokeCreateView(typeof(ActivatorCreatedView))).IsTypeOf<ActivatorCreatedView>();
        }
        finally
        {
            AppLocator.SetLocator(originalLocator);
        }

        var registrationBuilder = AppBuilder.Configure<Application>()
            .RegisterReactiveUIViews(typeof(RegistrationViewModel).Assembly, typeof(RegistrationViewModel).Assembly);
        InvokeAfterPlatformServicesSetup(registrationBuilder);

        var serviceType = typeof(IViewFor<>).MakeGenericType(typeof(RegistrationViewModel));
        _ = new RegistrationViewModel();
        _ = new RegistrationView();
        _ = new ContractedRegistrationView();
        await Assert.That(AppLocator.Current.GetService(serviceType)).IsNotNull();
        await Assert.That(AppLocator.Current.GetService(serviceType, "reactive")).IsTypeOf<ContractedRegistrationView>();

        var nullAssembliesBuilder = AppBuilder.Configure<Application>().RegisterReactiveUIViews((Assembly[]?)null!);
        InvokeAfterPlatformServicesSetup(nullAssembliesBuilder);
        var emptyAssembliesBuilder = AppBuilder.Configure<Application>().RegisterReactiveUIViews();
        InvokeAfterPlatformServicesSetup(emptyAssembliesBuilder);
        InvokeRegisterReactiveUIViews(null, [typeof(RegistrationViewModel).Assembly]);
        InvokeRegisterReactiveUIViews(AppLocator.CurrentMutable!, null);
        InvokeRegisterReactiveUIViews(AppLocator.CurrentMutable!, []);

        var markerBuilder = AppBuilder.Configure<Application>().RegisterReactiveUIViewsFromAssemblyOf<RegistrationViewModel>();
        var reflectedMarkerBuilder = InvokeRegisterReactiveUIViewsFromAssemblyOf<RegistrationViewModel>(AppBuilder.Configure<Application>());
        InvokeAfterPlatformServicesSetup(markerBuilder);
        InvokeAfterPlatformServicesSetup(reflectedMarkerBuilder);
        await Assert.That(AppLocator.Current.GetService(serviceType)).IsNotNull();

        var entryBuilder = AppBuilder.Configure<Application>().RegisterReactiveUIViewsFromEntryAssembly();
        InvokeAfterPlatformServicesSetup(entryBuilder);
        await Assert.That(InvokeRegisterReactiveUIViewsFromEntryAssembly(AppBuilder.Configure<Application>(), null)).IsNotNull();
        var reflectedEntryBuilder = InvokeRegisterReactiveUIViewsFromEntryAssembly(
            AppBuilder.Configure<Application>(),
            typeof(RegistrationViewModel).Assembly);
        InvokeAfterPlatformServicesSetup(reflectedEntryBuilder);

        var resolvedByLocator = new LocatorCreatedView();
        originalLocator = Locator.GetLocator();
        try
        {
            AppLocator.SetLocator(new ConstantResolver(resolvedByLocator));
            await Assert.That(InvokeCreateView(typeof(LocatorCreatedView))).IsSameReferenceAs(resolvedByLocator);
        }
        finally
        {
            AppLocator.SetLocator(originalLocator);
        }

        ReactiveUIBuilder? reactiveBuilder = null;
        await Assert.That(() => reactiveBuilder!.WithAvalonia()).ThrowsExactly<ArgumentNullException>();
        reactiveBuilder = RxAppBuilder.CreateReactiveUIBuilder();
        await Assert.That(reactiveBuilder.WithAvalonia()).IsSameReferenceAs(reactiveBuilder);

        var containerFactoryCalled = false;
        var containerConfigCalled = false;
        var dependencyResolverFactoryCalled = false;
        originalLocator = Locator.GetLocator();
        try
        {
            Locator.SetLocator(new ThrowingResolver());

            var containerBuilder = AppBuilder.Configure<Application>().UseReactiveUIWithDIContainer(
                containerFactory: () =>
                {
                    containerFactoryCalled = true;
                    return new object();
                },
                containerConfig: _ => containerConfigCalled = true,
                dependencyResolverFactory: _ =>
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

        var helperFactoryCalled = false;
        InvokeConfigureReactiveUIDIContainer<object>(
            null,
            () =>
            {
                helperFactoryCalled = true;
                return new object();
            },
            _ => { },
            _ => new ThrowingResolver());
        await Assert.That(helperFactoryCalled).IsFalse();
    }

    /// <summary>Covers reactive AppBuilder paths before async assertion continuations run.</summary>
    [Test]
    public void AppBuilderExtensions_CoverReactivePathsSynchronously()
    {
        var activatorCreated = InvokeCreateView(typeof(ActivatorCreatedView)) is ActivatorCreatedView;
        var invalidCreateThrows = ThrowsExactly<InvalidOperationException>(() => InvokeCreateView(typeof(int?)));
        var originalLocator = Locator.GetLocator();
        var fallbackCreated = false;
        try
        {
            AppLocator.SetLocator(new ThrowingResolver());
            fallbackCreated = InvokeCreateView(typeof(ActivatorCreatedView)) is ActivatorCreatedView;
        }
        finally
        {
            AppLocator.SetLocator(originalLocator);
        }

        var registrationBuilder = AppBuilder.Configure<Application>()
            .RegisterReactiveUIViews(typeof(RegistrationViewModel).Assembly, typeof(RegistrationViewModel).Assembly);
        InvokeAfterPlatformServicesSetup(registrationBuilder);

        var serviceType = typeof(IViewFor<>).MakeGenericType(typeof(RegistrationViewModel));
        var registered = AppLocator.Current.GetService(serviceType) is not null;
        var contracted = AppLocator.Current.GetService(serviceType, "reactive") is ContractedRegistrationView;

        var nullAssembliesBuilder = AppBuilder.Configure<Application>().RegisterReactiveUIViews((Assembly[]?)null!);
        InvokeAfterPlatformServicesSetup(nullAssembliesBuilder);
        var emptyAssembliesBuilder = AppBuilder.Configure<Application>().RegisterReactiveUIViews();
        InvokeAfterPlatformServicesSetup(emptyAssembliesBuilder);
        InvokeRegisterReactiveUIViews(null, [typeof(RegistrationViewModel).Assembly]);
        InvokeRegisterReactiveUIViews(AppLocator.CurrentMutable!, null);
        InvokeRegisterReactiveUIViews(AppLocator.CurrentMutable!, []);

        var markerBuilder = AppBuilder.Configure<Application>().RegisterReactiveUIViewsFromAssemblyOf<RegistrationViewModel>();
        var reflectedMarkerBuilder = InvokeRegisterReactiveUIViewsFromAssemblyOf<RegistrationViewModel>(AppBuilder.Configure<Application>());
        InvokeAfterPlatformServicesSetup(markerBuilder);
        InvokeAfterPlatformServicesSetup(reflectedMarkerBuilder);

        var entryBuilder = AppBuilder.Configure<Application>().RegisterReactiveUIViewsFromEntryAssembly();
        InvokeAfterPlatformServicesSetup(entryBuilder);
        var noEntryBuilder = AppBuilder.Configure<Application>();
        var noEntryReturned = ReferenceEquals(InvokeRegisterReactiveUIViewsFromEntryAssembly(noEntryBuilder, null), noEntryBuilder);
        var reflectedEntryBuilder = InvokeRegisterReactiveUIViewsFromEntryAssembly(
            AppBuilder.Configure<Application>(),
            typeof(RegistrationViewModel).Assembly);
        InvokeAfterPlatformServicesSetup(reflectedEntryBuilder);

        var resolvedByLocator = new LocatorCreatedView();
        originalLocator = Locator.GetLocator();
        var locatorResolved = false;
        try
        {
            AppLocator.SetLocator(new ConstantResolver(resolvedByLocator));
            locatorResolved = ReferenceEquals(InvokeCreateView(typeof(LocatorCreatedView)), resolvedByLocator);
        }
        finally
        {
            AppLocator.SetLocator(originalLocator);
        }

        var helperFactoryCalled = false;
        InvokeConfigureReactiveUIDIContainer<object>(
            null,
            () =>
            {
                helperFactoryCalled = true;
                return new object();
            },
            _ => { },
            _ => new ThrowingResolver());

        Ensure(
            activatorCreated &&
            invalidCreateThrows &&
            fallbackCreated &&
            registered &&
            contracted &&
            noEntryReturned &&
            locatorResolved &&
            !helperFactoryCalled);
    }

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
        await Assert.That(hook.ExecuteHook(null, new TextBlock(), () => [], () => [TextObservedChange(new TextBlock())], BindingDirection.OneWay)).IsTrue();
        await Assert.That(hook.ExecuteHook(null, items, () => [], () => [TagObservedChange(items)], BindingDirection.OneWay)).IsTrue();

        _ = hook.ExecuteHook(null, items, () => [], () => [ItemsObservedChange(items)], BindingDirection.OneWay);
        await Assert.That(items.ItemTemplate).IsNotNull();
        var control = items.ItemTemplate!.Build(new object());
        await Assert.That(control).IsTypeOf<ViewModelViewHost>();
        await Assert.That(((ViewModelViewHost)control!).HorizontalContentAlignment).IsEqualTo(HorizontalAlignment.Stretch);
        await Assert.That(((ViewModelViewHost)control).VerticalContentAlignment).IsEqualTo(VerticalAlignment.Stretch);

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

    /// <summary>Covers reactive scheduler immediate, delayed, posted, cancelled, and reentrant paths.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AvaloniaScheduler_CoversReactivePaths()
    {
        var scheduler = AvaloniaScheduler.Instance;
        await Assert.That(typeof(AvaloniaScheduler).Assembly.GetName().Name).IsEqualTo("ReactiveUI.Avalonia.Reactive");
        await Assert.That(scheduler.Now).IsLessThan(DateTimeOffset.Now.AddSeconds(1));
        await Assert.That(() => scheduler.Schedule("state", TimeSpan.Zero, null!)).ThrowsExactly<ArgumentNullException>();

        var immediate = false;
        using (scheduler.Schedule("state", TimeSpan.Zero, (s, state) =>
        {
            immediate = state == "state" && ReferenceEquals(s, scheduler);
            return RxDisposable.Empty;
        }))
        {
            await Assert.That(immediate).IsTrue();
        }

        var delayed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using (scheduler.Schedule("delay", TimeSpan.FromMilliseconds(10), (_, _) =>
        {
            delayed.SetResult();
            return RxDisposable.Empty;
        }))
        {
            await delayed.Task.WaitAsync(TimeSpan.FromSeconds(2));
        }

        var posted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var backgroundHadUiAccess = true;
        await Task.Run(() =>
        {
            backgroundHadUiAccess = Dispatcher.UIThread.CheckAccess();
            _ = InvokeSchedule(scheduler, "background", TimeSpan.Zero, (_, state) =>
            {
                if (state == "background")
                {
                    posted.SetResult();
                }

                return RxDisposable.Empty;
            });
        });
        await Assert.That(backgroundHadUiAccess).IsFalse();
        await posted.Task.WaitAsync(TimeSpan.FromSeconds(2));

        var directlyPosted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using (InvokePostOnDispatcher(scheduler, "direct", (_, state) =>
        {
            if (state == "direct")
            {
                directlyPosted.SetResult();
            }

            return RxDisposable.Empty;
        }))
        {
            await directlyPosted.Task.WaitAsync(TimeSpan.FromSeconds(2));
        }

        var workRan = false;
        var scheduledWork = new AvaloniaScheduler.ScheduledWork<string>(scheduler, "work", (_, state) =>
        {
            workRan = state == "work";
            return RxDisposable.Empty;
        });
        InvokeScheduledWork(scheduledWork, "Execute");
        await Assert.That(workRan).IsTrue();

        workRan = false;
        InvokeScheduledWork(scheduledWork, "ExecuteUnlessCancelled");
        await Assert.That(workRan).IsTrue();

        workRan = false;
        scheduledWork.Cancellation.Dispose();
        InvokeScheduledWork(scheduledWork, "ExecuteUnlessCancelled");
        await Assert.That(workRan).IsFalse();

        var cancelledRan = false;
        var cancelled = InvokePostOnDispatcher(scheduler, "cancelled", (_, _) =>
        {
            cancelledRan = true;
            return RxDisposable.Empty;
        });
        cancelled.Dispose();

        var flushed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Dispatcher.UIThread.Post(flushed.SetResult, DispatcherPriority.Background);
        await flushed.Task.WaitAsync(TimeSpan.FromSeconds(2));
        await Assert.That(cancelledRan).IsFalse();

        var reentrant = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        IDisposable ScheduleNext(RxScheduler _, int depth)
        {
            if (depth > 40)
            {
                reentrant.SetResult();
                return RxDisposable.Empty;
            }

            return scheduler.Schedule(depth + 1, TimeSpan.Zero, ScheduleNext);
        }

        _ = scheduler.Schedule(0, TimeSpan.Zero, ScheduleNext);
        await reentrant.Task.WaitAsync(TimeSpan.FromSeconds(2));

        var guardField = typeof(AvaloniaScheduler).GetField("_reentrancyGuard", BindingFlags.Instance | BindingFlags.NonPublic);
        guardField!.SetValue(scheduler, 32);
        try
        {
            var guarded = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            using (InvokeSchedule(scheduler, "guarded", TimeSpan.Zero, (_, _) =>
            {
                guarded.SetResult();
                return RxDisposable.Empty;
            }))
            {
                await guarded.Task.WaitAsync(TimeSpan.FromSeconds(2));
            }
        }
        finally
        {
            guardField.SetValue(scheduler, 0);
        }
    }

    /// <summary>Covers reactive scheduler paths before async assertion continuations run.</summary>
    [Test]
    public void AvaloniaScheduler_CoversReactivePathsSynchronously()
    {
        var scheduler = AvaloniaScheduler.Instance;
        var directlyPosted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var directPost = InvokePostOnDispatcher(scheduler, "direct", (_, state) =>
        {
            if (state == "direct")
            {
                directlyPosted.SetResult();
            }

            return RxDisposable.Empty;
        });

        var workRuns = 0;
        var scheduledWork = new AvaloniaScheduler.ScheduledWork<string>(scheduler, "work", (_, state) =>
        {
            if (state == "work")
            {
                workRuns++;
            }

            return RxDisposable.Empty;
        });
        InvokeScheduledWork(scheduledWork, "Execute");
        InvokeScheduledWork(scheduledWork, "ExecuteUnlessCancelled");
        scheduledWork.Cancellation.Dispose();
        InvokeScheduledWork(scheduledWork, "ExecuteUnlessCancelled");

        var guardField = typeof(AvaloniaScheduler).GetField("_reentrancyGuard", BindingFlags.Instance | BindingFlags.NonPublic);
        var guarded = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        IDisposable? guardedPost = null;
        guardField!.SetValue(scheduler, 32);
        try
        {
            guardedPost = InvokeSchedule(scheduler, "guarded", TimeSpan.Zero, (_, state) =>
            {
                if (state == "guarded")
                {
                    guarded.SetResult();
                }

                return RxDisposable.Empty;
            });
        }
        finally
        {
            guardField.SetValue(scheduler, 0);
        }

        var background = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Task.Run(() =>
        {
            _ = InvokeSchedule(scheduler, "background", TimeSpan.Zero, (_, state) =>
            {
                if (state == "background")
                {
                    background.SetResult();
                }

                return RxDisposable.Empty;
            });
        }).GetAwaiter().GetResult();

        WaitForCompletion(directlyPosted.Task);
        WaitForCompletion(guarded.Task);
        guardedPost.Dispose();
        WaitForCompletion(background.Task);
        Ensure(workRuns == 2);
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

        var control = new ReactiveUserControl<TestViewModel>
        {
            DataContext = vm
        };
        await Assert.That(control.ViewModel).IsSameReferenceAs(vm);
        control.DataContext = new();
        await Assert.That(control.ViewModel).IsSameReferenceAs(vm);
        ((IViewFor)control).ViewModel = second;
        await Assert.That(control.DataContext).IsSameReferenceAs(second);
        await Assert.That(((IViewFor)control).ViewModel).IsSameReferenceAs(second);
        ((IViewFor)control).ViewModel = null;
        await Assert.That(control.DataContext).IsNull();
        await Assert.That(() => SetInvalidViewModel(control)).ThrowsExactly<InvalidCastException>();

        var window = new ReactiveWindow<TestViewModel>
        {
            DataContext = vm
        };
        await Assert.That(window.ViewModel).IsSameReferenceAs(vm);
        window.DataContext = new();
        await Assert.That(window.ViewModel).IsSameReferenceAs(vm);
        ((IViewFor)window).ViewModel = second;
        await Assert.That(window.DataContext).IsSameReferenceAs(second);
        ((IViewFor)window).ViewModel = null;
        await Assert.That(window.DataContext).IsNull();
        await Assert.That(() => SetInvalidViewModel(window)).ThrowsExactly<InvalidCastException>();

        RuntimeHelpers.RunClassConstructor(typeof(ReactiveWindowBase).TypeHandle);
        var reflectedWindowType = typeof(ReactiveWindow<TestViewModel>);
        var reflectedWindow = (ReactiveWindow<TestViewModel>)Activator.CreateInstance(reflectedWindowType)!;
        var reflectedWindowProperty = reflectedWindowType.GetProperty(nameof(ReactiveWindow<>.ViewModel))!;
        reflectedWindowProperty.SetValue(reflectedWindow, vm);
        await Assert.That(reflectedWindowProperty.GetValue(reflectedWindow)).IsSameReferenceAs(vm);
        typeof(IViewFor).GetProperty(nameof(IViewFor.ViewModel))!.SetValue(reflectedWindow, second);
        await Assert.That(reflectedWindow.ViewModel).IsSameReferenceAs(second);

        var reflectedControlType = typeof(ReactiveUserControl<TestViewModel>);
        var reflectedControl = (ReactiveUserControl<TestViewModel>)Activator.CreateInstance(reflectedControlType)!;
        var reflectedControlProperty = reflectedControlType.GetProperty(nameof(ReactiveUserControl<>.ViewModel))!;
        reflectedControlProperty.SetValue(reflectedControl, vm);
        await Assert.That(reflectedControlProperty.GetValue(reflectedControl)).IsSameReferenceAs(vm);
        typeof(IViewFor).GetProperty(nameof(IViewFor.ViewModel))!.SetValue(reflectedControl, second);
        await Assert.That(reflectedControl.ViewModel).IsSameReferenceAs(second);

        var validViewModel = typeof(ReactiveWindowBase)
            .GetMethod("IsValidViewModelValue", BindingFlags.Instance | BindingFlags.NonPublic)!;
        await Assert.That((bool)validViewModel.Invoke(reflectedWindow, [vm])!).IsTrue();
        await Assert.That((bool)validViewModel.Invoke(reflectedWindow, [new object()])!).IsFalse();

        var directControlBase = new TestableUserControlBase
        {
            DataContext = vm
        };
        await Assert.That(directControlBase.ViewModel).IsSameReferenceAs(vm);
        directControlBase.ViewModel = second;
        await Assert.That(directControlBase.DataContext).IsSameReferenceAs(second);
        await Assert.That(directControlBase.IsValidViewModel(new object())).IsTrue();

        var baseWindow = new TestableWindowBase
        {
            DataContext = vm
        };
        await Assert.That(baseWindow.ViewModel).IsSameReferenceAs(vm);
        baseWindow.DataContext = new();
        await Assert.That(baseWindow.ViewModel).IsSameReferenceAs(baseWindow.DataContext);
        baseWindow.ViewModel = second;
        await Assert.That(baseWindow.DataContext).IsSameReferenceAs(second);

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

        await Assert.That(sut.GetAffinityForObject(typeof(TestControl), nameof(TestControl.Text))).IsEqualTo(4);
        await Assert.That(sut.GetAffinityForObject((Type?)null, nameof(TestControl.Text), beforeChanged: false)).IsEqualTo(0);
        await Assert.That(sut.GetAffinityForObject(typeof(object), "Text")).IsEqualTo(0);
        await Assert.That(sut.GetAffinityForObject(typeof(TestControl), "Missing")).IsEqualTo(0);

        IObservedChange<object?, object?>? observed = null;
        using (sut.GetNotificationForProperty(control, expression, nameof(TestControl.Text), beforeChanged: false)
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
        await Assert.That(() => sut.GetNotificationForProperty(null!, expression, "Text"))
            .ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Covers reactive view-host navigation and visual-tree subscriptions.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ViewHosts_CoverReactivePaths()
    {
        var view = new ViewB();
        var host = new TestableViewModelViewHost
        {
            DefaultContent = "default",
            ViewContract = "contract",
            ViewLocator = new StaticViewLocator(view, "contract")
        };
        var vm = new VmB();

        host.ViewModel = vm;
        await Assert.That(host.ViewModel).IsSameReferenceAs(vm);
        InvokeNavigation(host, vm, "contract");
        await Assert.That(host.Content).IsSameReferenceAs(view);
        InvokeNavigation(host, null, null);
        await Assert.That(host.Content).IsEqualTo("default");
        host.ViewLocator = new StaticViewLocator(null, "contract");
        InvokeNavigation(host, vm, "contract");
        await Assert.That(host.Content).IsEqualTo("default");
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
        await Assert.That(host.Content).IsEqualTo("default");

        var source = GetPresentationSource();
        host.Attach(source);
        host.ViewContract = "other";
        host.ViewModel = new VmB();
        host.Attach(source);
        host.Detach(source);
        SetNavigationDisposables(host, new System.Reactive.Disposables.CompositeDisposable());
        host.Attach(source);
        InvokeDisposeNavigationDisposables(host);

        var screen = new ScreenImpl();
        var routedView = new ViewA();
        var routed = new TestableRoutedViewHost
        {
            DefaultContent = "default",
            Router = screen.Router,
            ViewContract = "contract",
            ViewLocator = new StaticViewLocator(routedView, "contract")
        };
        var route = new VmA(screen);

        InvokeNavigation(routed, route, "contract");
        await Assert.That(routed.Content).IsSameReferenceAs(routedView);
        routed.Router = null;
        InvokeNavigation(routed, route, null);
        await Assert.That(routed.Content).IsEqualTo("default");
        routed.Router = screen.Router;
        InvokeNavigation(routed, null, null);
        await Assert.That(routed.Content).IsEqualTo("default");
        routed.ViewLocator = new StaticViewLocator(null, "contract");
        InvokeNavigation(routed, route, "contract");
        await Assert.That(routed.Content).IsEqualTo("default");
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
        await Assert.That(routed.Content).IsEqualTo("default");

        routed.Attach(source);
        routed.Router = null;
        routed.Attach(source);
        routed.Detach(source);
        SetNavigationDisposables(routed, new System.Reactive.Disposables.CompositeDisposable());
        routed.Attach(source);
        InvokeDisposeNavigationDisposables(routed);
    }

    /// <summary>Covers reactive view-host attach paths before async assertion continuations run.</summary>
    [Test]
    public void ViewHosts_CoverReactiveAttachPathsSynchronously()
    {
        var source = GetPresentationSource();
        var viewModelView = new ViewB();
        var viewModelHost = new TestableViewModelViewHost
        {
            DefaultContent = "default",
            ViewLocator = new StaticViewLocator(viewModelView)
        };

        viewModelHost.Attach(source);
        viewModelHost.Attach(source);
        viewModelHost.ViewContract = "contract";
        viewModelHost.ViewModel = new VmB();
        viewModelHost.Detach(source);

        var screen = new ScreenImpl();
        var routedView = new ViewA();
        var routedHost = new TestableRoutedViewHost
        {
            DefaultContent = "default",
            Router = screen.Router,
            ViewLocator = new StaticViewLocator(routedView)
        };

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
    public async Task SubscriptionErrors_CoversReactiveThrow()
    {
        await Assert.That(() => SubscriptionErrors.Throw(new InvalidOperationException("expected")))
            .ThrowsExactly<InvalidOperationException>();
    }

    /// <summary>Invokes AppBuilder platform setup callback.</summary>
    /// <param name="builder">The app builder.</param>
    private static void InvokeAfterPlatformServicesSetup(AppBuilder builder)
    {
        var property = typeof(AppBuilder).GetProperty(
            "AfterPlatformServicesSetupCallback",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var callback = (Action<AppBuilder>?)property?.GetValue(builder);
        callback?.Invoke(builder);
    }

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
                candidate.Name == "RegisterReactiveUIViews" &&
                candidate.GetParameters() is [{ ParameterType: var resolverType }, { ParameterType: var assembliesType }] &&
                resolverType == typeof(IMutableDependencyResolver) &&
                assembliesType == typeof(Assembly[]));

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
                candidate.Name == "RegisterReactiveUIViewsFromEntryAssembly" &&
                candidate.GetParameters() is [{ ParameterType: var builderType }, { ParameterType: var assemblyType }] &&
                builderType == typeof(AppBuilder) &&
                assemblyType == typeof(Assembly));

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

    /// <summary>Invokes AvaloniaScheduler.Schedule through reflection.</summary>
    /// <typeparam name="TState">The scheduled state type.</typeparam>
    /// <param name="scheduler">The scheduler.</param>
    /// <param name="state">The scheduled state.</param>
    /// <param name="dueTime">The due time.</param>
    /// <param name="action">The scheduled action.</param>
    /// <returns>The returned disposable.</returns>
    private static IDisposable InvokeSchedule<TState>(
        AvaloniaScheduler scheduler,
        TState state,
        TimeSpan dueTime,
        Func<RxScheduler, TState, IDisposable> action)
    {
        var method = typeof(AvaloniaScheduler)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Single(candidate => candidate.Name == "Schedule" && candidate.IsGenericMethodDefinition && candidate.GetParameters().Length == 3);

        return (IDisposable)method.MakeGenericMethod(typeof(TState)).Invoke(scheduler, [state, dueTime, action])!;
    }

    /// <summary>Invokes AvaloniaScheduler.PostOnDispatcher through reflection.</summary>
    /// <typeparam name="TState">The scheduled state type.</typeparam>
    /// <param name="scheduler">The scheduler.</param>
    /// <param name="state">The scheduled state.</param>
    /// <param name="action">The scheduled action.</param>
    /// <returns>The returned disposable.</returns>
    private static IDisposable InvokePostOnDispatcher<TState>(
        AvaloniaScheduler scheduler,
        TState state,
        Func<RxScheduler, TState, IDisposable> action)
    {
        var method = typeof(AvaloniaScheduler)
            .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
            .Single(candidate => candidate.Name == "PostOnDispatcher" && candidate.IsGenericMethodDefinition);

        return (IDisposable)method.MakeGenericMethod(typeof(TState)).Invoke(scheduler, [state, action])!;
    }

    /// <summary>Invokes a scheduled work method through reflection.</summary>
    /// <typeparam name="TState">The scheduled state type.</typeparam>
    /// <param name="scheduledWork">The scheduled work.</param>
    /// <param name="methodName">The method name.</param>
    private static void InvokeScheduledWork<TState>(AvaloniaScheduler.ScheduledWork<TState> scheduledWork, string methodName)
    {
        var method = typeof(AvaloniaScheduler.ScheduledWork<TState>).GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
        _ = method!.Invoke(scheduledWork, null);
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

        var baseControl = new TestableUserControlBase
        {
            DataContext = viewModel
        };
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
        var window = new ReactiveWindow<TestViewModel>
        {
            DataContext = viewModel
        };
        var windowInitial = ReferenceEquals(window.ViewModel, viewModel);
        window.DataContext = CreateObject();
        var windowIgnoresInvalidDataContext = ReferenceEquals(window.ViewModel, viewModel);
        ((IViewFor)window).ViewModel = secondViewModel;
        var windowInterfaceGet = ReferenceEquals(((IViewFor)window).ViewModel, secondViewModel);
        var windowUpdatesDataContext = ReferenceEquals(window.DataContext, secondViewModel);
        ((IViewFor)window).ViewModel = null;
        var windowClearsDataContext = window.DataContext is null;
        var windowInvalidThrows = ThrowsExactly<InvalidCastException>(() => SetInvalidViewModel(window));

        var baseWindow = new TestableWindowBase
        {
            DataContext = viewModel
        };
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

    /// <summary>Waits for a task to complete within the coverage-test timeout.</summary>
    /// <param name="task">The task to wait for.</param>
    /// <exception cref="TimeoutException">Thrown when the task does not complete in time.</exception>
    private static void WaitForCompletion(Task task)
    {
        if (task.Wait(TimeSpan.FromSeconds(2)))
        {
            return;
        }

        throw new TimeoutException("The scheduled coverage work did not complete.");
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
        return new ObservedChange<object, object>(items, member, items.Items!);
    }

    /// <summary>Creates an observed change for ItemsControl.ItemsSource.</summary>
    /// <param name="items">The items control.</param>
    /// <returns>The observed change.</returns>
    private static ObservedChange<object, object> ItemsSourceObservedChange(ItemsControl items)
    {
        var param = Expression.Parameter(typeof(ItemsControl), "x");
        var member = Expression.Property(param, nameof(ItemsControl.ItemsSource));
        return new ObservedChange<object, object>(items, member, items.ItemsSource!);
    }

    /// <summary>Creates an observed change for Control.Tag.</summary>
    /// <param name="control">The control.</param>
    /// <returns>The observed change.</returns>
    private static ObservedChange<object, object> TagObservedChange(Control control)
    {
        var param = Expression.Parameter(typeof(Control), "x");
        var member = Expression.Property(param, nameof(Control.Tag));
        return new ObservedChange<object, object>(control, member, control.Tag!);
    }

    /// <summary>Creates an observed change for TextBlock.Text.</summary>
    /// <param name="text">The text block.</param>
    /// <returns>The observed change.</returns>
    private static ObservedChange<object, object> TextObservedChange(TextBlock text)
    {
        var param = Expression.Parameter(typeof(TextBlock), "x");
        var member = Expression.Property(param, nameof(TextBlock.Text));
        return new ObservedChange<object, object>(text, member, text.Text!);
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

    /// <summary>A recording observer.</summary>
    /// <typeparam name="T">The observed type.</typeparam>
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

    /// <summary>A test control with a styled text property.</summary>
    private sealed class TestControl : Control
    {
        /// <summary>The text property.</summary>
        public static readonly StyledProperty<string?> TextProperty =
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

    /// <summary>A view contract attribute for registration tests.</summary>
    /// <param name="contract">The contract.</param>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    private sealed class ViewContractAttribute(string contract) : Attribute
    {
        /// <summary>Gets the contract.</summary>
        public string Contract { get; } = contract;
    }

    /// <summary>A static view locator for host tests.</summary>
    private sealed class StaticViewLocator : IViewLocator
    {
        /// <summary>The view to return.</summary>
        private readonly IViewFor? _view;

        /// <summary>The matching contract.</summary>
        private readonly string? _contract;

        /// <summary>Initializes a new instance of the <see cref="StaticViewLocator"/> class.</summary>
        /// <param name="view">The view to return.</param>
        /// <param name="contract">The matching contract.</param>
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
            OnAttachedToVisualTree(new VisualTreeAttachmentEventArgs(this, source));

        /// <summary>Raises detached-from-visual-tree.</summary>
        /// <param name="source">The presentation source.</param>
        public void Detach(IPresentationSource source) =>
            OnDetachedFromVisualTree(new VisualTreeAttachmentEventArgs(this, source));
    }

    /// <summary>A testable RoutedViewHost.</summary>
    private sealed class TestableRoutedViewHost : RoutedViewHost
    {
        /// <summary>Raises attached-to-visual-tree.</summary>
        /// <param name="source">The presentation source.</param>
        public void Attach(IPresentationSource source) =>
            OnAttachedToVisualTree(new VisualTreeAttachmentEventArgs(this, source));

        /// <summary>Raises detached-from-visual-tree.</summary>
        /// <param name="source">The presentation source.</param>
        public void Detach(IPresentationSource source) =>
            OnDetachedFromVisualTree(new VisualTreeAttachmentEventArgs(this, source));
    }

    /// <summary>A resolver that throws during direct concrete lookup.</summary>
    private class ThrowingResolver : IDependencyResolver
    {
        /// <inheritdoc/>
        public void Dispose()
        {
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
            RxDisposable.Empty;

        /// <inheritdoc/>
        public IDisposable ServiceRegistrationCallback(Type serviceType, string? contract, Action<IDisposable> callback) =>
            RxDisposable.Empty;

        /// <inheritdoc/>
        public IDisposable ServiceRegistrationCallback<T>(Action<IDisposable> callback) =>
            RxDisposable.Empty;

        /// <inheritdoc/>
        public IDisposable ServiceRegistrationCallback<T>(string? contract, Action<IDisposable> callback) =>
            RxDisposable.Empty;

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

    /// <summary>A resolver that returns a single constant value for matching concrete types.</summary>
    private sealed class ConstantResolver : ThrowingResolver
    {
        /// <summary>The service value.</summary>
        private readonly object _value;

        /// <summary>Initializes a new instance of the <see cref="ConstantResolver"/> class.</summary>
        /// <param name="value">The service value.</param>
        public ConstantResolver(object value)
        {
            _value = value;
        }

        /// <inheritdoc/>
        public override object? GetService(Type? serviceType) =>
            serviceType == _value.GetType() ? _value : null;

        /// <inheritdoc/>
        public override object? GetService(Type? serviceType, string? contract) =>
            serviceType == _value.GetType() && contract is null ? _value : null;
    }
}
