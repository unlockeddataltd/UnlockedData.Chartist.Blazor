using System.Text.Json.Serialization;

namespace UnlockedData.Chartist.Blazor;

/// <summary>
/// Defines the Pie Chart Options
/// </summary>
/// <seealso cref="UnlockedData.Chartist.Blazor.ChartBaseOptions" />
public class PieOptions : ChartBaseOptions
{
    /// <summary>
    /// Gets or sets the Full Width Property of the chart
    /// </summary>
    public bool FullWidth { get; set; } = false;
    
    /// <summary>
    /// Gets or sets the chart padding.
    /// </summary>
    /// <value>
    /// The chart padding.
    /// </value>
    public double ChartPadding { get; set; } = 5;

    /// <summary>
    /// Gets or sets the start angle.
    /// </summary>
    /// <value>
    /// The start angle.
    /// </value>
    public double StartAngle { get; set; } = 0;


    /// <summary>
    /// Gets or sets the total.
    /// </summary>
    /// <value>
    /// The total.
    /// </value>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Total { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="PieOptions"/> is donut.
    /// </summary>
    /// <value>
    ///   <c>true</c> if donut; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Defaults to <c>false</c>
    /// </remarks>
    public bool Donut { get; set; } = false;

    /// <summary>
    /// Ignored. Chartist 2 has no solid donut, so a donut is always drawn with strokes.
    /// </summary>
    [Obsolete("Chartist 2 has no solid donut; ignored.")]
    public bool DonutSolid { get; set; } = false;

    /// <summary>
    /// Gets or sets the width of the donut.
    /// </summary>
    /// <value>
    /// The width of the donut.
    /// </value>
    public double DonutWidth { get; set; } = 60;

    /// <summary>
    /// Gets or sets the label offset.
    /// </summary>
    /// <value>
    /// The label offset.
    /// </value>
    public double LabelOffset { get; set; } = 0;

    /// <summary>
    /// Gets or sets the label position.
    /// </summary>
    /// <value>
    /// The label position.
    /// </value>
    public string LabelPosition { get; set; } = "inside"; //enum inside, outside, center

    /// <summary>
    /// Gets or sets the label direction.
    /// </summary>
    /// <value>
    /// The label direction.
    /// </value>
    public string LabelDirection { get; set; } = "neutral"; //enum neutral, implode, explode

    /// <summary>
    /// Gets or sets a value indicating whether empty values should be ignored.
    /// </summary>
    /// <value>
    ///   <c>true</c> if empty values should be ignored; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Defaults to <c>false</c>
    /// </remarks>
    public bool IgnoreEmptyValues { get; set; } = false;
}
