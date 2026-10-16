namespace FiapCloudGames.Orders.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddOrdersPresentation(this IServiceCollection services)
    {
        services
            .AddControllers()
            .AddApplicationPart(typeof(DependencyInjection).Assembly);

        services.AddAuthorization(options =>
        {
            options.AddPolicy(
                OrdersPolicies.PurchaseOrderCreate,
                policy => policy
                    .RequireAuthenticatedUser()
                    .RequireServiceToken()
                    .RequireScopes("orders.purchaseorder.create"));
            options.AddPolicy(
                OrdersPolicies.PurchaseOrderRead,
                policy => policy
                    .RequireAuthenticatedUser()
                    .RequireServiceToken()
                    .RequireScopes("orders.purchaseorder.read"));
            options.AddPolicy(
                OrdersPolicies.PurchaseOrderWrite,
                policy => policy
                    .RequireAuthenticatedUser()
                    .RequireServiceToken()
                    .RequireScopes("orders.purchaseorder.write"));
        });

        return services;
    }
}
