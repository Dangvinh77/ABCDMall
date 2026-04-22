import { useEffect, useMemo, useState } from "react";
import { ArrowLeft, Clock3, ExternalLink, MapPin, Phone, Star, X } from "lucide-react";
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
} from "../data/foodStoreMedia";

export default function FoodDetailPage() {
  const { slug } = useParams();
  const [food, setFood] = useState<FoodListItem | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
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
        if (!active) {
          return;
        }

        setFood(result);
        setError(null);
      } catch (err) {
        if (!active) {
          return;
        }

        setError(err instanceof Error ? err.message : "Unable to load restaurant details.");
      } finally {
        if (active) {
          setLoading(false);
        }
      }
    };

    void load();

    return () => {
      active = false;
    };
  }, [slug]);

  useEffect(() => {
    if (food) {
      document.title = `${food.name} | ABCD Mall`;
      const meta = document.querySelector("meta[name='description']");
      if (meta) {
        meta.setAttribute("content", food.description ?? "");
      }
    }
  }, [food]);

  const category = useMemo(() => normalizeFoodCategory(food), [food]);
  const preset = CATEGORY_PRESETS[category];
  const menu = useMemo(() => (food ? buildFoodMenu(food) : []), [food]);
  const gallery = useMemo(() => (food ? buildFoodGallery(food) : []), [food]);
  const heroImage = gallery[0] ?? imageSrc(food?.imageUrl);
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
    { label: "Category", value: preset.label, icon: MapPin },
    { label: "Opening hours", value: preset.hours, icon: Clock3 },
    { label: "Rating", value: "4.6 / 5 • 120+ reviews", icon: Star },
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
            <span>/</span>
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
              <a
                href="#menu"
                className="inline-flex items-center gap-2 rounded-full bg-white px-5 py-3 text-sm font-semibold text-gray-900 shadow-lg transition hover:-translate-y-0.5"
              >
                View menu
                <ExternalLink className="h-4 w-4" />
              </a>
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
            <p className="text-xs font-semibold uppercase tracking-[0.24em] text-gray-400">About the store</p>
            <h2 className="mt-2 text-2xl font-bold">A dedicated food court destination</h2>
            <p className="mt-5 max-w-3xl text-sm leading-7 text-gray-600">
              {food.description || `${brand} brings a distinct dining style to the Food Court.`}
            </p>
            <p className="mt-4 max-w-3xl text-sm leading-7 text-gray-600">{preset.subtitle}</p>
          </section>

          <section id="menu" className="rounded-[28px] border border-gray-200 bg-white p-6 shadow-sm">
            <div className="flex items-end justify-between gap-4">
              <div>
                <p className="text-xs font-semibold uppercase tracking-[0.24em] text-gray-400">Menu</p>
                <h2 className="mt-1 text-2xl font-bold">Main items and signature dishes</h2>
              </div>
              <p className="text-sm font-semibold text-gray-400">{menu.length} items</p>
            </div>

            <div className="mt-5 grid gap-4 md:grid-cols-2">
              {menu.slice(0, 6).map((item, index) => (
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
                    <p className="mt-3 text-sm leading-6 text-gray-500">{item.note}</p>
                  </div>
                </button>
              ))}
            </div>
          </section>

          <section className="rounded-[28px] border border-gray-200 bg-white p-6 shadow-sm">
            <div className="flex items-end justify-between gap-4">
              <div>
                <p className="text-xs font-semibold uppercase tracking-[0.24em] text-gray-400">Gallery</p>
                <h2 className="mt-1 text-2xl font-bold">Store visuals and menu imagery</h2>
              </div>
              <p className="text-sm font-semibold text-gray-400">{gallery.length} images</p>
            </div>

            <div className="mt-5 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
              {gallery.map((image, index) => (
                <button
                  key={`${image}-${index}`}
                  type="button"
                  onClick={() => setPreviewImage(image)}
                  className="overflow-hidden rounded-3xl border border-gray-200 bg-gray-50 transition hover:shadow-lg"
                >
                  <img src={image} alt={`${brand} gallery ${index + 1}`} className="h-48 w-full object-cover" />
                </button>
              ))}
            </div>
          </section>
        </div>

        <aside className="space-y-6">
          <section className="rounded-[28px] border border-gray-200 bg-white p-6 shadow-sm">
            <p className="text-xs font-semibold uppercase tracking-[0.24em] text-gray-400">Quick facts</p>
            <div className="mt-5 space-y-4">
              {quickFacts.map((fact) => {
                const Icon = fact.icon;
                return (
                  <div key={fact.label} className="flex gap-3">
                    <div className={`rounded-2xl p-3 ${preset.soft}`}>
                      <Icon className="h-5 w-5 text-gray-900" />
                    </div>
                    <div>
                      <p className="text-xs font-semibold uppercase tracking-[0.18em] text-gray-400">{fact.label}</p>
                      <p className="mt-1 text-sm font-medium text-gray-700">{fact.value}</p>
                    </div>
                  </div>
                );
              })}
            </div>
          </section>

          <section className={`rounded-[28px] p-6 shadow-sm ${preset.soft}`}>
            <p className="text-xs font-semibold uppercase tracking-[0.24em] text-gray-500">Recommendation</p>
            <h3 className="mt-2 text-xl font-bold text-gray-900">Best time to visit</h3>
            <p className="mt-3 text-sm leading-7 text-gray-600">{preset.promo}</p>
          </section>
        </aside>
      </div>

      {previewImage ? (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/70 p-6" onClick={() => setPreviewImage(null)}>
          <button
            type="button"
            onClick={() => setPreviewImage(null)}
            className="absolute right-6 top-6 rounded-full bg-white/15 p-3 text-white transition hover:bg-white/25"
          >
            <X className="h-5 w-5" />
          </button>
          <img
            src={previewImage}
            alt={brand}
            className="max-h-[90vh] max-w-[90vw] rounded-3xl object-contain shadow-2xl"
            onClick={(event) => event.stopPropagation()}
          />
        </div>
      ) : null}
    </div>
  );
}
