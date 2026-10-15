namespace FiapCloudGames.Orders.Domain.Extensions;

/// <summary>
/// Classe de extensão para recuperação de metadados de qualquer enum.
/// </summary>
public static class EnumExtension
{
    /// <summary>
    /// Obtém o valor do atributo Display(Name = "valor") do enum que estiver configurado.
    /// </summary>
    /// <param name="value">A enumeração</param>
    /// <returns>Retorna um string contendo o valor do atributo Display(Name = "valor").</returns>
    public static string GetDisplayName(this Enum value)
    {
        // Obtém o tipo da enum e as informações do campo específico.
        Type type = value.GetType();
        FieldInfo fieldInfo = type.GetField(value.ToString())!;

        // Busca pelo atributo [Display] no campo.
        DisplayAttribute attribute = fieldInfo?.GetCustomAttribute<DisplayAttribute>()!;

        // Se encontrar o atributo, retorna o Name, senão retorna o ToString padrão.
        return attribute?.GetName() ?? value.ToString();
    }
}
