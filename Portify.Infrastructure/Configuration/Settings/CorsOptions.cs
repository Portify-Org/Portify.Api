namespace Portify.Infrastructure.Configuration.Settings;

   public sealed class CorsOptions
{
    public const string PolicyName = "PortifyCorsPolicy";
    public const string SectionName = "Cors";

    public required string[] AllowedOrigins { get; init; }
}

