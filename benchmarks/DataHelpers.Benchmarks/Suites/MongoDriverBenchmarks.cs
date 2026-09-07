using BenchmarkDotNet.Attributes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDriver.Helpers;
using MongoDriver.Helpers.Models;

namespace DataHelpers.Benchmarks.Suites;

public class OrderMongoDocument : Document<Guid>
{
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}

[MemoryDiagnoser]
public class MongoDriverBenchmarks
{
    private OrderMongoDocument _sampleDoc = null!;
    private Guid _searchId;

    [GlobalSetup]
    public void Setup()
    {
        _searchId = Guid.NewGuid();
        _sampleDoc = new OrderMongoDocument
        {
            Id = _searchId,
            OrderNumber = "ORD-2026-MONGO",
            CustomerName = "Mega Store Varejo",
            TotalAmount = 9850.25m,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
    }

    [Benchmark(Baseline = true, Description = "1. Filters: Raw BsonDocument Manual Assembly")]
    public BsonDocument Mongo_Filters_RawBson()
    {
        return new BsonDocument
        {
            { "CustomerName", "Mega Store Varejo" },
            { "TotalAmount", new BsonDocument("$gte", 5000) },
            { "IsActive", true }
        };
    }

    [Benchmark(Description = "1. Filters: MongoDriver Filters Fluently Composed")]
    public FilterDefinition<OrderMongoDocument> Mongo_Filters_FluentHelper()
    {
        var f1 = Filters.Equals<OrderMongoDocument>("CustomerName", "Mega Store Varejo");
        var f2 = Filters.GreaterThanOrEquals<OrderMongoDocument>("TotalAmount", 5000);
        var f3 = Filters.Equals<OrderMongoDocument>("IsActive", true);
        return Builders<OrderMongoDocument>.Filter.And(f1, f2, f3);
    }

    [Benchmark(Description = "2. Serialization: Full Document Bson Assembly")]
    public BsonDocument Mongo_Serialization_FullDocument()
    {
        return new BsonDocument
        {
            { "_id", _sampleDoc.Id.ToString() },
            { "OrderNumber", _sampleDoc.OrderNumber },
            { "CustomerName", _sampleDoc.CustomerName },
            { "TotalAmount", _sampleDoc.TotalAmount.ToString() },
            { "CreatedAt", _sampleDoc.CreatedAt.ToString("O") },
            { "IsActive", _sampleDoc.IsActive }
        };
    }

    [Benchmark(Description = "2. Serialization: Projected Subset Serialization")]
    public BsonDocument Mongo_Serialization_ProjectedSubset()
    {
        return new BsonDocument
        {
            { "_id", _sampleDoc.Id.ToString() },
            { "TotalAmount", _sampleDoc.TotalAmount.ToString() }
        };
    }

    [Benchmark(Description = "3. IdSeek: Generic Expression Filter")]
    public FilterDefinition<OrderMongoDocument> Mongo_IdSeek_GenericExpression()
    {
        return Builders<OrderMongoDocument>.Filter.Where(x => x.Id == _searchId);
    }

    [Benchmark(Description = "3. IdSeek: Filters.ById Direct Key Lookup")]
    public FilterDefinition<OrderMongoDocument> Mongo_IdSeek_FiltersById()
    {
        return Filters.ById<OrderMongoDocument>(_searchId);
    }
}
