// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Runtime.CompilerServices;
using Avalonia.Metadata;

[assembly: XmlnsDefinition("http://reactiveui.net", "ReactiveUI.Avalonia")]
[assembly: XmlnsDefinition("http://reactiveui.net", "ReactiveUI.Avalonia.Reactive")]

// Allow test projects to access internal types for unit testing
[assembly: InternalsVisibleTo("ReactiveUI.Avalonia.Tests")]
[assembly: InternalsVisibleTo("ReactiveUI.Avalonia.Microsoft.Tests")]
[assembly: InternalsVisibleTo("ReactiveUI.Avalonia.DryIoc.Tests")]
[assembly: InternalsVisibleTo("ReactiveUI.Avalonia.Reactive.Tests")]
[assembly: InternalsVisibleTo("ReactiveUI.Avalonia.Microsoft.Reactive.Tests")]
[assembly: InternalsVisibleTo("ReactiveUI.Avalonia.DryIoc.Reactive.Tests")]
[assembly: InternalsVisibleTo("ReactiveUI.Avalonia.Autofac")]
[assembly: InternalsVisibleTo("ReactiveUI.Avalonia.DryIoc")]
[assembly: InternalsVisibleTo("ReactiveUI.Avalonia.Microsoft.Extensions.DependencyInjection")]
[assembly: InternalsVisibleTo("ReactiveUI.Avalonia.Ninject")]
[assembly: InternalsVisibleTo("ReactiveUI.Avalonia.Autofac.Reactive")]
[assembly: InternalsVisibleTo("ReactiveUI.Avalonia.DryIoc.Reactive")]
[assembly: InternalsVisibleTo("ReactiveUI.Avalonia.Microsoft.Extensions.DependencyInjection.Reactive")]
[assembly: InternalsVisibleTo("ReactiveUI.Avalonia.Ninject.Reactive")]
