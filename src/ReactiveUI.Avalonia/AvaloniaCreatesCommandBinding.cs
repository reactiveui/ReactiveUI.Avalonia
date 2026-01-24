// Copyright (c) 2025 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

namespace ReactiveUI.Avalonia;

/// <summary>
/// Provides command binding creation logic for Avalonia input elements, enabling commands to be bound to UI controls
/// and routed events at runtime.
/// </summary>
/// <remarks>This class is intended for internal use within the Avalonia framework to support command binding
/// scenarios, such as associating commands with buttons, menu items, or other input elements. It implements the
/// ICreatesCommandBinding interface to facilitate binding commands to objects and events, handling both ICommandSource
/// types and routed event handlers. Thread safety and usage details are determined by the consuming framework
/// components.</remarks>
internal class AvaloniaCreatesCommandBinding : ICreatesCommandBinding
{
    /// <summary>
    /// Calculates an affinity score indicating how suitable the specified type is for data binding in input scenarios.
    /// </summary>
    /// <remarks>A higher affinity score suggests that the type is more appropriate for binding in input or
    /// command scenarios. This method provides best-effort support for event-based bindings and prioritizes command
    /// source types when no event target is present.</remarks>
    /// <typeparam name="T">The type of object to evaluate for binding affinity. Typically an input element or command source.</typeparam>
    /// <param name="hasEventTarget">Indicates whether the binding target has an associated event handler. If <see langword="true"/>, the method
    /// returns a score reflecting event-based binding suitability.</param>
    /// <returns>An integer representing the binding affinity score for the specified type. Returns 0 if the type is not an input
    /// element; returns 6 if the type is an input element with an event target; returns 10 if the type is a command
    /// source input element without an event target.</returns>
    public int GetAffinityForObject<T>(bool hasEventTarget)
    {
        var isInputElement = typeof(InputElement).IsAssignableFrom(typeof(T));
        if (!isInputElement)
        {
            return 0;
        }

        if (hasEventTarget)
        {
            // This method doesn't know which event we are going to bind.
            // Best effort support.
            return 6;
        }

        // Command/CommandParameter bindings is only available on ICommandSource-types (usually Buttons and MenuItem).
        var isCommandSource = typeof(ICommandSource).IsAssignableFrom(typeof(T));
        return isCommandSource ? 10 : 0;
    }

