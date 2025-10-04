using Avalonia.Controls;
using Avalonia.Interactivity;
using NUnit.Framework;

namespace ReactiveUI.Avalonia.Tests;

public class AvaloniaActivationForViewFetcherTests
{
    [Test]
    public void GetAffinityForView_Control_ReturnsPositive()
    {
        var sut = new AvaloniaActivationForViewFetcher();
        Assert.That(sut.GetAffinityForView(typeof(Button)), Is.GreaterThan(0));
    }

    [Test]
    public void GetActivationForView_Control_EmitsOnLoadedAndUnloaded()
    {
        var sut = new AvaloniaActivationForViewFetcher();
        var btn = new ActivatableButton();
        bool? last = null;

        using var sub = sut.GetActivationForView(btn).Subscribe(b => last = b);

        btn.RaiseEvent(new RoutedEventArgs(Button.LoadedEvent));
        Assert.That(last, Is.True);

        btn.RaiseEvent(new RoutedEventArgs(Button.UnloadedEvent));
        Assert.That(last, Is.False);
    }

    private sealed class ActivatableButton : Button, IActivatableView
    {
#pragma warning disable CA1822 // Mark members as static
        public IViewFor? ViewFor => null;
#pragma warning restore CA1822 // Mark members as static
    }
}
