// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Avalonia.Controls;
using Avalonia.Rendering;
using Splat;

namespace ReactiveUI.Avalonia.Tests;

/// <summary>Tests for ViewModelViewHost and RoutedViewHost navigation behavior.</summary>
public class ViewHostsNavigationTests
{
    /// <summary>The default content used by ViewModelViewHost tests.</summary>
    private const string DefaultContentValue = "default";

    /// <summary>The view contract used by navigation tests.</summary>
    private const string ViewContractValue = "contract";

    /// <summary>Verifies that ViewModelViewHost navigates to a resolved view and sets its ViewModel.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ViewModelViewHost_Navigates_To_Resolved_View()
    {
        RegisterViews();
        var host = new ViewModelViewHost { DefaultContent = DefaultContentValue };
        var vm = new VmB();

        host.NavigateToViewModel(vm, null);

        await Assert.That(host.Content).IsTypeOf<ViewB>();
        await Assert.That(((IViewFor)host.Content!).ViewModel).IsSameReferenceAs(vm);
    }

    /// <summary>Verifies that ViewModelViewHost exposes Avalonia property values and style key.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ViewModelViewHost_Properties_Return_Assigned_Values()
    {
        var locator = new StaticViewLocator(new ViewB());
        var host = new TestableViewModelViewHost { DefaultContent = DefaultContentValue, ViewContract = ViewContractValue, ViewLocator = locator };
        var vm = new VmB();

        host.ViewModel = vm;

        await Assert.That(host.ViewModel).IsSameReferenceAs(vm);
        await Assert.That(host.DefaultContent).IsEqualTo(DefaultContentValue);
        await Assert.That(host.ViewContract).IsEqualTo(ViewContractValue);
        await Assert.That(host.ViewLocator).IsSameReferenceAs(locator);
        await Assert.That(host.ExposedStyleKey).IsEqualTo(typeof(TransitioningContentControl));
    }

