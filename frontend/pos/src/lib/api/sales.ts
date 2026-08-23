import { posApiClient, PagedResult } from "./client";

export const SaleStatus = { Draft: 1, Completed: 2, Voided: 3 } as const;
export type SaleStatusValue = (typeof SaleStatus)[keyof typeof SaleStatus];

export const PaymentMethodType = {
  Cash: 1,
  Card: 2,
  MobileMoney: 3,
  StoreCredit: 4,
  Other: 5,
} as const;
export type PaymentMethodTypeValue = (typeof PaymentMethodType)[keyof typeof PaymentMethodType];

export const PAYMENT_METHOD_LABELS: Record<PaymentMethodTypeValue, string> = {
  [PaymentMethodType.Cash]: "Cash",
  [PaymentMethodType.Card]: "Card",
  [PaymentMethodType.MobileMoney]: "Mobile money",
  [PaymentMethodType.StoreCredit]: "Store credit",
  [PaymentMethodType.Other]: "Other",
};

export interface SaleItem {
  id: string;
  productId: string;
  productName: string;
  sku: string;
  unitPrice: number;
  quantity: number;
  discountAmount: number;
  taxAmount: number;
  lineTotal: number;
}

export interface Payment {
  id: string;
  method: PaymentMethodTypeValue;
  amount: number;
  referenceNumber: string | null;
  paidAt: string;
}

export interface Sale {
  id: string;
  saleNumber: string;
  storeId: string;
  registerId: string;
  cashierId: string;
  cashSessionId: string;
  customerId: string | null;
  saleDate: string;
  status: SaleStatusValue;
  subtotalAmount: number;
  discountAmount: number;
  taxAmount: number;
  totalAmount: number;
  paidAmount: number;
  changeAmount: number;
  voidReason: string | null;
  items: SaleItem[];
  payments: Payment[];
}

export interface SaleListItem {
  id: string;
  saleNumber: string;
  saleDate: string;
  status: SaleStatusValue;
  totalAmount: number;
  cashierId: string;
  storeId: string;
}

export interface CreateSaleInput {
  storeId: string;
  registerId: string;
  cashierId: string;
  cashSessionId: string;
  customerId?: string | null;
}

export interface AddSaleItemInput {
  saleId: string;
  productId: string;
  productName: string;
  sku: string;
  unitPrice: number;
  quantity: number;
  discountAmount?: number;
  taxAmount?: number;
}

export interface SalePaymentInput {
  method: PaymentMethodTypeValue;
  amount: number;
  referenceNumber?: string | null;
}

export interface SaleListParams {
  pageNumber?: number;
  pageSize?: number;
  storeId?: string;
  cashierId?: string;
  status?: SaleStatusValue;
  fromDate?: string;
  toDate?: string;
}

export const salesApi = {
  /** Returns the new sale's ID (draft). */
  create: (input: CreateSaleInput) => posApiClient.post<string>("/api/v1/sales", input),

  getById: (id: string) => posApiClient.get<Sale>(`/api/v1/sales/${id}`),

  list: (params: SaleListParams = {}) =>
    posApiClient.get<PagedResult<SaleListItem>>("/api/v1/sales", {
      pageNumber: params.pageNumber ?? 1,
      pageSize: params.pageSize ?? 20,
      storeId: params.storeId,
      cashierId: params.cashierId,
      status: params.status,
      fromDate: params.fromDate,
      toDate: params.toDate,
    }),

  /** Returns the new sale item's ID. */
  addItem: (input: AddSaleItemInput) => posApiClient.post<string>("/api/v1/sales/items", input),

  /** 204 No Content on success. */
  removeItem: (saleId: string, saleItemId: string) =>
    posApiClient.delete<void>("/api/v1/sales/items", { saleId, saleItemId }),

  /** 204 No Content on success — re-fetch via getById for the receipt. */
  complete: (saleId: string, payments: SalePaymentInput[]) =>
    posApiClient.post<void>("/api/v1/sales/complete", { saleId, payments }),

  /** 204 No Content on success. */
  void: (saleId: string, reason: string) => posApiClient.post<void>("/api/v1/sales/void", { saleId, reason }),
};
