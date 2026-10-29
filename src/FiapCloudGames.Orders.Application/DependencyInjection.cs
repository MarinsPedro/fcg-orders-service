namespace FiapCloudGames.Orders.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddOrdersApplication(this IServiceCollection services)
    {
        services.AddScoped<CreatePurchaseOrderService>();
        services.AddScoped<GetPurchaseOrderService>();

        return services;
    }
}
