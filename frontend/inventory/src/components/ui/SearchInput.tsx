"use client";

import { InputHTMLAttributes } from "react";

export function SearchInput(props: InputHTMLAttributes<HTMLInputElement>) {
  return (
    <div className="search-input-wrap">
      <span className="search-input-icon" aria-hidden="true">
        ⌕
      </span>
      <input type="search" className="input search-input" {...props} />
    </div>
  );
}
