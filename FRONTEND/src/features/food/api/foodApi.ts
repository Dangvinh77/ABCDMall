import { api } from "../../../core/api/api";

export const getFoods = () => api.get("/food");

export const createFood = (data: any) => api.post("/food", data);

export const getFoodBySlug = (slug: string) =>
  api.get(`/food/${slug}`);