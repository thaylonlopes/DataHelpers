using MongoDriver.Helpers.Models;

namespace Example.Showcase.Models;

/// <summary>
/// DTO plano para importação de registros de pedidos a partir de streams CSV.
/// </summary>
public sealed class OrderImportDto
{
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string ItemDescription { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string Region { get; set; } = string.Empty;
}

/// <summary>
/// Entidade de domínio para persistência relacional no SQLite e processamento transacional.
/// </summary>
public sealed class OrderEntity
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string ItemDescription { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Processing";
    public string Region { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Modelo de projeção de leitura e arquivamento para o MongoDB (CQRS Read Model).
/// </summary>
public sealed class OrderAuditDocument : Document<Guid>
{
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public Guid AuditLogId { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}

