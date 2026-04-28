import React, { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import api, { BASE_URL } from "../../../core/api/api";

const RENTAL_AREA_PAGE_SIZE = 10;

const initialRentalForm = {
  cccd: "",
  managerName: "",
  shopName: "",
  location: "",
  startDate: "",
  electricityFee: "",
  waterFee: "",
  serviceFee: "",
  leaseTermDays: "",
};

const initialMonthlyBillForm = {
  billingMonth: "",
  usageMonth: "",
  electricityUsage: "",
  waterUsage: "",
};

const formatCurrency = (value) =>
  new Intl.NumberFormat("vi-VN", {
    style: "currency",
    currency: "VND",
    maximumFractionDigits: 0,
  }).format(value || 0);

const getTomorrowDateInput = () => {
  const tomorrow = new Date();
  tomorrow.setDate(tomorrow.getDate() + 1);
  tomorrow.setHours(0, 0, 0, 0);
  const timezoneOffset = tomorrow.getTimezoneOffset() * 60000;
  return new Date(tomorrow.getTime() - timezoneOffset).toISOString().slice(0, 10);
};

const getFloorLabel = (floor) => {
  const value = String(floor || "").trim();
  if (!value) return "-";
  return value.toLowerCase().includes("floor") ? value : `Floor ${value}`;
};

const resolveAssetUrl = (value) => {
  if (!value) return "";
  if (/^https?:\/\//i.test(value)) return value;
  const assetPath = value.startsWith("/") ? value : `/${value}`;
  return `${BASE_URL.replace(/\/api\/?$/, "")}${assetPath}`;
};

function DetailCard({ label, value }) {
  return (
    <div className="rounded-[18px] bg-white px-4 py-3">
      <p className="text-xs font-semibold uppercase tracking-[0.18em] text-slate-400">{label}</p>
      <p className="mt-1 break-words font-bold text-slate-950">{value || "-"}</p>
    </div>
  );
}

export default function RentalAreasAdmin() {
  const role = localStorage.getItem("role") || "Guest";
  const isAdmin = role === "Admin";
  const tomorrowDate = getTomorrowDateInput();

  const [rentalAreas, setRentalAreas] = useState([]);
  const [loading, setLoading] = useState(isAdmin);
  const [saving, setSaving] = useState(false);
  const [checkingCccd, setCheckingCccd] = useState(false);
  const [loadError, setLoadError] = useState("");
  const [actionError, setActionError] = useState("");
  const [success, setSuccess] = useState("");
  const [statusTab, setStatusTab] = useState("available");
  const [floorFilter, setFloorFilter] = useState("all");
  const [leaseLeftSort, setLeaseLeftSort] = useState(null);
  const [currentPage, setCurrentPage] = useState(1);
  const [selectedArea, setSelectedArea] = useState(null);
  const [selectedViewArea, setSelectedViewArea] = useState(null);
  const [selectedContract, setSelectedContract] = useState("");
  const [loadingViewDetail, setLoadingViewDetail] = useState(false);
  const [contractFile, setContractFile] = useState(null);
  const [rentalForm, setRentalForm] = useState(initialRentalForm);
  const [monthlyBillForm, setMonthlyBillForm] = useState(initialMonthlyBillForm);

  const floorOptions = useMemo(
    () =>
      Array.from(
        new Set(rentalAreas.map((area) => String(area.floor || "").trim()).filter(Boolean)),
      ).sort((a, b) => a.localeCompare(b, undefined, { numeric: true, sensitivity: "base" })),
    [rentalAreas],
  );

  const filteredRentalAreas = useMemo(() => {
    const byStatus = rentalAreas.filter((area) =>
      statusTab === "available" ? area.status === "Available" : area.status === "Rented",
    );
    const byFloor =
      floorFilter === "all"
        ? byStatus
        : byStatus.filter((area) => String(area.floor || "").trim() === floorFilter);

    return [...byFloor].sort((left, right) => {
      if (!leaseLeftSort) return 0;
      const leftValue = Number.isFinite(left.remainingLeaseDays) ? left.remainingLeaseDays : -1;
      const rightValue = Number.isFinite(right.remainingLeaseDays) ? right.remainingLeaseDays : -1;
      return leaseLeftSort === "asc" ? leftValue - rightValue : rightValue - leftValue;
    });
  }, [floorFilter, leaseLeftSort, rentalAreas, statusTab]);

  const rentedCount = rentalAreas.filter((area) => area.status === "Rented").length;
  const availableCount = rentalAreas.filter((area) => area.status === "Available").length;
  const totalPages = Math.max(1, Math.ceil(filteredRentalAreas.length / RENTAL_AREA_PAGE_SIZE));
  const safeCurrentPage = Math.min(currentPage, totalPages);
  const pageStart = (safeCurrentPage - 1) * RENTAL_AREA_PAGE_SIZE;
  const paginatedRentalAreas = filteredRentalAreas.slice(pageStart, pageStart + RENTAL_AREA_PAGE_SIZE);

  const resetMessages = () => {
    setActionError("");
    setSuccess("");
  };

  const updateRentalForm = (field, value) => {
    setRentalForm((current) => ({ ...current, [field]: value }));
  };

  const updateMonthlyBillForm = (field, value) => {
    setMonthlyBillForm((current) => ({ ...current, [field]: value }));
  };

  const loadRentalAreaDetail = async (areaId) => {
    try {
      setLoadingViewDetail(true);
      setActionError("");
      const detail = await api.get(`/RentalArea/${areaId}`);
      setSelectedViewArea(detail);
      return detail;
    } catch (error) {
      setActionError(error.message || "Unable to load rental area details.");
      return null;
    } finally {
      setLoadingViewDetail(false);
    }
  };

  const loadRentalAreas = async () => {
    try {
      setLoading(true);
      setLoadError("");
      const areas = await api.get("/RentalArea");
      setRentalAreas(areas || []);
      if ((areas || []).some((area) => area.status === "Available")) {
        setStatusTab((current) => current || "available");
      } else if ((areas || []).some((area) => area.status === "Rented")) {
        setStatusTab("rented");
      }
      if (selectedViewArea?.id) {
        await loadRentalAreaDetail(selectedViewArea.id);
      }
    } catch (error) {
      setLoadError(error.message || "Unable to load rental areas.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (isAdmin) {
      loadRentalAreas();
    }
  }, [isAdmin]);

  const openRegisterModal = (area) => {
    setSelectedArea(area);
    setRentalForm({
      ...initialRentalForm,
      location: area.areaCode || area.areaName || "",
    });
    setContractFile(null);
    resetMessages();
  };

  const closeRegisterModal = () => {
    setSelectedArea(null);
    setRentalForm(initialRentalForm);
    setContractFile(null);
    resetMessages();
  };

  const openViewModal = async (area) => {
    setMonthlyBillForm(initialMonthlyBillForm);
    resetMessages();
    await loadRentalAreaDetail(area.id);
  };

  const closeViewModal = () => {
    setSelectedViewArea(null);
    setSelectedContract("");
    setMonthlyBillForm(initialMonthlyBillForm);
    resetMessages();
  };

  const handleCheckCccd = async () => {
    const normalizedCccd = rentalForm.cccd.trim();
    if (!normalizedCccd) {
      setActionError("CCCD is required.");
      return;
    }

    try {
      setCheckingCccd(true);
      resetMessages();
      const result = await api.get(`/RentalArea/check-manager/${encodeURIComponent(normalizedCccd)}`);
      setRentalForm((current) => ({
        ...current,
        managerName: result.managerName || "",
        shopName: result.shopName || "",
      }));
      setSuccess("Manager information loaded.");
    } catch (error) {
      setRentalForm((current) => ({ ...current, managerName: "", shopName: "" }));
      setActionError(error.message || "Unable to find manager by CCCD.");
    } finally {
      setCheckingCccd(false);
    }
  };

  const handleSubmitRental = async () => {
    const requiredValues = [
      rentalForm.cccd,
      rentalForm.managerName,
      rentalForm.shopName,
      rentalForm.location,
      rentalForm.startDate,
      rentalForm.electricityFee,
      rentalForm.waterFee,
      rentalForm.serviceFee,
      rentalForm.leaseTermDays,
    ];

    if (requiredValues.some((value) => !String(value).trim()) || !contractFile) {
      setActionError("Please complete all rental fields and upload the contract image.");
      return;
    }

    const leaseTermDays = Number(rentalForm.leaseTermDays);
    if (!Number.isInteger(leaseTermDays) || leaseTermDays < 30 || leaseTermDays % 30 !== 0) {
      setActionError("Rental duration must be at least 30 days and divisible by 30.");
      return;
    }

    if (rentalForm.startDate < tomorrowDate) {
      setActionError("Start date must be tomorrow or later.");
      return;
    }

    try {
      setSaving(true);
      resetMessages();
      const formData = new FormData();
      formData.append("cccd", rentalForm.cccd.trim());
      formData.append("location", rentalForm.location.trim());
      formData.append("startDate", rentalForm.startDate);
      formData.append("electricityFee", Number(rentalForm.electricityFee));
      formData.append("waterFee", Number(rentalForm.waterFee));
      formData.append("serviceFee", Number(rentalForm.serviceFee));
      formData.append("leaseTermDays", leaseTermDays);
      formData.append("contractImage", contractFile);

      await api.put(`/RentalArea/${selectedArea.id}/register-tenant`, formData, {
        headers: { "Content-Type": "multipart/form-data" },
      });

      closeRegisterModal();
      setSuccess("Tenant registered successfully.");
      await loadRentalAreas();
    } catch (error) {
      setActionError(error.message || "Unable to register tenant.");
    } finally {
      setSaving(false);
    }
  };

  const handleCancelTenant = async (areaId) => {
    try {
      setSaving(true);
      resetMessages();
      await api.put(`/RentalArea/${areaId}/cancel-tenant`);
      setSuccess("Tenant rental cancelled successfully.");
      await loadRentalAreas();
    } catch (error) {
      setActionError(error.message || "Unable to cancel tenant rental.");
    } finally {
      setSaving(false);
    }
  };

  const handleSubmitMonthlyBill = async () => {
    const requiredValues = [
      monthlyBillForm.billingMonth,
      monthlyBillForm.usageMonth,
      monthlyBillForm.electricityUsage,
      monthlyBillForm.waterUsage,
    ];

    if (requiredValues.some((value) => !String(value).trim())) {
      setActionError("Please complete all monthly bill fields.");
      return;
    }

    try {
      setSaving(true);
      resetMessages();
      await api.put(`/RentalArea/${selectedViewArea.id}/monthly-bill`, {
        billingMonth: monthlyBillForm.billingMonth,
        usageMonth: monthlyBillForm.usageMonth,
        electricityUsage: monthlyBillForm.electricityUsage.trim(),
        waterUsage: monthlyBillForm.waterUsage.trim(),
      });
      setMonthlyBillForm(initialMonthlyBillForm);
      setSuccess("Monthly bill updated successfully.");
      await loadRentalAreas();
    } catch (error) {
      setActionError(error.message || "Unable to update monthly bill.");
    } finally {
      setSaving(false);
    }
  };

  if (!isAdmin) {
    return (
      <div className="min-h-screen bg-[linear-gradient(180deg,#fff8ef_0%,#fffdf8_42%,#f8fbff_100%)] px-4 pb-6 pt-24 text-slate-900 sm:px-6 sm:pt-28 lg:px-8">
        <div className="mx-auto max-w-3xl rounded-[28px] border border-slate-200 bg-white/90 p-6 shadow-[0_20px_80px_rgba(15,23,42,0.08)]">
          <p className="text-xs font-semibold uppercase tracking-[0.28em] text-amber-700">Rental Areas</p>
          <h1 className="mt-3 text-3xl font-black">Admin access only</h1>
          <p className="mt-3 text-sm text-slate-600">Rental area management is only available for admin accounts.</p>
          <Link to="/dashboard" className="mt-6 inline-flex rounded-full bg-slate-950 px-5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5">
            Back to Dashboard
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-[linear-gradient(180deg,#fff8ef_0%,#fffdf8_42%,#f8fbff_100%)] text-slate-900">
      <div className="mx-auto flex min-h-screen w-full max-w-7xl flex-col px-4 pb-6 pt-24 sm:px-6 sm:pt-28 lg:px-8">
        <header className="rounded-[28px] border border-white/70 bg-white/80 px-5 py-4 shadow-[0_20px_80px_rgba(15,23,42,0.08)] backdrop-blur-xl sm:px-6">
          <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
            <div>
              <div className="inline-flex items-center gap-2 rounded-full bg-amber-100 px-3 py-1 text-xs font-semibold uppercase tracking-[0.28em] text-amber-700">
                Rental Areas
              </div>
              <h1 className="mt-3 text-3xl font-black tracking-tight sm:text-4xl">Mall Rental Area Status</h1>
              <p className="mt-2 max-w-2xl text-sm text-slate-600 sm:text-base">
                Manage map-backed rental slots, tenant registration, and monthly usage from one admin view.
              </p>
            </div>
            <Link to="/dashboard" className="inline-flex items-center justify-center rounded-full bg-slate-950 px-5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5">
              Back to Dashboard
            </Link>
          </div>
        </header>

        <main className="mt-6 flex-1 space-y-6">
          <section className="grid gap-4 sm:grid-cols-2">
            <div className="rounded-[28px] border border-emerald-100 bg-emerald-50 px-5 py-5">
              <p className="text-xs font-semibold uppercase tracking-[0.24em] text-emerald-700">Rented</p>
              <p className="mt-2 text-4xl font-black text-emerald-700">{rentedCount}</p>
            </div>
            <div className="rounded-[28px] border border-amber-100 bg-amber-50 px-5 py-5">
              <p className="text-xs font-semibold uppercase tracking-[0.24em] text-amber-700">Available</p>
              <p className="mt-2 text-4xl font-black text-amber-700">{availableCount}</p>
            </div>
          </section>

          <section className="overflow-hidden rounded-[30px] border border-slate-200 bg-white/90 shadow-[0_24px_90px_rgba(15,23,42,0.08)] backdrop-blur-xl">
            <div className="border-b border-slate-200 px-5 py-4 sm:px-6">
              <div className="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
                <div>
                  <p className="text-xs font-semibold uppercase tracking-[0.24em] text-slate-400">Admin View</p>
                  <h2 className="mt-2 text-2xl font-black text-slate-950">Rental Area List</h2>
                  <div className="mt-3 flex flex-wrap gap-2">
                    <button type="button" onClick={() => { setStatusTab("rented"); setCurrentPage(1); }} className={`rounded-full px-4 py-2 text-xs font-bold transition ${statusTab === "rented" ? "bg-slate-950 text-white" : "bg-slate-100 text-slate-600 hover:bg-slate-200"}`}>
                      Rented Areas
                    </button>
                    <button type="button" onClick={() => { setStatusTab("available"); setCurrentPage(1); }} className={`rounded-full px-4 py-2 text-xs font-bold transition ${statusTab === "available" ? "bg-slate-950 text-white" : "bg-slate-100 text-slate-600 hover:bg-slate-200"}`}>
                      Available Areas
                    </button>
                  </div>
                </div>
                <div className="flex w-full flex-col gap-3 sm:flex-row lg:w-auto">
                  <select value={floorFilter} onChange={(event) => { setFloorFilter(event.target.value); setCurrentPage(1); }} className="rounded-[16px] border border-slate-200 bg-white px-4 py-3 text-sm font-semibold text-slate-700 outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100">
                    <option value="all">All Floors</option>
                    {floorOptions.map((floor) => (
                      <option key={floor} value={floor}>{getFloorLabel(floor)}</option>
                    ))}
                  </select>
                  <button type="button" onClick={() => { setLeaseLeftSort((current) => current === "asc" ? null : "asc"); setCurrentPage(1); }} className={`rounded-[16px] px-4 py-3 text-sm font-semibold transition ${leaseLeftSort === "asc" ? "bg-slate-950 text-white" : "bg-slate-100 text-slate-700 hover:bg-slate-200"}`}>
                    Lease Left ↑
                  </button>
                  <button type="button" onClick={() => { setLeaseLeftSort((current) => current === "desc" ? null : "desc"); setCurrentPage(1); }} className={`rounded-[16px] px-4 py-3 text-sm font-semibold transition ${leaseLeftSort === "desc" ? "bg-slate-950 text-white" : "bg-slate-100 text-slate-700 hover:bg-slate-200"}`}>
                    Lease Left ↓
                  </button>
                </div>
              </div>
            </div>

            {loading ? <div className="px-6 py-8 text-sm text-slate-500">Loading rental areas...</div> : null}
            {!loading && loadError ? <div className="px-6 py-8 text-sm font-medium text-rose-600">{loadError}</div> : null}

            {!loading && !loadError ? (
              <>
                <div className="overflow-x-auto">
                  <table className="w-full min-w-[960px] border-collapse text-left">
                    <thead className="bg-slate-100 text-xs font-semibold uppercase tracking-[0.16em] text-slate-500">
                      <tr>
                        <th className="px-4 py-3">Area Code</th>
                        <th className="px-4 py-3">Floor</th>
                        <th className="px-4 py-3">Area Name</th>
                        <th className="px-4 py-3">Status</th>
                        <th className="px-4 py-3">Tenant</th>
                        <th className="px-4 py-3">Lease Left</th>
                        <th className="px-4 py-3 text-right">Action</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-slate-100">
                      {paginatedRentalAreas.length === 0 ? (
                        <tr>
                          <td colSpan={7} className="px-4 py-10 text-center text-sm text-slate-500">No rental areas match the current filters.</td>
                        </tr>
                      ) : (
                        paginatedRentalAreas.map((area) => (
                          <tr key={area.id} className="align-top">
                            <td className="px-4 py-4 font-bold text-slate-950">{area.areaCode || "-"}</td>
                            <td className="px-4 py-4 text-sm text-slate-600">{getFloorLabel(area.floor)}</td>
                            <td className="px-4 py-4 text-sm text-slate-600">{area.areaName || "-"}</td>
                            <td className="px-4 py-4">
                              <span className={`inline-flex rounded-full px-3 py-1 text-xs font-bold ${area.status === "Rented" ? "bg-emerald-100 text-emerald-700" : "bg-amber-100 text-amber-700"}`}>
                                {area.status}
                              </span>
                            </td>
                            <td className="px-4 py-4 text-sm text-slate-600">{area.tenantName || "No tenant"}</td>
                            <td className="px-4 py-4 text-sm text-slate-600">
                              {area.remainingLeaseLabel || (Number.isFinite(area.remainingLeaseDays) ? `${area.remainingLeaseDays} days` : "-")}
                            </td>
                            <td className="px-4 py-4">
                              <div className="flex justify-end gap-2">
                                <button type="button" onClick={() => openViewModal(area)} className="rounded-full bg-slate-950 px-4 py-2 text-xs font-semibold text-white transition hover:-translate-y-0.5">
                                  View Details
                                </button>
                                {area.status === "Available" ? (
                                  <button type="button" onClick={() => openRegisterModal(area)} className="rounded-full bg-amber-300 px-4 py-2 text-xs font-semibold text-slate-950 transition hover:-translate-y-0.5">
                                    Register Tenant
                                  </button>
                                ) : null}
                              </div>
                            </td>
                          </tr>
                        ))
                      )}
                    </tbody>
                  </table>
                </div>
                <div className="flex items-center justify-between border-t border-slate-200 px-5 py-4 text-sm text-slate-500">
                  <p>
                    Showing {filteredRentalAreas.length === 0 ? 0 : pageStart + 1}-{Math.min(pageStart + RENTAL_AREA_PAGE_SIZE, filteredRentalAreas.length)} of {filteredRentalAreas.length}
                  </p>
                  <div className="flex gap-2">
                    <button type="button" disabled={safeCurrentPage <= 1} onClick={() => setCurrentPage((page) => Math.max(1, page - 1))} className="rounded-full bg-slate-100 px-4 py-2 font-semibold text-slate-700 transition hover:bg-slate-200 disabled:cursor-not-allowed disabled:opacity-40">
                      Previous
                    </button>
                    <button type="button" disabled={safeCurrentPage >= totalPages} onClick={() => setCurrentPage((page) => Math.min(totalPages, page + 1))} className="rounded-full bg-slate-100 px-4 py-2 font-semibold text-slate-700 transition hover:bg-slate-200 disabled:cursor-not-allowed disabled:opacity-40">
                      Next
                    </button>
                  </div>
                </div>
              </>
            ) : null}
          </section>
        </main>

        {selectedArea ? (
          <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-950/70 px-4 py-6 backdrop-blur-sm">
            <div className="max-h-[92vh] w-full max-w-4xl overflow-auto rounded-[30px] bg-white shadow-[0_30px_120px_rgba(15,23,42,0.3)]">
              <div className="sticky top-0 z-10 flex items-center justify-between border-b border-slate-200 bg-white px-5 py-4">
                <div>
                  <h3 className="text-xl font-black text-slate-950">Register Rental</h3>
                  <p className="text-sm text-slate-500">Area: {selectedArea.areaCode}</p>
                </div>
                <button type="button" onClick={closeRegisterModal} className="rounded-full bg-slate-950 px-4 py-2 text-sm font-semibold text-white">Close</button>
              </div>

              <div className="grid gap-4 p-5 sm:grid-cols-2">
                <div className="sm:col-span-2">
                  <label className="mb-2 block text-sm font-semibold text-slate-700">CCCD</label>
                  <div className="flex gap-2">
                    <input value={rentalForm.cccd} onChange={(event) => updateRentalForm("cccd", event.target.value)} placeholder="Enter CCCD" className="min-w-0 flex-1 rounded-[16px] border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100" />
                    <button type="button" onClick={handleCheckCccd} disabled={checkingCccd} className="rounded-[16px] bg-amber-300 px-5 py-3 text-sm font-semibold text-slate-950 transition hover:bg-amber-200 disabled:cursor-not-allowed disabled:opacity-50">
                      {checkingCccd ? "Checking..." : "Check"}
                    </button>
                  </div>
                </div>
                <div>
                  <label className="mb-2 block text-sm font-semibold text-slate-700">Manager Name</label>
                  <input value={rentalForm.managerName} readOnly className="w-full rounded-[16px] border border-slate-200 bg-slate-100 px-4 py-3 text-sm text-slate-700 outline-none" />
                </div>
                <div>
                  <label className="mb-2 block text-sm font-semibold text-slate-700">Shop Name</label>
                  <input value={rentalForm.shopName} readOnly className="w-full rounded-[16px] border border-slate-200 bg-slate-100 px-4 py-3 text-sm text-slate-700 outline-none" />
                </div>
                <div>
                  <label className="mb-2 block text-sm font-semibold text-slate-700">Location</label>
                  <input value={rentalForm.location} onChange={(event) => updateRentalForm("location", event.target.value)} className="w-full rounded-[16px] border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100" />
                </div>
                <div>
                  <label className="mb-2 block text-sm font-semibold text-slate-700">Start Date</label>
                  <input type="date" min={tomorrowDate} value={rentalForm.startDate} onChange={(event) => updateRentalForm("startDate", event.target.value)} className="w-full rounded-[16px] border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100" />
                </div>
                <div>
                  <label className="mb-2 block text-sm font-semibold text-slate-700">Electricity Fee</label>
                  <input type="number" min="0" value={rentalForm.electricityFee} onChange={(event) => updateRentalForm("electricityFee", event.target.value)} className="w-full rounded-[16px] border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100" />
                </div>
                <div>
                  <label className="mb-2 block text-sm font-semibold text-slate-700">Water Fee</label>
                  <input type="number" min="0" value={rentalForm.waterFee} onChange={(event) => updateRentalForm("waterFee", event.target.value)} className="w-full rounded-[16px] border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100" />
                </div>
                <div>
                  <label className="mb-2 block text-sm font-semibold text-slate-700">Fee</label>
                  <input type="number" min="0" value={rentalForm.serviceFee} onChange={(event) => updateRentalForm("serviceFee", event.target.value)} className="w-full rounded-[16px] border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100" />
                </div>
                <div>
                  <label className="mb-2 block text-sm font-semibold text-slate-700">Rental Duration (days)</label>
                  <input type="number" min="30" step="30" value={rentalForm.leaseTermDays} onChange={(event) => updateRentalForm("leaseTermDays", event.target.value)} className="w-full rounded-[16px] border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100" />
                </div>
                <div className="sm:col-span-2">
                  <label className="mb-2 block text-sm font-semibold text-slate-700">Contract Image</label>
                  <input type="file" accept="image/*" onChange={(event) => setContractFile(event.target.files?.[0] || null)} className="w-full rounded-[16px] border border-slate-200 px-4 py-3 text-sm file:mr-4 file:rounded-full file:border-0 file:bg-slate-950 file:px-4 file:py-2 file:text-sm file:font-semibold file:text-white" />
                </div>
              </div>

              <div className="sticky bottom-0 flex flex-col gap-3 border-t border-slate-200 bg-white px-5 py-4 sm:flex-row sm:items-center sm:justify-between">
                <div className="space-y-1">
                  {actionError ? <p className="text-sm font-medium text-rose-600">{actionError}</p> : null}
                  {success ? <p className="text-sm font-medium text-emerald-600">{success}</p> : null}
                </div>
                <button type="button" onClick={handleSubmitRental} disabled={saving} className="rounded-full bg-slate-950 px-5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5 disabled:cursor-not-allowed disabled:opacity-50">
                  {saving ? "Submitting..." : "Submit Rental"}
                </button>
              </div>
            </div>
          </div>
        ) : null}

        {selectedViewArea ? (
          <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-950/70 px-4 py-6 backdrop-blur-sm">
            <div className="max-h-[92vh] w-full max-w-5xl overflow-auto rounded-[30px] bg-white shadow-[0_30px_120px_rgba(15,23,42,0.3)]">
              <div className="sticky top-0 z-10 flex items-center justify-between border-b border-slate-200 bg-white px-5 py-4">
                <div>
                  <h3 className="text-xl font-black text-slate-950">Rental Area Details</h3>
                  <p className="text-sm text-slate-500">Area: {selectedViewArea.areaCode} | Status: {selectedViewArea.status}</p>
                </div>
                <button type="button" onClick={closeViewModal} className="rounded-full bg-slate-950 px-4 py-2 text-sm font-semibold text-white">Close</button>
              </div>

              <div className="space-y-5 p-5">
                <section className="rounded-[24px] border border-slate-200 bg-slate-50 p-4">
                  <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
                    <div>
                      <p className="text-xs font-semibold uppercase tracking-[0.22em] text-slate-400">Area Information</p>
                      <h4 className="mt-2 text-2xl font-black text-slate-950">{selectedViewArea.areaName}</h4>
                    </div>
                    <span className={`inline-flex w-fit rounded-full px-3 py-1 text-xs font-bold ${selectedViewArea.status === "Rented" ? "bg-emerald-100 text-emerald-700" : "bg-amber-100 text-amber-700"}`}>
                      {selectedViewArea.status}
                    </span>
                  </div>

                  <div className="mt-5 grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
                    <DetailCard label="Area Code" value={selectedViewArea.areaCode} />
                    <DetailCard label="Floor" value={selectedViewArea.floor} />
                    <DetailCard label="Size" value={selectedViewArea.size} />
                    <DetailCard label="Monthly Rent" value={formatCurrency(selectedViewArea.monthlyRent)} />
                  </div>

                  {loadingViewDetail ? <div className="mt-4 rounded-[18px] bg-white px-4 py-6 text-sm font-medium text-slate-500">Loading rental details...</div> : null}

                  {!loadingViewDetail ? (
                    <>
                      <div className="mt-5 grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
                        <DetailCard label="Tenant" value={selectedViewArea.tenantName || "No tenant registered"} />
                        <DetailCard label="CCCD" value={selectedViewArea.cccd} />
                        <DetailCard label="Manager Name" value={selectedViewArea.managerName} />
                        <DetailCard label="Shop Name" value={selectedViewArea.shopName} />
                        <DetailCard label="Location" value={selectedViewArea.rentalLocation} />
                        <DetailCard label="Start Date" value={selectedViewArea.leaseStartDate} />
                        <DetailCard label="Electricity Fee" value={formatCurrency(selectedViewArea.electricityFee)} />
                        <DetailCard label="Water Fee" value={formatCurrency(selectedViewArea.waterFee)} />
                        <DetailCard label="Fee" value={formatCurrency(selectedViewArea.serviceFee)} />
                        <DetailCard label="Rental Duration" value={selectedViewArea.leaseTermDays ? `${selectedViewArea.leaseTermDays} days` : "-"} />
                      </div>

                      <div className="mt-3 rounded-[18px] bg-white px-4 py-3">
                        <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
                          <div>
                            <p className="text-xs font-semibold uppercase tracking-[0.18em] text-slate-400">Contract Image</p>
                            <p className="mt-1 font-bold text-slate-950">{resolveAssetUrl(selectedViewArea.contractImage || selectedViewArea.contractImages) ? "Contract available" : "No contract uploaded"}</p>
                          </div>
                          <button type="button" disabled={!resolveAssetUrl(selectedViewArea.contractImage || selectedViewArea.contractImages)} onClick={() => setSelectedContract(resolveAssetUrl(selectedViewArea.contractImage || selectedViewArea.contractImages))} className="rounded-full bg-slate-950 px-4 py-2 text-sm font-semibold text-white transition hover:-translate-y-0.5 disabled:cursor-not-allowed disabled:bg-slate-200 disabled:text-slate-400 disabled:hover:translate-y-0">
                            View Contract
                          </button>
                        </div>
                      </div>
                    </>
                  ) : null}
                </section>

                {selectedViewArea.status === "Rented" ? (
                  <section className="rounded-[24px] border border-amber-100 bg-amber-50 p-4">
                    <div className="flex flex-col gap-3 lg:flex-row lg:items-start lg:justify-between">
                      <div>
                        <p className="text-xs font-semibold uppercase tracking-[0.22em] text-amber-700">Monthly Usage</p>
                        <h4 className="mt-2 text-xl font-black text-slate-950">Update electricity and water usage</h4>
                      </div>
                      <button type="button" onClick={() => handleCancelTenant(selectedViewArea.id)} disabled={saving} className="rounded-full bg-rose-500 px-5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5 hover:bg-rose-600 disabled:cursor-not-allowed disabled:opacity-50">
                        Cancel Tenant
                      </button>
                    </div>

                    <div className="mt-5 grid gap-4 sm:grid-cols-2">
                      <div>
                        <label className="mb-2 block text-sm font-semibold text-slate-700">Billing Month</label>
                        <input type="month" value={monthlyBillForm.billingMonth} onChange={(event) => updateMonthlyBillForm("billingMonth", event.target.value)} className="w-full rounded-[16px] border border-slate-200 bg-white px-4 py-3 text-sm outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100" />
                      </div>
                      <div>
                        <label className="mb-2 block text-sm font-semibold text-slate-700">Usage Month</label>
                        <input type="month" value={monthlyBillForm.usageMonth} onChange={(event) => updateMonthlyBillForm("usageMonth", event.target.value)} className="w-full rounded-[16px] border border-slate-200 bg-white px-4 py-3 text-sm outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100" />
                      </div>
                      <div>
                        <label className="mb-2 block text-sm font-semibold text-slate-700">Electricity Usage</label>
                        <input value={monthlyBillForm.electricityUsage} onChange={(event) => updateMonthlyBillForm("electricityUsage", event.target.value)} className="w-full rounded-[16px] border border-slate-200 bg-white px-4 py-3 text-sm outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100" />
                      </div>
                      <div>
                        <label className="mb-2 block text-sm font-semibold text-slate-700">Water Usage</label>
                        <input value={monthlyBillForm.waterUsage} onChange={(event) => updateMonthlyBillForm("waterUsage", event.target.value)} className="w-full rounded-[16px] border border-slate-200 bg-white px-4 py-3 text-sm outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100" />
                      </div>
                    </div>
                  </section>
                ) : (
                  <section className="rounded-[24px] border border-slate-200 bg-slate-50 p-4">
                    <p className="text-xs font-semibold uppercase tracking-[0.22em] text-slate-400">Tenant Registration</p>
                    <h4 className="mt-2 text-xl font-black text-slate-950">This area is available</h4>
                    <p className="mt-1 text-sm text-slate-600">Register a tenant from this detail view when the rental information is ready.</p>
                    <button type="button" onClick={() => { openRegisterModal(selectedViewArea); setSelectedViewArea(null); }} disabled={saving} className="mt-4 rounded-full bg-slate-950 px-5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5 disabled:cursor-not-allowed disabled:opacity-50">
                      Register Tenant
                    </button>
                  </section>
                )}
              </div>

              <div className="sticky bottom-0 flex flex-col gap-3 border-t border-slate-200 bg-white px-5 py-4 sm:flex-row sm:items-center sm:justify-between">
                <div className="space-y-1">
                  {actionError ? <p className="text-sm font-medium text-rose-600">{actionError}</p> : null}
                  {success ? <p className="text-sm font-medium text-emerald-600">{success}</p> : null}
                </div>
                {selectedViewArea.status === "Rented" ? (
                  <button type="button" onClick={handleSubmitMonthlyBill} disabled={saving} className="rounded-full bg-slate-950 px-5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5 disabled:cursor-not-allowed disabled:opacity-50">
                    {saving ? "Saving..." : "Save Monthly Usage"}
                  </button>
                ) : null}
              </div>
            </div>
          </div>
        ) : null}

        {selectedContract ? (
          <div className="fixed inset-0 z-[60] flex items-center justify-center bg-slate-950/80 px-4 py-6 backdrop-blur-sm">
            <div className="max-h-[90vh] w-full max-w-5xl overflow-hidden rounded-[30px] bg-white shadow-[0_30px_120px_rgba(15,23,42,0.35)]">
              <div className="flex items-center justify-between border-b border-slate-200 px-5 py-4">
                <h3 className="text-lg font-black text-slate-950">Contract Image</h3>
                <button type="button" onClick={() => setSelectedContract("")} className="rounded-full bg-slate-950 px-4 py-2 text-sm font-semibold text-white">Close</button>
              </div>
              <div className="max-h-[78vh] overflow-auto bg-slate-100 p-4">
                <img src={selectedContract} alt="Contract" className="mx-auto max-h-[72vh] w-auto rounded-2xl object-contain shadow-lg" />
              </div>
            </div>
          </div>
        ) : null}
      </div>
    </div>
  );
}
