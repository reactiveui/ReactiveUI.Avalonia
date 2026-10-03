// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Avalonia;
using ReactiveUI.Avalonia.Splat;
using Splat;

namespace ReactiveUI.Avalonia.Example;

/// <summary>Composes the dependency injection and ReactiveUI setup shared by every platform head.</summary>
public static class ExampleAppBuilderExtensions
{
    /// <summary>Creates a builder for <see cref="App"/> with the shared showcase setup and no platform backend.</summary>
    /// <returns>The configured app builder. Each head adds its own platform backend.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AppBuilder Create() => AppBuilder.Configure<App>().UseShowcase();

    /// <summary>Extends <see cref="AppBuilder"/> with the shared showcase setup.</summary>
    /// <param name="builder">The builder to configure.</param>
    extension(AppBuilder builder)
    {
        /// <summary>Adds the Inter font, Microsoft dependency resolver integration and view registrations.</summary>
        /// <returns>The same builder, for chaining.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AppBuilder UseShowcase() =>
            builder
                .WithInterFont()
                .UseReactiveUIWithMicrosoftDependencyResolver(
                    static _ => App.RegisterViews(AppLocator.CurrentMutable),
                    null,
                    static services => services.WithCoreServices());
    }
}
