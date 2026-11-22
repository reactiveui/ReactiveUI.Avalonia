// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.VisualTree;
using Splat;

namespace ReactiveUI.Avalonia.Tests;

/// <summary>Tests for LayoutViewHost layout resolution, reuse and release.</summary>
public class LayoutViewHostTests
{
    /// <summary>The default content used by the hosts under test.</summary>
    private const string DefaultContentValue = "default";

    /// <summary>The name of the border the chrome layout draws around its content.</summary>
    private const string ChromeName = "Chrome";

    /// <summary>Verifies that a view naming a layout view model is wrapped in that layout's view.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Wraps_View_In_Layout_Resolved_From_Layout_ViewModel()
    {
        var host = CreateHost();
        var vm = new LoginVm();

        host.NavigateToViewModel(vm, null);

        var layout = await Assert.That(host.Content).IsTypeOf<AuthLayoutView>();
        await Assert.That(layout!.ViewModel).IsNotNull();
        await Assert.That(layout.DataContext).IsSameReferenceAs(layout.ViewModel);
        var view = await Assert.That(layout.Content).IsTypeOf<LoginView>();
        await Assert.That(view!.ViewModel).IsSameReferenceAs(vm);
    }

    /// <summary>Verifies that consecutive views naming the same layout share one layout instance.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Reuses_Layout_For_Consecutive_Views_With_Same_Layout()
    {
        var host = CreateHost();

        host.NavigateToViewModel(new LoginVm(), null);
        var layout = host.Content;
        host.NavigateToViewModel(new RegisterVm(), null);

        await Assert.That(host.Content).IsSameReferenceAs(layout);
        await Assert.That(((ContentControl)host.Content!).Content).IsTypeOf<RegisterView>();
    }

    /// <summary>Verifies that a view naming a different layout gets a new layout.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Creates_New_Layout_When_Layout_Type_Changes()
    {
        var host = CreateHost();

        host.NavigateToViewModel(new LoginVm(), null);
        host.NavigateToViewModel(new SettingsVm(), null);

        var layout = await Assert.That(host.Content).IsTypeOf<PlainLayout>();
        await Assert.That(layout!.Content).IsTypeOf<SettingsView>();
    }

    /// <summary>Verifies that a view without a layout is shown directly and the previous layout is not kept.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Releases_Layout_When_View_Has_No_Layout()
    {
        var host = CreateHost();

        host.NavigateToViewModel(new LoginVm(), null);
        var firstLayout = host.Content;
        host.NavigateToViewModel(new HomeVm(), null);

        await Assert.That(host.Content).IsTypeOf<HomeView>();

        host.NavigateToViewModel(new LoginVm(), null);

        await Assert.That(host.Content).IsTypeOf<AuthLayoutView>();
        await Assert.That(host.Content).IsNotSameReferenceAs(firstLayout);
    }

    /// <summary>Verifies that the default content replaces the layout when there is no view model.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Shows_Default_Content_And_Releases_Layout_When_ViewModel_Null()
    {
        var host = CreateHost();

        host.NavigateToViewModel(new LoginVm(), null);
        var firstLayout = host.Content;
        host.NavigateToViewModel(null, null);

        await Assert.That(host.Content).IsEqualTo(DefaultContentValue);

        host.NavigateToViewModel(new LoginVm(), null);

        await Assert.That(host.Content).IsNotSameReferenceAs(firstLayout);
    }

    /// <summary>Verifies that a layout declared on the view model is used when the view declares none.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Uses_Layout_Declared_On_ViewModel()
    {
        var host = CreateHost();

        host.NavigateToViewModel(new ProfileVm(), null);

        var layout = await Assert.That(host.Content).IsTypeOf<PlainLayout>();
        await Assert.That(layout!.Content).IsTypeOf<ProfileView>();
    }

    /// <summary>Verifies that a layout declared on the view wins over one declared on the view model.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task View_Layout_Takes_Precedence_Over_ViewModel_Layout()
    {
        var host = CreateHost();

        host.NavigateToViewModel(new SettingsVm(), null);

        await Assert.That(host.Content).IsTypeOf<PlainLayout>();
    }

    /// <summary>Verifies that a layout registered with the dependency resolver is used instead of a new instance.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Uses_Layout_Registered_With_Dependency_Resolver()
    {
        var registered = new AuthLayoutVm();
        AppLocator.CurrentMutable.RegisterConstant(registered);
        var host = CreateHost();

        host.NavigateToViewModel(new LoginVm(), null);

        var layout = await Assert.That(host.Content).IsTypeOf<AuthLayoutView>();
        await Assert.That(layout!.ViewModel).IsSameReferenceAs(registered);
    }

    /// <summary>Verifies that the view is shown directly when its layout does not lead to a content control.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Shows_View_Without_Layout_When_Layout_Is_Not_A_ContentControl()
    {
        var host = CreateHost();

        host.NavigateToViewModel(new BrokenVm(), null);

        await Assert.That(host.Content).IsTypeOf<BrokenView>();
    }

