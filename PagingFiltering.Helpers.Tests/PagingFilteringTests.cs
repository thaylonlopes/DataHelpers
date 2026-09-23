using FluentAssertions;
using PagingFiltering.Helpers.Attributes;
using PagingFiltering.Helpers.Builders;
using PagingFiltering.Helpers.Extensions;
using PagingFiltering.Helpers.Implementations;
using PagingFiltering.Helpers.Models;
using PagingFiltering.Helpers.Specifications;
using System.Security;
using Xunit;

namespace PagingFiltering.Helpers.Tests;

public class PagingFilteringTests
{
    private record TestItem(string Id, string Nome, int Idade);

    private class Conta
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;

        [FilterIgnore]
        public string PasswordHash { get; set; } = string.Empty;

        [FilterIgnore]
        public bool IsSuperAdmin { get; set; }
    }

    private class RegistroVenda
    {
        public int Id { get; set; }
        public DateTime CriadoEm { get; set; }
        public DateTime? DataCancelamento { get; set; }
        public decimal Valor { get; set; }
    }

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
        var helper = new PaginationFilterHelper<TestItem>();
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
    public void PaginationFilterHelper_ApplyComplexFilters_ShouldCombineAllPredicates()
    {
        var helper = new PaginationFilterHelper<TestItem>();
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

    [Fact]
    public void ApplyKeyset_WithNullableKey_ShouldThrowInvalidOperationException()
    {
        var act = () => new KeysetSeekBuilder<RegistroVenda>()
            .OrderBy(x => x.DataCancelamento);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*NOT NULL*");
    }

    [Fact]
    public void ApplyKeyset_WithMixedCompositeOrdering_ShouldApplyCorrectSeekClause()
    {
        var baseDate = new DateTime(2026, 9, 22, 10, 0, 0, DateTimeKind.Utc);
        var vendas = new List<RegistroVenda>
        {
            new() { Id = 101, CriadoEm = baseDate.AddMinutes(30), Valor = 10 },
            new() { Id = 102, CriadoEm = baseDate.AddMinutes(30), Valor = 20 },
            new() { Id = 103, CriadoEm = baseDate.AddMinutes(20), Valor = 30 },
            new() { Id = 104, CriadoEm = baseDate.AddMinutes(10), Valor = 40 }
        };

        var cursorDate = baseDate.AddMinutes(30);
        var cursorId = 101;

        var result = vendas.ApplyKeysetComposite(
            primaryKeySelector: x => x.CriadoEm,
            primaryAscending: false,
            secondaryKeySelector: x => x.Id,
            secondaryAscending: true,
            afterCursor: (cursorDate, cursorId),
            pageSize: 2
        ).ToList();

        result.Should().HaveCount(2);
        result[0].Id.Should().Be(102);
        result[1].Id.Should().Be(103);

        var builder = new KeysetSeekBuilder<RegistroVenda>()
            .OrderBy(x => x.CriadoEm, ascending: false)
            .ThenBy(x => x.Id, ascending: true);

        var builderResult = builder.ApplySeek(vendas, new object[] { cursorDate, cursorId }, pageSize: 2).ToList();
        builderResult.Should().HaveCount(2);
        builderResult[0].Id.Should().Be(102);
        builderResult[1].Id.Should().Be(103);
    }

    [Fact]
    public void DynamicFilterParser_WithFilterIgnoreAttribute_ShouldThrowSecurityException()
    {
        var criterionPassword = new FilterCriterion("PasswordHash", FilterOperator.Equals, "secret_hash");
        var criterionAdmin = new FilterCriterion("IsSuperAdmin", FilterOperator.Equals, true);

        var actPassword = () => DynamicFilterParser.Parse<Conta>(criterionPassword);
        var actAdmin = () => DynamicFilterParser.Parse<Conta>(criterionAdmin);

        actPassword.Should().Throw<SecurityException>()
            .WithMessage("*PasswordHash*restrição de segurança*");

        actAdmin.Should().Throw<SecurityException>()
            .WithMessage("*IsSuperAdmin*restrição de segurança*");
    }

    [Fact]
    public void PaginationFilterHelper_ShouldOperatePurelyWithoutCache()
    {
        var helper = new PaginationFilterHelper<TestItem>();
        var items = new List<TestItem>
        {
            new("1", "A", 10),
            new("2", "B", 20),
            new("3", "C", 30)
        };

        var paged = helper.ApplyPagination(items, pageNumber: 1, pageSize: 2);
        paged.Items.Should().HaveCount(2);
        paged.TotalItems.Should().Be(3);
    }
}
