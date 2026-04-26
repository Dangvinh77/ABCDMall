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
  locationType: 1,
  hasGiftRegistration: false,
  giftDescription: "",
};

export function ManagerEventsPage() {
  const role = localStorage.getItem("role") || "Guest";
  const isManager = role === "Manager";
  const [events, setEvents] = useState<EventDto[]>([]);
  const [form, setForm] = useState<CreateEventRequest>(defaultForm);
  const [uploadingImage, setUploadingImage] = useState(false);
  const [uploadError, setUploadError] = useState<string>("");
  const [loading, setLoading] = useState(false);
  const [statusMessage, setStatusMessage] = useState("");
  const [error, setError] = useState("");

  const loadEvents = async () => {
    try {
      setLoading(true);
      setError("");
      const data = await eventsApi.getManagerEvents();
      setEvents(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Unable to load your events.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (isManager) {
      loadEvents();
    }
  }, [isManager]);

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
    setStatusMessage("");

    try {
      setLoading(true);
      await eventsApi.createShopEvent({
        title: form.title,
        description: form.description,
        imageUrl: form.imageUrl,
        startDateTime: form.startDateTime,
        endDateTime: form.endDateTime,
        locationType: 1,
        hasGiftRegistration: form.hasGiftRegistration,
        giftDescription: form.giftDescription,
      });
      setStatusMessage("Your shop event request has been created and sent for admin approval.");
      setForm(defaultForm);
      await loadEvents();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Unable to create your event.");
    } finally {
      setLoading(false);
    }
  };

  if (!isManager) {
    return (
      <div className="min-h-screen bg-[linear-gradient(180deg,#fff8ef_0%,#fffdf8_42%,#f8fbff_100%)] px-4 py-6 text-slate-900 sm:px-6 lg:px-8">
        <div className="mx-auto max-w-3xl rounded-[28px] border border-slate-200 bg-white/90 p-6 shadow-[0_20px_80px_rgba(15,23,42,0.08)]">
          <p className="text-xs font-semibold uppercase tracking-[0.28em] text-amber-700">Event Creation</p>
          <h1 className="mt-3 text-3xl font-black">Manager access only</h1>
          <p className="mt-3 text-sm text-slate-600">Only manager accounts may request new shop events.</p>
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
              <div className="inline-flex items-center gap-2 rounded-full bg-amber-100 px-3 py-1 text-xs font-semibold uppercase tracking-[0.28em] text-amber-700">Manager Events</div>
              <h1 className="mt-3 text-3xl font-black tracking-tight sm:text-4xl">Create Shop Event</h1>
              <p className="mt-2 max-w-2xl text-sm text-slate-600 sm:text-base">Request a shop event that will be reviewed by mall admin before appearing on the map.</p>
            </div>
            <Link to="/dashboard" className="inline-flex items-center justify-center rounded-full bg-slate-950 px-5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5">Back to Dashboard</Link>
          </div>
        </header>

        <main className="mt-6 grid gap-6 xl:grid-cols-[0.95fr_1.05fr]">
          <section className="rounded-[30px] border border-slate-200 bg-white/90 p-6 shadow-[0_24px_90px_rgba(15,23,42,0.08)] backdrop-blur-xl">
            <div className="mb-6">
              <p className="text-xs font-semibold uppercase tracking-[0.24em] text-slate-400">Event Request</p>
              <h2 className="mt-2 text-2xl font-black text-slate-950">Shop event template</h2>
              <p className="mt-2 text-sm text-slate-500">Use this form to describe your shop event, upload a hero image, and add gift registration details.</p>
            </div>

            {error && <div className="mb-5 rounded-3xl border border-red-200 bg-red-50 px-4 py-4 text-sm text-red-700">{error}</div>}
            {statusMessage && <div className="mb-5 rounded-3xl border border-emerald-200 bg-emerald-50 px-4 py-4 text-sm text-emerald-700">{statusMessage}</div>}

            <form className="space-y-6" onSubmit={handleSubmit}>
              <div className="grid gap-4 sm:grid-cols-2">
                <label className="block">
                  <span className="mb-2 block text-sm font-semibold text-slate-700">Title</span>
                  <input value={form.title} onChange={(e) => onChange("title", e.target.value)} placeholder="Spring Store Meetup" className="w-full rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3 text-slate-900 outline-none focus:border-amber-300 focus:bg-white" />
                </label>
                <label className="block">
                  <span className="mb-2 block text-sm font-semibold text-slate-700">Upload event image</span>
                  <input
                    type="file"
                    accept="image/*"
                    onChange={(e) => handleImageChange(e.target.files?.[0])}
                    className="w-full rounded-2xl border border-slate-200 bg-slate-50 px-3 py-2 text-slate-900 outline-none focus:border-amber-300 focus:bg-white"
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
                <textarea value={form.description} onChange={(e) => onChange("description", e.target.value)} rows={5} placeholder="Invite customers for a special discount and product demo." className="w-full rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3 text-slate-900 outline-none focus:border-amber-300 focus:bg-white" />
              </label>

              <div className="grid gap-4 sm:grid-cols-2">
                <label className="block">
                  <span className="mb-2 block text-sm font-semibold text-slate-700">Start date & time</span>
                  <input type="datetime-local" value={form.startDateTime} onChange={(e) => onChange("startDateTime", e.target.value)} className="w-full rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3 text-slate-900 outline-none focus:border-amber-300 focus:bg-white" />
                </label>
                <label className="block">
                  <span className="mb-2 block text-sm font-semibold text-slate-700">End date & time</span>
                  <input type="datetime-local" value={form.endDateTime} onChange={(e) => onChange("endDateTime", e.target.value)} className="w-full rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3 text-slate-900 outline-none focus:border-amber-300 focus:bg-white" />
                </label>
              </div>

              <label className="block">
                <span className="mb-2 block text-sm font-semibold text-slate-700">Gift registration</span>
                <div className="flex items-center gap-3 rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3">
                  <input id="shopEventGift" type="checkbox" checked={form.hasGiftRegistration} onChange={(e) => onChange("hasGiftRegistration", e.target.checked)} className="h-5 w-5 rounded border-slate-300 text-amber-500 focus:ring-amber-500" />
                  <label htmlFor="shopEventGift" className="text-sm text-slate-700">Enable gift registration</label>
                </div>
              </label>

              {form.hasGiftRegistration && (
                <label className="block">
                  <span className="mb-2 block text-sm font-semibold text-slate-700">Gift description</span>
                  <input value={form.giftDescription} onChange={(e) => onChange("giftDescription", e.target.value)} placeholder="Free keychain for the first 50 guests" className="w-full rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3 text-slate-900 outline-none focus:border-amber-300 focus:bg-white" />
                </label>
              )}

              <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
                <p className="text-sm text-slate-500">Shop event requests are reviewed by mall admin before appearing on the public event map.</p>
                <button type="submit" disabled={loading} className="inline-flex items-center justify-center rounded-full bg-amber-500 px-6 py-3 text-sm font-semibold text-white transition hover:bg-amber-600 disabled:cursor-not-allowed disabled:opacity-60">Submit Event Request</button>
              </div>
            </form>
          </section>

          <section className="rounded-[30px] border border-slate-200 bg-white/90 p-6 shadow-[0_24px_90px_rgba(15,23,42,0.08)] backdrop-blur-xl">
            <div className="mb-6">
              <p className="text-xs font-semibold uppercase tracking-[0.24em] text-slate-400">Your Events</p>
              <h2 className="mt-2 text-2xl font-black text-slate-950">Submitted shop events</h2>
              <p className="mt-2 text-sm text-slate-500">Pending and approved events are shown here with status and next steps.</p>
            </div>

            {loading && !events.length ? (
              <div className="rounded-3xl border border-slate-200 bg-slate-50 p-6 text-center text-slate-500">Loading your event submissions...</div>
            ) : events.length === 0 ? (
              <div className="rounded-3xl border border-slate-200 bg-slate-50 p-6 text-center text-slate-500">No event requests found yet.</div>
            ) : (
              <div className="space-y-4">
                {events.map((event) => (
                  <article key={event.id} className="rounded-[24px] border border-slate-200 p-5 shadow-sm">
                    <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
                      <div>
                        <h3 className="text-lg font-black text-slate-950">{event.title}</h3>
                        <p className="mt-2 text-sm text-slate-600">{event.description}</p>
                      </div>
                      <span className="rounded-full bg-amber-100 px-3 py-1 text-xs font-semibold uppercase tracking-[0.24em] text-amber-700">{event.approvalStatus}</span>
                    </div>
                    <div className="mt-4 flex flex-wrap gap-2 text-sm text-slate-600">
                      <span>{new Date(event.startDateTime).toLocaleString()}</span>
                      <span>→</span>
                      <span>{new Date(event.endDateTime).toLocaleString()}</span>
                    </div>
                    <div className="mt-5 flex items-center gap-3">
                      <Link to={`/events/${event.id}`} className="rounded-full border border-amber-200 bg-amber-50 px-4 py-2 text-sm font-semibold text-amber-700 transition hover:bg-amber-100">View Detail</Link>
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
