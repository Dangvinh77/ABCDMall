import { useEffect, useMemo, useState } from "react";
import { Link, useParams } from "react-router-dom";
import {
  ArrowLeft,
  BadgeInfo,
  ChevronRight,
  Clock3,
  ExternalLink,
  MapPin,
  Phone,
  Sparkles,
  Star,
  Ticket,
  X,
} from "lucide-react";
import { getFoodBySlug } from "../api/foodApi";
import type { FoodItemDto, FoodMenuItemDto } from "../types/food";
import { getImageUrl } from "../../../core/utils/image";

// const DOOKKI_IMAGES = [
//   "/img/Dookki/logo.png",
//   "/img/Dookki/logo2.jpeg",
//   "/img/Dookki/logo1.jpg",
// ];

// const LOCAL_MENU: Record<string, FoodMenuItemDto[]> = {
//   "dookki-vietnam": DOOKKI_IMAGES.map((img, index) => ({
//     name: `Dookki Vietnam ${index + 1}`,
//     price: `${39 + index * 10}K`,
//     note: "Topokki buffet signature dish.",
//     tag: index === 0 ? "Signature" : "Popular",
//     imageUrl: img,
//     ingredients: ["Korean Cuisine", "Chef recommended", "Hot & spicy"],
//   })),
// };


const STORE_FOLDER_MAP: Record<string, string> = {
  "dookki-vietnam": "Dookki",
  "king-bbq": "KingBBQ",
};
const STORE_IMAGES: Record<string, string[]> = {
  "dookki-vietnam": [
    "/img/Dookki/menu.jpg",
    "/img/Dookki/logo1.jpg",
    "/img/Dookki/logo2.jpeg",
    "/img/Dookki/menu1.jpg",
    "/img/Dookki/menu2.webp",
  ],
};

function generateMenuFromFolder(slug: string): FoodMenuItemDto[] {
  const images = STORE_IMAGES[slug];

  if (!images) return [];

  return images.map((img, i) => ({
    // name: `${slug} item ${i + 1}`,
    //price: `${39 + i * 10}K`,
    note: "Signature dish",
    tag: i === 0 ? "Signature" : "Popular",
    imageUrl: img,
    ingredients: ["Chef recommended"],
  }));
}

const CATEGORY_PRESETS = {
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
} as const;

type CategoryKey = keyof typeof CATEGORY_PRESETS;

function normalizeCategory(food?: FoodItemDto | null): CategoryKey {
  const slug = `${food?.categorySlug ?? ""} ${food?.slug ?? ""} ${food?.name ?? ""}`.toLowerCase();

  if (/(coffee|starbuck|highland|cafe|espresso)/.test(slug)) return "coffee";
  if (/(drink|tea|boba|milk tea|juice)/.test(slug)) return "drinks";
  if (/(seafood|ocean|lobster|oyster|fish)/.test(slug)) return "seafood";
  if (/(vietnam|saigon|pho|grill express|hoang yen)/.test(slug)) return "vietnamese";
  if (/(japan|sushi|ramen|udon|kei|mikado|marukame)/.test(slug)) return "japanese";
  if (/(korea|gogi|bonchon|dookki|joopii|bbq|yakiniku|subin)/.test(slug)) return "korean";
  if (/(lao|khao lao)/.test(slug)) return "lao";
  if (/(burger|chicken|fast|texas)/.test(slug)) return "fastfood";
  if (/(thai|thaiexpress|chilli|chang modern)/.test(slug)) return "thai";
  if (/(hotpot|kichi|shabu|lok lok|story)/.test(slug)) return "hotpot";
  if (/(pizza|pasta)/.test(slug)) return "pizza";
  if (/(chinese|dim sum|taipan|crystal jade|mei wei|chang kang kung)/.test(slug)) return "chinese";
  if (/(bakery|yamazaki|bread|pastry|cake)/.test(slug)) return "bakery";
  if (/(bbq|grill|tasaki|king bbq|h bbq|lu nuong|wow)/.test(slug)) return "bbq";

  return "international";
}

function titleCase(value: string) {
  return value
    .replace(/[-_]+/g, " ")
    .replace(/\s+/g, " ")
    .trim()
    .replace(/\b\w/g, (char) => char.toUpperCase());
}

function seededImage(seed: string, width = 1200, height = 900) {
  return `https://picsum.photos/seed/${encodeURIComponent(seed)}/${width}/${height}`;
}

function getRating(seed?: string | null) {
  const value = (seed ?? "").length;
  return (4.3 + (value % 7) * 0.1).toFixed(1);
}

