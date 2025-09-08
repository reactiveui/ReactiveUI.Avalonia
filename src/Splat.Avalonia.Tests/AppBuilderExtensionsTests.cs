using NUnit.Framework;
using ReactiveUI.Avalonia;
using System;
using Avalonia;

namespace ReactiveUI.Avalonia.Tests
{
    public class AppBuilderExtensionsTests
    {
        [Test]
        public void UseReactiveUI_ThrowsOnNullBuilder()
        {
            // Test that UseReactiveUI throws ArgumentNullException for null builder
            AppBuilder? nullBuilder = null;
            Assert.Throws<ArgumentNullException>(() => nullBuilder!.UseReactiveUI());
        }

        [Test]
        public void UseReactiveUI_MethodExists()
        {
            // Test that the UseReactiveUI extension method exists
            var method = typeof(AppBuilderExtensions).GetMethod("UseReactiveUI");
            Assert.That(method, Is.Not.Null);
            Assert.That(method!.IsStatic, Is.True);
            Assert.That(method.IsPublic, Is.True);
        }

        [Test]
        public void AppBuilderExtensions_IsStaticClass()
        {
            // Test that AppBuilderExtensions is a static class
            var type = typeof(AppBuilderExtensions);
            Assert.That(type.IsClass, Is.True);
            Assert.That(type.IsAbstract, Is.True);
            Assert.That(type.IsSealed, Is.True);
        }

        [Test]
        public void UseReactiveUI_HasCorrectSignature()
        {
            // Test that UseReactiveUI has the correct method signature
            var method = typeof(AppBuilderExtensions).GetMethod("UseReactiveUI");
            Assert.That(method, Is.Not.Null);
            
            var parameters = method!.GetParameters();
            Assert.That(parameters.Length, Is.EqualTo(1));
            Assert.That(parameters[0].ParameterType, Is.EqualTo(typeof(AppBuilder)));
            Assert.That(method.ReturnType, Is.EqualTo(typeof(AppBuilder)));
        }
    }
}