using NUnit.Framework;
using ReactiveUI.Avalonia;
using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;

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

        [Test]
        public void AvaloniaScheduler_IsLocalScheduler()
        {
            // Test that AvaloniaScheduler is a LocalScheduler
            var scheduler = AvaloniaScheduler.Instance;
            Assert.That(scheduler, Is.InstanceOf<LocalScheduler>());
        }

        [Test]
        public void AvaloniaScheduler_Now_ReturnsCurrentTime()
        {
            // Test that Now property returns approximately current time
            var scheduler = AvaloniaScheduler.Instance;
            var beforeNow = DateTimeOffset.Now;
            var schedulerNow = scheduler.Now;
            var afterNow = DateTimeOffset.Now;
            
            Assert.That(schedulerNow, Is.GreaterThanOrEqualTo(beforeNow.AddSeconds(-1)));
            Assert.That(schedulerNow, Is.LessThanOrEqualTo(afterNow.AddSeconds(1)));
        }

        [Test]
        public void AvaloniaScheduler_Schedule_WithZeroDelay_ExecutesAction()
        {
            var scheduler = AvaloniaScheduler.Instance;
            bool actionExecuted = false;
            
            // Schedule action with zero delay
            var disposable = scheduler.Schedule("test", TimeSpan.Zero, (s, state) =>
            {
                actionExecuted = true;
                Assert.That(state, Is.EqualTo("test"));
                return Disposable.Empty;
            });
            
            // Give some time for the action to execute
            Thread.Sleep(50);
            
            Assert.That(actionExecuted, Is.True);
            Assert.That(disposable, Is.Not.Null);
        }

        [Test]
        public void AvaloniaScheduler_Schedule_WithPositiveDelay_ReturnsDisposable()
        {
            var scheduler = AvaloniaScheduler.Instance;
            var delay = TimeSpan.FromMilliseconds(10);
            
            // Schedule action with delay - we're not testing execution timing in headless environment
            var disposable = scheduler.Schedule("test", delay, (s, state) => Disposable.Empty);
            
            // Test that we get a disposable back
            Assert.That(disposable, Is.Not.Null);
            Assert.That(disposable, Is.InstanceOf<IDisposable>());
            
            // Clean up
            disposable.Dispose();
        }

        [Test]
        public void AvaloniaScheduler_Schedule_ReturnsDisposable()
        {
            var scheduler = AvaloniaScheduler.Instance;
            
            var disposable = scheduler.Schedule("test", TimeSpan.Zero, (s, state) => Disposable.Empty);
            
            Assert.That(disposable, Is.Not.Null);
            Assert.That(disposable, Is.InstanceOf<IDisposable>());
        }

        [Test]
        public void AvaloniaScheduler_Schedule_CanDisposeBeforeExecution()
        {
            var scheduler = AvaloniaScheduler.Instance;
            bool actionExecuted = false;
            
            // Schedule action with delay
            var disposable = scheduler.Schedule("test", TimeSpan.FromMilliseconds(100), (s, state) =>
            {
                actionExecuted = true;
                return Disposable.Empty;
            });
            
            // Dispose before execution
            disposable.Dispose();
            
            // Wait longer than the delay
            Thread.Sleep(200);
            
            // Action should not have been executed
            Assert.That(actionExecuted, Is.False);
        }
    }
}
