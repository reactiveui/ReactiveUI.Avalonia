using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using NUnit.Framework;
using Splat;

namespace ReactiveUI.Avalonia.Tests
{
    public class AppBuilderExtensionsRegistrationTests
    {
        private interface ITestVm
        {
        }

        [SetUp]
        public void ResetLocator()
        {
            Assert.That(AppLocator.CurrentMutable, Is.Not.Null);

            _ = new TestVm();
            _ = new TestView();
            _ = new ContractedTestView();

            var serviceType = typeof(IViewFor<>).MakeGenericType(typeof(TestVm));
            AppLocator.CurrentMutable.UnregisterCurrent(serviceType, null);
            AppLocator.CurrentMutable.UnregisterCurrent(serviceType, "C1");
        }

        [Test]
        public void RegisterViewsInternal_Registers_View_For_ViewModel_Using_Activator()
        {
            var resolver = AppLocator.CurrentMutable!;
            var method = typeof(AppBuilderExtensions)
                .GetMethod("RegisterViewsInternal", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(method, Is.Not.Null);

            method!.Invoke(null, [resolver, new[] { typeof(AppBuilderExtensionsRegistrationTests).Assembly }]);

            var serviceType = typeof(IViewFor<>).MakeGenericType(typeof(TestVm));
            var resolved = AppLocator.Current.GetService(serviceType);

            Assert.That(resolved, Is.Not.Null);
            Assert.That(serviceType.IsInstanceOfType(resolved), Is.True);
        }

        [Test]
        public void RegisterViewsInternal_Honors_ViewContractAttribute()
        {
            var resolver = AppLocator.CurrentMutable!;
            var method = typeof(AppBuilderExtensions)
                .GetMethod("RegisterViewsInternal", BindingFlags.NonPublic | BindingFlags.Static);

            method!.Invoke(null, [resolver, new[] { typeof(AppBuilderExtensionsRegistrationTests).Assembly }]);

            var serviceType = typeof(IViewFor<>).MakeGenericType(typeof(TestVm));
            var resolvedDefault = AppLocator.Current.GetService(serviceType);
            var resolvedC1 = AppLocator.Current.GetService(serviceType, "C1");

            Assert.That(resolvedDefault, Is.Not.Null);
            Assert.That(serviceType.IsInstanceOfType(resolvedDefault), Is.True);
            Assert.That(resolvedC1, Is.InstanceOf<ContractedTestView>());
        }

        [Test]
        public void RegisterViewsInternal_Prefers_DI_Resolution_Over_Activator()
        {
            var resolver = AppLocator.CurrentMutable!;

            var diInstance = new DiBackedView();
            resolver.RegisterConstant(diInstance);

            var method = typeof(AppBuilderExtensions)
                .GetMethod("RegisterViewsInternal", BindingFlags.NonPublic | BindingFlags.Static);

            method!.Invoke(null, [resolver, new[] { typeof(AppBuilderExtensionsRegistrationTests).Assembly }]);

            var serviceType = typeof(IViewFor<>).MakeGenericType(typeof(TestVm));
            var resolved = AppLocator.Current.GetService(serviceType);

            Assert.That(resolved, Is.SameAs(diInstance));
        }

        [Test]
        public void RegisterReactiveUIViews_Throws_On_Null_Builder()
        {
            AppBuilder? builder = null;
            Assert.Throws<ArgumentNullException>(() => builder!.RegisterReactiveUIViews());
        }

        [Test]
        public void RegisterReactiveUIViewsFromAssemblyOf_Returns_Same_Builder()
        {
            var builder = AppBuilder.Configure<Application>();
            var result = builder.RegisterReactiveUIViewsFromAssemblyOf<AppBuilderExtensionsRegistrationTests>();
            Assert.That(result, Is.SameAs(builder));
        }

        [Test]
        public void RegisterReactiveUIViewsFromEntryAssembly_Does_Not_Throw()
        {
            var builder = AppBuilder.Configure<Application>();
            var result = builder.RegisterReactiveUIViewsFromEntryAssembly();
            Assert.That(result, Is.SameAs(builder));
        }

        private sealed class TestVm : ReactiveObject, ITestVm
        {
        }

        [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
        private sealed class ViewContractAttribute(string contract) : Attribute
        {
            public string Contract { get; } = contract;
        }

        private sealed class TestView : UserControl, IViewFor<TestVm>
        {
            public TestVm? ViewModel { get; set; }

            object? IViewFor.ViewModel
            {
                get => ViewModel;
                set => ViewModel = (TestVm?)value;
            }
        }

        [ViewContract("C1")]
        private sealed class ContractedTestView : UserControl, IViewFor<TestVm>
        {
            public TestVm? ViewModel { get; set; }

            object? IViewFor.ViewModel
            {
                get => ViewModel;
                set => ViewModel = (TestVm?)value;
            }
        }

        private sealed class DiBackedView : UserControl, IViewFor<TestVm>
        {
            public TestVm? ViewModel { get; set; }

            object? IViewFor.ViewModel
            {
                get => ViewModel;
                set => ViewModel = (TestVm?)value;
            }
        }
    }
}
