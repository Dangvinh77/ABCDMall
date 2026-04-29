import { useEffect, useState } from "react";

export type MenuItem = {
  name: string;
  price: string;
  note: string;
  imageUrl?: string;
};

export function useLocalMenu(slug: string) {
  const [menu, setMenu] = useState<MenuItem[]>([]);

  useEffect(() => {
    const data = localStorage.getItem(`menu-${slug}`);
    if (data) setMenu(JSON.parse(data));
  }, [slug]);

  const saveMenu = (newMenu: MenuItem[]) => {
    setMenu(newMenu);
    localStorage.setItem(`menu-${slug}`, JSON.stringify(newMenu));
  };

  return { menu, saveMenu };
}