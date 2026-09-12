// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI;

namespace ReactiveUI.Avalonia.Example.ViewModels;

/// <summary>Base routed page view model.</summary>
[System.Diagnostics.DebuggerDisplay("PageViewModel: {UrlPathSegment}")]
public class PageViewModel : ViewModelBase, IRoutableViewModel
{
    /// <summary>Initializes a new instance of the <see cref="PageViewModel"/> class.</summary>
    /// <param name="hostScreen">The owning screen.</param>
    /// <param name="urlPathSegment">The route segment.</param>
    /// <param name="title">The display title.</param>
    /// <param name="summary">The page summary.</param>
    protected PageViewModel(IScreen hostScreen, string urlPathSegment, string title, string summary)
    {
        HostScreen = hostScreen;
        UrlPathSegment = urlPathSegment;
        Title = title;
        Summary = summary;
    }

    /// <inheritdoc/>
    public string UrlPathSegment { get; }

    /// <inheritdoc/>
    public IScreen HostScreen { get; }

    /// <summary>Gets the display title.</summary>
    public string Title { get; }

    /// <summary>Gets the page summary.</summary>
    public string Summary { get; }
}
