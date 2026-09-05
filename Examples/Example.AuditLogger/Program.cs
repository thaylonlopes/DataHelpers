using AuditLogger;
using AuditLogger.Models;

Console.WriteLine("=== Demo: TL.AuditLogger ===");

var settings = new AuditLoggerSettings { StorageType = "MemoryCache", CacheDuration = TimeSpan.FromMinutes(30) };
var storage = AuditLogStorageFactory.Create(settings);
var logger = new AuditLogger.AuditLogger(storage);

var logId = logger.LogCreate(new { Id = 1, Name = "Alice", Role = "Admin" }, "usuario_admin");
Console.WriteLine($"Audit log entry created with ID: {logId}");

var updateId = logger.LogUpdate(
    new { Id = 1, Name = "Alice", Role = "Admin" },
    new { Id = 1, Name = "Alice Smith", Role = "SuperAdmin" },
    "usuario_admin");
Console.WriteLine($"Audit log update diff created with ID: {updateId}");
