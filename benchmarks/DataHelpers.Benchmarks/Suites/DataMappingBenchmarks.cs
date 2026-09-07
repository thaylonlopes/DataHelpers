using System.Reflection;
using BenchmarkDotNet.Attributes;
using DataHelpers.Benchmarks.Models;
using DataMapping.Helpers;
using DataMapping.Helpers.Common;

namespace DataHelpers.Benchmarks.Suites;

[MemoryDiagnoser]
public class DataMappingBenchmarks
{
    private SimpleMapper _mapper = null!;
    private OrderDto _orderDto = null!;
    private CustomerOrderDto _complexDto = null!;
    private PropertyInfo[] _orderDtoProps = null!;
    private PropertyInfo[] _orderEntityProps = null!;

    [GlobalSetup]
    public void Setup()
    {
        _mapper = new SimpleMapper();
        _orderDto = new OrderDto
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-2026-999",
            CustomerName = "Empresa Alpha Logistica",
            CustomerEmail = "contato@alpha.com",
            TotalAmount = 4590.50m,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            ItemCount = 12,
            City = "Sao Paulo",
            Country = "Brasil"
        };

        _complexDto = new CustomerOrderDto
        {
            OrderId = Guid.NewGuid(),
            CustomerName = "Nexus Global Corp",
            Amount = 18900.00m,
            Address = new DeliveryAddressDto
            {
                Street = "Av Paulista 1000",
                PostalCode = "01310-100",
                City = "Sao Paulo"
            }
        };

        _orderDtoProps = typeof(OrderDto).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        _orderEntityProps = typeof(OrderEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        _mapper.Map<OrderDto, OrderEntity>(_orderDto);
        _mapper.Map<CustomerOrderDto, CustomerOrderEntity>(_complexDto);
    }

    [Benchmark(Baseline = true, Description = "1. Mapping: Reflection Property-by-Property")]
    public OrderEntity Map_SimpleProperties_Reflection()
    {
        var entity = new OrderEntity();
        for (int i = 0; i < _orderDtoProps.Length; i++)
        {
            var sourceProp = _orderDtoProps[i];
            for (int j = 0; j < _orderEntityProps.Length; j++)
            {
                var destProp = _orderEntityProps[j];
                if (destProp.Name == sourceProp.Name && destProp.PropertyType == sourceProp.PropertyType)
                {
                    destProp.SetValue(entity, sourceProp.GetValue(_orderDto));
                    break;
                }
            }
        }
        return entity;
    }

    [Benchmark(Description = "1. Mapping: SimpleMapper Expression Tree")]
    public OrderEntity Map_SimpleProperties_SimpleMapper()
    {
        return _mapper.Map<OrderDto, OrderEntity>(_orderDto);
    }

    [Benchmark(Description = "2. Mapping: Nested Complex Manual Assignment")]
    public CustomerOrderEntity Map_ComplexNested_Manual()
    {
        return new CustomerOrderEntity
        {
            OrderId = _complexDto.OrderId,
            CustomerName = _complexDto.CustomerName,
            Amount = _complexDto.Amount,
            Address = _complexDto.Address is null ? null : new DeliveryAddressEntity
            {
                Street = _complexDto.Address.Street,
                PostalCode = _complexDto.Address.PostalCode,
                City = _complexDto.Address.City
            }
        };
    }

    [Benchmark(Description = "2. Mapping: Nested Complex SimpleMapper")]
    public CustomerOrderEntity Map_ComplexNested_SimpleMapper()
    {
        return _mapper.Map<CustomerOrderDto, CustomerOrderEntity>(_complexDto);
    }

    [Benchmark(Description = "3. Mapping: Defensive Exception Handling")]
    public OrderEntity? TryMap_ResultPattern_DefensiveTryCatch()
    {
        try
        {
            if (_orderDto is null)
            {
                throw new ArgumentNullException(nameof(_orderDto));
            }
            return _mapper.Map<OrderDto, OrderEntity>(_orderDto);
        }
        catch
        {
            return null;
        }
    }

    [Benchmark(Description = "3. Mapping: SimpleMapper Result Pattern")]
    public Result<OrderEntity> TryMap_ResultPattern_SimpleMapper()
    {
        return _mapper.TryMap<OrderDto, OrderEntity>(_orderDto);
    }
}
