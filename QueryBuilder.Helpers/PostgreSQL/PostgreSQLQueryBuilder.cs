using QueryBuilder.Helpers.Core;
using QueryBuilder.Helpers.Enums;
using QueryBuilder.Helpers.Security;
using System.Text;

namespace QueryBuilder.Helpers.PostgreSQL
{
    public class PostgreSQLQueryBuilder : QueryBuilderBase
    {
        public PostgreSQLQueryBuilder(int initialCapacity = DefaultInitialCapacity) : base(initialCapacity)
        {
        }

        public override IQueryBuilder Select(params string[] columns)
        {
            QueryBuilderInternal.Append("SELECT ");
            AppendColumns(columns);
            return this;
        }

        public override IQueryBuilder From(string table)
        {
            QueryBuilderInternal.Append(" FROM ").Append(table);
            return this;
        }

        public override IQueryBuilder Where(string condition)
        {
            QueryBuilderInternal.Append(" WHERE ").Append(condition);
            return this;
        }

        public override IQueryBuilder And(string condition)
        {
            QueryBuilderInternal.Append(" AND ").Append(condition);
            return this;
        }

        public override IQueryBuilder Or(string condition)
        {
            QueryBuilderInternal.Append(" OR ").Append(condition);
            return this;
        }

        public override IQueryBuilder Limit(int limit)
        {
            QueryBuilderInternal.Append(" LIMIT ").Append(limit);
            return this;
        }

        public override IQueryBuilder Offset(int offset)
        {
            QueryBuilderInternal.Append(" OFFSET ").Append(offset); return this;
        }

        public override IQueryBuilder OrderBy(string column, bool ascending = true)
        {
            var escaped = SqlIdentifierValidator.ValidateAndEscape(column, SqlDialect.PostgreSql);
            QueryBuilderInternal.Append(" ORDER BY ").Append(escaped).Append(ascending ? " ASC" : " DESC");
            return this;
        }

        public override IQueryBuilder Join(string table, string condition, JoinType joinType = JoinType.Inner)
        {
            var joinTypeStr = joinType switch
            {
                JoinType.Inner => "INNER JOIN",
                JoinType.Left => "LEFT JOIN",
                JoinType.Right => "RIGHT JOIN",
                JoinType.Full => "FULL JOIN",
                _ => "INNER JOIN"
            };
            QueryBuilderInternal.Append(' ').Append(joinTypeStr).Append(' ').Append(table).Append(" ON ").Append(condition);
            return this;
        }

        public override IQueryBuilder GroupBy(params string[] columns)
        {
            if (columns != null && columns.Length > 0)
            {
                QueryBuilderInternal.Append(" GROUP BY ");
                AppendColumns(columns);
            }
            return this;
        }

        public override IQueryBuilder Having(string condition)
        {
            QueryBuilderInternal.Append(" HAVING ").Append(condition);
            return this;
        }

        public override IQueryBuilder Distinct()
        {
            QueryBuilderInternal.Insert(7, "DISTINCT ");
            return this;
        }

        public override IQueryBuilder SubQuery(IQueryBuilder subQueryBuilder, string alias)
        {
            QueryBuilderInternal.Append("(").Append(subQueryBuilder.BuildQuery()).Append(") AS ").Append(alias);
            return this;
        }

        public override IQueryBuilder Union(IQueryBuilder queryBuilder)
        {
            QueryBuilderInternal.Append(" UNION ").Append(queryBuilder.BuildQuery());
            return this;
        }

        public override IQueryBuilder UnionAll(IQueryBuilder queryBuilder)
        {
            QueryBuilderInternal.Append(" UNION ALL ").Append(queryBuilder.BuildQuery());
            return this;
        }

        public override string BuildQuery()
        {
            return QueryBuilderInternal.ToString();
        }
    }
}