import { useEffect, useState } from "react";
import { Badge } from "../../movies/component/ui/badge";
import {
  type MoviesAdminMovie,
  type MoviesAdminMovieUpsertRequest,
  moviesAdminApi,
} from "../services/moviesAdminApi";

const emptyForm: MoviesAdminMovieUpsertRequest = {
  title: "",
  slug: "",
  synopsis: "",
  durationMinutes: 120,
  posterUrl: "",
  trailerUrl: "",
  releaseDate: "",
  ratingLabel: "",
  defaultLanguage: "Subtitle",
  status: "Draft",
};

export function MoviesAdminMoviesPage() {
  const [movies, setMovies] = useState<MoviesAdminMovie[]>([]);
  const [form, setForm] = useState<MoviesAdminMovieUpsertRequest>(emptyForm);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");

  async function loadMovies() {
    try {
      setLoading(true);
      setError("");
      setMovies(await moviesAdminApi.getMovies());
    } catch (err) {
      setError(err instanceof Error ? err.message : "Unable to load movies.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void loadMovies();
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

      const payload: MoviesAdminMovieUpsertRequest = {
        ...form,
        releaseDate: form.releaseDate || undefined,
      };

      if (editingId) {
        await moviesAdminApi.updateMovie(editingId, payload);
      } else {
        await moviesAdminApi.createMovie(payload);
      }

      resetForm();
      await loadMovies();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Unable to save movie.");
    } finally {
      setSubmitting(false);
    }
  }

  async function handleDelete(movieId: string) {
    try {
      setError("");
      await moviesAdminApi.deleteMovie(movieId);
      if (editingId === movieId) {
        resetForm();
      }
      await loadMovies();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Unable to delete movie.");
    }
  }

  function handleEdit(movie: MoviesAdminMovie) {
    setEditingId(movie.id);
    setForm({
      title: movie.title,
      slug: movie.slug,
      synopsis: movie.synopsis ?? "",
      durationMinutes: movie.durationMinutes,
      posterUrl: movie.posterUrl ?? "",
      trailerUrl: movie.trailerUrl ?? "",
      releaseDate: movie.releaseDate ?? "",
      ratingLabel: movie.ratingLabel ?? "",
      defaultLanguage: movie.defaultLanguage,
      status: movie.status,
    });
  }

  return (
    <div className="space-y-6">
      <section className="rounded-[2rem] border border-white/8 bg-[radial-gradient(circle_at_top_left,rgba(34,211,238,0.12),transparent_24%),linear-gradient(180deg,rgba(255,255,255,0.05),rgba(255,255,255,0.02))] p-6 shadow-[0_24px_80px_rgba(2,6,23,0.34)]">
        <div className="flex items-start justify-between gap-4">
          <div>
            <p className="text-xs font-semibold uppercase tracking-[0.28em] text-cyan-200/70">
              Movies
            </p>
            <h1 className="mt-2 text-3xl font-black uppercase tracking-[0.08em] text-white">
              Catalog management
            </h1>
            <p className="mt-3 max-w-3xl text-sm leading-7 text-gray-400">
              Create, update or disable movies for the public booking flow.
            </p>
          </div>
          <Badge className="border border-fuchsia-400/20 bg-fuchsia-500/10 px-3 py-1.5 text-fuchsia-200">
            {movies.length} movies
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
              {editingId ? "Edit movie" : "Create movie"}
            </h2>
            {editingId ? (
              <button type="button" className="text-sm text-cyan-200" onClick={resetForm}>
                Reset
              </button>
            ) : null}
          </div>

          <input
            value={form.title}
            onChange={(event) => setForm((current) => ({ ...current, title: event.target.value }))}
            placeholder="Title"
            className="w-full rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white"
          />
          <input
            value={form.slug ?? ""}
            onChange={(event) => setForm((current) => ({ ...current, slug: event.target.value }))}
            placeholder="Slug"
            className="w-full rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white"
          />
          <textarea
            value={form.synopsis ?? ""}
            onChange={(event) => setForm((current) => ({ ...current, synopsis: event.target.value }))}
            placeholder="Synopsis"
            rows={4}
            className="w-full rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white"
          />
          <input
            type="number"
            value={form.durationMinutes}
            onChange={(event) =>
              setForm((current) => ({ ...current, durationMinutes: Number(event.target.value) }))
            }
            placeholder="Duration (minutes)"
            className="w-full rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white"
          />
          <input
            value={form.posterUrl ?? ""}
            onChange={(event) => setForm((current) => ({ ...current, posterUrl: event.target.value }))}
            placeholder="Poster URL"
            className="w-full rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white"
          />
          <input
            value={form.trailerUrl ?? ""}
            onChange={(event) => setForm((current) => ({ ...current, trailerUrl: event.target.value }))}
            placeholder="Trailer URL"
            className="w-full rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white"
          />
          <input
            type="date"
            value={form.releaseDate ?? ""}
            onChange={(event) => setForm((current) => ({ ...current, releaseDate: event.target.value }))}
            className="w-full rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white"
          />
          <input
            value={form.ratingLabel ?? ""}
            onChange={(event) => setForm((current) => ({ ...current, ratingLabel: event.target.value }))}
            placeholder="Rating label"
            className="w-full rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white"
          />
          <select
            value={form.defaultLanguage}
            onChange={(event) =>
              setForm((current) => ({ ...current, defaultLanguage: event.target.value }))
            }
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
            <option value="ComingSoon">ComingSoon</option>
            <option value="NowShowing">NowShowing</option>
            <option value="Ended">Ended</option>
            <option value="Inactive">Inactive</option>
          </select>
          <button
            type="submit"
            disabled={submitting}
            className="w-full rounded-2xl bg-cyan-500 px-4 py-3 text-sm font-semibold text-slate-950 disabled:opacity-50"
          >
            {submitting ? "Saving..." : editingId ? "Update movie" : "Create movie"}
          </button>
        </form>

        <div className="overflow-hidden rounded-[2rem] border border-white/8 bg-white/[0.03] shadow-[0_24px_80px_rgba(2,6,23,0.34)]">
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-white/8 text-left text-sm">
              <thead className="bg-white/[0.04]">
                <tr>
                  {["Title", "Slug", "Duration", "Status", "Showtimes", "Actions"].map((column) => (
                    <th
                      key={column}
                      className="px-4 py-3 text-xs font-semibold uppercase tracking-[0.18em] text-gray-500"
                    >
                      {column}
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody className="divide-y divide-white/8">
                {loading ? (
                  <tr>
                    <td colSpan={6} className="px-4 py-6 text-gray-400">
                      Loading movies...
                    </td>
                  </tr>
                ) : null}
                {!loading && movies.length === 0 ? (
                  <tr>
                    <td colSpan={6} className="px-4 py-6 text-gray-400">
                      No movies found.
                    </td>
                  </tr>
                ) : null}
                {movies.map((movie) => (
                  <tr key={movie.id} className="bg-white/[0.02]">
                    <td className="px-4 py-3 text-white">{movie.title}</td>
                    <td className="px-4 py-3 text-gray-300">{movie.slug}</td>
                    <td className="px-4 py-3 text-gray-300">{movie.durationMinutes} min</td>
                    <td className="px-4 py-3 text-gray-300">{movie.status}</td>
                    <td className="px-4 py-3 text-gray-300">{movie.showtimeCount}</td>
                    <td className="px-4 py-3">
                      <div className="flex gap-2">
                        <button className="text-cyan-200" onClick={() => handleEdit(movie)}>
                          Edit
                        </button>
                        <button className="text-rose-200" onClick={() => void handleDelete(movie.id)}>
                          Disable
                        </button>
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
