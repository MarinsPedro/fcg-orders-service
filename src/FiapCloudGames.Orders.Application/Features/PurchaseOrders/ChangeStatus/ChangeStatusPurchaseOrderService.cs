namespace FiapCloudGames.Orders.Application.Features.PurchaseOrders.ChangeStatus;

public class ChangeStatusPurchaseOrderService(
    IPurchaseOrderRepository purchaseOrderRepository,
    IOrdersUnitOfWork unitOfWork,
    TimeProvider clock,
    ILogger<ChangeStatusPurchaseOrderService> logger)
{
    public async Task<PurchaseOrderResult> ExecuteAsync(
        Guid id,
        ChangeStatusPurchaseOrderInput input,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Atualização do estado da ordem de compra id {ID}", id);

        var purchaseOrder = await purchaseOrderRepository.GetAsync(id, cancellationToken)
            ?? throw AppException.NotFound("Ordem de compra não econtrada");

        var currentStatus = purchaseOrder.Status;

        purchaseOrder.ChangeStatus(input.Status, clock.GetUtcNow());
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Ordem de compra com estado alterado de {From} para {TO}", currentStatus.GetDisplayName(), purchaseOrder.Status.GetDisplayName());
        return OrdersApplicationMappings.ToResult(purchaseOrder);
    }
}
