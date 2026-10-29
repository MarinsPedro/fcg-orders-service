using FiapCloudGames.Orders.Infrastructure.Orchestration;

namespace FiapCloudGames.Orders.UnitTests;

public sealed class PurchaseOrderSagaGameTests
{
    [Fact]
    public void MarkGranted_ShouldCountGameOnlyAfterItWasRequested()
    {
        var game = CreateSagaGame();
        var requestedAt = UtcNow();
        var grantedAt = requestedAt.AddSeconds(1);

        game.MarkRequested(requestedAt);
        var grantedNow = game.MarkGranted(grantedAt);

        Assert.True(grantedNow);
        Assert.Equal(PurchaseOrderSagaGameStatus.Granted, game.Status);
        Assert.Equal(requestedAt, game.RequestedAtUtc);
        Assert.Equal(grantedAt, game.GrantedAtUtc);
    }

    [Fact]
    public void MarkGranted_ShouldIgnoreDuplicatedResponse()
    {
        var game = CreateSagaGame();
        game.MarkRequested(UtcNow());
        var firstGrant = game.MarkGranted(UtcNow().AddSeconds(1));

        var duplicatedGrant = game.MarkGranted(UtcNow().AddSeconds(2));

        Assert.True(firstGrant);
        Assert.False(duplicatedGrant);
        Assert.Equal(UtcNow().AddSeconds(1), game.GrantedAtUtc);
    }

    [Fact]
    public void MarkGranted_ShouldRejectResponseBeforeRequest()
    {
        var game = CreateSagaGame();

        Assert.Throws<InvalidOperationException>(
            () => game.MarkGranted(UtcNow()));
    }

    private static PurchaseOrderSagaGame CreateSagaGame() =>
        PurchaseOrderSagaGame.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

    private static DateTimeOffset UtcNow() =>
        new(2026, 10, 29, 12, 0, 0, TimeSpan.Zero);
}
