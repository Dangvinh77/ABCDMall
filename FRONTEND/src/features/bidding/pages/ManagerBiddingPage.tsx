import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { biddingApi, type ManagerCarouselBid, type SubmitCarouselBidPayload, type TemplateType } from "../api/biddingApi";

const acceptedImageFormats = ".jpg,.jpeg,.png,.webp,.gif,.bmp,.avif";

function createEmptyForm(): SubmitCarouselBidPayload {
  return {
    bidAmount: 0,
    templateType: "ShopAd",
    shopImageFile: null,
    message: "",
    productImageFile: null,
    originalPrice: undefined,
    discountPrice: undefined,
    eventImageFile: null,
    startDate: "",
    startTime: "",
  };
}

function formatCurrency(value?: number | null) {
  if (typeof value !== "number") {
    return "N/A";
  }

  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(value);
}

function templateSummary(bid: ManagerCarouselBid) {
  switch (bid.templateType) {
    case "ShopAd":
      return bid.message || "Shop ad";
    case "DiscountAd":
      return `${formatCurrency(bid.originalPrice)} -> ${formatCurrency(bid.discountPrice)}`;
    case "EventAd":
      return [bid.eventStartDate ? new Date(bid.eventStartDate).toLocaleDateString() : null, bid.startTime]
        .filter(Boolean)
        .join(" • ");
    default:
      return "";
  }
}

