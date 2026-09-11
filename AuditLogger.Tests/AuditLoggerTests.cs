using AuditLogger.Interfaces;
using AuditLogger.Models;
using AuditLogger.Services;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
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
    public void LogUpdate_ShouldMaskSensitiveDataAnnotatedProperty_AndNeverSerializePlainValue()
    {
        var oldUser = new UserEntity { Id = 1, Nome = "Admin", SenhaHash = "secret-hash-123" };
        var newUser = new UserEntity { Id = 1, Nome = "Admin", SenhaHash = "new-hash-456" };
        AuditLogEntry? captured = null;

        _storageMock.Setup(s => s.SaveLog(It.IsAny<AuditLogEntry>()))
            .Callback<AuditLogEntry>(e => captured = e);

        var updateId = _auditLogger.LogUpdate(oldUser, newUser, "admin");

        updateId.Should().NotBeEmpty();
        captured.Should().NotBeNull();
        captured!.Data.Should().NotContain("secret-hash-123");
        captured.Data.Should().NotContain("new-hash-456");
        captured.Data.Should().Contain("\"SenhaHash\":\"***\"");
    }

    [Fact]
    public void LogUpdate_ShouldMaskPropertiesByHeuristicConvention_WhenEnableHeuristicMaskingIsTrue()
    {
        var oldUser = new UserEntity
        {
            Id = 1,
            Password = "OldPassword99",
            Token = "eyJhbGciOiJIUzI1NiJ9",
            Secret = "top-secret-api-key",
            Cpf = "123.456.789-00",
            CreditCard = "4111-2222-3333-4444"
        };
        var newUser = new UserEntity
        {
            Id = 1,
            Password = "NewPassword100",
            Token = "eyJhbGciOiJIUzI1NiJ9_mutated",
            Secret = "new-top-secret-key",
            Cpf = "123.456.789-00",
            CreditCard = "5111-2222-3333-5555"
        };
        AuditLogEntry? captured = null;

        _storageMock.Setup(s => s.SaveLog(It.IsAny<AuditLogEntry>()))
            .Callback<AuditLogEntry>(e => captured = e);

        _auditLogger.LogUpdate(oldUser, newUser, "security-operator");

        captured.Should().NotBeNull();
        captured!.Data.Should().NotContain("OldPassword99");
        captured.Data.Should().NotContain("NewPassword100");
        captured.Data.Should().NotContain("eyJhbGciOiJIUzI1NiJ9");
        captured.Data.Should().NotContain("top-secret-api-key");
        captured.Data.Should().NotContain("123.456.789-00");
        captured.Data.Should().NotContain("4111-2222-3333-4444");
        captured.Data.Should().NotContain("5111-2222-3333-5555");
    }

    [Fact]
    public void LogUpdate_ShouldPreserveConventionalPropertiesWithoutCorruption()
    {
        var oldUser = new UserEntity
        {
            Id = 1,
            Nome = "Alice Smith",
            Email = "alice@example.com",
            DataCadastro = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc)
        };
        var newUser = new UserEntity
        {
            Id = 1,
            Nome = "Alice Johnson",
            Email = "alice.johnson@example.com",
            DataCadastro = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc)
        };
        AuditLogEntry? captured = null;

        _storageMock.Setup(s => s.SaveLog(It.IsAny<AuditLogEntry>()))
            .Callback<AuditLogEntry>(e => captured = e);

        _auditLogger.LogUpdate(oldUser, newUser, "hr-admin");

        captured.Should().NotBeNull();
        captured!.Data.Should().Contain("Alice Smith");
        captured.Data.Should().Contain("Alice Johnson");
        captured.Data.Should().Contain("alice@example.com");
        captured.Data.Should().Contain("alice.johnson@example.com");
    }

    [Fact]
    public void LogCreate_ShouldSupportCustomMaskPatternFromAttribute()
    {
        var item = new CustomMaskEntity { Id = 10, PinCode = "1234" };
        AuditLogEntry? captured = null;

        _storageMock.Setup(s => s.SaveLog(It.IsAny<AuditLogEntry>()))
            .Callback<AuditLogEntry>(e => captured = e);

        _auditLogger.LogCreate(item, "admin");

        captured.Should().NotBeNull();
        captured!.Data.Should().NotContain("1234");
        captured.Data.Should().Contain("[PROTECTED]");
    }

    [Fact]
    public void AuditLogger_ShouldEmitStructuredLog_ThroughILogger()
    {
        var loggerMock = new Mock<ILogger<AuditLogger>>();
        var auditLogger = new AuditLogger(_storageMock.Object, loggerMock.Object);
        var item = new SampleEntity { Id = 5, Name = "Mouse" };

        auditLogger.LogCreate(item, "operator-1");

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void AuditLogger_ShouldCaptureDistributedContext_CorrelationIdAndTenantId()
    {
        using (AuditCorrelationContext.SetContext("corr-test-123", "tenant-alpha"))
        {
            var item = new SampleEntity { Id = 8, Name = "Keyboard" };
            AuditLogEntry? captured = null;

            _storageMock.Setup(s => s.SaveLog(It.IsAny<AuditLogEntry>()))
                .Callback<AuditLogEntry>(e => captured = e);

            _auditLogger.LogCreate(item, "operator-2");

            captured.Should().NotBeNull();
            captured!.CorrelationId.Should().Be("corr-test-123");
            captured.TenantId.Should().Be("tenant-alpha");
        }
    }

    [Fact]
    public void AuditLogger_ShouldSanitizeCrlfCharacters_ToPreventLogInjection()
    {
        var maliciousUser = "admin\r\n[2026-09-09] FAKE LOG: Injected entry";
        var item = new SampleEntity { Id = 9, Name = "Item\r\nName" };
        AuditLogEntry? captured = null;

        _storageMock.Setup(s => s.SaveLog(It.IsAny<AuditLogEntry>()))
            .Callback<AuditLogEntry>(e => captured = e);

        _auditLogger.LogCreate(item, maliciousUser);

        captured.Should().NotBeNull();
        captured!.UserId.Should().NotContain("\r");
        captured.UserId.Should().NotContain("\n");
        captured.UserId.Should().Contain(@"\r\n");
        captured.Data.Should().NotContain("\r");
        captured.Data.Should().NotContain("\n");
    }

    [Fact]
    public void AuditLogEntry_ShouldSanitizeRawData_WhenConstructedDirectlyWithCrlf()
    {
        var rawDataWithCrlf = "{\"id\": 1,\r\n\"injected\": \"fake_log_line\"\n}";
        var entry = new AuditLogEntry("CREATE", "user-1", "Entity", rawDataWithCrlf);

        entry.Data.Should().NotContain("\r");
        entry.Data.Should().NotContain("\n");
        entry.Data.Should().Contain(@"\r\n");
    }

    [Fact]
    public async Task LogUpdateAsync_ShouldExecuteAsynchronouslyAndSaveLog()
    {
        var oldItem = new SampleEntity { Id = 1, Name = "Old" };
        var newItem = new SampleEntity { Id = 1, Name = "New" };

        var resultId = await _auditLogger.LogUpdateAsync(oldItem, newItem, "user-async");

        resultId.Should().NotBeEmpty();
        _storageMock.Verify(s => s.SaveLogAsync(It.IsAny<AuditLogEntry>(), It.IsAny<CancellationToken>()), Times.Once);
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
        var loggerSettings = new AuditLoggerSettings { StorageType = "Logger" };

        var memStorage = AuditLogStorageFactory.Create(memorySettings);
        var serilogStorage = AuditLogStorageFactory.Create(serilogSettings);
        var nlogStorage = AuditLogStorageFactory.Create(nlogSettings);
        var genericLoggerStorage = AuditLogStorageFactory.Create(loggerSettings);

        memStorage.Should().BeOfType<MemoryCacheAuditLogStorage>();
        serilogStorage.Should().BeAssignableTo<IAuditLogStorage>();
        nlogStorage.Should().BeAssignableTo<IAuditLogStorage>();
        genericLoggerStorage.Should().BeOfType<LoggerAuditLogStorage>();
    }

    private class SampleEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private class UserEntity
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime DataCadastro { get; set; }

        [SensitiveData]
        public string SenhaHash { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Secret { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        public string CreditCard { get; set; } = string.Empty;
    }

    private class CustomMaskEntity
    {
        public int Id { get; set; }

        [SensitiveData("[PROTECTED]")]
        public string PinCode { get; set; } = string.Empty;
    }
}
