// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace ReactiveUI.Avalonia.Reactive.Tests;

/// <summary>Provides a controlled desktop lifetime implementation for shutdown tests.</summary>
public sealed class ControlledLifetimeFixture
{
    /// <summary>Method attributes used for emitted interface implementations.</summary>
    private const MethodAttributes InterfaceImplementationAttributes = MethodAttributes.Public
        | MethodAttributes.Virtual
        | MethodAttributes.Final
        | MethodAttributes.HideBySig;

    /// <summary>Stores event handlers for each dynamic lifetime instance.</summary>
    private static readonly ConditionalWeakTable<object, LifetimeEvents> s_events = [];

    /// <summary>Stores the generated lifetime implementation type.</summary>
    private static readonly Lazy<Type> s_lifetimeType = new(CreateLifetimeType);

    /// <summary>Initializes a new instance of the <see cref="ControlledLifetimeFixture"/> class.</summary>
    public ControlledLifetimeFixture() =>
        Lifetime = (IClassicDesktopStyleApplicationLifetime)Activator.CreateInstance(s_lifetimeType.Value)!;

    /// <summary>Gets the dynamically generated desktop lifetime.</summary>
    public IClassicDesktopStyleApplicationLifetime Lifetime { get; }

    /// <summary>Adds a startup event handler to the dynamic lifetime.</summary>
    /// <param name="instance">The dynamic lifetime instance.</param>
    /// <param name="handler">The handler to add.</param>
    public static void AddStartup(object instance, EventHandler<ControlledApplicationLifetimeStartupEventArgs> handler) =>
        GetEvents(instance).AddStartup(handler);

    /// <summary>Removes a startup event handler from the dynamic lifetime.</summary>
    /// <param name="instance">The dynamic lifetime instance.</param>
    /// <param name="handler">The handler to remove.</param>
    public static void RemoveStartup(object instance, EventHandler<ControlledApplicationLifetimeStartupEventArgs> handler) =>
        GetEvents(instance).RemoveStartup(handler);

    /// <summary>Adds an exit event handler to the dynamic lifetime.</summary>
    /// <param name="instance">The dynamic lifetime instance.</param>
    /// <param name="handler">The handler to add.</param>
    public static void AddExit(object instance, EventHandler<ControlledApplicationLifetimeExitEventArgs> handler) =>
        GetEvents(instance).AddExit(handler);

    /// <summary>Removes an exit event handler from the dynamic lifetime.</summary>
    /// <param name="instance">The dynamic lifetime instance.</param>
    /// <param name="handler">The handler to remove.</param>
    public static void RemoveExit(object instance, EventHandler<ControlledApplicationLifetimeExitEventArgs> handler) =>
        GetEvents(instance).RemoveExit(handler);

    /// <summary>Adds a shutdown-requested event handler to the dynamic lifetime.</summary>
    /// <param name="instance">The dynamic lifetime instance.</param>
    /// <param name="handler">The handler to add.</param>
    public static void AddShutdownRequested(object instance, EventHandler<ShutdownRequestedEventArgs> handler) =>
        GetEvents(instance).AddShutdownRequested(handler);

    /// <summary>Removes a shutdown-requested event handler from the dynamic lifetime.</summary>
    /// <param name="instance">The dynamic lifetime instance.</param>
    /// <param name="handler">The handler to remove.</param>
    public static void RemoveShutdownRequested(object instance, EventHandler<ShutdownRequestedEventArgs> handler) =>
        GetEvents(instance).RemoveShutdownRequested(handler);

    /// <summary>Raises the exit event on a dynamic lifetime instance.</summary>
    /// <param name="instance">The dynamic lifetime instance.</param>
    /// <param name="exitCode">The exit code to publish.</param>
    public static void RaiseExit(object instance, int exitCode) => GetEvents(instance).RaiseExit(instance, exitCode);

    /// <summary>Raises the exit event on the dynamic lifetime.</summary>
    public void RaiseExit() => RaiseExit(Lifetime, 0);

    /// <summary>Raises the exit event on the dynamic lifetime.</summary>
    /// <param name="exitCode">The exit code to publish.</param>
    public void RaiseExit(int exitCode) => RaiseExit(Lifetime, exitCode);

