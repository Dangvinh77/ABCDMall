import { api, BASE_URL } from "../../../core/api/api";

export interface CreateFoodRequest {
  name: string;
  description?: string;
  imageUrl?: string;
}

export const getFoods = <T = unknown>() => api.get<T>("/food");

export const getFoodBySlug = <T = unknown>(slug: string) => api.get<T>(`/food/slug/${slug}`);

export const createFood = async (data: CreateFoodRequest, file?: File) => {
  const formData = new FormData();

  formData.append("name", data.name);
  formData.append("description", data.description || "");

  if (file) {
    formData.append("file", file);
  } else if (data.imageUrl) {
    formData.append("imageUrl", data.imageUrl);
  }

  const res = await fetch(`${BASE_URL}/food`, {
    method: "POST",
    body: formData, // 🔥 KHÔNG set header
  });

  return res.json();

  
};

export const uploadFoodImage = async (file: File) => {
  const formData = new FormData();
  formData.append("file", file);

  const res = await fetch(`${BASE_URL}/food/upload`, {
    method: "POST",
    body: formData,
  });

  return res.json();
};
 
  
