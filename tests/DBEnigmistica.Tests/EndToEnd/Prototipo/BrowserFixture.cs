using Microsoft.Playwright;

namespace DBEnigmistica.Tests.EndToEnd.Prototipo;

/// <summary>Chromium di Playwright, avviato una volta per classe di test; in locale lo scarica se manca (in CI lo installa il lavoro).</summary>
public sealed class BrowserFixture : IAsyncLifetime
{
    private IPlaywright? _playwright;

    public IBrowser Browser { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        try
        {
            Browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        }
        catch (PlaywrightException ex) when (ex.Message.Contains("Executable doesn't exist", StringComparison.Ordinal) && !FattoConBrowserAttribute.InCi)
        {
            if (Microsoft.Playwright.Program.Main(["install", "chromium"]) != 0)
                throw new InvalidOperationException("Installazione di Chromium non riuscita: esegui playwright.ps1 install chromium.", ex);

            Browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        }
    }

    public async Task DisposeAsync()
    {
        if (Browser is not null)
            await Browser.DisposeAsync();
        _playwright?.Dispose();
    }
}
