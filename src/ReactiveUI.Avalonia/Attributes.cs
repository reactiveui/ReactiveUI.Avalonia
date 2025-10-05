// Copyright (c) 2025 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Avalonia.Metadata;

[assembly: XmlnsDefinition("http://reactiveui.net", "ReactiveUI.Avalonia")]

// Allow test projects to access internal types for unit testing
[assembly: InternalsVisibleTo("ReactiveUI.Avalonia.Tests")]
[assembly: InternalsVisibleTo("ReactiveUI.Avalonia.Microsoft.Tests")]
[assembly: InternalsVisibleTo("ReactiveUI.Avalonia.DryIoc.Tests")]
