namespace FiapCloudGames.Orders.Worker.Sagas.PurchaseOrders;

internal sealed class MissingPurchaseOrderSagaException(
    string message) : Exception(message);
