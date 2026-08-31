using MediatR;
using PosService.Application.Cashiers.Dtos;
using SharedKernel;

namespace PosService.Application.Cashiers.EnsureCashier;

public record EnsureCashierCommand(EnsureCashierRequest Request) : IRequest<Result<CashierDto>>;
