using NUnit.Framework;
using ReactiveUI.Avalonia;
using System;

namespace ReactiveUI.Avalonia.Tests
{
    public class ReactiveControlsTests
    {
        [Test]
        public void ReactiveUserControl_CanBeCreatedWithoutSetup()
        {
            // Test that we can create a ReactiveUserControl type (just the type, not an instance)
            var controlType = typeof(ReactiveUserControl<object>);
            Assert.That(controlType, Is.Not.Null);
            Assert.That(controlType.IsClass, Is.True);
        }

        [Test]
        public void ReactiveWindow_CanBeCreatedWithoutSetup()
        {
            // Test that we can create a ReactiveWindow type (just the type, not an instance)
            var windowType = typeof(ReactiveWindow<object>);
            Assert.That(windowType, Is.Not.Null);
            Assert.That(windowType.IsClass, Is.True);
        }

        [Test]
        public void ViewModelViewHost_TypeExists()
        {
            // Test that ViewModelViewHost type exists
            var hostType = typeof(ViewModelViewHost);
            Assert.That(hostType, Is.Not.Null);
            Assert.That(hostType.IsClass, Is.True);
        }

        [Test]
        public void RoutedViewHost_TypeExists()
        {
            // Test that RoutedViewHost type exists
            var hostType = typeof(RoutedViewHost);
            Assert.That(hostType, Is.Not.Null);
            Assert.That(hostType.IsClass, Is.True);
        }
    }
}