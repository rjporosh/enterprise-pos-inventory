namespace PosService.Application.Cashiers.Dtos;

/// <summary>
/// pos-service has no knowledge of auth-service's identity model (separate databases, ADR-001) —
/// a POS "Cashier" is its own row, scoped to a Store, with its own Guid Id. This request bridges
/// the two: the frontend calls /api/v1/cashiers/ensure once per (signed-in user, store) pair using
/// the user's email as Username, and gets back the pos-service CashierId to use for every
/// subsequent sales/cash-session call — Username is unique, so this is idempotent.
/// </summary>
public record EnsureCashierRequest(Guid StoreId, string Username, string FullName, string? Email, string? Phone);

public record CashierDto(Guid Id, string FullName, string Username, string? Email, string? Phone, Guid StoreId, bool IsActive);
