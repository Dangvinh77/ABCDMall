import { useEffect, useState } from "react";
import { ImagePlus, LoaderCircle } from "lucide-react";
import { getImageUrl } from "../../../core/utils/image";
import { Badge } from "../../movies/component/ui/badge";
import {
  AdminDateInput,
  formatDateForDisplay,
  parseDisplayDateToIsoBoundary,
} from "../components/AdminDateInput";
import {
  type MoviesAdminPromotion,
  type MoviesAdminPromotionRule,
  type MoviesAdminPromotionUpsertRequest,
  moviesAdminApi,
} from "../services/moviesAdminApi";

const emptyRule: MoviesAdminPromotionRule = {
  ruleType: "PaymentProvider",
  ruleValue: "",
  thresholdValue: undefined,
  sortOrder: 0,
  isRequired: true,
};

const emptyForm: MoviesAdminPromotionUpsertRequest = {
  code: "",
  name: "",
  description: "",
  category: "ticket",
  status: "Draft",
  validFromUtc: "",
  validToUtc: "",
  percentageValue: undefined,
  flatDiscountValue: undefined,
  maximumDiscountAmount: undefined,
  minimumSpendAmount: undefined,
  maxRedemptions: undefined,
  maxRedemptionsPerCustomer: undefined,
  isAutoApplied: false,
  imageUrl: "",
  badgeText: "",
  accentFrom: "",
  accentTo: "",
  displayCondition: "",
  isFeatured: false,
  displayPriority: 0,
  metadataJson: "",
  rules: [],
};

function toLocalDateTimeInput(value?: string | null) {
  return formatDateForDisplay(value);
}

