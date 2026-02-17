
using DeliveryApp.Application.Interfaces;
using DeliveryApp.Application.Services;
using DeliveryApp.Data.Context;
using DeliveryApp.Data.Repositories;
using DeliveryApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DeliveryApp.Adapter.Extensions;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IDeliveryDriverService, DeliveryDriverService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ICategoryService, CategoryService>();
        return services;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var host = configuration["Database:Host"];
        var port = configuration["Database:Port"];
        var name = configuration["Database:Name"];
        var user = configuration["Database:User"];
        var password = configuration["Database:Password"];
        var connectionString = $"Host={host};Port={port};Database={name};Username={user};Password={password}";

        services.AddDbContext<DeliveryAppDbContext>(options =>
            options.UseNpgsql(connectionString)
                   .UseSnakeCaseNamingConvention());

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IDeliveryDriverRepository, DeliveryDriverRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();

        return services;
    }
}
