using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace UnlockedData.Chartist.Blazor.BrowserTests;

[Collection(DemoSiteCollection.Name)]
public sealed class BrowserScenariosShould(DemoSiteFixture site)
{
    private const string Route = "/BrowserScenarios";

    private static readonly LocatorClickOptions ForceClick = new() { Force = true };
    private static readonly LocatorHoverOptions ForceHover = new() { Force = true };

    [Theory]
    [InlineData("named-line", ".ct-point", "Line B")]
    [InlineData("named-bar", ".ct-bar", "Bar B")]
    public async Task ReportClickEnterAndExitWithMetaValueAndSeries(string scenario, string selector, string seriesName)
    {
        await using var chart = await ChartPage.OpenAsync(site, Route);
        var section = chart.Scenario(scenario);
        var point = section.Locator($".ct-series-b {selector}").Nth(1);

        await point.ClickAsync(ForceClick);
        await ExpectReadout(section, "click", "Tue", "6", seriesName);

        await chart.Page.Mouse.MoveAsync(0, 0);
        await ExpectReadout(section, "exit", "Tue", "6", seriesName);

        await point.HoverAsync(ForceHover);
        await ExpectReadout(section, "enter", "Tue", "6", seriesName);

        chart.AssertNoProblems();
    }

    [Fact]
    public async Task ReportTheOwnValueOfAStackedBar()
    {
        await using var chart = await ChartPage.OpenAsync(site, Route);
        var section = chart.Scenario("stacked-bar");

        await section.Locator(".ct-series-b .ct-bar").First.ClickAsync(ForceClick);

        await ExpectReadout(section, "click", "Mon", "4", "Stack B");
        chart.AssertNoProblems();
    }

    [Fact]
    public async Task ReportNegativeValues()
    {
        await using var chart = await ChartPage.OpenAsync(site, Route);
        var section = chart.Scenario("zero-negative-bar");

        await section.Locator(".ct-bar").Nth(1).ClickAsync(ForceClick);

        await ExpectReadout(section, "click", "Tue", "-5", "Only series");
        chart.AssertNoProblems();
    }

    [Theory]
    [InlineData("zero-line")]
    [InlineData("zero-negative-bar")]
    public async Task LabelAZeroValue(string scenario)
    {
        await using var chart = await ChartPage.OpenAsync(site, Route);

        var labels = chart.Scenario(scenario).Locator("svg text.ct-label");

        await Expect(labels.First).ToHaveTextAsync("0");
        chart.AssertNoProblems();
    }

    [Fact]
    public async Task ApplyCustomSeriesClassAndCssClassUpdates()
    {
        await using var chart = await ChartPage.OpenAsync(site, Route);

        await Expect(chart.Scenario("custom-class").Locator(".custom-series .ct-point.special")).ToHaveCountAsync(1);
        chart.AssertNoProblems();
    }

    [Fact]
    public async Task DrawWithoutLabels()
    {
        await using var chart = await ChartPage.OpenAsync(site, Route);

        await Expect(chart.Scenario("missing-labels").Locator(".ct-point")).ToHaveCountAsync(3);
        chart.AssertNoProblems();
    }

    [Fact]
    public async Task GiveAPointWithoutMetaTheDefaultMetaAndNoSeries()
    {
        await using var chart = await ChartPage.OpenAsync(site, Route);
        var section = chart.Scenario("no-meta");

        await section.Locator(".ct-point").Nth(1).ClickAsync(ForceClick);

        await ExpectReadout(section, "click", "No Label", "8", "(none)");
        chart.AssertNoProblems();
    }

    [Theory]
    [InlineData("[1,2]")]
    [InlineData("\"text\"")]
    [InlineData("null")]
    public async Task TreatUnusableMetaAsNoMeta(string meta)
    {
        await using var chart = await ChartPage.OpenAsync(site, Route);

        var result = await chart.Page.EvaluateAsync<string>(
            """
            meta => {
                const element = document.createElementNS('http://www.w3.org/2000/svg', 'line');
                element.setAttribute('ct:meta', Chartist.serialize(JSON.parse(meta)) ?? '');
                return JSON.stringify(bChartist.pointMetaInfo(element));
            }
            """,
            meta);

        Assert.Equal("{}", result);
    }