    /// <summary>Gets the event store for a dynamic lifetime instance.</summary>
    /// <param name="instance">The dynamic lifetime instance.</param>
    /// <returns>The event store associated with the instance.</returns>
    private static LifetimeEvents GetEvents(object instance) => s_events.GetValue(instance, static _ => new LifetimeEvents());

    /// <summary>Creates the dynamic lifetime implementation type.</summary>
    /// <returns>A type implementing the Avalonia desktop lifetime interface.</returns>
    private static Type CreateLifetimeType()
    {
        var assembly = AssemblyBuilder.DefineDynamicAssembly(
            new("ReactiveUI.Avalonia.Reactive.Tests.ControlledLifetime"),
            AssemblyBuilderAccess.Run);
        var module = assembly.DefineDynamicModule("Main");
        var type = module.DefineType(
            "ControlledDesktopStyleApplicationLifetime",
            TypeAttributes.Public | TypeAttributes.Sealed);
        type.AddInterfaceImplementation(typeof(IClassicDesktopStyleApplicationLifetime));

        ImplementMethodCall(type, typeof(IControlledApplicationLifetime).GetMethod("add_Startup")!, nameof(AddStartup));
        ImplementMethodCall(type, typeof(IControlledApplicationLifetime).GetMethod("remove_Startup")!, nameof(RemoveStartup));
        ImplementMethodCall(type, typeof(IControlledApplicationLifetime).GetMethod("add_Exit")!, nameof(AddExit));
        ImplementMethodCall(type, typeof(IControlledApplicationLifetime).GetMethod("remove_Exit")!, nameof(RemoveExit));
        ImplementMethodCall(
            type,
            typeof(IControlledApplicationLifetime).GetMethod(nameof(IControlledApplicationLifetime.Shutdown))!,
            nameof(RaiseExit));
        ImplementTryShutdown(type);
        ImplementStringArrayGetter(type, nameof(IClassicDesktopStyleApplicationLifetime.Args));
        ImplementShutdownModeProperty(type);
        ImplementMainWindowProperty(type);
        ImplementWindowsGetter(type);
        ImplementMethodCall(
            type,
            typeof(IClassicDesktopStyleApplicationLifetime).GetMethod("add_ShutdownRequested")!,
            nameof(AddShutdownRequested));
        ImplementMethodCall(
            type,
            typeof(IClassicDesktopStyleApplicationLifetime).GetMethod("remove_ShutdownRequested")!,
            nameof(RemoveShutdownRequested));

        return type.CreateType();
    }

    /// <summary>Implements an interface method by forwarding to a static helper method.</summary>
    /// <param name="type">The type builder to update.</param>
    /// <param name="interfaceMethod">The interface method to implement.</param>
    /// <param name="targetName">The target static helper method name.</param>
    private static void ImplementMethodCall(TypeBuilder type, MethodInfo interfaceMethod, string targetName)
    {
        var parameterTypes = GetParameterTypes(interfaceMethod);
        var method = type.DefineMethod(
            interfaceMethod.Name,
            GetImplementationAttributes(interfaceMethod),
            interfaceMethod.ReturnType,
            parameterTypes);
        var il = method.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        for (var i = 0; i < parameterTypes.Length; i++)
        {
            il.Emit(OpCodes.Ldarg, i + 1);
        }

        il.Emit(OpCodes.Call, GetStaticHelper(targetName, parameterTypes));
        il.Emit(OpCodes.Ret);
        type.DefineMethodOverride(method, interfaceMethod);
    }

