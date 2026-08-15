using PosService.Domain.Common;
using SharedKernel;
using BaseEntity = PosService.Domain.Common.BaseEntity;

namespace PosService.Domain.Stores;

public class Store : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string Currency { get; set; } = "USD";
    public bool IsActive { get; set; } = true;

    public Store() { }

    public Store(string name, string code, string currency = "USD")
    {
        Name = Guard.NotNullOrEmpty(name, nameof(name));
        Code = Guard.NotNullOrEmpty(code, nameof(code));
        Currency = Guard.NotNullOrEmpty(currency, nameof(currency));
    }

    public void Rename(string name)
    {
        Name = Guard.NotNullOrEmpty(name, nameof(name));
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;
}
