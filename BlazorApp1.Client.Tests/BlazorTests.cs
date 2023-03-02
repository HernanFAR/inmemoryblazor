using Microsoft.Playwright;
using FluentAssertions;

namespace BlazorApp1.Client.Tests;

public class BlazorTests : IClassFixture<IntegratedSolutionFixture>
{
    private readonly IntegratedSolutionFixture _fixture;

    public BlazorTests(IntegratedSolutionFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task TestBlazorWasmApp()
    {
        var context = await _fixture.ChromiumBrowser
            .NewContextAsync(
                new BrowserNewContextOptions
                {
                    IgnoreHTTPSErrors = true
                });

        var page = await context.NewPageAsync();

        var gotoResult = await page.GotoAsync(
            $"{_fixture.Blazor.ServerAddress}counter",
            new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle
            });

        gotoResult.Should().NotBeNull();

        await gotoResult!.FinishedAsync();
        gotoResult.Ok.Should().BeTrue();

        await page.WaitForSelectorAsync("#app");

        var countElement = await page.QuerySelectorAsync("#current-count");

        countElement.Should().NotBeNull();

        (await countElement!.InnerTextAsync())
            .Should().Be("Current count: 0");

        // Click the increment button and verify the updated count value
        await page.ClickAsync("#increment-count");

        (await countElement.InnerTextAsync())
            .Should().Be("Current count: 1");

    }
}