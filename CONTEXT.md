# UnlockedData.Chartist.Blazor

A Blazor component library that wraps the Chartist charting library, so Blazor apps can draw Chartist line, bar and pie charts from C# data and options.

## Language

### Chartist and what we ship

**Chartist**:
The upstream JavaScript charting library (chartist-js/chartist) that draws the charts as SVG.
_Avoid_: ChartistJS, chartist-js (as a name for the library in prose)

**Vendor files**:
The Chartist files the package ships, exactly as upstream publishes them.
_Avoid_: bundle, chartist.js (for the vendor build)

**First-party plugins**:
The chart plugins this project owns and ships next to the Vendor files: legend, tooltip and point labels.
_Avoid_: community plugins, Chartist plugins (once they are ours)

**Interop**:
The project's JavaScript layer that turns C# chart options into Chartist calls and reports chart events back to C#.
_Avoid_: JS wrapper, bridge

**Consumer**:
A Blazor app that references the package and puts its files on its pages.
_Avoid_: client, host app

### Charts

**Chart**:
One rendered Chartist line, bar or pie chart, owned by one chart component.

**Series**:
One named set of values in a Chart, drawn with its own style.

**Data point**:
One drawn value in a Chart that a user can click or hover: a point on a line chart or a bar on a bar chart.
_Avoid_: node, marker

**Data point event**:
A click, enter or exit on a Data point, reported to C# with the point's meta, value and Series name.
_Avoid_: mouse event (for the C# callback)

### Releases

**Parity port**:
A release that changes the Chartist version underneath without changing the C# API or chart behaviour.
_Avoid_: migration, rewrite
