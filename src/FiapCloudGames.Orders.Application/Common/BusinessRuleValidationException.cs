namespace FiapCloudGames.Orders.Application.Common;

public sealed class BusinessRuleValidationException : Exception
{
    public BusinessRuleValidationException(
        IReadOnlyCollection<AppError> errors)
        : base("Uma ou mais regras de negócio não foram atendidas.")
    {
        ArgumentNullException.ThrowIfNull(errors);

        if (errors.Count == 0)
            throw new ArgumentException(
                "Informe pelo menos um erro de regra de negócio.",
                nameof(errors));

        Errors = errors
            .Distinct()
            .ToArray();
    }

    public IReadOnlyCollection<AppError> Errors { get; }
}
