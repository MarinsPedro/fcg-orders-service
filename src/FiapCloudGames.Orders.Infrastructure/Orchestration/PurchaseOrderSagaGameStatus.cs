namespace FiapCloudGames.Orders.Infrastructure.Orchestration;

public enum PurchaseOrderSagaGameStatus
{
    WaitingPayment = 1,
    Requested = 2,
    Granted = 3
}
