import { inventoryApiClient, PagedResult } from "./client";

/**
 * Read-only product lookup for the POS catalog. Matches InventoryService's ProductListItemDto
 * exactly (see frontend/inventory/src/lib/api/products.ts for the authoritative shape).
 *
 * IMPORTANT GAP: `searchTerm` only matches product Name and SKU on the backend
 * (ProductRepository filters on `Name`/`Sku` only) — it does NOT match Barcode, even though
 * Product has a Barcode field and the repository has an unused GetByBarcodeAsync method that no
 * controller exposes. A USB barcode scanner (which types the barcode digits + Enter into
 * whatever input is focused) will therefore only find a product if its barcode also happens to
 * match text in the name/SKU. Documented in docs/API-GAPS.md as a priority gap; the search box
 * is still scanner-friendly (auto-submits on Enter) so it works the moment the backend adds
 * barcode matching, but today it is effectively name/SKU search only.
 */
export interface CatalogProduct {
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

export const catalogApi = {
  search: (searchTerm: string, pageSize = 15) =>
    inventoryApiClient.get<PagedResult<CatalogProduct>>("/api/v1/products", {
      searchTerm: searchTerm || undefined,
      isActive: true,
      pageNumber: 1,
      pageSize,
      sortBy: "name",
    }),
};
