import { getImageUrl } from "../../../core/utils/image";

export type FoodListItem = {
  id?: string | null;
  name: string;
  imageUrl?: string | null;
  slug?: string | null;
  description?: string | null;
};

export type FoodCategoryKey =
  | "coffee"
  | "drinks"
  | "seafood"
  | "international"
  | "vietnamese"
  | "japanese"
  | "korean"
  | "lao"
  | "fastfood"
  | "thai"
  | "hotpot"
  | "bbq"
  | "pizza"
  | "chinese"
  | "bakery";

export type FoodMenuItem = {
  name: string;
  price: string;
  note: string;
  tag: string;
  imageUrl: string;
  ingredients: string[];
};

export const CATEGORY_PRESETS: Record<
  FoodCategoryKey,
  {
    label: string;
    subtitle: string;
    hours: string;
    theme: string;
    soft: string;
    badge: string;
    promo: string;
    location: string;
  }
> = {
  coffee: {
    label: "Coffee & Espresso",
    subtitle: "Warm aromas, espresso drinks, and easy grab-and-go moments.",
    hours: "07:00 - 22:00",
    theme: "from-amber-950 via-amber-800 to-amber-600",
    soft: "bg-amber-50",
    badge: "bg-amber-100 text-amber-800",
    promo: "Morning coffee combos are available with pastry add-ons.",
    location: "Coffee zone",
  },
  drinks: {
    label: "Tea & Drinks",
    subtitle: "Milk tea, fruit tea, and refreshing topping combinations.",
    hours: "09:00 - 22:00",
    theme: "from-teal-950 via-teal-700 to-cyan-500",
    soft: "bg-teal-50",
    badge: "bg-teal-100 text-teal-800",
    promo: "Try multiple flavors in one visit with a sharing set.",
    location: "Drink counter",
  },
  seafood: {
    label: "Seafood",
    subtitle: "Fresh seafood, buffet-style servings, and sharing platters.",
    hours: "10:00 - 22:00",
    theme: "from-cyan-950 via-sky-700 to-sky-400",
    soft: "bg-cyan-50",
    badge: "bg-cyan-100 text-cyan-800",
    promo: "Reserve early for the best seafood platters.",
    location: "Seafood zone",
  },
  international: {
    label: "International Comfort Food",
    subtitle: "Popular dishes for lunch groups and easy sharing.",
    hours: "10:00 - 22:00",
    theme: "from-slate-950 via-slate-700 to-slate-500",
    soft: "bg-slate-50",
    badge: "bg-slate-100 text-slate-800",
    promo: "Lunch sets are served during the midday rush.",
    location: "International counter",
  },
  vietnamese: {
    label: "Vietnamese Cuisine",
    subtitle: "Familiar flavors, rice plates, noodle bowls, and herb-forward meals.",
    hours: "09:30 - 22:00",
    theme: "from-orange-950 via-orange-700 to-amber-500",
    soft: "bg-orange-50",
    badge: "bg-orange-100 text-orange-800",
    promo: "Rice and noodle sets are a great lunch option.",
    location: "Vietnamese counter",
  },
  japanese: {
    label: "Japanese Cuisine",
    subtitle: "Sushi, ramen, udon, and set meals with a clean presentation.",
    hours: "10:00 - 22:00",
    theme: "from-blue-950 via-blue-700 to-indigo-500",
    soft: "bg-blue-50",
    badge: "bg-blue-100 text-blue-800",
    promo: "Japanese set menus are ideal for a quick dinner.",
    location: "Japanese counter",
  },
  korean: {
    label: "Korean Cuisine",
    subtitle: "BBQ, fried chicken, and bold spicy flavors for sharing.",
    hours: "10:00 - 22:00",
    theme: "from-pink-950 via-pink-700 to-rose-500",
    soft: "bg-pink-50",
    badge: "bg-pink-100 text-pink-800",
    promo: "Group set menus make ordering faster and easier.",
    location: "Korean zone",
  },
  lao: {
    label: "Lao Cuisine",
    subtitle: "Light, herb-forward dishes with a clean and fresh finish.",
    hours: "10:00 - 21:30",
    theme: "from-emerald-950 via-emerald-700 to-green-500",
    soft: "bg-emerald-50",
    badge: "bg-emerald-100 text-emerald-800",
    promo: "Light sharing sets are recommended for lunch visits.",
    location: "Indochina counter",
  },
  fastfood: {
    label: "Fast Food",
    subtitle: "Fast service, easy ordering, and familiar comfort dishes.",
    hours: "09:00 - 22:00",
    theme: "from-red-950 via-red-700 to-rose-500",
    soft: "bg-red-50",
    badge: "bg-red-100 text-red-800",
    promo: "Grab-and-go combos are perfect before a movie.",
    location: "Fast food counter",
  },
  thai: {
    label: "Thai Cuisine",
    subtitle: "Sweet, sour, salty, and spicy dishes with a bold aroma.",
    hours: "10:00 - 22:00",
    theme: "from-violet-950 via-violet-700 to-fuchsia-500",
    soft: "bg-violet-50",
    badge: "bg-violet-100 text-violet-800",
    promo: "Lunch set menus often give better value.",
    location: "Thai counter",
  },
  hotpot: {
    label: "Hotpot",
    subtitle: "Sharing hotpot for groups, family meals, and casual meetups.",
    hours: "10:00 - 22:00",
    theme: "from-yellow-950 via-orange-700 to-amber-500",
    soft: "bg-yellow-50",
    badge: "bg-yellow-100 text-yellow-800",
    promo: "Choose a group set for a more complete hotpot experience.",
    location: "Hotpot zone",
  },
  bbq: {
    label: "BBQ",
    subtitle: "Grilled meats, smoky flavor, and sets made for sharing.",
    hours: "10:00 - 22:00",
    theme: "from-stone-950 via-stone-700 to-amber-600",
    soft: "bg-amber-50",
    badge: "bg-amber-100 text-amber-800",
    promo: "BBQ sets are the easiest way to sample the signature menu.",
    location: "BBQ zone",
  },
  pizza: {
    label: "Pizza & Pasta",
    subtitle: "Easy sharing, crisp crusts, and familiar Italian-inspired plates.",
    hours: "09:30 - 22:00",
    theme: "from-orange-950 via-orange-700 to-red-500",
    soft: "bg-orange-50",
    badge: "bg-orange-100 text-orange-800",
    promo: "Large pizzas work well for groups and families.",
    location: "Pizza counter",
  },
  chinese: {
    label: "Chinese Cuisine",
    subtitle: "Dim sum, roast dishes, noodles, and classic shared plates.",
    hours: "10:00 - 22:00",
    theme: "from-rose-950 via-rose-700 to-red-500",
    soft: "bg-rose-50",
    badge: "bg-rose-100 text-rose-800",
    promo: "Dim sum sets are recommended in the morning.",
    location: "Chinese zone",
  },
  bakery: {
    label: "Bakery",
    subtitle: "Fresh bread, pastries, and easy snacks for any time of day.",
    hours: "08:00 - 22:00",
    theme: "from-amber-950 via-amber-700 to-yellow-500",
    soft: "bg-amber-50",
    badge: "bg-amber-100 text-amber-800",
    promo: "Bread-and-drink combos are a simple breakfast option.",
    location: "Bakery counter",
  },
};

