import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { getImageUrl } from "../../core/utils/image";
import { eventsApi } from "./api/eventsApi";
import type { EventDto, EventRegistrationResult, RegisterEventRequest } from "./types/event.types";

export function EventDetailFeature() {
  const { id } = useParams();
  const [event, setEvent] = useState<EventDto | null>(null);
  const [showModal, setShowModal] = useState(false);
  const [form, setForm] = useState<RegisterEventRequest>({ customerName: "", customerEmail: "", customerPhone: "" });
  const [registrationResult, setRegistrationResult] = useState<EventRegistrationResult | null>(null);
  const [submitError, setSubmitError] = useState<string>("");
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    if (!id) return;
    eventsApi.getEventById(id).then(setEvent).catch(() => setEvent(null));
  }, [id]);

  if (!event) return <div className="p-10 text-center text-slate-500">Event not found.</div>;

  return (
    <main className="min-h-screen bg-slate-50 pt-28 pb-20">
      <section className="mx-auto max-w-5xl px-6">
        <img src={getImageUrl(event.imageUrl)} alt={event.title} className="h-96 w-full rounded-[1.5rem] object-cover" />
        <h1 className="mt-6 text-4xl font-black text-slate-900">{event.title}</h1>
        <p className="mt-4 text-slate-600">{event.description}</p>
        <p className="mt-2 text-sm font-semibold text-slate-500">{new Date(event.startDateTime).toLocaleString()} - {new Date(event.endDateTime).toLocaleString()}</p>

        {event.hasGiftRegistration ? (
          <button onClick={() => setShowModal(true)} className="mt-6 rounded-full bg-mall-primary px-7 py-3 font-bold text-white">
            Register Online for Gifts
          </button>
        ) : null}

        {registrationResult ? (
          <div className="mt-8 rounded-3xl border border-emerald-200 bg-emerald-50 p-6 text-slate-900">
            <p className="text-sm uppercase tracking-[0.24em] text-emerald-700">Registration confirmed</p>
            <h2 className="mt-3 text-2xl font-black">You're signed up!</h2>
            <p className="mt-4 text-sm text-slate-700">
              Thank you for registering for this event. A confirmation email has been sent with your redemption details.
            </p>
            <div className="mt-6 grid gap-3 sm:grid-cols-3">
              <div className="rounded-2xl bg-white p-4 shadow-sm">
                <p className="text-xs uppercase tracking-[0.24em] text-slate-500">Code</p>
                <p className="mt-2 text-lg font-black text-slate-900">{registrationResult.redeemCode}</p>
              </div>
              <div className="rounded-2xl bg-white p-4 shadow-sm">
                <p className="text-xs uppercase tracking-[0.24em] text-slate-500">Registration ID</p>
                <p className="mt-2 text-lg font-black text-slate-900">{registrationResult.registrationId}</p>
              </div>
              <div className="rounded-2xl bg-white p-4 shadow-sm">
                <p className="text-xs uppercase tracking-[0.24em] text-slate-500">Registered</p>
                <p className="mt-2 text-lg font-black text-slate-900">{new Date(registrationResult.registeredAt).toLocaleString()}</p>
              </div>
            </div>
          </div>
        ) : null}
      </section>

      {showModal ? (
        <div className="fixed inset-0 z-50 grid place-items-center bg-black/60 p-4">
          <form
            className="w-full max-w-md space-y-3 rounded-2xl bg-white p-6"
            onSubmit={async (e) => {
              e.preventDefault();
              if (!id) return;
              setSubmitError("");
              setSubmitting(true);

              try {
                const result = await eventsApi.registerEvent(id, form);
                setRegistrationResult(result);
                setShowModal(false);
              } catch (err) {
                setSubmitError(err instanceof Error ? err.message : "Registration failed. Please try again.");
              } finally {
                setSubmitting(false);
              }
            }}
          >
            <h2 className="text-2xl font-black">Gift Registration</h2>
            {submitError ? <p className="rounded-xl bg-red-50 px-4 py-3 text-sm text-red-700">{submitError}</p> : null}
            <input
              className="w-full rounded-xl border p-2"
              placeholder="Name"
              value={form.customerName}
              onChange={(e) => setForm((x) => ({ ...x, customerName: e.target.value }))}
            />
            <input
              className="w-full rounded-xl border p-2"
              placeholder="Email"
              type="email"
              value={form.customerEmail}
              onChange={(e) => setForm((x) => ({ ...x, customerEmail: e.target.value }))}
            />
            <input
              className="w-full rounded-xl border p-2"
              placeholder="Phone"
              value={form.customerPhone}
              onChange={(e) => setForm((x) => ({ ...x, customerPhone: e.target.value }))}
            />
            <div className="flex gap-2">
              <button type="button" onClick={() => setShowModal(false)} className="flex-1 rounded-xl bg-slate-100 py-2 font-bold">
                Cancel
              </button>
              <button type="submit" disabled={submitting} className="flex-1 rounded-xl bg-mall-primary py-2 font-bold text-white disabled:cursor-not-allowed disabled:opacity-60">
                {submitting ? "Submitting..." : "Submit"}
              </button>
            </div>
          </form>
        </div>
      ) : null}
    </main>
  );
}