export function MoviesAdminPromotionsPage() {
  const [promotions, setPromotions] = useState<MoviesAdminPromotion[]>([]);
  const [form, setForm] = useState<MoviesAdminPromotionUpsertRequest>(emptyForm);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [uploadingImage, setUploadingImage] = useState(false);
  const [error, setError] = useState("");
  const [filters, setFilters] = useState({
    status: "",
    query: "",
    activeOnly: true,
  });

  async function loadPromotions() {
    try {
      setLoading(true);
      setError("");
      setPromotions(await moviesAdminApi.getPromotions({
        status: filters.status || undefined,
        query: filters.query || undefined,
        activeOnly: filters.activeOnly || undefined,
      }));
    } catch (err) {
      setError(err instanceof Error ? err.message : "Unable to load promotions.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void loadPromotions();
  }, [filters]);

  function resetForm() {
    setEditingId(null);
    setForm(emptyForm);
  }

  function addRule() {
    setForm((current) => ({
      ...current,
      rules: [...current.rules, { ...emptyRule, sortOrder: current.rules.length }],
    }));
  }

  function updateRule(index: number, patch: Partial<MoviesAdminPromotionRule>) {
    setForm((current) => ({
      ...current,
      rules: current.rules.map((rule, ruleIndex) =>
        ruleIndex === index ? { ...rule, ...patch } : rule),
    }));
  }

  function removeRule(index: number) {
    setForm((current) => ({
      ...current,
      rules: current.rules
        .filter((_, ruleIndex) => ruleIndex !== index)
        .map((rule, ruleIndex) => ({ ...rule, sortOrder: ruleIndex })),
    }));
  }

  async function handleEdit(promotionId: string) {
    try {
      setError("");
      const promotion = await moviesAdminApi.getPromotionById(promotionId);
      setEditingId(promotion.id);
      setForm({
        code: promotion.code,
        name: promotion.name,
        description: promotion.description,
        category: promotion.category,
        status: promotion.status,
        validFromUtc: toLocalDateTimeInput(promotion.validFromUtc),
        validToUtc: toLocalDateTimeInput(promotion.validToUtc),
        percentageValue: promotion.percentageValue ?? undefined,
        flatDiscountValue: promotion.flatDiscountValue ?? undefined,
        maximumDiscountAmount: promotion.maximumDiscountAmount ?? undefined,
        minimumSpendAmount: promotion.minimumSpendAmount ?? undefined,
        maxRedemptions: promotion.maxRedemptions ?? undefined,
        maxRedemptionsPerCustomer: promotion.maxRedemptionsPerCustomer ?? undefined,
        isAutoApplied: promotion.isAutoApplied,
        imageUrl: promotion.imageUrl ?? "",
        badgeText: promotion.badgeText ?? "",
        accentFrom: promotion.accentFrom ?? "",
        accentTo: promotion.accentTo ?? "",
        displayCondition: promotion.displayCondition ?? "",
        isFeatured: promotion.isFeatured,
        displayPriority: promotion.displayPriority,
        metadataJson: promotion.metadataJson ?? "",
        rules: promotion.rules.map((rule) => ({
          ruleType: rule.ruleType,
          ruleValue: rule.ruleValue,
          thresholdValue: rule.thresholdValue ?? undefined,
          sortOrder: rule.sortOrder,
          isRequired: rule.isRequired,
        })),
      });
    } catch (err) {
      setError(err instanceof Error ? err.message : "Unable to load promotion detail.");
    }
  }

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();

    try {
      setSubmitting(true);
      setError("");

      const payload: MoviesAdminPromotionUpsertRequest = {
        ...form,
        validFromUtc: parseDisplayDateToIsoBoundary(form.validFromUtc ?? "", "start"),
        validToUtc: parseDisplayDateToIsoBoundary(form.validToUtc ?? "", "end"),
        imageUrl: form.imageUrl || undefined,
        badgeText: form.badgeText || undefined,
        accentFrom: form.accentFrom || undefined,
        accentTo: form.accentTo || undefined,
        displayCondition: form.displayCondition || undefined,
        metadataJson: form.metadataJson || undefined,
        rules: form.rules.map((rule, index) => ({
          ...rule,
          sortOrder: index,
          thresholdValue: rule.thresholdValue ?? undefined,
        })),
      };

      if (editingId) {
        await moviesAdminApi.updatePromotion(editingId, payload);
      } else {
        await moviesAdminApi.createPromotion(payload);
      }

      resetForm();
      await loadPromotions();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Unable to save promotion.");
    } finally {
      setSubmitting(false);
    }
  }

  async function handleDisable(promotionId: string) {
    try {
      setError("");
      await moviesAdminApi.deletePromotion(promotionId);
      if (editingId === promotionId) {
        resetForm();
      }
      await loadPromotions();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Unable to disable promotion.");
    }
  }

  async function handleImageSelected(file?: File | null) {
    if (!file) {
      return;
    }

    try {
      setUploadingImage(true);
      setError("");
      const result = await moviesAdminApi.uploadPromotionImage(file);
      setForm((current) => ({ ...current, imageUrl: result.imageUrl }));
    } catch (err) {
      setError(err instanceof Error ? err.message : "Unable to upload promotion image.");
    } finally {
      setUploadingImage(false);
    }
  }

  return (
    <div className="space-y-6">
      <section className="rounded-[2rem] border border-white/8 bg-[linear-gradient(180deg,rgba(255,255,255,0.045),rgba(255,255,255,0.02))] p-6 shadow-[0_24px_80px_rgba(2,6,23,0.28)]">
        <div className="flex items-start justify-between gap-4">
          <div>
            <p className="text-xs font-semibold uppercase tracking-[0.28em] text-cyan-200/70">
              Promotions
            </p>
            <h1 className="mt-2 text-3xl font-black uppercase tracking-[0.08em] text-white">
              Campaign operations
            </h1>
            <p className="mt-3 max-w-3xl text-sm leading-7 text-gray-400">
              Create, schedule, activate and retire promotion campaigns for ticket, combo and payment flows.
            </p>
          </div>
          <Badge className="border border-white/10 bg-white/[0.04] px-3 py-1.5 text-gray-200">
            {promotions.length} campaigns
          </Badge>
        </div>
      </section>

      {error ? (
        <div className="rounded-3xl border border-red-500/20 bg-red-500/10 px-4 py-3 text-sm text-red-200">
          {error}
        </div>
      ) : null}

      <section className="grid gap-4 lg:grid-cols-3">
        <input
          value={filters.query}
          onChange={(event) => setFilters((current) => ({ ...current, query: event.target.value }))}
          placeholder="Search code, name, description..."
          className="rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white"
        />
        <select
          value={filters.status}
          onChange={(event) => setFilters((current) => ({ ...current, status: event.target.value }))}
          className="rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white"
        >
          <option value="">All statuses</option>
          <option value="Draft">Draft</option>
          <option value="Scheduled">Scheduled</option>
          <option value="Active">Active</option>
          <option value="Inactive">Inactive</option>
          <option value="Expired">Expired</option>
        </select>
        <div className="flex items-center gap-3 rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white">
          <label className="flex items-center gap-2">
            <input
              type="checkbox"
              checked={filters.activeOnly}
              onChange={(event) => setFilters((current) => ({ ...current, activeOnly: event.target.checked }))}
            />
            Active only
          </label>
        </div>
      </section>

      <section className="grid gap-6 xl:grid-cols-[420px_minmax(0,1fr)]">
        <form
          onSubmit={handleSubmit}
          className="space-y-4 rounded-[2rem] border border-white/8 bg-white/[0.03] p-6 shadow-[0_24px_80px_rgba(2,6,23,0.34)]"
        >
          <div className="flex items-center justify-between">
            <h2 className="text-lg font-black uppercase tracking-[0.08em] text-white">
              {editingId ? "Edit promotion" : "Create promotion"}
            </h2>
            {editingId ? (
              <button type="button" className="text-sm text-cyan-200" onClick={resetForm}>
                Reset
              </button>
            ) : null}
          </div>

          <input value={form.code} onChange={(event) => setForm((current) => ({ ...current, code: event.target.value }))} placeholder="Code" className="w-full rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white" />
          <input value={form.name} onChange={(event) => setForm((current) => ({ ...current, name: event.target.value }))} placeholder="Name" className="w-full rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white" />
          <textarea value={form.description} onChange={(event) => setForm((current) => ({ ...current, description: event.target.value }))} placeholder="Description" rows={3} className="w-full rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white" />
          <div className="grid gap-4 md:grid-cols-2">
            <select value={form.category} onChange={(event) => setForm((current) => ({ ...current, category: event.target.value }))} className="rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white">
              <option value="ticket">Ticket</option>
              <option value="combo">Combo</option>
              <option value="bank">Bank / Wallet</option>
              <option value="member">Member</option>
              <option value="weekend">Weekend</option>
              <option value="all">General</option>
            </select>
            <select value={form.status} onChange={(event) => setForm((current) => ({ ...current, status: event.target.value }))} className="rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white">
              <option value="Draft">Draft</option>
              <option value="Scheduled">Scheduled</option>
              <option value="Active">Active</option>
              <option value="Inactive">Inactive</option>
              <option value="Expired">Expired</option>
            </select>
            <label className="flex items-center gap-2 rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white">
              <input type="checkbox" checked={form.isAutoApplied} onChange={(event) => setForm((current) => ({ ...current, isAutoApplied: event.target.checked }))} />
              Auto apply
            </label>
          </div>
          <div className="grid gap-4 md:grid-cols-2">
            <div className="space-y-3">
              <input value={form.imageUrl ?? ""} onChange={(event) => setForm((current) => ({ ...current, imageUrl: event.target.value }))} placeholder="Image URL" className="w-full rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white" />
              <label className="flex cursor-pointer items-center justify-center gap-2 rounded-2xl border border-dashed border-cyan-300/30 bg-cyan-500/5 px-4 py-3 text-sm font-semibold text-cyan-100 transition hover:border-cyan-300/60 hover:bg-cyan-500/10">
                {uploadingImage ? <LoaderCircle className="size-4 animate-spin" /> : <ImagePlus className="size-4" />}
                {uploadingImage ? "Uploading image..." : "Choose image from computer"}
                <input
                  type="file"
                  accept="image/*"
          className="hidden"
                  onChange={(event) => void handleImageSelected(event.target.files?.[0] ?? null)}
                />
              </label>
              {form.imageUrl ? (
                <div className="overflow-hidden rounded-2xl border border-white/10 bg-slate-950/40">
                  <img
                    src={getImageUrl(form.imageUrl)}
                    alt="Promotion preview"
                    className="h-40 w-full object-cover"
                  />
                </div>
              ) : null}
            </div>
            <input value={form.badgeText ?? ""} onChange={(event) => setForm((current) => ({ ...current, badgeText: event.target.value }))} placeholder="Badge text" className="rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white" />
          </div>
          <div className="grid gap-4 md:grid-cols-2">
            <input value={form.accentFrom ?? ""} onChange={(event) => setForm((current) => ({ ...current, accentFrom: event.target.value }))} placeholder="Accent from (#9333ea)" className="rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white" />
            <input value={form.accentTo ?? ""} onChange={(event) => setForm((current) => ({ ...current, accentTo: event.target.value }))} placeholder="Accent to (#ec4899)" className="rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white" />
          </div>
          <div className="grid gap-4 md:grid-cols-[minmax(0,1fr)_140px]">
            <input value={form.displayCondition ?? ""} onChange={(event) => setForm((current) => ({ ...current, displayCondition: event.target.value }))} placeholder="Display condition" className="rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white" />
            <input type="number" value={form.displayPriority} onChange={(event) => setForm((current) => ({ ...current, displayPriority: Number(event.target.value) || 0 }))} placeholder="Priority" className="rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white" />
          </div>
          <label className="flex items-center gap-2 rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white">
            <input type="checkbox" checked={form.isFeatured} onChange={(event) => setForm((current) => ({ ...current, isFeatured: event.target.checked }))} />
            Featured on public pages
          </label>
          <div className="grid gap-4 md:grid-cols-2">
            <div className="space-y-2">
              <p className="text-xs font-semibold uppercase tracking-[0.18em] text-gray-500">Valid from</p>
              <AdminDateInput
                value={form.validFromUtc ?? ""}
                onChange={(value) => setForm((current) => ({ ...current, validFromUtc: value }))}
              />
            </div>
            <div className="space-y-2">
              <p className="text-xs font-semibold uppercase tracking-[0.18em] text-gray-500">Valid to</p>
              <AdminDateInput
                value={form.validToUtc ?? ""}
                onChange={(value) => setForm((current) => ({ ...current, validToUtc: value }))}
              />
            </div>
          </div>
          <div className="grid gap-4 md:grid-cols-2">
            <input type="number" value={form.percentageValue ?? ""} onChange={(event) => setForm((current) => ({ ...current, percentageValue: event.target.value ? Number(event.target.value) : undefined }))} placeholder="Percentage discount" className="rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white" />
            <input type="number" value={form.flatDiscountValue ?? ""} onChange={(event) => setForm((current) => ({ ...current, flatDiscountValue: event.target.value ? Number(event.target.value) : undefined }))} placeholder="Flat discount" className="rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white" />
          </div>
          <div className="grid gap-4 md:grid-cols-2">
            <input type="number" value={form.maximumDiscountAmount ?? ""} onChange={(event) => setForm((current) => ({ ...current, maximumDiscountAmount: event.target.value ? Number(event.target.value) : undefined }))} placeholder="Max discount amount" className="rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white" />
            <input type="number" value={form.minimumSpendAmount ?? ""} onChange={(event) => setForm((current) => ({ ...current, minimumSpendAmount: event.target.value ? Number(event.target.value) : undefined }))} placeholder="Minimum spend" className="rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white" />
          </div>
          <div className="grid gap-4 md:grid-cols-2">
            <input type="number" value={form.maxRedemptions ?? ""} onChange={(event) => setForm((current) => ({ ...current, maxRedemptions: event.target.value ? Number(event.target.value) : undefined }))} placeholder="Max redemptions" className="rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white" />
            <input type="number" value={form.maxRedemptionsPerCustomer ?? ""} onChange={(event) => setForm((current) => ({ ...current, maxRedemptionsPerCustomer: event.target.value ? Number(event.target.value) : undefined }))} placeholder="Max/customer" className="rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white" />
          </div>
          <textarea value={form.metadataJson ?? ""} onChange={(event) => setForm((current) => ({ ...current, metadataJson: event.target.value }))} placeholder="Metadata JSON (optional)" rows={2} className="w-full rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white" />

          <div className="space-y-3 rounded-2xl border border-white/8 bg-white/[0.02] p-4">
            <div className="flex items-center justify-between">
              <p className="text-sm font-semibold text-white">Rules</p>
              <button type="button" onClick={addRule} className="text-sm text-cyan-200">Add rule</button>
            </div>
            {form.rules.length === 0 ? <p className="text-sm text-gray-400">No rules yet. Add rules for provider, combo, seat type, coupon code, or business date.</p> : null}
            {form.rules.map((rule, index) => (
              <div key={`${rule.ruleType}-${index}`} className="space-y-3 rounded-2xl border border-white/8 bg-slate-950/30 p-3">
                <div className="grid gap-3 md:grid-cols-2">
                  <select value={rule.ruleType} onChange={(event) => updateRule(index, { ruleType: event.target.value })} className="rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white">
                    <option value="MinimumSpend">MinimumSpend</option>
                    <option value="SeatCount">SeatCount</option>
                    <option value="SeatType">SeatType</option>
                    <option value="Showtime">Showtime</option>
                    <option value="BusinessDate">BusinessDate</option>
                    <option value="PaymentProvider">PaymentProvider</option>
                    <option value="Combo">Combo</option>
                    <option value="CouponCode">CouponCode</option>
                    <option value="BirthdayMonth">BirthdayMonth</option>
                  </select>
                  <input value={rule.ruleValue} onChange={(event) => updateRule(index, { ruleValue: event.target.value })} placeholder="Rule value" className="rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white" />
                </div>
                <div className="grid gap-3 md:grid-cols-3">
                  <input type="number" value={rule.thresholdValue ?? ""} onChange={(event) => updateRule(index, { thresholdValue: event.target.value ? Number(event.target.value) : undefined })} placeholder="Threshold" className="rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white" />
                  <input type="number" value={rule.sortOrder} onChange={(event) => updateRule(index, { sortOrder: Number(event.target.value) })} placeholder="Sort order" className="rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white" />
                  <label className="flex items-center gap-2 rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white">
                    <input type="checkbox" checked={rule.isRequired} onChange={(event) => updateRule(index, { isRequired: event.target.checked })} />
                    Required
                  </label>
                </div>
                <button type="button" onClick={() => removeRule(index)} className="text-sm text-rose-200">Remove rule</button>
              </div>
            ))}
          </div>

          <button type="submit" disabled={submitting} className="w-full rounded-2xl bg-white px-4 py-3 text-sm font-semibold text-slate-950 transition hover:bg-white/90 disabled:opacity-50">
            {submitting ? "Saving..." : editingId ? "Update promotion" : "Create promotion"}
          </button>
        </form>

        <div className="overflow-hidden rounded-[2rem] border border-white/8 bg-white/[0.03] shadow-[0_24px_80px_rgba(2,6,23,0.34)]">
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-white/8 text-left text-sm">
              <thead className="bg-white/[0.04]">
                <tr>
                  {["Code", "Name", "Status", "Discount", "Rules", "Redemptions", "Actions"].map((column) => (
                    <th key={column} className="px-4 py-3 text-xs font-semibold uppercase tracking-[0.18em] text-gray-500">{column}</th>
                  ))}
                </tr>
              </thead>
              <tbody className="divide-y divide-white/8">
                {loading ? <tr><td colSpan={7} className="px-4 py-6 text-gray-400">Loading promotions...</td></tr> : null}
                {!loading && promotions.length === 0 ? <tr><td colSpan={7} className="px-4 py-6 text-gray-400">No promotions found.</td></tr> : null}
                {promotions.map((promotion) => (
                  <tr key={promotion.id} className="bg-white/[0.02]">
                    <td className="px-4 py-3 text-white">{promotion.code}</td>
                    <td className="px-4 py-3 text-gray-300">
                      <div>{promotion.name}</div>
                      <div className="text-xs text-gray-500">{promotion.description}</div>
                    </td>
                    <td className="px-4 py-3 text-gray-300">
                      <div>{promotion.status}</div>
                      <div className="text-xs text-gray-500">{promotion.category}</div>
                    </td>
                    <td className="px-4 py-3 text-gray-300">
                      {promotion.percentageValue ? `${promotion.percentageValue}%` : null}
                      {promotion.percentageValue && promotion.flatDiscountValue ? " / " : null}
                      {promotion.flatDiscountValue ? promotion.flatDiscountValue.toLocaleString("vi-VN") : null}
                    </td>
                    <td className="px-4 py-3 text-gray-300">{promotion.ruleCount}</td>
                    <td className="px-4 py-3 text-gray-300">{promotion.redemptionCount}</td>
                    <td className="px-4 py-3">
                      <div className="flex gap-2">
                        <button className="text-cyan-200" onClick={() => void handleEdit(promotion.id)}>Edit</button>
                        <button className="text-rose-200" onClick={() => void handleDisable(promotion.id)}>Disable</button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </section>
    </div>
  );
}
