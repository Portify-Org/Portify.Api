using Portify.API.Startup;
using Portify.Infrastructure.Configuration.Settings;


WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Configure services using ServiceConfiguration
ServiceConfiguration.ConfigureServices(builder.Services, builder.Configuration, builder.Environment);

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors(CorsOptions.PolicyName);

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