    /// <summary>Verifies that ViewModelViewHost lifecycle hooks create and dispose navigation subscriptions.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ViewModelViewHost_AttachAndDetach_ManageNavigationSubscriptions()
    {
        var host = new TestableViewModelViewHost { DefaultContent = DefaultContentValue };
        IPresentationSource? source = null;
        host.AttachedToVisualTree += (_, args) => source = args.PresentationSource;
        var window = new Window { Content = host };

        try
        {
            window.Show();
            await Assert.That(source).IsNotNull();

            host.Attach(source!);
            window.Content = null;

            await Assert.That(host.Content).IsEqualTo(DefaultContentValue);
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>Verifies that ViewModelViewHost navigates through its visual-tree subscription.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ViewModelViewHost_AttachedSubscription_Navigates_OnViewModelChange()
    {
        var view = new ViewB();
        var host = new TestableViewModelViewHost { DefaultContent = DefaultContentValue, ViewLocator = new StaticViewLocator(view) };
        var window = new Window { Content = host };
        var vm = new VmB();

        try
        {
            window.Show();
            host.ViewModel = vm;

            await Assert.That(host.Content).IsSameReferenceAs(view);
            await Assert.That(view.ViewModel).IsSameReferenceAs(vm);
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>Verifies that ViewModelViewHost can detach before navigation subscriptions are created.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ViewModelViewHost_DetachBeforeAttach_DoesNotThrow()
    {
        var source = GetPresentationSource();
        var host = new TestableViewModelViewHost { DefaultContent = DefaultContentValue };

        host.Detach(source);

        await Assert.That(host.Content).IsNull();
    }

    /// <summary>Verifies that ViewModelViewHost manual detach disposes an existing navigation subscription and tolerates a second detach.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ViewModelViewHost_AttachThenManualDetach_DisposesNavigationSubscription()
    {
        var source = GetPresentationSource();
        var host = new TestableViewModelViewHost { DefaultContent = DefaultContentValue };

        host.Attach(source);
        host.Detach(source);
        host.Detach(source);

        await Assert.That(host.Content).IsEqualTo(DefaultContentValue);
    }

    /// <summary>Verifies that ViewModelViewHost tolerates disposal when no navigation subscription exists.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ViewModelViewHost_DisposeNavigationDisposables_WhenEmpty_Returns()
    {
        var host = new ViewModelViewHost();

        host.DisposeNavigationDisposables();

        await Assert.That(host.Content).IsNull();
    }

    /// <summary>Verifies that ViewModelViewHost falls back to default content when no view is found.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ViewModelViewHost_When_No_View_Falls_Back_To_Default()
    {
        var host = new ViewModelViewHost { DefaultContent = DefaultContentValue };

        host.NavigateToViewModel(new(), null);

        await Assert.That(host.Content).IsEqualTo(DefaultContentValue);
    }

    /// <summary>Verifies that ViewModelViewHost falls back to default content when the view model is null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ViewModelViewHost_When_ViewModel_Null_Falls_Back_To_Default()
    {
        var host = new ViewModelViewHost { DefaultContent = DefaultContentValue };

        host.NavigateToViewModel(null, null);

        await Assert.That(host.Content).IsEqualTo(DefaultContentValue);
    }

    /// <summary>Verifies that ViewModelViewHost uses contracts when resolving a view.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ViewModelViewHost_Navigates_To_Contract_View()
    {
        var view = new ViewB();
        var host = new ViewModelViewHost { DefaultContent = DefaultContentValue, ViewLocator = new StaticViewLocator(view, ViewContractValue) };
        var vm = new VmB();

        host.NavigateToViewModel(vm, ViewContractValue);

        await Assert.That(host.Content).IsSameReferenceAs(view);
        await Assert.That(view.ViewModel).IsSameReferenceAs(vm);
        await Assert.That(view.DataContext).IsSameReferenceAs(vm);
    }

    /// <summary>Verifies that ViewModelViewHost logs the contract-specific missing view path.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ViewModelViewHost_When_Contract_View_Missing_Falls_Back_To_Default()
    {
        var host = new ViewModelViewHost { DefaultContent = DefaultContentValue, ViewLocator = new StaticViewLocator(null, "different") };

        host.NavigateToViewModel(new VmB(), ViewContractValue);

        await Assert.That(host.Content).IsEqualTo(DefaultContentValue);
    }

    /// <summary>Verifies that RoutedViewHost navigates to a resolved view when a Router is set.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RoutedViewHost_Navigates_To_Resolved_View_When_Router_Set()
    {
        RegisterViews();
        var screen = new ScreenImpl();
        var host = new RoutedViewHost { DefaultContent = "def", Router = screen.Router };
        var vm = new VmA(screen);

        host.NavigateToViewModel(vm, null);

        await Assert.That(host.Content).IsTypeOf<ViewA>();
        await Assert.That(((IViewFor)host.Content!).ViewModel).IsSameReferenceAs(vm);
    }

    /// <summary>Verifies that RoutedViewHost exposes Avalonia property values and style key.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RoutedViewHost_Properties_Return_Assigned_Values()
    {
        var router = new RoutingState();
        var locator = new StaticViewLocator(new ViewA());
        var host = new TestableRoutedViewHost { DefaultContent = "def", Router = router, ViewContract = ViewContractValue, ViewLocator = locator };

        await Assert.That(host.Router).IsSameReferenceAs(router);
        await Assert.That(host.DefaultContent).IsEqualTo("def");
        await Assert.That(host.ViewContract).IsEqualTo(ViewContractValue);
        await Assert.That(host.ViewLocator).IsSameReferenceAs(locator);
        await Assert.That(host.ExposedStyleKey).IsEqualTo(typeof(TransitioningContentControl));
    }

    /// <summary>Verifies that RoutedViewHost lifecycle hooks create and dispose navigation subscriptions.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RoutedViewHost_AttachAndDetach_ManageNavigationSubscriptions()
    {
        var host = new TestableRoutedViewHost { DefaultContent = "def" };
        IPresentationSource? source = null;
        host.AttachedToVisualTree += (_, args) => source = args.PresentationSource;
        var window = new Window { Content = host };

        try
        {
            window.Show();
            await Assert.That(source).IsNotNull();

            host.Attach(source!);
            window.Content = null;

            await Assert.That(host.Content).IsEqualTo("def");
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>Verifies that RoutedViewHost navigates through its visual-tree subscription.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RoutedViewHost_AttachedSubscription_Navigates_OnRouterNavigation()
    {
        RegisterViews();
        var screen = new ScreenImpl();
        var host = new TestableRoutedViewHost { DefaultContent = "def", Router = screen.Router };
        var window = new Window { Content = host };
        var vm = new VmA(screen);

        try
        {
            window.Show();
            _ = screen.Router.Navigate.Execute(vm).Subscribe();

            await Assert.That(host.Content).IsTypeOf<ViewA>();
            await Assert.That(((IViewFor)host.Content!).ViewModel).IsSameReferenceAs(vm);
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>Verifies that RoutedViewHost falls back when the router is removed after attachment.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RoutedViewHost_AttachedSubscription_FallsBack_WhenRouterRemoved()
    {
        var screen = new ScreenImpl();
        var host = new TestableRoutedViewHost { DefaultContent = "def", Router = screen.Router };
        var window = new Window { Content = host };

        try
        {
            window.Show();
            host.Router = null;

            await Assert.That(host.Content).IsEqualTo("def");
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>Verifies that RoutedViewHost can detach before navigation subscriptions are created.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RoutedViewHost_DetachBeforeAttach_DoesNotThrow()
    {
        var source = GetPresentationSource();
        var host = new TestableRoutedViewHost { DefaultContent = "def" };

        host.Detach(source);

        await Assert.That(host.Content).IsNull();
    }

    /// <summary>Verifies that RoutedViewHost manual detach disposes an existing navigation subscription and tolerates a second detach.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RoutedViewHost_AttachThenManualDetach_DisposesNavigationSubscription()
    {
        var source = GetPresentationSource();
        var host = new TestableRoutedViewHost { DefaultContent = "def", Router = new() };

        host.Attach(source);
        host.Detach(source);
        host.Detach(source);

        await Assert.That(host.Content).IsEqualTo("def");
    }

    /// <summary>Verifies that RoutedViewHost tolerates disposal when no navigation subscription exists.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RoutedViewHost_DisposeNavigationDisposables_WhenEmpty_Returns()
    {
        var host = new RoutedViewHost();

        host.DisposeNavigationDisposables();

        await Assert.That(host.Content).IsNull();
    }

    /// <summary>Verifies that RoutedViewHost falls back to default content when no Router is set.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RoutedViewHost_No_Router_Falls_Back_To_Default()
    {
        var host = new RoutedViewHost { DefaultContent = "def" };

        host.NavigateToViewModel(new(), null);

        await Assert.That(host.Content).IsEqualTo("def");
    }

    /// <summary>Verifies that RoutedViewHost falls back to default content when the current view model is null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RoutedViewHost_When_ViewModel_Null_Falls_Back_To_Default()
    {
        var host = new RoutedViewHost { DefaultContent = "def", Router = new() };

        host.NavigateToViewModel(null, null);

        await Assert.That(host.Content).IsEqualTo("def");
    }

    /// <summary>Verifies that RoutedViewHost uses contracts when resolving a view.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RoutedViewHost_Navigates_To_Contract_View()
    {
        var view = new ViewA();
        var screen = new ScreenImpl();
        var host = new RoutedViewHost { DefaultContent = "def", Router = screen.Router, ViewLocator = new StaticViewLocator(view, ViewContractValue) };
        var vm = new VmA(screen);

        host.NavigateToViewModel(vm, ViewContractValue);

        await Assert.That(host.Content).IsSameReferenceAs(view);
        await Assert.That(view.ViewModel).IsSameReferenceAs(vm);
        await Assert.That(view.DataContext).IsSameReferenceAs(vm);
    }

    /// <summary>Verifies that RoutedViewHost logs the default-contract missing view path when no view is found.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RoutedViewHost_When_Default_View_Missing_Falls_Back_To_Default()
    {
        var host = new RoutedViewHost { DefaultContent = "def", Router = new(), ViewLocator = new StaticViewLocator(null) };

        host.NavigateToViewModel(new(), null);

        await Assert.That(host.Content).IsEqualTo("def");
    }

    /// <summary>Verifies that RoutedViewHost logs the contract-specific missing view path.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RoutedViewHost_When_Contract_View_Missing_Falls_Back_To_Default()
    {
        var host = new RoutedViewHost { DefaultContent = "def", Router = new(), ViewLocator = new StaticViewLocator(null, "different") };

        host.NavigateToViewModel(new(), ViewContractValue);

        await Assert.That(host.Content).IsEqualTo("def");
    }

    /// <summary>Registers test views into the locator.</summary>
    private static void RegisterViews()
    {
        Locator.CurrentMutable.Register(static () => new ViewA(), typeof(IViewFor<VmA>));
        Locator.CurrentMutable.Register(static () => new ViewB(), typeof(IViewFor<VmB>));
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

    /// <summary>A testable ViewModelViewHost that exposes protected members.</summary>
    private sealed class TestableViewModelViewHost : ViewModelViewHost
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

    /// <summary>A testable RoutedViewHost that exposes protected members.</summary>
    private sealed class TestableRoutedViewHost : RoutedViewHost
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
    private sealed class VmA : ReactiveObject, IRoutableViewModel
    {
        /// <summary>Initializes a new instance of the <see cref="VmA"/> class.</summary>
        /// <param name="screen">The host screen.</param>
        public VmA(IScreen screen)
        {
            HostScreen = screen;
        }

        /// <summary>Gets the URL path segment.</summary>
        public string? UrlPathSegment => "A";

        /// <summary>Gets the host screen.</summary>
        public IScreen HostScreen { get; }
    }

    /// <summary>A simple view model for testing non-routable navigation.</summary>
    private sealed class VmB : ReactiveObject;

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
    private sealed class ScreenImpl : ReactiveObject, IScreen
    {
        /// <summary>Gets the routing state.</summary>
        public RoutingState Router { get; } = new();
    }
}
