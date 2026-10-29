namespace FiapCloudGames.Orders.Worker.Sagas.PurchaseOrders;

public sealed class PurchaseOrderSagaOptions
{
    public const string SectionName = "PurchaseOrderSaga";

    public TimeSpan[] MissingInstanceRedeliveryIntervals { get; init; } =
    [
        TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(3),
        TimeSpan.FromSeconds(5)
    ];
}