    /// <summary>
    /// Binds the specified command to the given target object, updating the command parameter dynamically as the
    /// observable emits new values.
    /// </summary>
    /// <remarks>Disposing the returned IDisposable will remove the command binding and stop updates to the
    /// command parameter. This method is typically used to enable dynamic command parameter updates in UI elements such
    /// as buttons or menu items.</remarks>
    /// <typeparam name="T">The type of the target object. Must be a class that is both an InputElement and implements ICommandSource.</typeparam>
    /// <param name="command">The command to bind to the target object. Cannot be null.</param>
    /// <param name="target">The object to which the command will be bound. Must be an InputElement and implement ICommandSource. Cannot be
    /// null.</param>
    /// <param name="commandParameter">An observable sequence that provides values for the command parameter. The command parameter will be updated
    /// whenever the observable emits a new value.</param>
    /// <returns>An IDisposable that, when disposed, unbinds the command and command parameter from the target object.</returns>
    /// <exception cref="ArgumentNullException">Thrown if either the command or target parameter is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the target object does not implement both InputElement and ICommandSource.</exception>
    public IDisposable? BindCommandToObject<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicEvents | DynamicallyAccessedMemberTypes.NonPublicEvents)] T>(
        ICommand? command,
        T? target,
        IObservable<object?> commandParameter)
        where T : class
    {
        if (command is null)
        {
            throw new ArgumentNullException(nameof(command));
        }

        if (target is null)
        {
            throw new ArgumentNullException(nameof(target));
        }

        if (target is not (InputElement element and ICommandSource))
        {
            throw new InvalidOperationException("Target must be an InputElement and implement ICommandSource.");
        }

        // Button.CommandProperty is reused for all button-like controls and menu item
        element.SetCurrentValue(Button.CommandProperty, command);
        var paramDisposable = element.Bind(Button.CommandParameterProperty, commandParameter);
        return Disposable.Create((avaloniaObject: element, paramDisposable), static (t) =>
        {
            t.paramDisposable.Dispose();
            t.avaloniaObject.ClearValue(Button.CommandProperty);
        });
    }

    /// <summary>
    /// Binds the specified command to an event on the target object, enabling command execution when the event is
    /// raised.
    /// </summary>
    /// <remarks>Disposing the returned IDisposable will detach the event handler and unbind the command. This
    /// method is typically used to enable MVVM-style command binding to UI events.</remarks>
    /// <typeparam name="T">The type of the target object to which the command is bound. Must be a class and an InputElement.</typeparam>
    /// <typeparam name="TEventArgs">The type of the event arguments associated with the event.</typeparam>
    /// <param name="command">The command to execute when the event is raised. Cannot be null.</param>
    /// <param name="target">The target object on which to bind the event. Must be an InputElement and cannot be null.</param>
    /// <param name="commandParameter">An observable sequence that provides the parameter to pass to the command when it is executed.</param>
    /// <param name="eventName">The name of the event on the target object to bind to. Must correspond to a routed event on the target.</param>
    /// <returns>An IDisposable instance that can be used to unbind the command from the event. Returns null if binding is
    /// unsuccessful.</returns>
    /// <exception cref="ArgumentNullException">Thrown if either the command or target parameter is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the target is not an InputElement, or if the specified event is not found on the target object.</exception>
    public IDisposable? BindCommandToObject<T, TEventArgs>(
        ICommand? command,
        T? target,
        IObservable<object?> commandParameter,
        string eventName)
        where T : class
    {
        if (command is null)
        {
            throw new ArgumentNullException(nameof(command));
        }

        if (target is null)
        {
            throw new ArgumentNullException(nameof(target));
        }

        if (target is not InputElement element)
        {
            throw new InvalidOperationException("Target must be an InputElement.");
        }

        var routedEvent = FindRoutedEvent(target, eventName) ?? throw new InvalidOperationException($"Routed Event {eventName} not found on {target.GetType().Name} element.");
        return new RoutedEventSubscriptionClosure(element, routedEvent, command, commandParameter);
    }

    /// <summary>
    /// Binds the specified command to an event on the target object, enabling command execution when the event is
    /// raised.
    /// </summary>
    /// <remarks>The returned <see cref="IDisposable"/> should be disposed to clean up event subscriptions and
    /// prevent memory leaks. This method is typically used to facilitate command binding in MVVM scenarios where UI
    /// events trigger command execution.</remarks>
    /// <typeparam name="T">The type of the target object to which the command is bound. Must be a reference type.</typeparam>
    /// <typeparam name="TEventArgs">The type of the event arguments associated with the event handler. Must derive from <see cref="EventArgs"/>.</typeparam>
    /// <param name="command">The command to execute when the event is raised. If <see langword="null"/>, no command will be executed.</param>
    /// <param name="target">The object whose event will be monitored for command execution. Must not be <see langword="null"/>.</param>
    /// <param name="commandParameter">An observable sequence that provides the parameter to pass to the command when it is executed.</param>
    /// <param name="addHandler">An action that attaches the event handler to the target object's event.</param>
    /// <param name="removeHandler">An action that detaches the event handler from the target object's event.</param>
    /// <returns>An <see cref="IDisposable"/> instance that, when disposed, unbinds the command from the event. Returns <see
    /// langword="null"/> if binding could not be established.</returns>
    /// <exception cref="NotImplementedException">Thrown in all cases as the method is not implemented.</exception>
    public IDisposable? BindCommandToObject<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicEvents | DynamicallyAccessedMemberTypes.NonPublicEvents)] T,
        TEventArgs>(
        ICommand? command,
        T? target,
        IObservable<object?> commandParameter,
        Action<EventHandler<TEventArgs>> addHandler,
        Action<EventHandler<TEventArgs>> removeHandler)
        where T : class
        where TEventArgs : EventArgs => Disposable.Empty;

    /// <summary>
    /// Searches the type hierarchy of the specified target for a routed event with the given name.
    /// </summary>
    /// <remarks>This method searches for routed events declared on the target's type and its base types. Use
    /// this method to locate events such as Button.Click when working with derived types.</remarks>
    /// <param name="target">The object whose type hierarchy is searched for the routed event. Must be an instance of a type derived from
    /// InputElement.</param>
    /// <param name="eventName">The name of the routed event to locate. The comparison is case-sensitive.</param>
    /// <returns>The RoutedEvent instance matching the specified name if found; otherwise, null.</returns>
    private static RoutedEvent? FindRoutedEvent(object target, string eventName)
    {
        // Search the type hierarchy so events declared on base classes (e.g., Button.Click on Button
        // when the target is a derived type) can be located.
        for (var type = target.GetType(); type is not null && typeof(InputElement).IsAssignableFrom(type); type = type.BaseType)
        {
            foreach (var routedEvent in RoutedEventRegistry.Instance.GetRegistered(type))
            {
                if (string.Equals(routedEvent.Name, eventName, StringComparison.Ordinal))
                {
                    return routedEvent;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Manages the subscription of a routed event to a command, enabling automatic command execution when the event is
    /// raised and updating the enabled state of the associated input element based on the command's ability to execute.
    /// </summary>
    /// <remarks>This class encapsulates the logic required to bind a routed event to a command, including
    /// tracking command parameters and handling the enabled state of the input element. It is intended for internal use
    /// to facilitate event-to-command binding scenarios. Instances of this class should be disposed when the
    /// subscription is no longer needed to release event handlers and resources.</remarks>
    private sealed class RoutedEventSubscriptionClosure : IDisposable
    {
        private readonly InputElement _element;
        private readonly RoutedEvent _routedEvent;
        private readonly ICommand _command;
        private readonly IDisposable _commandSubscription;
        private object? _lastCommandParameter;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoutedEventSubscriptionClosure"/> class, subscribing the specified element to.
        /// a routed event and binding the event to a command with an observable command parameter.
        /// </summary>
        /// <remarks>This constructor attaches an event handler to the specified element for the given
        /// routed event using bubble routing. The command parameter is dynamically updated based on the observable's
        /// values. All provided arguments must be non-null; otherwise, an exception may be thrown.</remarks>
        /// <param name="element">The input element to associate with the routed event subscription. Cannot be null.</param>
        /// <param name="routedEvent">The routed event to subscribe to on the specified element. Cannot be null.</param>
        /// <param name="command">The command to execute when the routed event is raised. Cannot be null.</param>
        /// <param name="commandParameter">An observable that provides the parameter to pass to the command when executed. Cannot be null.</param>
        public RoutedEventSubscriptionClosure(
            InputElement element,
            RoutedEvent routedEvent,
            ICommand command,
            IObservable<object?> commandParameter)
        {
            _element = element;
            _routedEvent = routedEvent;
            _command = command;
            _commandSubscription = commandParameter.Subscribe(OnCommandParameterChanged);
            element.AddHandler(routedEvent, Handler, RoutingStrategies.Bubble);
        }

        /// <summary>
        /// Handles the routed event by invoking the associated command if it can be executed.
        /// </summary>
        /// <remarks>This method is intended to be used as an event handler for UI elements that trigger
        /// command execution. The command is executed only if its CanExecute method returns <see langword="true"/> for
        /// the last command parameter.</remarks>
        /// <param name="sender">The source of the event, typically the control that raised the routed event.</param>
        /// <param name="args">The event data associated with the routed event.</param>
        public void Handler(object? sender, RoutedEventArgs args)
        {
            if (_command.CanExecute(_lastCommandParameter))
            {
                _command.Execute(_lastCommandParameter);
            }
        }

        /// <summary>
        /// Releases all resources used by the instance and detaches event handlers to prevent memory leaks.
        /// </summary>
        /// <remarks>Call this method when the instance is no longer needed to ensure that event handlers
        /// are removed and resources are properly released. After calling <see cref="Dispose"/>, the instance should
        /// not be used.</remarks>
        public void Dispose()
        {
            _commandSubscription.Dispose();
            _element.RemoveHandler(_routedEvent, Handler);
            _element.ClearValue(InputElement.IsEnabledProperty);
        }

        /// <summary>
        /// Updates the command parameter and sets the enabled state of the associated input element based on whether
        /// the command can execute with the specified parameter.
        /// </summary>
        /// <remarks>This method should be called when the command parameter changes to ensure the input
        /// element's enabled state reflects the command's ability to execute. The enabled state is determined by
        /// invoking the command's CanExecute method with the provided parameter.</remarks>
        /// <param name="value">The new parameter value to evaluate with the command. Can be null if the command does not require a
        /// parameter.</param>
        private void OnCommandParameterChanged(object? value)
        {
            _lastCommandParameter = value;
            _element.SetCurrentValue(InputElement.IsEnabledProperty, _command.CanExecute(_lastCommandParameter));
        }
    }
}
