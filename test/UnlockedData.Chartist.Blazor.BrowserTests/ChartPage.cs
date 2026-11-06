using Microsoft.Playwright;
using Xunit;

namespace UnlockedData.Chartist.Blazor.BrowserTests;

/// <summary>
/// One open demo page, with the console problems seen on it. Charts only draw once the
/// Blazor circuit is up, so a drawn chart also means the page is interactive.
/// </summary>
public sealed class ChartPage : IAsyncDisposable
{
    private readonly List<string> _problems = new();

    private ChartPage(IPage page)
    {
        Page = page;
        page.Console += (_, message) =>
        {
            var isChartistWarning = message.Type == "warning" && message.Text.StartsWith("Chartist:", StringComparison.Ordinal);
            if (message.Type == "error" || isChartistWarning)
            {
                _problems.Add($"{message.Type}: {message.Text}");
            }
        };
        page.PageError += (_, error) => _problems.Add($"exception: {error}");
    }

    public IPage Page { get; }

    public static async Task<ChartPage> OpenAsync(DemoSiteFixture site, string route)
    {
        var chartPage = new ChartPage(await site.Browser.NewPageAsync());
        await chartPage.Page.GotoAsync(DemoSiteFixture.BaseUrl + route);
        await chartPage.Page.Locator(".ct-chart svg").First.WaitForAsync();
        return chartPage;
    }

    public ILocator Scenario(string name) => Page.Locator($"section[data-scenario='{name}']");

    public static ILocator Readout(ILocator scenario, string field) =>
        scenario.Locator($".event-readout [data-field='{field}']");

    public void AssertNoProblems() => Assert.Empty(_problems);

    public ValueTask DisposeAsync() => new(Page.CloseAsync());
}
