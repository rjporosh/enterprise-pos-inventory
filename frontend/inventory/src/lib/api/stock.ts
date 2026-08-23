import { apiClient, PagedResult } from "./client";

export interface Stock {
  id: string;
  productId: string;
  productName: string;
  productSku: string;
  warehouseId: string;
  warehouseName: string;
  warehouseCode: string;
  quantityOnHand: number;
  quantityReserved: number;
  availableQuantity: number;
  reorderLevel: number;
  maxStockLevel: number;
  lastRestockedAt: string | null;
  createdAt: string;
  updatedAt: string | null;
}

export interface StockListItem {
  id: string;
  productId: string;
  productName: string;
  productSku: string;
  warehouseId: string;
  warehouseName: string;
  warehouseCode: string;
  quantityOnHand: number;
  quantityReserved: number;
  availableQuantity: number;
  reorderLevel: number;
  isLowStock: boolean;
  lastRestockedAt: string | null;
}

export type StockMovementType =
  | "StockIn"
  | "StockOut"
  | "Adjustment"
  | "TransferIn"
  | "TransferOut"
  | "Sale"
  | "Return";

export interface StockMovement {
  id: string;
  stockId: string;
  productId: string;
  productName: string;
  warehouseId: string;
  warehouseName: string;
  movementType: number | StockMovementType;
  quantity: number;
  balanceAfter: number;
  unitCost: number | null;
  referenceType: string | null;
  referenceId: string | null;
  notes: string | null;
  createdAt: string;
}

export interface StockListParams {
  pageNumber?: number;
  pageSize?: number;
  productId?: string;
  warehouseId?: string;
  lowStock?: boolean;
  outOfStock?: boolean;
  sortBy?: string;
  sortDescending?: boolean;
}

export interface CreateStockInput {
  productId: string;
  warehouseId: string;
  initialQuantity: number;
  reorderLevel: number;
  maxStockLevel: number;
  unitCost?: number | null;
}

export interface UpdateStockInput {
  id: string;
  productId: string;
  warehouseId: string;
  reorderLevel: number;
  maxStockLevel: number;
}

export interface StockInInput {
  productId: string;
  warehouseId: string;
  quantity: number;
  unitCost?: number | null;
  referenceType?: string | null;
  referenceId?: string | null;
  notes?: string | null;
}

export interface StockOutInput {
  productId: string;
  warehouseId: string;
  quantity: number;
  referenceType?: string | null;
  referenceId?: string | null;
  notes?: string | null;
}

export interface StockAdjustmentInput {
  productId: string;
  warehouseId: string;
  quantityChange: number;
  notes?: string | null;
}

export interface StockTransferInput {
  productId: string;
  fromWarehouseId: string;
  toWarehouseId: string;
  quantity: number;
  notes?: string | null;
}

export const stockApi = {
  list: (params: StockListParams = {}) =>
    apiClient.get<PagedResult<StockListItem>>("/api/v1/stocks", {
      pageNumber: params.pageNumber ?? 1,
      pageSize: params.pageSize ?? 20,
      productId: params.productId,
      warehouseId: params.warehouseId,
      lowStock: params.lowStock,
      outOfStock: params.outOfStock,
      sortBy: params.sortBy ?? "productName",
      sortDescending: params.sortDescending ?? false,
    }),

  getById: (id: string) => apiClient.get<Stock>(`/api/v1/stocks/${id}`),

  create: (input: CreateStockInput) => apiClient.post<Stock>("/api/v1/stocks", input),

  update: (input: UpdateStockInput) => apiClient.put<Stock>(`/api/v1/stocks/${input.id}`, input),

  remove: (id: string) => apiClient.delete<void>(`/api/v1/stocks/${id}`),

  stockIn: (input: StockInInput) => apiClient.post<StockMovement>("/api/v1/stocks/in", input),

  stockOut: (input: StockOutInput) => apiClient.post<StockMovement>("/api/v1/stocks/out", input),

  adjustment: (input: StockAdjustmentInput) =>
    apiClient.post<StockMovement>("/api/v1/stocks/adjustment", input),

  transfer: (input: StockTransferInput) => apiClient.post<StockMovement>("/api/v1/stocks/transfer", input),
};
