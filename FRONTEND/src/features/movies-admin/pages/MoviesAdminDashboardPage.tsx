import { useEffect, useState } from "react";
import { Badge } from "../../movies/component/ui/badge";
import {
  type MoviesAdminDashboardResponse,
  moviesAdminApi,
} from "../services/moviesAdminApi";

function formatCurrency(value: number) {
  return new Intl.NumberFormat("vi-VN", {
    style: "currency",
    currency: "VND",
    maximumFractionDigits: 0,
  }).format(value);
}

function formatDateTime(value: string) {
  return new Date(value).toLocaleString("vi-VN");
}

export function MoviesAdminDashboardPage() {
  const [data, setData] = useState<MoviesAdminDashboardResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    let active = true;

    void (async () => {
      try {
        setLoading(true);
        setError("");
        const response = await moviesAdminApi.getDashboard();
        if (active) {
          setData(response);
        }
      } catch (err) {
        if (active) {
          setError(err instanceof Error ? err.message : "Unable to load dashboard.");
        }
      } finally {
        if (active) {
          setLoading(false);
        }
      }
    })();

    return () => {
      active = false;
    };
  }, []);

  const stats = [
    { label: "Active movies", value: data?.activeMovies ?? 0, hint: "Now showing + coming soon" },
    { label: "Upcoming showtimes", value: data?.upcomingShowtimes ?? 0, hint: "Open sessions ahead" },
    { label: "Total bookings", value: data?.totalBookings ?? 0, hint: "All recorded orders" },
    { label: "Paid revenue", value: data ? formatCurrency(data.paidRevenue) : formatCurrency(0), hint: "Succeeded payments only" },
  ];

  return (
    <div className="space-y-6">
      <section className="rounded-[2rem] border border-white/8 bg-[radial-gradient(circle_at_top_left,rgba(168,85,247,0.16),transparent_30%),linear-gradient(180deg,rgba(255,255,255,0.05),rgba(255,255,255,0.02))] p-6 shadow-[0_24px_80px_rgba(2,6,23,0.34)]">
        <div className="flex flex-wrap items-start justify-between gap-4">
          <div>
            <p className="text-xs font-semibold uppercase tracking-[0.28em] text-fuchsia-200/70">
              Dashboard
            </p>
            <h1 className="mt-2 text-3xl font-black uppercase tracking-[0.08em] text-white">
              Movies admin control room
            </h1>
            <p className="mt-3 max-w-2xl text-sm leading-7 text-gray-400">
              Live summary for movie operations, bookings and revenue.
            </p>
          </div>
          <Badge className="border border-cyan-400/20 bg-cyan-500/10 px-3 py-1.5 text-cyan-200">
            {loading ? "Syncing..." : "Live data"}
          </Badge>
        </div>

        {error ? (
          <div className="mt-6 rounded-3xl border border-red-500/20 bg-red-500/10 px-4 py-3 text-sm text-red-200">
            {error}
          </div>
        ) : null}

        <div className="mt-6 grid gap-4 md:grid-cols-2 xl:grid-cols-4">
          {stats.map((stat) => (
            <div
              key={stat.label}
              className="rounded-3xl border border-white/8 bg-[linear-gradient(180deg,rgba(255,255,255,0.05),rgba(255,255,255,0.025))] p-4"
            >
              <p className="text-sm text-gray-400">{stat.label}</p>
              <p className="mt-3 text-3xl font-black tracking-tight text-white">
                {loading ? "..." : stat.value}
              </p>
              <p className="mt-2 text-xs text-gray-500">{stat.hint}</p>
            </div>
          ))}
        </div>
      </section>

      <section className="grid gap-6 xl:grid-cols-2">
        <div className="rounded-[2rem] border border-white/8 bg-white/[0.03] p-6 shadow-[0_24px_80px_rgba(2,6,23,0.34)]">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-xs font-semibold uppercase tracking-[0.28em] text-cyan-200/70">
                Upcoming showtimes
              </p>
              <h2 className="mt-2 text-xl font-black uppercase tracking-[0.08em] text-white">
                Next sessions
              </h2>
            </div>
            <Badge className="border border-white/10 bg-white/[0.04] text-gray-200">
              {data?.upcomingShowtimesSnapshot.length ?? 0} rows
            </Badge>
          </div>

          <div className="mt-6 space-y-3">
            {loading ? <p className="text-sm text-gray-400">Loading showtimes...</p> : null}
            {!loading && (data?.upcomingShowtimesSnapshot.length ?? 0) === 0 ? (
              <p className="text-sm text-gray-400">No upcoming showtimes found.</p>
            ) : null}
            {data?.upcomingShowtimesSnapshot.map((item) => (
              <div
                key={item.showtimeId}
                className="rounded-3xl border border-white/8 bg-white/[0.02] px-4 py-4"
              >
                <div className="flex items-start justify-between gap-4">
                  <div>
                    <p className="font-semibold text-white">{item.movieTitle}</p>
                    <p className="mt-1 text-sm text-gray-400">
                      {item.cinemaName} • {item.hallName}
                    </p>
                    <p className="mt-1 text-xs text-gray-500">
                      {item.businessDate} • {formatDateTime(item.startAtUtc)}
                    </p>
                  </div>
                  <Badge className="border border-emerald-400/20 bg-emerald-500/10 text-emerald-200">
                    {item.status}
                  </Badge>
                </div>
              </div>
            ))}
          </div>
        </div>

        <div className="space-y-6">
          <div className="rounded-[2rem] border border-white/8 bg-white/[0.03] p-6 shadow-[0_24px_80px_rgba(2,6,23,0.34)]">
            <p className="text-xs font-semibold uppercase tracking-[0.28em] text-fuchsia-200/70">
              Recent bookings
            </p>
            <div className="mt-4 space-y-3">
              {loading ? <p className="text-sm text-gray-400">Loading bookings...</p> : null}
              {data?.recentBookings.map((booking) => (
                <div
                  key={booking.bookingId}
                  className="rounded-2xl border border-white/8 bg-white/[0.02] px-4 py-3"
                >
                  <div className="flex items-center justify-between gap-3">
                    <div>
                      <p className="font-semibold text-white">{booking.bookingCode}</p>
                      <p className="text-sm text-gray-400">
                        {booking.customerName} • {booking.movieTitle}
                      </p>
                    </div>
                    <div className="text-right">
                      <p className="font-semibold text-cyan-200">{formatCurrency(booking.grandTotal)}</p>
                      <p className="text-xs text-gray-500">{booking.paymentStatus}</p>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>

          <div className="rounded-[2rem] border border-white/8 bg-white/[0.03] p-6 shadow-[0_24px_80px_rgba(2,6,23,0.34)]">
            <p className="text-xs font-semibold uppercase tracking-[0.28em] text-cyan-200/70">
              Top movies
            </p>
            <div className="mt-4 space-y-3">
              {loading ? <p className="text-sm text-gray-400">Loading ranking...</p> : null}
              {data?.topMovies.map((movie, index) => (
                <div
                  key={movie.movieId}
                  className="flex items-center justify-between rounded-2xl border border-white/8 bg-white/[0.02] px-4 py-3"
                >
                  <div>
                    <p className="font-semibold text-white">
                      {index + 1}. {movie.movieTitle}
                    </p>
                    <p className="text-sm text-gray-400">{movie.bookedSeats} booked seats</p>
                  </div>
                  <p className="font-semibold text-emerald-300">{formatCurrency(movie.revenue)}</p>
                </div>
              ))}
            </div>
          </div>
        </div>
      </section>
    </div>
  );
}
