// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Reflection;
using System.Runtime.ExceptionServices;
using Avalonia;
using Avalonia.Controls;
using Splat;

namespace ReactiveUI.Avalonia.Tests;

/// <summary>Tests for view registration via AppBuilderExtensions internal methods.</summary>
public class AppBuilderExtensionsRegistrationTests
{
    /// <summary>Contract interface for the test view model.</summary>
    public interface ITestVm
    {
        /// <summary>Gets a marker value for the test contract.</summary>
        object? TestContract { get; }
    }

    /// <summary>Verifies that RegisterViewsInternal registers a view for a view model using the activator.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RegisterViewsInternal_Registers_View_For_ViewModel_Using_Activator()
    {
        var resolver = AppLocator.CurrentMutable!;
        Assembly[] assemblies = [typeof(AppBuilderExtensionsRegistrationTests).Assembly];
        var method = typeof(AppBuilderExtensions)
            .GetMethod("RegisterViewsInternal", BindingFlags.NonPublic | BindingFlags.Static);
        await Assert.That(method).IsNotNull();

        _ = method!.Invoke(null, [resolver, assemblies]);

        var serviceType = typeof(IViewFor<>).MakeGenericType(typeof(TestVm));
        var resolved = AppLocator.Current.GetService(serviceType);

        await Assert.That(resolved).IsNotNull();
        await Assert.That(serviceType.IsInstanceOfType(resolved)).IsTrue();
    }

    /// <summary>Verifies that RegisterViewsInternal honors the ViewContract attribute for contract-based resolution.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RegisterViewsInternal_Honors_ViewContractAttribute()
    {
        var resolver = AppLocator.CurrentMutable!;
        Assembly[] assemblies = [typeof(AppBuilderExtensionsRegistrationTests).Assembly];
        var method = typeof(AppBuilderExtensions)
            .GetMethod("RegisterViewsInternal", BindingFlags.NonPublic | BindingFlags.Static);

        _ = method!.Invoke(null, [resolver, assemblies]);

        var serviceType = typeof(IViewFor<>).MakeGenericType(typeof(TestVm));
        var resolvedDefault = AppLocator.Current.GetService(serviceType);
        var resolvedC1 = AppLocator.Current.GetService(serviceType, "C1");

        await Assert.That(resolvedDefault).IsNotNull();
        await Assert.That(serviceType.IsInstanceOfType(resolvedDefault)).IsTrue();
        await Assert.That(resolvedC1).IsTypeOf<ContractedTestView>();
    }

    /// <summary>Verifies that a ViewContractAttribute without a Contract property is treated as the default contract.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RegisterViewsInternal_ViewContractAttributeWithoutContract_UsesDefaultContract()
    {
        var resolver = AppLocator.CurrentMutable!;
        Assembly[] assemblies = [typeof(AppBuilderExtensionsRegistrationTests).Assembly];
        var method = typeof(AppBuilderExtensions)
            .GetMethod("RegisterViewsInternal", BindingFlags.NonPublic | BindingFlags.Static);

        _ = method!.Invoke(null, [resolver, assemblies]);

        var serviceType = typeof(IViewFor<>).MakeGenericType(typeof(NoContractVm));
        var resolved = AppLocator.Current.GetService(serviceType);

        await Assert.That(resolved).IsTypeOf<NoContractView>();
    }

    /// <summary>Verifies that RegisterViewsInternal prefers DI-resolved instances over activator-created ones.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RegisterViewsInternal_Prefers_DI_Resolution_Over_Activator()
    {
        var resolver = AppLocator.CurrentMutable!;

        var resolverBackedView = new DiBackedView();
        resolver.RegisterConstant(resolverBackedView);

        Assembly[] assemblies = [typeof(AppBuilderExtensionsRegistrationTests).Assembly];
        var method = typeof(AppBuilderExtensions)
            .GetMethod("RegisterViewsInternal", BindingFlags.NonPublic | BindingFlags.Static);

        _ = method!.Invoke(null, [resolver, assemblies]);

        var serviceType = typeof(IViewFor<>).MakeGenericType(typeof(TestVm));
        var resolved = AppLocator.Current.GetService(serviceType);

        await Assert.That(resolved).IsSameReferenceAs(resolverBackedView);
    }

