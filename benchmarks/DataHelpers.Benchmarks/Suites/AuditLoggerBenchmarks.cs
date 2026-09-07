using System.Text.Json;
using AuditLogger.Models;
using AuditLogger.Services;
using BenchmarkDotNet.Attributes;
using DataHelpers.Benchmarks.Models;
using Microsoft.Extensions.Caching.Memory;

namespace DataHelpers.Benchmarks.Suites;

[MemoryDiagnoser]
public class AuditLoggerBenchmarks
{
    private AuditLogger.AuditLogger _auditLogger = null!;
    private OrderEntity _originalOrder = null!;
    private OrderEntity _updatedOrder = null!;
    private MemoryCacheAuditLogStorage _memoryStorage = null!;

    [GlobalSetup]
    public void Setup()
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        _memoryStorage = new MemoryCacheAuditLogStorage(memoryCache, TimeSpan.FromHours(1));
        _auditLogger = new AuditLogger.AuditLogger(_memoryStorage);

        _originalOrder = new OrderEntity
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-2026-ORIGINAL",
            CustomerName = "Cliente Antigo Ltda",
            CustomerEmail = "antigo@empresa.com",
            TotalAmount = 2500.00m,
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            IsActive = true,
            ItemCount = 5,
            City = "Curitiba",
            Country = "Brasil"
        };

        _updatedOrder = new OrderEntity
        {
            Id = _originalOrder.Id,
            OrderNumber = _originalOrder.OrderNumber,
            CustomerName = "Cliente Novo SA",
            CustomerEmail = _originalOrder.CustomerEmail,
            TotalAmount = 3200.00m,
            CreatedAt = _originalOrder.CreatedAt,
            IsActive = false,
            ItemCount = 8,
            City = _originalOrder.City,
            Country = _originalOrder.Country
        };
    }

    [Benchmark(Baseline = true, Description = "1. Mutation: Full Two-State Clones")]
    public string Audit_Mutation_FullSnapshots()
    {
        var beforeJson = JsonSerializer.Serialize(_originalOrder);
        var afterJson = JsonSerializer.Serialize(_updatedOrder);
        return $"{{\"Before\":{beforeJson},\"After\":{afterJson}}}";
    }

    [Benchmark(Description = "1. Mutation: AuditLogger LogUpdate Differential Diff")]
    public Guid Audit_Mutation_DifferentialDiff()
    {
        return _auditLogger.LogUpdate(_originalOrder, _updatedOrder, "audit.operator@enterprise.com");
    }

    [Benchmark(Description = "2. Storage: Serialization to String Buffer")]
    public string Audit_Storage_UnbufferedSerialization()
    {
        var entry = new AuditLogEntry("UPDATE", "audit.operator@enterprise.com", nameof(OrderEntity), JsonSerializer.Serialize(_updatedOrder));
        return JsonSerializer.Serialize(entry);
    }

    [Benchmark(Description = "2. Storage: MemoryCacheAuditLogStorage Direct")]
    public Guid Audit_Storage_MemoryCache()
    {
        var entry = new AuditLogEntry("UPDATE", "audit.operator@enterprise.com", nameof(OrderEntity), "{\"Status\":\"Mutated\"}");
        _memoryStorage.SaveLog(entry);
        return entry.Id;
    }

    [Benchmark(Description = "3. Structure: Manual Unstructured String Formatting")]
    public string Audit_Structure_UnstructuredText()
    {
        return $"[{DateTime.UtcNow:O}] USER: operator ACTION: UPDATE ENTITY: OrderEntity ID: {_updatedOrder.Id} AMOUNT: {_updatedOrder.TotalAmount}";
    }

    [Benchmark(Description = "3. Structure: AuditLogEntry Strongly Typed")]
    public AuditLogEntry Audit_Structure_TypedEntry()
    {
        return new AuditLogEntry("UPDATE", "operator", nameof(OrderEntity), "{\"TotalAmount\":3200.00}");
    }
}
