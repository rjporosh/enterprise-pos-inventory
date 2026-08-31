namespace PosService.Application.Stores.Dtos;

public record CreateStoreRequest(
    string Name,
    string Code,
    string? Address,
    string? City,
    string? Country,
    string? Phone,
    string? Email,
    string Currency = "USD");

public record StoreDto(
    Guid Id,
    string Name,
    string Code,
    string? Address,
    string? City,
    string? Country,
    string? Phone,
    string? Email,
    string Currency,
    bool IsActive);
