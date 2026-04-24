import { api } from "../../../core/api/api";
import type { EventDto } from "../types/event.types";

export const eventsApi = {
  getEvents: async (keyword?: string, eventType?: number, status?: string): Promise<EventDto[]> => {
    try {
      const params: Record<string, string | number> = {};
      if (keyword) params.keyword = keyword;
      if (eventType) params.eventType = eventType;
      if (status) params.status = status;

      return await api.get<EventDto[]>("/events", params);
    } catch (error) {
      console.error("Lỗi gọi API Events, dùng Mock Data thay thế:", error);
      return [];
    } 
  },

  getHotEvents: async (): Promise<EventDto[]> => {
    try {
      return await api.get<EventDto[]>("/events/hot");
    } catch {
      return [];
    }
  }
};
