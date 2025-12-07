using Microsoft.Extensions.DependencyInjection;
using Shared.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Ordering.Infrastructure.Persistence;


namespace Ordering.Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();
        var configuration = serviceProvider.GetService<IConfiguration>();
        if (configuration == null)
            throw new InvalidOperationException("IConfiguration is not registered in the service collection.");
            
        var databaseSettings = configuration.GetSection(nameof(DatabaseSettings)).Get<DatabaseSettings>();
        if (databaseSettings == null || string.IsNullOrEmpty(databaseSettings.ConnectionString))
            throw new ArgumentNullException("Connection string is not configured.");
        
        services.AddDbContext<OrderContext>(options =>
        {
            options.UseMySql(
                databaseSettings.ConnectionString,
                ServerVersion.AutoDetect(databaseSettings.ConnectionString),
                builder => 
                    builder.MigrationsAssembly(typeof(OrderContext).Assembly.FullName));
        });

        services.AddScoped<OrderContextSeed>();
        //services.AddScoped<IOrderRepository, OrderRepository>();
        //services.AddScoped(typeof(IUnitOfWork<>), typeof(UnitOfWork<>));

        //services.AddScoped(typeof(ISmtpEmailService), typeof(SmtpEmailService));

        return services;
    }
}
