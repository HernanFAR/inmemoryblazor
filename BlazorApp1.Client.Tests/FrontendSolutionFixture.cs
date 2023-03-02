using Microsoft.Playwright;

namespace BlazorApp1.Client.Tests;

public class IntegratedSolutionFixture : IAsyncLifetime
{
    public IPlaywright Playwright { get; private set; } = default!;

    public IBrowser ChromiumBrowser { get; private set; } = default!;

    public BlazorWasmAppFactory Blazor { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        InstallPlaywright();

        Blazor = new BlazorWasmAppFactory();
        await Blazor.InitializeAsync();

        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        ChromiumBrowser = await Playwright.Chromium
            .LaunchAsync(
                new BrowserTypeLaunchOptions
                {
                    Headless = false
                });

    }

    public async Task DisposeAsync()
    {
        await ChromiumBrowser.DisposeAsync();
        Playwright.Dispose();

        await Blazor.DisposeAsync();


    }

    private static void InstallPlaywright()
    {
        var exitCode = Microsoft.Playwright.Program.Main(new[] { "install-deps" });
        if (exitCode != 0)
        {
            throw new Exception(
                $"Playwright exited with code {exitCode} on install-deps");
        }

        exitCode = Microsoft.Playwright.Program.Main(new[] { "install" });
        if (exitCode != 0)
        {
            throw new Exception(
                $"Playwright exited with code {exitCode} on install");
        }
    }
}
