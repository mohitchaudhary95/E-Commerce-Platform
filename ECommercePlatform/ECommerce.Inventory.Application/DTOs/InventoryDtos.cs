namespace ECommerce.Inventory.Application.DTOs;

public class InventoryDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public int ReservedQuantity { get; set; }
    public int AvailableQuantity { get; set; }
    public int LowStockThreshold { get; set; }
    public bool IsLowStock { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class SetStockDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int LowStockThreshold { get; set; } = 10;
}

public class AdjustStockDto
{
    public int Quantity { get; set; } // Positive = increase, Negative = decrease
    public string Reason { get; set; } = string.Empty;
}
