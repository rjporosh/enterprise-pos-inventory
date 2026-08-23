import { createSlice, PayloadAction, nanoid } from "@reduxjs/toolkit";

export interface Toast {
  id: string;
  variant: "success" | "error" | "info";
  message: string;
}

interface ToastState {
  items: Toast[];
}

const initialState: ToastState = { items: [] };

const toastSlice = createSlice({
  name: "toast",
  initialState,
  reducers: {
    toastShown: {
      reducer(state, action: PayloadAction<Toast>) {
        state.items.push(action.payload);
      },
      prepare(variant: Toast["variant"], message: string) {
        return { payload: { id: nanoid(), variant, message } };
      },
    },
    toastDismissed(state, action: PayloadAction<string>) {
      state.items = state.items.filter((t) => t.id !== action.payload);
    },
  },
});

export const { toastShown, toastDismissed } = toastSlice.actions;
export const toastReducer = toastSlice.reducer;
