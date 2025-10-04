using System.Linq.Expressions;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using NUnit.Framework;

namespace ReactiveUI.Avalonia.Tests
{
    public class AvaloniaObjectObservableForPropertyTests
    {
        [Test]
        public void GetAffinity_AvaloniaObjectWithProperty_ReturnsPositive()
        {
            var sut = new AvaloniaObjectObservableForProperty();
            var affinity = ((ICreatesObservableForProperty)sut).GetAffinityForObject(typeof(TestControl), nameof(TestControl.Text));
            Assert.That(affinity, Is.GreaterThan(0));
        }

        [Test]
        public void GetAffinity_NonAvaloniaObject_ReturnsZero()
        {
            var sut = new AvaloniaObjectObservableForProperty();
            var affinity = ((ICreatesObservableForProperty)sut).GetAffinityForObject(typeof(object), "Foo");
            Assert.That(affinity, Is.EqualTo(0));
        }

        [Test]
        public void GetNotification_HappyPath_EmitsOnChange()
        {
            var ctrl = new TestControl();
            var sut = new AvaloniaObjectObservableForProperty();
            Expression<Func<string?>> expr = () => ctrl.Text;
            var changes = ((ICreatesObservableForProperty)sut).GetNotificationForProperty(ctrl, expr, nameof(TestControl.Text));

            IObservedChange<object?, object?>? last = null;
            using var sub = changes.Subscribe(c => last = c);

            ctrl.Text = "hello";

            Assert.That(last, Is.Not.Null);
            Assert.That(last!.Sender, Is.EqualTo(ctrl));
            Assert.That(last.Value, Is.EqualTo("hello"));
        }

        [Test]
        public void GetNotification_MissingProperty_ThrowsImmediately()
        {
            var ctrl = new TestControl();
            var sut = new AvaloniaObjectObservableForProperty();
            Expression<Func<string?>> expr = () => ctrl.Text;

            Assert.Throws<NullReferenceException>(() =>
                ((ICreatesObservableForProperty)sut).GetNotificationForProperty(ctrl, expr, "Missing", beforeChanged: false, suppressWarnings: true));
        }

        [Test]
        public void GetNotification_NonAvaloniaSender_Throws()
        {
            var sut = new AvaloniaObjectObservableForProperty();
            Expression<Func<object?>> expr = () => new object();
            Assert.Throws<InvalidOperationException>(() =>
                ((ICreatesObservableForProperty)sut).GetNotificationForProperty(new object(), expr, "Foo"));
        }

        private class TestControl : Control
        {
            public static readonly StyledProperty<string?> TextProperty =
                AvaloniaProperty.Register<TestControl, string?>(nameof(Text));

            public string? Text
            {
                get => GetValue(TextProperty);
                set => SetValue(TextProperty, value);
            }
        }
    }
}
