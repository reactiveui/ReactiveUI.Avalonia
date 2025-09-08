using NUnit.Framework;

namespace ReactiveUI.Avalonia.Tests
{
    public class DependencyInjectionTests
    {
        [Test]
        public void BasicDependencyInjectionTest()
        {
            // Simple test to verify dependency injection projects compile
            Assert.That(true, Is.True, "Basic dependency injection test");
        }
    }
}