namespace D33.FlagTesting.Tests;

/// <summary>
/// A flag someone can flip at 2am is a branch you must have tested. Both sides, every time.
/// Flags are configuration, so the in-memory provider is the whole test story - no file, no cloud.
/// </summary>
public sealed class CheckoutFlagTests
{
    [Fact]
    public async Task Checkout_takes_the_new_path_when_the_flag_is_on()
    {
        using FlagFactory factory = new(enabled: true);
        using HttpClient client = factory.CreateClient();

        string body = await client.GetStringAsync(new Uri("/checkout", UriKind.Relative), TestContext.Current.CancellationToken);

        Assert.Equal("\"new checkout\"", body);
    }

    [Fact]
    public async Task Checkout_takes_the_legacy_path_when_the_flag_is_off()
    {
        using FlagFactory factory = new(enabled: false);
        using HttpClient client = factory.CreateClient();

        string body = await client.GetStringAsync(new Uri("/checkout", UriKind.Relative), TestContext.Current.CancellationToken);

        Assert.Equal("\"legacy checkout\"", body);
    }
}
