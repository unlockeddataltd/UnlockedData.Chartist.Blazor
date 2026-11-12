using Microsoft.AspNetCore.Components;

namespace UnlockedData.Chartist.Blazor.Demo.ServerSide.Shared;

/// <summary>
/// Records the last Data point event of a chart for <see cref="EventReadout"/>.
/// The callbacks use this class as their receiver, so an event re-renders only the readout,
/// not the page that holds the chart.
/// </summary>
public class EventLog
{
    public event Action? Changed;

    public string? LastType { get; private set; }

    public ChartistMouseEventArgs? LastArgs { get; private set; }

    public EventCallback<ChartistMouseEventArgs> Clicked =>
        EventCallback.Factory.Create<ChartistMouseEventArgs>(this, args => Record("click", args));

    public EventCallback<ChartistMouseEventArgs> Entered =>
        EventCallback.Factory.Create<ChartistMouseEventArgs>(this, args => Record("enter", args));

    public EventCallback<ChartistMouseEventArgs> Exited =>
        EventCallback.Factory.Create<ChartistMouseEventArgs>(this, args => Record("exit", args));

    private void Record(string type, ChartistMouseEventArgs args)
    {
        LastType = type;
        LastArgs = args;
        Changed?.Invoke();
    }
}
