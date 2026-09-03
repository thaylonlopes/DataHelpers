using QueryBuilder.Helpers.Enums;
using System.Text;

namespace QueryBuilder.Helpers.Core;

/// <summary>
/// Classe base abstrata para todos os construtores de consulta da biblioteca QueryBuilder.
/// </summary>
public abstract class QueryBuilderBase : IQueryBuilder
{
    protected StringBuilder QueryBuilderInternal;

    public QueryBuilderBase()
    {
        QueryBuilderInternal = new StringBuilder();
    }

    public abstract IQueryBuilder Select(params string[] columns);
    public abstract IQueryBuilder From(string table);
    public abstract IQueryBuilder Where(string condition);
    public abstract IQueryBuilder And(string condition);
    public abstract IQueryBuilder Or(string condition);
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
        var partitionClause = string.IsNullOrWhiteSpace(partitionBy) ? "" : $"PARTITION BY {partitionBy} ";
        var windowFunc = $", ROW_NUMBER() OVER ({partitionClause}ORDER BY {orderBy}) AS {alias}";
        QueryBuilderInternal.Append(windowFunc);
        return this;
    }

    public virtual IQueryBuilder WithRank(string partitionBy, string orderBy, string alias = "Rank")
    {
        var partitionClause = string.IsNullOrWhiteSpace(partitionBy) ? "" : $"PARTITION BY {partitionBy} ";
        var windowFunc = $", RANK() OVER ({partitionClause}ORDER BY {orderBy}) AS {alias}";
        QueryBuilderInternal.Append(windowFunc);
        return this;
    }

    public abstract string BuildQuery();
}