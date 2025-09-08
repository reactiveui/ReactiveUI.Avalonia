using NUnit.Framework;
using ReactiveUI.Avalonia;
using System;
using System.Reflection;
using Avalonia.Controls;

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

        [Test]
        public void ReactiveUserControl_InheritsFromUserControl()
        {
            // Test that ReactiveUserControl inherits from UserControl
            var controlType = typeof(ReactiveUserControl<object>);
            Assert.That(controlType.BaseType, Is.Not.Null);
            
            // Check if it inherits from UserControl somewhere in the hierarchy
            var currentType = controlType.BaseType;
            bool inheritsFromUserControl = false;
            while (currentType != null && currentType != typeof(object))
            {
                if (currentType == typeof(UserControl) || 
                    currentType.Name.Contains("UserControl"))
                {
                    inheritsFromUserControl = true;
                    break;
                }
                currentType = currentType.BaseType;
            }
            
            Assert.That(inheritsFromUserControl, Is.True);
        }

        [Test]
        public void ReactiveWindow_InheritsFromWindow()
        {
            // Test that ReactiveWindow inherits from Window
            var windowType = typeof(ReactiveWindow<object>);
            Assert.That(windowType.BaseType, Is.Not.Null);
            
            // Check if it inherits from Window somewhere in the hierarchy
            var currentType = windowType.BaseType;
            bool inheritsFromWindow = false;
            while (currentType != null && currentType != typeof(object))
            {
                if (currentType == typeof(Window) || 
                    currentType.Name.Contains("Window"))
                {
                    inheritsFromWindow = true;
                    break;
                }
                currentType = currentType.BaseType;
            }
            
            Assert.That(inheritsFromWindow, Is.True);
        }

        [Test]
        public void ViewModelViewHost_InheritsFromContentControl()
        {
            // Test that ViewModelViewHost inherits from ContentControl
            var hostType = typeof(ViewModelViewHost);
            Assert.That(hostType.BaseType, Is.Not.Null);
            
            // Check if it inherits from ContentControl somewhere in the hierarchy
            var currentType = hostType.BaseType;
            bool inheritsFromContentControl = false;
            while (currentType != null && currentType != typeof(object))
            {
                if (currentType == typeof(ContentControl) || 
                    currentType.Name.Contains("ContentControl"))
                {
                    inheritsFromContentControl = true;
                    break;
                }
                currentType = currentType.BaseType;
            }
            
            Assert.That(inheritsFromContentControl, Is.True);
        }

        [Test]
        public void RoutedViewHost_InheritsFromContentControl()
        {
            // Test that RoutedViewHost inherits from ContentControl
            var hostType = typeof(RoutedViewHost);
            Assert.That(hostType.BaseType, Is.Not.Null);
            
            // Check if it inherits from ContentControl somewhere in the hierarchy
            var currentType = hostType.BaseType;
            bool inheritsFromContentControl = false;
            while (currentType != null && currentType != typeof(object))
            {
                if (currentType == typeof(ContentControl) || 
                    currentType.Name.Contains("ContentControl"))
                {
                    inheritsFromContentControl = true;
                    break;
                }
                currentType = currentType.BaseType;
            }
            
            Assert.That(inheritsFromContentControl, Is.True);
        }

        [Test]
        public void ReactiveUserControl_IsGeneric()
        {
            // Test that ReactiveUserControl is generic
            var controlType = typeof(ReactiveUserControl<>);
            Assert.That(controlType.IsGenericTypeDefinition, Is.True);
            
            var genericArguments = controlType.GetGenericArguments();
            Assert.That(genericArguments.Length, Is.EqualTo(1));
        }

        [Test]
        public void ReactiveWindow_IsGeneric()
        {
            // Test that ReactiveWindow is generic
            var windowType = typeof(ReactiveWindow<>);
            Assert.That(windowType.IsGenericTypeDefinition, Is.True);
            
            var genericArguments = windowType.GetGenericArguments();
            Assert.That(genericArguments.Length, Is.EqualTo(1));
        }

        [Test]
        public void ViewModelViewHost_HasExpectedProperties()
        {
            // Test that ViewModelViewHost has the expected public properties
            var hostType = typeof(ViewModelViewHost);
            
            // Check for ViewModel property
            var viewModelProp = hostType.GetProperty("ViewModel");
            Assert.That(viewModelProp, Is.Not.Null);
            
            // Check for DefaultContent property
            var defaultContentProp = hostType.GetProperty("DefaultContent");
            Assert.That(defaultContentProp, Is.Not.Null);
            
            // Check for ViewContract property
            var viewContractProp = hostType.GetProperty("ViewContract");
            Assert.That(viewContractProp, Is.Not.Null);
        }

        [Test]
        public void RoutedViewHost_HasExpectedProperties()
        {
            // Test that RoutedViewHost has the expected public properties
            var hostType = typeof(RoutedViewHost);
            
            // Check for DefaultContent property
            var defaultContentProp = hostType.GetProperty("DefaultContent");
            Assert.That(defaultContentProp, Is.Not.Null);
            
            // Check for ViewContract property
            var viewContractProp = hostType.GetProperty("ViewContract");
            Assert.That(viewContractProp, Is.Not.Null);
            
            // Check for Router property
            var routerProp = hostType.GetProperty("Router");
            Assert.That(routerProp, Is.Not.Null);
        }
    }
}