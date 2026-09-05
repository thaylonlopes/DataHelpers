using QueryBuilder.Helpers.Core;
using QueryBuilder.Helpers.Enums;
using System.Text;

namespace QueryBuilder.Helpers.MySQL;

/// <summary>
/// Construtor de consultas fluente para o dialeto MySQL.
/// </summary>
public class MySQLQueryBuilder : QueryBuilderBase
{
    /// <summary>
    /// Inicializa uma nova instância de <see cref="MySQLQueryBuilder"/>.
    /// </summary>
    public MySQLQueryBuilder()
    public MySQLQueryBuilder(int initialCapacity = DefaultInitialCapacity) : base(initialCapacity)
    {
        QueryBuilderInternal = new StringBuilder();
    }

    /// <inheritdoc/>
    public override IQueryBuilder Select(params string[] columns)
    {
        QueryBuilderInternal.Append("SELECT ");
        QueryBuilderInternal.Append(columns.Length > 0 ? string.Join(", ", columns) : "*");
        return this;
    }

    /// <inheritdoc/>
    public override IQueryBuilder From(string table)
    {
        QueryBuilderInternal.Append(" FROM ").Append(table);
        return this;
    }

    /// <inheritdoc/>
    public override IQueryBuilder Where(string condition)
    {
        QueryBuilderInternal.Append(" WHERE ").Append(condition);
        return this;
    }

    /// <inheritdoc/>
    public override IQueryBuilder And(string condition)
    {
        QueryBuilderInternal.Append(" AND ").Append(condition);
        return this;
    }

    /// <inheritdoc/>
    public override IQueryBuilder Or(string condition)
    {
        QueryBuilderInternal.Append(" OR ").Append(condition);
        return this;
    }

    /// <inheritdoc/>
    public override IQueryBuilder OrderBy(string column, bool ascending = true)
    {
        QueryBuilderInternal.Append(" ORDER BY ").Append(column).Append(ascending ? " ASC" : " DESC");
        return this;
    }

    /// <inheritdoc/>
    public override IQueryBuilder Limit(int limit)
    {
        QueryBuilderInternal.Append(" LIMIT ").Append(limit);
        return this;
    }

    /// <inheritdoc/>
    public override IQueryBuilder Offset(int offset)
    {
        QueryBuilderInternal.Append(" OFFSET ").Append(offset);
        return this;
    }

    /// <inheritdoc/>
    public override IQueryBuilder Join(string table, string condition, JoinType joinType = JoinType.Inner)
    {
        var joinTypeStr = joinType switch
        {
            JoinType.Inner => " INNER JOIN ",
            JoinType.Left => " LEFT JOIN ",
            JoinType.Right => " RIGHT JOIN ",
            JoinType.Full => " FULL JOIN ",
            _ => " INNER JOIN "
        };
        QueryBuilderInternal.Append(joinTypeStr).Append(table).Append(" ON ").Append(condition);
        return this;
    }

    /// <inheritdoc/>
    public override IQueryBuilder GroupBy(params string[] columns)
    {
        QueryBuilderInternal.Append(" GROUP BY ").Append(string.Join(", ", columns));
        return this;
    }

    /// <inheritdoc/>
    public override IQueryBuilder Having(string condition)
    {
        QueryBuilderInternal.Append(" HAVING ").Append(condition);
        return this;
    }

    /// <inheritdoc/>
    public override IQueryBuilder Distinct()
    {
        QueryBuilderInternal.Insert(7, "DISTINCT ");
        return this;
    }

    /// <inheritdoc/>
    public override IQueryBuilder SubQuery(IQueryBuilder subQueryBuilder, string alias)
    {
        QueryBuilderInternal.Append(" (").Append(subQueryBuilder.BuildQuery()).Append(") AS ").Append(alias);
        return this;
    }

    /// <inheritdoc/>
    public override IQueryBuilder Union(IQueryBuilder queryBuilder)
    {
        QueryBuilderInternal.Append(" UNION ").Append(queryBuilder.BuildQuery());
        return this;
    }

    /// <inheritdoc/>
    public override IQueryBuilder UnionAll(IQueryBuilder queryBuilder)
    {
        QueryBuilderInternal.Append(" UNION ALL ").Append(queryBuilder.BuildQuery());
        return this;
    }

    /// <inheritdoc/>
    public override string BuildQuery()
    {
        return QueryBuilderInternal.ToString();
    }
}
