namespace FiapCloudGames.Orders.Infrastructure.Catalog;

internal sealed class CatalogApiOptions
{
    public const string SectionName = "CatalogApi";

    [Required]
    [Url]
    public string BaseAddess { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    [Required]
    public string Scopes { get; set; } = string.Empty;
}
