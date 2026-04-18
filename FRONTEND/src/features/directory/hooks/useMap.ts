import { useState, useEffect } from 'react';
import type { FloorPlan } from '../types/map.types';
import { mapApi } from '../api/mapApi';

export function useMap() {
  const [floors, setFloors] = useState<FloorPlan[]>([]);
  const [activeFloor, setActiveFloor] = useState<FloorPlan | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    mapApi.getAllFloors()
      .then((data) => {
        setFloors(data);
        setActiveFloor(data[0] ?? null);
      })
      .catch(() => setError('Không thể tải sơ đồ tầng.'))
      .finally(() => setLoading(false));
  }, []);

  const switchFloor = (floor: FloorPlan) => {
    setActiveFloor(floor);
  };

  return { floors, activeFloor, switchFloor, loading, error };
}