    /// <summary>Verifies that CreateView falls back to Activator when service resolution throws.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CreateView_WhenResolverThrows_FallsBackToActivator()
    {
        AppLocator.CurrentMutable!.Register(
            static () => throw new InvalidOperationException("expected"),
            typeof(FallbackView));

        var method = typeof(AppBuilderExtensions)
            .GetMethod("CreateView", BindingFlags.NonPublic | BindingFlags.Static);

        var resolved = method!.Invoke(null, [typeof(FallbackView)]);

        await Assert.That(resolved).IsTypeOf<FallbackView>();
    }

    /// <summary>Verifies that CreateView throws when Activator returns null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CreateView_WhenActivatorReturnsNull_ThrowsInvalidOperationException()
    {
        await Assert.That(() => InvokeCreateView(typeof(int?))).ThrowsExactly<InvalidOperationException>();
    }

    /// <summary>Verifies that RegisterReactiveUIViews throws on a null builder.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RegisterReactiveUIViews_Throws_On_Null_Builder()
    {
        AppBuilder? builder = null;
        await Assert.That(() => builder!.RegisterReactiveUIViews()).ThrowsExactly<ArgumentNullException>();
    }

    /// <summary>Verifies that RegisterReactiveUIViewsFromAssemblyOf returns the same builder instance.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RegisterReactiveUIViewsFromAssemblyOf_Returns_Same_Builder()
    {
        var builder = AppBuilder.Configure<Application>();
        var result = builder.RegisterReactiveUIViewsFromAssemblyOf<AppBuilderExtensionsRegistrationTests>();
        await Assert.That(result).IsSameReferenceAs(builder);
    }

    /// <summary>Verifies that RegisterReactiveUIViewsFromEntryAssembly does not throw and returns the same builder.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RegisterReactiveUIViewsFromEntryAssembly_Does_Not_Throw()
    {
        var builder = AppBuilder.Configure<Application>();
        var result = builder.RegisterReactiveUIViewsFromEntryAssembly();
        await Assert.That(result).IsSameReferenceAs(builder);
    }

    /// <summary>Verifies that the entry-assembly helper returns the original builder when no entry assembly is available.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RegisterReactiveUIViewsFromEntryAssembly_WithNullEntry_ReturnsSameBuilder()
    {
        var builder = AppBuilder.Configure<Application>();

        var result = InvokeRegisterReactiveUIViewsFromEntryAssembly(builder, null);

        await Assert.That(result).IsSameReferenceAs(builder);
    }

    /// <summary>Verifies that the entry-assembly helper registers views when an entry assembly is supplied.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RegisterReactiveUIViewsFromEntryAssembly_WithEntryAssembly_RegistersViews()
    {
        var builder = AppBuilder.Configure<Application>();

        var result = InvokeRegisterReactiveUIViewsFromEntryAssembly(builder, typeof(AppBuilderExtensionsRegistrationTests).Assembly);
        InvokeAfterPlatformServicesSetup(result);

        var serviceType = typeof(IViewFor<>).MakeGenericType(typeof(DistinctRegistrationVm));
        var resolved = AppLocator.Current.GetService(serviceType);

        await Assert.That(result).IsSameReferenceAs(builder);
        await Assert.That(resolved).IsTypeOf<DistinctRegistrationView>();
    }

    /// <summary>Verifies that RegisterReactiveUIViews executes its platform setup callback for empty assemblies.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RegisterReactiveUIViews_AfterPlatformCallback_WithEmptyAssemblies_Returns()
    {
        var builder = AppBuilder.Configure<Application>().RegisterReactiveUIViews();

        InvokeAfterPlatformServicesSetup(builder);

        await Assert.That(builder).IsNotNull();
    }

