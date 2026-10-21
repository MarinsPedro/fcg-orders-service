namespace FiapCloudGames.Orders.Application.Abstractions.External;

public interface IPromotionsGateway
{
    Task<CalculatePromotion> PostCalcularDescontoAsync(Guid promotionId, Guid gameId, decimal basePrice, CancellationToken cancellationToken = default);
}

public sealed record CalculatePromotion(decimal FinalPrice);
