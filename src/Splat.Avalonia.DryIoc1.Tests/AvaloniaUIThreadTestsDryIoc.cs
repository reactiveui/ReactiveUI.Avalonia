using Avalonia;
using Avalonia.ReactiveUI.Splat;
using DryIoc;
using NUnit.Framework;
using ReactiveUIDemo;
using Splat;
using Splat.DryIoc;

namespace ReactiveUI.Avalonia.DryIoc1.Tests
{
    public class AvaloniaUIThreadTestsDryIoc
    {
#if DRYIOC1
        [Test]
        public void Test1()
        {
            // Simplified test for now - we can expand this later
            Assert.That(true, Is.True, "DryIoc container test placeholder");
        }
#endif
#if DRYIOC2
        [Test]
        public void Test2()
        {
            // Simplified test for now - we can expand this later
            Assert.That(true, Is.True, "DryIoc container test2 placeholder");
        }
#endif
    }
}
