import { describe, expect, it } from "vitest";
import { emptyProductForm, toCreateProductInput, validateProductForm, ProductFormValues } from "../validation";

const VALID_GUID = "11111111-1111-1111-1111-111111111111";

function validForm(overrides: Partial<ProductFormValues> = {}): ProductFormValues {
  return {
    ...emptyProductForm,
    name: "Cotton Burkha - Black, XL",
    sku: "BRK-001",
    categoryId: VALID_GUID,
    brandId: VALID_GUID,
    unitId: VALID_GUID,
    costPrice: "30",
    sellingPrice: "45",
    reorderLevel: "5",
    maxStockLevel: "100",
    ...overrides,
  };
}

describe("validateProductForm", () => {
  it("passes for a fully valid form", () => {
    expect(validateProductForm(validForm())).toEqual({});
  });

  it("requires name and sku", () => {
    const errors = validateProductForm(validForm({ name: "", sku: "" }));
    expect(errors.name).toBeTruthy();
    expect(errors.sku).toBeTruthy();
  });

  it("requires category/brand/unit to be valid GUIDs", () => {
    const errors = validateProductForm(validForm({ categoryId: "not-a-guid", brandId: "", unitId: "123" }));
    expect(errors.categoryId).toBeTruthy();
    expect(errors.brandId).toBeTruthy();
    expect(errors.unitId).toBeTruthy();
  });

  it("rejects negative prices", () => {
    const errors = validateProductForm(validForm({ costPrice: "-1", sellingPrice: "-5" }));
    expect(errors.costPrice).toBeTruthy();
    expect(errors.sellingPrice).toBeTruthy();
  });

  it("rejects discount/tax outside 0-100", () => {
    const errors = validateProductForm(validForm({ discountPercent: "150", taxPercent: "-10" }));
    expect(errors.discountPercent).toBeTruthy();
    expect(errors.taxPercent).toBeTruthy();
  });

  it("allows empty optional discount/tax/supplier", () => {
    const errors = validateProductForm(validForm({ discountPercent: "", taxPercent: "", supplierId: "" }));
    expect(errors.discountPercent).toBeUndefined();
    expect(errors.taxPercent).toBeUndefined();
    expect(errors.supplierId).toBeUndefined();
  });
});

describe("toCreateProductInput", () => {
  it("converts string form values to typed numbers and trims/nulls optional fields", () => {
    const input = toCreateProductInput(validForm({ description: "  nice  ", barcode: "", discountPercent: "10" }));
    expect(input.description).toBe("nice");
    expect(input.barcode).toBeNull();
    expect(input.costPrice).toBe(30);
    expect(input.sellingPrice).toBe(45);
    expect(input.discountPercent).toBe(10);
    expect(input.taxPercent).toBeNull();
  });
});
