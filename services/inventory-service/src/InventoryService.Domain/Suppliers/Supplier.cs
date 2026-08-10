using InventoryService.Domain.Common;

namespace InventoryService.Domain.Suppliers;

public class Supplier : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public bool IsActive { get; set; } = true;

    public Supplier() { }

    public Supplier(string name, string? contactName = null, string? email = null, string? phone = null)
    {
        Name = name;
        ContactName = contactName;
        Email = email;
        Phone = phone;
    }

    public void UpdateContact(string? email, string? phone)
    {
        Email = email;
        Phone = phone;
    }
}
