import { posApiClient } from "./client";

export interface EnsureCashierInput {
  storeId: string;
  username: string;
  fullName: string;
  email?: string | null;
  phone?: string | null;
}

export interface Cashier {
  id: string;
  fullName: string;
  username: string;
  email: string | null;
  phone: string | null;
  storeId: string;
  isActive: boolean;
}

/**
 * pos-service has its own Cashier entity (separate database from auth-service, ADR-001) — this
 * bridges an authenticated user to their pos-service Cashier record, get-or-create by Username
 * (the user's email), idempotent to call on every Setup page load.
 */
export const cashiersApi = {
  ensure: (input: EnsureCashierInput) => posApiClient.post<Cashier>("/api/v1/cashiers/ensure", input),
};
