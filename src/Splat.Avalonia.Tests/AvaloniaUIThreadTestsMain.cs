using NUnit.Framework;
using ReactiveUI.Avalonia;
using System;

namespace ReactiveUI.Avalonia.Tests
{
    public class AvaloniaUIThreadTestsMain
    {
        [Test]
        public void SimpleTest()
        {
            // Simple test to verify NUnit is working
            Assert.That(1 + 1, Is.EqualTo(2));
        }

        [Test]
        public void AvaloniaScheduler_Instance_IsNotNull()
        {
            // Test that the AvaloniaScheduler singleton instance exists
            Assert.That(AvaloniaScheduler.Instance, Is.Not.Null);
        }

        [Test]
        public void AvaloniaScheduler_Instance_IsSingleton()
        {
            // Test that multiple calls return the same instance
            var instance1 = AvaloniaScheduler.Instance;
            var instance2 = AvaloniaScheduler.Instance;
            Assert.That(instance1, Is.SameAs(instance2));
        }

        [Test]
        public void AvaloniaScheduler_Schedule_ThrowsOnNullAction()
        {
            // Test that Schedule throws ArgumentNullException for null action
            var scheduler = AvaloniaScheduler.Instance;
            Assert.Throws<ArgumentNullException>(() => 
                scheduler.Schedule<object>(new object(), TimeSpan.Zero, null!));
        }
    }
}
