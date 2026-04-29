import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { getImageUrl } from "../../core/utils/image";
import { eventsApi } from "./api/eventsApi";
import type { EventDto } from "./types/event.types";

type FilterValue = "all" | "ongoing" | "upcoming";
type OwnerFilter = "all" | "mall" | "shop";

export function EventsFeature() {
  const [filter, setFilter] = useState<FilterValue>("all");
  const [ownerFilter, setOwnerFilter] = useState<OwnerFilter>("all");
  const [events, setEvents] = useState<EventDto[]>([]);

  useEffect(() => {
    eventsApi.getPublicEvents(filter === "all" ? undefined : filter).then(setEvents).catch(() => setEvents([]));
  }, [filter]);

  const filteredEvents = events.filter((event) => {
    if (ownerFilter === "all") {
      return true;
    }

    const isShopEvent = Boolean(event.shopId);
    return ownerFilter === "shop" ? isShopEvent : !isShopEvent;
  });

  return (
    <main className="min-h-screen bg-slate-50 pt-28 pb-20">
      <section className="mx-auto max-w-7xl px-6">
        <div className="mb-8 flex flex-wrap items-center justify-between gap-3">
          <div>
            <h1 className="text-4xl font-black text-slate-900">Events</h1>
            <p className="mt-2 text-sm text-slate-500">Filter events by status and event owner type.</p>
          </div>

          <div className="flex flex-wrap items-center gap-3">
            <select
              value={filter}
              onChange={(e) => setFilter(e.target.value as FilterValue)}
              className="rounded-xl border border-slate-200 bg-white px-4 py-2 font-semibold text-slate-700"
            >
              <option value="all">All Events</option>
              <option value="ongoing">Ongoing</option>
              <option value="upcoming">Upcoming</option>
            </select>

            <select
              value={ownerFilter}
              onChange={(e) => setOwnerFilter(e.target.value as OwnerFilter)}
              className="rounded-xl border border-slate-200 bg-white px-4 py-2 font-semibold text-slate-700"
            >
              <option value="all">All Owners</option>
              <option value="mall">Mall Events</option>
              <option value="shop">Shop Events</option>
            </select>
          </div>
        </div>

        <div className="mb-6 text-sm font-semibold text-slate-600">
          Showing <span className="text-slate-900">{filteredEvents.length}</span> event(s) for <span className="text-slate-900">{ownerFilter === "all" ? "all owners" : ownerFilter === "mall" ? "mall" : "shop"}</span>.
        </div>

        <div className="grid gap-6 md:grid-cols-2 xl:grid-cols-3">
          {filteredEvents.map((event) => (
            <Link
              key={event.id}
              to={`/events/${event.id}`}
              className="overflow-hidden rounded-[1.5rem] border border-slate-200 bg-white shadow-sm transition hover:-translate-y-1 hover:shadow-xl"
            >
              <img src={getImageUrl(event.imageUrl)} alt={event.title} className="h-56 w-full object-cover" />
              <div className="space-y-3 p-5">
                <div className="flex flex-wrap items-center justify-between gap-2 text-xs font-bold uppercase tracking-wider">
                  <span className="rounded-full bg-slate-100 px-3 py-1 text-slate-600">
                    {event.shopId ? "Shop Event" : "Mall Event"}
                  </span>
                  <span className="rounded-full bg-slate-100 px-3 py-1 text-slate-600">
                    {event.isOngoing ? "Ongoing" : "Upcoming"}
                  </span>
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
