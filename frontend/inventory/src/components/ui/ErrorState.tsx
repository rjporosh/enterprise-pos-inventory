import { Button } from "./Button";

export function ErrorState({ message, onRetry }: { message: string; onRetry?: () => void }) {
  return (
    <div className="state-block">
      <div style={{ fontSize: 28, color: "var(--color-danger)" }} aria-hidden="true">
        !
      </div>
      <div className="state-block-title">Something went wrong</div>
      <div className="state-block-desc">{message}</div>
      {onRetry && (
        <Button variant="secondary" size="sm" onClick={onRetry}>
          Try again
        </Button>
      )}
    </div>
  );
}
