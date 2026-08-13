import { useEffect, useState } from "react";
import { Badge } from "../../movies/component/ui/badge";
import { AdminDateInput, parseDisplayDateToIsoBoundary } from "../components/AdminDateInput";
import { type MoviesAdminPayment, type MoviesAdminPaymentDetail, moviesAdminApi } from "../services/moviesAdminApi";

function formatCurrency(value: number, currency: string) {
  return new Intl.NumberFormat("vi-VN", { style: "currency", currency: currency || "VND", maximumFractionDigits: 0 }).format(value);
}

export function MoviesAdminPaymentsPage() {
  const [payments, setPayments] = useState<MoviesAdminPayment[]>([]);
  const [detail, setDetail] = useState<MoviesAdminPaymentDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [filters, setFilters] = useState({
    status: "",
    provider: "",
    query: "",
    dateFromUtc: "",
    dateToUtc: "",
  });

  async function loadPayments() {
    try {
      setLoading(true);
      setError("");
      setPayments(await moviesAdminApi.getPayments({
        status: filters.status || undefined,
        provider: filters.provider || undefined,
        query: filters.query || undefined,
        dateFromUtc: parseDisplayDateToIsoBoundary(filters.dateFromUtc, "start"),
        dateToUtc: parseDisplayDateToIsoBoundary(filters.dateToUtc, "end"),
      }));
    } catch (err) {
      setError(err instanceof Error ? err.message : "Unable to load payments.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void loadPayments();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  async function openDetail(paymentId: string) {
    try {
      setError("");
      setDetail(await moviesAdminApi.getPaymentById(paymentId));
    } catch (err) {
      setError(err instanceof Error ? err.message : "Unable to load payment detail.");
    }
  }

  return (
    <div className="space-y-6">
      <section className="rounded-[2rem] border border-white/8 bg-[linear-gradient(180deg,rgba(255,255,255,0.045),rgba(255,255,255,0.02))] p-6 shadow-[0_24px_80px_rgba(2,6,23,0.28)]">
        <div className="flex items-start justify-between gap-4">
          <div>
            <p className="text-xs font-semibold uppercase tracking-[0.28em] text-cyan-200/70">Payments</p>
            <h1 className="mt-2 text-3xl font-black uppercase tracking-[0.08em] text-white">Payment monitor</h1>
          </div>
          <Badge className="border border-white/10 bg-white/[0.04] px-3 py-1.5 text-gray-200">{payments.length} payments</Badge>
        </div>
      </section>
      {error ? <div className="rounded-3xl border border-red-500/20 bg-red-500/10 px-4 py-3 text-sm text-red-200">{error}</div> : null}
      <section className="flex flex-wrap gap-4">
        <input
          value={filters.query}
          onChange={(event) => setFilters((current) => ({ ...current, query: event.target.value }))}
          placeholder="Search booking, email, transaction..."
          className="min-w-[220px] flex-1 rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white"
        />
        <select
          value={filters.status}
          onChange={(event) => setFilters((current) => ({ ...current, status: event.target.value }))}
          className="min-w-[220px] flex-1 rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white"
        >
          <option value="">All payment statuses</option>
          <option value="Pending">Pending</option>
          <option value="Processing">Processing</option>
          <option value="Succeeded">Succeeded</option>
          <option value="Failed">Failed</option>
          <option value="Cancelled">Cancelled</option>
          <option value="Refunded">Refunded</option>
        </select>
        <select
          value={filters.provider}
          onChange={(event) => setFilters((current) => ({ ...current, provider: event.target.value }))}
          className="min-w-[220px] flex-1 rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white"
        >
          <option value="">All providers</option>
          <option value="Mock">Mock</option>
          <option value="Momo">Momo</option>
          <option value="VnPay">VnPay</option>
          <option value="Stripe">Stripe</option>
          <option value="PayPal">PayPal</option>
        </select>
        <AdminDateInput
          value={filters.dateFromUtc}
          onChange={(value) => setFilters((current) => ({ ...current, dateFromUtc: value }))}
          className="min-w-[220px] flex-1"
        />
        <div className="flex min-w-[280px] flex-1 gap-3">
          <AdminDateInput
            value={filters.dateToUtc}
            onChange={(value) => setFilters((current) => ({ ...current, dateToUtc: value }))}
            className="flex-1"
          />
          <button onClick={() => void loadPayments()} className="shrink-0 rounded-2xl border border-white/12 bg-white/[0.06] px-5 py-3 text-sm font-semibold text-white transition hover:bg-white/[0.12]">Apply</button>
        </div>
      </section>
      <section className="grid gap-6 xl:grid-cols-[minmax(0,1.2fr)_360px]">
        <div className="overflow-hidden rounded-[2rem] border border-white/8 bg-white/[0.03] shadow-[0_24px_80px_rgba(2,6,23,0.34)]">
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-white/8 text-left text-sm">
              <thead className="bg-white/[0.04]"><tr>{["Booking", "Provider", "Status", "Amount", "Customer", "Actions"].map((c)=><th key={c} className="px-4 py-3 text-xs font-semibold uppercase tracking-[0.18em] text-gray-500">{c}</th>)}</tr></thead>
              <tbody className="divide-y divide-white/8">
                {loading ? <tr><td colSpan={6} className="px-4 py-6 text-gray-400">Loading payments...</td></tr> : null}
                {!loading && payments.map((payment) => (
                  <tr key={payment.id} className="bg-white/[0.02]">
                    <td className="px-4 py-3 text-white">{payment.bookingCode}</td>
                    <td className="px-4 py-3 text-gray-300">{payment.provider}</td>
                    <td className="px-4 py-3 text-gray-300">{payment.status}</td>
                    <td className="px-4 py-3 text-gray-300">{formatCurrency(payment.amount, payment.currency)}</td>
                    <td className="px-4 py-3 text-gray-300">{payment.customerEmail}</td>
                    <td className="px-4 py-3"><button className="text-cyan-200" onClick={() => void openDetail(payment.id)}>View</button></td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
        <div className="rounded-[2rem] border border-white/8 bg-white/[0.03] p-6 shadow-[0_24px_80px_rgba(2,6,23,0.34)]">
          <p className="text-xs font-semibold uppercase tracking-[0.28em] text-fuchsia-200/70">Payment detail</p>
          {!detail ? <p className="mt-4 text-sm text-gray-400">Select a payment to inspect payload and failure reason.</p> : (
            <div className="mt-4 space-y-3 text-sm text-gray-300">
              <div className="rounded-2xl border border-white/8 bg-white/[0.02] p-4">
                <p className="font-semibold text-white">{detail.bookingCode}</p>
                <p>{detail.movieTitle}</p>
                <p>{detail.provider} • {detail.status}</p>
                <p>{formatCurrency(detail.amount, detail.currency)}</p>
              </div>
              <div className="rounded-2xl border border-white/8 bg-white/[0.02] p-4">
                <p className="font-semibold text-white">Failure reason</p>
                <p>{detail.failureReason ?? "-"}</p>
              </div>
              <div className="rounded-2xl border border-white/8 bg-white/[0.02] p-4">
                <p className="font-semibold text-white">Callback payload</p>
                <pre className="mt-2 overflow-x-auto whitespace-pre-wrap text-xs text-gray-400">{detail.callbackPayloadJson ?? "-"}</pre>
              </div>
            </div>
          )}
        </div>
      </section>
    </div>
  );
}
