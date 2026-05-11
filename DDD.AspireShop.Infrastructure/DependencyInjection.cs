using DDD.AspireShop.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace DDD.AspireShop.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddShopInfrastructure(this IServiceCollection services)
    {
        services.AddHostedService<MySqlSchemaInitializer>();
        services.AddScoped<IProductRepository, MySqlProductRepository>();
        services.AddScoped<IOrderRepository, MySqlOrderRepository>();

        return services;
    }
}
