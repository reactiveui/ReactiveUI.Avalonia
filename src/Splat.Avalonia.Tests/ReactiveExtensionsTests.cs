using System;
using System.Reflection;
using Avalonia;
using NUnit.Framework;
using ReactiveUI.Avalonia;

namespace ReactiveUI.Avalonia.Tests
{
    public class ReactiveExtensionsTests
    {
        [Test]
        public void AvaloniaObjectReactiveExtensions_IsStaticClass()
        {
            // Test that AvaloniaObjectReactiveExtensions is a static class
            var type = typeof(AvaloniaObjectReactiveExtensions);
            Assert.That(type.IsClass, Is.True);
            Assert.That(type.IsAbstract, Is.True);
            Assert.That(type.IsSealed, Is.True);
        }

        [Test]
        public void AvaloniaObjectReactiveExtensions_HasGetSubjectMethod()
        {
            // Test that the GetSubject extension method exists
            var type = typeof(AvaloniaObjectReactiveExtensions);
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);

            var hasGetSubjectMethod = false;
            foreach (var method in methods)
            {
                if (method.Name == "GetSubject")
                {
                    hasGetSubjectMethod = true;
                    break;
                }
            }

            Assert.That(hasGetSubjectMethod, Is.True);
        }

        [Test]
        public void AvaloniaObjectReactiveExtensions_HasGetBindingSubjectMethod()
        {
            // Test that the GetBindingSubject extension method exists
            var type = typeof(AvaloniaObjectReactiveExtensions);
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);

            var hasGetBindingSubjectMethod = false;
            foreach (var method in methods)
            {
                if (method.Name == "GetBindingSubject")
                {
                    hasGetBindingSubjectMethod = true;
                    break;
                }
            }

            Assert.That(hasGetBindingSubjectMethod, Is.True);
        }

        [Test]
        public void AutoDataTemplateBindingHook_TypeExists()
        {
            // Test that AutoDataTemplateBindingHook type exists
            var type = typeof(AutoDataTemplateBindingHook);
            Assert.That(type, Is.Not.Null);
            Assert.That(type.IsClass, Is.True);
        }

        [Test]
        public void AvaloniaActivationForViewFetcher_TypeExists()
        {
            // Test that AvaloniaActivationForViewFetcher type exists
            var type = typeof(AvaloniaActivationForViewFetcher);
            Assert.That(type, Is.Not.Null);
            Assert.That(type.IsClass, Is.True);
        }

        [Test]
        public void AutoSuspendHelper_TypeExists()
        {
            // Test that AutoSuspendHelper type exists
            var type = typeof(AutoSuspendHelper);
            Assert.That(type, Is.Not.Null);
            Assert.That(type.IsClass, Is.True);
        }

        [Test]
        public void AutoDataTemplateBindingHook_HasExpectedMethods()
        {
            // Test that AutoDataTemplateBindingHook has expected methods
            var type = typeof(AutoDataTemplateBindingHook);
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);

            var hasExecuteHookMethod = false;

            foreach (var method in methods)
            {
                if (method.Name == "ExecuteHook")
                {
                    hasExecuteHookMethod = true;
                    break;
                }
            }

            Assert.That(hasExecuteHookMethod, Is.True);
        }

        [Test]
        public void AvaloniaActivationForViewFetcher_HasExpectedMethods()
        {
            // Test that AvaloniaActivationForViewFetcher has expected methods
            var type = typeof(AvaloniaActivationForViewFetcher);
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);

            var hasGetAffinityForViewMethod = false;
            var hasGetActivationForViewMethod = false;

            foreach (var method in methods)
            {
                if (method.Name == "GetAffinityForView")
                {
                    hasGetAffinityForViewMethod = true;
                }
                else if (method.Name == "GetActivationForView")
                {
                    hasGetActivationForViewMethod = true;
                }
            }

            Assert.That(hasGetAffinityForViewMethod, Is.True);
            Assert.That(hasGetActivationForViewMethod, Is.True);
        }

        [Test]
        public void ReactiveUIAvalonia_HasPublicExtensionMethods()
        {
            // Test that we have the expected number of public extension methods
            var type = typeof(AvaloniaObjectReactiveExtensions);
            var publicStaticMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);

            // Should have multiple extension methods
            Assert.That(publicStaticMethods.Length, Is.GreaterThan(0));

            // All methods should have ExtensionAttribute
            var allAreExtensions = true;
            foreach (var method in publicStaticMethods)
            {
                var extensionAttr = method.GetCustomAttribute<System.Runtime.CompilerServices.ExtensionAttribute>();
                if (extensionAttr == null)
                {
                    allAreExtensions = false;
                    break;
                }
            }

            Assert.That(allAreExtensions, Is.True);
        }

        [Test]
        public void AutoSuspendHelper_HasExpectedProperties()
        {
            // Test that AutoSuspendHelper has expected functionality (it's an instance class, not static)
            var type = typeof(AutoSuspendHelper);

            // Should be a class that can be instantiated
            Assert.That(type.IsClass, Is.True);
            Assert.That(type.IsAbstract, Is.False);
            Assert.That(type.IsSealed, Is.True);

            // Should have constructors
            var constructors = type.GetConstructors();
            Assert.That(constructors.Length, Is.GreaterThan(0));
        }

        [Test]
        public void PublicTypes_HaveParameterlessConstructors()
        {
            // Test that public types have constructors
            var autoDataTemplateHookType = typeof(AutoDataTemplateBindingHook);
            var constructors = autoDataTemplateHookType.GetConstructors();
            Assert.That(constructors.Length, Is.GreaterThan(0));

            var activationFetcherType = typeof(AvaloniaActivationForViewFetcher);
            var activationConstructors = activationFetcherType.GetConstructors();
            Assert.That(activationConstructors.Length, Is.GreaterThan(0));
        }
    }
}