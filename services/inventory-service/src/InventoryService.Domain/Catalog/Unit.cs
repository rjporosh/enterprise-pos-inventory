using InventoryService.Domain.Common;

namespace InventoryService.Domain.Catalog;

public class Unit : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public Unit() { }

    public Unit(string name, string symbol, string? description = null)
    {
        Name = name;
        Symbol = symbol;
        Description = description;
    }
}
