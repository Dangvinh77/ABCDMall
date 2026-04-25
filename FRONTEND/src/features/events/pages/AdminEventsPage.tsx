import { useEffect, useState, type FormEvent } from "react";
import { Link } from "react-router-dom";
import { getImageUrl } from "../../../core/utils/image";
import { eventsApi } from "../api/eventsApi";
import type { CreateEventRequest, EventDto } from "../types/event.types";

const defaultForm: CreateEventRequest = {
  title: "",
  description: "",
  imageUrl: "",
  startDateTime: new Date().toISOString().slice(0, 16),
  endDateTime: new Date(Date.now() + 1000 * 60 * 60 * 4).toISOString().slice(0, 16),
  locationType: 2,
  hasGiftRegistration: false,
  giftDescription: "",
};

const locationOptions = [
  { value: 2, label: "Floor 1 Event Hall" },
  { value: 3, label: "Floor 2 Event Hall" },
  { value: 4, label: "Floor 3 Event Hall" },
  { value: 5, label: "Floor 4 Event Hall" },
];

export function AdminEventsPage() {
  const role = localStorage.getItem("role") || "Guest";
  const isAdmin = role === "Admin";
  const [reviewEvents, setReviewEvents] = useState<EventDto[]>([]);
  const [form, setForm] = useState<CreateEventRequest>(defaultForm);
  const [uploadingImage, setUploadingImage] = useState(false);
  const [uploadError, setUploadError] = useState<string>("");
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState<string>("");
  const [error, setError] = useState<string>("");

  const loadReviewEvents = async () => {
    try {
      setLoading(true);
      setError("");
      const events = await eventsApi.getAdminReviewEvents();
      setReviewEvents(events);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Unable to load review events.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (isAdmin) {
      loadReviewEvents();
    }
  }, [isAdmin]);

  const onChange = (field: keyof CreateEventRequest, value: string | boolean) => {
    setForm((prev) => ({ ...prev, [field]: value }));
  };

  const handleImageChange = async (file?: File) => {
    if (!file) return;

    setUploadError("");
    setUploadingImage(true);

    try {
      const result = await eventsApi.uploadEventImage(file);
      setForm((prev) => ({ ...prev, imageUrl: result.imageUrl }));
    } catch (err) {
      setUploadError(err instanceof Error ? err.message : "Unable to upload event image.");
    } finally {
      setUploadingImage(false);
    }
  };

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError("");
    setMessage("");

    try {
      setLoading(true);
      await eventsApi.createMallEvent(form);
      setMessage("Mall event created successfully. It is now visible in the event catalog.");
      setForm(defaultForm);
      await loadReviewEvents();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Unable to create event.");
    } finally {
      setLoading(false);
    }
  };

  const handleApprove = async (eventId: string) => {
    try {
      setLoading(true);
      setError("");
      await eventsApi.approveEvent(eventId);
      setMessage("Event approved and published.");
      await loadReviewEvents();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Unable to approve event.");
    } finally {
      setLoading(false);
    }
  };

  const handleReject = async (eventId: string) => {
    try {
      setLoading(true);
      setError("");
      await eventsApi.rejectEvent(eventId);
      setMessage("Event rejected and removed from the review queue.");
      await loadReviewEvents();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Unable to reject event.");
    } finally {
      setLoading(false);
    }
  };

  if (!isAdmin) {
    return (
      <div className="min-h-screen bg-[linear-gradient(180deg,#fff8ef_0%,#fffdf8_42%,#f8fbff_100%)] px-4 py-6 text-slate-900 sm:px-6 lg:px-8">
        <div className="mx-auto max-w-3xl rounded-[28px] border border-slate-200 bg-white/90 p-6 shadow-[0_20px_80px_rgba(15,23,42,0.08)]">
          <p className="text-xs font-semibold uppercase tracking-[0.28em] text-amber-700">Event Management</p>
          <h1 className="mt-3 text-3xl font-black">Admin access only</h1>
          <p className="mt-3 text-sm text-slate-600">Only admin accounts can create mall events and approve pending shop event submissions.</p>
          <Link to="/dashboard" className="mt-6 inline-flex rounded-full bg-slate-950 px-5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5">Back to Dashboard</Link>
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
              <div className="inline-flex items-center gap-2 rounded-full bg-rose-100 px-3 py-1 text-xs font-semibold uppercase tracking-[0.28em] text-rose-700">Event Management</div>
              <h1 className="mt-3 text-3xl font-black tracking-tight sm:text-4xl">Mall Event Control</h1>
              <p className="mt-2 max-w-2xl text-sm text-slate-600 sm:text-base">Create promotional mall events and review shop-submitted event requests.</p>
            </div>
            <Link to="/admin-management" className="inline-flex items-center justify-center rounded-full bg-slate-950 px-5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5">Back to Admin</Link>
          </div>
        </header>

        <main className="mt-6 grid gap-6 xl:grid-cols-[0.95fr_1.05fr]">
          <section className="rounded-[30px] border border-slate-200 bg-white/90 p-6 shadow-[0_24px_90px_rgba(15,23,42,0.08)] backdrop-blur-xl">
            <div className="mb-6">
              <p className="text-xs font-semibold uppercase tracking-[0.24em] text-rose-500">Create Mall Event</p>
              <h2 className="mt-2 text-2xl font-black text-slate-950">Mall event template</h2>
              <p className="mt-2 text-sm text-slate-500">Publish hall-based events and highlight special rewards for shoppers.</p>
            </div>

            {error && <div className="mb-5 rounded-3xl border border-red-200 bg-red-50 px-4 py-4 text-sm text-red-700">{error}</div>}
            {message && <div className="mb-5 rounded-3xl border border-emerald-200 bg-emerald-50 px-4 py-4 text-sm text-emerald-700">{message}</div>}

            <form className="space-y-6" onSubmit={handleSubmit}>
              <div className="grid gap-4 sm:grid-cols-2">
                <label className="block">
                  <span className="mb-2 block text-sm font-semibold text-slate-700">Title</span>
                  <input value={form.title} onChange={(e) => onChange("title", e.target.value)} placeholder="Spring Gift Carnival" className="w-full rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3 text-slate-900 outline-none focus:border-rose-300 focus:bg-white" />
                </label>
                <label className="block">
                  <span className="mb-2 block text-sm font-semibold text-slate-700">Upload event image</span>
                  <input
                    type="file"
                    accept="image/*"
                    onChange={(e) => handleImageChange(e.target.files?.[0])}
                    className="w-full rounded-2xl border border-slate-200 bg-slate-50 px-3 py-2 text-slate-900 outline-none focus:border-rose-300 focus:bg-white"
                  />
                  <p className="mt-2 text-xs text-slate-500">Supported formats: PNG, JPG, JPEG, WEBP, GIF, AVIF</p>
                  {uploadError ? <p className="mt-2 text-sm text-red-600">{uploadError}</p> : null}
                  {form.imageUrl ? (
                    <img src={getImageUrl(form.imageUrl)} alt="Uploaded event" className="mt-3 h-24 w-full rounded-2xl object-cover" />
                  ) : null}
                </label>
              </div>

              <label className="block">
                <span className="mb-2 block text-sm font-semibold text-slate-700">Description</span>
                <textarea value={form.description} onChange={(e) => onChange("description", e.target.value)} rows={5} placeholder="A mall-wide festival with prizes and special check-in rewards." className="w-full rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3 text-slate-900 outline-none focus:border-rose-300 focus:bg-white" />
              </label>

              <div className="grid gap-4 sm:grid-cols-2">
                <label className="block">
                  <span className="mb-2 block text-sm font-semibold text-slate-700">Start date & time</span>
                  <input type="datetime-local" value={form.startDateTime} onChange={(e) => onChange("startDateTime", e.target.value)} className="w-full rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3 text-slate-900 outline-none focus:border-rose-300 focus:bg-white" />
                </label>
                <label className="block">
                  <span className="mb-2 block text-sm font-semibold text-slate-700">End date & time</span>
                  <input type="datetime-local" value={form.endDateTime} onChange={(e) => onChange("endDateTime", e.target.value)} className="w-full rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3 text-slate-900 outline-none focus:border-rose-300 focus:bg-white" />
                </label>
              </div>

              <div className="grid gap-4 sm:grid-cols-2">
                <label className="block">
                  <span className="mb-2 block text-sm font-semibold text-slate-700">Event hall location</span>
                  <select value={form.locationType} onChange={(e) => onChange("locationType", Number(e.target.value))} className="w-full rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3 text-slate-900 outline-none focus:border-rose-300 focus:bg-white">
                    {locationOptions.map((option) => (
                      <option key={option.value} value={option.value}>{option.label}</option>
                    ))}
                  </select>
                </label>
                <label className="block">
                  <span className="mb-2 block text-sm font-semibold text-slate-700">Gift registration</span>
                  <div className="flex items-center gap-3 rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3">
                    <input id="mallEventGift" type="checkbox" checked={form.hasGiftRegistration} onChange={(e) => onChange("hasGiftRegistration", e.target.checked)} className="h-5 w-5 rounded border-slate-300 text-rose-500 focus:ring-rose-500" />
                    <label htmlFor="mallEventGift" className="text-sm text-slate-700">Enable online gift registration</label>
                  </div>
                </label>
              </div>

              {form.hasGiftRegistration && (
                <label className="block">
                  <span className="mb-2 block text-sm font-semibold text-slate-700">Reward description</span>
                  <input value={form.giftDescription} onChange={(e) => onChange("giftDescription", e.target.value)} placeholder="10% Discount Voucher" className="w-full rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3 text-slate-900 outline-none focus:border-rose-300 focus:bg-white" />
                </label>
              )}

              <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
                <p className="text-sm text-slate-500">Mall admin created events are published immediately with no extra review step.</p>
                <button type="submit" disabled={loading} className="inline-flex items-center justify-center rounded-full bg-rose-500 px-6 py-3 text-sm font-semibold text-white transition hover:bg-rose-600 disabled:cursor-not-allowed disabled:opacity-60">
                  Create Event
                </button>
              </div>
            </form>
          </section>

          <section className="rounded-[30px] border border-slate-200 bg-white/90 p-6 shadow-[0_24px_90px_rgba(15,23,42,0.08)] backdrop-blur-xl">
            <div className="mb-6">
              <p className="text-xs font-semibold uppercase tracking-[0.24em] text-slate-400">Pending approvals</p>
              <h2 className="mt-2 text-2xl font-black text-slate-950">Shop event review queue</h2>
              <p className="mt-2 text-sm text-slate-500">Approve or reject incoming shop event requests before they become public on the mall map.</p>
            </div>

            {loading && !reviewEvents.length ? (
              <div className="rounded-3xl border border-slate-200 bg-slate-50 p-6 text-center text-slate-500">Loading review queue...</div>
            ) : reviewEvents.length === 0 ? (
              <div className="rounded-3xl border border-slate-200 bg-slate-50 p-6 text-center text-slate-500">No pending shop event requests at the moment.</div>
            ) : (
              <div className="space-y-4">
                {reviewEvents.map((event) => (
                  <article key={event.id} className="rounded-[24px] border border-slate-200 p-5 shadow-sm">
                    <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
                      <div>
                        <p className="text-xs font-semibold uppercase tracking-[0.24em] text-slate-500">{event.locationType}</p>
                        <h3 className="mt-1 text-lg font-black text-slate-950">{event.title}</h3>
                        <p className="mt-2 text-sm text-slate-600">{event.description}</p>
                      </div>
                      <span className="rounded-full bg-amber-100 px-3 py-1 text-xs font-semibold uppercase tracking-[0.24em] text-amber-700">{event.approvalStatus}</span>
                    </div>
                    <div className="mt-4 flex flex-wrap gap-2 text-sm text-slate-600">
                      <span>{new Date(event.startDateTime).toLocaleString()}</span>
                      <span>→</span>
                      <span>{new Date(event.endDateTime).toLocaleString()}</span>
                      <span>• Created by {event.createdByName}</span>
                    </div>
                    <div className="mt-5 flex flex-col gap-3 sm:flex-row">
                      <button onClick={() => handleApprove(event.id)} disabled={loading} className="rounded-full bg-emerald-500 px-4 py-2 text-sm font-semibold text-white transition hover:bg-emerald-600 disabled:cursor-not-allowed disabled:opacity-60">Approve</button>
                      <button onClick={() => handleReject(event.id)} disabled={loading} className="rounded-full border border-slate-200 bg-white px-4 py-2 text-sm font-semibold text-slate-700 transition hover:bg-slate-100 disabled:cursor-not-allowed disabled:opacity-60">Reject</button>
                      <Link to={`/events/${event.id}`} className="rounded-full border border-rose-200 bg-rose-50 px-4 py-2 text-sm font-semibold text-rose-700 transition hover:bg-rose-100">View Event</Link>
                    </div>
                  </article>
                ))}
              </div>
            )}
          </section>
        </main>
      </div>
    </div>
  );
}
