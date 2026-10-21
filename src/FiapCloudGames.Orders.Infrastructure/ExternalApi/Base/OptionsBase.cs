namespace FiapCloudGames.Orders.Infrastructure.ExternalApi.Base;

public abstract class OptionsBase
{
    [Required]
    [Url]
    public string BaseAddress { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    [Required]
    public string Scopes { get; set; } = string.Empty;
}
