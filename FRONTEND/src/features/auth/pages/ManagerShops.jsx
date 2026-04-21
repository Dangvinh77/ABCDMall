import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import {
  createMyManagedShop,
  deleteMyManagedShop,
  getMyManagedShops,
  getMyShopCreationStatus,
  updateMyManagedShop,
} from "../../shops/api/shopApi";
import { getImageUrl } from "../../../core/utils/image";

const emptyForm = {
  name: "",
  slug: "",
  category: "",
  floor: "",
  locationSlot: "",
  summary: "",
  description: "",
  logoUrl: "",
  coverImageUrl: "",
  logoFile: null,
  coverFile: null,
  openHours: "09:30 - 22:00",
  badge: "",
  offer: "",
  tagsText: "",
  products: [],
};

const emptyProduct = {
  id: "",
  name: "",
  imageUrl: "",
  imageFile: null,
  price: "",
  oldPrice: "",
  discountPercent: "",
  isFeatured: true,
  isDiscounted: false,
};

const toSlug = (value) =>
  value
    .trim()
    .toLowerCase()
    .replace(/[^a-z0-9\s-]/g, "")
    .replace(/\s+/g, "-")
    .replace(/-+/g, "-");

const toForm = (shop) => ({
  name: shop.name || "",
  slug: shop.slug || "",
  category: shop.category || "",
  floor: shop.floor || "",
  locationSlot: shop.locationSlot || "",
  summary: shop.summary || "",
  description: shop.description || "",
  logoUrl: shop.logoUrl || "",
  coverImageUrl: shop.coverImageUrl || "",
  logoFile: null,
  coverFile: null,
  openHours: shop.openHours || "09:30 - 22:00",
  badge: shop.badge || "",
  offer: shop.offer || "",
  tagsText: (shop.tags || []).join(", "),
  products: (shop.products || []).map((product) => ({
    id: product.id || "",
    name: product.name || "",
    imageUrl: product.imageUrl || "",
    imageFile: null,
    price: product.price?.toString() || "",
    oldPrice: product.oldPrice?.toString() || "",
    discountPercent: product.discountPercent?.toString() || "",
    isFeatured: product.isFeatured ?? true,
    isDiscounted: product.isDiscounted ?? false,
  })),
});

const appendIfValue = (formData, key, value) => {
  if (value) {
    formData.append(key, value);
  }
};

const toRequest = (form) => {
  const formData = new FormData();
  const tags = form.tagsText
    .split(",")
    .map((tag) => tag.trim())
    .filter(Boolean);

  formData.append("name", form.name.trim());
  formData.append("slug", toSlug(form.slug || form.name));
  formData.append("category", form.category.trim());
  formData.append("floor", form.floor.trim());
  formData.append("locationSlot", form.locationSlot.trim());
  formData.append("summary", form.summary.trim());
  formData.append("description", form.description.trim());
  formData.append("logoUrl", form.logoUrl.trim());
  formData.append("coverImageUrl", form.coverImageUrl.trim());
  formData.append("openHours", form.openHours.trim() || "09:30 - 22:00");
  appendIfValue(formData, "badge", form.badge.trim());
  appendIfValue(formData, "offer", form.offer.trim());
  tags.forEach((tag) => formData.append("tags", tag));

  const productImages = [];
  const products = form.products
    .filter((product) => product.name.trim() || product.price || product.imageUrl || product.imageFile)
    .map((product) => {
      const imageFileIndex = product.imageFile ? productImages.push(product.imageFile) - 1 : null;

      return {
        id: product.id || null,
        name: product.name.trim(),
        imageUrl: product.imageUrl.trim(),
        imageFileIndex,
        price: Number(product.price || 0),
        oldPrice: product.oldPrice ? Number(product.oldPrice) : null,
        discountPercent: product.discountPercent ? Number(product.discountPercent) : null,
        isFeatured: product.isFeatured,
        isDiscounted: product.isDiscounted,
      };
    });

  formData.append("productsJson", JSON.stringify(products));
  productImages.forEach((file) => formData.append("ProductImages", file));

  if (form.logoFile) {
    formData.append("LogoImage", form.logoFile);
  }

  if (form.coverFile) {
    formData.append("CoverImage", form.coverFile);
  }

  return formData;
};

