import { describe, expect, it } from "vitest";
import {
  stockReducer,
  stockInRequested,
  stockMovementSucceeded,
  stockMovementFailed,
  stockMovementReset,
  stockListRequested,
  stockListLoaded,
} from "../slice";
import { StockMovement } from "@/lib/api/stock";

describe("stock slice", () => {
  it("marks the movement as saving with the right kind when stock-in is requested", () => {
    const state = stockReducer(
      undefined,
      stockInRequested({ productId: "p1", warehouseId: "w1", quantity: 5 })
    );
    expect(state.movement.status).toBe("saving");
    expect(state.movement.kind).toBe("in");
  });

  it("records the movement result on success", () => {
    let state = stockReducer(undefined, stockInRequested({ productId: "p1", warehouseId: "w1", quantity: 5 }));
    const movement = { id: "m1", balanceAfter: 15 } as StockMovement;
    state = stockReducer(state, stockMovementSucceeded(movement));
    expect(state.movement.status).toBe("succeeded");
    expect(state.movement.lastMovement).toEqual(movement);
  });

  it("records the error message on failure", () => {
    let state = stockReducer(undefined, stockInRequested({ productId: "p1", warehouseId: "w1", quantity: 5 }));
    state = stockReducer(state, stockMovementFailed("Insufficient stock"));
    expect(state.movement.status).toBe("failed");
    expect(state.movement.error).toBe("Insufficient stock");
  });

  it("resets movement state", () => {
    let state = stockReducer(undefined, stockInRequested({ productId: "p1", warehouseId: "w1", quantity: 5 }));
    state = stockReducer(state, stockMovementReset());
    expect(state.movement.status).toBe("idle");
    expect(state.movement.kind).toBeNull();
  });

  it("tracks list loading and params", () => {
    let state = stockReducer(undefined, stockListRequested({ pageNumber: 2, lowStock: true }));
    expect(state.list.status).toBe("loading");
    expect(state.list.params).toEqual({ pageNumber: 2, lowStock: true });

    const result = { items: [], totalCount: 0, pageNumber: 2, pageSize: 20 };
    state = stockReducer(state, stockListLoaded(result));
    expect(state.list.status).toBe("succeeded");
    expect(state.list.result).toEqual(result);
  });
});
