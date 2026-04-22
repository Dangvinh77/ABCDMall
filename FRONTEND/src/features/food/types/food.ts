export interface FoodMenuItemDto {
  name: string;
  price?: string;
  note?: string;
  tag?: string;
  imageUrl?: string | null;
  ingredients: string[];
}

export interface FoodItemDto {
  id?: string | number;
  name: string;
  slug?: string | null;
  description?: string | null;
  imageUrl?: string | null;
  categorySlug?: string | null;
}
