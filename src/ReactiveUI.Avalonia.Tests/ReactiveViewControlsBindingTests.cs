#pragma warning disable SA1201
using NUnit.Framework;
using Splat;

namespace ReactiveUI.Avalonia.Tests
{
    public class ReactiveViewControlsBindingTests
    {
        [SetUp]
        public void Setup()
        {
            // Ensure activation fetcher is registered to prevent WhenActivated errors.
            Locator.CurrentMutable.RegisterConstant<IActivationForViewFetcher>(new AvaloniaActivationForViewFetcher());
        }

        private sealed class VM : ReactiveObject
        {
            private string? _name;

            public string? Name
            {
                get => _name;
                set => this.RaiseAndSetIfChanged(ref _name, value);
            }
        }

        private sealed class View : ReactiveUserControl<VM>;

        [Test]
        public void ReactiveUserControl_ViewModel_DataContext_Syncs()
        {
            var v = new View();
            var vm = new VM();

            v.DataContext = vm; // should push into ViewModel per override
            Assert.That(v.ViewModel, Is.SameAs(vm));

            var vm2 = new VM();
            v.ViewModel = vm2; // should push into DataContext
            Assert.That(v.DataContext, Is.SameAs(vm2));
        }
    }
}
