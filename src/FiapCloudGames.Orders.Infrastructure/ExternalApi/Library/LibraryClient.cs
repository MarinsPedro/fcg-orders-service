namespace FiapCloudGames.Orders.Infrastructure.ExternalApi.Library;

internal sealed class LibraryClient(
    HttpClient httpClient,
    IServiceTokenProvider tokenProvider,
    IOptions<LibraryApiOptions> configuredOptions) : ILibraryGateway
{
    private readonly LibraryApiOptions options = configuredOptions.Value;

    public async Task<ExternalApiResponse<UserLibrary>> CheckGameExistsOnLibraryAsync(
        Guid userId,
        Guid gameId,
        CancellationToken cancellationToken = default)
    {
        // O Building Blocks consulta o Discovery do Identity na primeira vez,
        // pede o token por Client Credentials e reutiliza o token em memória.
        var accessToken = await tokenProvider.GetAccessTokenAsync(
            options.Audience,
            options.Scopes,
            cancellationToken);

        using var request = new HttpRequestMessage(HttpMethod.Get, $"internal/v1/library/users/{userId}/games/{gameId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return await ExternalApiResponseFactory.FailureAsync<UserLibrary>(
                response,
                cancellationToken);
        }

        // A Library está devolvendo direito um bool sem um objeto wrapper.
        var apiResponse = await response.Content.ReadFromJsonAsync<bool>(cancellationToken);

        return ExternalApiResponseFactory.Success(
            response.StatusCode,
            new UserLibrary(apiResponse));
    }
}
