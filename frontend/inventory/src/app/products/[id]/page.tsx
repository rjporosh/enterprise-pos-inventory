"use client";

import { useEffect } from "react";
import { useParams, useRouter } from "next/navigation";
import { PageHeader, Card, ErrorState, Skeleton } from "@/components/ui";
import { ProductForm } from "@/features/products/components/ProductForm";
import { ProductFormValues, toCreateProductInput } from "@/features/products/validation";
import { Product } from "@/lib/api/products";
import {
  productDetailCleared,
  productDetailRequested,
  productUpdateReset,
  productUpdateRequested,
} from "@/features/products/slice";
import { useAppDispatch, useAppSelector } from "@/lib/store/hooks";

export default function EditProductPage() {
  const { id } = useParams<{ id: string }>();
  const dispatch = useAppDispatch();
  const router = useRouter();
  const detail = useAppSelector((s) => s.products.detail);
  const updateState = useAppSelector((s) => s.products.update);

  useEffect(() => {
    dispatch(productDetailRequested(id));
    dispatch(productUpdateReset());
    return () => {
      dispatch(productDetailCleared());
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id]);

  useEffect(() => {
    if (updateState.status === "succeeded") {
      router.push("/products");
    }
  }, [updateState.status, router]);

  return (
    <>
      <PageHeader title="Edit product" subtitle={detail.data ? `SKU ${detail.data.sku}` : undefined} />

      {detail.status === "loading" && (
        <Card>
          <Skeleton height={280} />
        </Card>
      )}

      {detail.status === "failed" && (
        <ErrorState message={detail.error ?? "Failed to load product."} onRetry={() => dispatch(productDetailRequested(id))} />
      )}

      {detail.status === "succeeded" && detail.data && (
        <Card>
          <ProductForm
            isEdit
            initialValues={toFormValues(detail.data)}
            submitLabel="Save changes"
            saving={updateState.status === "saving"}
            serverError={updateState.status === "failed" ? updateState.error : null}
            onSubmit={(values) =>
              dispatch(
                productUpdateRequested({
                  ...toCreateProductInput(values),
                  id,
                  isActive: values.isActive,
                })
              )
            }
            onCancel={() => router.push("/products")}
          />
        </Card>
      )}
    </>
  );
}

function toFormValues(product: Product): ProductFormValues {
  return {
    name: product.name,
    description: product.description ?? "",
    sku: product.sku,
    barcode: product.barcode ?? "",
    categoryId: product.categoryId,
    brandId: product.brandId,
    unitId: product.unitId,
    supplierId: product.supplierId ?? "",
    costPrice: String(product.costPrice),
    sellingPrice: String(product.sellingPrice),
    discountPercent: product.discountPercent != null ? String(product.discountPercent) : "",
    taxPercent: product.taxPercent != null ? String(product.taxPercent) : "",
    reorderLevel: String(product.reorderLevel),
    maxStockLevel: String(product.maxStockLevel),
    trackInventory: product.trackInventory,
    isActive: product.isActive,
  };
}
