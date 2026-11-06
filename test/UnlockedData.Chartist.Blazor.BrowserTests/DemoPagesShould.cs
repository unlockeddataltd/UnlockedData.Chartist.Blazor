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
    public static TheoryData<string, string> Pages => new()
    {
        { "/SimpleLine", ".ct-line, .ct-point" },
        { "/MultipleLine", ".ct-line, .ct-point" },
        { "/MultipleLineWithOverides", ".ct-area" },
        { "/MultipleWithCustomClassName", ".custom-class-area .ct-area" },
        { "/JaggedLine", ".ct-line" },
        { "/monotoneCubic", ".ct-line" },
        { "/StepLine", ".ct-line" },
        { "/CardinalLine", ".ct-line" },
        { "/eventedline", ".ct-point" },
        { "/SimpleBar", ".ct-bar" },
        { "/SimpleBarInterpolation", ".ct-bar" },
        { "/MultipleBar", ".ct-series-b .ct-bar" },
        { "/MultipleBarMissingSets", ".ct-bar" },
        { "/HorizontalBar", ".ct-chart-bar.ct-horizontal-bars .ct-bar" },
        { "/Stacked", ".ct-bar" },
        { "/eventedbar", ".ct-bar" },
        { "/EventedMultipleBar", ".ct-bar" },
        { "/EventedMultipleBarHorizontal", ".ct-bar" },
        { "/PluginsEvents", ".ct-legend li" },
        { "/plugins", ".ct-legend li" },
        { "/simplepie", ".ct-slice-pie" }
    };

    [Theory]
    [MemberData(nameof(Pages))]
    public async Task DrawTheirChartsWithoutProblems(string route, string expected)
    {
        await using var chart = await ChartPage.OpenAsync(site, route);

        await Expect(chart.Page.Locator(expected).First).ToBeAttachedAsync();

        chart.AssertNoProblems();
    }

    [Fact]
    public async Task RotateHorizontalLabels()
    {
        await using var chart = await ChartPage.OpenAsync(site, "/HorizontalBar");

        await Expect(chart.Page.Locator(".ct-chart.rotate-horizontal-labels svg")).ToHaveCountAsync(1);
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
        Assert.Contains(errors, error => error.Contains("Load these scripts in this order"));
        await page.CloseAsync();
    }
}
