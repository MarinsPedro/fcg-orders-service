using FiapCloudGames.Orders.Infrastructure.Orchestration;
using MassTransit;

namespace FiapCloudGames.Orders.Worker.Sagas.PurchaseOrders;

public abstract class PurchaseOrderSagaActivity<TMessage> :
    IStateMachineActivity<PurchaseOrderSagaState, TMessage>
    where TMessage : class
{
    public async Task Execute(
        BehaviorContext<PurchaseOrderSagaState, TMessage> context,
        IBehavior<PurchaseOrderSagaState, TMessage> next)
    {
        await ExecuteAsync(context);
        await next.Execute(context);
    }

    protected abstract Task ExecuteAsync(
        BehaviorContext<PurchaseOrderSagaState, TMessage> context);

    public Task Faulted<TException>(
        BehaviorExceptionContext<PurchaseOrderSagaState, TMessage, TException> context,
        IBehavior<PurchaseOrderSagaState, TMessage> next)
        where TException : Exception =>
        next.Faulted(context);

    public void Probe(ProbeContext context)
    {
        context.CreateScope(GetType().Name);
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }
}
