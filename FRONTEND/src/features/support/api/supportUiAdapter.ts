import { supportContent, type SupportContentEntry } from "../data/supportContent";
import type { SupportFaqCollection } from "../types/support";

export function mapSupportFaqs(
  items: SupportContentEntry[],
): SupportFaqCollection {
  const categories: SupportFaqCollection["categories"] = [];
  const itemsByCategory: SupportFaqCollection["itemsByCategory"] = {};

  items.forEach((item) => {
    if (!itemsByCategory[item.categoryKey]) {
      categories.push({ id: item.categoryKey, name: item.categoryLabel });
      itemsByCategory[item.categoryKey] = [];
    }

    itemsByCategory[item.categoryKey].push({
      id: item.id,
      q: item.question,
      a: item.answer,
    });
  });

  return {
    categories,
    itemsByCategory,
    featuredItems: categories
      .flatMap((category) => itemsByCategory[category.id])
      .slice(0, 3),
  };
}

export async function loadSupportFaqs() {
  return mapSupportFaqs(supportContent);
}
