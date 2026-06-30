// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
#if REACTIVE_SHIM
namespace ReactiveUI.Avalonia.Reactive;
#else
namespace ReactiveUI.Avalonia;
#endif

/// <summary>Provides reactive observation of AvaloniaObject property changes using Avalonia's property system.</summary>
/// <remarks>This class is intended for use within reactive UI frameworks to facilitate property change
/// notifications for AvaloniaObject instances. It supports both standard and 'before changed' notifications, and
/// integrates with Avalonia's property registry to identify observable properties. Thread safety and error handling are
/// managed according to Avalonia and ReactiveUI conventions.</remarks>
internal class AvaloniaObjectObservableForProperty : ICreatesObservableForProperty
{
    /// <inheritdoc/>
    [RequiresUnreferencedCode("Uses reflection over runtime types which is not trim- or AOT-safe.")]
    public int GetAffinityForObject(Type type, string propertyName) =>
        GetAffinityForObject(type, propertyName, beforeChanged: false);

    /// <inheritdoc/>
    [RequiresUnreferencedCode("Uses reflection over runtime types which is not trim- or AOT-safe.")]
    public int GetAffinityForObject(Type? type, string propertyName, bool beforeChanged = false)
    {
        if (type is null)
        {
            return 0;
        }

        if (!typeof(AvaloniaObject).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
        {
            return 0;
        }

        return GetAvaloniaProperty(type, propertyName) is not null ? 4 : 0;
    }

    /// <inheritdoc/>
    [RequiresUnreferencedCode("Uses reflection over runtime types which is not trim- or AOT-safe.")]
    public IObservable<IObservedChange<object?, object?>> GetNotificationForProperty(
        object sender,
        Expression expression,
        string propertyName) =>
        GetNotificationForProperty(sender, expression, propertyName, beforeChanged: false, suppressWarnings: false);

    /// <inheritdoc/>
    [RequiresUnreferencedCode("Uses reflection over runtime types which is not trim- or AOT-safe.")]
    public IObservable<IObservedChange<object?, object?>> GetNotificationForProperty(
        object sender,
        Expression expression,
        string propertyName,
        bool beforeChanged) =>
        GetNotificationForProperty(sender, expression, propertyName, beforeChanged, suppressWarnings: false);

    /// <inheritdoc/>
    [RequiresUnreferencedCode("Uses reflection over runtime types which is not trim- or AOT-safe.")]
    public IObservable<IObservedChange<object?, object?>> GetNotificationForProperty(object sender, Expression expression, string propertyName, bool beforeChanged = false, bool suppressWarnings = false)
    {
        ArgumentNullException.ThrowIfNull(sender);

        if (sender is not AvaloniaObject avaloniaObject)
        {
            throw new InvalidOperationException("The sender is not an AvaloniaObject");
        }

        var type = sender.GetType();

        var avaloniaProperty = GetAvaloniaProperty(type, propertyName);

        if (avaloniaProperty is null)
        {
            if (!suppressWarnings)
            {
                this.Log().Error("Couldn't find avalonia property " + propertyName + " on " + type.Name);
            }

            throw new MissingMemberException(type.FullName, propertyName);
        }

        return avaloniaObject
            .GetPropertyChangedObservable(avaloniaProperty)
            .Select(args => new ObservedChange<object?, object?>(args.Sender, expression, args.NewValue));
    }

    /// <summary>Retrieves the registered Avalonia property with the specified name for the given type.</summary>
    /// <param name="type">The type for which to search for the Avalonia property. Must be a registered Avalonia type.</param>
    /// <param name="propertyName">The name of the Avalonia property to retrieve. The comparison is case-sensitive.</param>
    /// <returns>The Avalonia property matching the specified name for the given type, or null if no such property is registered.</returns>
    private static AvaloniaProperty? GetAvaloniaProperty(Type type, string propertyName)
    {
        foreach (var property in AvaloniaPropertyRegistry.Instance.GetRegistered(type))
        {
            if (string.Equals(property.Name, propertyName, StringComparison.Ordinal))
            {
                return property;
            }
        }

        return null;
    }
}
