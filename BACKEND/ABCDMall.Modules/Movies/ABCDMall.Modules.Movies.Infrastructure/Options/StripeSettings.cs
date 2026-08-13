namespace ABCDMall.Modules.Movies.Infrastructure.Options;

public sealed class StripeSettings
{
    public string? SecretKey { get; set; }
    public string? PublishableKey { get; set; }
    public string? WebhookSecret { get; set; }
    public string? FrontendBaseUrl { get; set; }
}
