import { Link } from "react-router-dom";
import type { Shop } from "../api/shopApi";

type ShopCardProps = {
  shop: Shop;
};

export default function ShopCard({ shop }: ShopCardProps) {
  return (
    <Link
      to={`/shops/${shop.slug}`}
      className="group overflow-hidden rounded-[2rem] border border-slate-200 bg-white shadow-sm transition hover:-translate-y-1 hover:shadow-xl"
    >
      <div className="relative h-64 overflow-hidden">
        <img
          src={shop.imageUrl}
          alt={shop.name}
          className="h-full w-full object-cover transition duration-500 group-hover:scale-105"
        />
        <div className="absolute inset-0 bg-gradient-to-t from-slate-950/70 via-transparent to-transparent" />
        {shop.badge ? (
          <span className="absolute left-5 top-5 rounded-full bg-mall-primary px-4 py-2 text-xs font-bold text-white">
            {shop.badge}
          </span>
        ) : null}
        <div className="absolute bottom-5 left-5 right-5 text-white">
          <p className="text-sm opacity-90">{shop.category}</p>
          <h3 className="mt-1 text-2xl font-extrabold">{shop.name}</h3>
        </div>
      </div>

      <div className="space-y-4 p-6">
        <p className="text-sm leading-6 text-slate-600">{shop.summary}</p>
        <div className="flex flex-wrap gap-2">
          {shop.tags.map((tag) => (
            <span
              key={tag}
              className="rounded-full bg-slate-100 px-3 py-1 text-xs font-semibold text-slate-600"
            >
              {tag}
            </span>
          ))}
        </div>
        <div className="flex items-center justify-between text-sm text-slate-500">
          <span>{shop.location}</span>
          <span className="font-semibold text-mall-primary">View details</span>
        </div>
      </div>
    </Link>
  );
}

