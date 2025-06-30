using Portify.Application.Interfaces;
using Portify.Infrastructure.Services;
using Portify.Infrastructure.Configuration.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Portify.Domain.Interfaces;
using Portify.Persistence.Data.Context;

namespace Portify.API.Startup;

public class ServiceConfiguration
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            ConfigureDevelopmentServices(services, configuration);
        }
        else if (environment.IsProduction())
        {
            ConfigureProductionServices(services, configuration);
        }

        // Common services for all environments
        services.AddControllers();
        AddAuthentication(services, configuration);
        // Add other shared services here
        AddServices(services);
        AddDatabase(services, configuration);
        AddCorsPolicy(services, configuration);


    }

    private static void ConfigureDevelopmentServices(IServiceCollection services, IConfiguration configuration)
        // ReSharper disable once ArrangeMethodOrOperatorBody
    {
        services.AddOptions<GitHubOAuthConfig>().Configure(options => configuration.GetSection(nameof(GitHubOAuthConfig)).Bind(options));
        services.Configure<JwtSettings>(configuration.GetSection(nameof(JwtSettings)));
    }

    private static void ConfigureProductionServices(IServiceCollection services, IConfiguration configuration)
        // ReSharper disable once ArrangeMethodOrOperatorBody
    {
        services.AddOptions<GitHubOAuthConfig>().Configure(options => configuration.GetSection(nameof(GitHubOAuthConfig)).Bind(options));
        services.Configure<JwtSettings>(configuration.GetSection(nameof(JwtSettings)));
    }

    private static void AddAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        IConfigurationSection jwtSettingsSection = configuration.GetSection(nameof(JwtSettings));
        services.Configure<JwtSettings>(jwtSettingsSection);

        JwtSettings? jwtSettings = jwtSettingsSection.Get<JwtSettings>();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization();
    }

      private static void AddDatabase(IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<PortifyDbContext>(options =>
        {

          options.UseNpgsql(connectionString);
        });

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<PortifyDbContext>());
    }

    private static void AddCorsPolicy(IServiceCollection services, IConfiguration configuration)
    {
        CorsOptions corsOptions = configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>()!;

        services.AddCors(options =>
        {
            options.AddPolicy(CorsOptions.PolicyName, policy =>
            {
                policy.WithOrigins(corsOptions.AllowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });
    }
    private static void AddServices(IServiceCollection services)
        => services.AddScoped<IJwtService, JwtService>();
}
