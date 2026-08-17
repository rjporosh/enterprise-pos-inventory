using InventoryService.Domain.Common;

namespace InventoryService.Domain.Catalog;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    public Category() { }

    public Category(string name, string? description = null, Guid? parentCategoryId = null)
    {
        Name = SharedKernel.Guard.NotNullOrEmpty(name, nameof(name));
        Description = description;
        ParentCategoryId = parentCategoryId;
    }

    public void Rename(string name)
    {
        Name = SharedKernel.Guard.NotNullOrEmpty(name, nameof(name));
    }
}
