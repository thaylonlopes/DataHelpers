using System.Globalization;
using AuditLogger;
using AuditLogger.Models;
using Caching.Helpers;
using Caching.Helpers.Utils;
using CsvHelper.Configuration;
using Dapper.Helpers;
using Dapper.Helpers.Context;
using DataImportExport.Helpers;
using DataImportExport.Helpers.Models;
using DataMapping.Helpers;
using Example.Showcase.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Driver;
using MongoDriver.Helpers;
using MongoDriver.Helpers.Interface.Context;
using Moq;
using PagingFiltering.Helpers.Implementations;
using QueryBuilder.Helpers.PostgreSQL;

namespace Example.Showcase;

public static class Program
{
    public static async Task Main()
    {
        var (csvFilePath, importedDtos) = await RunDataIngestionStageAsync();

        var domainEntities = RunDataMappingStage(importedDtos);

        RunQueryBuilderStage();

        var persistedOrders = await RunRelationalPersistenceStageAsync(domainEntities);

        var pagedOrders = RunPagingFilteringStage(persistedOrders);

        await RunHierarchicalCachingStageAsync(persistedOrders, pagedOrders.Items);

        var (originalOrder, updatedOrder, auditLogId) = RunAuditLoggingStage(persistedOrders);

        await RunMongoProjectionStageAsync(updatedOrder, auditLogId);

        CleanupTemporaryArtifacts(csvFilePath);

        PrintPipelineSummary(persistedOrders, updatedOrder);
    }

    private static async Task<(string CsvFilePath, List<OrderImportDto> Dtos)> RunDataIngestionStageAsync()
    {
        var sampleOrders = GenerateSampleOrders();
        var tempCsvPath = Path.Combine(Path.GetTempPath(), $"showcase_orders_{Guid.NewGuid():N}.csv");

        var csvSettings = new CsvSettings
        {
            HasHeaderRecord = true,
            Delimiter = ","
        };

        var exporter = new CsvDataExporter(csvSettings);
        await exporter.ExportAsync(tempCsvPath, sampleOrders);

        var logger = new SimpleLogger();
        var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Delimiter = ","
        };

        var importer = new CsvDataImporter(logger, csvConfig, csvSettings);
        var imported = (await importer.ImportAsync<OrderImportDto>(tempCsvPath)).ToList();

        Console.WriteLine($"DataImportExport: {imported.Count} pedidos importados de lote CSV.");

