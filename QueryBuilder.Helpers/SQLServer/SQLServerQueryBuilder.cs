using QueryBuilder.Helpers.Core;
using QueryBuilder.Helpers.Enums;
using QueryBuilder.Helpers.Security;
using System.Text;

namespace QueryBuilder.Helpers.SQLServer;

/// <summary>
/// Construtor de consultas fluente especializado para o dialeto Microsoft SQL Server (T-SQL).
/// </summary>
public class SQLServerQueryBuilder : QueryBuilderBase
{
    /// <summary>
    /// Inicializa uma nova instância de <see cref="SQLServerQueryBuilder"/> com capacidade inicial configurável.
    /// </summary>
    /// <param name="initialCapacity">Capacidade inicial do buffer em caracteres.</param>
    public SQLServerQueryBuilder(int initialCapacity = DefaultInitialCapacity) : base(initialCapacity)
    {
    }

    /// <inheritdoc />
    public override IQueryBuilder Select(params string[] columns)
    {
        QueryBuilderInternal.Append("SELECT ");
        AppendColumns(columns);
        return this;
    }

    /// <inheritdoc />
    public override IQueryBuilder From(string table)
    {
        QueryBuilderInternal.Append(" FROM ").Append(table);
        return this;
    }

    /// <inheritdoc />
    public override IQueryBuilder Where(string condition)
    {
        QueryBuilderInternal.Append(" WHERE ").Append(condition);
        return this;
    }

    /// <inheritdoc />
    public override IQueryBuilder And(string condition)
    {
        QueryBuilderInternal.Append(" AND ").Append(condition);
        return this;
    }

    /// <inheritdoc />
    public override IQueryBuilder Or(string condition)
    {
        QueryBuilderInternal.Append(" OR ").Append(condition);
        return this;
    }

    /// <inheritdoc />
    public override IQueryBuilder OrderBy(string column, bool ascending = true)
    {
        var escaped = SqlIdentifierValidator.ValidateAndEscape(column, SqlDialect.SqlServer);
        QueryBuilderInternal.Append(" ORDER BY ").Append(escaped).Append(ascending ? " ASC" : " DESC");
        return this;
    }

    /// <inheritdoc />
    public override IQueryBuilder Limit(int limit)
    {
        QueryBuilderInternal.Append(" OFFSET 0 ROWS FETCH NEXT ").Append(limit).Append(" ROWS ONLY");
        return this;
    }

    /// <inheritdoc />
    public override IQueryBuilder Offset(int offset)
    {
        QueryBuilderInternal.Append(" OFFSET ").Append(offset).Append(" ROWS");
        return this;
    }

    /// <inheritdoc />
    public override IQueryBuilder Join(string table, string condition, JoinType joinType = JoinType.Inner)
    {
        string joinTypeString = joinType switch
        {
            JoinType.Inner => " INNER JOIN ",
            JoinType.Left => " LEFT JOIN ",
            JoinType.Right => " RIGHT JOIN ",
            JoinType.Full => " FULL OUTER JOIN ",
            _ => " INNER JOIN "
        };

        QueryBuilderInternal.Append(joinTypeString).Append(table).Append(" ON ").Append(condition);
        return this;
    }

    /// <inheritdoc />
    public override IQueryBuilder GroupBy(params string[] columns)
    {
        QueryBuilderInternal.Append(" GROUP BY ");
        for (int i = 0; i < columns.Length; i++)
        {
            if (i > 0) QueryBuilderInternal.Append(", ");
            QueryBuilderInternal.Append(columns[i]);
        }
        return this;
    }

    /// <inheritdoc />
    public override IQueryBuilder Having(string condition)
    {
        QueryBuilderInternal.Append(" HAVING ").Append(condition);
        return this;
    }

    /// <inheritdoc />
    public override IQueryBuilder Distinct()
    {
        QueryBuilderInternal.Insert(0, "DISTINCT ");
        return this;
    }

    /// <inheritdoc />
    public override IQueryBuilder SubQuery(IQueryBuilder subQueryBuilder, string alias)
    {
        QueryBuilderInternal.Append(" (").Append(subQueryBuilder.BuildQuery()).Append(") AS ").Append(alias);
        return this;
    }

    /// <inheritdoc />
    public override IQueryBuilder Union(IQueryBuilder queryBuilder)
    {
        QueryBuilderInternal.Append(" UNION ").Append(queryBuilder.BuildQuery());
        return this;
    }

    /// <inheritdoc />
    public override IQueryBuilder UnionAll(IQueryBuilder queryBuilder)
    {
        QueryBuilderInternal.Append(" UNION ALL ").Append(queryBuilder.BuildQuery());
        return this;
    }

    /// <inheritdoc />
    public override string BuildQuery()
    {
        return QueryBuilderInternal.ToString().Trim();
    }
}