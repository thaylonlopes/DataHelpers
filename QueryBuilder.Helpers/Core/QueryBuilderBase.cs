using QueryBuilder.Helpers.Enums;
using QueryBuilder.Helpers.Security;
using System.Text;

namespace QueryBuilder.Helpers.Core;

/// <summary>
/// Classe base abstrata para todos os construtores de consulta da biblioteca QueryBuilder.
/// </summary>
public abstract class QueryBuilderBase : IQueryBuilder
{
    /// <summary>
    /// Instância interna do <see cref="StringBuilder"/> utilizada na montagem incremental da instrução SQL.
    /// </summary>
    protected StringBuilder QueryBuilderInternal;

    /// <summary>
    /// Capacidade inicial padrão pré-alocada (256 caracteres) para evitar redimensionamentos sucessivos de buffer.
    /// </summary>
    public const int DefaultInitialCapacity = 256;

    /// <summary>
    /// Inicializa uma nova instância de <see cref="QueryBuilderBase"/> com capacidade inicial configurável.
    /// </summary>
    /// <param name="initialCapacity">Capacidade inicial em caracteres do buffer interno.</param>
    public QueryBuilderBase(int initialCapacity = DefaultInitialCapacity)
    {
        QueryBuilderInternal = new StringBuilder(initialCapacity);
    }

    /// <inheritdoc />
    public abstract IQueryBuilder Select(params string[] columns);

    /// <inheritdoc />
    public abstract IQueryBuilder From(string table);

    /// <inheritdoc />
    public abstract IQueryBuilder Where(string condition);

    /// <inheritdoc />
    public abstract IQueryBuilder And(string condition);

    /// <inheritdoc />
    public abstract IQueryBuilder Or(string condition);

    /// <inheritdoc />
    public virtual IQueryBuilder WhereLike(string column, string searchTerm, bool escapeWildcards = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(column);
        ArgumentNullException.ThrowIfNull(searchTerm);

        var term = escapeWildcards
            ? SqlIdentifierValidator.EscapeLikeWildcards(searchTerm)
            : searchTerm;

        return Where($"{column} LIKE '%{term}%'");
    }

    /// <inheritdoc />
    public abstract IQueryBuilder OrderBy(string column, bool ascending = true);

    /// <inheritdoc />
    public abstract IQueryBuilder Limit(int limit);

    /// <inheritdoc />
    public abstract IQueryBuilder Offset(int offset);

    /// <inheritdoc />
    public abstract IQueryBuilder Join(string table, string condition, JoinType joinType = JoinType.Inner);

    /// <inheritdoc />
    public abstract IQueryBuilder GroupBy(params string[] columns);

    /// <inheritdoc />
    public abstract IQueryBuilder Having(string condition);

    /// <inheritdoc />
    public abstract IQueryBuilder Distinct();

    /// <inheritdoc />
    public abstract IQueryBuilder SubQuery(IQueryBuilder subQueryBuilder, string alias);

    /// <inheritdoc />
    public abstract IQueryBuilder Union(IQueryBuilder queryBuilder);

    /// <inheritdoc />
    public abstract IQueryBuilder UnionAll(IQueryBuilder queryBuilder);

    /// <inheritdoc />
    public virtual IQueryBuilder WithRowNumber(string partitionBy, string orderBy, string alias = "RowNumber")
    {
        QueryBuilderInternal.Append(", ROW_NUMBER() OVER (");
        if (!string.IsNullOrWhiteSpace(partitionBy))
        {
            QueryBuilderInternal.Append("PARTITION BY ").Append(partitionBy).Append(' ');
        }
        QueryBuilderInternal.Append("ORDER BY ").Append(orderBy).Append(") AS ").Append(alias);
        return this;
    }

    /// <inheritdoc />
    public virtual IQueryBuilder WithRank(string partitionBy, string orderBy, string alias = "Rank")
    {
        QueryBuilderInternal.Append(", RANK() OVER (");
        if (!string.IsNullOrWhiteSpace(partitionBy))
        {
            QueryBuilderInternal.Append("PARTITION BY ").Append(partitionBy).Append(' ');
        }
        QueryBuilderInternal.Append("ORDER BY ").Append(orderBy).Append(") AS ").Append(alias);
        return this;
    }

    /// <summary>
    /// Anexa as colunas ao buffer interno separadas por vírgula.
    /// </summary>
    /// <param name="columns">Lista de colunas.</param>
    protected void AppendColumns(params string[] columns)
    {
        if (columns == null || columns.Length == 0)
        {
            QueryBuilderInternal.Append('*');
            return;
        }

        for (int i = 0; i < columns.Length; i++)
        {
            if (i > 0)
            {
                QueryBuilderInternal.Append(", ");
            }
            QueryBuilderInternal.Append(columns[i]);
        }
    }

    /// <summary>
    /// Anexa um span de caracteres ao buffer interno sem alocação intermediária de string.
    /// </summary>
    /// <param name="span">O span contendo os caracteres.</param>
    protected void AppendSpan(ReadOnlySpan<char> span)
    {
        QueryBuilderInternal.Append(span);
    }

    /// <inheritdoc />
    public abstract string BuildQuery();
}