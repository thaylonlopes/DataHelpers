namespace Dapper.Helpers.Models;

/// <summary>
/// Define os critérios de ordenação por campo e direção (ascendente/descendente) para consultas paginadas.
/// </summary>
public class SortDefinition
{
    /// <summary>
    /// O nome da coluna ou propriedade a ser utilizada na cláusula de ordenação.
    /// </summary>
    public string Field { get; set; }

    /// <summary>
    /// Indica se a ordenação deve ser ascendente (<c>true</c>) ou descendente (<c>false</c>).
    /// </summary>
    public bool Ascending { get; set; }

    /// <summary>
    /// Inicializa uma nova definição de ordenação para uma propriedade especificada.
    /// </summary>
    /// <param name="field">O nome do campo a ser ordenado.</param>
    /// <param name="ascending">Indica se a ordenação é ascendente (padrão true).</param>
    public SortDefinition(string field, bool ascending = true)
    {
        Field = field;
        Ascending = ascending;
    }
}
