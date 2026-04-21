import { useEffect, useMemo, useState } from "react";
import { Badge } from "../../movies/component/ui/badge";
import {
  type MoviesAdminLookups,
  type MoviesAdminShowtime,
  type MoviesAdminShowtimeUpsertRequest,
  moviesAdminApi,
} from "../services/moviesAdminApi";

const emptyForm: MoviesAdminShowtimeUpsertRequest = {
  movieId: "",
  cinemaId: "",
  hallId: "",
  businessDate: "",
  startAtUtc: "",
  basePrice: 90000,
  language: "Subtitle",
  status: "Open",
};

function toLocalDateTimeInput(value: string) {
  if (!value) return "";
  const date = new Date(value);
  const offset = date.getTimezoneOffset();
  const local = new Date(date.getTime() - offset * 60_000);
  return local.toISOString().slice(0, 16);
}

export function MoviesAdminShowtimesPage() {
  const [showtimes, setShowtimes] = useState<MoviesAdminShowtime[]>([]);
  const [lookups, setLookups] = useState<MoviesAdminLookups>({ movies: [], cinemas: [], halls: [] });
  const [form, setForm] = useState<MoviesAdminShowtimeUpsertRequest>(emptyForm);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");

  const filteredHalls = useMemo(
    () => lookups.halls.filter((hall) => hall.cinemaId === form.cinemaId),
    [lookups.halls, form.cinemaId],
  );

  async function loadData() {
    try {
      setLoading(true);
      setError("");
      const [showtimeData, lookupData] = await Promise.all([
        moviesAdminApi.getShowtimes(),
        moviesAdminApi.getLookups(),
      ]);
      setShowtimes(showtimeData);
      setLookups(lookupData);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Unable to load showtimes.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void loadData();
  }, []);

  function resetForm() {
    setForm(emptyForm);
    setEditingId(null);
  }

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    try {
      setSubmitting(true);
      setError("");

      const payload: MoviesAdminShowtimeUpsertRequest = {
        ...form,
        startAtUtc: new Date(form.startAtUtc).toISOString(),
      };

      if (editingId) {
        await moviesAdminApi.updateShowtime(editingId, payload);
      } else {
        await moviesAdminApi.createShowtime(payload);
      }

      resetForm();
      await loadData();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Unable to save showtime.");
    } finally {
      setSubmitting(false);
    }
  }

  function handleEdit(showtime: MoviesAdminShowtime) {
    setEditingId(showtime.id);
    setForm({
      movieId: showtime.movieId,
      cinemaId: showtime.cinemaId,
      hallId: showtime.hallId,
      businessDate: showtime.businessDate,
      startAtUtc: toLocalDateTimeInput(showtime.startAtUtc),
      basePrice: showtime.basePrice,
      language: showtime.language,
      status: showtime.status,
    });
  }

  async function handleDelete(showtimeId: string) {
    try {
      setError("");
      await moviesAdminApi.deleteShowtime(showtimeId);
      if (editingId === showtimeId) {
        resetForm();
      }
      await loadData();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Unable to cancel showtime.");
    }
  }

  return (
    <div className="space-y-6">
      <section className="rounded-[2rem] border border-white/8 bg-[radial-gradient(circle_at_top_left,rgba(34,211,238,0.12),transparent_24%),linear-gradient(180deg,rgba(255,255,255,0.05),rgba(255,255,255,0.02))] p-6 shadow-[0_24px_80px_rgba(2,6,23,0.34)]">
        <div className="flex items-start justify-between gap-4">
          <div>
            <p className="text-xs font-semibold uppercase tracking-[0.28em] text-cyan-200/70">
              Showtimes
            </p>
            <h1 className="mt-2 text-3xl font-black uppercase tracking-[0.08em] text-white">
              Schedule operations
            </h1>
          </div>
          <Badge className="border border-fuchsia-400/20 bg-fuchsia-500/10 px-3 py-1.5 text-fuchsia-200">
            {showtimes.length} showtimes
          </Badge>
        </div>
      </section>

      {error ? (
        <div className="rounded-3xl border border-red-500/20 bg-red-500/10 px-4 py-3 text-sm text-red-200">
          {error}
        </div>
      ) : null}

      <section className="grid gap-6 xl:grid-cols-[360px_minmax(0,1fr)]">
        <form
          onSubmit={handleSubmit}
          className="space-y-4 rounded-[2rem] border border-white/8 bg-white/[0.03] p-6 shadow-[0_24px_80px_rgba(2,6,23,0.34)]"
        >
          <div className="flex items-center justify-between">
            <h2 className="text-lg font-black uppercase tracking-[0.08em] text-white">
              {editingId ? "Edit showtime" : "Create showtime"}
            </h2>
            {editingId ? (
              <button type="button" className="text-sm text-cyan-200" onClick={resetForm}>
                Reset
              </button>
            ) : null}
          </div>

          <select
            value={form.movieId}
            onChange={(event) => setForm((current) => ({ ...current, movieId: event.target.value }))}
            className="w-full rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white"
          >
            <option value="">Select movie</option>
            {lookups.movies.map((movie) => (
              <option key={movie.id} value={movie.id}>
                {movie.name}
              </option>
            ))}
          </select>
          <select
            value={form.cinemaId}
            onChange={(event) =>
              setForm((current) => ({ ...current, cinemaId: event.target.value, hallId: "" }))
            }
            className="w-full rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white"
          >
            <option value="">Select cinema</option>
            {lookups.cinemas.map((cinema) => (
              <option key={cinema.id} value={cinema.id}>
                {cinema.name}
              </option>
            ))}
          </select>
          <select
            value={form.hallId}
            onChange={(event) => setForm((current) => ({ ...current, hallId: event.target.value }))}
            className="w-full rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white"
          >
            <option value="">Select hall</option>
            {filteredHalls.map((hall) => (
              <option key={hall.id} value={hall.id}>
                {hall.name}
              </option>
            ))}
          </select>
          <input
            type="date"
            value={form.businessDate}
            onChange={(event) => setForm((current) => ({ ...current, businessDate: event.target.value }))}
            className="w-full rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white"
          />
          <input
            type="datetime-local"
            value={form.startAtUtc}
            onChange={(event) => setForm((current) => ({ ...current, startAtUtc: event.target.value }))}
            className="w-full rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white"
          />
          <input
            type="number"
            value={form.basePrice}
            onChange={(event) => setForm((current) => ({ ...current, basePrice: Number(event.target.value) }))}
            className="w-full rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white"
          />
          <select
            value={form.language}
            onChange={(event) => setForm((current) => ({ ...current, language: event.target.value }))}
            className="w-full rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white"
          >
            <option value="Subtitle">Subtitle</option>
            <option value="Dubbed">Dubbed</option>
          </select>
          <select
            value={form.status}
            onChange={(event) => setForm((current) => ({ ...current, status: event.target.value }))}
            className="w-full rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white"
          >
            <option value="Draft">Draft</option>
            <option value="Open">Open</option>
            <option value="SoldOut">SoldOut</option>
            <option value="Cancelled">Cancelled</option>
            <option value="Completed">Completed</option>
          </select>
          <button
            type="submit"
            disabled={submitting}
            className="w-full rounded-2xl bg-cyan-500 px-4 py-3 text-sm font-semibold text-slate-950 disabled:opacity-50"
          >
            {submitting ? "Saving..." : editingId ? "Update showtime" : "Create showtime"}
          </button>
        </form>

        <div className="overflow-hidden rounded-[2rem] border border-white/8 bg-white/[0.03] shadow-[0_24px_80px_rgba(2,6,23,0.34)]">
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-white/8 text-left text-sm">
              <thead className="bg-white/[0.04]">
                <tr>
                  {["Movie", "Cinema", "Hall", "Date", "Start", "Price", "Status", "Actions"].map((column) => (
                    <th key={column} className="px-4 py-3 text-xs font-semibold uppercase tracking-[0.18em] text-gray-500">
                      {column}
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody className="divide-y divide-white/8">
                {loading ? (
                  <tr>
                    <td colSpan={8} className="px-4 py-6 text-gray-400">Loading showtimes...</td>
                  </tr>
                ) : null}
                {!loading && showtimes.length === 0 ? (
                  <tr>
                    <td colSpan={8} className="px-4 py-6 text-gray-400">No showtimes found.</td>
                  </tr>
                ) : null}
                {showtimes.map((showtime) => (
                  <tr key={showtime.id} className="bg-white/[0.02]">
                    <td className="px-4 py-3 text-white">{showtime.movieTitle}</td>
                    <td className="px-4 py-3 text-gray-300">{showtime.cinemaName}</td>
                    <td className="px-4 py-3 text-gray-300">{showtime.hallName}</td>
                    <td className="px-4 py-3 text-gray-300">{showtime.businessDate}</td>
                    <td className="px-4 py-3 text-gray-300">{new Date(showtime.startAtUtc).toLocaleString("vi-VN")}</td>
                    <td className="px-4 py-3 text-gray-300">{showtime.basePrice.toLocaleString("vi-VN")}</td>
                    <td className="px-4 py-3 text-gray-300">{showtime.status}</td>
                    <td className="px-4 py-3">
                      <div className="flex gap-2">
                        <button className="text-cyan-200" onClick={() => handleEdit(showtime)}>Edit</button>
                        <button className="text-rose-200" onClick={() => void handleDelete(showtime.id)}>Cancel</button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </section>
    </div>
  );
}
