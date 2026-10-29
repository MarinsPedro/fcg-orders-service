using FiapCloudGames.BuildingBlocks.Exceptions;
using FiapCloudGames.Orders.Application.Common;
using Npgsql;

namespace FiapCloudGames.Orders.Infrastructure.Persistence;

internal sealed class OrdersUnitOfWork(OrdersDbContext dbContext) : IOrdersUnitOfWork
{
    private const string ActiveGameConstraint =
        "ux_purchase_order_game_reservations_customer_game";

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException
            {
                ConstraintName: ActiveGameConstraint
            })
        {
            throw new BusinessRuleValidationException(
                [new AppError(
                    "Um dos produtos já está associado a uma ordem em andamento.",
                    "items")]);
        }
    }
}
