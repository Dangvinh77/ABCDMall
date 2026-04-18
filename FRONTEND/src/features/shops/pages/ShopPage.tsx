import { useState } from "react";
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

  return (
    <main className="min-h-screen bg-mall-light pb-20">
      <section className="relative overflow-hidden bg-slate-950 text-white">
        <img
          src="https://images.unsplash.com/photo-1519567241046-7f570eee3ce6?q=80&w=2000&auto=format&fit=crop"
          alt="ABCD Mall shops"
          className="absolute inset-0 h-full w-full object-cover opacity-35"
        />
        <div className="absolute inset-0 bg-gradient-to-r from-slate-950 via-slate-900/80 to-slate-950/40" />
        <div className="relative mx-auto max-w-7xl px-6 py-24 md:px-10 md:py-32">
          <p className="text-sm font-semibold uppercase tracking-[0.3em] text-mall-accent">
            Shopping Center
          </p>
          <h1 className="mt-4 max-w-3xl text-4xl font-black md:text-6xl">
            Explore flagship stores integrated into the main mall experience.
          </h1>
          <p className="mt-6 max-w-2xl text-lg text-slate-200">
            Browse fashion, sportswear, and lifestyle brands curated for the ABCD Mall homepage,
            promotions, and quick navigation flow.
          </p>
          <div className="mt-10 max-w-xl rounded-[1.75rem] bg-white p-2 shadow-2xl">
            <input
              value={keyword}
              onChange={(event) => setKeyword(event.target.value)}
              placeholder="Search stores, categories, or locations..."
              className="w-full rounded-[1.25rem] px-5 py-4 text-slate-700 outline-none"
            />
          </div>
        </div>
      </section>

      <section className="mx-auto max-w-7xl px-6 pt-14 md:px-10">
        <div className="flex flex-col gap-4 rounded-[2rem] bg-white p-6 shadow-sm md:flex-row md:items-center md:justify-between">
          <div>
            <p className="text-sm font-semibold uppercase tracking-[0.2em] text-mall-primary">
              Integrated Module
            </p>
            <h2 className="mt-2 text-2xl font-black text-slate-900">
              Shops now follow the same `features/pages/routes` structure as MAIN.
            </h2>
          </div>
          <div className="text-sm text-slate-500">
            <p>{shops.length} stores available</p>
            <p>Home carousel and quick nav links now resolve correctly.</p>
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