export default function ManagerBiddingPage() {
  const role = localStorage.getItem("role") || "Guest";
  const isManager = role === "Manager";
  const [form, setForm] = useState<SubmitCarouselBidPayload>(() => createEmptyForm());
  const [formVersion, setFormVersion] = useState(0);
  const [bids, setBids] = useState<ManagerCarouselBid[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  async function loadBids() {
    try {
      setLoading(true);
      setError("");
      setBids(await biddingApi.getManagerBids());
    } catch (err) {
      setError(err instanceof Error ? err.message : "Unable to load bidding history.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    if (isManager) {
      loadBids();
    } else {
      setLoading(false);
    }
  }, [isManager]);

  const targetMondayLabel = bids[0]?.targetMondayDate
    ? new Date(bids[0].targetMondayDate).toLocaleDateString()
    : "next Monday";

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();

    try {
      setSaving(true);
      setError("");
      setSuccess("");

      await biddingApi.submitBid({
        ...form,
        bidAmount: Number(form.bidAmount),
        originalPrice: typeof form.originalPrice === "number" ? Number(form.originalPrice) : undefined,
        discountPrice: typeof form.discountPrice === "number" ? Number(form.discountPrice) : undefined,
      });

      setForm(createEmptyForm());
      setFormVersion((current) => current + 1);
      setSuccess("Bid submitted successfully.");
      await loadBids();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Unable to submit bid.");
    } finally {
      setSaving(false);
    }
  }

  function updateField<K extends keyof SubmitCarouselBidPayload>(field: K, value: SubmitCarouselBidPayload[K]) {
    setForm((current) => ({ ...current, [field]: value }));
  }

  function handleTemplateTypeChange(templateType: TemplateType) {
    setFormVersion((current) => current + 1);
    setForm({
      ...createEmptyForm(),
      bidAmount: form.bidAmount,
      templateType,
    });
  }

  if (!isManager) {
    return (
      <div className="min-h-screen bg-[linear-gradient(180deg,#fff8ef_0%,#fffdf8_42%,#f8fbff_100%)] px-4 py-6 text-slate-900 sm:px-6 lg:px-8">
        <div className="mx-auto max-w-3xl rounded-[28px] border border-slate-200 bg-white/90 p-6 shadow-[0_20px_80px_rgba(15,23,42,0.08)]">
          <p className="text-xs font-semibold uppercase tracking-[0.28em] text-amber-700">Manager Bidding</p>
          <h1 className="mt-3 text-3xl font-black">Manager access only</h1>
          <p className="mt-3 text-sm text-slate-600">This area is only available for manager accounts.</p>
          <Link to="/dashboard" className="mt-6 inline-flex rounded-full bg-slate-950 px-5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5">
            Back to Dashboard
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-[linear-gradient(180deg,#fff8ef_0%,#fffdf8_42%,#f8fbff_100%)] text-slate-900">
      <div className="mx-auto flex min-h-screen w-full max-w-7xl flex-col px-4 py-6 sm:px-6 lg:px-8">
        <header className="rounded-[28px] border border-white/70 bg-white/80 px-5 py-4 shadow-[0_20px_80px_rgba(15,23,42,0.08)] backdrop-blur-xl sm:px-6">
          <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
            <div>
              <div className="inline-flex items-center gap-2 rounded-full bg-amber-100 px-3 py-1 text-xs font-semibold uppercase tracking-[0.28em] text-amber-700">
                Homepage Carousel
              </div>
              <h1 className="mt-3 text-3xl font-black tracking-tight sm:text-4xl">Advertisement Bidding</h1>
              <p className="mt-2 max-w-2xl text-sm text-slate-600 sm:text-base">
                Submit a bid for the homepage carousel slot scheduled for {targetMondayLabel}.
              </p>
            </div>
            <Link to="/dashboard" className="inline-flex items-center justify-center rounded-full bg-slate-950 px-5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5">
              Back to Dashboard
            </Link>
          </div>
        </header>

        <main className="mt-6 grid gap-6 xl:grid-cols-[0.92fr_1.08fr]">
          <section className="rounded-[30px] border border-slate-200 bg-white/90 p-5 shadow-[0_24px_90px_rgba(15,23,42,0.08)] backdrop-blur-xl sm:p-6">
            <p className="text-xs font-semibold uppercase tracking-[0.24em] text-amber-700">Submit Bid</p>
            <h2 className="mt-3 text-2xl font-black">Create next week's ad bid</h2>
            <p className="mt-2 text-sm text-slate-500">
              Upload artwork directly from your device. Supported formats: JPG, JPEG, PNG, WEBP, GIF, BMP, AVIF.
            </p>

            <form onSubmit={handleSubmit} className="mt-6 space-y-4">
              <label className="block">
                <span className="mb-2 block text-sm font-semibold text-slate-700">Bid Amount (USD)</span>
                <input
                  type="number"
                  min="1"
                  step="0.01"
                  value={form.bidAmount || ""}
                  onChange={(event) => updateField("bidAmount", Number(event.target.value))}
                  className="w-full rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3 outline-none transition focus:border-amber-400 focus:bg-white focus:ring-4 focus:ring-amber-100"
                />
              </label>

              <label className="block">
                <span className="mb-2 block text-sm font-semibold text-slate-700">Template Type</span>
                <select
                  value={form.templateType}
                  onChange={(event) => handleTemplateTypeChange(event.target.value as TemplateType)}
                  className="w-full rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3 outline-none transition focus:border-amber-400 focus:bg-white focus:ring-4 focus:ring-amber-100"
                >
                  <option value="ShopAd">Shop Ad</option>
                  <option value="DiscountAd">Discount Ad</option>
                  <option value="EventAd">Event Ad</option>
                </select>
              </label>

              {form.templateType === "ShopAd" && (
                <>
                  <ImageUploadField
                    key={`shop-${formVersion}`}
                    label="Shop Image"
                    file={form.shopImageFile}
                    required
                    onChange={(file) => updateField("shopImageFile", file)}
                  />
                  <TextAreaField label="Message" value={form.message || ""} onChange={(value) => updateField("message", value)} rows={4} />
                </>
              )}

              {form.templateType === "DiscountAd" && (
                <>
                  <ImageUploadField
                    key={`discount-${formVersion}`}
                    label="Product Image"
                    file={form.productImageFile}
                    required
                    onChange={(file) => updateField("productImageFile", file)}
                  />
                  <div className="grid gap-4 sm:grid-cols-2">
                    <NumberField label="Original Price (USD)" value={form.originalPrice} onChange={(value) => updateField("originalPrice", value)} />
                    <NumberField label="Discount Price (USD)" value={form.discountPrice} onChange={(value) => updateField("discountPrice", value)} />
                  </div>
                </>
              )}

              {form.templateType === "EventAd" && (
                <>
                  <ImageUploadField
                    key={`event-${formVersion}`}
                    label="Event Image"
                    file={form.eventImageFile}
                    required
                    onChange={(file) => updateField("eventImageFile", file)}
                  />
                  <div className="grid gap-4 sm:grid-cols-2">
                    <label className="block">
                      <span className="mb-2 block text-sm font-semibold text-slate-700">Start Date</span>
                      <input
                        type="date"
                        value={form.startDate || ""}
                        onChange={(event) => updateField("startDate", event.target.value)}
                        className="w-full rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3 outline-none transition focus:border-amber-400 focus:bg-white focus:ring-4 focus:ring-amber-100"
                      />
                    </label>
                    <label className="block">
                      <span className="mb-2 block text-sm font-semibold text-slate-700">Start Time</span>
                      <input
                        type="time"
                        value={form.startTime || ""}
                        onChange={(event) => updateField("startTime", event.target.value)}
                        className="w-full rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3 outline-none transition focus:border-amber-400 focus:bg-white focus:ring-4 focus:ring-amber-100"
                      />
                    </label>
                  </div>
                </>
              )}

              {error && <p className="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">{error}</p>}
              {success && <p className="rounded-2xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-700">{success}</p>}

              <button
                type="submit"
                disabled={saving}
                className="inline-flex items-center justify-center rounded-full bg-slate-950 px-6 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5 disabled:cursor-not-allowed disabled:opacity-60"
              >
                {saving ? "Submitting..." : "Submit Bid"}
              </button>
            </form>
          </section>

          <section className="rounded-[30px] border border-slate-200 bg-white/90 p-5 shadow-[0_24px_90px_rgba(15,23,42,0.08)] backdrop-blur-xl sm:p-6">
            <p className="text-xs font-semibold uppercase tracking-[0.24em] text-slate-400">History</p>
            <h2 className="mt-3 text-2xl font-black">My bidding history</h2>
            <p className="mt-2 text-sm text-slate-500">Won bids can proceed to Stripe payment immediately.</p>

            {loading ? (
              <div className="mt-6 rounded-3xl border border-dashed border-slate-200 bg-slate-50 p-8 text-center text-sm text-slate-500">
                Loading bids...
              </div>
            ) : bids.length === 0 ? (
              <div className="mt-6 rounded-3xl border border-dashed border-slate-200 bg-slate-50 p-8 text-center text-sm text-slate-500">
                No bids yet.
              </div>
            ) : (
              <div className="mt-6 overflow-hidden rounded-[26px] border border-slate-200">
                <div className="overflow-x-auto">
                  <table className="min-w-full divide-y divide-slate-200 text-sm">
                    <thead className="bg-slate-50 text-left text-slate-500">
                      <tr>
                        <th className="px-4 py-3 font-semibold">Template</th>
                        <th className="px-4 py-3 font-semibold">Summary</th>
                        <th className="px-4 py-3 font-semibold">Bid</th>
                        <th className="px-4 py-3 font-semibold">Status</th>
                        <th className="px-4 py-3 font-semibold">Target Week</th>
                        <th className="px-4 py-3 font-semibold">Action</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-slate-200 bg-white">
                      {bids.map((bid) => (
                        <tr key={bid.id}>
                          <td className="px-4 py-4 font-semibold text-slate-900">{bid.templateType}</td>
                          <td className="px-4 py-4 text-slate-600">{templateSummary(bid)}</td>
                          <td className="px-4 py-4 text-slate-900">{formatCurrency(bid.bidAmount)}</td>
                          <td className="px-4 py-4">
                            <span className={`inline-flex rounded-full px-3 py-1 text-xs font-semibold ${
                              bid.status === "Won"
                                ? "bg-amber-100 text-amber-800"
                                : bid.status === "Paid" || bid.status === "Active"
                                  ? "bg-emerald-100 text-emerald-800"
                                  : bid.status === "Lost" || bid.status === "Expired"
                                    ? "bg-rose-100 text-rose-700"
                                    : "bg-slate-100 text-slate-600"
                            }`}>
                              {bid.status}
                            </span>
                          </td>
                          <td className="px-4 py-4 text-slate-600">{new Date(bid.targetMondayDate).toLocaleDateString()}</td>
                          <td className="px-4 py-4">
                            {bid.status === "Won" ? (
                              <Link to={`/manager-bidding/checkout/${bid.id}`} className="inline-flex rounded-full bg-slate-950 px-4 py-2 text-xs font-semibold text-white transition hover:-translate-y-0.5">
                                Proceed to Payment
                              </Link>
                            ) : (
                              <span className="text-xs text-slate-400">No action</span>
                            )}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </div>
            )}
          </section>
        </main>
      </div>
    </div>
  );
}

function NumberField({ label, value, onChange }: { label: string; value?: number; onChange: (value: number | undefined) => void }) {
  return (
    <label className="block">
      <span className="mb-2 block text-sm font-semibold text-slate-700">{label}</span>
      <input
        type="number"
        min="0"
        step="0.01"
        value={value ?? ""}
        onChange={(event) => onChange(event.target.value ? Number(event.target.value) : undefined)}
        className="w-full rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3 outline-none transition focus:border-amber-400 focus:bg-white focus:ring-4 focus:ring-amber-100"
      />
    </label>
  );
}

function TextAreaField({
  label,
  value,
  onChange,
  rows,
}: {
  label: string;
  value: string;
  onChange: (value: string) => void;
  rows: number;
}) {
  return (
    <label className="block">
      <span className="mb-2 block text-sm font-semibold text-slate-700">{label}</span>
      <textarea
        value={value}
        rows={rows}
        onChange={(event) => onChange(event.target.value)}
        className="w-full resize-none rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3 outline-none transition focus:border-amber-400 focus:bg-white focus:ring-4 focus:ring-amber-100"
      />
    </label>
  );
}

function ImageUploadField({
  label,
  file,
  onChange,
  required = false,
}: {
  label: string;
  file?: File | null;
  onChange: (file: File | null) => void;
  required?: boolean;
}) {
  return (
    <label className="block">
      <span className="mb-2 block text-sm font-semibold text-slate-700">{label}</span>
      <input
        type="file"
        accept={acceptedImageFormats}
        required={required}
        onChange={(event) => onChange(event.target.files?.[0] ?? null)}
        className="block w-full rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3 text-sm text-slate-700 file:mr-4 file:rounded-full file:border-0 file:bg-slate-950 file:px-4 file:py-2 file:text-sm file:font-semibold file:text-white"
      />
      <p className="mt-2 text-xs text-slate-500">
        {file ? `Selected file: ${file.name}` : "Upload from your computer or phone."}
      </p>
    </label>
  );
}