const STORE_IMAGES: Record<string, string[]> = {
  "babushka-a-la-carte": ["/img/crystaljade/menu.webp", "/img/crystaljade/menu1.webp", "/img/crystaljade/menu2.webp", "/img/crystaljade/menu3.jpg", "/img/crystaljade/menu4.jpg"],
  "bobapop": ["/img/Bobapop/menu.jpg", "/img/Bobapop/menu1.webp", "/img/Bobapop/menu2.png", "/img/Bobapop/menu3.webp", "/img/Bobapop/menu4.jpg"],
  "boba-bella": ["/img/Boba Bella Milk Tea/menu.jpg", "/img/Boba Bella Milk Tea/menu1.jpg", "/img/Boba Bella Milk Tea/menu2.jpg", "/img/Boba Bella Milk Tea/menu3.jpg", "/img/Boba Bella Milk Tea/menu4.jpg"],
  "dookki-vietnam": ["/img/Dookki/menu.jpg", "/img/Dookki/logo1.jpg", "/img/Dookki/logo2.jpeg", "/img/Dookki/menu1.jpg", "/img/Dookki/menu2.webp"],
  "chang-kang-kung": ["/img/Chang Kang Kung/menu.jpg", "/img/Chang Kang Kung/menu1.webp", "/img/Chang Kang Kung/menu2.jpg", "/img/Chang Kang Kung/menu3.jpg", "/img/Chang Kang Kung/menu4.webp"],
  bonchon: ["/img/bonchon/menu.jpg", "/img/bonchon/menu1.jpg", "/img/bonchon/menu2.jpg", "/img/bonchon/menu3.png", "/img/bonchon/menu4.jpg"],
  "chang-modern-thai-cuisine": ["/img/chang/menu.jpg", "/img/chang/menu1.jpg", "/img/chang/menu2.jpg", "/img/chang/menu3.jpg", "/img/chang/menu4.jpg"],
  "chilli-thai": ["/img/Chilli Thai/menu.webp", "/img/Chilli Thai/menu1.webp", "/img/Chilli Thai/menu2.webp", "/img/Chilli Thai/menu3.webp", "/img/Chilli Thai/menu4.jpg"],
  "crystal-jade": ["/img/crystaljade/menu.webp", "/img/crystaljade/menu1.webp", "/img/crystaljade/menu2.webp", "/img/crystaljade/menu3.jpg", "/img/crystaljade/menu4.jpg"],
  "dao-niu-guo": ["/img/daoniuguo/menu.jpg", "/img/daoniuguo/menu1.jpg", "/img/daoniuguo/menu2.jpg", "/img/daoniuguo/menu3.jpg", "/img/daoniuguo/menu4.jpg"],
  "gogi-house": ["/img/gogi/menu.jpg", "/img/gogi/menu1.jpg", "/img/gogi/menu2.jpg", "/img/gogi/menu3.jpg", "/img/gogi/menu4.jpg"],
  "h-bbq-buffet": ["/img/H BBQ Buffet/menu.jpg", "/img/H BBQ Buffet/menu1.jpg", "/img/H BBQ Buffet/menu2.jpg", "/img/H BBQ Buffet/menu3.jpg", "/img/H BBQ Buffet/menu4.jpg"],
  "highlands-coffee": ["/img/2lands/menu.jpg", "/img/2lands/menu1.jpg", "/img/2lands/menu2.webp", "/img/2lands/menu3.jpg", "/img/2lands/menu4.jpg"],
  starbucks: ["/img/starbuck/menu.jpg", "/img/starbuck/menu1.jpg", "/img/starbuck/menu2.jpg", "/img/starbuck/menu3.jpg", "/img/starbuck/menu4.jpg"],
  "hoang-yen-buffet": ["/img/buffethoangyen/menu.jpg", "/img/buffethoangyen/menu1.jpg", "/img/buffethoangyen/menu2.jpg", "/img/buffethoangyen/menu3.jpg", "/img/buffethoangyen/menu4.jpg"],
  "hot-pot-story": ["/img/Hot Pot Story/menu.jpg", "/img/Hot Pot Story/menu1.jpg", "/img/Hot Pot Story/menu2.jpg", "/img/Hot Pot Story/menu3.jpg", "/img/Hot Pot Story/menu4.jpg", "/img/Hot Pot Story/gallery.jpg"],
  joopii: ["/img/Joopii/menu.jpg", "/img/Joopii/menu1.jpg", "/img/Joopii/menu2.jpg", "/img/Joopii/menu3.jpg", "/img/Joopii/menu4.jpg", "/img/Joopii/gallery.jpg"],
  "khao-lao": ["/img/khaolao/menu.jpg", "/img/khaolao/menu1.jpg", "/img/khaolao/menu2.jpg", "/img/khaolao/menu3.jpg", "/img/khaolao/menu4.webp"],
  "kichi-kichi": ["/img/kichi/menu.jpg", "/img/kichi/menu1.jpg", "/img/kichi/menu2.webp", "/img/kichi/menu3.jpg", "/img/kichi/menu4.jpg"],
  "king-bbq": ["/img/King BBQ/menu.jpg", "/img/King BBQ/menu1.jpg", "/img/King BBQ/menu2.webp", "/img/King BBQ/menu3.jpg", "/img/King BBQ/menu4.jpg"],
  "marukame-udon": ["/img/Marukame Udon/menu.webp", "/img/Marukame Udon/menu1.webp", "/img/Marukame Udon/menu2.webp", "/img/Marukame Udon/menu3.webp", "/img/Marukame Udon/menu4.webp"],
  "lok-lok-hotpot": ["/img/Lok Lok Hotpot/menu.jpg", "/img/Lok Lok Hotpot/menu1.jpg", "/img/Lok Lok Hotpot/menu2.jpg", "/img/Lok Lok Hotpot/menu3.jpg", "/img/Lok Lok Hotpot/menu4.jpg", "/img/Lok Lok Hotpot/gallery.jpg"],
  "mei-wei": ["/img/Mei Wei/menu.jpg", "/img/Mei Wei/menu1.png", "/img/Mei Wei/menu2.png", "/img/Mei Wei/menu3.jpg", "/img/Mei Wei/menu4.png"],
  "mikado-sushi": ["/img/Mikado Sushi/menu.jpg", "/img/Mikado Sushi/menu1.jpg", "/img/Mikado Sushi/menu2.jpg", "/img/Mikado Sushi/menu3.jpg", "/img/Mikado Sushi/menu4.webp"],
  "sushi-kei": ["/img/sushikei/menu.webp", "/img/sushikei/menu1.webp", "/img/sushikei/menu2.jpg", "/img/sushikei/menu3.png", "/img/sushikei/menu4.png"],
  "tasaki-bbq": ["/img/King BBQ/menu.jpg", "/img/King BBQ/menu1.jpg", "/img/King BBQ/menu2.webp", "/img/King BBQ/menu3.jpg", "/img/King BBQ/menu4.jpg"],
  sushix: ["/img/Mikado Sushi/menu.jpg", "/img/Mikado Sushi/menu1.jpg", "/img/Mikado Sushi/menu2.jpg", "/img/Mikado Sushi/menu3.jpg", "/img/Mikado Sushi/menu4.webp"],
  thaiexpress: ["/img/thaiexpress/menu.jpg", "/img/thaiexpress/menu1.jpg", "/img/thaiexpress/menu2.jpg", "/img/thaiexpress/menu3.jpg", "/img/thaiexpress/menu4.jpg"],
  "the-pizza-company": ["/img/pizzaCompany/menu.webp", "/img/pizzaCompany/menu1.webp", "/img/pizzaCompany/menu2.jpg", "/img/pizzaCompany/menu3.jpg", "/img/pizzaCompany/menu4.jpg"],
  "texas-chicken": ["/img/texaschicken/menu.webp", "/img/texaschicken/menu1.jpg", "/img/texaschicken/menu2.jpg", "/img/texaschicken/menu3.webp", "/img/texaschicken/menu4.webp"],
  "ocean-blue": ["/img/Lok Lok Hotpot/menu.jpg", "/img/Lok Lok Hotpot/menu1.jpg", "/img/Lok Lok Hotpot/menu2.jpg", "/img/Lok Lok Hotpot/menu3.jpg", "/img/Lok Lok Hotpot/menu4.jpg", "/img/Lok Lok Hotpot/gallery.jpg"],
  "lu-nuong-88": ["/img/Lẩu Nướng 88/menu.jpg", "/img/Lẩu Nướng 88/menu1.jpg", "/img/Lẩu Nướng 88/menu2.jpg", "/img/Lẩu Nướng 88/menu3.jpg", "/img/Lẩu Nướng 88/menu4.jpg"],
  "shabu-ya": ["/img/Shabu Ya/menu.jpg", "/img/Shabu Ya/menu1.jpg", "/img/Shabu Ya/in_1.jpg", "/img/Shabu Ya/in_3.jpg", "/img/Shabu Ya/in.jpg"],
  "tokyo-ramen-station": ["/img/Marukame Udon/menu.webp", "/img/Marukame Udon/menu1.webp", "/img/Marukame Udon/menu2.webp", "/img/Marukame Udon/menu3.webp", "/img/Marukame Udon/menu4.webp"],
  taipan: ["/img/taipan/menu.jpg", "/img/taipan/menu1.jpg", "/img/taipan/menu2.jpg", "/img/taipan/menu3.jpg", "/img/taipan/menu4.jpg"],
  "wow-yakiniku": ["/img/wowyakiniku/menu.jpg", "/img/wowyakiniku/menu2.png", "/img/wowyakiniku/menu3.jpg", "/img/wowyakiniku/menu4.jpg", "/img/wowyakiniku/menu5.jpg"],
  "yamazaki-bakery": ["/img/Yamazaki Bakery/menu.jpg", "/img/Yamazaki Bakery/menu1.jpg", "/img/Yamazaki Bakery/menu2.jpg", "/img/Yamazaki Bakery/in.jpg", "/img/Yamazaki Bakery/in_1.jpg", "/img/Yamazaki Bakery/out.jpg"],
};

