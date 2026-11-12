using System.Diagnostics;
using System.Reflection;
using Microsoft.Playwright;
using Xunit;

namespace UnlockedData.Chartist.Blazor.BrowserTests;

/// <summary>
/// Starts the built ServerSide demo on a fixed port and a headless Chromium for the test run.
/// Build UnlockedData.Chartist.Blazor.Demo.sln first, in the same configuration as these tests.
/// </summary>
public sealed class DemoSiteFixture : IAsyncLifetime
{
    private const string DemoName = "UnlockedData.Chartist.Blazor.Demo.ServerSide";

    private static readonly TimeSpan StartTimeout = TimeSpan.FromSeconds(60);

    private Process? _demo;
    private IPlaywright? _playwright;

    public const string BaseUrl = "http://127.0.0.1:5199";

    public IBrowser Browser { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        _demo = StartDemo();
        await WaitForDemoAsync();

        _playwright = await Playwright.CreateAsync();
        Browser = await _playwright.Chromium.LaunchAsync();
    }

    public async Task DisposeAsync()
    {
        if (Browser is not null)
        {
            await Browser.DisposeAsync();
        }

        _playwright?.Dispose();

        if (_demo is not null)
        {
            _demo.Kill(entireProcessTree: true);
            await _demo.WaitForExitAsync();
            _demo.Dispose();
        }
    }

    private static Process StartDemo()
    {
        var binDirectory = Path.Combine(FindRepoRoot(), "demo", DemoName, "bin", BuildConfiguration(), "net10.0");
        var dll = Path.Combine(binDirectory, DemoName + ".dll");
        if (!File.Exists(dll))
        {
            throw new InvalidOperationException($"{dll} not found. Build UnlockedData.Chartist.Blazor.Demo.sln first.");
        }

        var startInfo = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = binDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        startInfo.ArgumentList.Add(dll);
        startInfo.ArgumentList.Add("--urls");
        startInfo.ArgumentList.Add(BaseUrl);
        // The static web assets of the library are only served from the build output in Development
        startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";

        var demo = Process.Start(startInfo) ?? throw new InvalidOperationException("The demo did not start.");
        demo.OutputDataReceived += (_, _) => { };
        demo.ErrorDataReceived += (_, _) => { };
        demo.BeginOutputReadLine();
        demo.BeginErrorReadLine();
        return demo;
    }

    private async Task WaitForDemoAsync()
    {
        using var client = new HttpClient();
        var timer = Stopwatch.StartNew();
        while (timer.Elapsed < StartTimeout)
        {
            if (_demo!.HasExited)
            {
                throw new InvalidOperationException($"The demo exited with code {_demo.ExitCode}.");
            }

            try
            {
                using var response = await client.GetAsync(BaseUrl);
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch (HttpRequestException)
            {
                // Not listening yet
            }

            await Task.Delay(250);
        }

        throw new TimeoutException($"The demo did not answer on {BaseUrl} within {StartTimeout}.");
    }

    private static string FindRepoRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "UnlockedData.Chartist.Blazor.Demo.sln")))
            {
                return directory.FullName;
            }
        }

        throw new InvalidOperationException("Repo root not found.");
    }

    private static string BuildConfiguration() =>
        typeof(DemoSiteFixture).Assembly.GetCustomAttribute<AssemblyConfigurationAttribute>()?.Configuration ?? "Debug";
}

[CollectionDefinition(Name)]
public sealed class DemoSiteCollection : ICollectionFixture<DemoSiteFixture>
{
    public const string Name = "Demo site";
}
