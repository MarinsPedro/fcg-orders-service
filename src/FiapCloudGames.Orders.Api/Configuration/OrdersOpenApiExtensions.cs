namespace FiapCloudGames.Orders.Api.Configuration;

internal static class OrdersOpenApiExtensions
{
    public static IServiceCollection AddOrdersOpenApi(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(
                "v1",
                new OpenApiInfo
                {
                    Title = "Fiap Cloud Games Orders API",
                    Version = "v1",
                    Description = "Serviço de pedido de compra."
                });

            options.AddSecurityDefinition(
                "bearer",
                new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Informe somente o JWT, sem o prefixo Bearer."
                });

            options.AddSecurityRequirement(document =>
                new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("bearer", document)] = []
                });
        });

        return services;
    }

    public static WebApplication UseOrdersOpenApi(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return app;
        }

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Fiap Cloud Games Orders API v1");
            options.EnablePersistAuthorization();
        });

        return app;
    }
}
