# UnlockedData.Chartist.Blazor

[![ci-build](https://github.com/UnlockedData/UnlockedData.Chartist.Blazor/actions/workflows/ci-build.yml/badge.svg)](https://github.com/UnlockedData/UnlockedData.Chartist.Blazor/actions/workflows/ci-build.yml)

A Blazor implementation of [Chartist](https://chartist.dev) strongly influenced by [Steve Sanderson's NDC demo](https://github.com/SteveSandersonMS/presentation-2020-01-NdcBlazorComponentLibraries) 

The package ships Chartist 2.1.0.

## Installation

To Install:

```ps1
Install-Package UnlockedData.Chartist.Blazor
```

or

```ps1
dotnet add package UnlockedData.Chartist.Blazor
```

For client-side and server-side Blazor - add these to index.html or _Layout.cshtml (head section). The scripts must load in this order:

```xml
    <link href="_content/UnlockedData.Chartist.Blazor/vendor/chartist/index.min.css" rel="stylesheet" />
    <link href="_content/UnlockedData.Chartist.Blazor/chartist-plugins.css" rel="stylesheet" />
    <script src="_content/UnlockedData.Chartist.Blazor/vendor/chartist/index.umd.js"></script>
    <script src="_content/UnlockedData.Chartist.Blazor/vendor/chartist/compat.umd.js"></script>
    <script src="_content/UnlockedData.Chartist.Blazor/chartist-plugins.js"></script>
    <script src="_content/UnlockedData.Chartist.Blazor/blazorChartistInterop.js"></script>
```

## Upgrading from 3.x to 4.0

4.0 moves from Chartist 0.11.4 to Chartist 2. The chart components and their options work as before, apart from the breaking changes below.

### Breaking changes

- **File names.** `chartist(.min).js`, `chartist(.min).css` and `blazorChartistInterop.min.js` are gone. Replace your tags with the ones above. A stale tag fails with a 404, and a wrong load order logs an error that names the right one.
- **Zoom is removed.** `UseZoom` is obsolete and ignored.
- **The solid donut is removed.** Chartist 2 doesn't have one. `PieOptions.DonutSolid` and `PieHtmlNames.SliceDonutSolid` are obsolete and ignored, and `DonutSolid = true` draws a normal donut.
- **Attributes.** Chart elements carry Chartist's own `ct:value`, `ct:meta` and `ct:series-name` attributes instead of `ct-value`, `ct-meta` and `ct-series-name`.
- **Colours** in the Chartist stylesheet now use CSS custom properties (such as `--ct-series-a`). The class names are unchanged, but check any CSS that restyles `.ct-*`.
- **Line dashes.** `.ct-line` now sets its own `stroke-dasharray`, so one set on a series class no longer reaches the line. Set `--ct-line-dasharray` on the series class instead.
- **Axis range.** Chartist 2 can add a grid step where a value falls just past a step. A low of -500000.11 now draws the axis down to -1M, where 0.11.4 stopped at -500K.
- **Null values** draw no bar element. 3.x drew an invisible zero-length one, so CSS or scripts that count `.ct-bar` see fewer.
- **Scripts that call Chartist directly** keep working through the compat build only with `new`: `Chartist.Bar(...)` now throws, `new Chartist.Bar(...)` works. Prefer `new Chartist.BarChart(...)`.
- With `TreatWarningsAsErrors`, setting an obsolete property is a build error. Remove the setting.

### Also new

- `ChartistMouseEventArgs.SeriesName` gives the name of the series of the Data point, or null for an unnamed series.
- `ChartistMouseEventArgs.PointMetaInfo` is never null. A point without meta gets the default (`Label` is `"No Label"`).
- Tooltips work on points without meta, such as plain numeric pie slices. They used to throw on hover.
- Point labels show zero values.

### Known limitations

- `ChartistLine` and `ChartistBar` draw a new chart on every render, and leak a `DotNetObjectReference` each time, as they did in 3.x. See [#5](https://github.com/unlockeddataltd/UnlockedData.Chartist.Blazor/issues/5).
- This package can't move to Chartist 3, which removes the classes and compat build it uses. See `docs/adr/0001-chartist-2-via-1x-classes-and-compat.md`.

## Running the tests

```bash
dotnet test UnlockedData.Chartist.Blazor.sln
```

The browser tests drive the demo in headless Chromium. Build first, and install the browser once:

```bash
dotnet build UnlockedData.Chartist.Blazor.Demo.sln -c Release
```

```bash
pwsh test/UnlockedData.Chartist.Blazor.BrowserTests/bin/Release/net10.0/playwright.ps1 install chromium
```

```bash
dotnet test test/UnlockedData.Chartist.Blazor.BrowserTests/UnlockedData.Chartist.Blazor.BrowserTests.csproj --no-build -c Release
```
