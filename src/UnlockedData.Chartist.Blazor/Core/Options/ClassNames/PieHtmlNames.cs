namespace UnlockedData.Chartist.Blazor
{
    /// <summary>
    /// Defines the html class names that apply styling to a Pie chart
    /// </summary>
    /// <seealso cref="UnlockedData.Chartist.Blazor.HtmlNamesBase" />
    public class PieHtmlNames : HtmlNamesBase
    {
        /// <summary>
        /// Gets or sets the chart pie style class name.
        /// </summary>
        /// <value>
        /// The chart pie style class name.
        /// </value>
        public string ChartPie { get; set; } = "ct-chart-pie";

        /// <summary>
        /// Gets or sets the chart donut style class name.
        /// </summary>
        /// <value>
        /// The chart donut style class name.
        /// </value>
        public string ChartDonut { get; set; } = "ct-chart-donut";
        /// <summary>
        /// Gets or sets the pie slice style class name.
        /// </summary>
        /// <value>
        /// The pie slice style class name.
        /// </value>
        public string SlicePie { get; set; } = "ct-slice-pie";

        /// <summary>
        /// Gets or sets the donut slice style class name.
        /// </summary>
        /// <value>
        /// The donut slice style class name.
        /// </value>
        public string SliceDonut { get; set; } = "ct-slice-donut";

        /// <summary>
        /// Ignored. Chartist 2 has no solid donut.
        /// </summary>
        [System.Obsolete("Chartist 2 has no solid donut; ignored.")]
        public string SliceDonutSolid { get; set; } = "ct-slice-donut-solid";
    }
}
