using AuditLogger.Interfaces;
using AuditLogger.Models;
using AuditLogger.Services;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

namespace AuditLogger.Tests;

public class AuditLoggerTests
{
    private readonly Mock<IAuditLogStorage> _storageMock;
    private readonly AuditLogger _auditLogger;

    public AuditLoggerTests()
    {
        _storageMock = new Mock<IAuditLogStorage>();
        _auditLogger = new AuditLogger(_storageMock.Object);
    }

    [Fact]
    public void LogCreate_ShouldSaveEntryWithCorrectMetadata_WhenValidInput()
    {
        var item = new SampleEntity { Id = 1, Name = "Notebook" };
        var userId = "user-100";
        AuditLogEntry? capturedEntry = null;

        _storageMock.Setup(s => s.SaveLog(It.IsAny<AuditLogEntry>()))
            .Callback<AuditLogEntry>(e => capturedEntry = e);

        var resultId = _auditLogger.LogCreate(item, userId);

        resultId.Should().NotBeEmpty();
        capturedEntry.Should().NotBeNull();
        capturedEntry!.Operation.Should().Be("CREATE");
        capturedEntry.UserId.Should().Be(userId);
        capturedEntry.Entity.Should().Be(nameof(SampleEntity));
        capturedEntry.Data.Should().Contain("Notebook");
        capturedEntry.Id.Should().Be(resultId);
        _storageMock.Verify(s => s.SaveLog(It.IsAny<AuditLogEntry>()), Times.Once);
    }

    [Fact]
    public void LogRead_ShouldSaveEntryWithReadOperation()
    {
        var item = new SampleEntity { Id = 2, Name = "Mouse" };
        AuditLogEntry? captured = null;
        _storageMock.Setup(s => s.SaveLog(It.IsAny<AuditLogEntry>()))
            .Callback<AuditLogEntry>(e => captured = e);

        var id = _auditLogger.LogRead(item, "analyst-1");

        id.Should().NotBeEmpty();
        captured!.Operation.Should().Be("READ");
        captured.UserId.Should().Be("analyst-1");
    }

    [Fact]
    public void LogUpdate_ShouldIncludeOldAndNewStateInStructuredDiff()
    {
        var oldItem = new SampleEntity { Id = 3, Name = "Old Name" };
        var newItem = new SampleEntity { Id = 3, Name = "New Name" };
        AuditLogEntry? captured = null;
        _storageMock.Setup(s => s.SaveLog(It.IsAny<AuditLogEntry>()))
            .Callback<AuditLogEntry>(e => captured = e);

        var id = _auditLogger.LogUpdate(oldItem, newItem, "admin-1");

        id.Should().NotBeEmpty();
        captured!.Operation.Should().Be("UPDATE");
        captured.Data.Should().Contain("Old");
        captured.Data.Should().Contain("New");
        captured.Data.Should().Contain("Diff");
        captured.Data.Should().Contain("Old Name");
        captured.Data.Should().Contain("New Name");
    }

    [Fact]
    public void LogDelete_ShouldSaveEntryWithDeleteOperation()
    {
        var item = new SampleEntity { Id = 4, Name = "To Delete" };
        AuditLogEntry? captured = null;
        _storageMock.Setup(s => s.SaveLog(It.IsAny<AuditLogEntry>()))
            .Callback<AuditLogEntry>(e => captured = e);

        var id = _auditLogger.LogDelete(item, "admin-2");

        id.Should().NotBeEmpty();
        captured!.Operation.Should().Be("DELETE");
    }

    [Fact]
    public void LogCreate_ShouldThrowArgumentNullException_WhenItemIsNull()
    {
        var act = () => _auditLogger.LogCreate<SampleEntity>(null!, "user-1");

        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void LogCreate_ShouldThrowArgumentException_WhenUserIdIsInvalid(string? invalidUser)
    {
        var item = new SampleEntity { Id = 1, Name = "Item" };

        var act = () => _auditLogger.LogCreate(item, invalidUser!);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void MemoryCacheAuditLogStorage_ShouldStoreAndRetrieveEntry()
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var storage = new MemoryCacheAuditLogStorage(memoryCache, TimeSpan.FromMinutes(5));
        var entry = new AuditLogEntry("CREATE", "u1", "Entity", "{ \"id\": 1 }");

        storage.SaveLog(entry);
        var retrieved = storage.GetLog(entry.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(entry.Id);
        retrieved.Operation.Should().Be("CREATE");
    }

    [Fact]
    public void AuditLogStorageFactory_ShouldInstantiateConfiguredStorage()
    {
        var memorySettings = new AuditLoggerSettings { StorageType = "MemoryCache" };
        var serilogSettings = new AuditLoggerSettings { StorageType = "Serilog" };
        var nlogSettings = new AuditLoggerSettings { StorageType = "NLog" };

        var memStorage = AuditLogStorageFactory.Create(memorySettings);
        var serilogStorage = AuditLogStorageFactory.Create(serilogSettings);
        var nlogStorage = AuditLogStorageFactory.Create(nlogSettings);

        memStorage.Should().BeOfType<MemoryCacheAuditLogStorage>();
        serilogStorage.Should().BeOfType<SerilogAuditLogStorage>();
        nlogStorage.Should().BeOfType<NLogAuditLogStorage>();
    }

    private class SampleEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
