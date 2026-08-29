namespace DataMapping.Helpers.Interfaces;

/// <summary>
/// Contrato para conversores customizados de tipos de dados durante o mapeamento.
/// </summary>
/// <typeparam name="TSource">O tipo de origem.</typeparam>
/// <typeparam name="TDestination">O tipo de destino convertido.</typeparam>
public interface ITypeConverter<in TSource, out TDestination>
{
    /// <summary>
    /// Converte um valor do tipo de origem para o tipo de destino.
    /// </summary>
    /// <param name="source">O valor de origem.</param>
    /// <returns>O valor convertido.</returns>
    TDestination Convert(TSource source);
}