const FormSection = ({ eyebrow, title, description, children }) => (
  <section className="rounded-[28px] border border-slate-200 bg-white p-4 shadow-sm sm:p-5">
    <div className="mb-4">
      <p className="text-xs font-semibold uppercase tracking-[0.22em] text-amber-600">{eyebrow}</p>
      <h3 className="mt-2 text-xl font-black text-slate-950">{title}</h3>
      {description && <p className="mt-1 text-sm leading-6 text-slate-500">{description}</p>}
    </div>
    {children}
  </section>
);

const TextInput = ({ field, label, placeholder, form, onChange }) => (
  <label className="block">
    <span className="mb-2 block text-sm font-semibold text-slate-700">{label}</span>
    <input
      value={form[field]}
      placeholder={placeholder}
      onChange={(event) => onChange(field, event.target.value)}
      className="w-full rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3 text-slate-800 outline-none transition focus:border-amber-400 focus:bg-white focus:ring-4 focus:ring-amber-100"
    />
  </label>
);

const TextArea = ({ field, label, placeholder, rows, form, onChange }) => (
  <label className="block">
    <span className="mb-2 block text-sm font-semibold text-slate-700">{label}</span>
    <textarea
      value={form[field]}
      placeholder={placeholder}
      rows={rows}
      onChange={(event) => onChange(field, event.target.value)}
      className="w-full resize-none rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3 text-slate-800 outline-none transition focus:border-amber-400 focus:bg-white focus:ring-4 focus:ring-amber-100"
    />
  </label>
);

