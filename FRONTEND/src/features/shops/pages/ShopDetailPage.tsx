import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { getImageUrl } from "../../../core/utils/image";
import { getShopBySlug, type ShopDetail } from "../api/shopApi";

function formatCurrency(value: number) {
  return new Intl.NumberFormat("vi-VN", {
    style: "currency",
    currency: "VND",
    maximumFractionDigits: 0,
  }).format(value);
}

export default function ShopDetailPage() {
  const { slug } = useParams();
  const [shop, setShop] = useState<ShopDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let active = true;

    if (!slug) {
      setLoading(false);
      return;
    }

    getShopBySlug(slug)
      .then((data) => {
        if (active) {
          setShop(data);
          setError(null);
        }
      })
      .catch((requestError: unknown) => {
        if (active) {
          setShop(null);
          setError(requestError instanceof Error ? requestError.message : "Unable to load shop details.");
        }
      })
      .finally(() => {
        if (active) {
          setLoading(false);
        }
      });

    return () => {
      active = false;
    };
  }, [slug]);

  useEffect(() => {
    if (!shop) {
      return;
    }

    document.title = `${shop.name} | ABCD Mall`;
  }, [shop]);

  if (loading) {
    return <div className="p-10 text-center text-slate-500">Loading shop details...</div>;
  }

  if (error) {
    return <div className="p-10 text-center text-red-500">{error}</div>;
  }

  if (!shop) {
    return (
      <div className="p-10 text-center text-slate-500">
        Shop not found. <Link to="/shops" className="font-semibold text-mall-primary">Back to shops</Link>
      </div>
    );
  }

  return (
    <main className="min-h-screen bg-mall-light pb-16">
      <section className="relative h-[340px] overflow-hidden bg-slate-950">
        <img
          src={getImageUrl(shop.imageUrl || shop.coverImageUrl)}
          alt={shop.name}
          className="absolute inset-0 h-full w-full object-cover"
        />
        <div className="absolute inset-0 bg-gradient-to-t from-slate-950 via-slate-950/55 to-transparent" />
        <div className="relative mx-auto flex h-full max-w-7xl flex-col justify-end px-6 pb-10 text-white md:px-10">
          <p className="text-sm text-slate-200">Home / Shops / {shop.name}</p>
          <h1 className="mt-3 text-4xl font-black md:text-5xl">{shop.name}</h1>
          <p className="mt-3 max-w-2xl text-lg text-slate-200">{shop.summary}</p>
        </div>
      </section>

      <section className="mx-auto grid max-w-7xl gap-10 px-6 py-12 md:grid-cols-[1.5fr_0.9fr] md:px-10">
        <div className="space-y-10">
          <div className="rounded-[2rem] bg-white p-8 shadow-sm">
            <p className="text-sm font-semibold uppercase tracking-[0.2em] text-mall-primary">
              Store Overview
            </p>
            <h2 className="mt-3 text-3xl font-black text-slate-900">About {shop.name}</h2>
            <p className="mt-6 leading-8 text-slate-600">{shop.description}</p>

            <div className="mt-8 flex flex-wrap gap-3">
              {shop.tags.map((tag) => (
                <span
                  key={tag}
                  className="rounded-full bg-slate-100 px-4 py-2 text-sm font-semibold text-slate-600"
                >
                  {tag}
                </span>
              ))}
            </div>
          </div>

          <div className="rounded-[2rem] bg-white p-8 shadow-sm">
            <div className="flex items-end justify-between gap-4">
              <div>
                <p className="text-sm font-semibold uppercase tracking-[0.2em] text-mall-primary">
                  Featured Products
                </p>
                <h2 className="mt-3 text-3xl font-black text-slate-900">What to shop</h2>
              </div>
              <p className="text-sm text-slate-500">{shop.products.length} items</p>
            </div>

            {shop.products.length > 0 ? (
              <div className="mt-8 grid gap-5 md:grid-cols-2 xl:grid-cols-3">
                {shop.products.map((product) => (
                  <article key={product.id} className="overflow-hidden rounded-[1.5rem] border border-slate-100 bg-slate-50">
                    <div className="relative h-56 overflow-hidden bg-white">
                      <img
                        src={getImageUrl(product.imageUrl)}
                        alt={product.name}
                        className="h-full w-full object-cover"
                      />
                      {product.isFeatured ? (
                        <span className="absolute left-4 top-4 rounded-full bg-slate-950 px-3 py-1 text-xs font-semibold uppercase tracking-[0.18em] text-white">
                          Featured
                        </span>
                      ) : null}
                    </div>
                    <div className="space-y-3 p-5">
                      <h3 className="text-lg font-bold text-slate-900">{product.name}</h3>
                      <div className="flex flex-wrap items-center gap-3">
                        <span className="text-lg font-black text-mall-primary">{formatCurrency(product.price)}</span>
                        {product.oldPrice ? (
                          <span className="text-sm text-slate-400 line-through">{formatCurrency(product.oldPrice)}</span>
                        ) : null}
                        {product.discountPercent ? (
                          <span className="rounded-full bg-emerald-100 px-3 py-1 text-xs font-semibold text-emerald-700">
                            -{product.discountPercent}%
                          </span>
                        ) : null}
                      </div>
                    </div>
                  </article>
                ))}
              </div>
            ) : (
              <p className="mt-6 text-slate-500">This shop has no featured catalog items yet.</p>
            )}
          </div>
        </div>

        <aside className="space-y-8">
          <div className="rounded-[2rem] bg-white p-8 shadow-sm">
          <p className="text-sm font-semibold uppercase tracking-[0.2em] text-mall-primary">
            Visit Information
          </p>
          <h2 className="mt-3 text-2xl font-black text-slate-900">Plan your visit</h2>
          <div className="mt-6 space-y-4 text-slate-600">
            <p><span className="font-semibold text-slate-900">Category:</span> {shop.category}</p>
            <p><span className="font-semibold text-slate-900">Location:</span> {shop.location}</p>
            <p><span className="font-semibold text-slate-900">Floor:</span> {shop.floor}</p>
            <p><span className="font-semibold text-slate-900">Unit:</span> {shop.locationSlot}</p>
            <p><span className="font-semibold text-slate-900">Opening hours:</span> {shop.openHours}</p>
            {shop.offer ? (
              <div className="rounded-2xl bg-mall-primary/10 p-4 text-sm">
                <p className="font-semibold text-mall-primary">Current offer</p>
                <p className="mt-1 text-slate-700">{shop.offer}</p>
              </div>
            ) : null}
          </div>

          <Link
            to="/shops"
            className="mt-8 inline-flex rounded-full bg-mall-primary px-6 py-3 font-bold text-white transition hover:bg-mall-secondary"
          >
            Back to all shops
          </Link>
          </div>

          <div className="rounded-[2rem] bg-white p-8 shadow-sm">
            <p className="text-sm font-semibold uppercase tracking-[0.2em] text-mall-primary">
              Voucher Board
            </p>
            <h2 className="mt-3 text-2xl font-black text-slate-900">Current offers</h2>

            {shop.vouchers.length > 0 ? (
              <div className="mt-6 space-y-4">
                {shop.vouchers.map((voucher) => (
                  <article key={voucher.id} className="rounded-[1.5rem] border border-dashed border-mall-primary/30 bg-mall-primary/5 p-5">
                    <div className="flex items-start justify-between gap-3">
                      <div>
                        <p className="text-xs font-semibold uppercase tracking-[0.18em] text-mall-primary">{voucher.code}</p>
                        <h3 className="mt-2 text-lg font-bold text-slate-900">{voucher.title}</h3>
                      </div>
                      <span className={`rounded-full px-3 py-1 text-xs font-semibold ${voucher.isActive ? "bg-emerald-100 text-emerald-700" : "bg-slate-200 text-slate-500"}`}>
                        {voucher.isActive ? "Active" : "Inactive"}
                      </span>
                    </div>
                    <p className="mt-3 text-sm leading-6 text-slate-600">{voucher.description}</p>
                    <p className="mt-3 text-xs font-semibold uppercase tracking-[0.15em] text-slate-400">
                      Valid until {voucher.validUntil}
                    </p>
                  </article>
                ))}
              </div>
            ) : (
              <p className="mt-6 text-slate-500">No vouchers are available for this shop right now.</p>
            )}
          </div>
        </aside>
      </section>
    </main>
  );
}
