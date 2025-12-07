using Infrastructure.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Shared.Configurations;

namespace Ordering.API.Extensions;

public static class ServiceExtensions
{
    internal static IServiceCollection AddConfigurationSettings(this IServiceCollection services,
        IConfiguration configuration)
    {
        var databaseSettings = configuration.GetSection(nameof(DatabaseSettings))
            .Get<DatabaseSettings>() ?? throw new InvalidOperationException("DatabaseSettings configuration section is missing or invalid.");
        services.AddSingleton(databaseSettings);

        return services;
    }

    public static void ConfigureHealthChecks(this IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();
        var databaseSettings = serviceProvider.GetRequiredService<DatabaseSettings>();
        services.AddHealthChecks()
            .AddSqlServer(databaseSettings.ConnectionString,
                name: "SqlServer Health",
                failureStatus: HealthStatus.Degraded);
    }
}