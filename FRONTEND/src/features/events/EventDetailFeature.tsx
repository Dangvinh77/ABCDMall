import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { getImageUrl } from "../../core/utils/image";
import { eventsApi } from "./api/eventsApi";
import type { EventDto } from "./types/event.types";

export function EventDetailFeature() {
  const { id } = useParams();
  const [event, setEvent] = useState<EventDto | null>(null);
  const [showModal, setShowModal] = useState(false);
  const [form, setForm] = useState({ customerName: "", customerEmail: "", customerPhone: "" });

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
      </section>
      {showModal ? (
        <div className="fixed inset-0 z-50 grid place-items-center bg-black/60 p-4">
          <form
            className="w-full max-w-md space-y-3 rounded-2xl bg-white p-6"
            onSubmit={async (e) => {
              e.preventDefault();
              if (!id) return;
              await eventsApi.registerEvent(id, form);
              alert("Registration successful. Please check your email.");
              setShowModal(false);
            }}
          >
            <h2 className="text-2xl font-black">Gift Registration</h2>
            <input className="w-full rounded-xl border p-2" placeholder="Name" onChange={(e) => setForm((x) => ({ ...x, customerName: e.target.value }))} />
            <input className="w-full rounded-xl border p-2" placeholder="Email" onChange={(e) => setForm((x) => ({ ...x, customerEmail: e.target.value }))} />
            <input className="w-full rounded-xl border p-2" placeholder="Phone" onChange={(e) => setForm((x) => ({ ...x, customerPhone: e.target.value }))} />
            <div className="flex gap-2">
              <button type="button" onClick={() => setShowModal(false)} className="flex-1 rounded-xl bg-slate-100 py-2 font-bold">Cancel</button>
              <button type="submit" className="flex-1 rounded-xl bg-mall-primary py-2 font-bold text-white">Submit</button>
            </div>
          </form>
        </div>
      ) : null}
    </main>
  );
}
