using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDriver.Helpers;
using MongoDriver.Helpers.Interface.Context;
using MongoDriver.Helpers.Models;
using Moq;
using Xunit;

namespace MongoDriver.Helpers.Tests;

public class MongoDriverTests
{
    public class SampleDocument : Document<Guid>
    {
        public string Nome { get; set; } = string.Empty;
    }

    [Fact]
    public void Document_ShouldInitializeWithDefaultValues()
    {
        var doc = new SampleDocument
        {
            Id = Guid.NewGuid(),
            Nome = "Teste Mongo"
        };

        doc.Nome.Should().Be("Teste Mongo");
        doc.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task MongoCommandRepository_AddAsync_ShouldCallInsertOneAsyncOnCollection()
    {
        var mockContext = new Mock<IMongoContext>();
        var mockCollection = new Mock<IMongoCollection<SampleDocument>>();

        mockContext.Setup(c => c.GetCollection<SampleDocument>(It.IsAny<string>()))
            .Returns(mockCollection.Object);

        var repo = new MongoCommandRepository<SampleDocument>(mockContext.Object);
        var item = new SampleDocument { Id = Guid.NewGuid(), Nome = "Item Inserido" };

        await repo.AddAsync(item);

        mockCollection.Verify(c => c.InsertOneAsync(item, It.IsAny<InsertOneOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MongoCommandRepository_DeleteAsync_ShouldCallDeleteManyAsyncOnCollection()
    {
        var mockContext = new Mock<IMongoContext>();
        var mockCollection = new Mock<IMongoCollection<SampleDocument>>();

        mockContext.Setup(c => c.GetCollection<SampleDocument>(It.IsAny<string>()))
            .Returns(mockCollection.Object);

        var repo = new MongoCommandRepository<SampleDocument>(mockContext.Object);

        await repo.DeleteAsync(d => d.Nome == "Deletar");

        mockCollection.Verify(c => c.DeleteManyAsync(It.IsAny<FilterDefinition<SampleDocument>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void MongoQueryRepository_Instantiation_ShouldResolveCollectionFromContext()
    {
        var mockContext = new Mock<IMongoContext>();
        var mockCollection = new Mock<IMongoCollection<SampleDocument>>();

        mockContext.Setup(c => c.GetCollection<SampleDocument>(It.IsAny<string>()))
            .Returns(mockCollection.Object);

        var repo = new MongoQueryRepository<SampleDocument>(mockContext.Object);

        repo.Should().NotBeNull();
        mockContext.Verify(c => c.GetCollection<SampleDocument>(It.IsAny<string>()), Times.Once);
    }
}