const FALLBACK_MENU_NAMES: Record<FoodCategoryKey, string[]> = {
  coffee: ["Reserve Cold Brew", "Signature Latte", "Mocha Cloud", "Butter Croissant"],
  drinks: ["Brown Sugar Milk Tea", "Fruit Tea Splash", "Cheese Foam Cup", "Taro Ice Blend"],
  seafood: ["Lobster Feast", "Oyster Platter", "Grilled Squid Skewer", "Sashimi Tower"],
  international: ["Pasta Aglio Olio", "Baked Rice Set", "Cream Soup Bowl", "Dessert Plate"],
  vietnamese: ["Grilled Pork Rice", "Pho Special Bowl", "Spring Roll Plate", "Herb Plate"],
  japanese: ["Ramen Bowl", "Sushi Roll", "Donburi Set", "Tempura Basket"],
  korean: ["BBQ Set", "Kimchi Stew", "Fried Chicken Box", "Rice & Side Dish"],
  lao: ["Larb Plate", "Herb Noodle", "Steamed Fish", "Sticky Rice Set"],
  fastfood: ["Chicken Combo", "Crunch Burger", "Loaded Fries", "Honey Biscuit"],
  thai: ["Pad Thai", "Green Curry", "Tom Yum Soup", "Sticky Rice Dessert"],
  hotpot: ["Hotpot Combo", "Broth Set", "Meat Platter", "Vegetable Basket"],
  bbq: ["Yakiniku Set", "Marinated Beef", "Pork Belly", "Rice & Soup"],
  pizza: ["Seafood Pizza", "Cheese Pizza", "Chicken Wings", "Pasta Bowl"],
  chinese: ["Dim Sum Basket", "Roast Duck", "Stir-fry Noodles", "Dumpling Soup"],
  bakery: ["Shokupan Loaf", "Cream Bun", "Butter Croissant", "Cheese Tart"],
};

