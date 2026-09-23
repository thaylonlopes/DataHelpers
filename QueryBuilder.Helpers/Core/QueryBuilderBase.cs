using QueryBuilder.Helpers.Enums;
using QueryBuilder.Helpers.Security;
using System.Text;

namespace QueryBuilder.Helpers.Core;

/// <summary>
/// Classe base abstrata para todos os construtores de consulta da biblioteca QueryBuilder.
/// </summary>
public abstract class QueryBuilderBase : IQueryBuilder
{
    protected StringBuilder QueryBuilderInternal;

    public const int DefaultInitialCapacity = 256;

    public QueryBuilderBase(int initialCapacity = DefaultInitialCapacity)
    {
        QueryBuilderInternal = new StringBuilder(initialCapacity);
    }

    public abstract IQueryBuilder Select(params string[] columns);
    public abstract IQueryBuilder From(string table);
    public abstract IQueryBuilder Where(string condition);
    public abstract IQueryBuilder And(string condition);
    public abstract IQueryBuilder Or(string condition);

    public virtual IQueryBuilder WhereLike(string column, string searchTerm, bool escapeWildcards = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(column);
        ArgumentNullException.ThrowIfNull(searchTerm);

        var term = escapeWildcards
            ? SqlIdentifierValidator.EscapeLikeWildcards(searchTerm)
            : searchTerm;

        return Where($"{column} LIKE '%{term}%'");
    }

    public abstract IQueryBuilder OrderBy(string column, bool ascending = true);
    public abstract IQueryBuilder Limit(int limit);
    public abstract IQueryBuilder Offset(int offset);
    public abstract IQueryBuilder Join(string table, string condition, JoinType joinType = JoinType.Inner);
    public abstract IQueryBuilder GroupBy(params string[] columns);
    public abstract IQueryBuilder Having(string condition);
    public abstract IQueryBuilder Distinct();
    public abstract IQueryBuilder SubQuery(IQueryBuilder subQueryBuilder, string alias);
    public abstract IQueryBuilder Union(IQueryBuilder queryBuilder);
    public abstract IQueryBuilder UnionAll(IQueryBuilder queryBuilder);

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

    protected void AppendSpan(ReadOnlySpan<char> span)
    {
        QueryBuilderInternal.Append(span);
    }

    public abstract string BuildQuery();
}