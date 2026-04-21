import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import api from "../../../core/api/api";

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

const initialAreaForm = {
  areaCode: "",
  floor: "",
  areaName: "",
  size: "",
  monthlyRent: "",
};

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

const RENTAL_AREA_PAGE_SIZE = 10;

const getFloorLabel = (floor) => {
  const value = String(floor || "").trim();
  return value.toLowerCase().includes("floor") ? value : `Floor ${value}`;
};

export default function RentalAreasAdmin() {
  const role = localStorage.getItem("role") || "Guest";
  const isAdmin = role === "Admin";
  const tomorrowDate = getTomorrowDateInput();
  const [rentalAreas, setRentalAreas] = useState([]);
  const [loading, setLoading] = useState(isAdmin);
  const [loadError, setLoadError] = useState("");
  const [actionError, setActionError] = useState("");
  const [success, setSuccess] = useState("");
  const [areaForm, setAreaForm] = useState(initialAreaForm);
  const [saving, setSaving] = useState(false);
  const [selectedViewArea, setSelectedViewArea] = useState(null);
  const [selectedArea, setSelectedArea] = useState(null);
  const [rentalForm, setRentalForm] = useState(initialRentalForm);
  const [contractFile, setContractFile] = useState(null);
  const [checkingCccd, setCheckingCccd] = useState(false);
  const [monthlyBillForm, setMonthlyBillForm] = useState(initialMonthlyBillForm);
  const [floorFilter, setFloorFilter] = useState("all");
  const [currentPage, setCurrentPage] = useState(1);

  const loadRentalAreas = async () => {
    try {
      setLoading(true);
      setLoadError("");
      const res = await api.get("/RentalArea");
      const areas = res.data || [];
      setRentalAreas(areas);
      setSelectedViewArea((current) => (
        current ? areas.find((area) => area.id === current.id) || null : null
      ));
      return areas;
    } catch (err) {
      setLoadError(err.response?.data || "Unable to load rental areas.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (isAdmin) {
      loadRentalAreas();
    }
  }, [isAdmin]);

  const handleAreaFormChange = (field, value) => {
    setAreaForm((prev) => ({ ...prev, [field]: value }));
  };

  const handleRentalFormChange = (field, value) => {
    setRentalForm((prev) => ({ ...prev, [field]: value }));
  };

  const handleMonthlyBillFormChange = (field, value) => {
    setMonthlyBillForm((prev) => ({ ...prev, [field]: value }));
  };

  const handleFloorFilterChange = (value) => {
    setFloorFilter(value);
    setCurrentPage(1);
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

  const openRegisterModal = (area) => {
    setSelectedArea(area);
    setRentalForm({
      ...initialRentalForm,
      location: area.areaCode || "",
    });
    setContractFile(null);
    setActionError("");
    setSuccess("");
  };

  const closeRegisterModal = () => {
    setSelectedArea(null);
    setRentalForm(initialRentalForm);
    setContractFile(null);
    setActionError("");
  };

  const openViewModal = (area) => {
    setSelectedViewArea(area);
    setMonthlyBillForm(initialMonthlyBillForm);
    setActionError("");
    setSuccess("");
  };

  const closeViewModal = () => {
    setSelectedViewArea(null);
    setMonthlyBillForm(initialMonthlyBillForm);
    setActionError("");
  };

  const handleCheckCccd = async () => {
    try {
      setCheckingCccd(true);
      setActionError("");
      setSuccess("");

      if (!rentalForm.cccd.trim()) {
        setActionError("CCCD is required.");
        return;
      }

      const normalizedCccd = rentalForm.cccd.trim();
      const res = await api.get(`/RentalArea/check-manager/${encodeURIComponent(normalizedCccd)}`);
      setRentalForm((prev) => ({
        ...prev,
        managerName: res.data.managerName || "",
        shopName: res.data.shopName || "",
      }));
      setSuccess("Manager information loaded.");
    } catch (err) {
      setRentalForm((prev) => ({ ...prev, managerName: "", shopName: "" }));
      setActionError(err.response?.data || "Unable to find manager by CCCD.");
    } finally {
      setCheckingCccd(false);
    }
  };

  const handleSubmitRental = async () => {
    try {
      setSaving(true);
      setActionError("");
      setSuccess("");

      const requiredFields = [
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

      if (requiredFields.some((value) => !String(value).trim()) || !contractFile) {
        setActionError("Please complete all rental fields and upload the contract image.");
        return;
      }

      if (rentalForm.startDate < tomorrowDate) {
        setActionError("Start date must be tomorrow or later.");
        return;
      }

      const rentalDurationDays = Number(rentalForm.leaseTermDays);
      if (!Number.isInteger(rentalDurationDays) || rentalDurationDays < 30 || rentalDurationDays % 30 !== 0) {
        setActionError("Rental duration must be at least 30 days and divisible by 30.");
        return;
      }

      const formData = new FormData();
      formData.append("cccd", rentalForm.cccd.trim());
      formData.append("location", rentalForm.location.trim());
      formData.append("startDate", rentalForm.startDate);
      formData.append("electricityFee", Number(rentalForm.electricityFee));
      formData.append("waterFee", Number(rentalForm.waterFee));
      formData.append("serviceFee", Number(rentalForm.serviceFee));
      formData.append("leaseTermDays", rentalDurationDays);
      formData.append("contractImage", contractFile);

      await api.put(`/RentalArea/${selectedArea.id}/register-tenant`, formData, {
        headers: { "Content-Type": "multipart/form-data" },
      });

      closeRegisterModal();
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

  const handleSubmitMonthlyBill = async () => {
    try {
      setSaving(true);
      setActionError("");
      setSuccess("");

      const requiredFields = [
        monthlyBillForm.billingMonth,
        monthlyBillForm.usageMonth,
        monthlyBillForm.electricityUsage,
        monthlyBillForm.waterUsage,
      ];

      if (requiredFields.some((value) => !String(value).trim())) {
        setActionError("Please complete all monthly bill fields.");
        return;
      }

      await api.put(`/RentalArea/${selectedViewArea.id}/monthly-bill`, {
        billingMonth: monthlyBillForm.billingMonth,
        usageMonth: monthlyBillForm.usageMonth,
        electricityUsage: monthlyBillForm.electricityUsage.trim(),
        waterUsage: monthlyBillForm.waterUsage.trim(),
      });

      setMonthlyBillForm(initialMonthlyBillForm);
      setSuccess("Monthly bill updated successfully.");
      await loadRentalAreas();
    } catch (err) {
      setActionError(err.response?.data || "Unable to update monthly bill.");
    } finally {
      setSaving(false);
    }
  };

  if (!isAdmin) {
    return (
      <div className="min-h-screen bg-[linear-gradient(180deg,#fff8ef_0%,#fffdf8_42%,#f8fbff_100%)] px-4 pb-6 pt-24 text-slate-900 sm:px-6 sm:pt-28 lg:px-8">
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
  const floorOptions = Array.from(
    new Set(rentalAreas.map((area) => String(area.floor || "").trim()).filter(Boolean)),
  ).sort((a, b) => a.localeCompare(b, undefined, { numeric: true, sensitivity: "base" }));
  const filteredRentalAreas = floorFilter === "all"
    ? rentalAreas
    : rentalAreas.filter((area) => String(area.floor || "").trim() === floorFilter);
  const totalPages = Math.max(1, Math.ceil(filteredRentalAreas.length / RENTAL_AREA_PAGE_SIZE));
  const safeCurrentPage = Math.min(currentPage, totalPages);
  const startIndex = (safeCurrentPage - 1) * RENTAL_AREA_PAGE_SIZE;
  const paginatedRentalAreas = filteredRentalAreas.slice(startIndex, startIndex + RENTAL_AREA_PAGE_SIZE);
  const showingStart = filteredRentalAreas.length === 0 ? 0 : startIndex + 1;
  const showingEnd = Math.min(startIndex + RENTAL_AREA_PAGE_SIZE, filteredRentalAreas.length);

  return (
    <div className="min-h-screen bg-[linear-gradient(180deg,#fff8ef_0%,#fffdf8_42%,#f8fbff_100%)] text-slate-900">
      <div className="mx-auto flex min-h-screen w-full max-w-7xl flex-col px-4 pb-6 pt-24 sm:px-6 sm:pt-28 lg:px-8">
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
                Track rental spaces, register tenants, and upload contract images.
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
              <p className="text-xs font-semibold uppercase tracking-[0.24em] text-emerald-700">Rented</p>
              <p className="mt-2 text-4xl font-black text-emerald-700">{rentedCount}</p>
            </div>
            <div className="rounded-[28px] border border-amber-100 bg-amber-50 px-5 py-5">
              <p className="text-xs font-semibold uppercase tracking-[0.24em] text-amber-700">Available</p>
              <p className="mt-2 text-4xl font-black text-amber-700">{availableCount}</p>
            </div>
          </section>

          <section className="rounded-[30px] border border-slate-200 bg-white/90 p-5 shadow-[0_24px_90px_rgba(15,23,42,0.08)] backdrop-blur-xl sm:p-6">
            <p className="text-xs font-semibold uppercase tracking-[0.24em] text-slate-400">Admin Action</p>
            <h2 className="mt-2 text-2xl font-black text-slate-950">Add Rental Area</h2>

            <div className="mt-5 grid gap-3 md:grid-cols-2 xl:grid-cols-5">
              <input value={areaForm.areaCode} placeholder="Area code" className="rounded-[16px] border border-slate-200 bg-white px-4 py-3 text-sm outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100" onChange={(e) => handleAreaFormChange("areaCode", e.target.value)} />
              <input value={areaForm.floor} placeholder="Floor" className="rounded-[16px] border border-slate-200 bg-white px-4 py-3 text-sm outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100" onChange={(e) => handleAreaFormChange("floor", e.target.value)} />
              <input value={areaForm.areaName} placeholder="Area name" className="rounded-[16px] border border-slate-200 bg-white px-4 py-3 text-sm outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100" onChange={(e) => handleAreaFormChange("areaName", e.target.value)} />
              <input value={areaForm.size} placeholder="Size" className="rounded-[16px] border border-slate-200 bg-white px-4 py-3 text-sm outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100" onChange={(e) => handleAreaFormChange("size", e.target.value)} />
              <input type="number" min="0" value={areaForm.monthlyRent} placeholder="Monthly rent" className="rounded-[16px] border border-slate-200 bg-white px-4 py-3 text-sm outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100" onChange={(e) => handleAreaFormChange("monthlyRent", e.target.value)} />
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
              <div className="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
                <div>
                  <p className="text-xs font-semibold uppercase tracking-[0.24em] text-slate-400">Admin View</p>
                  <h2 className="mt-2 text-2xl font-black text-slate-950">Rental Area List</h2>
                </div>

                <div className="w-full lg:w-56">
                  <label className="mb-2 block text-xs font-semibold uppercase tracking-[0.18em] text-slate-400">
                    Filter by floor
                  </label>
                  <select
                    value={floorFilter}
                    onChange={(e) => handleFloorFilterChange(e.target.value)}
                    className="w-full rounded-[16px] border border-slate-200 bg-white px-4 py-3 text-sm font-semibold text-slate-700 outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100"
                  >
                    <option value="all">All Floors</option>
                    {floorOptions.map((floor) => (
                      <option key={floor} value={floor}>
                        {getFloorLabel(floor)}
                      </option>
                    ))}
                  </select>
                </div>
              </div>
            </div>

            {loading ? (
              <div className="px-6 py-8 text-sm text-slate-500">Loading rental areas...</div>
            ) : loadError ? (
              <div className="px-6 py-8 text-sm font-medium text-rose-600">{loadError}</div>
            ) : (
              <>
                <div className="overflow-x-auto">
                  <table className="w-full min-w-[1080px] table-fixed border-collapse text-left">
                    <colgroup>
                      <col className="w-[12%]" />
                      <col className="w-[8%]" />
                      <col className="w-[20%]" />
                      <col className="w-[10%]" />
                      <col className="w-[14%]" />
                      <col className="w-[12%]" />
                      <col className="w-[16%]" />
                      <col className="w-[8%]" />
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
                      {paginatedRentalAreas.length === 0 ? (
                        <tr>
                          <td colSpan={8} className="px-4 py-8 text-center text-sm font-medium text-slate-500">
                            No rental areas found for this floor.
                          </td>
                        </tr>
                      ) : (
                        paginatedRentalAreas.map((area) => {
                          const isRented = area.status === "Rented";

                          return (
                            <tr key={area.id} className="align-top transition hover:bg-amber-50/60">
                              <td className="px-4 py-4 font-semibold text-slate-950">{area.areaCode}</td>
                              <td className="px-4 py-4">{area.floor}</td>
                              <td className="px-4 py-4">{area.areaName}</td>
                              <td className="px-4 py-4">{area.size}</td>
                              <td className="px-4 py-4">{formatCurrency(area.monthlyRent)}</td>
                              <td className="px-4 py-4">
                                <span className={`inline-flex rounded-full px-3 py-1 text-xs font-bold ${isRented ? "bg-emerald-100 text-emerald-700" : "bg-amber-100 text-amber-700"}`}>
                                  {area.status}
                                </span>
                              </td>
                              <td className="px-4 py-4">{area.tenantName || "-"}</td>
                              <td className="px-4 py-4">
                                <button type="button" onClick={() => openViewModal(area)} className="rounded-full bg-slate-950 px-4 py-2 text-xs font-semibold text-white transition hover:-translate-y-0.5">
                                  View
                                </button>
                              </td>
                            </tr>
                          );
                        })
                      )}
                    </tbody>
                  </table>
                </div>

                <div className="flex flex-col gap-3 border-t border-slate-200 px-5 py-4 text-sm text-slate-600 sm:flex-row sm:items-center sm:justify-between sm:px-6">
                  <p>
                    Showing <span className="font-bold text-slate-950">{showingStart}</span>
                    {" - "}
                    <span className="font-bold text-slate-950">{showingEnd}</span>
                    {" of "}
                    <span className="font-bold text-slate-950">{filteredRentalAreas.length}</span> rental areas
                  </p>

                  <div className="flex items-center gap-2">
                    <button
                      type="button"
                      disabled={safeCurrentPage === 1}
                      onClick={() => setCurrentPage((page) => Math.max(1, page - 1))}
                      className="rounded-full border border-slate-200 bg-white px-4 py-2 text-xs font-bold text-slate-700 transition hover:-translate-y-0.5 hover:border-amber-300 disabled:cursor-not-allowed disabled:opacity-40 disabled:hover:translate-y-0"
                    >
                      Previous
                    </button>
                    <span className="rounded-full bg-slate-100 px-4 py-2 text-xs font-bold text-slate-700">
                      Page {safeCurrentPage} / {totalPages}
                    </span>
                    <button
                      type="button"
                      disabled={safeCurrentPage === totalPages}
                      onClick={() => setCurrentPage((page) => Math.min(totalPages, page + 1))}
                      className="rounded-full border border-slate-200 bg-white px-4 py-2 text-xs font-bold text-slate-700 transition hover:-translate-y-0.5 hover:border-amber-300 disabled:cursor-not-allowed disabled:opacity-40 disabled:hover:translate-y-0"
                    >
                      Next
                    </button>
                  </div>
                </div>
              </>
            )}
          </section>
        </main>
      </div>

      {selectedArea && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-950/70 px-4 py-6 backdrop-blur-sm">
          <div className="max-h-[92vh] w-full max-w-4xl overflow-auto rounded-[30px] bg-white shadow-[0_30px_120px_rgba(15,23,42,0.3)]">
            <div className="sticky top-0 z-10 flex items-center justify-between border-b border-slate-200 bg-white px-5 py-4">
              <div>
                <h3 className="text-xl font-black text-slate-950">Register Rental</h3>
                <p className="text-sm text-slate-500">Area: {selectedArea.areaCode}</p>
              </div>
              <button type="button" onClick={closeRegisterModal} className="rounded-full bg-slate-950 px-4 py-2 text-sm font-semibold text-white">
                Close
              </button>
            </div>

            <div className="grid gap-4 p-5 sm:grid-cols-2">
              <div className="sm:col-span-2">
                <label className="mb-2 block text-sm font-semibold text-slate-700">CCCD</label>
                <div className="flex gap-2">
                  <input value={rentalForm.cccd} placeholder="Enter CCCD" className="min-w-0 flex-1 rounded-[16px] border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100" onChange={(e) => handleRentalFormChange("cccd", e.target.value)} />
                  <button type="button" disabled={checkingCccd} onClick={handleCheckCccd} className="rounded-[16px] bg-amber-300 px-5 py-3 text-sm font-semibold text-slate-950 transition hover:bg-amber-200 disabled:cursor-not-allowed disabled:opacity-50">
                    {checkingCccd ? "Checking..." : "Check"}
                  </button>
                </div>
              </div>

              <div>
                <label className="mb-2 block text-sm font-semibold text-slate-700">Manager Name</label>
                <input value={rentalForm.managerName} readOnly placeholder="Check CCCD first" className="w-full rounded-[16px] border border-slate-200 bg-slate-100 px-4 py-3 text-sm text-slate-700 outline-none" />
              </div>

              <div>
                <label className="mb-2 block text-sm font-semibold text-slate-700">Shop Name</label>
                <input value={rentalForm.shopName} readOnly placeholder="Check CCCD first" className="w-full rounded-[16px] border border-slate-200 bg-slate-100 px-4 py-3 text-sm text-slate-700 outline-none" />
              </div>

              <div>
                <label className="mb-2 block text-sm font-semibold text-slate-700">Location</label>
                <input value={rentalForm.location} placeholder="Enter location" className="w-full rounded-[16px] border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100" onChange={(e) => handleRentalFormChange("location", e.target.value)} />
              </div>

              <div>
                <label className="mb-2 block text-sm font-semibold text-slate-700">Start Date</label>
                <input type="date" min={tomorrowDate} value={rentalForm.startDate} className="w-full rounded-[16px] border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100" onChange={(e) => handleRentalFormChange("startDate", e.target.value)} />
              </div>

              <div>
                <label className="mb-2 block text-sm font-semibold text-slate-700">Electricity Fee</label>
                <input type="number" min="0" value={rentalForm.electricityFee} placeholder="Enter electricity fee" className="w-full rounded-[16px] border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100" onChange={(e) => handleRentalFormChange("electricityFee", e.target.value)} />
              </div>

              <div>
                <label className="mb-2 block text-sm font-semibold text-slate-700">Water Fee</label>
                <input type="number" min="0" value={rentalForm.waterFee} placeholder="Enter water fee" className="w-full rounded-[16px] border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100" onChange={(e) => handleRentalFormChange("waterFee", e.target.value)} />
              </div>

              <div>
                <label className="mb-2 block text-sm font-semibold text-slate-700">Fee</label>
                <input type="number" min="0" value={rentalForm.serviceFee} placeholder="Enter fee" className="w-full rounded-[16px] border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100" onChange={(e) => handleRentalFormChange("serviceFee", e.target.value)} />
              </div>

              <div>
                <label className="mb-2 block text-sm font-semibold text-slate-700">Rental Duration (days)</label>
                <input type="number" min="30" step="30" value={rentalForm.leaseTermDays} placeholder="Example: 30, 60, 90" className="w-full rounded-[16px] border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100" onChange={(e) => handleRentalFormChange("leaseTermDays", e.target.value)} />
              </div>

              <div className="sm:col-span-2">
                <label className="mb-2 block text-sm font-semibold text-slate-700">Contract Image</label>
                <input type="file" accept="image/*" className="w-full rounded-[16px] border border-slate-200 px-4 py-3 text-sm file:mr-4 file:rounded-full file:border-0 file:bg-slate-950 file:px-4 file:py-2 file:text-sm file:font-semibold file:text-white" onChange={(e) => setContractFile(e.target.files?.[0] || null)} />
              </div>
            </div>

            <div className="sticky bottom-0 flex flex-col gap-3 border-t border-slate-200 bg-white px-5 py-4 sm:flex-row sm:items-center sm:justify-between">
              <div className="space-y-1">
                {actionError && <p className="text-sm font-medium text-rose-600">{actionError}</p>}
                {success && <p className="text-sm font-medium text-emerald-600">{success}</p>}
              </div>
              <button type="button" disabled={saving} onClick={handleSubmitRental} className="rounded-full bg-slate-950 px-5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5 disabled:cursor-not-allowed disabled:opacity-50">
                {saving ? "Submitting..." : "Submit Rental"}
              </button>
            </div>
          </div>
        </div>
      )}

      {selectedViewArea && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-950/70 px-4 py-6 backdrop-blur-sm">
          <div className="max-h-[92vh] w-full max-w-5xl overflow-auto rounded-[30px] bg-white shadow-[0_30px_120px_rgba(15,23,42,0.3)]">
            <div className="sticky top-0 z-10 flex items-center justify-between border-b border-slate-200 bg-white px-5 py-4">
              <div>
                <h3 className="text-xl font-black text-slate-950">Rental Area Details</h3>
                <p className="text-sm text-slate-500">
                  Area: {selectedViewArea.areaCode} | Status: {selectedViewArea.status}
                </p>
              </div>
              <button type="button" onClick={closeViewModal} className="rounded-full bg-slate-950 px-4 py-2 text-sm font-semibold text-white">
                Close
              </button>
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
                  <div className="rounded-[18px] bg-white px-4 py-3">
                    <p className="text-xs font-semibold uppercase tracking-[0.18em] text-slate-400">Area Code</p>
                    <p className="mt-1 font-bold text-slate-950">{selectedViewArea.areaCode || "-"}</p>
                  </div>
                  <div className="rounded-[18px] bg-white px-4 py-3">
                    <p className="text-xs font-semibold uppercase tracking-[0.18em] text-slate-400">Floor</p>
                    <p className="mt-1 font-bold text-slate-950">{selectedViewArea.floor || "-"}</p>
                  </div>
                  <div className="rounded-[18px] bg-white px-4 py-3">
                    <p className="text-xs font-semibold uppercase tracking-[0.18em] text-slate-400">Size</p>
                    <p className="mt-1 font-bold text-slate-950">{selectedViewArea.size || "-"}</p>
                  </div>
                  <div className="rounded-[18px] bg-white px-4 py-3">
                    <p className="text-xs font-semibold uppercase tracking-[0.18em] text-slate-400">Monthly Rent</p>
                    <p className="mt-1 font-bold text-slate-950">{formatCurrency(selectedViewArea.monthlyRent)}</p>
                  </div>
                </div>

                <div className="mt-3 rounded-[18px] bg-white px-4 py-3">
                  <p className="text-xs font-semibold uppercase tracking-[0.18em] text-slate-400">Tenant</p>
                  <p className="mt-1 font-bold text-slate-950">{selectedViewArea.tenantName || "No tenant registered"}</p>
                </div>
              </section>

              {selectedViewArea.status === "Rented" ? (
                <section className="rounded-[24px] border border-amber-100 bg-amber-50 p-4">
                  <div className="flex flex-col gap-3 lg:flex-row lg:items-start lg:justify-between">
                    <div>
                      <p className="text-xs font-semibold uppercase tracking-[0.22em] text-amber-700">Monthly Usage</p>
                      <h4 className="mt-2 text-xl font-black text-slate-950">Update electricity and water usage</h4>
                      <p className="mt-1 text-sm text-amber-800">
                        Electricity fee, water fee, and service fee are reused from the rental registration.
                      </p>
                    </div>
                    <button type="button" disabled={saving} onClick={() => handleCancelTenant(selectedViewArea.id)} className="rounded-full bg-rose-500 px-5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5 hover:bg-rose-600 disabled:cursor-not-allowed disabled:opacity-50">
                      Cancel Tenant
                    </button>
                  </div>

                  <div className="mt-5 grid gap-4 sm:grid-cols-2">
                    <div>
                      <label className="mb-2 block text-sm font-semibold text-slate-700">Billing Month</label>
                      <input type="month" value={monthlyBillForm.billingMonth} className="w-full rounded-[16px] border border-slate-200 bg-white px-4 py-3 text-sm outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100" onChange={(e) => handleMonthlyBillFormChange("billingMonth", e.target.value)} />
                    </div>

                    <div>
                      <label className="mb-2 block text-sm font-semibold text-slate-700">Usage Month</label>
                      <input type="month" value={monthlyBillForm.usageMonth} className="w-full rounded-[16px] border border-slate-200 bg-white px-4 py-3 text-sm outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100" onChange={(e) => handleMonthlyBillFormChange("usageMonth", e.target.value)} />
                    </div>

                    <div>
                      <label className="mb-2 block text-sm font-semibold text-slate-700">Electricity Usage</label>
                      <input value={monthlyBillForm.electricityUsage} placeholder="Example: 238" className="w-full rounded-[16px] border border-slate-200 bg-white px-4 py-3 text-sm outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100" onChange={(e) => handleMonthlyBillFormChange("electricityUsage", e.target.value)} />
                    </div>

                    <div>
                      <label className="mb-2 block text-sm font-semibold text-slate-700">Water Usage</label>
                      <input value={monthlyBillForm.waterUsage} placeholder="Example: 18" className="w-full rounded-[16px] border border-slate-200 bg-white px-4 py-3 text-sm outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100" onChange={(e) => handleMonthlyBillFormChange("waterUsage", e.target.value)} />
                    </div>
                  </div>
                </section>
              ) : (
                <section className="rounded-[24px] border border-slate-200 bg-slate-50 p-4">
                  <p className="text-xs font-semibold uppercase tracking-[0.22em] text-slate-400">Tenant Registration</p>
                  <h4 className="mt-2 text-xl font-black text-slate-950">This area is available</h4>
                  <p className="mt-1 text-sm text-slate-600">
                    Register a tenant from this detail view when the rental information is ready.
                  </p>
                  <button
                    type="button"
                    disabled={saving}
                    onClick={() => {
                      openRegisterModal(selectedViewArea);
                      setSelectedViewArea(null);
                    }}
                    className="mt-4 rounded-full bg-slate-950 px-5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5 disabled:cursor-not-allowed disabled:opacity-50"
                  >
                    Register Tenant
                  </button>
                </section>
              )}
            </div>

            <div className="sticky bottom-0 flex flex-col gap-3 border-t border-slate-200 bg-white px-5 py-4 sm:flex-row sm:items-center sm:justify-between">
              <div className="space-y-1">
                {actionError && <p className="text-sm font-medium text-rose-600">{actionError}</p>}
                {success && <p className="text-sm font-medium text-emerald-600">{success}</p>}
              </div>
              {selectedViewArea.status === "Rented" && (
                <button type="button" disabled={saving} onClick={handleSubmitMonthlyBill} className="rounded-full bg-slate-950 px-5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5 disabled:cursor-not-allowed disabled:opacity-50">
                  {saving ? "Saving..." : "Save Monthly Usage"}
                </button>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
