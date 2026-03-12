// Copyright (c) 2019-2026 ReactiveUI and Avalonia Teams, and Contributors. All rights reserved.
// Licensed under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Splat;

namespace ReactiveUI.Avalonia.Tests;

/// <summary>
/// Tests for view registration via AppBuilderExtensions internal methods.
/// </summary>
public class AppBuilderExtensionsRegistrationTests
{
    /// <summary>
    /// Contract interface for the test view model.
    /// </summary>
    private interface ITestVm
    {
    }

    /// <summary>
    /// Verifies that RegisterViewsInternal registers a view for a view model using the activator.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RegisterViewsInternal_Registers_View_For_ViewModel_Using_Activator()
    {
        var resolver = AppLocator.CurrentMutable!;
        var method = typeof(AppBuilderExtensions)
            .GetMethod("RegisterViewsInternal", BindingFlags.NonPublic | BindingFlags.Static);
        await Assert.That(method).IsNotNull();

        method!.Invoke(null, [resolver, new[] { typeof(AppBuilderExtensionsRegistrationTests).Assembly }]);

        var serviceType = typeof(IViewFor<>).MakeGenericType(typeof(TestVm));
        var resolved = AppLocator.Current.GetService(serviceType);

        await Assert.That(resolved).IsNotNull();
        await Assert.That(serviceType.IsInstanceOfType(resolved)).IsTrue();
    }

    /// <summary>
    /// Verifies that RegisterViewsInternal honors the ViewContract attribute for contract-based resolution.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RegisterViewsInternal_Honors_ViewContractAttribute()
    {
        var resolver = AppLocator.CurrentMutable!;
        var method = typeof(AppBuilderExtensions)
            .GetMethod("RegisterViewsInternal", BindingFlags.NonPublic | BindingFlags.Static);

        method!.Invoke(null, [resolver, new[] { typeof(AppBuilderExtensionsRegistrationTests).Assembly }]);

        var serviceType = typeof(IViewFor<>).MakeGenericType(typeof(TestVm));
        var resolvedDefault = AppLocator.Current.GetService(serviceType);
        var resolvedC1 = AppLocator.Current.GetService(serviceType, "C1");

        await Assert.That(resolvedDefault).IsNotNull();
        await Assert.That(serviceType.IsInstanceOfType(resolvedDefault)).IsTrue();
        await Assert.That(resolvedC1).IsTypeOf<ContractedTestView>();
    }

    /// <summary>
    /// Verifies that RegisterViewsInternal prefers DI-resolved instances over activator-created ones.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RegisterViewsInternal_Prefers_DI_Resolution_Over_Activator()
    {
        var resolver = AppLocator.CurrentMutable!;

        var diInstance = new DiBackedView();
        resolver.RegisterConstant(diInstance);

        var method = typeof(AppBuilderExtensions)
            .GetMethod("RegisterViewsInternal", BindingFlags.NonPublic | BindingFlags.Static);

        method!.Invoke(null, [resolver, new[] { typeof(AppBuilderExtensionsRegistrationTests).Assembly }]);

        var serviceType = typeof(IViewFor<>).MakeGenericType(typeof(TestVm));
        var resolved = AppLocator.Current.GetService(serviceType);

        await Assert.That(resolved).IsSameReferenceAs(diInstance);
    }

