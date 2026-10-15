namespace FiapCloudGames.Orders.Domain.Orders;

/// <summary>
/// Representa as situações que uma ordem de compra pode possuir.
/// </summary>
public enum PurchaseOrderStatus
{
    /// <summary>
    /// Estado inicial da ordem de pedido, indicando que o pedido foi recebido (criado).
    /// </summary>
    [Display(Name = "Recebido")]
    Received = 1,

    /// <summary>
    /// Estado indicando que o pagamento precisa ser criado e ainda está pendente.
    /// </summary>
    [Display(Name = "Pagamento Pendente")]
    PendingPayment = 2,

    /// <summary>
    /// Estado indicando que o pagamento foi pago.
    /// </summary>
    [Display(Name = "Pagamento Aprovado")]
    PaymentApproved = 3,

    /// <summary>
    /// Estado que indica que o pagamento foi rejeitado.
    /// </summary>
    [Display(Name = "Pagamento Rejeitado")]
    PaymentRejected = 4,

    /// <summary>
    /// Estado que indica que o microsserviço de Library está processando a aquisição do jogo para a biblioteca do cliente (jogador).
    /// </summary>
    [Display(Name = "Biblioteca Pendente")]
    LibraryPending = 5,

    /// <summary>
    /// Estado indicando a fase final, ou seja, que o pedido foi concluído.
    /// </summary>
    [Display(Name = "Concluído")]
    OrderCompleted = 6,

    /// <summary>
    /// Estado indicando falha em algum ponto do processamento da ordem de pedido.
    /// </summary>
    [Display(Name = "Falhou")]
    Failed = 7,

    /// <summary>
    /// Estado indicando que a ordem foi cancelada antes de ser concluído (antes do pagamento).
    /// </summary>
    [Display(Name = "Cancelado pelo usuário")]
    UserCncelled = 8
}
