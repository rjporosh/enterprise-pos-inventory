"use client";

import { forwardRef, InputHTMLAttributes } from "react";

export const SearchInput = forwardRef<HTMLInputElement, InputHTMLAttributes<HTMLInputElement>>((props, ref) => {
  return (
    <div className="search-input-wrap">
      <span className="search-input-icon" aria-hidden="true">
        ⌕
      </span>
      <input ref={ref} type="search" className="input search-input" {...props} />
    </div>
  );
});
SearchInput.displayName = "SearchInput";
