using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace UnlockedData.Chartist.Blazor.BrowserTests;

/// <summary>
/// The contract of each demo page: what it must draw, with no console errors and no Chartist warnings.
/// </summary>
[Collection(DemoSiteCollection.Name)]
public sealed class DemoPagesShould(DemoSiteFixture site)
{
    private const string Line = ".ct-chart-line";
    private const string Bar = ".ct-chart-bar";
    private const string HorizontalBar = ".ct-chart-bar.ct-horizontal-bars";
    private const string Pie = ".ct-chart-pie";
    private const string NoFeature = "";

    // Route, chart type, drawn element, the fewest of them the page should draw, and any feature it shows
    public static TheoryData<string, string, string, int, string> Pages => new()
    {
        { "/SimpleLine", Line, ".ct-point", 5, NoFeature },
        { "/MultipleLine", Line, ".ct-point", 10, NoFeature },
        { "/MultipleLineWithOverides", Line, ".ct-point", 5, ".ct-area" },
        { "/MultipleWithCustomClassName", Line, ".custom-class-dots .ct-line", 1, ".custom-class-area .ct-area" },
        { "/JaggedLine", Line, ".ct-point", 5, NoFeature },
        { "/monotoneCubic", Line, ".ct-point", 5, NoFeature },
        { "/StepLine", Line, ".ct-point", 9, NoFeature },
        { "/CardinalLine", Line, ".ct-point", 5, NoFeature },
        { "/eventedline", Line, ".ct-point", 5, NoFeature },
        { "/SimpleBar", Bar, ".ct-bar", 5, NoFeature },
        { "/SimpleBarInterpolation", Bar, ".ct-bar", 5, NoFeature },
        { "/MultipleBar", Bar, ".ct-series-b .ct-bar", 5, NoFeature },
        { "/MultipleBarMissingSets", Bar, ".ct-bar", 7, NoFeature },
        { "/HorizontalBar", HorizontalBar, ".ct-bar", 4, ".ct-chart.rotate-horizontal-labels" },
        { "/Stacked", Bar, ".ct-bar", 15, NoFeature },
        { "/eventedbar", Bar, ".ct-bar", 5, NoFeature },
        { "/EventedMultipleBar", Bar, ".ct-bar", 5, NoFeature },
        { "/EventedMultipleBarHorizontal", HorizontalBar, ".ct-bar", 5, NoFeature },
        { "/PluginsEvents", Bar, ".ct-bar", 5, ".ct-legend li" },
        { "/plugins", Bar, ".ct-bar", 10, ".ct-legend li" },
        { "/simplepie", Pie, ".ct-slice-pie", 3, NoFeature }
    };

    [Theory]
    [MemberData(nameof(Pages))]
    public async Task DrawTheirChartsWithoutProblems(string route, string chartType, string element, int fewest, string feature)
    {
        await using var chart = await ChartPage.OpenAsync(site, route);

        await Expect(chart.Page.Locator($"svg{chartType}")).ToHaveCountAsync(1);
        await Expect(chart.Page.Locator(element).Nth(fewest - 1)).ToBeAttachedAsync();
        if (feature != NoFeature)
        {
            await Expect(chart.Page.Locator(feature).First).ToBeAttachedAsync();
        }

        chart.AssertNoProblems();
    }

    [Fact]
    public async Task RotateHorizontalLabels()
    {
        await using var chart = await ChartPage.OpenAsync(site, "/HorizontalBar");

        var label = chart.Page.Locator(".rotate-horizontal-labels .ct-label.ct-horizontal").First;

        await Expect(label).ToHaveCSSAsync("transform", new System.Text.RegularExpressions.Regex("^matrix"));
    }

    [Fact]
    public async Task DashTheLineFromASeriesClass()
    {
        await using var chart = await ChartPage.OpenAsync(site, "/MultipleWithCustomClassName");

        await Expect(chart.Page.Locator(".custom-class-dots .ct-line")).ToHaveCSSAsync("stroke-dasharray", "1000px");
    }

    [Fact]
    public async Task DrawPointLabelsAndTooltipsOnThePluginsPage()
    {
        await using var chart = await ChartPage.OpenAsync(site, "/plugins");

        await Expect(chart.Page.Locator("svg text.ct-label").First).ToBeAttachedAsync();
        // A bar is a line with no width, which Playwright counts as not visible
        await chart.Page.Locator(".ct-bar").First.HoverAsync(new LocatorHoverOptions { Force = true });
        await Expect(chart.Page.Locator(".chartist-tooltip.tooltip-show")).ToContainTextAsync("Week 1");

        chart.AssertNoProblems();
    }

    [Theory]
    [InlineData("/eventedline", ".ct-point", "Tuesday", "9")]
    [InlineData("/eventedbar", ".ct-bar", "Tuesday", "9")]
    public async Task KeepEventsWorkingWhenAClickReRendersTheParent(string route, string selector, string label, string value)
    {
        await using var chart = await ChartPage.OpenAsync(site, route);

        // Each click re-renders the page, which draws the chart again
        await chart.Page.Locator(selector).Nth(0).ClickAsync(new LocatorClickOptions { Force = true });
        await Expect(chart.Page.Locator("td").Nth(1)).ToHaveTextAsync("Monday");
        await chart.Page.Locator(selector).Nth(1).ClickAsync(new LocatorClickOptions { Force = true });

        await Expect(chart.Page.Locator("td").Nth(1)).ToHaveTextAsync(label);
        await Expect(chart.Page.Locator("td").Nth(2)).ToHaveTextAsync(value);
        await Expect(chart.Page.Locator(".ct-chart svg")).ToHaveCountAsync(1);
        chart.AssertNoProblems();
    }

    [Fact]
    public async Task ReportAWrongLoadOrder()
    {
        var page = await site.Browser.NewPageAsync();
        var errors = new List<string>();
        page.Console += (_, message) =>
        {
            if (message.Type == "error")
            {
                errors.Add(message.Text);
            }
        };

        await page.GotoAsync(DemoSiteFixture.BaseUrl + "/load-order-check.html");

        await Expect(page.Locator("#create-error")).ToContainTextAsync("Load these scripts in this order");
        Assert.Single(errors, error => error.Contains("Load these scripts in this order"));
        await page.CloseAsync();
    }
}
