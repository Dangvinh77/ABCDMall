import { useEffect, useState } from "react";
import { Badge } from "../../movies/component/ui/badge";
import { AdminDateInput, parseDisplayDateToIsoBoundary } from "../components/AdminDateInput";
import {
  type MoviesAdminBooking,
  type MoviesAdminBookingDetail,
  moviesAdminApi,
} from "../services/moviesAdminApi";

function formatCurrency(value: number, currency: string) {
  return new Intl.NumberFormat("vi-VN", {
    style: "currency",
    currency: currency || "VND",
    maximumFractionDigits: 0,
  }).format(value);
}

export function MoviesAdminBookingsPage() {
  const [bookings, setBookings] = useState<MoviesAdminBooking[]>([]);
  const [selectedBooking, setSelectedBooking] = useState<MoviesAdminBookingDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [detailLoading, setDetailLoading] = useState(false);
  const [error, setError] = useState("");
  const [filters, setFilters] = useState({
    status: "",
    paymentStatus: "",
    query: "",
    dateFromUtc: "",
    dateToUtc: "",
  });

  async function loadBookings() {
    try {
      setLoading(true);
      setError("");
      setBookings(await moviesAdminApi.getBookings({
        status: filters.status || undefined,
        paymentStatus: filters.paymentStatus || undefined,
        query: filters.query || undefined,
        dateFromUtc: parseDisplayDateToIsoBoundary(filters.dateFromUtc, "start"),
        dateToUtc: parseDisplayDateToIsoBoundary(filters.dateToUtc, "end"),
      }));
    } catch (err) {
      setError(err instanceof Error ? err.message : "Unable to load bookings.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void loadBookings();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  async function openBooking(bookingId: string) {
    try {
      setDetailLoading(true);
      setError("");
      setSelectedBooking(await moviesAdminApi.getBookingById(bookingId));
    } catch (err) {
      setError(err instanceof Error ? err.message : "Unable to load booking detail.");
    } finally {
      setDetailLoading(false);
    }
  }

  return (
    <div className="space-y-6">
      <section className="rounded-[2rem] border border-white/8 bg-[linear-gradient(180deg,rgba(255,255,255,0.045),rgba(255,255,255,0.02))] p-6 shadow-[0_24px_80px_rgba(2,6,23,0.28)]">
        <div className="flex items-start justify-between gap-4">
          <div>
            <p className="text-xs font-semibold uppercase tracking-[0.28em] text-cyan-200/70">
              Bookings
            </p>
            <h1 className="mt-2 text-3xl font-black uppercase tracking-[0.08em] text-white">
              Orders and ticket history
            </h1>
          </div>
          <Badge className="border border-white/10 bg-white/[0.04] px-3 py-1.5 text-gray-200">
            {bookings.length} bookings
          </Badge>
        </div>
      </section>

      {error ? (
        <div className="rounded-3xl border border-red-500/20 bg-red-500/10 px-4 py-3 text-sm text-red-200">
          {error}
        </div>
      ) : null}

      <section className="flex flex-wrap gap-4">
        <input
          value={filters.query}
          onChange={(event) => setFilters((current) => ({ ...current, query: event.target.value }))}
          placeholder="Search booking, customer, email..."
          className="min-w-[220px] flex-1 rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white"
        />
        <select
          value={filters.status}
          onChange={(event) => setFilters((current) => ({ ...current, status: event.target.value }))}
          className="min-w-[220px] flex-1 rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white"
        >
          <option value="">All booking statuses</option>
          <option value="PendingPayment">PendingPayment</option>
          <option value="Confirmed">Confirmed</option>
          <option value="Cancelled">Cancelled</option>
          <option value="Expired">Expired</option>
          <option value="Refunded">Refunded</option>
        </select>
        <select
          value={filters.paymentStatus}
          onChange={(event) => setFilters((current) => ({ ...current, paymentStatus: event.target.value }))}
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
          <button
            onClick={() => void loadBookings()}
            className="shrink-0 rounded-2xl border border-white/12 bg-white/[0.06] px-5 py-3 text-sm font-semibold text-white transition hover:bg-white/[0.12]"
          >
            Apply
          </button>
        </div>
      </section>

      <section className="grid gap-6 xl:grid-cols-[minmax(0,1.2fr)_360px]">
        <div className="overflow-hidden rounded-[2rem] border border-white/8 bg-white/[0.03] shadow-[0_24px_80px_rgba(2,6,23,0.34)]">
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-white/8 text-left text-sm">
              <thead className="bg-white/[0.04]">
                <tr>
                  {["Code", "Customer", "Movie", "Showtime", "Total", "Payment", "Actions"].map((column) => (
                    <th key={column} className="px-4 py-3 text-xs font-semibold uppercase tracking-[0.18em] text-gray-500">
                      {column}
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody className="divide-y divide-white/8">
                {loading ? (
                  <tr>
                    <td colSpan={7} className="px-4 py-6 text-gray-400">Loading bookings...</td>
                  </tr>
                ) : null}
                {!loading && bookings.length === 0 ? (
                  <tr>
                    <td colSpan={7} className="px-4 py-6 text-gray-400">No bookings found.</td>
                  </tr>
                ) : null}
                {bookings.map((booking) => (
                  <tr key={booking.id} className="bg-white/[0.02]">
                    <td className="px-4 py-3 text-white">{booking.bookingCode}</td>
                    <td className="px-4 py-3 text-gray-300">
                      <div>{booking.customerName}</div>
                      <div className="text-xs text-gray-500">{booking.customerEmail}</div>
                    </td>
                    <td className="px-4 py-3 text-gray-300">{booking.movieTitle}</td>
                    <td className="px-4 py-3 text-gray-300">{new Date(booking.showtimeStartAtUtc).toLocaleString("vi-VN")}</td>
                    <td className="px-4 py-3 text-gray-300">{formatCurrency(booking.grandTotal, booking.currency)}</td>
                    <td className="px-4 py-3 text-gray-300">{booking.paymentStatus}</td>
                    <td className="px-4 py-3">
                      <button className="text-cyan-200" onClick={() => void openBooking(booking.id)}>
                        View
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>

        <div className="rounded-[2rem] border border-white/8 bg-white/[0.03] p-6 shadow-[0_24px_80px_rgba(2,6,23,0.34)]">
          <p className="text-xs font-semibold uppercase tracking-[0.28em] text-fuchsia-200/70">
            Booking detail
          </p>
          {detailLoading ? <p className="mt-4 text-sm text-gray-400">Loading detail...</p> : null}
          {!detailLoading && !selectedBooking ? (
            <p className="mt-4 text-sm text-gray-400">Select a booking to inspect payment and ticket lines.</p>
          ) : null}
          {selectedBooking ? (
            <div className="mt-4 space-y-4">
              <div className="rounded-2xl border border-white/8 bg-white/[0.02] p-4">
                <p className="font-semibold text-white">{selectedBooking.bookingCode}</p>
                <p className="mt-1 text-sm text-gray-400">
                  {selectedBooking.movieTitle} • {selectedBooking.cinemaName} • {selectedBooking.hallName}
                </p>
                <p className="mt-1 text-sm text-gray-400">{new Date(selectedBooking.startAtUtc).toLocaleString("vi-VN")}</p>
              </div>
              <div className="rounded-2xl border border-white/8 bg-white/[0.02] p-4 text-sm text-gray-300">
                <p>{selectedBooking.customerName}</p>
                <p>{selectedBooking.customerEmail}</p>
                <p>{selectedBooking.customerPhoneNumber}</p>
                <p className="mt-2 text-cyan-200">Payment: {selectedBooking.paymentStatus}</p>
                <p>Status: {selectedBooking.status}</p>
                <p>Total: {formatCurrency(selectedBooking.grandTotal, selectedBooking.currency)}</p>
              </div>
              <div className="space-y-2">
                {selectedBooking.items.map((item) => (
                  <div key={`${item.itemType}-${item.itemCode}`} className="rounded-2xl border border-white/8 bg-white/[0.02] px-4 py-3 text-sm text-gray-300">
                    <p className="font-semibold text-white">{item.description}</p>
                    <p>
                      {item.itemType} • Qty {item.quantity} • {formatCurrency(item.lineTotal, selectedBooking.currency)}
                    </p>
                  </div>
                ))}
              </div>
            </div>
          ) : null}
        </div>
      </section>
    </div>
  );
}
