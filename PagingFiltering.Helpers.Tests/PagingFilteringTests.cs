using FluentAssertions;
using Moq;
using PagingFiltering.Helpers.Extensions;
using PagingFiltering.Helpers.Implementations;
using PagingFiltering.Helpers.Interfaces;
using PagingFiltering.Helpers.Models;
using PagingFiltering.Helpers.Specifications;
using Xunit;

namespace PagingFiltering.Helpers.Tests;

public class PagingFilteringTests
{
    private record TestItem(string Id, string Nome, int Idade);

    [Fact]
    public void PaginationHelper_ApplyPagination_ShouldReturnCorrectSliceAndMetadata()
    {
        var helper = new PaginationHelper<TestItem>();
        var items = Enumerable.Range(1, 50).Select(i => new TestItem($"id_{i}", $"Item {i}", i)).ToList();

        var result = helper.ApplyPagination(items, pageNumber: 2, pageSize: 10);

        result.Should().NotBeNull();
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(10);
        result.TotalItems.Should().Be(50);
        result.TotalPages.Should().Be(5);
        result.Items.Should().HaveCount(10);
        result.Items.First().Nome.Should().Be("Item 11");
        result.Items.Last().Nome.Should().Be("Item 20");
    }

    [Fact]
    public void PaginationFilterHelper_ApplySorting_ShouldSortAscendingAndDescending()
    {
        var mockCache = new Mock<ICacheService>();
        var helper = new PaginationFilterHelper<TestItem>(mockCache.Object);
        var items = new List<TestItem>
        {
            new("3", "Carlos", 30),
            new("1", "Ana", 25),
            new("2", "Bruno", 28)
        };

        var sortedAsc = helper.ApplySorting(items, "Nome", ascending: true).ToList();
        var sortedDesc = helper.ApplySorting(items, "Idade", ascending: false).ToList();

        sortedAsc[0].Nome.Should().Be("Ana");
        sortedAsc[1].Nome.Should().Be("Bruno");
        sortedAsc[2].Nome.Should().Be("Carlos");

        sortedDesc[0].Idade.Should().Be(30);
        sortedDesc[1].Idade.Should().Be(28);
        sortedDesc[2].Idade.Should().Be(25);
    }

    [Fact]
    public async Task PaginationFilterHelper_ApplyCachingAsync_ShouldUseCacheWhenAvailable()
    {
        var mockCache = new Mock<ICacheService>();
        var cachedItems = (IEnumerable<TestItem>)new List<TestItem> { new("1", "Cache", 99) };
        mockCache.Setup(c => c.GetAsync<IEnumerable<TestItem>>("my_key"))
            .ReturnsAsync(cachedItems);

        var helper = new PaginationFilterHelper<TestItem>(mockCache.Object);
        var dataFetchCalled = false;

        var result = await helper.ApplyCachingAsync("my_key", () =>
        {
            dataFetchCalled = true;
            return Task.FromResult((IEnumerable<TestItem>)new List<TestItem>());
        });

        result.Should().BeSameAs(cachedItems);
        dataFetchCalled.Should().BeFalse();
    }

    [Fact]
    public void PaginationFilterHelper_ApplyComplexFilters_ShouldCombineAllPredicates()
    {
        var mockCache = new Mock<ICacheService>();
        var helper = new PaginationFilterHelper<TestItem>(mockCache.Object);
        var items = new List<TestItem>
        {
            new("1", "Ana", 20),
            new("2", "Ana Maria", 30),
            new("3", "Bruno", 20),
            new("4", "Carlos", 40)
        };

        var filtered = helper.ApplyComplexFilters(items, new()
        {
            x => x.Nome.StartsWith("Ana"),
            x => x.Idade > 25
        });

        filtered.Should().HaveCount(1);
        filtered.First().Nome.Should().Be("Ana Maria");
    }

    [Fact]
    public void PagedResultCursor_ShouldCalculateCurrentPageAndHoldNextCursor()
    {
        var items = new List<TestItem> { new("1", "A", 10), new("2", "B", 20) };
        var cursor = new PagedResultCursor<TestItem>(items, pageSize: 2, totalItems: 10, nextCursor: "cursor_xyz");

        cursor.Items.Should().HaveCount(2);
        cursor.PageSize.Should().Be(2);
        cursor.TotalItems.Should().Be(10);
        cursor.NextCursor.Should().Be("cursor_xyz");
        cursor.CurrentPage.Should().Be(5);
    }

    [Fact]
    public void ApplyKeyset_ShouldPaginateSeekStyle_Correctly()
    {
        var dataset = Enumerable.Range(1, 100)
            .Select(i => new TestItem($"id_{i}", $"Nome_{i:D3}", i))
            .ToList();

        var firstPage = dataset.ApplyKeyset(x => x.Idade, afterKey: 0, pageSize: 10);

        var secondPage = dataset.ApplyKeyset(x => x.Idade, afterKey: 10, pageSize: 10);

        firstPage.Items.Should().HaveCount(10);
        firstPage.HasMore.Should().BeTrue();
        firstPage.PreviousKey.Should().Be(1);
        firstPage.NextKey.Should().Be(10);

        secondPage.Items.Should().HaveCount(10);
        secondPage.Items.First().Idade.Should().Be(11);
        secondPage.Items.Last().Idade.Should().Be(20);
        secondPage.NextKey.Should().Be(20);
    }

    [Fact]
    public void DynamicFilterParser_ShouldParseCriteriaAndFilterEntities()
    {
        var items = new List<TestItem>
        {
            new("1", "Carlos Silva", 35),
            new("2", "Ana Souza", 25),
            new("3", "Carlos Eduardo", 20)
        };

        var nameCriterion = new FilterCriterion("Nome", FilterOperator.StartsWith, "Carlos");
        var ageCriterion = new FilterCriterion("Idade", FilterOperator.GreaterThan, 22);

        var nameSpec = DynamicFilterParser.Parse<TestItem>(nameCriterion);
        var ageSpec = DynamicFilterParser.Parse<TestItem>(ageCriterion);
        var combinedSpec = nameSpec.And(ageSpec);

        var filtered = items.Where(combinedSpec.ToExpression().Compile()).ToList();

        filtered.Should().HaveCount(1);
        filtered[0].Nome.Should().Be("Carlos Silva");
        filtered[0].Idade.Should().Be(35);
    }
}
