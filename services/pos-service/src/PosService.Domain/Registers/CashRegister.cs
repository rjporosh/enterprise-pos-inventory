using PosService.Domain.Common;
using PosService.Domain.Stores;
using SharedKernel;

namespace PosService.Domain.Registers;

public class CashRegister : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public Guid StoreId { get; set; }
    public bool IsActive { get; set; } = true;

    public Store Store { get; set; } = null!;

    public CashRegister() { }

    public CashRegister(string name, string code, Guid storeId)
    {
        Name = Guard.NotNullOrEmpty(name, nameof(name));
        Code = Guard.NotNullOrEmpty(code, nameof(code));
        StoreId = storeId;
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;
}
