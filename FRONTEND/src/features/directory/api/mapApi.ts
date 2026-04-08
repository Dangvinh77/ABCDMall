import { FloorPlan } from "../types/map.types";

const BASE_URL = 'http://localhost:5173'; 

export const mapApi = {
  getAllFloors: async (): Promise<FloorPlan[]> => {
    const response = await fetch(`${BASE_URL}/map/floors`);
    if (!response.ok) throw new Error('Không thể tải danh sách tầng');
    return response.json();
  },

  getFloorPlan: async (floorLevel: string): Promise<FloorPlan> => {
    const response = await fetch(`${BASE_URL}/map/floors/${floorLevel}`);
    if (!response.ok) throw new Error('Không thể tải dữ liệu sơ đồ tầng');
    return response.json();
  }
};