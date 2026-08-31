"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { Button, Card, Field, Input, PageHeader, Select } from "@/components/ui";
import { useAppDispatch, useAppSelector } from "@/lib/store/hooks";
import { configSaved, ReceiptPaperWidth } from "@/features/session/slice";
import {
  cashSessionCloseRequested,
  cashSessionCloseReset,
  cashSessionOpenRequested,
} from "@/features/session/slice";

export default function SetupPage() {
  const dispatch = useAppDispatch();
  const router = useRouter();
  const { config, openSession, openStatus, openError, closeStatus, closeError } = useAppSelector((s) => s.session);
  const user = useAppSelector((s) => s.auth.user);
  // The cashier is now the signed-in user — no more pasting a cashier GUID (see AI-HANDOVER.md,
  // auth-service integration). Store/register still need manual GUIDs: no Store/Register CRUD
  // exists yet (docs/API-GAPS.md), and that's a real backend gap, not something this page can fix.
  const cashierId = user?.id ?? "";

  const [storeId, setStoreId] = useState("");
  const [registerId, setRegisterId] = useState("");
  const [receiptPaperWidthMm, setReceiptPaperWidthMm] = useState<ReceiptPaperWidth>(80);
  const [openingBalance, setOpeningBalance] = useState("0");
  const [closingBalance, setClosingBalance] = useState("0");
  const [closeNotes, setCloseNotes] = useState("");

  useEffect(() => {
    if (config) {
      setStoreId(config.storeId);
      setRegisterId(config.registerId);
      setReceiptPaperWidthMm(config.receiptPaperWidthMm);
    }
  }, [config]);

  useEffect(() => {
    if (closeStatus === "succeeded") {
      dispatch(cashSessionCloseReset());
    }
  }, [closeStatus, dispatch]);

  function saveConfig(e: React.FormEvent) {
    e.preventDefault();
    dispatch(configSaved({ storeId, registerId, cashierId, receiptPaperWidthMm }));
  }

  function openSessionHandler(e: React.FormEvent) {
    e.preventDefault();
    dispatch(cashSessionOpenRequested({ registerId, cashierId, openingBalance: Number(openingBalance) }));
  }

  function closeSessionHandler(e: React.FormEvent) {
    e.preventDefault();
    if (!openSession) return;
    dispatch(
      cashSessionCloseRequested({
        sessionId: openSession.id,
        closingBalance: Number(closingBalance),
        expectedBalance: Number(closingBalance),
        notes: closeNotes || null,
      })
    );
  }

  return (
    <>
      <PageHeader title="Terminal setup" subtitle="Configure this terminal's store, register and receipt printer." />

      <div
        className="demo-banner"
        role="note"
      >
        STORE/REGISTER MANAGEMENT NOT YET AVAILABLE — paste an existing store/register GUID below.
        The cashier is now your signed-in account ({user?.email ?? "…"}) — no GUID needed. See docs/API-GAPS.md.
      </div>

      <Card style={{ marginBottom: 16 }}>
        <h2 style={{ marginTop: 0, fontSize: 15 }}>1. Terminal identity</h2>
        <form onSubmit={saveConfig}>
          <div className="form-grid">
            <Field label="Store ID" htmlFor="storeId" required hint="Store management isn't available yet — paste an existing store GUID.">
              <Input id="storeId" value={storeId} onChange={(e) => setStoreId(e.target.value)} required />
            </Field>
            <Field label="Register ID" htmlFor="registerId" required hint="Register management isn't available yet — paste an existing register GUID.">
              <Input id="registerId" value={registerId} onChange={(e) => setRegisterId(e.target.value)} required />
            </Field>
            <Field label="Cashier" htmlFor="cashierId" hint="Derived from your signed-in account.">
              <Input id="cashierId" value={user?.email ?? ""} disabled />
            </Field>
            <Field label="Receipt printer paper width" htmlFor="receiptPaperWidthMm" hint="Thermal printer width — controls the receipt print layout.">
              <Select
                id="receiptPaperWidthMm"
                value={receiptPaperWidthMm}
                onChange={(e) => setReceiptPaperWidthMm(Number(e.target.value) as ReceiptPaperWidth)}
              >
                <option value={58}>58mm</option>
                <option value={80}>80mm</option>
              </Select>
            </Field>
          </div>
          <div className="form-actions">
            <Button type="submit">Save terminal identity</Button>
          </div>
        </form>
      </Card>

      {config && !openSession && (
        <Card style={{ marginBottom: 16 }}>
          <h2 style={{ marginTop: 0, fontSize: 15 }}>2. Open cash session</h2>
          {openStatus === "failed" && (
            <div role="alert" style={{ background: "var(--color-danger-soft)", color: "var(--color-danger)", padding: "10px 14px", borderRadius: 6, marginBottom: 16, fontSize: 13.5 }}>
              {openError}
            </div>
          )}
          <form onSubmit={openSessionHandler}>
            <div className="form-grid">
              <Field label="Opening cash amount" htmlFor="openingBalance" required>
                <Input id="openingBalance" type="number" min={0} step="0.01" value={openingBalance} onChange={(e) => setOpeningBalance(e.target.value)} required />
              </Field>
            </div>
            <div className="form-actions">
              <Button type="submit" loading={openStatus === "opening"}>
                Open session
              </Button>
            </div>
          </form>
        </Card>
      )}

      {config && openSession && (
        <>
          <Card style={{ marginBottom: 16, background: "var(--color-success-soft)" }}>
            <strong style={{ fontSize: 13.5 }}>Session open</strong>
            <p style={{ margin: "6px 0 0", fontSize: 13, color: "var(--color-text-muted)" }}>
              Opened with {openSession.openingBalance.toFixed(2)} at {new Date(openSession.openedAt).toLocaleTimeString()}.
            </p>
            <div style={{ marginTop: 12 }}>
              <Button size="sm" onClick={() => router.push("/")}>
                Go to POS terminal
              </Button>
            </div>
          </Card>

          <Card>
            <h2 style={{ marginTop: 0, fontSize: 15 }}>Close cash session</h2>
            {closeStatus === "failed" && (
              <div role="alert" style={{ background: "var(--color-danger-soft)", color: "var(--color-danger)", padding: "10px 14px", borderRadius: 6, marginBottom: 16, fontSize: 13.5 }}>
                {closeError}
              </div>
            )}
            <form onSubmit={closeSessionHandler}>
              <div className="form-grid">
                <Field label="Actual cash counted" htmlFor="closingBalance" required hint="The backend records this as both closing and expected balance (no separate expected-cash calculation is exposed yet).">
                  <Input id="closingBalance" type="number" min={0} step="0.01" value={closingBalance} onChange={(e) => setClosingBalance(e.target.value)} required />
                </Field>
                <div className="form-grid-full">
                  <Field label="Notes" htmlFor="closeNotes">
                    <Input id="closeNotes" value={closeNotes} onChange={(e) => setCloseNotes(e.target.value)} />
                  </Field>
                </div>
              </div>
              <div className="form-actions">
                <Button type="submit" variant="danger" loading={closeStatus === "closing"}>
                  Close session
                </Button>
              </div>
            </form>
          </Card>
        </>
      )}
    </>
  );
}
