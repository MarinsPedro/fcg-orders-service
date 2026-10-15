namespace FiapCloudGames.Orders.Infrastructure.Health;

internal sealed class OrdersDatabaseHealthCheck(IServiceScopeFactory scopeFactory) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var database = scope.ServiceProvider
                .GetRequiredService<OrdersDbContext>();
            return await database.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy("PostgreSQL acessível.")
                : HealthCheckResult.Unhealthy("PostgreSQL indisponível.");
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            return HealthCheckResult.Unhealthy("Falha ao verificar o PostgreSQL.", exception);
        }
    }
}
