import React, { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import {
  createMyFoodStall,
  deleteMyFoodStall,
  getMyFoodCourtCreationStatus,
  getMyFoodStalls,
  updateMyFoodStall,
} from "../../food/api/foodApi";
import { getImageUrl } from "../../../core/utils/image";

const emptyMenuItem = {
  id: "",
  name: "",
  price: "",
  note: "",
  tag: "",
  imageUrl: "",
  ingredientsText: "",
  isAvailable: true,
};

const emptyForm = {
  name: "",
  slug: "",
  description: "",
  imageUrl: "",
  imageFile: null,
  categorySlug: "",
  location: "",
  openHours: "09:00 - 22:00",
  phone: "",
  promo: "",
  isActive: true,
  menuItems: [{ ...emptyMenuItem }],
};

const toSlug = (value) =>
  value
    .trim()
    .toLowerCase()
    .replace(/[^a-z0-9\s-]/g, "")
    .replace(/\s+/g, "-")
    .replace(/-+/g, "-");

const toForm = (stall) => ({
  name: stall.name || "",
  slug: stall.slug || "",
  description: stall.description || "",
  imageUrl: stall.imageUrl || "",
  imageFile: null,
  categorySlug: stall.categorySlug || "",
  location: stall.location || "",
  openHours: stall.openHours || "09:00 - 22:00",
  phone: stall.phone || "",
  promo: stall.promo || "",
  isActive: stall.isActive ?? true,
  menuItems:
    stall.menuItems?.length > 0
      ? stall.menuItems.map((item) => ({
          id: item.id || "",
          name: item.name || "",
          price: item.price?.toString() || "",
          note: item.note || "",
          tag: item.tag || "",
          imageUrl: item.imageUrl || "",
          ingredientsText: (item.ingredients || []).join(", "),
          isAvailable: item.isAvailable ?? true,
        }))
      : [{ ...emptyMenuItem }],
});

const appendMenuItems = (formData, menuItems) => {
  menuItems
    .filter((item) => item.name.trim() || item.price || item.note.trim() || item.imageUrl.trim())
    .forEach((item, index) => {
      if (item.id) {
        formData.append(`MenuItems[${index}].Id`, item.id);
      }
      formData.append(`MenuItems[${index}].Name`, item.name.trim());
      formData.append(`MenuItems[${index}].Price`, String(Number(item.price || 0)));
      formData.append(`MenuItems[${index}].Note`, item.note.trim());
      formData.append(`MenuItems[${index}].Tag`, item.tag.trim());
      formData.append(`MenuItems[${index}].ImageUrl`, item.imageUrl.trim());
      item.ingredientsText
        .split(",")
        .map((ingredient) => ingredient.trim())
        .filter(Boolean)
        .forEach((ingredient, ingredientIndex) => {
          formData.append(`MenuItems[${index}].Ingredients[${ingredientIndex}]`, ingredient);
        });
      formData.append(`MenuItems[${index}].IsAvailable`, String(item.isAvailable));
      formData.append(`MenuItems[${index}].DisplayOrder`, String(index + 1));
    });
};

const toRequest = (form) => {
  const formData = new FormData();
  formData.append("name", form.name.trim());
  formData.append("slug", toSlug(form.slug || form.name));
  formData.append("description", form.description.trim());
  formData.append("imageUrl", form.imageUrl.trim());
  formData.append("categorySlug", form.categorySlug.trim());
  formData.append("location", form.location.trim());
  formData.append("openHours", form.openHours.trim());
  formData.append("phone", form.phone.trim());
  formData.append("promo", form.promo.trim());
  formData.append("isActive", String(form.isActive));
  if (form.imageFile) {
    formData.append("imageFile", form.imageFile);
  }
  appendMenuItems(formData, form.menuItems);
  return formData;
};

export default function FoodCourtManager() {
  const [stalls, setStalls] = useState([]);
  const [creationStatus, setCreationStatus] = useState(null);
  const [form, setForm] = useState(emptyForm);
  const [editingStallId, setEditingStallId] = useState(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  const availableRentalLocations = creationStatus?.availableRentalLocations || [];
  const selectedLocation = availableRentalLocations.find((location) => location.locationSlot === form.location);

  const loadData = async () => {
    try {
      setLoading(true);
      setError("");
      const [statusData, stallsData] = await Promise.all([
        getMyFoodCourtCreationStatus(),
        getMyFoodStalls(),
      ]);
      setCreationStatus(statusData);
      setStalls(stallsData);
    } catch (err) {
      setError(err.message || "Unable to load food-court manager data.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  useEffect(() => {
    if (editingStallId) {
      return;
    }

    if (availableRentalLocations.length === 1) {
      setForm((current) => ({
        ...current,
        location: availableRentalLocations[0].locationSlot || "",
      }));
    }
  }, [availableRentalLocations, editingStallId]);

  const canCreate = useMemo(() => {
    if (editingStallId) {
      return true;
    }
    return creationStatus?.canCreate ?? false;
  }, [creationStatus, editingStallId]);

  const handleChange = (field, value) => {
    setForm((current) => ({
      ...current,
      [field]: field === "slug" ? toSlug(value) : value,
      ...(field === "name" && !editingStallId && !current.slug ? { slug: toSlug(value) } : {}),
    }));
  };

  const handleMenuChange = (index, field, value) => {
    setForm((current) => ({
      ...current,
      menuItems: current.menuItems.map((item, itemIndex) =>
        itemIndex === index ? { ...item, [field]: value } : item,
      ),
    }));
  };

  const addMenuItem = () => {
    setForm((current) => ({
      ...current,
      menuItems: [...current.menuItems, { ...emptyMenuItem }],
    }));
  };

  const removeMenuItem = (index) => {
    setForm((current) => ({
      ...current,
      menuItems:
        current.menuItems.length === 1
          ? [{ ...emptyMenuItem }]
          : current.menuItems.filter((_, itemIndex) => itemIndex !== index),
    }));
  };

  const resetForm = () => {
    setForm(emptyForm);
    setEditingStallId(null);
    setError("");
    setSuccess("");
  };

  const handleEdit = (stall) => {
    setEditingStallId(stall.id);
    setForm(toForm(stall));
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
      if (editingStallId) {
        await updateMyFoodStall(editingStallId, request);
        setSuccess("Food stall updated successfully.");
      } else {
        await createMyFoodStall(request);
        setSuccess("Food stall created successfully.");
      }

      resetForm();
      await loadData();
    } catch (err) {
      setError(err.message || "Unable to save food stall.");
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (stall) => {
    const confirmed = window.confirm(`Delete ${stall.name}? This action cannot be undone.`);
    if (!confirmed) {
      return;
    }

    try {
      setError("");
      setSuccess("");
      await deleteMyFoodStall(stall.id);
      setSuccess("Food stall deleted successfully.");
      if (editingStallId === stall.id) {
        resetForm();
      }
      await loadData();
    } catch (err) {
      setError(err.message || "Unable to delete food stall.");
    }
  };

  return (
    <div className="min-h-screen bg-[linear-gradient(180deg,#fff8ef_0%,#fffdf8_42%,#f8fbff_100%)] text-slate-900">
      <div className="mx-auto flex min-h-screen w-full max-w-7xl flex-col px-4 pb-6 pt-28 sm:px-6 lg:px-8">
        <header className="rounded-[28px] border border-white/70 bg-white/80 px-5 py-4 shadow-[0_20px_80px_rgba(15,23,42,0.08)] backdrop-blur-xl sm:px-6">
          <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
            <div>
              <div className="inline-flex items-center gap-2 rounded-full bg-amber-100 px-3 py-1 text-xs font-semibold uppercase tracking-[0.28em] text-amber-700">
                Food Court Manager
              </div>
              <h1 className="mt-3 text-3xl font-black tracking-tight sm:text-4xl">My Food Stall Management</h1>
              <p className="mt-2 max-w-2xl text-sm text-slate-600 sm:text-base">
                Manage your rented food-court stall profile and the menu items shown on the public storefront.
              </p>
            </div>

            <Link to="/dashboard" className="inline-flex items-center justify-center rounded-full bg-slate-950 px-5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5">
              Back to Dashboard
            </Link>
          </div>
        </header>

        <main className="mt-6 grid flex-1 gap-6 xl:grid-cols-[0.95fr_1.05fr]">
          <section className="rounded-[30px] border border-slate-200 bg-white/90 p-5 shadow-[0_24px_90px_rgba(15,23,42,0.08)] backdrop-blur-xl sm:p-6">
            <div className="mb-5">
              <p className="text-xs font-semibold uppercase tracking-[0.24em] text-slate-400">Food stall quota</p>
              <h2 className="mt-2 text-2xl font-black text-slate-950">
                {editingStallId ? "Edit your food stall" : "Create a new food stall"}
              </h2>
            </div>

            {creationStatus ? (
              <div className="mb-5 rounded-3xl border border-slate-200 bg-slate-50 px-4 py-4 text-sm text-slate-600">
                <p className="font-semibold text-slate-900">
                  Created stalls: {creationStatus.stallCount} / Rented food-court slots: {creationStatus.rentedFoodCourtCount}
                </p>
                <p className="mt-1">{creationStatus.message}</p>
                {availableRentalLocations.length > 0 ? (
                  <div className="mt-3 flex flex-wrap gap-2">
                    {availableRentalLocations.map((location) => (
                      <span key={location.locationSlot} className="rounded-full bg-white px-3 py-1 text-xs font-semibold text-slate-700">
                        {location.locationSlot} · Floor {location.floor} · {location.areaName}
                      </span>
                    ))}
                  </div>
                ) : null}
              </div>
            ) : null}

            {error ? <div className="mb-4 rounded-2xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-600">{error}</div> : null}
            {success ? <div className="mb-4 rounded-2xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-700">{success}</div> : null}

            {!loading && !canCreate ? (
              <div className="rounded-[28px] border border-amber-200 bg-amber-50 px-5 py-6 text-sm leading-6 text-amber-800">
                <p className="text-base font-black text-amber-950">Create stall is currently unavailable</p>
                <p className="mt-2">{creationStatus?.message || "All rented food-court slots already have a managed stall."}</p>
              </div>
            ) : (
              <form onSubmit={handleSubmit} className="space-y-5">
                <div className="grid gap-4 sm:grid-cols-2">
                  <label className="block">
                    <span className="mb-2 block text-sm font-semibold text-slate-700">Stall Name</span>
                    <input value={form.name} onChange={(event) => handleChange("name", event.target.value)} className="w-full rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3 text-slate-800 outline-none transition focus:border-amber-400 focus:bg-white focus:ring-4 focus:ring-amber-100" />
                  </label>
                  <label className="block">
                    <span className="mb-2 block text-sm font-semibold text-slate-700">Slug</span>
                    <input value={form.slug} onChange={(event) => handleChange("slug", event.target.value)} className="w-full rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3 text-slate-800 outline-none transition focus:border-amber-400 focus:bg-white focus:ring-4 focus:ring-amber-100" />
                  </label>
                  <label className="block">
                    <span className="mb-2 block text-sm font-semibold text-slate-700">Category</span>
                    <input value={form.categorySlug} onChange={(event) => handleChange("categorySlug", event.target.value)} className="w-full rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3 text-slate-800 outline-none transition focus:border-amber-400 focus:bg-white focus:ring-4 focus:ring-amber-100" />
                  </label>
                  <label className="block">
                    <span className="mb-2 block text-sm font-semibold text-slate-700">Location</span>
                    {editingStallId ? (
                      <input value={form.location} readOnly className="w-full rounded-2xl border border-slate-200 bg-slate-100 px-4 py-3 text-slate-700 outline-none" />
                    ) : availableRentalLocations.length > 1 ? (
                      <select value={form.location} onChange={(event) => handleChange("location", event.target.value)} className="w-full rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3 text-slate-800 outline-none transition focus:border-amber-400 focus:bg-white focus:ring-4 focus:ring-amber-100">
                        <option value="">Select a rented food-court slot</option>
                        {availableRentalLocations.map((location) => (
                          <option key={location.locationSlot} value={location.locationSlot}>
                            {location.locationSlot}
                          </option>
                        ))}
                      </select>
                    ) : (
                      <input value={form.location} readOnly className="w-full rounded-2xl border border-slate-200 bg-slate-100 px-4 py-3 text-slate-700 outline-none" />
                    )}
                  </label>
                  <label className="block">
                    <span className="mb-2 block text-sm font-semibold text-slate-700">Open Hours</span>
                    <input value={form.openHours} onChange={(event) => handleChange("openHours", event.target.value)} className="w-full rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3 text-slate-800 outline-none transition focus:border-amber-400 focus:bg-white focus:ring-4 focus:ring-amber-100" />
                  </label>
                  <label className="block">
                    <span className="mb-2 block text-sm font-semibold text-slate-700">Phone</span>
                    <input value={form.phone} onChange={(event) => handleChange("phone", event.target.value)} className="w-full rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3 text-slate-800 outline-none transition focus:border-amber-400 focus:bg-white focus:ring-4 focus:ring-amber-100" />
                  </label>
                  <label className="block sm:col-span-2">
                    <span className="mb-2 block text-sm font-semibold text-slate-700">Promo</span>
                    <input value={form.promo} onChange={(event) => handleChange("promo", event.target.value)} className="w-full rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3 text-slate-800 outline-none transition focus:border-amber-400 focus:bg-white focus:ring-4 focus:ring-amber-100" />
                  </label>
                  <label className="block sm:col-span-2">
                    <span className="mb-2 block text-sm font-semibold text-slate-700">Description</span>
                    <textarea value={form.description} rows={4} onChange={(event) => handleChange("description", event.target.value)} className="w-full resize-none rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3 text-slate-800 outline-none transition focus:border-amber-400 focus:bg-white focus:ring-4 focus:ring-amber-100" />
                  </label>
                  <label className="block">
                    <span className="mb-2 block text-sm font-semibold text-slate-700">Image URL</span>
                    <input value={form.imageUrl} onChange={(event) => handleChange("imageUrl", event.target.value)} className="w-full rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3 text-slate-800 outline-none transition focus:border-amber-400 focus:bg-white focus:ring-4 focus:ring-amber-100" />
                  </label>
                  <label className="block">
                    <span className="mb-2 block text-sm font-semibold text-slate-700">Stall Image Upload</span>
                    <input type="file" accept="image/*" onChange={(event) => setForm((current) => ({ ...current, imageFile: event.target.files?.[0] ?? null }))} className="w-full rounded-2xl border border-slate-200 bg-white px-4 py-3 text-sm text-slate-700 file:mr-4 file:rounded-full file:border-0 file:bg-slate-950 file:px-4 file:py-2 file:text-sm file:font-semibold file:text-white" />
                  </label>
                </div>

                {selectedLocation ? (
                  <div className="rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3 text-sm text-slate-600">
                    Selected slot: <span className="font-semibold text-slate-900">{selectedLocation.locationSlot}</span> · Floor {selectedLocation.floor} · {selectedLocation.areaName}
                  </div>
                ) : null}

                <section className="rounded-[28px] border border-slate-200 bg-slate-50 p-4">
                  <div className="mb-4 flex items-center justify-between gap-3">
                    <div>
                      <p className="text-xs font-semibold uppercase tracking-[0.22em] text-slate-400">Menu Items</p>
                      <h3 className="mt-1 text-xl font-black text-slate-950">Food and drink entries</h3>
                    </div>
                    <button type="button" onClick={addMenuItem} className="rounded-full border border-slate-300 bg-white px-4 py-2 text-sm font-semibold text-slate-700">
                      Add Menu Item
                    </button>
                  </div>

                  <div className="space-y-4">
                    {form.menuItems.map((item, index) => (
                      <article key={`${item.id || "new"}-${index}`} className="rounded-3xl border border-slate-200 bg-white p-4">
                        <div className="mb-4 flex items-center justify-between gap-3">
                          <div>
                            <p className="text-xs font-semibold uppercase tracking-[0.18em] text-slate-400">Menu item #{index + 1}</p>
                            <h4 className="mt-1 text-base font-black text-slate-900">{item.name || "New menu item"}</h4>
                          </div>
                          <button type="button" onClick={() => removeMenuItem(index)} className="rounded-full bg-rose-500 px-3 py-2 text-xs font-semibold text-white">
                            Remove
                          </button>
                        </div>

                        <div className="grid gap-4 sm:grid-cols-2">
                          <label className="block">
                            <span className="mb-2 block text-sm font-semibold text-slate-700">Menu Item Name</span>
                            <input value={item.name} onChange={(event) => handleMenuChange(index, "name", event.target.value)} className="w-full rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3 text-slate-800 outline-none transition focus:border-amber-400 focus:bg-white focus:ring-4 focus:ring-amber-100" />
                          </label>
                          <label className="block">
                            <span className="mb-2 block text-sm font-semibold text-slate-700">Menu Item Price</span>
                            <input type="number" min="0" value={item.price} onChange={(event) => handleMenuChange(index, "price", event.target.value)} className="w-full rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3 text-slate-800 outline-none transition focus:border-amber-400 focus:bg-white focus:ring-4 focus:ring-amber-100" />
                          </label>
                          <label className="block">
                            <span className="mb-2 block text-sm font-semibold text-slate-700">Tag</span>
                            <input value={item.tag} onChange={(event) => handleMenuChange(index, "tag", event.target.value)} className="w-full rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3 text-slate-800 outline-none transition focus:border-amber-400 focus:bg-white focus:ring-4 focus:ring-amber-100" />
                          </label>
                          <label className="block">
                            <span className="mb-2 block text-sm font-semibold text-slate-700">Image URL</span>
                            <input value={item.imageUrl} onChange={(event) => handleMenuChange(index, "imageUrl", event.target.value)} className="w-full rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3 text-slate-800 outline-none transition focus:border-amber-400 focus:bg-white focus:ring-4 focus:ring-amber-100" />
                          </label>
                          <label className="block sm:col-span-2">
                            <span className="mb-2 block text-sm font-semibold text-slate-700">Note</span>
                            <textarea value={item.note} rows={3} onChange={(event) => handleMenuChange(index, "note", event.target.value)} className="w-full resize-none rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3 text-slate-800 outline-none transition focus:border-amber-400 focus:bg-white focus:ring-4 focus:ring-amber-100" />
                          </label>
                          <label className="block sm:col-span-2">
                            <span className="mb-2 block text-sm font-semibold text-slate-700">Ingredients</span>
                            <input value={item.ingredientsText} onChange={(event) => handleMenuChange(index, "ingredientsText", event.target.value)} placeholder="Black tea, boba, milk foam" className="w-full rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3 text-slate-800 outline-none transition focus:border-amber-400 focus:bg-white focus:ring-4 focus:ring-amber-100" />
                          </label>
                        </div>
                      </article>
                    ))}
                  </div>
                </section>

                <div className="flex flex-wrap gap-3">
                  <button type="submit" disabled={saving} className="rounded-full bg-slate-950 px-5 py-3 text-sm font-semibold text-white shadow-lg transition hover:-translate-y-0.5 disabled:cursor-not-allowed disabled:opacity-60">
                    {saving ? "Saving..." : editingStallId ? "Update Food Stall" : "Create Food Stall"}
                  </button>
                  {editingStallId ? <button type="button" onClick={resetForm} className="rounded-full border border-slate-200 bg-white px-5 py-3 text-sm font-semibold text-slate-700 transition hover:-translate-y-0.5">Cancel</button> : null}
                </div>
              </form>
            )}
          </section>

          <section className="rounded-[30px] border border-slate-200 bg-white/90 shadow-[0_24px_90px_rgba(15,23,42,0.08)] backdrop-blur-xl">
            <div className="border-b border-slate-200 px-5 py-4 sm:px-6">
              <p className="text-xs font-semibold uppercase tracking-[0.24em] text-slate-400">Your Stalls</p>
              <h2 className="mt-2 text-2xl font-black text-slate-950">Managed food-court profiles</h2>
            </div>

            {loading ? (
              <div className="px-6 py-8 text-sm text-slate-500">Loading your food stalls...</div>
            ) : stalls.length === 0 ? (
              <div className="px-6 py-8 text-sm text-slate-500">You have not created any food stall page yet.</div>
            ) : (
              <div className="divide-y divide-slate-200">
                {stalls.map((stall) => (
                  <article key={stall.id} className="grid gap-4 px-5 py-5 sm:grid-cols-[120px_1fr] sm:px-6">
                    <div className="h-28 overflow-hidden rounded-3xl bg-slate-100">
                      {stall.imageUrl ? (
                        <img src={getImageUrl(stall.imageUrl)} alt={stall.name} className="h-full w-full object-cover" />
                      ) : (
                        <div className="flex h-full items-center justify-center text-3xl font-black text-slate-300">
                          {stall.name?.charAt(0)?.toUpperCase() || "F"}
                        </div>
                      )}
                    </div>
                    <div className="min-w-0">
                      <div className="flex flex-col gap-3 lg:flex-row lg:items-start lg:justify-between">
                        <div>
                          <h3 className="text-xl font-black text-slate-950">{stall.name}</h3>
                          <p className="mt-1 text-sm text-slate-500">/food-court/{stall.slug} · {stall.location}</p>
                        </div>
                        <div className="flex flex-wrap gap-2">
                          <Link to={`/food-court/${stall.slug}`} className="rounded-full bg-slate-950 px-4 py-2 text-xs font-semibold text-white">
                            View
                          </Link>
                          <button type="button" onClick={() => handleEdit(stall)} className="rounded-full bg-amber-400 px-4 py-2 text-xs font-semibold text-slate-950">
                            Edit
                          </button>
                          <button type="button" onClick={() => handleDelete(stall)} className="rounded-full bg-rose-500 px-4 py-2 text-xs font-semibold text-white">
                            Delete
                          </button>
                        </div>
                      </div>
                      <p className="mt-3 line-clamp-2 text-sm leading-6 text-slate-600">{stall.description}</p>
                      <div className="mt-3 flex flex-wrap gap-2">
                        <span className="rounded-full bg-slate-100 px-3 py-1 text-xs font-semibold text-slate-600">{stall.categorySlug}</span>
                        <span className="rounded-full bg-slate-100 px-3 py-1 text-xs font-semibold text-slate-600">{stall.openHours}</span>
                        <span className="rounded-full bg-slate-100 px-3 py-1 text-xs font-semibold text-slate-600">{stall.menuItems?.length || 0} menu items</span>
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
