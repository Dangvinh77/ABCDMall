import type { EventDto } from '../types/event.types';

// Sửa lại port này cho khớp với port Terminal Backend của bạn (thường là 5000, 5001 hoặc 5184)
const BASE_URL = 'http://localhost:5184/api/events'; 

export const eventsApi = {
  getEvents: async (keyword?: string, eventType?: number, status?: string): Promise<EventDto[]> => {
    try {
      const params = new URLSearchParams();
      if (keyword) params.append('keyword', keyword);
      if (eventType) params.append('eventType', eventType.toString());
      if (status) params.append('status', status);

      const response = await fetch(`${BASE_URL}?${params.toString()}`);
      if (!response.ok) throw new Error('Network response was not ok');
      return await response.json();
    } catch (error) {
      console.error("Lỗi gọi API Events, dùng Mock Data thay thế:", error);
      return []; // Trả về mảng rỗng để trigger Mock Data
    }
  },

  getHotEvents: async (): Promise<EventDto[]> => {
    try {
      const response = await fetch(`${BASE_URL}/hot`);
      if (!response.ok) throw new Error('Network response was not ok');
      return await response.json();
    } catch (error) {
      return []; 
    }
  }
};