function getReviewCount(seed?: string | null) {
  return 80 + (seed ?? "").length * 9;
}

function fallbackGallery(food: FoodItemDto, category: CategoryKey) {
  const slug = (food.slug ?? food.name).toLowerCase().replace(/\s+/g, "-");
  const cover = food.imageUrl ? getImageUrl(food.imageUrl) : seededImage(`${slug}-cover`, 1600, 1000);
  return [
    cover,
    seededImage(`${slug}-${category}-1`, 1200, 900),
    seededImage(`${slug}-${category}-2`, 1200, 900),
    seededImage(`${slug}-${category}-3`, 1200, 900),
  ];
}

function fallbackMenu(food: FoodItemDto, category: CategoryKey): FoodMenuItemDto[] {
  const brand = titleCase(food.name);
  const slug = (food.slug ?? food.name).toLowerCase().replace(/\s+/g, "-");
  const baseNames: Record<CategoryKey, string[]> = {
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

  return baseNames[category].map((suffix, index) => ({
    name: `${brand} ${suffix}`,
    price: `${category === "seafood" ? 129 : category === "bbq" ? 119 : 39 + index * 12}K`,
    note: `A house favorite prepared for the ${brand} counter.`,
    tag: index === 0 ? "Signature" : index === 1 ? "Popular" : index === 2 ? "Shareable" : "Fresh Pick",
    imageUrl: seededImage(`${slug}-menu-${index + 1}`, 1200, 900),
    ingredients: [CATEGORY_PRESETS[category].label, index === 0 ? "Chef recommended" : "Guest favorite", "Fast service"],
  }));
}

function imageSrc(url?: string | null) {
  return getImageUrl(url ?? "");
}

function MenuModal({
  open,
  onClose,
  items,
  onPreview
}: {
  open: boolean;
  onClose: () => void;
  items: FoodMenuItemDto[];
  onPreview: (img: string) => void;
}) {
  const [activeIndex, setActiveIndex] = useState(0);

  useEffect(() => {
    if (open) setActiveIndex(0);
  }, [open]);

  if (!open) return null;

  const active = items[activeIndex] ?? items[0];

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 p-4">
      <div className="max-h-[88vh] w-full max-w-5xl overflow-hidden rounded-[28px] bg-white shadow-2xl">
        <div className="flex items-center justify-between border-b border-gray-100 px-6 py-4">
          <div>
            <p className="text-xs font-semibold uppercase tracking-[0.24em] text-gray-400">Menu preview</p>
            <h3 className="mt-1 text-xl font-bold text-gray-900">Signature dishes and drinks</h3>
          </div>
          <button
            type="button"
            onClick={onClose}
            className="rounded-full bg-gray-100 p-2 text-gray-600 transition hover:bg-gray-200 hover:text-gray-900"
          >
            <X className="h-5 w-5" />
          </button>
        </div>

        <div className="grid gap-0 md:grid-cols-[1.1fr_0.9fr]">
          <div className="max-h-[calc(88vh-73px)] overflow-auto p-5">
            <div className="grid gap-4 sm:grid-cols-2">
              {items.map((item, index) => (
                <button
                  key={`${item.name}-${index}`}
                  type="button"
                  onClick={() => setActiveIndex(index)}
                  className={`overflow-hidden rounded-3xl border text-left transition ${index === activeIndex ? "border-gray-900 shadow-lg" : "border-gray-200 hover:border-gray-300"
                    }`}
                >
                  <img src={imageSrc(item.imageUrl)} alt={item.name} className="h-40 w-full object-cover" onClick={() => onPreview(imageSrc(item.imageUrl))} />
                  <div className="p-4">
                    <div className="flex items-start justify-between gap-3">
                      <h4 className="font-semibold text-gray-900">{item.name}</h4>
                      <span className="rounded-full bg-gray-100 px-2.5 py-1 text-xs font-semibold text-gray-700">
                        {item.price}
                      </span>
                    </div>
                    <p className="mt-2 line-clamp-2 text-sm leading-6 text-gray-500">{item.note}</p>
                  </div>
                </button>
              ))}
            </div>
          </div>

          <div className="border-t border-gray-100 bg-gray-50 p-6 md:border-l md:border-t-0">
            {active ? (
              <>
                <img src={imageSrc(active.imageUrl)} alt={active.name} className="h-72 w-full rounded-3xl object-cover" onClick={() => onPreview(imageSrc(active.imageUrl))} />
                <div className="mt-5">
                  <div className="flex items-start justify-between gap-4">
                    <div>
                      <p className="text-xs font-semibold uppercase tracking-[0.24em] text-gray-400">Featured item</p>
                      <h4 className="mt-1 text-2xl font-bold text-gray-900">{active.name}</h4>
                    </div>
                    <span className="rounded-full bg-white px-3 py-1 text-sm font-semibold text-gray-900 shadow-sm">
                      {active.price}
                    </span>
                  </div>
                  <p className="mt-4 text-sm leading-7 text-gray-600">{active.note}</p>
                  <div className="mt-4 flex flex-wrap gap-2">
                    {active.ingredients.map((ingredient) => (
                      <span key={ingredient} className="rounded-full bg-white px-3 py-1 text-xs font-semibold text-gray-700 shadow-sm">
                        {ingredient}
                      </span>
                    ))}
                  </div>
                </div>
              </>
            ) : null}
          </div>
        </div>
      </div>
    </div>
  );
}

