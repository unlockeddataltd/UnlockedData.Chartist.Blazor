using System.Text.Json.Serialization;
using Microsoft.JSInterop;

namespace UnlockedData.Chartist.Blazor;

/// <summary>
/// Defines a the basic options for all charts.
/// </summary>
public class ChartBaseOptions
{
    /// <summary>
    /// Gets or sets the x axis options.
    /// </summary>
    /// <value>
    /// The axis y axis options.
    /// </value>
    public AxisOptions AxisX { get; set; } = new AxisXOptions();

    /// <summary>
    /// Gets or sets the y axis options.
    /// </summary>
    /// <value>
    /// The y axis options.
    /// </value>
    public AxisOptions AxisY { get; set; } = new AxisYOptions();

    /// <summary>
    /// Gets or sets a value indicating whether the data should be reversed.
    /// </summary>
    /// <value>
    ///   <c>true</c> if the data should be reversed; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Default value is <c>false</c>
    /// </remarks>
    public bool ReverseData { get; set; } = false;

    /// <summary>
    /// Gets or sets the width.
    /// </summary>
    /// <value>
    /// The width.
    /// </value>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public dynamic Width { get; set; }

    /// <summary>
    /// Gets or sets the height.
    /// </summary>
    /// <value>
    /// The height.
    /// </value>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public dynamic Height { get; set; }

    /// <summary>
    /// Gets or sets the high value.
    /// </summary>
    /// <value>
    /// The high value.
    /// </value>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? High { get; set; }

    /// <summary>
    /// Gets or sets the low value.
    /// </summary>
    /// <value>
    /// The low value.
    /// </value>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Low { get; set; }

    
    /// <summary>
    /// Turn on legend plugin
    /// </summary>

    public bool ShowLegend { get; set; } = false;
    public LegendOptions LegendOptions { get; set; }= new ();
    
    /// <summary>
    /// Enable tooltips
    /// </summary>
    public bool ShowTooltips { get; set; } = false;
    public ToolTipOptions ToolTipOptions { get; set; }= new ();

    /// <summary>
    /// Enable point labels
    /// </summary>
    public bool ShowPointLabels { get; set; } = false;
    /// <summary>
    /// Ignored. The zoom plugin was removed in 4.0.
    /// </summary>
    [Obsolete("Zoom was removed in 4.0 and is ignored.")]
    public bool UseZoom { get; set; } = false;
    /// <summary>
    /// Use this to update elements in the SVG with specific classes so they can be targeted by CSS
    /// </summary>
    public List<CssClassUpdate> CssClassUpdates { get; set; } = new List<CssClassUpdate>();

    

}
