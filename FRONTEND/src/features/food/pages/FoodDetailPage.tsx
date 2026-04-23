import { useEffect, useMemo, useState } from "react";
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
import { Link, useParams } from "react-router-dom";
import { getFoodBySlug } from "../api/foodApi";
import {
  buildFoodGallery,
  buildFoodMenu,
  CATEGORY_PRESETS,
  imageSrc,
  normalizeFoodCategory,
  titleCase,
  type FoodListItem,
  type FoodMenuItem,
} from "../data/foodStoreMedia";

function getRating(seed?: string | null) {
  const value = (seed ?? "").length;
  return (4.3 + (value % 7) * 0.1).toFixed(1);
}

function getReviewCount(seed?: string | null) {
  return 80 + (seed ?? "").length * 9;
}

function MenuModal({
  open,
  onClose,
  items,
  onPreview,
}: {
  open: boolean;
  onClose: () => void;
  items: FoodMenuItem[];
  onPreview: (img: string) => void;
}) {
  const [activeIndex, setActiveIndex] = useState(0);

  useEffect(() => {
    if (open) setActiveIndex(0);
  }, [open]);

  if (!open) return null;

  const active = items[activeIndex] ?? items[0];

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 p-4" onClick={onClose}>
      <div
        className="max-h-[88vh] w-full max-w-5xl overflow-hidden rounded-[28px] bg-white shadow-2xl"
        onClick={(event) => event.stopPropagation()}
      >
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
                  className={`overflow-hidden rounded-3xl border text-left transition ${
                    index === activeIndex ? "border-gray-900 shadow-lg" : "border-gray-200 hover:border-gray-300"
                  }`}
                >
                  <img
                    src={imageSrc(item.imageUrl)}
                    alt={item.name}
                    className="h-40 w-full object-cover"
                    onClick={() => onPreview(imageSrc(item.imageUrl))}
                  />
                  <div className="p-4">
                    <div className="flex items-start justify-between gap-3">
                      <h4 className="font-semibold text-gray-900">{item.name}</h4>
                      <span className="rounded-full bg-gray-100 px-2.5 py-1 text-xs font-semibold text-gray-700">{item.price}</span>
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
                <img
                  src={imageSrc(active.imageUrl)}
                  alt={active.name}
                  className="h-72 w-full rounded-3xl object-cover"
                  onClick={() => onPreview(imageSrc(active.imageUrl))}
                />
                <div className="mt-5">
                  <div className="flex items-start justify-between gap-4">
                    <div>
                      <p className="text-xs font-semibold uppercase tracking-[0.24em] text-gray-400">Featured item</p>
                      <h4 className="mt-1 text-2xl font-bold text-gray-900">{active.name}</h4>
                    </div>
                    <span className="rounded-full bg-white px-3 py-1 text-sm font-semibold text-gray-900 shadow-sm">{active.price}</span>
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
  const [food, setFood] = useState<FoodListItem | null>(null);
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
        const result = await getFoodBySlug<FoodListItem>(slug);
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

  useEffect(() => {
    if (!food) return;

    document.title = `${food.name} | ABCD Mall`;
    const meta = document.querySelector("meta[name='description']");
    if (meta) {
      meta.setAttribute("content", food.description ?? "");
    }
  }, [food]);

  const category = useMemo(() => normalizeFoodCategory(food), [food]);
  const preset = CATEGORY_PRESETS[category];
  const menu = useMemo(() => (food ? buildFoodMenu(food) : []), [food]);
  const gallery = useMemo(() => (food ? buildFoodGallery(food) : []), [food]);
  const heroImage = imageSrc(food?.imageUrl) || gallery[0] || "";
  const brand = food ? titleCase(food.name) : "Restaurant";

  if (loading) {
    return (
      <div className="flex min-h-[50vh] items-center justify-center bg-gray-50">
        <div className="text-center">
          <div className="mx-auto h-10 w-10 animate-spin rounded-full border-4 border-gray-200 border-t-red-500" />
          <p className="mt-4 text-sm font-medium text-gray-500">Loading restaurant details...</p>
        </div>
      </div>
    );
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

          <section id="menu" className="rounded-[28px] border border-gray-200 bg-white p-6 shadow-sm">
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
                <button
                  key={`${item.name}-${index}`}
                  type="button"
                  onClick={() => setPreviewImage(imageSrc(item.imageUrl))}
                  className="overflow-hidden rounded-3xl border border-gray-200 bg-gray-50 text-left transition hover:shadow-lg"
                >
                  <img src={imageSrc(item.imageUrl)} alt={item.name} className="h-44 w-full object-cover" />
                  <div className="p-4">
                    <div className="flex items-start justify-between gap-3">
                      <div>
                        <h3 className="font-semibold text-gray-900">{item.name}</h3>
                        <p className="mt-1 text-xs font-semibold uppercase tracking-[0.2em] text-gray-400">{item.tag}</p>
                      </div>
                      <span className="rounded-full bg-white px-3 py-1 text-xs font-semibold text-gray-700 shadow-sm">{item.price}</span>
                    </div>
                    <p className="mt-3 text-sm leading-6 text-gray-600">{item.note}</p>
                  </div>
                </button>
              ))}
            </div>
          </section>

          <section className="rounded-[28px] border border-gray-200 bg-white p-6 shadow-sm">
            <div className="flex items-center justify-between gap-3">
              <div>
                <p className="text-xs font-semibold uppercase tracking-[0.24em] text-gray-400">Gallery</p>
                <h2 className="mt-1 text-2xl font-bold">Photos for this restaurant</h2>
              </div>
              <span className="rounded-full bg-gray-100 px-3 py-1 text-xs font-semibold text-gray-600">{gallery.length} images</span>
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
        onPreview={(img) => setPreviewImage(img)}
      />

      {previewImage && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/80" onClick={() => setPreviewImage(null)}>
          <img src={previewImage} alt={brand} className="max-h-[90%] max-w-[90%] rounded-xl" onClick={(e) => e.stopPropagation()} />
        </div>
      )}
    </div>
  );
}
