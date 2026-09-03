using MongoDB.Bson;
using QueryBuilder.Helpers.Core;
using QueryBuilder.Helpers.Enums;
using System.Text;
using System.Text.Json;

namespace QueryBuilder.Helpers.MongoDB
{
    public class MongoDBQueryBuilder : QueryBuilderBase
    {
        private readonly JsonElement _filter;
        private readonly JsonElement _projection;
        private List<JsonDocument> _aggregationPipeline = new();
        private int _skip;
        private int _limit;

        public MongoDBQueryBuilder()
        {
            _filter = JsonDocument.Parse("{}").RootElement;
            _projection = JsonDocument.Parse("{}").RootElement;
            QueryBuilderInternal = new StringBuilder();
            _skip = 0;
            _limit = 0;
        }

        public override IQueryBuilder Select(params string[] fields)
        {
            QueryBuilderInternal.Append("\"projection\": { ");
            foreach (var field in fields)
            {
                QueryBuilderInternal.Append($"\"{field}\": 1, ");
            }
            QueryBuilderInternal.Append(" },");
            return this;
        }

        public override IQueryBuilder From(string collection)
        {
            QueryBuilderInternal.Insert(0, $"db.{collection}.find({{ ");
            return this;
        }

        public override IQueryBuilder Where(string condition)
        {
            QueryBuilderInternal.Append($"\"{condition}\",");
            return this;
        }

        public override IQueryBuilder And(string condition)
        {
            QueryBuilderInternal.Insert(QueryBuilderInternal.Length - 2, $"\"$and\": [{{\"{condition}\"}}], ");
            return this;
        }

        public override IQueryBuilder Or(string condition)
        {
            QueryBuilderInternal.Insert(QueryBuilderInternal.Length - 2, $"\"$or\": [{{\"{condition}\"}}], ");
            return this;
        }

        public override IQueryBuilder OrderBy(string field, bool ascending = true)
        {
            QueryBuilderInternal.Append($"\"sort\": {{ \"{field}\": {(ascending ? 1 : -1)} }},");
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

        public MongoDBQueryBuilder Aggregation()
        {
            _aggregationPipeline = new List<JsonDocument>();
            return this;
        }
        public MongoDBQueryBuilder Match(JsonElement filter)
        {
            var matchStage = JsonDocument.Parse($"{{ \"$match\": {filter.GetRawText()} }}");
            _aggregationPipeline.Add(matchStage);
            return this;
        }
        public MongoDBQueryBuilder Group(JsonElement groupBy)
        {
            var groupStage = JsonDocument.Parse($"{{ \"$group\": {groupBy.GetRawText()} }}");
            _aggregationPipeline.Add(groupStage);
            return this;
        }
        public MongoDBQueryBuilder Facet(JsonElement facets)
        {
            var facetStage = JsonDocument.Parse($"{{ \"$facet\": {facets.GetRawText()} }}");
            _aggregationPipeline.Add(facetStage);
            return this;
        }
        public override IQueryBuilder Distinct()
        {
            QueryBuilderInternal.Insert(7, "DISTINCT ");
            return this;
        }

        public override IQueryBuilder SubQuery(IQueryBuilder subQueryBuilder, string alias)
        {
            QueryBuilderInternal.Append($" ({subQueryBuilder.BuildQuery()}) AS {alias}");
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
        public override IQueryBuilder Join(string table, string condition, JoinType joinType = JoinType.Inner)
        {
            throw new NotSupportedException("Join type not supported in MongoDB use method Aggregation");
        }

        public override IQueryBuilder GroupBy(params string[] columns)
        {
            var groupStage = new BsonDocument
            {
                { "_id", new BsonDocument() }
            };
            foreach (var column in columns)
            {
                groupStage["_id"].AsBsonDocument.Add(column, $"${column}");
            }
            _aggregationPipeline.Add(JsonDocument.Parse($"{{ \"$group\": {groupStage.ToJson()} }}"));
            return this;
        }

        public override IQueryBuilder Having(string condition)
        {
            var matchStage = JsonDocument.Parse($"{{ \"$match\": {condition} }}");
            _aggregationPipeline.Add(matchStage);
            return this;
        }

        public List<JsonDocument> BuildAggregationPipeline()
        {
            return _aggregationPipeline;
        }
        public override string BuildQuery()
        {
            if (_skip > 0) QueryBuilderInternal.Append($"\"skip\": {_skip},");
            if (_limit > 0) QueryBuilderInternal.Append($"\"limit\": {_limit},");
            QueryBuilderInternal.Append("})");
            return QueryBuilderInternal.ToString().Replace(",}", "}").Replace(",)", ")");
        }

    }
}