    [Fact]
    public async Task HideASeriesFromTheLegend()
    {
        await using var chart = await ChartPage.OpenAsync(site, Route);
        var section = chart.Scenario("legend");
        await Expect(section.Locator(".ct-legend li")).ToHaveCountAsync(2);
        await Expect(section.Locator(".ct-bar")).ToHaveCountAsync(6);

        await section.Locator(".ct-legend li").First.ClickAsync();

        await Expect(section.Locator(".ct-legend li").First).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("inactive"));
        await Expect(section.Locator(".ct-bar")).ToHaveCountAsync(3);
        await Expect(section.Locator(".ct-series-a")).ToHaveCountAsync(0);

        // The legend survives the redraw and brings the series back
        await section.Locator(".ct-legend li").First.ClickAsync();

        await Expect(section.Locator(".ct-legend li")).ToHaveCountAsync(2);
        await Expect(section.Locator(".ct-bar")).ToHaveCountAsync(6);
        chart.AssertNoProblems();
    }

    [Fact]
    public async Task ShowATooltipWithMeta()
    {
        await using var chart = await ChartPage.OpenAsync(site, Route);
        var section = chart.Scenario("tooltip-line");

        await section.Locator(".ct-series-a .ct-point").Nth(1).HoverAsync(ForceHover);

        var tooltip = section.Locator(".chartist-tooltip.tooltip-show");
        await Expect(tooltip).ToContainTextAsync("Tue");
        await Expect(tooltip).ToContainTextAsync("9");
        chart.AssertNoProblems();
    }

    [Theory]
    [InlineData("plain-pie", ".ct-slice-pie")]
    [InlineData("plain-donut", ".ct-slice-donut")]
    public async Task ShowATooltipOnASliceWithoutMeta(string scenario, string selector)
    {
        await using var chart = await ChartPage.OpenAsync(site, Route);
        var section = chart.Scenario(scenario);

        await section.Locator(selector).First.DispatchEventAsync("mouseover");

        await Expect(section.Locator(".chartist-tooltip.tooltip-show")).ToBeAttachedAsync();
        chart.AssertNoProblems();
    }

    [Fact]
    public async Task DrawANormalDonutForTheObsoleteDonutSolid()
    {
        await using var chart = await ChartPage.OpenAsync(site, Route);

        await Expect(chart.Scenario("donut-solid").Locator(".ct-slice-donut")).ToHaveCountAsync(3);
        chart.AssertNoProblems();
    }

    [Theory]
    [InlineData("#rerender", "1")]
    [InlineData("#rapid-rerender", "5")]
    public async Task EndWithOneWorkingChartAfterReRenders(string button, string renderCount)
    {
        await using var chart = await ChartPage.OpenAsync(site, Route);
        var section = chart.Scenario("named-line");
        await Expect(section.Locator(".ct-point")).ToHaveCountAsync(6);

        await chart.Page.Locator(button).ClickAsync();
        await Expect(chart.Page.Locator("#render-count")).ToHaveTextAsync(renderCount);

        await Expect(section.Locator(".ct-point")).ToHaveCountAsync(6);
        await Expect(section.Locator("svg")).ToHaveCountAsync(1);
        await Expect(chart.Scenario("named-bar").Locator("svg")).ToHaveCountAsync(1);

        await section.Locator(".ct-series-b .ct-point").Nth(1).ClickAsync(ForceClick);
        await ExpectReadout(section, "click", "Tue", "6", "Line B");
        chart.AssertNoProblems();
    }

    private static async Task ExpectReadout(ILocator section, string type, string label, string y, string series)
    {
        await Expect(ChartPage.Readout(section, "type")).ToHaveTextAsync(type);
        await Expect(ChartPage.Readout(section, "label")).ToHaveTextAsync(label);
        await Expect(ChartPage.Readout(section, "y")).ToHaveTextAsync(y);
        await Expect(ChartPage.Readout(section, "series")).ToHaveTextAsync(series);
    }
}
