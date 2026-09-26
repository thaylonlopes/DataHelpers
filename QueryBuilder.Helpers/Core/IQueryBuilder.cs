using QueryBuilder.Helpers.Enums;

namespace QueryBuilder.Helpers.Core;

/// <summary>
/// Contrato unificado para construção fluente de consultas SQL parametrizadas e poliglota.
/// </summary>
public interface IQueryBuilder
{
    /// <summary>
    /// Define a projeção de colunas da consulta.
    /// </summary>
    /// <param name="columns">Lista de colunas ou expressões a projetar.</param>
    /// <returns>A própria instância do builder para encadeamento fluente.</returns>
    IQueryBuilder Select(params string[] columns);

    /// <summary>
    /// Define a cláusula FROM indicando a tabela ou view de origem.
    /// </summary>
    /// <param name="table">Nome da tabela de origem.</param>
    /// <returns>A própria instância do builder para encadeamento fluente.</returns>
    IQueryBuilder From(string table);

    /// <summary>
    /// Adiciona a primeira condição de filtragem na cláusula WHERE.
    /// </summary>
    /// <param name="condition">Expressão textual do predicado condicional.</param>
    /// <returns>A própria instância do builder para encadeamento fluente.</returns>
    IQueryBuilder Where(string condition);

    /// <summary>
    /// Adiciona uma condição conjuntiva (AND) à cláusula WHERE.
    /// </summary>
    /// <param name="condition">Expressão condicional a ser combinada com AND.</param>
    /// <returns>A própria instância do builder para encadeamento fluente.</returns>
    IQueryBuilder And(string condition);

    /// <summary>
    /// Adiciona uma condição disjuntiva (OR) à cláusula WHERE.
    /// </summary>
    /// <param name="condition">Expressão condicional a ser combinada com OR.</param>
    /// <returns>A própria instância do builder para encadeamento fluente.</returns>
    IQueryBuilder Or(string condition);

    /// <summary>
    /// Adiciona um predicado LIKE com sanitização opcional contra Wildcard DoS / LIKE Injection.
    /// </summary>
    /// <param name="column">Nome da coluna a ser comparada.</param>
    /// <param name="searchTerm">Termo de busca.</param>
    /// <param name="escapeWildcards">Indica se caracteres coringa (%, _) devem ser neutralizados.</param>
    /// <returns>A própria instância do builder para encadeamento fluente.</returns>
    IQueryBuilder WhereLike(string column, string searchTerm, bool escapeWildcards = true);

    /// <summary>
    /// Adiciona a cláusula ORDER BY com o identificador sanitizado e delimitado pelo dialeto.
    /// </summary>
    /// <param name="column">Nome da coluna de ordenação.</param>
    /// <param name="ascending">Indica se a ordenação é ascendente (padrão true).</param>
    /// <returns>A própria instância do builder para encadeamento fluente.</returns>
    IQueryBuilder OrderBy(string column, bool ascending = true);

    /// <summary>
    /// Limita a quantidade máxima de registros retornados (LIMIT / FETCH NEXT).
    /// </summary>
    /// <param name="limit">Quantidade máxima de registros.</param>
    /// <returns>A própria instância do builder para encadeamento fluente.</returns>
    IQueryBuilder Limit(int limit);

    /// <summary>
    /// Define o deslocamento de registros a serem pulados (OFFSET).
    /// </summary>
    /// <param name="offset">Quantidade de registros a ignorar.</param>
    /// <returns>A própria instância do builder para encadeamento fluente.</returns>
    IQueryBuilder Offset(int offset);

    /// <summary>
    /// Adiciona uma cláusula de junção (INNER, LEFT, RIGHT, FULL) com outra tabela.
    /// </summary>
    /// <param name="table">Tabela a ser juntada.</param>
    /// <param name="condition">Condição de junção (cláusula ON).</param>
    /// <param name="joinType">Tipo de junção a aplicar.</param>
    /// <returns>A própria instância do builder para encadeamento fluente.</returns>
    IQueryBuilder Join(string table, string condition, JoinType joinType = JoinType.Inner);

    /// <summary>
    /// Define as colunas para a cláusula GROUP BY.
    /// </summary>
    /// <param name="columns">Colunas de agrupamento.</param>
    /// <returns>A própria instância do builder para encadeamento fluente.</returns>
    IQueryBuilder GroupBy(params string[] columns);

    /// <summary>
    /// Define a cláusula de restrição HAVING para agrupamentos.
    /// </summary>
    /// <param name="condition">Condição de filtro para o agrupamento.</param>
    /// <returns>A própria instância do builder para encadeamento fluente.</returns>
    IQueryBuilder Having(string condition);

    /// <summary>
    /// Aplica o modificador DISTINCT para eliminar registros duplicados na projeção.
    /// </summary>
    /// <returns>A própria instância do builder para encadeamento fluente.</returns>
    IQueryBuilder Distinct();

    /// <summary>
    /// Aninha uma subconsulta como tabela derivada com um alias específico.
    /// </summary>
    /// <param name="subQueryBuilder">Instância do builder contendo a subconsulta.</param>
    /// <param name="alias">Alias da tabela derivada resultante.</param>
    /// <returns>A própria instância do builder para encadeamento fluente.</returns>
    IQueryBuilder SubQuery(IQueryBuilder subQueryBuilder, string alias);

    /// <summary>
    /// Combina os resultados com outra consulta eliminando duplicatas (UNION).
    /// </summary>
    /// <param name="queryBuilder">A outra consulta a ser unificada.</param>
    /// <returns>A própria instância do builder para encadeamento fluente.</returns>
    IQueryBuilder Union(IQueryBuilder queryBuilder);

    /// <summary>
    /// Combina os resultados com outra consulta preservando duplicatas (UNION ALL).
    /// </summary>
    /// <param name="queryBuilder">A outra consulta a ser unificada.</param>
    /// <returns>A própria instância do builder para encadeamento fluente.</returns>
    IQueryBuilder UnionAll(IQueryBuilder queryBuilder);

    /// <summary>
    /// Adiciona a função de janela analítica ROW_NUMBER() OVER (...) na projeção.
    /// </summary>
    /// <param name="partitionBy">Expressão de particionamento (PARTITION BY).</param>
    /// <param name="orderBy">Expressão de ordenação da janela (ORDER BY).</param>
    /// <param name="alias">Alias da coluna gerada (padrão 'RowNumber').</param>
    /// <returns>A própria instância do builder para encadeamento fluente.</returns>
    IQueryBuilder WithRowNumber(string partitionBy, string orderBy, string alias = "RowNumber");

    /// <summary>
    /// Adiciona a função de janela analítica RANK() OVER (...) na projeção.
    /// </summary>
    /// <param name="partitionBy">Expressão de particionamento (PARTITION BY).</param>
    /// <param name="orderBy">Expressão de ordenação da janela (ORDER BY).</param>
    /// <param name="alias">Alias da coluna gerada (padrão 'Rank').</param>
    /// <returns>A própria instância do builder para encadeamento fluente.</returns>
    IQueryBuilder WithRank(string partitionBy, string orderBy, string alias = "Rank");

    /// <summary>
    /// Compila e retorna a instrução SQL finalizada.
    /// </summary>
    /// <returns>A string SQL construída.</returns>
    string BuildQuery();
}
