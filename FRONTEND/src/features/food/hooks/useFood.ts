import { useEffect, useState } from "react";
import { getFoods } from "../api/foodApi"; 

export const useFood = () => {
  const [foods, setFoods] = useState<any[]>([]);

  useEffect(() => {
    getFoods().then(setFoods);
  }, []);

  return { foods };
};