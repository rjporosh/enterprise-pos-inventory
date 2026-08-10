using InventoryService.Domain.Common;

namespace InventoryService.Domain.Catalog;

public class Brand : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Website { get; set; }
    public bool IsActive { get; set; } = true;

    public Brand() { }

    public Brand(string name, string? description = null, string? website = null)
    {
        Name = name;
        Description = description;
        Website = website;
    }

    public void Rename(string name)
    {
        Name = SharedKernel.Guard.NotNullOrEmpty(name, nameof(name));
    }
}
