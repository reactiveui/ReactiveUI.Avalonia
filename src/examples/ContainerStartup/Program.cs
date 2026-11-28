// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Autofac;
using Avalonia;
using Avalonia.Headless;
using DryIoc;
using Microsoft.Extensions.DependencyInjection;
using Splat;

namespace ReactiveUI.Avalonia.Example.ContainerStartup;

/// <summary>Starts Avalonia with a chosen container and checks an application service after startup.</summary>
internal static class Program
{
    /// <summary>Registers the dispatcher's clock and looks it up through Splat.</summary>
    /// <param name="args">A container name. The default is microsoft.</param>
    /// <exception cref="ArgumentException">The container name is unknown.</exception>
    /// <exception cref="InvalidOperationException">The application clock was unavailable after startup.</exception>
    internal static void Main(string[] args)
    {
        var builder = AppBuilder.Configure<Application>().UseHeadless(new());
        var configured = args switch
        {
            [] or ["microsoft"] => builder.UseReactiveUIWithMicrosoftDependencyResolver(
                static services => services.AddSingleton(TimeProvider.System)),
            ["autofac"] => builder.UseReactiveUIWithAutofac(
                static container => container.RegisterInstance(TimeProvider.System)),
            ["dryioc"] => builder.UseReactiveUIWithDryIoc(
                static container => container.RegisterInstance(TimeProvider.System)),
            ["ninject"] => builder.UseReactiveUIWithNinject(
                static kernel => kernel.Bind<TimeProvider>().ToConstant(TimeProvider.System)),
            _ => throw new ArgumentException("Choose microsoft, autofac, dryioc or ninject.", nameof(args)),
        };

        _ = configured.SetupWithoutStarting();
        if (!ReferenceEquals(AppLocator.Current.GetService<TimeProvider>(), TimeProvider.System))
        {
            throw new InvalidOperationException("The selected container did not supply the registered clock.");
        }
    }
}
