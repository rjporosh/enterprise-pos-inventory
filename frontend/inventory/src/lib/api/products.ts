import { apiClient, PagedResult } from "./client";

/** Mirrors InventoryService.Application.Products.Dtos.ProductDto exactly. */
export interface Product {
  id: string;
  name: string;
  description: string | null;
  sku: string;
  barcode: string | null;
  categoryId: string;
  brandId: string;
  unitId: string;
  supplierId: string | null;
  costPrice: number;
  sellingPrice: number;
  discountPercent: number | null;
  taxPercent: number | null;
  reorderLevel: number;
  maxStockLevel: number;
  isActive: boolean;
  trackInventory: boolean;
  createdAt: string;
}

/** Mirrors ProductListItemDto exactly -- this is what the paginated list endpoint returns. */
export interface ProductListItem {
  id: string;
  name: string;
  sku: string;
  barcode: string | null;
  categoryName: string;
  brandName: string;
  unitSymbol: string;
  sellingPrice: number;
  isActive: boolean;
  reorderLevel: number;
}

export interface ProductListParams {
  pageNumber?: number;
  pageSize?: number;
  categoryId?: string;
  brandId?: string;
  isActive?: boolean;
  searchTerm?: string;
  sortBy?: "name" | "sku" | "sellingPrice" | "createdAt";
  sortDescending?: boolean;
}

export interface CreateProductInput {
  name: string;
  description?: string | null;
  sku: string;
  barcode?: string | null;
  categoryId: string;
  brandId: string;
  unitId: string;
  supplierId?: string | null;
  costPrice: number;
  sellingPrice: number;
  discountPercent?: number | null;
  taxPercent?: number | null;
  reorderLevel: number;
  maxStockLevel: number;
  trackInventory?: boolean;
}

export interface UpdateProductInput extends CreateProductInput {
  id: string;
  isActive: boolean;
}

export const productsApi = {
  list: (params: ProductListParams = {}) =>
    apiClient.get<PagedResult<ProductListItem>>("/api/v1/products", {
      pageNumber: params.pageNumber ?? 1,
      pageSize: params.pageSize ?? 20,
      categoryId: params.categoryId,
      brandId: params.brandId,
      isActive: params.isActive,
      searchTerm: params.searchTerm,
      sortBy: params.sortBy ?? "name",
      sortDescending: params.sortDescending ?? false,
    }),

  getById: (id: string) => apiClient.get<Product>(`/api/v1/products/${id}`),

  create: (input: CreateProductInput) => apiClient.post<string>("/api/v1/products", input),

  update: (input: UpdateProductInput) => apiClient.put<void>(`/api/v1/products/${input.id}`, input),

  remove: (id: string) => apiClient.delete<void>(`/api/v1/products/${id}`),
};
