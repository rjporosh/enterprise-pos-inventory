using PosService.Domain.Common;
using SharedKernel;
using BaseEntity = PosService.Domain.Common.BaseEntity;

namespace PosService.Domain.Customers;

public class Customer : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;

    public Customer() { }

    public Customer(string fullName, string? email = null, string? phone = null)
    {
        FullName = Guard.NotNullOrEmpty(fullName, nameof(fullName));
        Email = email;
        Phone = phone;
    }

    public void Rename(string fullName)
    {
        FullName = Guard.NotNullOrEmpty(fullName, nameof(fullName));
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;
}
