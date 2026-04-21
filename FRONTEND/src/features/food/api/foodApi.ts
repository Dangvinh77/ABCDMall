import { api } from "../../../core/api/api";
import type { FoodItemDto } from "../types/food";

// GET
export const getFoods = () => api.get("/food");

export const getFoodBySlug = (slug: string) =>
  api.get<FoodItemDto>(`/food/slug/${slug}`);

// CREATE
export const createFood = async (data: any, file?: File) => {
  const formData = new FormData();

  formData.append("name", data.name);
  formData.append("description", data.description || "");

  if (file) {
    formData.append("file", file);
  } else if (data.imageUrl) {
    formData.append("imageUrl", data.imageUrl);
  }

  const res = await fetch("http://localhost:5184/api/food", {
    method: "POST",
    body: formData, 
  });

  return res.json();
};

// UPLOAD riêng (nếu cần)
export const uploadFoodImage = async (file: File) => {
  const formData = new FormData();
  formData.append("file", file);

  const res = await fetch("http://localhost:5184/api/food/upload", {
    method: "POST",
    body: formData,
  });

  return res.json(); // { imageUrl: "/images/foodcourt/xxx.png" }
};
