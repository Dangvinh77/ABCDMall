import { useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { getImageUrl } from "../../../core/utils/image";
import { useFood } from "../hooks/useFood";

export default function FoodPage() {
  const { foods, loading, error } = useFood();
  const navigate = useNavigate();
  const [keyword, setKeyword] = useState("");

  const filteredFoods = useMemo(
    () => foods.filter((food) => food.name?.toLowerCase().includes(keyword.toLowerCase())),
    [foods, keyword],
  );

  return (
    <div className="bg-gray-50 min-h-screen">
      <div className="relative h-[380px] overflow-hidden">
        <img
          src="https://images.unsplash.com/photo-1504674900247-0877df9cc836"
          className="absolute inset-0 h-full w-full object-cover scale-105"
          alt="Food Court"
        />
        <div className="absolute inset-0 bg-black/60" />

        <div className="relative z-10 mx-auto flex h-full max-w-6xl flex-col items-start justify-center px-6 text-white md:px-10">
          <p className="rounded-full border border-white/20 bg-white/10 px-4 py-2 text-xs font-semibold uppercase tracking-[0.24em]">
            ABCD Mall
          </p>
          <h1 className="mt-5 text-4xl font-bold tracking-tight text-yellow-400 md:text-5xl">Food Court</h1>
          <p className="mt-4 max-w-2xl text-sm leading-7 text-white/85 md:text-base">
            Explore food, drinks, bakery, BBQ, sushi, hotpot, and quick-service counters in one curated destination.
          </p>
        </div>
      </div>

      <div className="mx-auto -mt-10 max-w-6xl px-6 md:px-10">
        <div className="rounded-3xl border border-gray-100 bg-white p-3 shadow-2xl">
          <div className="flex items-center gap-3 px-3">
            <span className="text-gray-400">🔍</span>
            <input
              value={keyword}
              onChange={(event) => setKeyword(event.target.value)}
              placeholder="Search stores, cuisines, or drinks..."
              className="w-full rounded-2xl p-4 text-gray-700 outline-none"
            />
          </div>
        </div>
      </div>

      <div className="mx-auto mt-14 max-w-6xl px-6 md:px-10">
        <section className="rounded-[28px] border border-gray-200 bg-white p-6 shadow-sm">
          <h2 className="text-2xl font-bold text-red-500">Special pick</h2>
          <div className="mt-5 flex flex-col gap-4 rounded-3xl bg-gray-50 p-5 md:flex-row md:items-center md:justify-between">
            <div>
              <p className="text-lg font-semibold text-gray-900">A-la-carte lunch</p>
              <p className="text-sm text-gray-500">Babushka and Boba Bella A la carte</p>
            </div>
            <p className="text-sm font-medium text-gray-500">Daily | 11:30 - 16:00</p>
          </div>
        </section>

        <section className="mt-8 rounded-[28px] border border-gray-200 bg-white p-6 shadow-sm">
          <h2 className="text-2xl font-bold text-red-500">About the Food Court</h2>
          <p className="mt-4 max-w-3xl text-sm leading-7 text-gray-600 md:text-base">
            The Food Court brings together multiple dining styles inside a single modern space, making it easy to move from coffee and bakery stops to BBQ, sushi, hotpot, and fast-service counters.
          </p>
        </section>

        <section className="mt-8 pb-16">
          <div className="flex items-end justify-between gap-4">
            <div>
              <h2 className="text-2xl font-bold text-red-500">Store directory</h2>
              <p className="mt-2 text-sm text-gray-500">Browse the latest restaurants and drink counters currently available.</p>
            </div>
            <p className="text-sm font-semibold text-gray-400">{filteredFoods.length} results</p>
          </div>

          {error ? (
            <div className="mt-8 rounded-[28px] border border-red-200 bg-red-50 p-8 text-center">
              <p className="text-xl font-bold text-red-600">Unable to load food court stores</p>
              <p className="mt-2 text-sm text-red-500">{error}</p>
            </div>
          ) : loading ? (
            <div className="mt-8 rounded-[28px] border border-gray-200 bg-white p-12 text-center shadow-sm">
              <div className="mx-auto h-10 w-10 animate-spin rounded-full border-4 border-gray-200 border-t-red-500" />
              <p className="mt-4 text-sm font-medium text-gray-500">Loading stores...</p>
            </div>
          ) : filteredFoods.length === 0 ? (
            <div className="mt-8 rounded-[28px] border border-gray-200 bg-white p-12 text-center shadow-sm">
              <p className="text-xl font-bold text-gray-900">No stores match your search</p>
              <p className="mt-2 text-sm text-gray-500">Try a different keyword or clear the search field.</p>
            </div>
          ) : (
            <div className="mt-8 grid gap-6 md:grid-cols-2">
              {filteredFoods.map((food) => (
                <button
                  key={food.id ?? food.slug ?? food.name}
                  type="button"
                  onClick={() => navigate(`/food/${food.slug}`)}
                  className="group flex items-center gap-5 rounded-[28px] border border-gray-200 bg-white p-5 text-left shadow-sm transition hover:-translate-y-1 hover:shadow-xl"
                >
                  <div className="flex h-24 w-24 items-center justify-center overflow-hidden rounded-2xl bg-gray-100">
                    <img
                      src={getImageUrl(food.imageUrl ?? "")}
                      alt={food.name}
                      className="h-full w-full object-cover transition group-hover:scale-105"
                    />
                  </div>
                  <div className="min-w-0 flex-1">
                    <h3 className="text-lg font-semibold text-gray-900">{food.name}</h3>
                    <p className="mt-2 line-clamp-2 text-sm leading-6 text-gray-500">
                      {food.description || "Open the store profile to view highlights, menu imagery, and quick facts."}
                    </p>
                    <p className="mt-3 text-sm font-semibold text-red-500">View details →</p>
                  </div>
                </button>
              ))}
            </div>
          )}
        </section>
      </div>
    </div>
  );
}
