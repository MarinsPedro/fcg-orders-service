namespace FiapCloudGames.Orders.Infrastructure.ExternalApi.Catalog;

internal sealed class CatalogClient(
    HttpClient httpClient,
    IServiceTokenProvider tokenProvider,
    IOptions<CatalogApiOptions> configuredOptions
    ) : ICatalogGateway
{
    private readonly CatalogApiOptions options = configuredOptions.Value;

    public async Task<ExternalApiResponse<Game>> GetGameByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // O Building Blocks consulta o Discovery do Identity na primeira vez,
        // pede o token por Client Credentials e reutiliza o token em memória.
        var accessToken = await tokenProvider.GetAccessTokenAsync(
            options.Audience,
            options.Scopes,
            cancellationToken);

        using var request = new HttpRequestMessage(HttpMethod.Get, $"v1/games/{id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        // TODO: rever este teste, pois tem erros que são na faixa 400..499, e eles não deveria retornar 500...
        if (!response.IsSuccessStatusCode)
        {
            return await ExternalApiResponseFactory.FailureAsync<Game>(
                response,
                cancellationToken);
        }

        var apiResponse = await response.Content.ReadFromJsonAsync<GameResponse>(cancellationToken);

        if (apiResponse is null
            || apiResponse.Id == Guid.Empty
            || string.IsNullOrWhiteSpace(apiResponse.Title)
            || string.IsNullOrWhiteSpace(apiResponse.Description)
            || string.IsNullOrWhiteSpace(apiResponse.Category))
        {
            throw new HttpRequestException("A API Category retornou uma resposta inválida.");
        }

        return ExternalApiResponseFactory.Success(
            response.StatusCode,
            new Game(
                apiResponse.Id,
                apiResponse.Title,
                apiResponse.Description,
                apiResponse.Category,
                apiResponse.BasePrice,
                apiResponse.IsActive));
    }

    private sealed record GameResponse(
        Guid Id,
        string Title,
        string Description,
        string Category,
        decimal BasePrice,
        bool IsActive);
}
