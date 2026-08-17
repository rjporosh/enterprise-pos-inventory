using InventoryService.Domain.Common;

namespace InventoryService.Domain.Warehouses;

public class Warehouse : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? ContactName { get; set; }
    public string? Phone { get; set; }
    public bool IsDefault { get; set; } = false;
    public bool IsActive { get; set; } = true;

    public Warehouse() { }

    public Warehouse(string name, string? code = null, string? address = null)
    {
        Name = name;
        Code = code;
        Address = address;
    }
}
