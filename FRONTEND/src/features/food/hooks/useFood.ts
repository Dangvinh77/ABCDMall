import { useEffect, useState } from "react";
import { getFoods } from "../api/foodApi"; 

export type FoodDto = {
  id?: string | null;
  name: string;
  slug?: string | null;
  description?: string | null;
  imageUrl?: string | null;
};

export const useFood = () => {
  const [foods, setFoods] = useState<FoodDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let active = true;

    const load = async () => {
      try {
        setLoading(true);
        setError(null);

        const data = await getFoods<FoodDto[]>();
        if (!active) {
          return;
        }

        setFoods(Array.isArray(data) ? data : []);
      } catch (err) {
        if (!active) {
          return;
        }

        setError(err instanceof Error ? err.message : "Unable to load food court stores.");
        setFoods([]);
      } finally {
        if (active) {
          setLoading(false);
        }
      }
    };

    void load();

    return () => {
      active = false;
    };
  }, []);

  return { foods, loading, error };
};