export default function ManagerShops() {
  const [shops, setShops] = useState([]);
  const [form, setForm] = useState(emptyForm);
  const [editingShopId, setEditingShopId] = useState(null);
  const [loading, setLoading] = useState(true);
  const [creationStatus, setCreationStatus] = useState(null);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  const loadShops = async () => {
    try {
      setLoading(true);
      setError("");
      const [shopsData, statusData] = await Promise.all([
        getMyManagedShops(),
        getMyShopCreationStatus(),
      ]);
      setShops(shopsData);
      setCreationStatus(statusData);
    } catch (err) {
      setError(err.message || "Unable to load your shops.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadShops();
  }, []);

  const handleChange = (field, value) => {
    setForm((prev) => ({
      ...prev,
      [field]: field === "slug" ? toSlug(value) : value,
      ...(field === "name" && !editingShopId && !prev.slug ? { slug: toSlug(value) } : {}),
    }));
  };

  const handleFileChange = (field, file) => {
    setForm((prev) => ({
      ...prev,
      [field]: file,
    }));
  };

  const addProduct = () => {
    setForm((prev) => ({
      ...prev,
      products: [...prev.products, { ...emptyProduct }],
    }));
  };

  const updateProduct = (index, field, value) => {
    setForm((prev) => ({
      ...prev,
      products: prev.products.map((product, productIndex) =>
        productIndex === index ? { ...product, [field]: value } : product,
      ),
    }));
  };

  const removeProduct = (index) => {
    setForm((prev) => ({
      ...prev,
      products: prev.products.filter((_, productIndex) => productIndex !== index),
    }));
  };

  const resetForm = () => {
    setForm(emptyForm);
    setEditingShopId(null);
    setError("");
    setSuccess("");
  };

  const handleEdit = (shop) => {
    setEditingShopId(shop.id);
    setForm(toForm(shop));
    setError("");
    setSuccess("");
    window.scrollTo({ top: 0, behavior: "smooth" });
  };

  const handleSubmit = async (event) => {
    event.preventDefault();

    try {
      setSaving(true);
      setError("");
      setSuccess("");

      const request = toRequest(form);
      if (editingShopId) {
        await updateMyManagedShop(editingShopId, request);
        setSuccess("Shop information updated successfully.");
      } else {
        await createMyManagedShop(request);
        setSuccess("Shop created successfully.");
      }

      resetForm();
      await loadShops();
    } catch (err) {
      setError(err.message || "Unable to save shop information.");
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (shop) => {
    const confirmed = window.confirm(`Delete ${shop.name}? This action cannot be undone.`);
    if (!confirmed) {
      return;
    }

    try {
      setError("");
      setSuccess("");
      await deleteMyManagedShop(shop.id);
      setSuccess("Shop deleted successfully.");
      if (editingShopId === shop.id) {
        resetForm();
      }
      await loadShops();
    } catch (err) {
      setError(err.message || "Unable to delete shop.");
    }
  };

  const canShowCreateForm = editingShopId || !creationStatus || creationStatus.canCreate;

  return (
    <div className="min-h-screen bg-[linear-gradient(180deg,#fff8ef_0%,#fffdf8_42%,#f8fbff_100%)] text-slate-900">
      <div className="mx-auto flex min-h-screen w-full max-w-7xl flex-col px-4 pb-6 pt-28 sm:px-6 lg:px-8">
        <header className="rounded-[28px] border border-white/70 bg-white/80 px-5 py-4 shadow-[0_20px_80px_rgba(15,23,42,0.08)] backdrop-blur-xl sm:px-6">
          <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
            <div>
              <div className="inline-flex items-center gap-2 rounded-full bg-amber-100 px-3 py-1 text-xs font-semibold uppercase tracking-[0.28em] text-amber-700">
                Manager Shop
              </div>
              <h1 className="mt-3 text-3xl font-black tracking-tight sm:text-4xl">
                My Shop Management
              </h1>
              <p className="mt-2 max-w-2xl text-sm text-slate-600 sm:text-base">
                Create, update, and remove public shop pages that belong to your manager account only.
              </p>
            </div>

            <Link
              to="/dashboard"
              className="inline-flex items-center justify-center rounded-full bg-slate-950 px-5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5"
            >
              Back to Dashboard
            </Link>
          </div>
        </header>

        <main className="mt-6 grid flex-1 gap-6 xl:grid-cols-[0.95fr_1.05fr]">
          <section className="rounded-[30px] border border-slate-200 bg-white/90 p-5 shadow-[0_24px_90px_rgba(15,23,42,0.08)] backdrop-blur-xl sm:p-6">
            <div className="mb-5">
              <p className="text-xs font-semibold uppercase tracking-[0.24em] text-slate-400">
                {editingShopId ? "Update" : "Create"}
              </p>
              <h2 className="mt-2 text-2xl font-black text-slate-950">
                {editingShopId ? "Edit shop information" : "Add new shop"}
              </h2>
            </div>

            {error && (
              <div className="mb-4 rounded-2xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-600">
                {error}
              </div>
            )}
            {success && (
              <div className="mb-4 rounded-2xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-700">
                {success}
              </div>
            )}

            {creationStatus && (
              <div className="mb-5 rounded-3xl border border-slate-200 bg-slate-50 px-4 py-4 text-sm text-slate-600">
                <p className="font-semibold text-slate-900">Shop creation quota</p>
                <p className="mt-1">
                  Created shops: {creationStatus.shopCount} / Rented areas: {creationStatus.rentedAreaCount}
                </p>
                <p className="mt-1">{creationStatus.message}</p>
              </div>
            )}

            {!canShowCreateForm ? (
              <div className="rounded-[28px] border border-amber-200 bg-amber-50 px-5 py-6 text-sm leading-6 text-amber-800">
                <p className="text-base font-black text-amber-950">Create shop is currently unavailable</p>
                <p className="mt-2">
                  The number of shop pages is equal to the number of rented areas, so no new shop can be created.
                </p>
              </div>
            ) : (
            <form onSubmit={handleSubmit} className="space-y-5">
              <FormSection
                eyebrow="Store Overview"
                title="Public shop identity"
                description="This information appears in the hero and overview area of the public shop page."
              >
                <div className="grid gap-4 sm:grid-cols-2">
                  <TextInput field="name" label="Shop name" placeholder="Miniso" form={form} onChange={handleChange} />
                  <TextInput field="slug" label="Slug" placeholder="miniso" form={form} onChange={handleChange} />
                  <TextInput field="badge" label="Badge" placeholder="New Store" form={form} onChange={handleChange} />
                  <TextInput field="tagsText" label="Tags" placeholder="Lifestyle, Floor 1, Gifts" form={form} onChange={handleChange} />
                </div>
                <div className="mt-4 space-y-4">
                  <TextArea
                    field="summary"
                    label="Summary"
                    placeholder="Short description for shop listing"
                    rows={3}
                    form={form}
                    onChange={handleChange}
                  />
                  <TextArea
                    field="description"
                    label="Description"
                    placeholder="Full shop detail shown on /shops/{slug}"
                    rows={5}
                    form={form}
                    onChange={handleChange}
                  />
                </div>
              </FormSection>

              <FormSection
                eyebrow="Store Media"
                title="Logo and cover image"
                description="Upload the images used in the shop hero banner and logo block."
              >
                <div className="grid gap-4 sm:grid-cols-2">
                  <label className="block rounded-3xl border border-slate-200 bg-slate-50 p-4">
                    <span className="mb-3 block text-sm font-semibold text-slate-700">Logo image</span>
                    {form.logoUrl && (
                      <div className="mb-3 h-24 overflow-hidden rounded-2xl bg-white">
                        <img
                          src={getImageUrl(form.logoUrl)}
                          alt="Current shop logo"
                          className="h-full w-full object-contain p-3"
                        />
                      </div>
                    )}
                    <input
                      type="file"
                      accept="image/*"
                      onChange={(event) => handleFileChange("logoFile", event.target.files?.[0] ?? null)}
                      className="w-full rounded-2xl border border-slate-200 bg-white px-4 py-3 text-sm text-slate-700 file:mr-4 file:rounded-full file:border-0 file:bg-slate-950 file:px-4 file:py-2 file:text-sm file:font-semibold file:text-white"
                    />
                    <p className="mt-2 text-xs text-slate-500">
                      {form.logoFile ? form.logoFile.name : "Upload a logo image. Leave empty to keep current logo."}
                    </p>
                  </label>

                  <label className="block rounded-3xl border border-slate-200 bg-slate-50 p-4">
                    <span className="mb-3 block text-sm font-semibold text-slate-700">Cover image</span>
                    {form.coverImageUrl && (
                      <div className="mb-3 h-24 overflow-hidden rounded-2xl bg-white">
                        <img
                          src={getImageUrl(form.coverImageUrl)}
                          alt="Current shop cover"
                          className="h-full w-full object-cover"
                        />
                      </div>
                    )}
                    <input
                      type="file"
                      accept="image/*"
                      required={!form.coverImageUrl && !form.coverFile}
                      onChange={(event) => handleFileChange("coverFile", event.target.files?.[0] ?? null)}
                      className="w-full rounded-2xl border border-slate-200 bg-white px-4 py-3 text-sm text-slate-700 file:mr-4 file:rounded-full file:border-0 file:bg-amber-400 file:px-4 file:py-2 file:text-sm file:font-semibold file:text-slate-950"
                    />
                    <p className="mt-2 text-xs text-slate-500">
                      {form.coverFile ? form.coverFile.name : "Required for new shops. Leave empty to keep current cover."}
                    </p>
                  </label>
                </div>
              </FormSection>

              <FormSection
                eyebrow="Visit Information"
                title="Location and opening details"
                description="These fields power the Plan your visit panel on the shop detail page."
              >
                <div className="grid gap-4 sm:grid-cols-2">
                  <TextInput field="category" label="Category" placeholder="Lifestyle" form={form} onChange={handleChange} />
                  <TextInput field="floor" label="Floor" placeholder="Floor 1" form={form} onChange={handleChange} />
                  <TextInput field="locationSlot" label="Location" placeholder="1-03" form={form} onChange={handleChange} />
                  <TextInput field="openHours" label="Open hours" placeholder="09:30 - 22:00" form={form} onChange={handleChange} />
                </div>
              </FormSection>

              <FormSection
                eyebrow="Featured Products"
                title="Catalog products"
                description="Add the products that should appear in the Featured Products area of the public shop page."
              >
                <div className="space-y-4">
                  {form.products.length === 0 ? (
                    <div className="rounded-3xl border border-dashed border-slate-300 bg-slate-50 px-4 py-5 text-sm leading-6 text-slate-600">
                      No featured products yet. Add one if this shop should show products on the public detail page.
                    </div>
                  ) : (
                    form.products.map((product, index) => (
                      <article key={`${product.id || "new"}-${index}`} className="rounded-3xl border border-slate-200 bg-slate-50 p-4">
                        <div className="mb-4 flex items-center justify-between gap-3">
                          <div>
                            <p className="text-xs font-semibold uppercase tracking-[0.18em] text-slate-400">
                              Product #{index + 1}
                            </p>
                            <h4 className="mt-1 text-base font-black text-slate-900">
                              {product.name || "New featured product"}
                            </h4>
                          </div>
                          <button
                            type="button"
                            onClick={() => removeProduct(index)}
                            className="rounded-full bg-rose-500 px-3 py-2 text-xs font-semibold text-white"
                          >
                            Remove
                          </button>
                        </div>

                        <div className="grid gap-4 sm:grid-cols-2">
                          <label className="block sm:col-span-2">
                            <span className="mb-2 block text-sm font-semibold text-slate-700">Product name</span>
                            <input
                              value={product.name}
                              placeholder="Mini wireless speaker"
                              onChange={(event) => updateProduct(index, "name", event.target.value)}
                              className="w-full rounded-2xl border border-slate-200 bg-white px-4 py-3 text-slate-800 outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100"
                            />
                          </label>

                          <label className="block">
                            <span className="mb-2 block text-sm font-semibold text-slate-700">Price</span>
                            <input
                              type="number"
                              min="0"
                              value={product.price}
                              placeholder="299000"
                              onChange={(event) => updateProduct(index, "price", event.target.value)}
                              className="w-full rounded-2xl border border-slate-200 bg-white px-4 py-3 text-slate-800 outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100"
                            />
                          </label>

                          <label className="block">
                            <span className="mb-2 block text-sm font-semibold text-slate-700">Old price</span>
                            <input
                              type="number"
                              min="0"
                              value={product.oldPrice}
                              placeholder="399000"
                              onChange={(event) => updateProduct(index, "oldPrice", event.target.value)}
                              className="w-full rounded-2xl border border-slate-200 bg-white px-4 py-3 text-slate-800 outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100"
                            />
                          </label>

                          <label className="block">
                            <span className="mb-2 block text-sm font-semibold text-slate-700">Discount percent</span>
                            <input
                              type="number"
                              min="0"
                              max="100"
                              value={product.discountPercent}
                              placeholder="20"
                              onChange={(event) => updateProduct(index, "discountPercent", event.target.value)}
                              className="w-full rounded-2xl border border-slate-200 bg-white px-4 py-3 text-slate-800 outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100"
                            />
                          </label>

                          <label className="block">
                            <span className="mb-2 block text-sm font-semibold text-slate-700">Product image</span>
                            {product.imageUrl && (
                              <div className="mb-3 h-24 overflow-hidden rounded-2xl bg-white">
                                <img
                                  src={getImageUrl(product.imageUrl)}
                                  alt={product.name || "Current product"}
                                  className="h-full w-full object-cover"
                                />
                              </div>
                            )}
                            <input
                              type="file"
                              accept="image/*"
                              required={!product.imageUrl && !product.imageFile}
                              onChange={(event) => updateProduct(index, "imageFile", event.target.files?.[0] ?? null)}
                              className="w-full rounded-2xl border border-slate-200 bg-white px-4 py-3 text-sm text-slate-700 file:mr-4 file:rounded-full file:border-0 file:bg-slate-950 file:px-4 file:py-2 file:text-sm file:font-semibold file:text-white"
                            />
                            <p className="mt-2 text-xs text-slate-500">
                              {product.imageFile ? product.imageFile.name : "Upload a product image. Leave empty to keep current image."}
                            </p>
                          </label>

                          <div className="flex flex-wrap gap-3 sm:col-span-2">
                            <label className="inline-flex items-center gap-2 rounded-full bg-white px-4 py-2 text-sm font-semibold text-slate-700">
                              <input
                                type="checkbox"
                                checked={product.isFeatured}
                                onChange={(event) => updateProduct(index, "isFeatured", event.target.checked)}
                              />
                              Featured
                            </label>
                            <label className="inline-flex items-center gap-2 rounded-full bg-white px-4 py-2 text-sm font-semibold text-slate-700">
                              <input
                                type="checkbox"
                                checked={product.isDiscounted}
                                onChange={(event) => updateProduct(index, "isDiscounted", event.target.checked)}
                              />
                              Discounted
                            </label>
                          </div>
                        </div>
                      </article>
                    ))
                  )}

                  <button
                    type="button"
                    onClick={addProduct}
                    className="rounded-full border border-slate-300 bg-white px-4 py-2 text-sm font-semibold text-slate-700 transition hover:-translate-y-0.5 hover:border-amber-300"
                  >
                    Add Product
                  </button>
                </div>
              </FormSection>

              <FormSection
                eyebrow="Voucher Board"
                title="Current offer"
                description="This short offer appears in the visit information panel. Voucher records are loaded from the voucher catalog."
              >
                <TextInput field="offer" label="Offer" placeholder="Opening promotion" form={form} onChange={handleChange} />
              </FormSection>

              <div className="flex flex-wrap gap-3">
                <button
                  type="submit"
                  disabled={saving}
                  className="rounded-full bg-slate-950 px-5 py-3 text-sm font-semibold text-white shadow-lg transition hover:-translate-y-0.5 disabled:cursor-not-allowed disabled:opacity-60"
                >
                  {saving ? "Saving..." : editingShopId ? "Update Shop" : "Create Shop"}
                </button>
                {editingShopId && (
                  <button
                    type="button"
                    onClick={resetForm}
                    className="rounded-full border border-slate-200 bg-white px-5 py-3 text-sm font-semibold text-slate-700 transition hover:-translate-y-0.5"
                  >
                    Cancel
                  </button>
                )}
              </div>
            </form>
            )}
          </section>

          <section className="rounded-[30px] border border-slate-200 bg-white/90 shadow-[0_24px_90px_rgba(15,23,42,0.08)] backdrop-blur-xl">
            <div className="border-b border-slate-200 px-5 py-4 sm:px-6">
              <p className="text-xs font-semibold uppercase tracking-[0.24em] text-slate-400">
                Your Shops
              </p>
              <h2 className="mt-2 text-2xl font-black text-slate-950">Owned shop pages</h2>
            </div>

            {loading ? (
              <div className="px-6 py-8 text-sm text-slate-500">Loading your shops...</div>
            ) : shops.length === 0 ? (
              <div className="px-6 py-8 text-sm text-slate-500">
                You have not created any public shop page yet.
              </div>
            ) : (
              <div className="divide-y divide-slate-200">
                {shops.map((shop) => (
                  <article key={shop.id} className="grid gap-4 px-5 py-5 sm:grid-cols-[120px_1fr] sm:px-6">
                    <div className="h-28 overflow-hidden rounded-3xl bg-slate-100">
                      {shop.coverImageUrl ? (
                        <img src={getImageUrl(shop.coverImageUrl)} alt={shop.name} className="h-full w-full object-cover" />
                      ) : (
                        <div className="flex h-full items-center justify-center text-3xl font-black text-slate-300">
                          {shop.name.charAt(0).toUpperCase()}
                        </div>
                      )}
                    </div>
                    <div className="min-w-0">
                      <div className="flex flex-col gap-3 lg:flex-row lg:items-start lg:justify-between">
                        <div>
                          <h3 className="text-xl font-black text-slate-950">{shop.name}</h3>
                          <p className="mt-1 text-sm text-slate-500">
                            /shops/{shop.slug} · {shop.floor}, {shop.locationSlot}
                          </p>
                        </div>
                        <div className="flex flex-wrap gap-2">
                          <Link
                            to={`/shops/${shop.slug}`}
                            className="rounded-full bg-slate-950 px-4 py-2 text-xs font-semibold text-white"
                          >
                            View
                          </Link>
                          <button
                            type="button"
                            onClick={() => handleEdit(shop)}
                            className="rounded-full bg-amber-400 px-4 py-2 text-xs font-semibold text-slate-950"
                          >
                            Edit
                          </button>
                          <button
                            type="button"
                            onClick={() => handleDelete(shop)}
                            className="rounded-full bg-rose-500 px-4 py-2 text-xs font-semibold text-white"
                          >
                            Delete
                          </button>
                        </div>
                      </div>
                      <p className="mt-3 line-clamp-2 text-sm leading-6 text-slate-600">{shop.summary}</p>
                      <div className="mt-3 flex flex-wrap gap-2">
                        {(shop.tags || []).map((tag) => (
                          <span key={tag} className="rounded-full bg-slate-100 px-3 py-1 text-xs font-semibold text-slate-600">
                            {tag}
                          </span>
                        ))}
                      </div>
                    </div>
                  </article>
                ))}
              </div>
            )}
          </section>
        </main>
      </div>
    </div>
  );
}
