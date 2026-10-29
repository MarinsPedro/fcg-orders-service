namespace FiapCloudGames.Orders.Application.Abstractions.External;

public interface IPromotionsGateway
{
    Task<ExternalApiResponse<CalculatePromotion>> PostCalcularDescontoAsync(
        Guid promotionId,
        Guid gameId,
        decimal basePrice,
        CancellationToken cancellationToken = default);

    Task<ExternalApiResponse<Promotion>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}

public sealed record CalculatePromotion(decimal FinalPrice);

public sealed record Promotion(Guid Id,
        string Name,
        string DiscountType,
        decimal DiscountValue,
        bool IsActive);
