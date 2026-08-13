import { useEffect, useMemo, useState } from "react";
import { Badge } from "../../movies/component/ui/badge";
import { type MoviesAdminLookups, type MoviesAdminRevenueReport, moviesAdminApi } from "../services/moviesAdminApi";

function formatCurrency(value: number) {
  return new Intl.NumberFormat("vi-VN", { style: "currency", currency: "VND", maximumFractionDigits: 0 }).format(value);
}

export function MoviesAdminRevenuePage() {
  const [report, setReport] = useState<MoviesAdminRevenueReport | null>(null);
  const [lookups, setLookups] = useState<MoviesAdminLookups>({ movies: [], cinemas: [], halls: [] });
  const [filters, setFilters] = useState({
    dateFromUtc: "",
    dateToUtc: "",
    movieId: "",
    cinemaId: "",
    provider: "",
    paymentStatus: "",
  });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const statCards = useMemo(() => [
    { label: "Paid revenue", value: formatCurrency(report?.totalPaidRevenue ?? 0) },
    { label: "Bookings", value: String(report?.totalBookings ?? 0) },
    { label: "Successful payments", value: String(report?.successfulPayments ?? 0) },
    { label: "Failed payments", value: String(report?.failedPayments ?? 0) },
  ], [report]);

  async function loadReport() {
    try {
      setLoading(true);
      setError("");
      const [reportResponse, lookupResponse] = await Promise.all([
        moviesAdminApi.getRevenueReport({
          dateFromUtc: filters.dateFromUtc || undefined,
          dateToUtc: filters.dateToUtc || undefined,
          movieId: filters.movieId || undefined,
          cinemaId: filters.cinemaId || undefined,
          provider: filters.provider || undefined,
          paymentStatus: filters.paymentStatus || undefined,
        }),
        moviesAdminApi.getLookups(),
      ]);
      setReport(reportResponse);
      setLookups(lookupResponse);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Unable to load revenue report.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void loadReport();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return (
    <div className="space-y-6">
      <section className="rounded-[2rem] border border-white/8 bg-[linear-gradient(180deg,rgba(255,255,255,0.045),rgba(255,255,255,0.02))] p-6 shadow-[0_24px_80px_rgba(2,6,23,0.28)]">
        <div className="flex items-start justify-between gap-4">
          <div>
            <p className="text-xs font-semibold uppercase tracking-[0.28em] text-cyan-200/70">Revenue</p>
            <h1 className="mt-2 text-3xl font-black uppercase tracking-[0.08em] text-white">Detailed revenue report</h1>
          </div>
          <Badge className="border border-white/10 bg-white/[0.04] px-3 py-1.5 text-gray-200">
            {loading ? "Loading..." : "Filtered report"}
          </Badge>
        </div>
      </section>
      {error ? <div className="rounded-3xl border border-red-500/20 bg-red-500/10 px-4 py-3 text-sm text-red-200">{error}</div> : null}
      <section className="grid gap-4 lg:grid-cols-6">
        <input type="datetime-local" value={filters.dateFromUtc} onChange={(e) => setFilters((c) => ({ ...c, dateFromUtc: e.target.value }))} className="rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white" />
        <input type="datetime-local" value={filters.dateToUtc} onChange={(e) => setFilters((c) => ({ ...c, dateToUtc: e.target.value }))} className="rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white" />
        <select value={filters.movieId} onChange={(e) => setFilters((c) => ({ ...c, movieId: e.target.value }))} className="rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white">
          <option value="">All movies</option>
          {lookups.movies.map((movie) => <option key={movie.id} value={movie.id}>{movie.name}</option>)}
        </select>
        <select value={filters.cinemaId} onChange={(e) => setFilters((c) => ({ ...c, cinemaId: e.target.value }))} className="rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white">
          <option value="">All cinemas</option>
          {lookups.cinemas.map((cinema) => <option key={cinema.id} value={cinema.id}>{cinema.name}</option>)}
        </select>
        <select value={filters.provider} onChange={(e) => setFilters((c) => ({ ...c, provider: e.target.value }))} className="rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white">
          <option value="">All providers</option>
          <option value="Stripe">Stripe</option>
          <option value="PayPal">PayPal</option>
          <option value="VnPay">VnPay</option>
          <option value="Momo">Momo</option>
        </select>
        <div className="flex gap-3">
          <select value={filters.paymentStatus} onChange={(e) => setFilters((c) => ({ ...c, paymentStatus: e.target.value }))} className="flex-1 rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white">
            <option value="">All statuses</option>
            <option value="Succeeded">Succeeded</option>
            <option value="Pending">Pending</option>
            <option value="Failed">Failed</option>
            <option value="Cancelled">Cancelled</option>
          </select>
          <button onClick={() => void loadReport()} className="rounded-2xl border border-white/12 bg-white/[0.06] px-4 py-3 text-sm font-semibold text-white transition hover:bg-white/[0.12]">Apply</button>
        </div>
      </section>
      <section className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        {statCards.map((stat) => (
          <div key={stat.label} className="rounded-3xl border border-white/8 bg-white/[0.03] p-4 shadow-[0_24px_80px_rgba(2,6,23,0.34)]">
            <p className="text-sm text-gray-400">{stat.label}</p>
            <p className="mt-3 text-3xl font-black tracking-tight text-white">{stat.value}</p>
          </div>
        ))}
      </section>
      <section className="grid gap-6 xl:grid-cols-3">
        {[
          { title: "By movie", rows: report?.byMovie ?? [] },
          { title: "By cinema", rows: report?.byCinema ?? [] },
          { title: "By provider", rows: report?.byProvider ?? [] },
        ].map((block) => (
          <div key={block.title} className="rounded-[2rem] border border-white/8 bg-white/[0.03] p-6 shadow-[0_24px_80px_rgba(2,6,23,0.34)]">
            <p className="text-xs font-semibold uppercase tracking-[0.28em] text-fuchsia-200/70">{block.title}</p>
            <div className="mt-4 space-y-3">
              {block.rows.map((row) => (
                <div key={row.label} className="rounded-2xl border border-white/8 bg-white/[0.02] px-4 py-3">
                  <div className="flex items-center justify-between gap-3">
                    <div>
                      <p className="font-semibold text-white">{row.label}</p>
                      <p className="text-sm text-gray-400">{row.bookingCount} bookings</p>
                    </div>
                    <p className="font-semibold text-cyan-200">{formatCurrency(row.revenue)}</p>
                  </div>
                </div>
              ))}
              {!loading && block.rows.length === 0 ? <p className="text-sm text-gray-400">No data for this filter.</p> : null}
            </div>
          </div>
        ))}
      </section>
    </div>
  );
}
