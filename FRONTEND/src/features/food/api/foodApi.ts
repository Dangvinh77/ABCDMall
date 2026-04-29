import { api, BASE_URL } from "../../../core/api/api";

export type FoodMenuItem = {
  id?: string;
  name: string;
  price: number;
  note: string;
  tag: string;
  imageUrl: string;
  ingredients: string[];
  isAvailable: boolean;
  displayOrder?: number;
};

export type FoodStall = {
  id?: string;
  name: string;
  imageUrl?: string | null;
  slug?: string | null;
  description?: string | null;
  categorySlug?: string | null;
  location?: string | null;
  openHours?: string | null;
  phone?: string | null;
  promo?: string | null;
  isActive?: boolean;
  menuItems?: FoodMenuItem[];
};

export type AvailableFoodCourtLocation = {
  rentalAreaId?: string;
  locationSlot: string;
  floor: string;
  areaName?: string | null;
};

export type FoodCourtCreationStatus = {
  stallCount: number;
  rentedFoodCourtCount: number;
  canCreate: boolean;
  message: string;
  availableRentalLocations?: AvailableFoodCourtLocation[];
};

export const getFoods = <T = FoodStall[]>() => api.get<T>("/food");

export const getFoodBySlug = <T = FoodStall>(slug: string) => api.get<T>(`/food/slug/${slug}`);

export async function getMyFoodStalls(): Promise<FoodStall[]> {
  return api.get<FoodStall[]>("/food/manager");
}

export async function getMyFoodCourtCreationStatus(): Promise<FoodCourtCreationStatus> {
  return api.get<FoodCourtCreationStatus>("/food/manager/creation-status");
}

export async function createMyFoodStall(request: FormData): Promise<FoodStall> {
  return api.post<FoodStall, FormData>("/food/manager", request);
}

export async function updateMyFoodStall(id: string, request: FormData): Promise<FoodStall> {
  return api.put<FoodStall, FormData>(`/food/manager/${id}`, request);
}

export async function deleteMyFoodStall(id: string): Promise<void> {
  return api.delete<void>(`/food/manager/${id}`);
}

export const createFood = async (data: { name: string; description?: string; imageUrl?: string }, file?: File) => {
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
    body: formData,
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
