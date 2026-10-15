namespace FiapCloudGames.Orders.Migrations;

internal sealed class MigrationRunner(OrdersDbContext dbContext, ILogger<MigrationRunner> logger)
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        try
        {
            var pending = (await dbContext.Database
                .GetPendingMigrationsAsync(cancellationToken))
                .ToArray();

            if (pending.Length == 0)
            {
                logger.LogInformation("O banco do Orders já está atualizado.");
            }
            else
            {
                logger.LogInformation("Aplicando {MigrationCount} migration(s) do Orders: {Migrations}.", pending.Length, string.Join(", ", pending));
                await dbContext.Database.MigrateAsync(cancellationToken);
                logger.LogInformation("Migrations do Orders aplicadas com sucesso.");
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("A execução das migrations foi cancelada.");
            throw;
        }
        catch (Exception exception)
        {
            logger.LogCritical(exception, "Falha ao aplicar as migrations do Orders.");
            throw;
        }
    }
}
