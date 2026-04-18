import { api } from '@/core/api/api';
import type { FloorPlan } from '../types/map.types';

export const mapApi = {
  getAllFloors: (): Promise<FloorPlan[]> =>
    api.get('map/floors'),

  getFloorPlan: (floorLevel: string): Promise<FloorPlan> =>
    api.get(`map/floors/${encodeURIComponent(floorLevel)}`),
};
