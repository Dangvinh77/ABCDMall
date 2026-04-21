import { api } from '@/core/api/api';
import type { FloorPlan  } from '../types/map.types';

export const mapApi = {
  // Lấy danh sách tầng (User bình thường)
  getAllFloors: (): Promise<FloorPlan[]> =>
    api.get('map/floors'),

  // Lấy sơ đồ tầng chi tiết (User bình thường)
  getFloorPlan: (floorLevel: string): Promise<FloorPlan> =>
    api.get(`map/floors/${encodeURIComponent(floorLevel)}`),

  // ==========================================
  // MỚI: DÀNH CHO ADMIN QUẢN LÝ SLOT
  // ==========================================

  // Lấy toàn bộ sơ đồ tầng bao gồm cả các Slot "Available" (màu bạc)
  getAdminFloors: (): Promise<FloorPlan[]> =>
    api.get('map/floors/admin'),

  // Lấy 1 tầng cụ thể cho Admin chọn slot
  getAdminFloorPlan: (floorLevel: string): Promise<FloorPlan> =>
    api.get(`map/floors/admin/${encodeURIComponent(floorLevel)}`),

  // Gán thủ công một slot cho một Shop (Nếu cần dùng sau khi đã tạo Acc)
  reserveSlot: (locationId: number, shopInfoId: string): Promise<void> =>
    api.put(`map/floors/locations/${locationId}/reserve`, { shopInfoId }),

  // Giải phóng một slot về trạng thái Trống (Available)
  releaseSlot: (locationId: number): Promise<void> =>
    api.put(`map/floors/locations/${locationId}/release`),
};