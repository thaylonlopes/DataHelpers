using System.Diagnostics;
using DataMapping.Helpers.Interfaces;
using FluentAssertions;
using Xunit;

namespace DataMapping.Helpers.Tests;

public class SimpleMapperTests
{
    private readonly SimpleMapper _mapper;

    public SimpleMapperTests()
    {
        _mapper = new SimpleMapper();
    }

    [Fact]
    public void Map_ShouldMapPropertiesCorrectly_WhenValidSourceProvided()
    {
        var source = new SourceModel { Id = 42, Name = "DataHelpers", Active = true, Score = 99.5 };

        var result = _mapper.Map<SourceModel, DestinationModel>(source);

        result.Should().NotBeNull();
        result.Id.Should().Be(source.Id);
        result.Name.Should().Be(source.Name);
        result.Active.Should().Be(source.Active);
        result.Score.Should().Be(source.Score);
    }

    [Fact]
    public void Map_ShouldThrowArgumentNullException_WhenSourceIsNull()
    {
        SourceModel? source = null;

        var act = () => _mapper.Map<SourceModel, DestinationModel>(source!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("source");
    }

    [Fact]
    public void Map_ShouldReuseCompiledDelegateFromCache_OnSubsequentCalls()
    {
        var firstSource = new SourceModel { Id = 1, Name = "First" };
        var secondSource = new SourceModel { Id = 2, Name = "Second" };

        var firstResult = _mapper.Map<SourceModel, DestinationModel>(firstSource);

        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < 10_000; i++)
        {
            var r = _mapper.Map<SourceModel, DestinationModel>(secondSource);
            r.Id.Should().Be(2);
        }
        stopwatch.Stop();

        firstResult.Id.Should().Be(1);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(500);
    }

    [Fact]
    public void Map_ShouldHandleConcurrentAccessSafely()
    {
        var parallelCount = 100;
        var sources = Enumerable.Range(1, parallelCount)
            .Select(i => new SourceModel { Id = i, Name = $"Item_{i}", Active = i % 2 == 0 })
            .ToList();

        var results = new DestinationModel[parallelCount];
        Parallel.For(0, parallelCount, i =>
        {
            results[i] = _mapper.Map<SourceModel, DestinationModel>(sources[i]);
        });

        for (int i = 0; i < parallelCount; i++)
        {
            results[i].Id.Should().Be(sources[i].Id);
            results[i].Name.Should().Be(sources[i].Name);
            results[i].Active.Should().Be(sources[i].Active);
        }
    }

    [Fact]
    public void Map_ShouldMapNestedObjectsAndCollections_Correctly()
    {
        var order = new OrderSource
        {
            Id = 101,
            Customer = new CustomerSource { Name = "Alice", Email = "alice@example.com" },
            Items = new List<OrderItemSource>
            {
                new() { Product = "Notebook", Price = 5000 },
                new() { Product = "Mouse", Price = 150 }
            }
        };

        var result = _mapper.Map<OrderSource, OrderDestination>(order);

        result.Should().NotBeNull();
        result.Id.Should().Be(101);
        result.Customer.Should().NotBeNull();
        result.Customer!.Name.Should().Be("Alice");
        result.Customer.Email.Should().Be("alice@example.com");
        result.Items.Should().HaveCount(2);
        result.Items![0].Product.Should().Be("Notebook");
        result.Items[0].Price.Should().Be(5000);
    }

    [Fact]
    public void RegisterConverter_ShouldApplyCustomTypeConversion()
    {
        SimpleMapper.RegisterConverter<DateTime, string>(dt => dt.ToString("yyyy-MM-dd"));

        var eventSource = new EventSource { Id = 1, EventDate = new DateTime(2026, 8, 30) };

        var result = _mapper.Map<EventSource, EventDestination>(eventSource);

        result.Id.Should().Be(1);
        result.EventDate.Should().Be("2026-08-30");
    }

    [Fact]
    public void MapCollection_ShouldMapAllElementsInList()
    {
        var list = new List<SourceModel>
        {
            new() { Id = 1, Name = "Item1" },
            new() { Id = 2, Name = "Item2" }
        };

        var result = _mapper.MapCollection<SourceModel, DestinationModel>(list).ToList();

        result.Should().HaveCount(2);
        result[0].Id.Should().Be(1);
        result[1].Id.Should().Be(2);
    }

    [Fact]
    public void TryMap_ShouldReturnSuccess_WhenMappingIsValid()
    {
        var source = new SourceModel { Id = 99, Name = "SafeMap" };

        var result = _mapper.TryMap<SourceModel, DestinationModel>(source);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(99);
    }

    [Fact]
    public void TryMap_ShouldReturnFailure_WhenSourceIsNull()
    {
        var result = _mapper.TryMap<SourceModel, DestinationModel>(null);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNullOrEmpty();
    }

    private class SourceModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool Active { get; set; }
        public double Score { get; set; }
    }

    private class DestinationModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool Active { get; set; }
        public double Score { get; set; }
    }

    private class CustomerSource
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    private class CustomerDestination
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    private class OrderItemSource
    {
        public string Product { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    private class OrderItemDestination
    {
        public string Product { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    private class OrderSource
    {
        public int Id { get; set; }
        public CustomerSource? Customer { get; set; }
        public List<OrderItemSource>? Items { get; set; }
    }

    private class OrderDestination
    {
        public int Id { get; set; }
        public CustomerDestination? Customer { get; set; }
        public List<OrderItemDestination>? Items { get; set; }
    }

    private class EventSource
    {
        public int Id { get; set; }
        public DateTime EventDate { get; set; }
    }

    private class EventDestination
    {
        public int Id { get; set; }
        public string EventDate { get; set; } = string.Empty;
    }
}
