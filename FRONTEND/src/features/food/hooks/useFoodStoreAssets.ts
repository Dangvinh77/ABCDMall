import { useCallback, useEffect, useMemo, useState } from "react";

export type FoodStoreMenuItem = {
  name: string;
  price: string;
  note: string;
  tag: string;
  imageUrl: string;
  ingredients: string[];
};

export type FoodStoreAssets = {
  coverImageUrl: string;
  galleryImageUrls: string[];
  menuItems: FoodStoreMenuItem[];
};

type FoodStoreAssetsInput = Partial<FoodStoreAssets> | null | undefined;

const STORAGE_PREFIX = "abcdmall:food-store-assets:";

const normalizeAssets = (
  input: FoodStoreAssetsInput,
  fallback?: FoodStoreAssetsInput,
): FoodStoreAssets => ({
  coverImageUrl: input?.coverImageUrl ?? fallback?.coverImageUrl ?? "",
  galleryImageUrls: input?.galleryImageUrls?.filter(Boolean) ?? fallback?.galleryImageUrls?.filter(Boolean) ?? [],
  menuItems: input?.menuItems?.filter(Boolean) ?? fallback?.menuItems?.filter(Boolean) ?? [],
});

const readStorage = (slug: string) => {
  if (typeof window === "undefined") return null;

  try {
    const raw = window.localStorage.getItem(`${STORAGE_PREFIX}${slug}`);
    if (!raw) return null;
    return JSON.parse(raw) as FoodStoreAssetsInput;
  } catch {
    return null;
  }
};

const writeStorage = (slug: string, assets: FoodStoreAssets) => {
  if (typeof window === "undefined") return;
  window.localStorage.setItem(`${STORAGE_PREFIX}${slug}`, JSON.stringify(assets));
};

const removeStorage = (slug: string) => {
  if (typeof window === "undefined") return;
  window.localStorage.removeItem(`${STORAGE_PREFIX}${slug}`);
};

const readFileAsDataUrl = (file: File) =>
  new Promise<string>((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = () => resolve(String(reader.result ?? ""));
    reader.onerror = () => reject(reader.error ?? new Error("Unable to read file."));
    reader.readAsDataURL(file);
  });

export function useFoodStoreAssets(slug?: string | null, fallback?: FoodStoreAssetsInput) {
  const [storedAssets, setStoredAssets] = useState<FoodStoreAssetsInput>(null);

  useEffect(() => {
    if (!slug) {
      setStoredAssets(fallback ?? null);
      return;
    }

    setStoredAssets(readStorage(slug) ?? fallback ?? null);
  }, [slug, fallback]);

  useEffect(() => {
    if (!slug) return;

    const normalized = normalizeAssets(storedAssets, fallback);
    writeStorage(slug, normalized);
  }, [slug, storedAssets, fallback]);

  const assets = useMemo(() => normalizeAssets(storedAssets, fallback), [storedAssets, fallback]);

  const updateAssets = useCallback(
    (updater: (current: FoodStoreAssets) => FoodStoreAssets) => {
      setStoredAssets((current) => updater(normalizeAssets(current, fallback)));
    },
    [fallback],
  );

  const setCoverImageFromFile = useCallback(
    async (file: File) => {
      const dataUrl = await readFileAsDataUrl(file);
      updateAssets((current) => ({
        ...current,
        coverImageUrl: dataUrl,
      }));
    },
    [updateAssets],
  );

  const appendGalleryImages = useCallback(
    async (files: FileList | File[]) => {
      const uploads = await Promise.all(Array.from(files).map((file) => readFileAsDataUrl(file)));
      updateAssets((current) => ({
        ...current,
        galleryImageUrls: [...current.galleryImageUrls, ...uploads],
      }));
    },
    [updateAssets],
  );

  const removeGalleryImage = useCallback(
    (index: number) => {
      updateAssets((current) => ({
        ...current,
        galleryImageUrls: current.galleryImageUrls.filter((_, currentIndex) => currentIndex !== index),
      }));
    },
    [updateAssets],
  );

  const addMenuItem = useCallback(
    async (item: Omit<FoodStoreMenuItem, "imageUrl"> & { imageFile?: File | null }) => {
      let imageUrl = "";
      if (item.imageFile) {
        imageUrl = await readFileAsDataUrl(item.imageFile);
      }

      updateAssets((current) => ({
        ...current,
        menuItems: [
          ...current.menuItems,
          {
            name: item.name.trim(),
            price: item.price.trim(),
            note: item.note.trim(),
            tag: item.tag.trim(),
            imageUrl,
            ingredients: item.ingredients,
          },
        ],
      }));
    },
    [updateAssets],
  );

  const replaceMenuImage = useCallback(
    async (index: number, file: File) => {
      const imageUrl = await readFileAsDataUrl(file);
      updateAssets((current) => ({
        ...current,
        menuItems: current.menuItems.map((item, currentIndex) =>
          currentIndex === index ? { ...item, imageUrl } : item,
        ),
      }));
    },
    [updateAssets],
  );

  const removeMenuItem = useCallback(
    (index: number) => {
      updateAssets((current) => ({
        ...current,
        menuItems: current.menuItems.filter((_, currentIndex) => currentIndex !== index),
      }));
    },
    [updateAssets],
  );

  const resetAssets = useCallback(() => {
    if (!slug) {
      setStoredAssets(fallback ?? null);
      return;
    }

    removeStorage(slug);
    setStoredAssets(fallback ?? null);
  }, [fallback, slug]);

  return {
    assets,
    setCoverImageFromFile,
    appendGalleryImages,
    removeGalleryImage,
    addMenuItem,
    replaceMenuImage,
    removeMenuItem,
    resetAssets,
  };
}
