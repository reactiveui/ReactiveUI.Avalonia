// Copyright (c) 2025 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Avalonia;

/// <summary>
/// Provides extension methods for creating reactive subjects that enable two-way binding between Avalonia properties
/// and observable sequences.
/// </summary>
/// <remarks>These methods facilitate integration between Avalonia's property system and reactive programming
/// paradigms by exposing property values as subjects. This allows developers to observe changes and push updates to
/// properties using standard reactive interfaces. The extension methods are intended for use with Avalonia objects and
/// properties, supporting both simple and binding-aware scenarios.</remarks>
public static class AvaloniaObjectReactiveExtensions
{
    /// <summary>
    /// Creates a reactive subject that synchronizes the value of the specified Avalonia property with observers and
    /// subscribers.
    /// </summary>
    /// <remarks>The returned subject can be used to both observe changes to the property and set its value
    /// reactively. This is useful for integrating Avalonia properties with reactive programming patterns.</remarks>
    /// <param name="o">The Avalonia object whose property value will be observed and updated.</param>
    /// <param name="property">The Avalonia property to observe and set values for.</param>
    /// <param name="priority">The binding priority to use when setting the property value. Defaults to <see
    /// cref="BindingPriority.LocalValue"/>.</param>
    /// <returns>An <see cref="ISubject{Object}"/> that emits changes to the property value and allows updates to the property.</returns>
    public static ISubject<object?> GetSubject(
        this AvaloniaObject o,
        AvaloniaProperty property,
        BindingPriority priority = BindingPriority.LocalValue) => Subject.Create<object?>(
                                       Observer.Create<object?>(x => o.SetValue(property, x, priority)),
                                       o.GetObservable(property));

    /// <summary>
    /// Creates a reactive subject that synchronizes the specified Avalonia property with observable and observer
    /// streams.
    /// </summary>
    /// <remarks>The returned subject allows two-way reactive binding to the specified property. Pushing a
    /// value to the subject updates the property, and changes to the property are emitted by the subject. This is
    /// useful for integrating Avalonia properties with reactive extensions or data streams.</remarks>
    /// <typeparam name="T">The type of the value stored in the Avalonia property.</typeparam>
    /// <param name="o">The Avalonia object whose property will be observed and updated.</param>
    /// <param name="property">The Avalonia property to bind to the subject. Cannot be null.</param>
    /// <param name="priority">The binding priority to use when setting the property value. Defaults to <see
    /// cref="BindingPriority.LocalValue"/>.</param>
    /// <returns>An <see cref="ISubject{T}"/> that emits changes to the property and updates the property when new values are
    /// pushed.</returns>
    public static ISubject<T> GetSubject<T>(
        this AvaloniaObject o,
        AvaloniaProperty<T> property,
        BindingPriority priority = BindingPriority.LocalValue) =>
        Subject.Create<T>(
                          Observer.Create<T>(x => o.SetValue(property, x, priority)),
                          o.GetObservable(property));

    /// <summary>
    /// Creates a reactive subject that allows observing and updating the value of a specified Avalonia property on an
    /// object, using the given binding priority.
    /// </summary>
    /// <remarks>The returned subject can be used to both observe changes to the specified property and set
    /// its value reactively. Setting a value through the subject updates the property on the target object with the
    /// specified priority. This is useful for integrating Avalonia property bindings with reactive programming
    /// patterns.</remarks>
    /// <param name="o">The Avalonia object whose property will be observed and updated.</param>
    /// <param name="property">The Avalonia property to bind to and observe for changes.</param>
    /// <param name="priority">The binding priority to use when setting the property value. Defaults to <see
    /// cref="BindingPriority.LocalValue"/>.</param>
    /// <returns>An ISubject{BindingValue{object}} that emits property value changes and accepts new values to
    /// update the property.</returns>
    public static ISubject<BindingValue<object?>> GetBindingSubject(
        this AvaloniaObject o,
        AvaloniaProperty property,
        BindingPriority priority = BindingPriority.LocalValue) =>
        Subject.Create<BindingValue<object?>>(
                                              Observer.Create<BindingValue<object?>>(x =>
                                              {
                                                  if (x.HasValue)
                                                  {
                                                      o.SetValue(property, x.Value, priority);
                                                  }
                                              }),
                                              o.GetBindingObservable(property));

    /// <summary>
    /// Creates a subject that enables reactive binding to a specified Avalonia property on an object, allowing values
    /// to be observed and set with a given binding priority.
    /// </summary>
    /// <remarks>The returned subject can be used to both observe changes to the property and push new values
    /// to it. Setting a value through the subject updates the property with the specified priority. This is useful for
    /// integrating Avalonia property bindings with reactive programming patterns.</remarks>
    /// <typeparam name="T">The type of the value held by the Avalonia property.</typeparam>
    /// <param name="o">The Avalonia object whose property will be bound and observed.</param>
    /// <param name="property">The Avalonia property to bind to and observe for value changes.</param>
    /// <param name="priority">The binding priority to use when setting the property's value. Defaults to <see
    /// cref="BindingPriority.LocalValue"/>.</param>
    /// <returns>An ISubject{BindingValue{T}} that observes changes to the specified property and allows values to
    /// be set reactively.</returns>
    public static ISubject<BindingValue<T>> GetBindingSubject<T>(
        this AvaloniaObject o,
        AvaloniaProperty<T> property,
        BindingPriority priority = BindingPriority.LocalValue) =>
        Subject.Create<BindingValue<T>>(
                                        Observer.Create<BindingValue<T>>(x =>
                                        {
                                            if (x.HasValue)
                                            {
                                                o.SetValue(property, x.Value, priority);
                                            }
                                        }),
                                        o.GetBindingObservable(property));
}