    /// <summary>Verifies that the private view registration guard returns for unavailable inputs.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RegisterReactiveUIViews_WithUnavailableInputs_Returns()
    {
        var resolver = AppLocator.CurrentMutable!;
        var assembly = typeof(AppBuilderExtensionsRegistrationTests).Assembly;

        InvokeRegisterReactiveUIViews(null, [assembly]);
        InvokeRegisterReactiveUIViews(resolver, null);
        InvokeRegisterReactiveUIViews(resolver, []);

        await Assert.That(AppLocator.CurrentMutable).IsSameReferenceAs(resolver);
    }

    /// <summary>Verifies that RegisterReactiveUIViews executes its platform setup callback and registers views.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RegisterReactiveUIViews_AfterPlatformCallback_RegistersViews()
    {
        var builder = AppBuilder.Configure<Application>()
            .RegisterReactiveUIViews(typeof(AppBuilderExtensionsRegistrationTests).Assembly);

        InvokeAfterPlatformServicesSetup(builder);

        var serviceType = typeof(IViewFor<>).MakeGenericType(typeof(DistinctRegistrationVm));
        var resolved = AppLocator.Current.GetService(serviceType);

        await Assert.That(resolved).IsTypeOf<DistinctRegistrationView>();
    }

    /// <summary>Verifies that RegisterViewsInternal ignores duplicate assemblies.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task RegisterViewsInternal_Ignores_Duplicate_Assemblies()
    {
        var resolver = AppLocator.CurrentMutable!;
        var method = typeof(AppBuilderExtensions)
            .GetMethod("RegisterViewsInternal", BindingFlags.NonPublic | BindingFlags.Static);

        var assembly = typeof(DistinctRegistrationVm).Assembly;
        Assembly[] assemblies = [assembly, assembly];
        var serviceType = typeof(IViewFor<>).MakeGenericType(typeof(DistinctRegistrationVm));
        var before = AppLocator.Current.GetServices(serviceType).Count();

        _ = method!.Invoke(null, [resolver, assemblies]);

        var after = AppLocator.Current.GetServices(serviceType).Count();
        var resolved = AppLocator.Current.GetService(serviceType);

        await Assert.That(after).IsEqualTo(before + 1);
        await Assert.That(resolved).IsTypeOf<DistinctRegistrationView>();
    }

