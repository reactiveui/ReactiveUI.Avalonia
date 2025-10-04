using System.Reflection;
using Avalonia.Controls;
using NUnit.Framework;
using ReactiveUI;
using Splat;

namespace ReactiveUI.Avalonia.Tests
{
    public class ViewHostsNavigationTests
    {
        [Test]
        public void ViewModelViewHost_Navigates_To_Resolved_View()
        {
            var host = new ViewModelViewHost { DefaultContent = "default" };
            var vm = new VmB();

            var m = typeof(ViewModelViewHost).GetMethod("NavigateToViewModel", BindingFlags.Instance | BindingFlags.NonPublic);
            m!.Invoke(host, new object?[] { vm, null });

            Assert.That(host.Content, Is.InstanceOf<ViewB>());
            Assert.That(((IViewFor)host.Content!).ViewModel, Is.SameAs(vm));
        }

        [Test]
        public void ViewModelViewHost_When_No_View_Falls_Back_To_Default()
        {
            var host = new ViewModelViewHost { DefaultContent = "default" };

            var m = typeof(ViewModelViewHost).GetMethod("NavigateToViewModel", BindingFlags.Instance | BindingFlags.NonPublic);
            m!.Invoke(host, new object?[] { new object(), null });

            Assert.That(host.Content, Is.EqualTo("default"));
        }

        [Test]
        public void RoutedViewHost_Navigates_To_Resolved_View_When_Router_Set()
        {
            var screen = new ScreenImpl();
            var host = new RoutedViewHost { DefaultContent = "def", Router = screen.Router };
            var vm = new VmA(screen);

            var m = typeof(RoutedViewHost).GetMethod("NavigateToViewModel", BindingFlags.Instance | BindingFlags.NonPublic);
            m!.Invoke(host, new object?[] { vm, null });

            Assert.That(host.Content, Is.InstanceOf<ViewA>());
            Assert.That(((IViewFor)host.Content!).ViewModel, Is.SameAs(vm));
        }

        [Test]
        public void RoutedViewHost_No_Router_Falls_Back_To_Default()
        {
            var host = new RoutedViewHost { DefaultContent = "def" };

            var m = typeof(RoutedViewHost).GetMethod("NavigateToViewModel", BindingFlags.Instance | BindingFlags.NonPublic);
            m!.Invoke(host, new object?[] { new object(), null });

            Assert.That(host.Content, Is.EqualTo("def"));
        }

        [SetUp]
        public void Setup()
        {
            Locator.CurrentMutable.Register(() => new ViewA(), typeof(IViewFor<VmA>));
            Locator.CurrentMutable.Register(() => new ViewB(), typeof(IViewFor<VmB>));
        }

        private sealed class VmA : ReactiveObject, IRoutableViewModel
        {
            public VmA(IScreen screen)
            {
                HostScreen = screen;
            }

            public string? UrlPathSegment => "A";

            public IScreen HostScreen { get; }
        }

        private sealed class VmB : ReactiveObject
        {
        }

        private sealed class ViewA : UserControl, IViewFor<VmA>
        {
            public VmA? ViewModel { get; set; }

            object? IViewFor.ViewModel
            {
                get => ViewModel;
                set => ViewModel = (VmA?)value;
            }

            public override string ToString() => nameof(ViewA);
        }

        private sealed class ViewB : UserControl, IViewFor<VmB>
        {
            public VmB? ViewModel { get; set; }

            object? IViewFor.ViewModel
            {
                get => ViewModel;
                set => ViewModel = (VmB?)value;
            }

            public override string ToString() => nameof(ViewB);
        }

        private sealed class ScreenImpl : ReactiveObject, IScreen
        {
            public RoutingState Router { get; } = new();
        }
    }
}
