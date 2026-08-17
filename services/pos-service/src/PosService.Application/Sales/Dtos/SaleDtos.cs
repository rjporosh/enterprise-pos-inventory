using PosService.Domain.Sales;

namespace PosService.Application.Sales.Dtos;

public record CreateSaleRequest(Guid StoreId, Guid RegisterId, Guid CashierId, Guid CashSessionId, Guid? CustomerId);

/// <summary>
/// Request to add a line item to an open (Draft) sale. ProductId/ProductName/Sku/UnitPrice are supplied
/// by the caller (the POS terminal, which already resolved the product via the Inventory service or a
/// local cache) rather than looked up here — POS never queries Inventory's database directly (ADR-001).
/// </summary>
public record AddSaleItemRequest(Guid SaleId, Guid ProductId, string ProductName, string Sku, decimal UnitPrice, int Quantity, decimal DiscountAmount = 0, decimal TaxAmount = 0);

public record RemoveSaleItemRequest(Guid SaleId, Guid SaleItemId);

public record CompleteSaleRequest(Guid SaleId, IReadOnlyList<SalePaymentRequest> Payments);

public record SalePaymentRequest(PaymentMethodType Method, decimal Amount, string? ReferenceNumber);

public record VoidSaleRequest(Guid SaleId, string Reason);

public record SaleItemDto(Guid Id, Guid ProductId, string ProductName, string Sku, decimal UnitPrice, int Quantity, decimal DiscountAmount, decimal TaxAmount, decimal LineTotal);

public record PaymentDto(Guid Id, PaymentMethodType Method, decimal Amount, string? ReferenceNumber, DateTime PaidAt);

public record SaleDto(
    Guid Id,
    string SaleNumber,
    Guid StoreId,
    Guid RegisterId,
    Guid CashierId,
    Guid CashSessionId,
    Guid? CustomerId,
    DateTime SaleDate,
    SaleStatus Status,
    decimal SubtotalAmount,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal ChangeAmount,
    string? VoidReason,
    IReadOnlyList<SaleItemDto> Items,
    IReadOnlyList<PaymentDto> Payments);

public record SaleListItemDto(Guid Id, string SaleNumber, DateTime SaleDate, SaleStatus Status, decimal TotalAmount, Guid CashierId, Guid StoreId);

public record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int PageNumber, int PageSize)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