    /// <summary>Verifies that one view instance can move between layouts and the bare host while attached.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Moves_Reused_View_Instance_Between_Layouts_While_Attached()
    {
        var view = new SharedView();
        var locator = new MappingViewLocator()
            .Map<AuthLayoutVm>(static () => new AuthLayoutView())
            .Map<SharedVm>(() => view)
            .Map<SharedAuthVm>(() => view)
            .Map<SharedPlainVm>(() => view);
        var host = new LayoutViewHost { DefaultContent = DefaultContentValue, Router = new(), ViewLocator = locator };
        var window = new Window { Content = host };

        try
        {
            window.Show();

            host.NavigateToViewModel(new SharedAuthVm(), null);
            await Assert.That(((ContentControl)host.Content!).Content).IsSameReferenceAs(view);

            host.NavigateToViewModel(new SharedPlainVm(), null);
            var plain = await Assert.That(host.Content).IsTypeOf<PlainLayout>();
            await Assert.That(plain!.Content).IsSameReferenceAs(view);

            host.NavigateToViewModel(new SharedVm(), null);
            await Assert.That(host.Content).IsSameReferenceAs(view);
            await Assert.That(plain.Content).IsNull();

            host.NavigateToViewModel(new SharedAuthVm(), null);
            var auth = await Assert.That(host.Content).IsTypeOf<AuthLayoutView>();
            await Assert.That(auth!.Content).IsSameReferenceAs(view);
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>Verifies that the host wraps views that arrive through router navigation.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Wraps_View_On_Router_Navigation()
    {
        var screen = new ScreenImpl();
        var host = CreateHost();
        host.Router = screen.Router;
        var window = new Window { Content = host };

        try
        {
            window.Show();
            _ = screen.Router.Navigate.Execute(new LoginVm { HostScreen = screen }).Subscribe();

            var layout = await Assert.That(host.Content).IsTypeOf<AuthLayoutView>();
            await Assert.That(layout!.Content).IsTypeOf<LoginView>();
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>Verifies that a layout's content template draws its chrome around the routed view.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Layout_Content_Template_Draws_Chrome_Around_View()
    {
        var view = new ChromeView();
        var host = new LayoutViewHost { Router = new(), ViewLocator = new MappingViewLocator().Map<ChromeVm>(() => view) };
        var window = new Window { Content = host };

        try
        {
            window.Show();
            host.NavigateToViewModel(new ChromeVm(), null);
            window.UpdateLayout();

            var chrome = view.FindAncestorOfType<Border>();
            await Assert.That(chrome?.Name).IsEqualTo(ChromeName);
            await Assert.That(chrome.FindAncestorOfType<ChromeLayout>()).IsSameReferenceAs(host.Content);
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>Verifies that the attribute exposes its layout type and rejects a null type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ViewLayoutAttribute_Exposes_LayoutType_And_Rejects_Null()
    {
        var attribute = new ViewLayoutAttribute(typeof(PlainLayout));

        await Assert.That(attribute.LayoutType).IsEqualTo(typeof(PlainLayout));
        await Assert.That(static () => new ViewLayoutAttribute(null!)).Throws<ArgumentNullException>();
    }

    /// <summary>Creates a host whose locator knows every test view.</summary>
    /// <returns>The host under test.</returns>
    private static LayoutViewHost CreateHost()
    {
        var locator = new MappingViewLocator()
            .Map<AuthLayoutVm>(static () => new AuthLayoutView())
            .Map<BrokenLayoutVm>(static () => new BrokenLayoutView())
            .Map<LoginVm>(static () => new LoginView())
            .Map<RegisterVm>(static () => new RegisterView())
            .Map<SettingsVm>(static () => new SettingsView())
            .Map<HomeVm>(static () => new HomeView())
            .Map<ProfileVm>(static () => new ProfileView())
            .Map<BrokenVm>(static () => new BrokenView());

        return new() { DefaultContent = DefaultContentValue, Router = new(), ViewLocator = locator };
    }

    /// <summary>A layout that the host creates directly and that has no view model.</summary>
    /// <remarks>Public because the host creates it through its parameterless constructor, not from test code.</remarks>
    public sealed class PlainLayout : ContentControl;

    /// <summary>The view model of a layout whose view cannot hold content.</summary>
    /// <remarks>Public because the host creates it through its parameterless constructor, not from test code.</remarks>
    public sealed class BrokenLayoutVm : ReactiveObject;

    /// <summary>A layout that draws a named border around its content.</summary>
    /// <remarks>Public because the host creates it through its parameterless constructor, not from test code.</remarks>
    public sealed class ChromeLayout : ContentControl
    {
        /// <summary>Initializes a new instance of the <see cref="ChromeLayout"/> class.</summary>
        public ChromeLayout() =>
            ContentTemplate = new FuncDataTemplate<object>(
                static (content, _) => new Border { Name = ChromeName, Child = new ContentPresenter { Content = content } });
    }

    /// <summary>A view locator that maps view model types to view factories.</summary>
    private sealed class MappingViewLocator : IViewLocator
    {
        /// <summary>The view factories keyed by view model type.</summary>
        private readonly Dictionary<Type, Func<IViewFor>> _factories = [];

        /// <summary>Maps a view model type to a view factory.</summary>
        /// <typeparam name="TViewModel">The view model type.</typeparam>
        /// <param name="factory">Creates the view.</param>
        /// <returns>This locator.</returns>
        public MappingViewLocator Map<TViewModel>(Func<IViewFor> factory)
        {
            _factories[typeof(TViewModel)] = factory;
            return this;
        }

        /// <inheritdoc/>
        IViewFor? IViewLocator.ResolveView<TViewModel>(TViewModel viewModel, string? contract) =>
            Resolve(viewModel);

        /// <inheritdoc/>
        IViewFor? IViewLocator.ResolveView(object? viewModel, string? contract) =>
            Resolve(viewModel);

        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.RequiresDynamicCode("Test locator implementation mirrors the runtime-type service lookup.")]
        IViewFor? IViewLocator.ResolveViewUnsafe(object? viewModel, string? contract) =>
            ((IViewLocator)this).ResolveView(viewModel, contract);

        /// <summary>Creates the view mapped to a view model's type.</summary>
        /// <param name="viewModel">The view model.</param>
        /// <returns>The view, or null when the type is not mapped.</returns>
        private IViewFor? Resolve(object? viewModel) =>
            viewModel is not null && _factories.TryGetValue(viewModel.GetType(), out var factory) ? factory() : null;
    }

    /// <summary>A view base that stores an untyped view model.</summary>
    private class TestView : UserControl, IViewFor
    {
        /// <inheritdoc/>
        public object? ViewModel { get; set; }
    }

    /// <summary>A routable view model base.</summary>
    private class TestVm : ReactiveObject, IRoutableViewModel
    {
        /// <inheritdoc/>
        public string? UrlPathSegment => GetType().Name;

        /// <inheritdoc/>
        public IScreen HostScreen { get; init; } = new ScreenImpl();
    }

    /// <summary>The view model of the authentication layout.</summary>
    private sealed class AuthLayoutVm : ReactiveObject;

    /// <summary>The authentication layout view, resolved through the view locator.</summary>
    private sealed class AuthLayoutView : TestView;

    /// <summary>A layout view that is not a content control.</summary>
    private sealed class BrokenLayoutView : Control, IViewFor
    {
        /// <inheritdoc/>
        public object? ViewModel { get; set; }
    }

    /// <summary>A view model whose view uses the authentication layout.</summary>
    private sealed class LoginVm : TestVm;

    /// <summary>A view that names its layout by layout view model type.</summary>
    [ViewLayout(typeof(AuthLayoutVm))]
    private sealed class LoginView : TestView;

    /// <summary>A second view model whose view uses the authentication layout.</summary>
    private sealed class RegisterVm : TestVm;

    /// <summary>A second view that names the authentication layout.</summary>
    [ViewLayout(typeof(AuthLayoutVm))]
    private sealed class RegisterView : TestView;

    /// <summary>A view model that names a layout its view overrides.</summary>
    [ViewLayout(typeof(AuthLayoutVm))]
    private sealed class SettingsVm : TestVm;

    /// <summary>A view that names its layout by layout view type.</summary>
    [ViewLayout(typeof(PlainLayout))]
    private sealed class SettingsView : TestView;

    /// <summary>A view model without a layout.</summary>
    private sealed class HomeVm : TestVm;

    /// <summary>A view without a layout.</summary>
    private sealed class HomeView : TestView;

    /// <summary>A view model that declares the layout for its view.</summary>
    [ViewLayout(typeof(PlainLayout))]
    private sealed class ProfileVm : TestVm;

    /// <summary>A view that leaves the layout to its view model.</summary>
    private sealed class ProfileView : TestView;

    /// <summary>A view model whose view names an unusable layout.</summary>
    private sealed class BrokenVm : TestVm;

    /// <summary>A view that names a layout whose view is not a content control.</summary>
    [ViewLayout(typeof(BrokenLayoutVm))]
    private sealed class BrokenView : TestView;

    /// <summary>A view model shown without a layout by the shared view.</summary>
    private sealed class SharedVm : TestVm;

    /// <summary>A view model that puts the shared view in the authentication layout.</summary>
    [ViewLayout(typeof(AuthLayoutVm))]
    private sealed class SharedAuthVm : TestVm;

    /// <summary>A view model that puts the shared view in the plain layout.</summary>
    [ViewLayout(typeof(PlainLayout))]
    private sealed class SharedPlainVm : TestVm;

    /// <summary>A view model whose view sits inside the chrome layout.</summary>
    private sealed class ChromeVm : TestVm;

    /// <summary>A view that names the chrome layout.</summary>
    [ViewLayout(typeof(ChromeLayout))]
    private sealed class ChromeView : TestView;

    /// <summary>A view whose single instance serves several view models.</summary>
    private sealed class SharedView : TestView;

    /// <summary>A screen implementation for testing routing.</summary>
    private sealed class ScreenImpl : ReactiveObject, IScreen
    {
        /// <summary>Gets the routing state.</summary>
        public RoutingState Router { get; } = new();
    }
}
