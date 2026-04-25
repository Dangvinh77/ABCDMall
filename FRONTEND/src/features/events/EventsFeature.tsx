import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { getImageUrl } from "../../core/utils/image";
import { eventsApi } from "./api/eventsApi";
import type { EventDto } from "./types/event.types";

type FilterValue = "all" | "ongoing" | "upcoming";

export function EventsFeature() {
  const [filter, setFilter] = useState<FilterValue>("all");
  const [events, setEvents] = useState<EventDto[]>([]);

  useEffect(() => {
    eventsApi.getPublicEvents(filter === "all" ? undefined : filter).then(setEvents).catch(() => setEvents([]));
  }, [filter]);

  return (
    <main className="min-h-screen bg-slate-50 pt-28 pb-20">
      <section className="mx-auto max-w-7xl px-6">
        <div className="mb-8 flex flex-wrap items-center justify-between gap-3">
          <h1 className="text-4xl font-black text-slate-900">Events</h1>
          <select
            value={filter}
            onChange={(e) => setFilter(e.target.value as FilterValue)}
            className="rounded-xl border border-slate-200 bg-white px-4 py-2 font-semibold text-slate-700"
          >
            <option value="all">All Events</option>
            <option value="ongoing">Ongoing</option>
            <option value="upcoming">Upcoming</option>
          </select>
        </div>
        <div className="grid gap-6 md:grid-cols-2 xl:grid-cols-3">
          {events.map((event) => (
            <Link key={event.id} to={`/events/${event.id}`} className="overflow-hidden rounded-[1.5rem] border border-slate-200 bg-white shadow-sm transition hover:-translate-y-1 hover:shadow-xl">
              <img src={getImageUrl(event.imageUrl)} alt={event.title} className="h-56 w-full object-cover" />
              <div className="space-y-3 p-5">
                <div className="flex items-center justify-between text-xs font-bold uppercase tracking-wider">
                  <span className="text-mall-primary">{event.locationType}</span>
                  <span className="rounded-full bg-slate-100 px-3 py-1 text-slate-600">{event.isOngoing ? "Ongoing" : "Upcoming"}</span>
                </div>
                <h3 className="line-clamp-2 text-xl font-black text-slate-900">{event.title}</h3>
                <p className="line-clamp-2 text-slate-600">{event.description}</p>
              </div>
            </Link>
          ))}
        </div>
      </section>
    </main>
  );
}
