using PosService.Domain.Common;
using PosService.Domain.Stores;
using SharedKernel;

namespace PosService.Domain.Cashiers;

public class Cashier : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public Guid StoreId { get; set; }
    public bool IsActive { get; set; } = true;

    public Store Store { get; set; } = null!;

    public Cashier() { }

    public Cashier(string fullName, string username, Guid storeId)
    {
        FullName = Guard.NotNullOrEmpty(fullName, nameof(fullName));
        Username = Guard.NotNullOrEmpty(username, nameof(username));
        StoreId = storeId;
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;
}
