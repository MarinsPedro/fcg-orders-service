namespace FiapCloudGames.Orders.Worker.Messaging;

// DTO externo publicado pelo Library como GameGrantedEventV1 em JSON puro.
public sealed class LibraryGrantMessage
{
    public Guid LibraryEntryId { get; set; }
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public Guid GameId { get; set; }
    public DateTimeOffset GrantedAtUtc { get; set; }
}
