import { CreateProductInput } from "@/lib/api/products";

export type ProductFormValues = {
  name: string;
  description: string;
  sku: string;
  barcode: string;
  categoryId: string;
  brandId: string;
  unitId: string;
  supplierId: string;
  costPrice: string;
  sellingPrice: string;
  discountPercent: string;
  taxPercent: string;
  reorderLevel: string;
  maxStockLevel: string;
  trackInventory: boolean;
  isActive: boolean;
};

export const emptyProductForm: ProductFormValues = {
  name: "",
  description: "",
  sku: "",
  barcode: "",
  categoryId: "",
  brandId: "",
  unitId: "",
  supplierId: "",
  costPrice: "0",
  sellingPrice: "0",
  discountPercent: "",
  taxPercent: "",
  reorderLevel: "0",
  maxStockLevel: "0",
  trackInventory: true,
  isActive: true,
};

export type ProductFormErrors = Partial<Record<keyof ProductFormValues, string>>;

const GUID_RE = /^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$/;

/**
 * Mirrors CreateProductValidator.cs / UpdateProductValidator.cs (FluentValidation) on the backend,
 * so the form fails fast with the same rules the API will enforce -- this does not replace
 * server-side validation, it just avoids a round trip for the common cases.
 */
export function validateProductForm(values: ProductFormValues): ProductFormErrors {
  const errors: ProductFormErrors = {};

  if (!values.name.trim()) errors.name = "Product name is required.";
  else if (values.name.length > 300) errors.name = "Name must not exceed 300 characters.";

  if (!values.sku.trim()) errors.sku = "SKU is required.";
  else if (values.sku.length > 100) errors.sku = "SKU must not exceed 100 characters.";

  if (values.barcode && values.barcode.length > 100) errors.barcode = "Barcode must not exceed 100 characters.";

  if (!values.categoryId.trim()) errors.categoryId = "Category ID is required.";
  else if (!GUID_RE.test(values.categoryId.trim())) errors.categoryId = "Must be a valid GUID.";

  if (!values.brandId.trim()) errors.brandId = "Brand ID is required.";
  else if (!GUID_RE.test(values.brandId.trim())) errors.brandId = "Must be a valid GUID.";

  if (!values.unitId.trim()) errors.unitId = "Unit ID is required.";
  else if (!GUID_RE.test(values.unitId.trim())) errors.unitId = "Must be a valid GUID.";

  if (values.supplierId && !GUID_RE.test(values.supplierId.trim())) errors.supplierId = "Must be a valid GUID.";

  const cost = Number(values.costPrice);
  if (Number.isNaN(cost) || cost < 0) errors.costPrice = "Cost price cannot be negative.";

  const selling = Number(values.sellingPrice);
  if (Number.isNaN(selling) || selling < 0) errors.sellingPrice = "Selling price cannot be negative.";

  if (values.discountPercent) {
    const d = Number(values.discountPercent);
    if (Number.isNaN(d) || d < 0 || d > 100) errors.discountPercent = "Discount must be between 0 and 100.";
  }

  if (values.taxPercent) {
    const t = Number(values.taxPercent);
    if (Number.isNaN(t) || t < 0 || t > 100) errors.taxPercent = "Tax must be between 0 and 100.";
  }

  const reorder = Number(values.reorderLevel);
  if (Number.isNaN(reorder) || reorder < 0) errors.reorderLevel = "Reorder level cannot be negative.";

  const maxStock = Number(values.maxStockLevel);
  if (Number.isNaN(maxStock) || maxStock < 0) errors.maxStockLevel = "Max stock level cannot be negative.";

  return errors;
}

export function toCreateProductInput(values: ProductFormValues): CreateProductInput {
  return {
    name: values.name.trim(),
    description: values.description.trim() || null,
    sku: values.sku.trim(),
    barcode: values.barcode.trim() || null,
    categoryId: values.categoryId.trim(),
    brandId: values.brandId.trim(),
    unitId: values.unitId.trim(),
    supplierId: values.supplierId.trim() || null,
    costPrice: Number(values.costPrice),
    sellingPrice: Number(values.sellingPrice),
    discountPercent: values.discountPercent ? Number(values.discountPercent) : null,
    taxPercent: values.taxPercent ? Number(values.taxPercent) : null,
    reorderLevel: Number(values.reorderLevel),
    maxStockLevel: Number(values.maxStockLevel),
    trackInventory: values.trackInventory,
  };
}
