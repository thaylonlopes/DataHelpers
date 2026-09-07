using System.Reflection;
using BenchmarkDotNet.Attributes;
using DataHelpers.Benchmarks.Models;
using PagingFiltering.Helpers.Specifications;

namespace DataHelpers.Benchmarks.Suites;

[MemoryDiagnoser]
public class PagingFilteringBenchmarks
{
    private List<OrderEntity> _orders = null!;
    private Specification<OrderEntity> _compiledSpec = null!;
    private PropertyInfo _amountProp = null!;

    [GlobalSetup]
    public void Setup()
    {
        _orders = new List<OrderEntity>(10000);
        for (int i = 0; i < 10000; i++)
        {
            _orders.Add(new OrderEntity
            {
                Id = Guid.NewGuid(),
                OrderNumber = $"ORD-2026-{i:D5}",
                CustomerName = $"Cliente Corporativo {i}",
                TotalAmount = i * 1.5m,
                ItemCount = i,
                IsActive = i % 2 == 0,
                City = i % 3 == 0 ? "Sao Paulo" : "Rio de Janeiro"
            });
        }

        _amountProp = typeof(OrderEntity).GetProperty(nameof(OrderEntity.TotalAmount))!;

        var criterion = new FilterCriterion(
            nameof(OrderEntity.TotalAmount),
            FilterOperator.GreaterThanOrEqual,
            5000.00m
        );
        _compiledSpec = DynamicFilterParser.Parse<OrderEntity>(criterion);
    }

    [Benchmark(Baseline = true, Description = "1. Pagination: Deep Page Offset Skip(9500)")]
    public List<OrderEntity> Pagination_DeepPage_OffsetSkip()
    {
        return _orders.Skip(9500).Take(50).ToList();
    }

    [Benchmark(Description = "1. Pagination: Keyset Seek Seek O(1)")]
    public List<OrderEntity> Pagination_DeepPage_KeysetSeek()
    {
        return _orders.Where(x => x.ItemCount >= 9500).Take(50).ToList();
    }

    [Benchmark(Description = "2. Filter: Evaluation via Reflection Loop")]
    public int Filter_Evaluation_Reflection()
    {
        var count = 0;
        for (int i = 0; i < 1000; i++)
        {
            var val = (decimal)_amountProp.GetValue(_orders[i])!;
            if (val >= 5000.00m)
            {
                count++;
            }
        }
        return count;
    }

    [Benchmark(Description = "2. Filter: DynamicFilterParser Specification")]
    public int Filter_Evaluation_Specification()
    {
        var count = 0;
        for (int i = 0; i < 1000; i++)
        {
            if (_compiledSpec.IsSatisfiedBy(_orders[i]))
            {
                count++;
            }
        }
        return count;
    }

    [Benchmark(Description = "3. Specification: Manual Boolean Check")]
    public bool Specification_Manual()
    {
        var sample = _orders[500];
        return sample.TotalAmount >= 500.00m && sample.IsActive && sample.City == "Sao Paulo";
    }

    [Benchmark(Description = "3. Specification: Composed AndSpecification")]
    public bool Specification_Composed()
    {
        var specA = new DirectSpecification<OrderEntity>(x => x.TotalAmount >= 500.00m);
        var specB = new DirectSpecification<OrderEntity>(x => x.IsActive);
        var specC = new DirectSpecification<OrderEntity>(x => x.City == "Sao Paulo");
        var composed = specA.And(specB).And(specC);
        return composed.IsSatisfiedBy(_orders[500]);
    }
}
