using UnlockedData.Chartist.Blazor.Core.Data;

namespace UnlockedData.Chartist.Blazor;

public class ChartistMouseEventArgs : SeriesPointBase
{
    //taken from the ct:meta of the dom element; never null, a point without meta gets the defaults
    public PointMetaInfo PointMetaInfo { get; set; } = new PointMetaInfo();

    //taken from the ct:series-name of the series group; null for an unnamed series
    public string? SeriesName { get; set; }

}
