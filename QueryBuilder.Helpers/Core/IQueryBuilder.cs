using QueryBuilder.Helpers.Enums;

namespace QueryBuilder.Helpers.Core;

/// <summary>
/// Contrato unificado para construção fluente de consultas SQL e NoSQL.
/// </summary>
public interface IQueryBuilder
{
    IQueryBuilder Select(params string[] columns);
    IQueryBuilder From(string table);
    IQueryBuilder Where(string condition);
    IQueryBuilder And(string condition);
    IQueryBuilder Or(string condition);
    IQueryBuilder WhereLike(string column, string searchTerm, bool escapeWildcards = true);
    IQueryBuilder OrderBy(string column, bool ascending = true);
    IQueryBuilder Limit(int limit);
    IQueryBuilder Offset(int offset);
    IQueryBuilder Join(string table, string condition, JoinType joinType = JoinType.Inner);
    IQueryBuilder GroupBy(params string[] columns);
    IQueryBuilder Having(string condition);
    IQueryBuilder Distinct();
    IQueryBuilder SubQuery(IQueryBuilder subQueryBuilder, string alias);
    IQueryBuilder Union(IQueryBuilder queryBuilder);
    IQueryBuilder UnionAll(IQueryBuilder queryBuilder);
    IQueryBuilder WithRowNumber(string partitionBy, string orderBy, string alias = "RowNumber");
    IQueryBuilder WithRank(string partitionBy, string orderBy, string alias = "Rank");
    string BuildQuery();
}