    /// <summary>Implements the desktop lifetime TryShutdown method.</summary>
    /// <param name="type">The type builder to update.</param>
    private static void ImplementTryShutdown(TypeBuilder type)
    {
        var interfaceMethod = typeof(IClassicDesktopStyleApplicationLifetime).GetMethod(
            nameof(IClassicDesktopStyleApplicationLifetime.TryShutdown))!;
        var method = type.DefineMethod(
            interfaceMethod.Name,
            GetImplementationAttributes(interfaceMethod),
            typeof(bool),
            [typeof(int)]);
        var il = method.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Call, typeof(ControlledLifetimeFixture).GetMethod(nameof(RaiseExit), [typeof(object), typeof(int)])!);
        il.Emit(OpCodes.Ldc_I4_1);
        il.Emit(OpCodes.Ret);
        type.DefineMethodOverride(method, interfaceMethod);
    }

    /// <summary>Implements a string array property getter.</summary>
    /// <param name="type">The type builder to update.</param>
    /// <param name="propertyName">The property name.</param>
    private static void ImplementStringArrayGetter(TypeBuilder type, string propertyName)
    {
        var interfaceMethod = typeof(IClassicDesktopStyleApplicationLifetime).GetMethod($"get_{propertyName}")!;
        var method = type.DefineMethod(
            interfaceMethod.Name,
            GetImplementationAttributes(interfaceMethod),
            typeof(string[]),
            Type.EmptyTypes);
        var il = method.GetILGenerator();
        il.Emit(OpCodes.Call, typeof(Array).GetMethod(nameof(Array.Empty))!.MakeGenericMethod(typeof(string)));
        il.Emit(OpCodes.Ret);
        type.DefineMethodOverride(method, interfaceMethod);
    }

    /// <summary>Implements the ShutdownMode property.</summary>
    /// <param name="type">The type builder to update.</param>
    private static void ImplementShutdownModeProperty(TypeBuilder type)
    {
        var field = type.DefineField("_shutdownMode", typeof(ShutdownMode), FieldAttributes.Private);
        ImplementFieldGetter(
            type,
            typeof(IClassicDesktopStyleApplicationLifetime).GetMethod(
                $"get_{nameof(IClassicDesktopStyleApplicationLifetime.ShutdownMode)}")!,
            field);
        ImplementFieldSetter(
            type,
            typeof(IClassicDesktopStyleApplicationLifetime).GetMethod(
                $"set_{nameof(IClassicDesktopStyleApplicationLifetime.ShutdownMode)}")!,
            field);
    }

    /// <summary>Implements the MainWindow property.</summary>
    /// <param name="type">The type builder to update.</param>
    private static void ImplementMainWindowProperty(TypeBuilder type)
    {
        var field = type.DefineField("_mainWindow", typeof(Window), FieldAttributes.Private);
        ImplementFieldGetter(
            type,
            typeof(IClassicDesktopStyleApplicationLifetime).GetMethod(
                $"get_{nameof(IClassicDesktopStyleApplicationLifetime.MainWindow)}")!,
            field);
        ImplementFieldSetter(
            type,
            typeof(IClassicDesktopStyleApplicationLifetime).GetMethod(
                $"set_{nameof(IClassicDesktopStyleApplicationLifetime.MainWindow)}")!,
            field);
    }

    /// <summary>Implements a field-backed getter.</summary>
    /// <param name="type">The type builder to update.</param>
    /// <param name="interfaceMethod">The interface getter to implement.</param>
    /// <param name="field">The backing field.</param>
    private static void ImplementFieldGetter(TypeBuilder type, MethodInfo interfaceMethod, FieldInfo field)
    {
        var method = type.DefineMethod(
            interfaceMethod.Name,
            GetImplementationAttributes(interfaceMethod),
            interfaceMethod.ReturnType,
            Type.EmptyTypes);
        var il = method.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldfld, field);
        il.Emit(OpCodes.Ret);
        type.DefineMethodOverride(method, interfaceMethod);
    }

    /// <summary>Implements a field-backed setter.</summary>
    /// <param name="type">The type builder to update.</param>
    /// <param name="interfaceMethod">The interface setter to implement.</param>
    /// <param name="field">The backing field.</param>
    private static void ImplementFieldSetter(TypeBuilder type, MethodInfo interfaceMethod, FieldInfo field)
    {
        var method = type.DefineMethod(
            interfaceMethod.Name,
            GetImplementationAttributes(interfaceMethod),
            typeof(void),
            [field.FieldType]);
        var il = method.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Stfld, field);
        il.Emit(OpCodes.Ret);
        type.DefineMethodOverride(method, interfaceMethod);
    }

    /// <summary>Implements the Windows property getter.</summary>
    /// <param name="type">The type builder to update.</param>
    private static void ImplementWindowsGetter(TypeBuilder type)
    {
        var interfaceMethod = typeof(IClassicDesktopStyleApplicationLifetime).GetMethod(
            $"get_{nameof(IClassicDesktopStyleApplicationLifetime.Windows)}")!;
        var method = type.DefineMethod(
            interfaceMethod.Name,
            GetImplementationAttributes(interfaceMethod),
            interfaceMethod.ReturnType,
            Type.EmptyTypes);
        var il = method.GetILGenerator();
        il.Emit(OpCodes.Call, typeof(Array).GetMethod(nameof(Array.Empty))!.MakeGenericMethod(typeof(Window)));
        il.Emit(OpCodes.Ret);
        type.DefineMethodOverride(method, interfaceMethod);
    }

    /// <summary>Gets implementation method attributes for an interface method.</summary>
    /// <param name="interfaceMethod">The interface method to implement.</param>
    /// <returns>The method attributes to use for the emitted method.</returns>
    private static MethodAttributes GetImplementationAttributes(MethodInfo interfaceMethod)
    {
        var attributes = InterfaceImplementationAttributes;
        if (interfaceMethod.IsSpecialName)
        {
            attributes |= MethodAttributes.SpecialName;
        }

        return attributes;
    }

    /// <summary>Gets a static helper method with an emitted-instance leading parameter.</summary>
    /// <param name="targetName">The target method name.</param>
    /// <param name="parameterTypes">The interface parameter types.</param>
    /// <returns>The matching static helper method.</returns>
    private static MethodInfo GetStaticHelper(string targetName, Type[] parameterTypes)
    {
        var helperParameterTypes = new Type[parameterTypes.Length + 1];
        helperParameterTypes[0] = typeof(object);
        for (var i = 0; i < parameterTypes.Length; i++)
        {
            helperParameterTypes[i + 1] = parameterTypes[i];
        }

        return typeof(ControlledLifetimeFixture).GetMethod(targetName, helperParameterTypes)!;
    }

    /// <summary>Gets parameter types for a reflected method.</summary>
    /// <param name="method">The method to inspect.</param>
    /// <returns>The method parameter types.</returns>
    private static Type[] GetParameterTypes(MethodInfo method)
    {
        var parameters = method.GetParameters();
        var parameterTypes = new Type[parameters.Length];
        for (var i = 0; i < parameters.Length; i++)
        {
            parameterTypes[i] = parameters[i].ParameterType;
        }

        return parameterTypes;
    }

    /// <summary>Stores event handlers for a dynamic lifetime instance.</summary>
    private sealed class LifetimeEvents
    {
        /// <summary>Stores startup handlers for the dynamic lifetime.</summary>
        private EventHandler<ControlledApplicationLifetimeStartupEventArgs>? _startupHandler;

        /// <summary>Stores exit handlers for the dynamic lifetime.</summary>
        private EventHandler<ControlledApplicationLifetimeExitEventArgs>? _exitHandler;

        /// <summary>Stores shutdown-requested handlers for the dynamic lifetime.</summary>
        private EventHandler<ShutdownRequestedEventArgs>? _shutdownRequestedHandler;

        /// <summary>Adds a startup handler.</summary>
        /// <param name="handler">The handler to add.</param>
        public void AddStartup(EventHandler<ControlledApplicationLifetimeStartupEventArgs> handler) =>
            _startupHandler += handler;

        /// <summary>Removes a startup handler.</summary>
        /// <param name="handler">The handler to remove.</param>
        public void RemoveStartup(EventHandler<ControlledApplicationLifetimeStartupEventArgs> handler) =>
            _startupHandler -= handler;

        /// <summary>Adds an exit handler.</summary>
        /// <param name="handler">The handler to add.</param>
        public void AddExit(EventHandler<ControlledApplicationLifetimeExitEventArgs> handler) =>
            _exitHandler += handler;

        /// <summary>Removes an exit handler.</summary>
        /// <param name="handler">The handler to remove.</param>
        public void RemoveExit(EventHandler<ControlledApplicationLifetimeExitEventArgs> handler) =>
            _exitHandler -= handler;

        /// <summary>Adds a shutdown-requested handler.</summary>
        /// <param name="handler">The handler to add.</param>
        public void AddShutdownRequested(EventHandler<ShutdownRequestedEventArgs> handler) =>
            _shutdownRequestedHandler += handler;

        /// <summary>Removes a shutdown-requested handler.</summary>
        /// <param name="handler">The handler to remove.</param>
        public void RemoveShutdownRequested(EventHandler<ShutdownRequestedEventArgs> handler) =>
            _shutdownRequestedHandler -= handler;

        /// <summary>Raises the stored exit handlers.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="exitCode">The exit code to publish.</param>
        public void RaiseExit(object sender, int exitCode) => _exitHandler?.Invoke(sender, new(exitCode));
    }
}
