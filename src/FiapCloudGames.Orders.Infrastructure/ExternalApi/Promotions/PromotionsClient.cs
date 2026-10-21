namespace FiapCloudGames.Orders.Infrastructure.ExternalApi.Promotions;

internal sealed class PromotionsClient(
    HttpClient httpClient,
    IServiceTokenProvider tokenProvider,
    IOptions<PromotionsApiOptions> configuredOptions
    ) : IPromotionsGateway
{
    private readonly PromotionsApiOptions options = configuredOptions.Value;

    public async Task<CalculatePromotion> PostCalcularDescontoAsync(Guid promotionId, Guid gameId, decimal basePrice, CancellationToken cancellationToken = default)
    {
        // O Building Blocks consulta o Discovery do Identity na primeira vez,
        // pede o token por Client Credentials e reutiliza o token em memória.
        var accessToken = await tokenProvider.GetAccessTokenAsync(
            options.Audience,
            options.Scopes,
            cancellationToken);

        using var request = new HttpRequestMessage(HttpMethod.Post, "internal/v1/promotions/calculate");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new { PromotionId = promotionId, GameId = gameId, BasePrice = basePrice });

        using var response = await httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"A API Promotions respondeu com status {(int)response.StatusCode}.",
                inner: null,
                response.StatusCode);
        }

        var apiResponse = await response.Content.ReadFromJsonAsync<CalculatePromotionResponse>(cancellationToken);

        if (apiResponse is null)
        {
            throw new HttpRequestException("A API Promotions retornou uma resposta inválida.");
        }

        return new CalculatePromotion(apiResponse.FinalPrice);
    }
    private sealed record CalculatePromotionResponse(decimal FinalPrice);
}
