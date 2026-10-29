namespace FiapCloudGames.Orders.Infrastructure.Messaging;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string Host { get; init; } = string.Empty;
    public ushort Port { get; init; } = 5672;
    public string VirtualHost { get; init; } = "/";
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public RabbitMqTopologyOptions Topology { get; init; } = new();
}

public sealed class RabbitMqTopologyOptions
{
    public RabbitMqExchangeOptions Exchanges { get; init; } = new();
    public RabbitMqQueueOptions Queues { get; init; } = new();
}

public sealed class RabbitMqExchangeOptions
{
    public string PurchaseOrderCreated { get; init; } = string.Empty;
    public string PaymentRequested { get; init; } = string.Empty;
    public string PaymentApproved { get; init; } = string.Empty;
    public string PaymentRejected { get; init; } = string.Empty;
    public string GameGrantRequested { get; init; } = string.Empty;
    public string GameGranted { get; init; } = string.Empty;
    public string OrderCompleted { get; init; } = string.Empty;
}

public sealed class RabbitMqQueueOptions
{
    public string PurchaseOrderSaga { get; init; } = string.Empty;
}
