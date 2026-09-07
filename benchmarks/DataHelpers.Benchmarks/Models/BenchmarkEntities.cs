namespace DataHelpers.Benchmarks.Models;

/// <summary>
/// Modelo de transferência de dados utilizado para validação de alta vazão em benchmarks.
/// </summary>
public class OrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public int ItemCount { get; set; }
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

/// <summary>
/// Entidade de domínio correspondente ao DTO de pedidos.
/// </summary>
public class OrderEntity
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public int ItemCount { get; set; }
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

/// <summary>
/// Modelo DTO aninhado para avaliação de complexidade de mapeamento.
/// </summary>
public class CustomerOrderDto
{
    public Guid OrderId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DeliveryAddressDto? Address { get; set; }
}

/// <summary>
/// Entidade de domínio aninhada correspondente.
/// </summary>
public class CustomerOrderEntity
{
    public Guid OrderId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DeliveryAddressEntity? Address { get; set; }
}

public class DeliveryAddressDto
{
    public string Street { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
}

public class DeliveryAddressEntity
{
    public string Street { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
}