    /// <summary>Invokes the AppBuilder platform setup callback registered by extension methods.</summary>
    /// <param name="builder">The application builder.</param>
    private static void InvokeAfterPlatformServicesSetup(AppBuilder builder)
    {
        var property = typeof(AppBuilder).GetProperty(
            "AfterPlatformServicesSetupCallback",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        var callback = (Action<AppBuilder>?)property?.GetValue(builder);
        callback?.Invoke(builder);
    }

    /// <summary>Invokes the private CreateView method and preserves the thrown exception type.</summary>
    /// <param name="viewType">The type to create.</param>
    /// <returns>The created view instance.</returns>
    private static object InvokeCreateView(Type viewType)
    {
        var method = typeof(AppBuilderExtensions)
            .GetMethod("CreateView", BindingFlags.NonPublic | BindingFlags.Static);

        try
        {
            return method!.Invoke(null, [viewType])!;
        }
        catch (TargetInvocationException error) when (error.InnerException is not null)
        {
            ExceptionDispatchInfo.Capture(error.InnerException).Throw();
            throw;
        }
    }

    /// <summary>Invokes the private entry-assembly registration helper.</summary>
    /// <param name="builder">The application builder.</param>
    /// <param name="entryAssembly">The entry assembly to pass.</param>
    /// <returns>The returned application builder.</returns>
    private static AppBuilder InvokeRegisterReactiveUIViewsFromEntryAssembly(AppBuilder builder, Assembly? entryAssembly)
    {
        var method = typeof(AppBuilderExtensions)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .Single(candidate =>
                candidate.Name == "RegisterReactiveUIViewsFromEntryAssembly" &&
                candidate.GetParameters() is [{ ParameterType: var builderType }, { ParameterType: var assemblyType }] &&
                builderType == typeof(AppBuilder) &&
                assemblyType == typeof(Assembly));

        return (AppBuilder)method.Invoke(null, [builder, entryAssembly])!;
    }

    /// <summary>Invokes the private guarded view registration helper.</summary>
    /// <param name="resolver">The resolver to register with.</param>
    /// <param name="assemblies">The assemblies to scan.</param>
    private static void InvokeRegisterReactiveUIViews(IMutableDependencyResolver? resolver, Assembly[]? assemblies)
    {
        var method = typeof(AppBuilderExtensions)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .Single(candidate =>
                candidate.Name == "RegisterReactiveUIViews" &&
                candidate.GetParameters() is [{ ParameterType: var resolverType }, { ParameterType: var assembliesType }] &&
                resolverType == typeof(IMutableDependencyResolver) &&
                assembliesType == typeof(Assembly[]));

        _ = method.Invoke(null, [resolver, assemblies]);
    }

    /// <summary>A test view model implementing ITestVm.</summary>
    public sealed class TestVm : ReactiveObject, ITestVm
    {
        /// <inheritdoc/>
        public object? TestContract => null;
    }

    /// <summary>Attribute to specify a view contract name.</summary>
    /// <param name="contract">The contract name.</param>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ViewContractAttribute(string contract) : Attribute
    {
        /// <summary>Gets the contract name.</summary>
        public string Contract { get; } = contract;
    }

    /// <summary>A test view for TestVm without a contract.</summary>
    public sealed class TestView : UserControl, IViewFor<TestVm>
    {
        /// <summary>Gets or sets the view model.</summary>
        public TestVm? ViewModel { get; set; }

        /// <inheritdoc/>
        object? IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (TestVm?)value;
        }
    }

    /// <summary>A test view for TestVm with contract "C1".</summary>
    [ViewContract("C1")]
    public sealed class ContractedTestView : UserControl, IViewFor<TestVm>
    {
        /// <summary>Gets or sets the view model.</summary>
        public TestVm? ViewModel { get; set; }

        /// <inheritdoc/>
        object? IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (TestVm?)value;
        }
    }

    /// <summary>A DI-backed test view for TestVm.</summary>
    public sealed class DiBackedView : UserControl, IViewFor<TestVm>
    {
        /// <summary>Gets or sets the view model.</summary>
        public TestVm? ViewModel { get; set; }

        /// <inheritdoc/>
        object? IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (TestVm?)value;
        }
    }

    /// <summary>A view model for a view decorated by an attribute without a Contract property.</summary>
    public sealed class NoContractVm : ReactiveObject;

    /// <summary>A view with an attribute named ViewContractAttribute that has no Contract property.</summary>
    [MissingContract.ViewContract]
    public sealed class NoContractView : UserControl, IViewFor<NoContractVm>
    {
        /// <summary>Gets or sets the view model.</summary>
        public NoContractVm? ViewModel { get; set; }

        /// <inheritdoc/>
        object? IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (NoContractVm?)value;
        }
    }

    /// <summary>A view used to validate Activator fallback.</summary>
    public sealed class FallbackView : UserControl;

    /// <summary>A distinct view model used to validate duplicate assembly handling.</summary>
    public sealed class DistinctRegistrationVm : ReactiveObject;

    /// <summary>A distinct view used to validate duplicate assembly handling.</summary>
    public sealed class DistinctRegistrationView : UserControl, IViewFor<DistinctRegistrationVm>
    {
        /// <summary>Gets or sets the view model.</summary>
        public DistinctRegistrationVm? ViewModel { get; set; }

        /// <inheritdoc/>
        object? IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (DistinctRegistrationVm?)value;
        }
    }

    /// <summary>Contains an attribute named ViewContractAttribute without a Contract property.</summary>
    private static class MissingContract
    {
        /// <summary>An attribute that intentionally has no Contract property.</summary>
        [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
        public sealed class ViewContractAttribute : Attribute;
    }
}