    /// <summary>
    /// Verifies that RegisterReactiveUIViews throws on a null builder.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RegisterReactiveUIViews_Throws_On_Null_Builder()
    {
        AppBuilder? builder = null;
        await Assert.That(() => builder!.RegisterReactiveUIViews()).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that RegisterReactiveUIViewsFromAssemblyOf returns the same builder instance.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RegisterReactiveUIViewsFromAssemblyOf_Returns_Same_Builder()
    {
        var builder = AppBuilder.Configure<Application>();
        var result = builder.RegisterReactiveUIViewsFromAssemblyOf<AppBuilderExtensionsRegistrationTests>();
        await Assert.That(result).IsSameReferenceAs(builder);
    }

    /// <summary>
    /// Verifies that RegisterReactiveUIViewsFromEntryAssembly does not throw and returns the same builder.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RegisterReactiveUIViewsFromEntryAssembly_Does_Not_Throw()
    {
        var builder = AppBuilder.Configure<Application>();
        var result = builder.RegisterReactiveUIViewsFromEntryAssembly();
        await Assert.That(result).IsSameReferenceAs(builder);
    }

    /// <summary>
    /// Verifies that RegisterViewsInternal ignores duplicate assemblies.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RegisterViewsInternal_Ignores_Duplicate_Assemblies()
    {
        var resolver = AppLocator.CurrentMutable!;
        var method = typeof(AppBuilderExtensions)
            .GetMethod("RegisterViewsInternal", BindingFlags.NonPublic | BindingFlags.Static);

        var assembly = typeof(DistinctRegistrationVm).Assembly;
        var serviceType = typeof(IViewFor<>).MakeGenericType(typeof(DistinctRegistrationVm));
        var before = AppLocator.Current.GetServices(serviceType).Count();

        method!.Invoke(null, [resolver, new[] { assembly, assembly }]);

        var after = AppLocator.Current.GetServices(serviceType).Count();
        var resolved = AppLocator.Current.GetService(serviceType);

        await Assert.That(after).IsEqualTo(before + 1);
        await Assert.That(resolved).IsTypeOf<DistinctRegistrationView>();
    }

    /// <summary>
    /// A test view model implementing ITestVm.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated via reflection by RegisterViewsInternal.")]
    private sealed class TestVm : ReactiveObject, ITestVm
    {
    }

    /// <summary>
    /// Attribute to specify a view contract name.
    /// </summary>
    /// <param name="contract">The contract name.</param>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    private sealed class ViewContractAttribute(string contract) : Attribute
    {
        /// <summary>
        /// Gets the contract name.
        /// </summary>
        public string Contract { get; } = contract;
    }

    /// <summary>
    /// A test view for TestVm without a contract.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated via reflection by RegisterViewsInternal.")]
    private sealed class TestView : UserControl, IViewFor<TestVm>
    {
        /// <summary>
        /// Gets or sets the view model.
        /// </summary>
        public TestVm? ViewModel { get; set; }

        /// <inheritdoc/>
        object? IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (TestVm?)value;
        }
    }

    /// <summary>
    /// A test view for TestVm with contract "C1".
    /// </summary>
    [ViewContract("C1")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated via reflection by RegisterViewsInternal.")]
    private sealed class ContractedTestView : UserControl, IViewFor<TestVm>
    {
        /// <summary>
        /// Gets or sets the view model.
        /// </summary>
        public TestVm? ViewModel { get; set; }

        /// <inheritdoc/>
        object? IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (TestVm?)value;
        }
    }

    /// <summary>
    /// A DI-backed test view for TestVm.
    /// </summary>
    private sealed class DiBackedView : UserControl, IViewFor<TestVm>
    {
        /// <summary>
        /// Gets or sets the view model.
        /// </summary>
        public TestVm? ViewModel { get; set; }

        /// <inheritdoc/>
        object? IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (TestVm?)value;
        }
    }

    /// <summary>
    /// A distinct view model used to validate duplicate assembly handling.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated via reflection by RegisterViewsInternal.")]
    private sealed class DistinctRegistrationVm : ReactiveObject
    {
    }

    /// <summary>
    /// A distinct view used to validate duplicate assembly handling.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated via reflection by RegisterViewsInternal.")]
    private sealed class DistinctRegistrationView : UserControl, IViewFor<DistinctRegistrationVm>
    {
        /// <summary>
        /// Gets or sets the view model.
        /// </summary>
        public DistinctRegistrationVm? ViewModel { get; set; }

        /// <inheritdoc/>
        object? IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (DistinctRegistrationVm?)value;
        }
    }
}
