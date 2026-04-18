import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import api from "../../../core/api/api";

const formatCurrency = (value) =>
  new Intl.NumberFormat("vi-VN", {
    style: "currency",
    currency: "VND",
    maximumFractionDigits: 0,
  }).format(value || 0);

const initialAreaForm = {
  areaCode: "",
  floor: "",
  areaName: "",
  size: "",
  monthlyRent: "",
};

export default function RentalAreas() {
  const role = localStorage.getItem("role") || "Guest";
  const isAdmin = role === "Admin";
  const [rentalAreas, setRentalAreas] = useState([]);
  const [loading, setLoading] = useState(isAdmin);
  const [loadError, setLoadError] = useState("");
  const [actionError, setActionError] = useState("");
  const [success, setSuccess] = useState("");
  const [areaForm, setAreaForm] = useState(initialAreaForm);
  const [tenantInputs, setTenantInputs] = useState({});
  const [saving, setSaving] = useState(false);

  const loadRentalAreas = async () => {
    try {
      setLoading(true);
      setLoadError("");
      const res = await api.get("/RentalArea");
      setRentalAreas(res.data);
    } catch (err) {
      setLoadError(err.response?.data || "Unable to load rental areas.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (!isAdmin) {
      return;
    }
    loadRentalAreas();
  }, [isAdmin]);

  const handleAreaFormChange = (field, value) => {
    setAreaForm((prev) => ({
      ...prev,
      [field]: value,
    }));
  };

  const handleAddArea = async () => {
    try {
      setSaving(true);
      setActionError("");
      setSuccess("");

      await api.post("/RentalArea", {
        ...areaForm,
        monthlyRent: Number(areaForm.monthlyRent),
      });

      setAreaForm(initialAreaForm);
      setSuccess("Rental area created successfully.");
      await loadRentalAreas();
    } catch (err) {
      setActionError(err.response?.data || "Unable to create rental area.");
    } finally {
      setSaving(false);
    }
  };

  const handleRegisterTenant = async (areaId) => {
    try {
      setSaving(true);
      setActionError("");
      setSuccess("");

      await api.put(`/RentalArea/${areaId}/register-tenant`, {
        tenantName: tenantInputs[areaId] || "",
      });

      setTenantInputs((prev) => ({
        ...prev,
        [areaId]: "",
      }));
      setSuccess("Tenant registered successfully.");
      await loadRentalAreas();
    } catch (err) {
      setActionError(err.response?.data || "Unable to register tenant.");
    } finally {
      setSaving(false);
    }
  };

  const handleCancelTenant = async (areaId) => {
    try {
      setSaving(true);
      setActionError("");
      setSuccess("");

      await api.put(`/RentalArea/${areaId}/cancel-tenant`);

      setSuccess("Tenant rental cancelled successfully.");
      await loadRentalAreas();
    } catch (err) {
      setActionError(err.response?.data || "Unable to cancel tenant rental.");
    } finally {
      setSaving(false);
    }
  };

  if (!isAdmin) {
    return (
      <div className="min-h-screen bg-[linear-gradient(180deg,#fff8ef_0%,#fffdf8_42%,#f8fbff_100%)] px-4 py-6 text-slate-900 sm:px-6 lg:px-8">
        <div className="mx-auto max-w-3xl rounded-[28px] border border-slate-200 bg-white/90 p-6 shadow-[0_20px_80px_rgba(15,23,42,0.08)]">
          <p className="text-xs font-semibold uppercase tracking-[0.28em] text-amber-700">
            Rental Areas
          </p>
          <h1 className="mt-3 text-3xl font-black">Admin access only</h1>
          <p className="mt-3 text-sm text-slate-600">
            Rental area management is only available for admin accounts.
          </p>
          <Link
            to="/dashboard"
            className="mt-6 inline-flex rounded-full bg-slate-950 px-5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5"
          >
            Back to Dashboard
          </Link>
        </div>
      </div>
    );
  }

  const rentedCount = rentalAreas.filter((area) => area.status === "Rented").length;
  const availableCount = rentalAreas.filter((area) => area.status === "Available").length;

  return (
    <div className="min-h-screen bg-[linear-gradient(180deg,#fff8ef_0%,#fffdf8_42%,#f8fbff_100%)] text-slate-900">
      <div className="mx-auto flex min-h-screen w-full max-w-7xl flex-col px-4 py-6 sm:px-6 lg:px-8">
        <header className="rounded-[28px] border border-white/70 bg-white/80 px-5 py-4 shadow-[0_20px_80px_rgba(15,23,42,0.08)] backdrop-blur-xl sm:px-6">
          <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
            <div>
              <div className="inline-flex items-center gap-2 rounded-full bg-amber-100 px-3 py-1 text-xs font-semibold uppercase tracking-[0.28em] text-amber-700">
                Rental Areas
              </div>
              <h1 className="mt-3 text-3xl font-black tracking-tight sm:text-4xl">
                Mall Rental Area Status
              </h1>
              <p className="mt-2 max-w-2xl text-sm text-slate-600 sm:text-base">
                Track which mall spaces are already rented and which ones are still available.
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

        <main className="mt-6 flex-1 space-y-6">
          <section className="grid gap-4 sm:grid-cols-2">
            <div className="rounded-[28px] border border-emerald-100 bg-emerald-50 px-5 py-5">
              <p className="text-xs font-semibold uppercase tracking-[0.24em] text-emerald-700">
                Rented
              </p>
              <p className="mt-2 text-4xl font-black text-emerald-700">{rentedCount}</p>
            </div>
            <div className="rounded-[28px] border border-amber-100 bg-amber-50 px-5 py-5">
              <p className="text-xs font-semibold uppercase tracking-[0.24em] text-amber-700">
                Available
              </p>
              <p className="mt-2 text-4xl font-black text-amber-700">{availableCount}</p>
            </div>
          </section>

          <section className="rounded-[30px] border border-slate-200 bg-white/90 p-5 shadow-[0_24px_90px_rgba(15,23,42,0.08)] backdrop-blur-xl sm:p-6">
            <div className="flex flex-col gap-2 sm:flex-row sm:items-end sm:justify-between">
              <div>
                <p className="text-xs font-semibold uppercase tracking-[0.24em] text-slate-400">
                  Admin Action
                </p>
                <h2 className="mt-2 text-2xl font-black text-slate-950">Add Rental Area</h2>
              </div>
            </div>

            <div className="mt-5 grid gap-3 md:grid-cols-2 xl:grid-cols-5">
              <input
                value={areaForm.areaCode}
                placeholder="Area code"
                className="rounded-[16px] border border-slate-200 bg-white px-4 py-3 text-sm outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100"
                onChange={(e) => handleAreaFormChange("areaCode", e.target.value)}
              />
              <input
                value={areaForm.floor}
                placeholder="Floor"
                className="rounded-[16px] border border-slate-200 bg-white px-4 py-3 text-sm outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100"
                onChange={(e) => handleAreaFormChange("floor", e.target.value)}
              />
              <input
                value={areaForm.areaName}
                placeholder="Area name"
                className="rounded-[16px] border border-slate-200 bg-white px-4 py-3 text-sm outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100"
                onChange={(e) => handleAreaFormChange("areaName", e.target.value)}
              />
              <input
                value={areaForm.size}
                placeholder="Size"
                className="rounded-[16px] border border-slate-200 bg-white px-4 py-3 text-sm outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100"
                onChange={(e) => handleAreaFormChange("size", e.target.value)}
              />
              <input
                type="number"
                min="0"
                value={areaForm.monthlyRent}
                placeholder="Monthly rent"
                className="rounded-[16px] border border-slate-200 bg-white px-4 py-3 text-sm outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100"
                onChange={(e) => handleAreaFormChange("monthlyRent", e.target.value)}
              />
            </div>

            <div className="mt-4 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
              <div className="space-y-2">
                {actionError && <p className="text-sm font-medium text-rose-600">{actionError}</p>}
                {success && <p className="text-sm font-medium text-emerald-600">{success}</p>}
              </div>
              <button
                type="button"
                disabled={saving}
                onClick={handleAddArea}
                className="rounded-full bg-slate-950 px-5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5 disabled:cursor-not-allowed disabled:opacity-50"
              >
                {saving ? "Saving..." : "Add Area"}
              </button>
            </div>
          </section>

          <section className="overflow-hidden rounded-[30px] border border-slate-200 bg-white/90 shadow-[0_24px_90px_rgba(15,23,42,0.08)] backdrop-blur-xl">
            <div className="border-b border-slate-200 px-5 py-4 sm:px-6">
              <p className="text-xs font-semibold uppercase tracking-[0.24em] text-slate-400">
                Admin View
              </p>
              <h2 className="mt-2 text-2xl font-black text-slate-950">Rental Area List</h2>
            </div>

            {loading ? (
              <div className="px-6 py-8 text-sm text-slate-500">Loading rental areas...</div>
            ) : loadError ? (
              <div className="px-6 py-8 text-sm font-medium text-rose-600">{loadError}</div>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full min-w-[1240px] table-fixed border-collapse text-left">
                  <colgroup>
                    <col className="w-[11%]" />
                    <col className="w-[8%]" />
                    <col className="w-[18%]" />
                    <col className="w-[9%]" />
                    <col className="w-[13%]" />
                    <col className="w-[10%]" />
                    <col className="w-[14%]" />
                    <col className="w-[17%]" />
                  </colgroup>
                  <thead className="bg-slate-100 text-xs font-semibold uppercase tracking-[0.16em] text-slate-500">
                    <tr>
                      <th className="px-4 py-3">Area Code</th>
                      <th className="px-4 py-3">Floor</th>
                      <th className="px-4 py-3">Area Name</th>
                      <th className="px-4 py-3">Size</th>
                      <th className="px-4 py-3">Monthly Rent</th>
                      <th className="px-4 py-3">Status</th>
                      <th className="px-4 py-3">Tenant</th>
                      <th className="px-4 py-3">Action</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-200 text-sm text-slate-700">
                    {rentalAreas.map((area) => {
                      const isRented = area.status === "Rented";

                      return (
                        <tr key={area.id} className="align-top transition hover:bg-amber-50/60">
                          <td className="px-4 py-4 font-semibold text-slate-950">{area.areaCode}</td>
                          <td className="px-4 py-4">{area.floor}</td>
                          <td className="px-4 py-4">{area.areaName}</td>
                          <td className="px-4 py-4">{area.size}</td>
                          <td className="px-4 py-4">{formatCurrency(area.monthlyRent)}</td>
                          <td className="px-4 py-4">
                            <span
                              className={`inline-flex rounded-full px-3 py-1 text-xs font-bold ${
                                isRented
                                  ? "bg-emerald-100 text-emerald-700"
                                  : "bg-amber-100 text-amber-700"
                              }`}
                            >
                              {area.status}
                            </span>
                          </td>
                          <td className="px-4 py-4">{area.tenantName || "-"}</td>
                          <td className="px-4 py-4">
                            {isRented ? (
                              <button
                                type="button"
                                disabled={saving}
                                onClick={() => handleCancelTenant(area.id)}
                                className="rounded-full bg-rose-500 px-4 py-2 text-xs font-semibold text-white transition hover:-translate-y-0.5 hover:bg-rose-600 disabled:cursor-not-allowed disabled:opacity-50"
                              >
                                Cancel Tenant
                              </button>
                            ) : (
                              <div className="flex gap-2">
                                <input
                                  value={tenantInputs[area.id] || ""}
                                  placeholder="Tenant name"
                                  className="min-w-0 flex-1 rounded-full border border-slate-200 bg-white px-3 py-2 text-xs outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100"
                                  onChange={(e) =>
                                    setTenantInputs((prev) => ({
                                      ...prev,
                                      [area.id]: e.target.value,
                                    }))
                                  }
                                />
                                <button
                                  type="button"
                                  disabled={saving}
                                  onClick={() => handleRegisterTenant(area.id)}
                                  className="rounded-full bg-slate-950 px-4 py-2 text-xs font-semibold text-white transition hover:-translate-y-0.5 disabled:cursor-not-allowed disabled:opacity-50"
                                >
                                  Register
                                </button>
                              </div>
                            )}
                          </td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              </div>
            )}
          </section>
        </main>
      </div>
    </div>
  );
}
