using MediatR;
using SharedKernel;
using InventoryService.Application.Products.Dtos;

namespace InventoryService.Application.Products.GetAllProducts;

public record GetAllProductsQuery(
    int PageNumber = 1,
    int PageSize = 20,
    Guid? CategoryId = null,
    Guid? BrandId = null,
    bool? IsActive = null,
    string? SearchTerm = null,
    string? SortBy = "name",
    bool SortDescending = false) : IRequest<Result<PagedResult<ProductListItemDto>>>;

public record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int PageNumber, int PageSize)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
