import { api } from "../../../core/api/api";

export const getFoods = () => api.get<any[]>("/food");

//export const createFood = (data: any) => api.post("/food", data);

export const getFoodBySlug = (slug: string) =>
                                                 api.get<any>(`/food/slug/${slug}`);

// export const uploadImage = async (file: File) => {
//   const formData = new FormData();
//   formData.append("file", file);

//   const res = await fetch("http://localhost:5184/api/food/upload", {
//     method: "POST",
//     body: formData,
//   });

//   return res.json();
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
    body: formData, // 🔥 KHÔNG set header
  });

  return res.json();

  
};
 
  