export default function FoodDetailPage() {
  const { slug } = useParams();

  const [food, setFood] = useState<FoodItemDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [menuOpen, setMenuOpen] = useState(false);
  const [previewImage, setPreviewImage] = useState<string | null>(null);


  useEffect(() => {
    let active = true;


    const load = async () => {
      if (!slug) {
        if (active) {
          setError("Missing restaurant slug.");
          setLoading(false);
        }
        return;
      }

      try {
        setLoading(true);
        const result = await getFoodBySlug(slug);
        if (!active) return;
        setFood(result);
        setError(null);
      } catch (err) {
        if (!active) return;
        setError(err instanceof Error ? err.message : "Unable to load restaurant details.");
      } finally {
        if (active) setLoading(false);
      }
    };

    void load();


    return () => {
      active = false;
    };
  }, [slug]);

  const category = useMemo(() => normalizeCategory(food), [food]);
  const preset = CATEGORY_PRESETS[category];
  const brand = food ? titleCase(food.name) : "Restaurant";
  // const gallery = useMemo(() => {
 
  //   return generateMenuFromFolder(slug);
  // }, [food]);

  // const gallery = useMemo(() => {
  //   if (!menu || menu.length === 0) return [];

  //   return menu
  //     .map((item) => item.imageUrl)
  //     .filter(Boolean); // tránh null
  // }, [menu]);

  const menu = useMemo(() => {
    if (!food) return [];
    const slug = food.slug?.toLowerCase() ?? "";
    return generateMenuFromFolder(slug);
  }, [food]);

  const gallery = useMemo(() => {
    if (!menu || menu.length === 0) return [];

    return menu
      .slice(0, 4)
      .map((item) => item.imageUrl)
      .filter(Boolean);
  }, [menu]);


  const heroImage = imageSrc(food?.imageUrl ?? gallery[0] ?? "");

  if (loading) {
    return <div className="flex min-h-[50vh] items-center justify-center text-gray-500">Loading restaurant details...</div>;
  }

  if (error || !food) {
    return (
      <div className="mx-auto flex min-h-[50vh] max-w-3xl flex-col items-center justify-center px-6 text-center">
        <p className="text-2xl font-bold text-gray-900">Restaurant not found</p>
        <p className="mt-2 text-gray-500">{error ?? "The requested store could not be loaded."}</p>
        <Link to="/food-court" className="mt-6 inline-flex items-center gap-2 rounded-full bg-gray-900 px-5 py-3 text-sm font-semibold text-white">
          <ArrowLeft className="h-4 w-4" />
          Back to food court
        </Link>
      </div>
    );
  }

  const quickFacts = [
    { label: "Category", value: preset.label, icon: BadgeInfo },
    { label: "Opening hours", value: preset.hours, icon: Clock3 },
    { label: "Rating", value: `${getRating(food.slug ?? food.name)} / 5 • ${getReviewCount(food.slug ?? food.name)}+ reviews`, icon: Star },
    { label: "Location", value: preset.location, icon: MapPin },
    { label: "Contact", value: "Store hotline / counter", icon: Phone },
  ];

  return (
    <div className="bg-gray-50 pb-16 text-gray-900">
      <div className={`bg-gradient-to-r ${preset.theme} text-white`}>
        <div className="mx-auto max-w-7xl px-6 py-5">
          <div className="flex flex-wrap items-center gap-2 text-sm text-white/80">
            <Link to="/food-court" className="hover:text-white">
              Food Court
            </Link>
            <ChevronRight className="h-4 w-4" />
            <span>{brand}</span>
          </div>
        </div>

        <div className="mx-auto grid max-w-7xl gap-12 px-6 pb-8 pt-8 lg:grid-cols-[2.25fr_0.9fr] lg:items-end">
          <div>
            <span className={`inline-flex rounded-full ${preset.badge} px-4 py-2 text-xs font-semibold uppercase tracking-[0.24em]`}>
              Featured store
            </span>
            <h1 className="mt-5 text-4xl font-bold tracking-tight md:text-5xl">{brand}</h1>
            <p className="mt-4 max-w-2xl text-base leading-7 text-white/85">{food.description || preset.subtitle}</p>
            <div className="mt-6 flex flex-wrap gap-3">
              <button
                type="button"
                onClick={() => setMenuOpen(true)}
                className="inline-flex items-center gap-2 rounded-full bg-white px-5 py-3 text-sm font-semibold text-gray-900 shadow-lg transition hover:-translate-y-0.5"
              >
                View menu
                <ExternalLink className="h-4 w-4" />
              </button>
              <Link
                to="/food-court"
                className="inline-flex items-center gap-2 rounded-full border border-white/25 bg-white/10 px-5 py-3 text-sm font-semibold text-white transition hover:bg-white/15"
              >
                <ArrowLeft className="h-4 w-4" />
                Back to list
              </Link>
            </div>
          </div>

          <div className="rounded-[28px] bg-white/12 p-3 backdrop-blur">
            <img src={heroImage} alt={brand} className="h-[320px] w-full rounded-[22px] object-cover shadow-2xl" />
          </div>
        </div>
      </div>

      <div className="mx-auto mt-8 grid max-w-7xl gap-8 px-6 lg:grid-cols-[1.4fr_0.9fr]">
        <div className="space-y-8">
          <section className="rounded-[28px] border border-gray-200 bg-white p-6 shadow-sm">
            <div className="flex items-center gap-3">
              <div className={`rounded-2xl p-3 ${preset.soft}`}>
                <Sparkles className="h-5 w-5 text-gray-900" />
              </div>
              <div>
                <p className="text-xs font-semibold uppercase tracking-[0.24em] text-gray-400">About the store</p>
                <h2 className="mt-1 text-2xl font-bold">A dedicated food court destination</h2>
              </div>
            </div>
            <p className="mt-5 max-w-3xl text-sm leading-7 text-gray-600">
              {food.description || `${brand} brings a distinct dining style to the Food Court.`}
            </p>
            <p className="mt-4 max-w-3xl text-sm leading-7 text-gray-600">{preset.subtitle}</p>
          </section>

          <section className="rounded-[28px] border border-gray-200 bg-white p-6 shadow-sm">
            <div className="flex items-center justify-between gap-3">
              <div>
                <p className="text-xs font-semibold uppercase tracking-[0.24em] text-gray-400">Menu</p>
                <h2 className="mt-1 text-2xl font-bold">Main items and signature dishes</h2>
              </div>
              <button
                type="button"
                onClick={() => setMenuOpen(true)}
                className="inline-flex items-center gap-2 rounded-full bg-gray-900 px-4 py-2 text-sm font-semibold text-white transition hover:bg-gray-800"
              >
                View menu
                <ExternalLink className="h-4 w-4" />
              </button>
            </div>

            <div className="mt-5 grid gap-4 md:grid-cols-2">
              {menu.slice(0, 4).map((item, index) => (
                <div key={`${item.name}-${index}`} className="overflow-hidden rounded-3xl border border-gray-200 bg-gray-50">
                  <img src={imageSrc(item.imageUrl)} alt={item.name} className="h-44 w-full object-cover" onClick={() => setPreviewImage(imageSrc(item.imageUrl))} />
                  <div className="p-4">
                    <div className="flex items-start justify-between gap-3">
                      <div>
                        <h3 className="font-semibold text-gray-900">{item.name}</h3>
                        <p className="mt-1 text-xs font-semibold uppercase tracking-[0.2em] text-gray-400">{item.tag}</p>
                      </div>
                      <span className="rounded-full bg-white px-3 py-1 text-sm font-semibold text-gray-900 shadow-sm">
                        {item.price}
                      </span>
                    </div>
                    <p className="mt-3 text-sm leading-6 text-gray-600">{item.note}</p>
                  </div>
                </div>
              ))}
            </div>
          </section>

          <section className="rounded-[28px] border border-gray-200 bg-white p-6 shadow-sm">
            <div className="flex items-center justify-between gap-3">
              <div>
                <p className="text-xs font-semibold uppercase tracking-[0.24em] text-gray-400">Gallery</p>
                <h2 className="mt-1 text-2xl font-bold">Photos for this restaurant</h2>
              </div>
              <span className="rounded-full bg-gray-100 px-3 py-1 text-xs font-semibold text-gray-600">
                {gallery.length} images
              </span>
            </div>

            <div className="mt-5 grid gap-3 md:grid-cols-2 xl:grid-cols-4">
              {gallery.map((image, index) => (
                <img
                  key={`${image}-${index}`}
                  src={imageSrc(image)}
                  alt={`${brand} gallery ${index + 1}`}
                  className="h-44 w-full rounded-3xl object-cover"
                  onClick={() => setPreviewImage(imageSrc(image))}

                />
              ))}
            </div>
          </section>


        </div>

        <aside className="space-y-8">
          <section className="rounded-[28px] border border-gray-200 bg-white p-6 shadow-sm">
            <div className="flex items-center gap-4">
              <div className={`flex h-14 w-14 items-center justify-center rounded-2xl bg-gradient-to-r ${preset.theme} text-white shadow-lg`}>
                <Ticket className="h-6 w-6" />
              </div>
              <div>
                <p className="text-xs font-semibold uppercase tracking-[0.24em] text-gray-400">Featured offer</p>
                <h3 className="mt-1 text-xl font-bold text-gray-900">Highlight of the day</h3>
              </div>
            </div>
            <p className="mt-4 text-sm leading-7 text-gray-600">{preset.promo}</p>

            <div className="mt-5 grid gap-3 sm:grid-cols-2 lg:grid-cols-1">
              {quickFacts.map((fact) => {
                const Icon = fact.icon;
                return (
                  <div key={fact.label} className={`rounded-2xl border border-gray-200 ${preset.soft} p-4`}>
                    <div className="flex items-start gap-3">
                      <div className="rounded-xl bg-white p-2 shadow-sm">
                        <Icon className="h-4 w-4 text-gray-700" />
                      </div>
                      <div>
                        <p className="text-xs font-semibold uppercase tracking-[0.18em] text-gray-400">{fact.label}</p>
                        <p className="mt-1 text-sm font-semibold text-gray-900">{fact.value}</p>
                      </div>
                    </div>
                  </div>
                );
              })}
            </div>
          </section>

          <section className="rounded-[28px] border border-gray-200 bg-white p-6 shadow-sm">
            <p className="text-xs font-semibold uppercase tracking-[0.24em] text-gray-400">Store details</p>
            <h3 className="mt-2 text-2xl font-bold text-gray-900">{brand}</h3>
            <p className="mt-3 text-sm leading-7 text-gray-600">{preset.subtitle}</p>
            <div className="mt-5 space-y-3 text-sm text-gray-700">
              <div className="flex items-center gap-3">
                <MapPin className="h-4 w-4 text-gray-400" />
                <span>ABCD Mall • Food Court</span>
              </div>
              <div className="flex items-center gap-3">
                <Clock3 className="h-4 w-4 text-gray-400" />
                <span>{preset.hours}</span>
              </div>
              <div className="flex items-center gap-3">
                <Phone className="h-4 w-4 text-gray-400" />
                <span>Contact counter / store hotline</span>
              </div>
            </div>
          </section>

          <section className="rounded-[28px] border border-gray-200 bg-white p-6 shadow-sm">
            <p className="text-xs font-semibold uppercase tracking-[0.24em] text-gray-400">Visit tips</p>
            <div className="mt-4 space-y-3">
              <div className="rounded-2xl bg-gray-50 p-4">
                <p className="font-semibold text-gray-900">Best for groups</p>
                <p className="mt-1 text-sm leading-6 text-gray-600">Shared plates and set meals are easier to order during peak hours.</p>
              </div>
              <div className="rounded-2xl bg-gray-50 p-4">
                <p className="font-semibold text-gray-900">Menu preview</p>
                <p className="mt-1 text-sm leading-6 text-gray-600">Open the menu modal to compare dishes quickly before heading in.</p>
              </div>
            </div>
          </section>
        </aside>
      </div>

      <MenuModal
        open={menuOpen}
        onClose={() => setMenuOpen(false)}
        items={menu}
        onPreview={(img) => setPreviewImage(img)} // ✅ thêm dòng này
      />


      {previewImage && (
        <div
          className="fixed inset-0 bg-black/80 flex items-center justify-center z-50"
          onClick={() => setPreviewImage(null)}
        >
          <img
            src={previewImage}
            className="max-h-[90%] max-w-[90%] rounded-xl"
            onClick={(e) => e.stopPropagation()}
          />
        </div>
      )}

    </div>




  );
}

