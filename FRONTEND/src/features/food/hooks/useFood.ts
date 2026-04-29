import { useEffect, useState } from "react";
import { getFoods } from "../api/foodApi"; 

export interface Food {
  id: string;
  name: string;
  slug: string;
  description: string;
  imageUrl: string;
  price?: number;
  tags?: string[];
  [key: string]: any;
}

export const useFood = () => {
  const [foods, setFoods] = useState<Food[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let active = true;

    const fetchFoods = async () => {
      setLoading(true);
      setError(null);
      try {
        const data = await getFoods();
        if (active) {
          setFoods(Array.isArray(data) ? data : []);
        }
      } catch (err: unknown) {
        if (active) {
          const errorMsg = err instanceof Error ? err.message : "Không thể tải danh sách thực phẩm";
          setError(errorMsg);
          console.error("Lỗi khi tải thực phẩm:", err);
        }
      } finally {
        if (active) {
          setLoading(false);
        }
      }
    };

    fetchFoods();

    return () => {
      active = false;
    };
  }, []);

  return { foods, loading, error };
};
