export interface SupportCategory {
  id: string;
  name: string;
}

export interface SupportFaqItem {
  id: string;
  q: string;
  a: string;
}

export interface SupportFaqCollection {
  categories: SupportCategory[];
  itemsByCategory: Record<string, SupportFaqItem[]>;
  featuredItems: SupportFaqItem[];
}
