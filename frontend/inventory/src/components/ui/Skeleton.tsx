export function Skeleton({ height = 14, width = "100%" }: { height?: number; width?: number | string }) {
  return <div className="skeleton" style={{ height, width }} />;
}

export function TableSkeleton({ rows = 6, cols = 5 }: { rows?: number; cols?: number }) {
  return (
    <div style={{ padding: 16 }}>
      {Array.from({ length: rows }).map((_, r) => (
        <div key={r} style={{ display: "flex", gap: 16, marginBottom: 14 }}>
          {Array.from({ length: cols }).map((__, c) => (
            <Skeleton key={c} height={14} width={c === 0 ? "24%" : "14%"} />
          ))}
        </div>
      ))}
    </div>
  );
}
