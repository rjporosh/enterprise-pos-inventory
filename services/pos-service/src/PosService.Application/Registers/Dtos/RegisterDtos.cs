namespace PosService.Application.Registers.Dtos;

public record CreateRegisterRequest(string Name, string Code, Guid StoreId);

public record RegisterDto(Guid Id, string Name, string Code, Guid StoreId, bool IsActive);
