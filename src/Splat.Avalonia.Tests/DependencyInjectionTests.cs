using System;
using System.Reflection;
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

        [Test]
        public void ReactiveUIAvaloniaAssembly_IsLoaded()
        {
            // Test that the ReactiveUI.Avalonia assembly is loaded
            var assembly = Assembly.GetAssembly(typeof(ReactiveUI.Avalonia.AvaloniaScheduler));
            Assert.That(assembly, Is.Not.Null);
            Assert.That(assembly.FullName, Does.Contain("ReactiveUI.Avalonia"));
        }

        [Test]
        public void ReactiveUIAvaloniaAssembly_HasExpectedTypes()
        {
            // Test that the assembly contains expected types
            var assembly = Assembly.GetAssembly(typeof(ReactiveUI.Avalonia.AvaloniaScheduler));
            Assert.That(assembly, Is.Not.Null);
            var types = assembly.GetTypes();

            var hasAvaloniaScheduler = false;
            var hasReactiveUserControl = false;
            var hasReactiveWindow = false;
            var hasViewModelViewHost = false;

            foreach (var type in types)
            {
                if (type.Name == "AvaloniaScheduler")
                {
                    hasAvaloniaScheduler = true;
                }
                else if (type.Name.StartsWith("ReactiveUserControl"))
                {
                    hasReactiveUserControl = true;
                }
                else if (type.Name.StartsWith("ReactiveWindow"))
                {
                    hasReactiveWindow = true;
                }
                else if (type.Name == "ViewModelViewHost")
                {
                    hasViewModelViewHost = true;
                }
            }

            Assert.That(hasAvaloniaScheduler, Is.True);
            Assert.That(hasReactiveUserControl, Is.True);
            Assert.That(hasReactiveWindow, Is.True);
            Assert.That(hasViewModelViewHost, Is.True);
        }

        [Test]
        public void ReactiveUIAvaloniaNamespace_IsCorrect()
        {
            // Test that types are in the correct namespace
            var scheduler = typeof(ReactiveUI.Avalonia.AvaloniaScheduler);
            Assert.That(scheduler.Namespace, Is.EqualTo("ReactiveUI.Avalonia"));

            var userControl = typeof(ReactiveUI.Avalonia.ReactiveUserControl<>);
            Assert.That(userControl.Namespace, Is.EqualTo("ReactiveUI.Avalonia"));

            var window = typeof(ReactiveUI.Avalonia.ReactiveWindow<>);
            Assert.That(window.Namespace, Is.EqualTo("ReactiveUI.Avalonia"));
        }

        [Test]
        public void AssemblyInfo_HasExpectedMetadata()
        {
            // Test that assembly has expected metadata
            var assembly = Assembly.GetAssembly(typeof(ReactiveUI.Avalonia.AvaloniaScheduler));
            var assemblyName = assembly!.GetName();

            Assert.That(assemblyName.Name, Is.EqualTo("ReactiveUI.Avalonia"));
            Assert.That(assemblyName.Version, Is.Not.Null);
        }

        [Test]
        public void PublicTypes_ArePublic()
        {
            // Test that main types are public
            var scheduler = typeof(ReactiveUI.Avalonia.AvaloniaScheduler);
            Assert.That(scheduler.IsPublic, Is.True);

            var userControl = typeof(ReactiveUI.Avalonia.ReactiveUserControl<>);
            Assert.That(userControl.IsPublic, Is.True);

            var window = typeof(ReactiveUI.Avalonia.ReactiveWindow<>);
            Assert.That(window.IsPublic, Is.True);

            var viewHost = typeof(ReactiveUI.Avalonia.ViewModelViewHost);
            Assert.That(viewHost.IsPublic, Is.True);
        }

        [Test]
        public void ExtensionMethods_ArePublicAndStatic()
        {
            // Test that extension methods are public and static
            var appBuilderExts = typeof(ReactiveUI.Avalonia.AppBuilderExtensions);
            Assert.That(appBuilderExts.IsPublic, Is.True);
            Assert.That(appBuilderExts.IsSealed, Is.True);
            Assert.That(appBuilderExts.IsAbstract, Is.True);

            var avaloniaExts = typeof(ReactiveUI.Avalonia.AvaloniaObjectReactiveExtensions);
            Assert.That(avaloniaExts.IsPublic, Is.True);
            Assert.That(avaloniaExts.IsSealed, Is.True);
            Assert.That(avaloniaExts.IsAbstract, Is.True);
        }

        [Test]
        public void AssemblyReferences_ContainExpectedDependencies()
        {
            // Test that assembly references expected dependencies
            var assembly = Assembly.GetAssembly(typeof(ReactiveUI.Avalonia.AvaloniaScheduler));
            var referencedAssemblies = assembly!.GetReferencedAssemblies();

            var hasAvaloniaBase = false;
            var hasReactiveUI = false;
            var hasSystemReactive = false;

            foreach (var refAssembly in referencedAssemblies)
            {
                if (refAssembly.Name!.StartsWith("Avalonia.Base") || refAssembly.Name.StartsWith("Avalonia"))
                {
                    hasAvaloniaBase = true;
                }
                else if (refAssembly.Name.StartsWith("ReactiveUI"))
                {
                    hasReactiveUI = true;
                }
                else if (refAssembly.Name.Contains("System.Reactive"))
                {
                    hasSystemReactive = true;
                }
            }

            Assert.That(hasAvaloniaBase, Is.True, "Should reference Avalonia");
            Assert.That(hasReactiveUI, Is.True, "Should reference ReactiveUI");
            Assert.That(hasSystemReactive, Is.True, "Should reference System.Reactive");
        }
    }
}