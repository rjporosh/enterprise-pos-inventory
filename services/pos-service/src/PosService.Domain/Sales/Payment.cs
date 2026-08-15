using PosService.Domain.Common;
using SharedKernel;
using BaseEntity = PosService.Domain.Common.BaseEntity;

namespace PosService.Domain.Sales;

public class Payment : BaseEntity
{
    public Guid SaleId { get; set; }
    public PaymentMethodType Method { get; set; }
    public decimal Amount { get; set; }
    public string? ReferenceNumber { get; set; }
    public DateTime PaidAt { get; set; }

    public Sale Sale { get; set; } = null!;

    public Payment() { }

    public Payment(Guid saleId, PaymentMethodType method, decimal amount, string? referenceNumber = null)
    {
        SaleId = saleId;
        Method = method;
        Amount = Guard.NotNegative(amount, nameof(amount));
        ReferenceNumber = referenceNumber;
        PaidAt = DateTime.UtcNow;
    }
}
