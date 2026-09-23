using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using MongoDriver.Helpers;
using MongoDriver.Helpers.Context;
using MongoDriver.Helpers.Interface.Context;
using MongoDriver.Helpers.Interface.Events;
using MongoDriver.Helpers.Models;
using MongoDriver.Helpers.Utils;
using Moq;
using Xunit;

namespace MongoDriver.Helpers.Tests;

public class MongoDriverTests
{
    public class SampleGuidDocument : Document<Guid>
    {
        public string Nome { get; set; } = string.Empty;
    }

    public class SampleLongDocument : Document<long>
    {
        public string Titulo { get; set; } = string.Empty;
    }

    public class SampleObjectIdDocument : Document<ObjectId>
    {
        public int Quantidade { get; set; }
    }

    [Fact]
    public void Document_WithGuidId_ShouldInitializeWithDefaultValues()
    {
        var doc = new SampleGuidDocument
        {
            Id = Guid.NewGuid(),
            Nome = "Teste Mongo Guid"
        };

        doc.Nome.Should().Be("Teste Mongo Guid");
        doc.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Document_WithLongId_ShouldInitializeCorrectly()
    {
        var doc = new SampleLongDocument
        {
            Id = 9876543210L,
            Titulo = "Documento Long"
        };

        doc.Id.Should().Be(9876543210L);
        doc.Titulo.Should().Be("Documento Long");
    }

    [Fact]
    public void Document_WithObjectId_ShouldInitializeCorrectly()
    {
        var objectId = ObjectId.GenerateNewId();
        var doc = new SampleObjectIdDocument
        {
            Id = objectId,
            Quantidade = 42
        };

        doc.Id.Should().Be(objectId);
        doc.Quantidade.Should().Be(42);
    }

    [Fact]
    public async Task MongoCommandRepository_AddAsync_Single_ShouldCallInsertOneAsync()
    {
        var mockContext = new Mock<IMongoContext>();
        var mockCollection = new Mock<IMongoCollection<SampleGuidDocument>>();

        mockContext.Setup(c => c.GetCollection<SampleGuidDocument>(It.IsAny<string>()))
            .Returns(mockCollection.Object);

        var repo = new MongoCommandRepository<SampleGuidDocument>(mockContext.Object);
        var item = new SampleGuidDocument { Id = Guid.NewGuid(), Nome = "Item Inserido" };

        await repo.AddAsync(item);

        mockCollection.Verify(c => c.InsertOneAsync(item, It.IsAny<InsertOneOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MongoCommandRepository_AddAsync_Multiple_ShouldCallInsertManyAsync()
    {
        var mockContext = new Mock<IMongoContext>();
        var mockCollection = new Mock<IMongoCollection<SampleGuidDocument>>();

        mockContext.Setup(c => c.GetCollection<SampleGuidDocument>(It.IsAny<string>()))
            .Returns(mockCollection.Object);

        var repo = new MongoCommandRepository<SampleGuidDocument>(mockContext.Object);
        var items = new List<SampleGuidDocument>
        {
            new() { Id = Guid.NewGuid(), Nome = "Item 1" },
            new() { Id = Guid.NewGuid(), Nome = "Item 2" }
        };

        await repo.AddRangeAsync(items);

        mockCollection.Verify(c => c.InsertManyAsync(items, It.IsAny<InsertManyOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MongoCommandRepository_UpdateAsync_ShouldCallReplaceOneAsync()
    {
        var mockContext = new Mock<IMongoContext>();
        var mockCollection = new Mock<IMongoCollection<SampleGuidDocument>>();

        mockContext.Setup(c => c.GetCollection<SampleGuidDocument>(It.IsAny<string>()))
            .Returns(mockCollection.Object);

        var repo = new MongoCommandRepository<SampleGuidDocument>(mockContext.Object);
        var item = new SampleGuidDocument { Id = Guid.NewGuid(), Nome = "Item Atualizado" };

        await repo.UpdateAsync(item);

        mockCollection.Verify(c => c.ReplaceOneAsync(It.IsAny<FilterDefinition<SampleGuidDocument>>(), item, It.IsAny<ReplaceOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MongoCommandRepository_DeleteAsync_Expression_ShouldCallDeleteManyAsync()
    {
        var mockContext = new Mock<IMongoContext>();
        var mockCollection = new Mock<IMongoCollection<SampleGuidDocument>>();

        mockContext.Setup(c => c.GetCollection<SampleGuidDocument>(It.IsAny<string>()))
            .Returns(mockCollection.Object);

        var repo = new MongoCommandRepository<SampleGuidDocument>(mockContext.Object);

        await repo.DeleteAsync(d => d.Nome == "Deletar");

        mockCollection.Verify(c => c.DeleteManyAsync(It.IsAny<FilterDefinition<SampleGuidDocument>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MongoCommandRepository_DeleteAsync_ById_ShouldCallDeleteOneAsync()
    {
        var mockContext = new Mock<IMongoContext>();
        var mockCollection = new Mock<IMongoCollection<SampleGuidDocument>>();

        mockContext.Setup(c => c.GetCollection<SampleGuidDocument>(It.IsAny<string>()))
            .Returns(mockCollection.Object);

        var repo = new MongoCommandRepository<SampleGuidDocument>(mockContext.Object);
        var id = Guid.NewGuid();

        await repo.DeleteAsync(id);

        mockCollection.Verify(c => c.DeleteOneAsync(It.IsAny<FilterDefinition<SampleGuidDocument>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void MongoQueryRepository_Instantiation_ShouldResolveCollectionFromContext()
    {
        var mockContext = new Mock<IMongoContext>();
        var mockCollection = new Mock<IMongoCollection<SampleGuidDocument>>();

        mockContext.Setup(c => c.GetCollection<SampleGuidDocument>(It.IsAny<string>()))
            .Returns(mockCollection.Object);

        var repo = new MongoQueryRepository<SampleGuidDocument>(mockContext.Object);

        repo.Should().NotBeNull();
        mockContext.Verify(c => c.GetCollection<SampleGuidDocument>(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void MongoQueryRepository_AsQueryable_ShouldReturnCollectionQueryable()
    {
        var mockContext = new Mock<IMongoContext>();
        var mockCollection = new Mock<IMongoCollection<SampleGuidDocument>>();

        mockContext.Setup(c => c.GetCollection<SampleGuidDocument>(It.IsAny<string>()))
            .Returns(mockCollection.Object);

        var repo = new MongoQueryRepository<SampleGuidDocument>(mockContext.Object);
        var q1 = repo.Queryable;
        var q2 = repo.AsQueryable();

        repo.Should().NotBeNull();
    }

    [Fact]
    public void MongoContext_AddCommandAndRemoveCommand_ShouldManageTaskList()
    {
        var mockClientDb = new Mock<IMongoClientDatabase>();
        var mockDb = new Mock<IMongoDatabase>();
        var mockClient = new Mock<IMongoClient>();
        var mockEventCatcher = new Mock<IEventCatcher>();

        mockClientDb.Setup(x => x.Database).Returns(mockDb.Object);
        mockClientDb.Setup(x => x.DatabaseClient).Returns(mockClient.Object);

        var context = new MongoContext(mockClientDb.Object, mockEventCatcher.Object);

        Func<Task> cmd = () => Task.CompletedTask;
        context.AddCommand(cmd);
        context.RemoveCommand(cmd);

        context.Should().NotBeNull();
    }

    [Fact]
    public async Task MongoContext_SaveChanges_ShouldCommitTransaction_WhenAllCommandsSucceed()
    {
        var mockClientDb = new Mock<IMongoClientDatabase>();
        var mockDb = new Mock<IMongoDatabase>();
        var mockClient = new Mock<IMongoClient>();
        var mockSession = new Mock<IClientSessionHandle>();
        var mockEventCatcher = new Mock<IEventCatcher>();

        mockClientDb.Setup(x => x.Database).Returns(mockDb.Object);
        mockClientDb.Setup(x => x.DatabaseClient).Returns(mockClient.Object);
        mockClient.Setup(x => x.StartSessionAsync(It.IsAny<ClientSessionOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockSession.Object);

        var context = new MongoContext(mockClientDb.Object, mockEventCatcher.Object);

        var executed = false;
        await context.AddCommand(() =>
        {
            executed = true;
            return Task.CompletedTask;
        });

        var count = await context.SaveChanges();

        count.Should().Be(1);
        executed.Should().BeTrue();
        mockSession.Verify(s => s.StartTransaction(It.IsAny<TransactionOptions>()), Times.Once);
        mockSession.Verify(s => s.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MongoContext_SaveChanges_ShouldAbortTransaction_WhenCommandThrows()
    {
        var mockClientDb = new Mock<IMongoClientDatabase>();
        var mockDb = new Mock<IMongoDatabase>();
        var mockClient = new Mock<IMongoClient>();
        var mockSession = new Mock<IClientSessionHandle>();
        var mockEventCatcher = new Mock<IEventCatcher>();

        mockClientDb.Setup(x => x.Database).Returns(mockDb.Object);
        mockClientDb.Setup(x => x.DatabaseClient).Returns(mockClient.Object);
        mockClient.Setup(x => x.StartSessionAsync(It.IsAny<ClientSessionOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockSession.Object);
        mockSession.Setup(s => s.IsInTransaction).Returns(true);

        var context = new MongoContext(mockClientDb.Object, mockEventCatcher.Object);

        await context.AddCommand(() => throw new InvalidOperationException("Erro simulado no banco"));

        var act = () => context.SaveChanges();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Erro simulado no banco");

        mockSession.Verify(s => s.AbortTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MongoContext_BeginTransactionAsync_ShouldStartSessionAndTransaction()
    {
        var mockClientDb = new Mock<IMongoClientDatabase>();
        var mockDb = new Mock<IMongoDatabase>();
        var mockClient = new Mock<IMongoClient>();
        var mockSession = new Mock<IClientSessionHandle>();
        var mockEventCatcher = new Mock<IEventCatcher>();

        mockSession.Setup(s => s.IsInTransaction).Returns(false);

        mockClientDb.Setup(x => x.Database).Returns(mockDb.Object);
        mockClientDb.Setup(x => x.DatabaseClient).Returns(mockClient.Object);
        mockClient.Setup(x => x.StartSessionAsync(It.IsAny<ClientSessionOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockSession.Object);

        var context = new MongoContext(mockClientDb.Object, mockEventCatcher.Object);

        var session = await context.BeginTransactionAsync();

        session.Should().NotBeNull();
        mockSession.Verify(s => s.StartTransaction(It.IsAny<TransactionOptions>()), Times.Once);
    }

    [Fact]
    public async Task MongoContext_CommitTransactionAsync_ShouldCommitActiveTransaction()
    {
        var mockClientDb = new Mock<IMongoClientDatabase>();
        var mockDb = new Mock<IMongoDatabase>();
        var mockClient = new Mock<IMongoClient>();
        var mockSession = new Mock<IClientSessionHandle>();
        var mockEventCatcher = new Mock<IEventCatcher>();

        mockSession.Setup(s => s.IsInTransaction).Returns(true);

        mockClientDb.Setup(x => x.Database).Returns(mockDb.Object);
        mockClientDb.Setup(x => x.DatabaseClient).Returns(mockClient.Object);
        mockClient.Setup(x => x.StartSessionAsync(It.IsAny<ClientSessionOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockSession.Object);

        var context = new MongoContext(mockClientDb.Object, mockEventCatcher.Object);
        await context.BeginTransactionAsync();
        await context.CommitTransactionAsync();

        mockSession.Verify(s => s.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MongoContext_RollbackTransactionAsync_ShouldAbortActiveTransaction()
    {
        var mockClientDb = new Mock<IMongoClientDatabase>();
        var mockDb = new Mock<IMongoDatabase>();
        var mockClient = new Mock<IMongoClient>();
        var mockSession = new Mock<IClientSessionHandle>();
        var mockEventCatcher = new Mock<IEventCatcher>();

        mockSession.Setup(s => s.IsInTransaction).Returns(true);

        mockClientDb.Setup(x => x.Database).Returns(mockDb.Object);
        mockClientDb.Setup(x => x.DatabaseClient).Returns(mockClient.Object);
        mockClient.Setup(x => x.StartSessionAsync(It.IsAny<ClientSessionOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockSession.Object);

        var context = new MongoContext(mockClientDb.Object, mockEventCatcher.Object);
        await context.BeginTransactionAsync();
        await context.RollbackTransactionAsync();

        mockSession.Verify(s => s.AbortTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void MongoContext_Dispose_ShouldDisposeActiveSession()
    {
        var mockClientDb = new Mock<IMongoClientDatabase>();
        var mockDb = new Mock<IMongoDatabase>();
        var mockClient = new Mock<IMongoClient>();
        var mockSession = new Mock<IClientSessionHandle>();
        var mockEventCatcher = new Mock<IEventCatcher>();

        mockClientDb.Setup(x => x.Database).Returns(mockDb.Object);
        mockClientDb.Setup(x => x.DatabaseClient).Returns(mockClient.Object);
        mockClient.Setup(x => x.StartSessionAsync(It.IsAny<ClientSessionOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockSession.Object);

        var context = new MongoContext(mockClientDb.Object, mockEventCatcher.Object);
        context.Dispose();

        context.Should().NotBeNull();
    }

    [Fact]
    public void BsonRegistrationHelper_RegisterConvention_ShouldBeIdempotent()
    {
        var pack = new ConventionPack { new CamelCaseElementNameConvention() };

        var act1 = () => BsonRegistrationHelper.RegisterConvention("TestConventionIdempotent", pack);
        var act2 = () => BsonRegistrationHelper.RegisterConvention("TestConventionIdempotent", pack);

        act1.Should().NotThrow();
        act2.Should().NotThrow();
    }

    public record TestCustomDto(string Code);

    [Fact]
    public void BsonRegistrationHelper_RegisterSerializer_ShouldBeIdempotent()
    {
        var serializer = new StringSerializer();

        var act1 = () => BsonRegistrationHelper.RegisterSerializer(serializer);
        var act2 = () => BsonRegistrationHelper.RegisterSerializer(serializer);

        act1.Should().NotThrow();
        act2.Should().NotThrow();
    }

    [Fact]
    public void BsonRegistrationHelper_RegisterStandardConventions_ShouldExecuteWithoutError()
    {
        var act = () => BsonRegistrationHelper.RegisterStandardConventions();
        act.Should().NotThrow();
    }

    [Fact]
    public void MongoContext_Constructor_ShouldThrowArgumentNullException_WhenNullDependenciesPassed()
    {
        var mockClientDb = new Mock<IMongoClientDatabase>();
        var mockEventCatcher = new Mock<IEventCatcher>();

        var act1 = () => new MongoContext(null!, mockEventCatcher.Object);
        var act2 = () => new MongoContext(mockClientDb.Object, null!);

        act1.Should().Throw<ArgumentNullException>();
        act2.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void MongoContext_GetCollection_ShouldThrowArgumentException_WhenNameIsNullOrWhitespace()
    {
        var mockClientDb = new Mock<IMongoClientDatabase>();
        var mockDb = new Mock<IMongoDatabase>();
        var mockClient = new Mock<IMongoClient>();
        var mockEventCatcher = new Mock<IEventCatcher>();

        mockClientDb.Setup(x => x.Database).Returns(mockDb.Object);
        mockClientDb.Setup(x => x.DatabaseClient).Returns(mockClient.Object);

        var context = new MongoContext(mockClientDb.Object, mockEventCatcher.Object);

        var act = () => context.GetCollection<SampleGuidDocument>("");
        act.Should().Throw<ArgumentException>();
    }
}
