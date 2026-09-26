using QueryBuilder.Helpers.Core;
using QueryBuilder.Helpers.Enums;
using QueryBuilder.Helpers.Security;
using System.Text;

namespace QueryBuilder.Helpers.Oracle;

/// <summary>
/// Construtor de consultas fluente para o dialeto Oracle Database.
/// </summary>
public class OracleQueryBuilder : QueryBuilderBase
{
    /// <summary>
    /// Inicializa uma nova instância de <see cref="OracleQueryBuilder"/> com capacidade inicial configurável.
    /// </summary>
    /// <param name="initialCapacity">Capacidade inicial do buffer em caracteres.</param>
    public OracleQueryBuilder(int initialCapacity = DefaultInitialCapacity) : base(initialCapacity)
    {
    }

    /// <inheritdoc/>
    public override IQueryBuilder Select(params string[] columns)
    {
        QueryBuilderInternal.Append("SELECT ");
        AppendColumns(columns);
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
        var escaped = SqlIdentifierValidator.ValidateAndEscape(column, SqlDialect.Oracle);
        QueryBuilderInternal.Append(" ORDER BY ").Append(escaped).Append(ascending ? " ASC" : " DESC");
        return this;
    }

    /// <inheritdoc/>
    public override IQueryBuilder Limit(int limit)
    {
        QueryBuilderInternal.Append(" FETCH FIRST ").Append(limit).Append(" ROWS ONLY");
        return this;
    }

    /// <inheritdoc/>
    public override IQueryBuilder Offset(int offset)
    {
        QueryBuilderInternal.Append(" OFFSET ").Append(offset).Append(" ROWS");
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
        if (columns != null && columns.Length > 0)
        {
            QueryBuilderInternal.Append(" GROUP BY ");
            AppendColumns(columns);
        }
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