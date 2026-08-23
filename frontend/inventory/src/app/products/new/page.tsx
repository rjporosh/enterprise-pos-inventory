"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { PageHeader, Card } from "@/components/ui";
import { ProductForm } from "@/features/products/components/ProductForm";
import { emptyProductForm, toCreateProductInput } from "@/features/products/validation";
import { productCreateReset, productCreateRequested } from "@/features/products/slice";
import { useAppDispatch, useAppSelector } from "@/lib/store/hooks";

export default function NewProductPage() {
  const dispatch = useAppDispatch();
  const router = useRouter();
  const createState = useAppSelector((s) => s.products.create);

  useEffect(() => {
    dispatch(productCreateReset());
  }, [dispatch]);

  useEffect(() => {
    if (createState.status === "succeeded") {
      router.push("/products");
    }
  }, [createState.status, router]);

  return (
    <>
      <PageHeader title="New product" subtitle="Add a product to the catalog." />
      <Card>
        <ProductForm
          initialValues={emptyProductForm}
          submitLabel="Create product"
          saving={createState.status === "saving"}
          serverError={createState.status === "failed" ? createState.error : null}
          onSubmit={(values) => dispatch(productCreateRequested(toCreateProductInput(values)))}
          onCancel={() => router.push("/products")}
        />
      </Card>
    </>
  );
}
