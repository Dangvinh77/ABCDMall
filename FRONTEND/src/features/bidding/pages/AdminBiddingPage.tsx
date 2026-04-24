import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { biddingApi, type AdminCarouselBid } from "../api/biddingApi";

function formatCurrency(value: number) {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(value);
}

export default function AdminBiddingPage() {
  const role = localStorage.getItem("role") || "Guest";
  const isAdmin = role === "Admin";
  const [bids, setBids] = useState<AdminCarouselBid[]>([]);
  const [imageUrl, setImageUrl] = useState("");
  const [description, setDescription] = useState("");
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");
  const [message, setMessage] = useState("");

  async function loadBids() {
    try {
      setLoading(true);
      setError("");
      setBids(await biddingApi.getAdminBids());
    } catch (err) {
      setError(err instanceof Error ? err.message : "Unable to load bids.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    if (isAdmin) {
      loadBids();
    } else {
      setLoading(false);
    }
  }, [isAdmin]);

  async function handleMovieAdSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    try {
      setSaving(true);
      setError("");
      const result = await biddingApi.upsertMovieAd({ imageUrl, description });
      setMessage(`Movie ad saved for ${new Date(result.targetMondayDate).toLocaleDateString()}.`);
      setImageUrl(result.imageUrl);
      setDescription(result.description);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Unable to save movie ad.");
    } finally {
      setSaving(false);
    }
  }

  async function simulateSaturday() {
    try {
      setError("");
      const result = await biddingApi.resolveUpcomingWeek();
      setMessage(`Saturday simulation complete. Won: ${result.wonCount}, Lost: ${result.lostCount}.`);
      await loadBids();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Unable to resolve bids.");
    }
  }

  async function simulateMonday() {
    try {
      setError("");
      const result = await biddingApi.publishUpcomingWeek();
      setMessage(`Monday simulation complete. Published slots: ${result.totalSlots}.`);
      await loadBids();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Unable to publish carousel.");
    }
  }

  if (!isAdmin) {
    return (
      <div className="min-h-screen bg-[linear-gradient(180deg,#fff8ef_0%,#fffdf8_42%,#f8fbff_100%)] px-4 py-6 text-slate-900 sm:px-6 lg:px-8">
        <div className="mx-auto max-w-3xl rounded-[28px] border border-slate-200 bg-white/90 p-6 shadow-[0_20px_80px_rgba(15,23,42,0.08)]">
          <p className="text-xs font-semibold uppercase tracking-[0.28em] text-amber-700">Admin Bidding</p>
          <h1 className="mt-3 text-3xl font-black">Admin access only</h1>
          <p className="mt-3 text-sm text-slate-600">This area is only available for admin accounts.</p>
          <Link to="/admin-management" className="mt-6 inline-flex rounded-full bg-slate-950 px-5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5">
            Back to Admin Management
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-[linear-gradient(180deg,#fff8ef_0%,#fffdf8_42%,#f8fbff_100%)] text-slate-900">
      <div className="mx-auto flex min-h-screen w-full max-w-7xl flex-col px-4 py-6 sm:px-6 lg:px-8">
        <header className="rounded-[28px] border border-white/70 bg-white/80 px-5 py-4 shadow-[0_20px_80px_rgba(15,23,42,0.08)] backdrop-blur-xl sm:px-6">
          <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
            <div>
              <div className="inline-flex items-center gap-2 rounded-full bg-amber-100 px-3 py-1 text-xs font-semibold uppercase tracking-[0.28em] text-amber-700">
                Admin Bidding
              </div>
              <h1 className="mt-3 text-3xl font-black tracking-tight sm:text-4xl">Carousel Bidding Control</h1>
              <p className="mt-2 max-w-2xl text-sm text-slate-600 sm:text-base">
                Review upcoming bids, submit the fixed movie slot, and run the Saturday or Monday simulation flows.
              </p>
            </div>
            <Link to="/admin-management" className="inline-flex items-center justify-center rounded-full bg-slate-950 px-5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5">
              Back to Admin Management
            </Link>
          </div>
        </header>

        <main className="mt-6 grid gap-6 xl:grid-cols-[1.08fr_0.92fr]">
          <section className="rounded-[30px] border border-slate-200 bg-white/90 p-5 shadow-[0_24px_90px_rgba(15,23,42,0.08)] backdrop-blur-xl sm:p-6">
            <p className="text-xs font-semibold uppercase tracking-[0.24em] text-slate-400">Upcoming Week</p>
            <h2 className="mt-3 text-2xl font-black">Bid table</h2>
            <p className="mt-2 text-sm text-slate-500">Read-only ranking view for the next weekly carousel round.</p>

            {loading ? (
              <div className="mt-6 rounded-3xl border border-dashed border-slate-200 bg-slate-50 p-8 text-center text-sm text-slate-500">
                Loading bids...
              </div>
            ) : (
              <div className="mt-6 overflow-hidden rounded-[26px] border border-slate-200">
                <div className="overflow-x-auto">
                  <table className="min-w-full divide-y divide-slate-200 text-sm">
                    <thead className="bg-slate-50 text-left text-slate-500">
                      <tr>
                        <th className="px-4 py-3 font-semibold">Shop</th>
                        <th className="px-4 py-3 font-semibold">Template</th>
                        <th className="px-4 py-3 font-semibold">Bid</th>
                        <th className="px-4 py-3 font-semibold">Status</th>
                        <th className="px-4 py-3 font-semibold">Created</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-slate-200 bg-white">
                      {bids.map((bid) => (
                        <tr key={bid.id}>
                          <td className="px-4 py-4 font-semibold text-slate-900">{bid.shopName}</td>
                          <td className="px-4 py-4 text-slate-600">{bid.templateType}</td>
                          <td className="px-4 py-4 text-slate-900">{formatCurrency(bid.bidAmount)}</td>
                          <td className="px-4 py-4">
                            <span className="inline-flex rounded-full bg-slate-100 px-3 py-1 text-xs font-semibold text-slate-700">
                              {bid.status}
                            </span>
                          </td>
                          <td className="px-4 py-4 text-slate-600">{new Date(bid.createdAt).toLocaleString()}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </div>
            )}
          </section>

          <section className="space-y-6">
            <form onSubmit={handleMovieAdSubmit} className="rounded-[30px] border border-slate-200 bg-white/90 p-5 shadow-[0_24px_90px_rgba(15,23,42,0.08)] backdrop-blur-xl sm:p-6">
              <p className="text-xs font-semibold uppercase tracking-[0.24em] text-amber-700">Movie Slot</p>
              <h2 className="mt-3 text-2xl font-black">Weekly movie ad</h2>
              <div className="mt-5 space-y-4">
                <label className="block">
                  <span className="mb-2 block text-sm font-semibold text-slate-700">Image URL</span>
                  <input
                    value={imageUrl}
                    onChange={(event) => setImageUrl(event.target.value)}
                    className="w-full rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3 outline-none transition focus:border-amber-400 focus:bg-white focus:ring-4 focus:ring-amber-100"
                  />
                </label>
                <label className="block">
                  <span className="mb-2 block text-sm font-semibold text-slate-700">Description</span>
                  <textarea
                    rows={4}
                    value={description}
                    onChange={(event) => setDescription(event.target.value)}
                    className="w-full resize-none rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3 outline-none transition focus:border-amber-400 focus:bg-white focus:ring-4 focus:ring-amber-100"
                  />
                </label>
                <button
                  type="submit"
                  disabled={saving}
                  className="inline-flex rounded-full bg-slate-950 px-5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5 disabled:cursor-not-allowed disabled:opacity-60"
                >
                  {saving ? "Saving..." : "Save Movie Ad"}
                </button>
              </div>
            </form>

            <section className="rounded-[30px] border border-slate-200 bg-white/90 p-5 shadow-[0_24px_90px_rgba(15,23,42,0.08)] backdrop-blur-xl sm:p-6">
              <p className="text-xs font-semibold uppercase tracking-[0.24em] text-slate-400">Simulation</p>
              <h2 className="mt-3 text-2xl font-black">Weekly trigger controls</h2>
              <div className="mt-5 grid gap-4">
                <button
                  type="button"
                  onClick={simulateSaturday}
                  className="inline-flex items-center justify-center rounded-[24px] bg-amber-300 px-5 py-5 text-base font-black text-slate-950 transition hover:-translate-y-0.5 hover:bg-amber-200"
                >
                  Simulate Saturday (Resolve Bids)
                </button>
                <button
                  type="button"
                  onClick={simulateMonday}
                  className="inline-flex items-center justify-center rounded-[24px] bg-slate-950 px-5 py-5 text-base font-black text-white transition hover:-translate-y-0.5 hover:bg-slate-800"
                >
                  Simulate Monday (Publish Carousel)
                </button>
              </div>

              {error && <p className="mt-4 rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">{error}</p>}
              {message && <p className="mt-4 rounded-2xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-700">{message}</p>}
            </section>
          </section>
        </main>
      </div>
    </div>
  );
}