export function titleCase(value: string) {
  return value
    .replace(/[-_]+/g, " ")
    .replace(/\s+/g, " ")
    .trim()
    .replace(/\b\w/g, (char) => char.toUpperCase());
}

export function normalizeFoodCategory(food?: FoodListItem | null): FoodCategoryKey {
  const slug = `${food?.slug ?? ""} ${food?.name ?? ""}`.toLowerCase();

  if (/(coffee|starbuck|highland|cafe|espresso)/.test(slug)) return "coffee";
  if (/(drink|tea|boba|milk tea|juice)/.test(slug)) return "drinks";
  if (/(seafood|ocean|lobster|oyster|fish)/.test(slug)) return "seafood";
  if (/(vietnam|saigon|pho|hoang yen)/.test(slug)) return "vietnamese";
  if (/(japan|sushi|ramen|udon|kei|mikado|marukame)/.test(slug)) return "japanese";
  if (/(korea|gogi|bonchon|dookki|joopii|bbq|yakiniku)/.test(slug)) return "korean";
  if (/(lao|khao lao)/.test(slug)) return "lao";
  if (/(burger|chicken|fast|texas)/.test(slug)) return "fastfood";
  if (/(thai|thaiexpress|chilli|chang)/.test(slug)) return "thai";
  if (/(hotpot|kichi|shabu|lok lok|story)/.test(slug)) return "hotpot";
  if (/(pizza|pasta)/.test(slug)) return "pizza";
  if (/(chinese|dim sum|taipan|crystal jade|mei wei|chang kang kung)/.test(slug)) return "chinese";
  if (/(bakery|yamazaki|bread|pastry|cake)/.test(slug)) return "bakery";
  if (/(bbq|grill|tasaki|king bbq|h bbq|lu nuong|wow)/.test(slug)) return "bbq";

  return "international";
}

