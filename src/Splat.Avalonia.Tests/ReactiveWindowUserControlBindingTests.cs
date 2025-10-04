using Avalonia;
using Avalonia.Controls;
using NUnit.Framework;

namespace ReactiveUI.Avalonia.Tests
{
    public class ReactiveWindowUserControlBindingTests
    {
        [Test]
        public void ReactiveUserControl_DataContext_And_ViewModel_Sync()
        {
            var c = new C();
            var vm = new VM();

            c.DataContext = vm;
            Assert.That(c.ViewModel, Is.SameAs(vm));

            var vm2 = new VM();
            c.ViewModel = vm2;
            Assert.That(c.DataContext, Is.SameAs(vm2));
        }

        [Test]
        public void ReactiveUserControl_ViewModel_Set_To_Null_Syncs_DataContext()
        {
            var c = new C();
            var vm = new VM();
            c.DataContext = vm;
            Assert.That(c.ViewModel, Is.SameAs(vm));

            c.ViewModel = null;
            Assert.That(c.DataContext, Is.Null);
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

        private sealed class C : ReactiveUserControl<VM>
        {
        }
    }
}