        return (tempCsvPath, imported);
    }

    private static List<OrderEntity> RunDataMappingStage(IEnumerable<OrderImportDto> dtos)
    {
        var mapper = new SimpleMapper();
        var entities = new List<OrderEntity>();

        foreach (var dto in dtos)
        {
            var result = mapper.TryMap<OrderImportDto, OrderEntity>(dto);
            if (result.IsSuccess && result.Value is not null)
            {
                var entity = result.Value;
                entity.TotalAmount = entity.Quantity * entity.UnitPrice;
                entities.Add(entity);
            }
        }

        Console.WriteLine($"DataMapping: {entities.Count} pedidos mapeados com Result Pattern.");

        return entities;
    }

    private static void RunQueryBuilderStage()
    {
        var queryBuilder = new PostgreSQLQueryBuilder();
        var generatedSql = queryBuilder
            .Select("OrderNumber", "CustomerName", "TotalAmount", "Status")
            .From("Orders")
            .Where("TotalAmount >= 1000.00")
            .And("Status = 'Processing'")
            .OrderBy("TotalAmount", ascending: false)
            .BuildQuery();

        Console.WriteLine($"QueryBuilder: query SQL gerada: {generatedSql.Trim()}");
    }

    private static async Task<List<OrderEntity>> RunRelationalPersistenceStageAsync(List<OrderEntity> entities)
    {
        using var connection = new SqliteConnection("Data Source=ShowcasePipeline;Mode=Memory;Cache=Shared");
        connection.Open();

        var dapper = new DapperHelper(connection);
        var unitOfWork = new DapperUnitOfWork(connection);

        await dapper.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS Orders (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                OrderNumber TEXT NOT NULL,
                CustomerName TEXT NOT NULL,
                CustomerEmail TEXT NOT NULL,
                ItemDescription TEXT NOT NULL,
                Quantity INTEGER NOT NULL,
                UnitPrice REAL NOT NULL,
                TotalAmount REAL NOT NULL,
                Status TEXT NOT NULL,
                Region TEXT NOT NULL,
                CreatedAt TEXT NOT NULL
            );");

        await unitOfWork.BeginTransactionAsync();

        foreach (var entity in entities)
        {
            await dapper.ExecuteAsync(@"
                INSERT INTO Orders (OrderNumber, CustomerName, CustomerEmail, ItemDescription, Quantity, UnitPrice, TotalAmount, Status, Region, CreatedAt)
                VALUES (@OrderNumber, @CustomerName, @CustomerEmail, @ItemDescription, @Quantity, @UnitPrice, @TotalAmount, @Status, @Region, @CreatedAt);",
                entity,
                unitOfWork.CurrentTransaction);
        }

        await unitOfWork.CommitAsync();

        var retrieved = (await dapper.QueryAsync<OrderEntity>("SELECT * FROM Orders ORDER BY Id ASC")).ToList();

        Console.WriteLine($"Dapper: {retrieved.Count} pedidos persistidos no SQLite in-memory via Unit of Work.");

        return retrieved;
    }

    private static PagingFiltering.Helpers.Models.PagedResult<OrderEntity> RunPagingFilteringStage(List<OrderEntity> orders)
    {
        var paginationHelper = new PaginationHelper<OrderEntity>();

        var filteredOrders = orders.Where(o => o.TotalAmount >= 1000.00m).ToList();
        var pagedResult = paginationHelper.ApplyPagination(filteredOrders, pageNumber: 1, pageSize: 2);

        Console.WriteLine($"PagingFiltering: paginação aplicada ({pagedResult.Items.Count} itens na página {pagedResult.PageNumber} de {pagedResult.TotalPages}).");

        return pagedResult;
    }

    private static async Task RunHierarchicalCachingStageAsync(List<OrderEntity> allOrders, IEnumerable<OrderEntity> pagedOrders)
    {
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var sqliteCache = new SQLiteCacheService();
        var hierarchicalCache = new HierarchicalCacheService(memoryCache, sqliteCache);

        var cacheKey = CacheKeyGenerator.GenerateKey("paged_orders", "page1");
        var ordersToCache = pagedOrders.ToList();

        await hierarchicalCache.SetAsync(cacheKey, ordersToCache, TimeSpan.FromMinutes(10));
        await hierarchicalCache.GetAsync<List<OrderEntity>>(cacheKey);

        var jsonPayload = System.Text.Json.JsonSerializer.Serialize(allOrders);
        var rawSize = System.Text.Encoding.UTF8.GetByteCount(jsonPayload);
        var compressedPayload = CompressionHelper.Compress(jsonPayload);
        var reduction = (1.0 - ((double)compressedPayload.Length / rawSize)) * 100.0;

        Console.WriteLine($"Caching: {ordersToCache.Count} itens armazenados em cache hierárquico L1/L2 ({reduction:F1}% de redução GZip).");
    }

    private static (OrderEntity Original, OrderEntity Updated, Guid AuditId) RunAuditLoggingStage(List<OrderEntity> orders)
    {
        var settings = new AuditLoggerSettings
        {
            StorageType = "MemoryCache",
            CacheDuration = TimeSpan.FromMinutes(30)
        };

        var storage = AuditLogStorageFactory.Create(settings);
        var auditLogger = new AuditLogger.AuditLogger(storage);

        var originalOrder = orders.First();
        var updatedOrder = new OrderEntity
        {
            Id = originalOrder.Id,
            OrderNumber = originalOrder.OrderNumber,
            CustomerName = originalOrder.CustomerName,
            CustomerEmail = originalOrder.CustomerEmail,
            ItemDescription = originalOrder.ItemDescription,
            Quantity = originalOrder.Quantity,
            UnitPrice = originalOrder.UnitPrice,
            TotalAmount = originalOrder.TotalAmount,
            Status = "Shipped",
            Region = originalOrder.Region,
            CreatedAt = originalOrder.CreatedAt
        };

        var logId = auditLogger.LogUpdate(originalOrder, updatedOrder, "system_fulfillment_agent");

        Console.WriteLine($"AuditLogger: status do pedido {originalOrder.OrderNumber} atualizado para '{updatedOrder.Status}' (Log ID: {logId}).");

        return (originalOrder, updatedOrder, logId);
    }

    private static async Task RunMongoProjectionStageAsync(OrderEntity updatedOrder, Guid auditLogId)
    {
        var mockContext = new Mock<IMongoContext>();
        var mockCollection = new Mock<IMongoCollection<OrderAuditDocument>>();

        mockContext
            .Setup(c => c.GetCollection<OrderAuditDocument>(It.IsAny<string>()))
            .Returns(mockCollection.Object);

        var commandRepo = new MongoCommandRepository<OrderAuditDocument>(mockContext.Object);
        new MongoQueryRepository<OrderAuditDocument>(mockContext.Object);

        var auditDoc = new OrderAuditDocument
        {
            Id = Guid.NewGuid(),
            OrderNumber = updatedOrder.OrderNumber,
            CustomerName = updatedOrder.CustomerName,
            Status = updatedOrder.Status,
            TotalAmount = updatedOrder.TotalAmount,
            AuditLogId = auditLogId,
            ProcessedAt = DateTime.UtcNow
        };

        await commandRepo.AddAsync(auditDoc);

        Console.WriteLine($"MongoDriver: documento de auditoria persistido no repositório (ID: {auditDoc.Id}).");
    }

    private static void CleanupTemporaryArtifacts(string tempCsvPath)
    {
        if (File.Exists(tempCsvPath))
        {
            File.Delete(tempCsvPath);
        }
    }

    private static void PrintPipelineSummary(List<OrderEntity> orders, OrderEntity shippedOrder)
    {
        Console.WriteLine();
        Console.WriteLine("Pedidos processados:");

        foreach (var order in orders)
        {
            var status = order.OrderNumber == shippedOrder.OrderNumber ? shippedOrder.Status : order.Status;
            Console.WriteLine($" - {order.OrderNumber} | {order.CustomerName,-18} | {order.ItemDescription,-20} | {order.TotalAmount,10:C2} | {status}");
        }
    }

    private static List<OrderImportDto> GenerateSampleOrders()
    {
        return new List<OrderImportDto>
        {
            new()
            {
                OrderNumber = "ORD-2026-001",
                CustomerName = "Acme Logistics",
                CustomerEmail = "contact@acme.com",
                ItemDescription = "Enterprise Server",
                Quantity = 3,
                UnitPrice = 1200.00m,
                Region = "South America"
            },
            new()
            {
                OrderNumber = "ORD-2026-002",
                CustomerName = "Global Retail SA",
                CustomerEmail = "purchasing@globalretail.com",
                ItemDescription = "Wireless Barcode",
                Quantity = 10,
                UnitPrice = 98.00m,
                Region = "North America"
            },
            new()
            {
                OrderNumber = "ORD-2026-003",
                CustomerName = "Cloud Systems Inc",
                CustomerEmail = "devops@cloudsystems.io",
                ItemDescription = "Cloud Firewall Box",
                Quantity = 1,
                UnitPrice = 2450.00m,
                Region = "Europe"
            },
            new()
            {
                OrderNumber = "ORD-2026-004",
                CustomerName = "Nexus Robotics",
                CustomerEmail = "procurement@nexusrobotics.ai",
                ItemDescription = "Sensor Array Hub",
                Quantity = 2,
                UnitPrice = 945.00m,
                Region = "Asia Pacific"
            },
            new()
            {
                OrderNumber = "ORD-2026-005",
                CustomerName = "Prime Health Corp",
                CustomerEmail = "supply@primehealth.org",
                ItemDescription = "Diagnostic Tablet",
                Quantity = 1,
                UnitPrice = 750.00m,
                Region = "South America"
            }
        };
    }
}