export function imageSrc(url?: string | null) {
  return getImageUrl(url ?? "");
}

export function getStoreImages(food?: FoodListItem | null) {
  const slug = food?.slug?.toLowerCase() ?? "";
  return STORE_IMAGES[slug] ?? [];
}

export function buildFoodMenu(food: FoodListItem): FoodMenuItem[] {
  const category = normalizeFoodCategory(food);
  const brand = titleCase(food.name);
  const menuImages = getStoreImages(food);

  if (menuImages.length > 0) {
    return menuImages.map((imageUrl, index) => ({
      name: `${brand} ${FALLBACK_MENU_NAMES[category][index % FALLBACK_MENU_NAMES[category].length]}`,
      price: `${category === "seafood" ? 129 : category === "bbq" ? 119 : 39 + index * 12}K`,
      note: `A house favorite prepared for the ${brand} counter.`,
      tag: index === 0 ? "Signature" : index === 1 ? "Popular" : index === 2 ? "Shareable" : "Fresh Pick",
      imageUrl,
      ingredients: [CATEGORY_PRESETS[category].label, index === 0 ? "Chef recommended" : "Guest favorite", "Fast service"],
    }));
  }

  return FALLBACK_MENU_NAMES[category].map((suffix, index) => ({
    name: `${brand} ${suffix}`,
    price: `${category === "seafood" ? 129 : category === "bbq" ? 119 : 39 + index * 12}K`,
    note: `A house favorite prepared for the ${brand} counter.`,
    tag: index === 0 ? "Signature" : index === 1 ? "Popular" : index === 2 ? "Shareable" : "Fresh Pick",
    imageUrl: imageSrc(food.imageUrl),
    ingredients: [CATEGORY_PRESETS[category].label, index === 0 ? "Chef recommended" : "Guest favorite", "Fast service"],
  }));
}

export function buildFoodGallery(food: FoodListItem) {
  const storeImages = getStoreImages(food);
  if (storeImages.length > 0) {
    return storeImages.map((image) => imageSrc(image));
  }

  const hero = imageSrc(food.imageUrl);
  return hero ? [hero] : [];
}
