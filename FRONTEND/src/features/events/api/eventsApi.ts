import { api } from "../../../core/api/api";
import type { EventDto, RegisterEventRequest } from "../types/event.types";

export const eventsApi = {
  getPublicEvents: (filter?: "ongoing" | "upcoming") =>
    api.get<EventDto[]>("/public/events", filter ? { filter } : undefined),
  getEventById: (id: string) => api.get<EventDto>(`/public/events/${id}`),
  getActiveEvents: () => api.get<EventDto[]>("/public/events/active"),
  getShopEvents: (shopId: string) => api.get<EventDto[]>(`/public/events/shop/${shopId}`),
  registerEvent: (id: string, payload: RegisterEventRequest) =>
    api.post<{ registrationId: string; redeemCode: string; registeredAt: string }, RegisterEventRequest>(`/public/events/${id}/register`, payload),
};
