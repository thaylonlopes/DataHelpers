using QueryBuilder.Helpers.Core;
using QueryBuilder.Helpers.Enums;
using System.Text;

namespace QueryBuilder.Helpers.DynamoDB
{
    public class DynamoDBQueryBuilder : QueryBuilderBase
    {
        private readonly StringBuilder _conditionExpression;
        private int _skip;
        private int _limit;
        private string _indexName;

        public DynamoDBQueryBuilder()
        {
            QueryBuilderInternal = new StringBuilder();
            _conditionExpression = new StringBuilder();
            _skip = 0;
            _limit = 0;
            _indexName = string.Empty;
        }

        public override IQueryBuilder Select(params string[] attributes)
        {
            QueryBuilderInternal.Append("SELECT ");
            QueryBuilderInternal.Append(attributes.Length > 0 ? string.Join(", ", attributes) : "*");
            return this;
        }

        public override IQueryBuilder From(string table)
        {
            QueryBuilderInternal.Append($" FROM {table}");
            return this;
        }

        public override IQueryBuilder Where(string condition)
        {
            QueryBuilderInternal.Append($" WHERE {condition}");
            return this;
        }

        public override IQueryBuilder And(string condition)
        {
            QueryBuilderInternal.Append($" AND {condition}");
            return this;
        }

        public override IQueryBuilder Or(string condition)
        {
            QueryBuilderInternal.Append($" OR {condition}");
            return this;
        }

        public override IQueryBuilder OrderBy(string attribute, bool ascending = true)
        {
            QueryBuilderInternal.Append($" ORDER BY {attribute} {(ascending ? "ASC" : "DESC")}");
            return this;
        }

        public override IQueryBuilder Offset(int offset)
        {
            _skip = offset;
            return this;
        }

        public override IQueryBuilder Limit(int limit)
        {
            _limit = limit;
            return this;
        }

        public IQueryBuilder UseIndex(string indexName)
        {
            _indexName = indexName;
            return this;
        }

        public IQueryBuilder WithConditionExpression(string expression)
        {
            if (_conditionExpression.Length > 0) _conditionExpression.Append(" AND ");
            _conditionExpression.Append(expression);
            return this;
        }

        public override IQueryBuilder Join(string table, string condition, JoinType joinType = JoinType.Inner)
        {
            return this;
        }

        public override IQueryBuilder GroupBy(params string[] columns)
        {
            return this;
        }

        public override IQueryBuilder Having(string condition)
        {
            return this;
        }

        public override IQueryBuilder Distinct()
        {
            return this;
        }

        public override IQueryBuilder SubQuery(IQueryBuilder subQueryBuilder, string alias)
        {
            return this;
        }

        public override IQueryBuilder Union(IQueryBuilder queryBuilder)
        {
            return this;
        }

        public override IQueryBuilder UnionAll(IQueryBuilder queryBuilder)
        {
            return this;
        }

        public override string BuildQuery()
        {
            var finalQuery = new StringBuilder();
            finalQuery.Append("{ ");
            finalQuery.Append(QueryBuilderInternal);
            if (_conditionExpression.Length > 0)
                finalQuery.Append($"KeyConditionExpression: \"{_conditionExpression}\", ");
            if (!string.IsNullOrEmpty(_indexName))
                finalQuery.Append($"IndexName: \"{_indexName}\", ");
            if (_limit > 0)
                finalQuery.Append($"Limit: {_limit}, ");
            finalQuery.Append("}");
            return finalQuery.ToString().Replace(", }", " }");
        }

    }
}
