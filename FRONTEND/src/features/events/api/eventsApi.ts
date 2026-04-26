import { api, http } from "../../../core/api/api";
import type { CreateEventRequest, EventDto, EventRegistrationResult, RegisterEventRequest } from "../types/event.types";

export const eventsApi = {
  getPublicEvents: (filter?: "ongoing" | "upcoming") =>
    api.get<EventDto[]>("/public/events", filter ? { filter } : undefined),
  getEventById: (id: string) => api.get<EventDto>(`/public/events/${id}`),
  getActiveEvents: () => api.get<EventDto[]>("/public/events/active"),
  getShopEvents: (shopId: string) => api.get<EventDto[]>(`/public/events/shop/${shopId}`),
  getAdminReviewEvents: () => api.get<EventDto[]>("/admin/events"),
  approveEvent: (id: string) => api.post<void>(`/admin/events/${id}/approve`),
  rejectEvent: (id: string) => api.post<void>(`/admin/events/${id}/reject`),
  getManagerEvents: () => api.get<EventDto[]>("/manager/events"),
  createMallEvent: (payload: CreateEventRequest) => api.post<EventDto, CreateEventRequest>("/admin/events", payload),
  createShopEvent: (payload: CreateEventRequest) => api.post<EventDto, CreateEventRequest>("/manager/events", payload),
  uploadEventImage: async (file: File): Promise<{ imageUrl: string }> => {
    try {
      const formData = new FormData();
      formData.append("file", file);
      const response = await http.post<{ imageUrl: string }>("/events/upload", formData);
      return response.data;
    } catch (error) {
      if (error instanceof Error) {
        throw error;
      }

      throw new Error("Unable to upload event image.");
    }
  },
  registerEvent: (id: string, payload: RegisterEventRequest) =>
    api.post<EventRegistrationResult, RegisterEventRequest>(`/public/events/${id}/register`, payload),
};
