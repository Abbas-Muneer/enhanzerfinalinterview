using Enhanzer.Api.Data;
using Enhanzer.Api.Interfaces;
using Enhanzer.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace Enhanzer.Api.Extensions;

public static class ServiceCollectionExtensions
{
    private const string CorsPolicyName = "AngularClient";

    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            if (!string.IsNullOrWhiteSpace(connectionString) &&
                connectionString.Contains("Data Source=", StringComparison.OrdinalIgnoreCase) &&
                !connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlite(connectionString);
                return;
            }

            options.UseSqlServer(connectionString);
        });

        services.AddHttpClient<IExternalPosApiService, ExternalPosApiService>(client =>
        {
            client.BaseAddress = new Uri(configuration["ExternalPosApi:BaseUrl"]!);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ILocationService, LocationService>();

        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        services.AddCors(options =>
        {
            options.AddPolicy(CorsPolicyName, policy =>
            {
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return services;
    }

    public static string GetCorsPolicyName() => CorsPolicyName;
}
