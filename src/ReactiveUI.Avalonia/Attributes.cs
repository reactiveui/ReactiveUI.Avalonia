// Copyright (c) 2019-2026 ReactiveUI and Avalonia Teams, and Contributors. All rights reserved.
// Licensed under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Avalonia.Metadata;

[assembly: XmlnsDefinition("http://reactiveui.net", "ReactiveUI.Avalonia")]

// Allow test projects to access internal types for unit testing
[assembly: InternalsVisibleTo("ReactiveUI.Avalonia.Tests")]
[assembly: InternalsVisibleTo("ReactiveUI.Avalonia.Microsoft.Tests")]
[assembly: InternalsVisibleTo("ReactiveUI.Avalonia.DryIoc.Tests")]
[assembly: InternalsVisibleTo("ReactiveUI.Avalonia.Autofac")]
[assembly: InternalsVisibleTo("ReactiveUI.Avalonia.DryIoc")]
[assembly: InternalsVisibleTo("ReactiveUI.Avalonia.Microsoft.Extensions.DependencyInjection")]
[assembly: InternalsVisibleTo("ReactiveUI.Avalonia.Ninject")]
