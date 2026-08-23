"use client";

import { useState } from "react";
import { Field, Input, Button } from "@/components/ui";
import { ProductFormErrors, ProductFormValues, validateProductForm } from "../validation";

interface ProductFormProps {
  initialValues: ProductFormValues;
  submitLabel: string;
  saving: boolean;
  serverError?: string | null;
  isEdit?: boolean;
  onSubmit: (values: ProductFormValues) => void;
  onCancel: () => void;
}

export function ProductForm({
  initialValues,
  submitLabel,
  saving,
  serverError,
  isEdit,
  onSubmit,
  onCancel,
}: ProductFormProps) {
  const [values, setValues] = useState<ProductFormValues>(initialValues);
  const [errors, setErrors] = useState<ProductFormErrors>({});

  function set<K extends keyof ProductFormValues>(key: K, value: ProductFormValues[K]) {
    setValues((v) => ({ ...v, [key]: value }));
  }

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    const nextErrors = validateProductForm(values);
    setErrors(nextErrors);
    if (Object.keys(nextErrors).length > 0) return;
    onSubmit(values);
  }

  return (
    <form onSubmit={handleSubmit} noValidate>
      {serverError && (
        <div
          role="alert"
          style={{
            background: "var(--color-danger-soft)",
            color: "var(--color-danger)",
            borderRadius: "var(--radius-sm)",
            padding: "10px 14px",
            marginBottom: 16,
            fontSize: 13.5,
          }}
        >
          {serverError}
        </div>
      )}

      <div className="form-grid">
        <div className="form-grid-full">
          <Field label="Product name" htmlFor="name" required error={errors.name}>
            <Input
              id="name"
              value={values.name}
              onChange={(e) => set("name", e.target.value)}
              hasError={!!errors.name}
              placeholder="e.g. Cotton Burkha - Black, XL"
            />
          </Field>
        </div>

        <Field label="SKU" htmlFor="sku" required error={errors.sku} hint="Unique across the catalog.">
          <Input id="sku" value={values.sku} onChange={(e) => set("sku", e.target.value)} hasError={!!errors.sku} />
        </Field>

        <Field label="Barcode" htmlFor="barcode" error={errors.barcode} hint="Optional. Scan or type.">
          <Input id="barcode" value={values.barcode} onChange={(e) => set("barcode", e.target.value)} hasError={!!errors.barcode} />
        </Field>

        <div className="form-grid-full">
          <Field label="Description" htmlFor="description">
            <Input id="description" value={values.description} onChange={(e) => set("description", e.target.value)} />
          </Field>
        </div>

        <Field
          label="Category ID"
          htmlFor="categoryId"
          required
          error={errors.categoryId}
          hint="Category management isn't available from the backend yet — paste an existing category GUID."
        >
          <Input id="categoryId" value={values.categoryId} onChange={(e) => set("categoryId", e.target.value)} hasError={!!errors.categoryId} />
        </Field>

        <Field
          label="Brand ID"
          htmlFor="brandId"
          required
          error={errors.brandId}
          hint="Brand management isn't available from the backend yet — paste an existing brand GUID."
        >
          <Input id="brandId" value={values.brandId} onChange={(e) => set("brandId", e.target.value)} hasError={!!errors.brandId} />
        </Field>

        <Field
          label="Unit ID"
          htmlFor="unitId"
          required
          error={errors.unitId}
          hint="Unit management isn't available from the backend yet — paste an existing unit GUID."
        >
          <Input id="unitId" value={values.unitId} onChange={(e) => set("unitId", e.target.value)} hasError={!!errors.unitId} />
        </Field>

        <Field label="Supplier ID" htmlFor="supplierId" error={errors.supplierId} hint="Optional.">
          <Input id="supplierId" value={values.supplierId} onChange={(e) => set("supplierId", e.target.value)} hasError={!!errors.supplierId} />
        </Field>

        <Field label="Cost price" htmlFor="costPrice" required error={errors.costPrice}>
          <Input id="costPrice" type="number" min={0} step="0.01" value={values.costPrice} onChange={(e) => set("costPrice", e.target.value)} hasError={!!errors.costPrice} />
        </Field>

        <Field label="Selling price" htmlFor="sellingPrice" required error={errors.sellingPrice}>
          <Input id="sellingPrice" type="number" min={0} step="0.01" value={values.sellingPrice} onChange={(e) => set("sellingPrice", e.target.value)} hasError={!!errors.sellingPrice} />
        </Field>

        <Field label="Discount %" htmlFor="discountPercent" error={errors.discountPercent}>
          <Input id="discountPercent" type="number" min={0} max={100} step="0.01" value={values.discountPercent} onChange={(e) => set("discountPercent", e.target.value)} hasError={!!errors.discountPercent} />
        </Field>

        <Field label="Tax %" htmlFor="taxPercent" error={errors.taxPercent}>
          <Input id="taxPercent" type="number" min={0} max={100} step="0.01" value={values.taxPercent} onChange={(e) => set("taxPercent", e.target.value)} hasError={!!errors.taxPercent} />
        </Field>

        <Field label="Reorder level" htmlFor="reorderLevel" required error={errors.reorderLevel}>
          <Input id="reorderLevel" type="number" min={0} value={values.reorderLevel} onChange={(e) => set("reorderLevel", e.target.value)} hasError={!!errors.reorderLevel} />
        </Field>

        <Field label="Max stock level" htmlFor="maxStockLevel" required error={errors.maxStockLevel}>
          <Input id="maxStockLevel" type="number" min={0} value={values.maxStockLevel} onChange={(e) => set("maxStockLevel", e.target.value)} hasError={!!errors.maxStockLevel} />
        </Field>

        <Field label="Track inventory" htmlFor="trackInventory">
          <label style={{ display: "flex", alignItems: "center", gap: 8, fontSize: 13.5 }}>
            <input
              id="trackInventory"
              type="checkbox"
              checked={values.trackInventory}
              onChange={(e) => set("trackInventory", e.target.checked)}
            />
            Deduct stock automatically when sold
          </label>
        </Field>

        {isEdit && (
          <Field label="Active" htmlFor="isActive">
            <label style={{ display: "flex", alignItems: "center", gap: 8, fontSize: 13.5 }}>
              <input id="isActive" type="checkbox" checked={values.isActive} onChange={(e) => set("isActive", e.target.checked)} />
              Visible for sale
            </label>
          </Field>
        )}
      </div>

      <div className="form-actions">
        <Button type="button" variant="secondary" onClick={onCancel} disabled={saving}>
          Cancel
        </Button>
        <Button type="submit" loading={saving}>
          {submitLabel}
        </Button>
      </div>
    </form>
  );
}
