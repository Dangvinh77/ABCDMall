import { useState } from "react";
import { getImageUrl } from "../../../core/utils/image";
import ShopCard from "../components/ShopCard";
import { useShops } from "../hooks/useShops";

export default function ShopPage() {
  const { shops, loading, error } = useShops();
  const [keyword, setKeyword] = useState("");

  const normalizedKeyword = keyword.trim().toLowerCase();
  const filteredShops = normalizedKeyword
    ? shops.filter((shop) =>
        [shop.name, shop.category, shop.location, ...shop.tags]
          .join(" ")
          .toLowerCase()
          .includes(normalizedKeyword),
      )
    : shops;
  const heroImageUrl = getImageUrl(filteredShops[0]?.imageUrl || shops[0]?.imageUrl || "");

  return (
    <main className="min-h-screen bg-mall-light pb-20">
      <section className="relative overflow-hidden bg-slate-950 text-white">
        {heroImageUrl ? (
          <img
            src={heroImageUrl}
            alt="ABCD Mall shops"
            className="absolute inset-0 h-full w-full object-cover opacity-35"
          />
        ) : null}
        <div className="absolute inset-0 bg-gradient-to-r from-slate-950 via-slate-900/80 to-slate-950/40" />
        <div className="relative mx-auto max-w-7xl px-6 py-24 md:px-10 md:py-32">
          <p className="text-sm font-semibold uppercase tracking-[0.3em] text-mall-accent">
            Shopping Directory
          </p>
          <h1 className="mt-4 max-w-3xl text-4xl font-black md:text-6xl">
            Explore stores with images and details synced directly from the API.
          </h1>
          <p className="mt-6 max-w-2xl text-lg text-slate-200">
            Store listings, locations, and cover images are loaded dynamically from the backend so the page always reflects real data.
          </p>
          <div className="mt-10 max-w-xl rounded-[1.75rem] bg-white p-2 shadow-2xl">
            <input
              value={keyword}
              onChange={(event) => setKeyword(event.target.value)}
              placeholder="Tìm cửa hàng, danh mục hoặc vị trí..."
              className="w-full rounded-[1.25rem] px-5 py-4 text-slate-700 outline-none"
            />
          </div>
        </div>
      </section>

      <section className="mx-auto max-w-7xl px-6 pt-14 md:px-10">
        <div className="flex flex-col gap-4 rounded-[2rem] bg-white p-6 shadow-sm md:flex-row md:items-center md:justify-between">
          <div>
            <p className="text-sm font-semibold uppercase tracking-[0.2em] text-mall-primary">
              Kết nối API
            </p>
            <h2 className="mt-2 text-2xl font-black text-slate-900">
              Store imagery is now rendered from API data instead of static content.
            </h2>
          </div>
          <div className="text-sm text-slate-500">
            <p>{shops.length} stores available</p>
            <p>Cover imagery updates automatically based on the current store data.</p>
          </div>
        </div>

        {loading ? (
          <div className="py-20 text-center text-slate-500">Loading shops...</div>
        ) : error ? (
          <div className="py-20 text-center text-red-500">{error}</div>
        ) : filteredShops.length === 0 ? (
          <div className="py-20 text-center text-slate-500">No shops match this search.</div>
        ) : (
          <div className="mt-10 grid gap-8 md:grid-cols-2 xl:grid-cols-3">
            {filteredShops.map((shop) => (
              <ShopCard key={shop.id} shop={shop} />
            ))}
          </div>
        )}
      </section>
    </main>
  );
}
