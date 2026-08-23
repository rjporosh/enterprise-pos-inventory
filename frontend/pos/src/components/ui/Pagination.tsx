"use client";

import { Button } from "./Button";

interface PaginationProps {
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  onPageChange: (page: number) => void;
}

export function Pagination({ pageNumber, pageSize, totalCount, onPageChange }: PaginationProps) {
  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));
  const start = totalCount === 0 ? 0 : (pageNumber - 1) * pageSize + 1;
  const end = Math.min(totalCount, pageNumber * pageSize);

  return (
    <div className="pagination">
      <span>
        {totalCount === 0 ? "No results" : `Showing ${start}–${end} of ${totalCount}`}
      </span>
      <div className="pagination-controls">
        <Button variant="secondary" size="sm" disabled={pageNumber <= 1} onClick={() => onPageChange(pageNumber - 1)}>
          Previous
        </Button>
        <Button variant="secondary" size="sm" disabled={pageNumber >= totalPages} onClick={() => onPageChange(pageNumber + 1)}>
          Next
        </Button>
      </div>
    </div>
  );
}
