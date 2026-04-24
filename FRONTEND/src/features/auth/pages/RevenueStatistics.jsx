import { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import api from "../../../core/api/api";

const API_ORIGIN = "http://localhost:5184";

const formatCurrency = (value) =>
  new Intl.NumberFormat("vi-VN", {
    style: "currency",
    currency: "VND",
    maximumFractionDigits: 0,
  }).format(value || 0);

const resolveContractUrl = (imagePath) => {
  if (!imagePath) {
    return "";
  }

  const normalizedPath = imagePath.replace(/\\/g, "/").trim();

  if (normalizedPath.startsWith("http://") || normalizedPath.startsWith("https://")) {
    return normalizedPath;
  }

  const relativePath = normalizedPath.startsWith("/")
    ? normalizedPath
    : normalizedPath.startsWith("images/")
      ? `/${normalizedPath}`
      : `/images/contracts/${normalizedPath}`;

    return `${API_ORIGIN}${relativePath}`;
};

const monthOptions = [
  { value: "01", label: "January" },
  { value: "02", label: "February" },
  { value: "03", label: "March" },
  { value: "04", label: "April" },
  { value: "05", label: "May" },
  { value: "06", label: "June" },
  { value: "07", label: "July" },
  { value: "08", label: "August" },
  { value: "09", label: "September" },
  { value: "10", label: "October" },
  { value: "11", label: "November" },
  { value: "12", label: "December" },
];

const locationOptions = ["1F", "2F", "3F", "4F"];
const BILL_PAGE_SIZE = 10;

const getBillingMonthKey = (item) => {
  if (item.billingMonthKey) {
    return item.billingMonthKey;
  }

  const parsedDate = new Date(item.month);
  return Number.isNaN(parsedDate.getTime())
    ? ""
    : `${parsedDate.getFullYear()}-${String(parsedDate.getMonth() + 1).padStart(2, "0")}`;
};

const parseUsageValue = (value) => {
  if (!value) {
    return 0;
  }

  const normalizedValue = String(value).replace(/,/g, "");
  const match = normalizedValue.match(/-?\d+(\.\d+)?/);
  return match ? Number(match[0]) || 0 : 0;
};

export default function RevenueStatistics() {
  const role = localStorage.getItem("role") || "Guest";
  const isAdmin = role === "Admin";
  const [rentalDetails, setRentalDetails] = useState([]);
  const [filters, setFilters] = useState({
    year: "all",
    month: "all",
    location: "all",
  });
  const [billStatus, setBillStatus] = useState("Paid");
  const [currentPage, setCurrentPage] = useState(1);
  const [loading, setLoading] = useState(isAdmin);
  const [error, setError] = useState("");
  const [selectedContract, setSelectedContract] = useState(null);
  const [selectedShopBills, setSelectedShopBills] = useState(null);
  const [selectedRevenueDetail, setSelectedRevenueDetail] = useState(null);

  const yearOptions = useMemo(() => {
    const years = rentalDetails
      .map((item) => getBillingMonthKey(item).slice(0, 4))
      .filter(Boolean);

    return [...new Set(years)].sort((a, b) => Number(b) - Number(a));
  }, [rentalDetails]);

  const filteredDetails = useMemo(
    () =>
      rentalDetails.filter((item) => {
        const billingMonthKey = getBillingMonthKey(item);
        const itemYear = billingMonthKey.slice(0, 4);
        const itemMonth = billingMonthKey.slice(5, 7);
        const itemLocation = String(item.rentalLocation || "").toUpperCase();
        const itemStatus = String(item.paymentStatus || "Unpaid").toLowerCase();

        const matchesYear = filters.year === "all" || itemYear === filters.year;
        const matchesMonth = filters.month === "all" || itemMonth === filters.month;
        const matchesLocation = filters.location === "all" || itemLocation.startsWith(filters.location);
        const matchesStatus = itemStatus === billStatus.toLowerCase();

        return matchesYear && matchesMonth && matchesLocation && matchesStatus;
      }),
    [billStatus, filters, rentalDetails]
  );

  const groupedShopSummaries = useMemo(() => {
    const groups = new Map();

    filteredDetails.forEach((item) => {
      const key = `${item.shopName || ""}__${item.rentalLocation || ""}`;
      const current = groups.get(key);
      const totalDue = Number(item.totalDue) || 0;
      const billingMonthKey = getBillingMonthKey(item);

      if (!current) {
        groups.set(key, {
          key,
          shopName: item.shopName,
          rentalLocation: item.rentalLocation,
          latestMonth: item.month,
          latestBillingMonthKey: billingMonthKey,
          latestTotalDue: totalDue,
          paidCount: String(item.paymentStatus || "").toLowerCase() === "paid" ? 1 : 0,
          unpaidCount: String(item.paymentStatus || "").toLowerCase() === "paid" ? 0 : 1,
          bills: [item],
        });
        return;
      }

      current.bills.push(item);
      if (String(item.paymentStatus || "").toLowerCase() === "paid") {
        current.paidCount += 1;
      } else {
        current.unpaidCount += 1;
      }

      if (billingMonthKey > current.latestBillingMonthKey) {
        current.latestBillingMonthKey = billingMonthKey;
        current.latestMonth = item.month;
        current.latestTotalDue = totalDue;
      }
    });

    return Array.from(groups.values())
      .map((group) => ({
        ...group,
        bills: [...group.bills].sort((left, right) => getBillingMonthKey(right).localeCompare(getBillingMonthKey(left))),
      }))
      .sort((left, right) => left.shopName.localeCompare(right.shopName, undefined, { sensitivity: "base" }));
  }, [filteredDetails]);

  const totals = useMemo(
    () =>
      filteredDetails.reduce(
        (result, item) => ({
          electricityFee:
            result.electricityFee + parseUsageValue(item.electricityUsage) * (Number(item.electricityFee) || 0),
          waterFee:
            result.waterFee + parseUsageValue(item.waterUsage) * (Number(item.waterFee) || 0),
          fee: result.fee + (Number(item.serviceFee) || 0),
          totalDue: result.totalDue + (Number(item.totalDue) || 0),
        }),
        { electricityFee: 0, waterFee: 0, fee: 0, totalDue: 0 }
      ),
    [filteredDetails]
  );

  const totalPages = Math.max(1, Math.ceil(groupedShopSummaries.length / BILL_PAGE_SIZE));
  const safeCurrentPage = Math.min(currentPage, totalPages);
  const startIndex = (safeCurrentPage - 1) * BILL_PAGE_SIZE;
  const paginatedSummaries = groupedShopSummaries.slice(startIndex, startIndex + BILL_PAGE_SIZE);
  const showingStart = groupedShopSummaries.length === 0 ? 0 : startIndex + 1;
  const showingEnd = Math.min(startIndex + BILL_PAGE_SIZE, groupedShopSummaries.length);

  const handleBillStatusChange = (status) => {
    setBillStatus(status);
    setCurrentPage(1);
  };

  const handleFilterChange = (name, value) => {
    setFilters((current) => ({
      ...current,
      [name]: value,
    }));
    setCurrentPage(1);
  };

  const resetFilters = () => {
    setFilters({
      year: "all",
      month: "all",
      location: "all",
    });
    setCurrentPage(1);
  };

  useEffect(() => {
    if (!isAdmin) {
      return;
    }

    const loadRentalRevenue = async () => {
      try {
        setLoading(true);
        setError("");
        const res = await api.get("/ShopInfo");
        setRentalDetails(res.data || []);
      } catch (err) {
        setError(err.response?.data || "Unable to load revenue statistics.");
      } finally {
        setLoading(false);
      }
    };

    loadRentalRevenue();
  }, [isAdmin]);

  if (!isAdmin) {
    return (
      <div className="min-h-screen bg-[linear-gradient(180deg,#fff8ef_0%,#fffdf8_42%,#f8fbff_100%)] px-4 py-6 text-slate-900 sm:px-6 lg:px-8">
        <div className="mx-auto max-w-3xl rounded-[28px] border border-slate-200 bg-white/90 p-6 shadow-[0_20px_80px_rgba(15,23,42,0.08)]">
          <p className="text-xs font-semibold uppercase tracking-[0.28em] text-amber-700">
            Revenue Statistics
          </p>
          <h1 className="mt-3 text-3xl font-black">Admin access only</h1>
          <p className="mt-3 text-sm text-slate-600">
            Revenue reports are only available for admin accounts.
          </p>
          <Link
            to="/admin-management"
            className="mt-6 inline-flex rounded-full bg-slate-950 px-5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5"
          >
            Back to Admin Management
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
                Revenue Statistics
              </div>
              <h1 className="mt-3 text-3xl font-black tracking-tight sm:text-4xl">
                Mall Revenue
              </h1>
              <p className="mt-2 max-w-2xl text-sm text-slate-600 sm:text-base">
                Choose a revenue source on the left and review location-level details on the right.
              </p>
            </div>

            <Link
              to="/admin-management"
              className="inline-flex items-center justify-center rounded-full bg-slate-950 px-5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5"
            >
              Back to Admin Management
            </Link>
          </div>
        </header>

        <main className="mt-6 grid flex-1 gap-5 lg:grid-cols-[3fr_7fr]">
          <aside className="rounded-[30px] border border-slate-200 bg-white/90 p-4 shadow-[0_24px_90px_rgba(15,23,42,0.08)] backdrop-blur-xl">
            <p className="px-2 text-xs font-semibold uppercase tracking-[0.24em] text-slate-400">
              Revenue Sources
            </p>
            <button
              type="button"
              onClick={() => handleBillStatusChange("Paid")}
              className={`mt-4 w-full rounded-[24px] px-5 py-5 text-left shadow-[0_18px_50px_rgba(15,23,42,0.18)] transition hover:-translate-y-0.5 ${
                billStatus === "Paid"
                  ? "bg-slate-950 text-white"
                  : "bg-slate-100 text-slate-700"
              }`}
            >
              <span className={`block text-xs font-semibold uppercase tracking-[0.24em] ${billStatus === "Paid" ? "text-amber-200" : "text-emerald-700"}`}>
                Paid
              </span>
              <span className="mt-2 block text-xl font-black">Paid Rental Bills</span>
              <span className={`mt-2 block text-sm leading-6 ${billStatus === "Paid" ? "text-slate-300" : "text-slate-500"}`}>
                Only paid bills are included in the revenue total.
              </span>
            </button>

            <button
              type="button"
              onClick={() => handleBillStatusChange("Unpaid")}
              className={`mt-3 w-full rounded-[24px] px-5 py-5 text-left shadow-[0_18px_50px_rgba(15,23,42,0.08)] transition hover:-translate-y-0.5 ${
                billStatus === "Unpaid"
                  ? "bg-slate-950 text-white"
                  : "bg-amber-50 text-slate-700"
              }`}
            >
              <span className={`block text-xs font-semibold uppercase tracking-[0.24em] ${billStatus === "Unpaid" ? "text-amber-200" : "text-amber-700"}`}>
                Unpaid
              </span>
              <span className="mt-2 block text-xl font-black">Unpaid Rental Bills</span>
              <span className={`mt-2 block text-sm leading-6 ${billStatus === "Unpaid" ? "text-slate-300" : "text-slate-500"}`}>
                Track outstanding bills waiting for manager payment.
              </span>
            </button>

          </aside>

          <section className="overflow-hidden rounded-[30px] border border-slate-200 bg-white/90 shadow-[0_24px_90px_rgba(15,23,42,0.08)] backdrop-blur-xl">
            <div className="border-b border-slate-200 px-5 py-4 sm:px-6">
              <div className="flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
                <div>
                  <p className="text-xs font-semibold uppercase tracking-[0.24em] text-slate-400">
                    Rental Area Revenue
                  </p>
                  <h2 className="mt-2 text-2xl font-black text-slate-950">Location Details</h2>
                  <p className="mt-1 text-sm text-slate-500">
                    Showing {showingStart} - {showingEnd} of {groupedShopSummaries.length} shops with {billStatus.toLowerCase()} bills
                  </p>
                </div>
                <div className="grid gap-2 text-sm sm:grid-cols-4">
                  <div className="rounded-2xl bg-slate-100 px-4 py-3">
                    <p className="text-xs font-semibold uppercase tracking-[0.16em] text-slate-400">Electricity</p>
                    <p className="mt-1 font-black text-slate-950">{formatCurrency(totals.electricityFee)}</p>
                  </div>
                  <div className="rounded-2xl bg-slate-100 px-4 py-3">
                    <p className="text-xs font-semibold uppercase tracking-[0.16em] text-slate-400">Water</p>
                    <p className="mt-1 font-black text-slate-950">{formatCurrency(totals.waterFee)}</p>
                  </div>
                  <div className="rounded-2xl bg-amber-100 px-4 py-3">
                    <p className="text-xs font-semibold uppercase tracking-[0.16em] text-amber-700">Fee</p>
                    <p className="mt-1 font-black text-slate-950">{formatCurrency(totals.fee)}</p>
                  </div>
                  <div className="rounded-2xl bg-slate-950 px-4 py-3 text-white">
                    <p className="text-xs font-semibold uppercase tracking-[0.16em] text-white/55">
                      {billStatus === "Paid" ? "Revenue" : "Outstanding"}
                    </p>
                    <p className="mt-1 font-black">{formatCurrency(totals.totalDue)}</p>
                  </div>
                </div>
              </div>

              <div className="mt-4 rounded-[24px] border border-slate-200 bg-slate-50 p-4">
                <div className="flex flex-col gap-3 xl:flex-row xl:items-end">
                  <label className="block flex-1">
                    <span className="mb-2 block text-xs font-bold uppercase tracking-[0.18em] text-slate-500">
                      Year
                    </span>
                    <select
                      value={filters.year}
                      className="w-full rounded-2xl border border-slate-200 bg-white px-4 py-3 text-sm font-semibold outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100"
                      onChange={(e) => handleFilterChange("year", e.target.value)}
                    >
                      <option value="all">All years</option>
                      {yearOptions.map((year) => (
                        <option key={year} value={year}>
                          {year}
                        </option>
                      ))}
                    </select>
                  </label>

                  <label className="block flex-1">
                    <span className="mb-2 block text-xs font-bold uppercase tracking-[0.18em] text-slate-500">
                      Month
                    </span>
                    <select
                      value={filters.month}
                      className="w-full rounded-2xl border border-slate-200 bg-white px-4 py-3 text-sm font-semibold outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100"
                      onChange={(e) => handleFilterChange("month", e.target.value)}
                    >
                      <option value="all">All months</option>
                      {monthOptions.map((month) => (
                        <option key={month.value} value={month.value}>
                          {month.label}
                        </option>
                      ))}
                    </select>
                  </label>

                  <label className="block flex-1">
                    <span className="mb-2 block text-xs font-bold uppercase tracking-[0.18em] text-slate-500">
                      Location
                    </span>
                    <select
                      value={filters.location}
                      className="w-full rounded-2xl border border-slate-200 bg-white px-4 py-3 text-sm font-semibold outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100"
                      onChange={(e) => handleFilterChange("location", e.target.value)}
                    >
                      <option value="all">All locations</option>
                      {locationOptions.map((location) => (
                        <option key={location} value={location}>
                          {location}
                        </option>
                      ))}
                    </select>
                  </label>

                  <button
                    type="button"
                    className="rounded-full bg-white px-5 py-3 text-sm font-bold text-slate-700 ring-1 ring-slate-200 transition hover:-translate-y-0.5 hover:bg-slate-950 hover:text-white"
                    onClick={resetFilters}
                  >
                    Reset Filters
                  </button>
                </div>
              </div>
            </div>

            {loading ? (
              <div className="px-6 py-8 text-sm text-slate-500">Loading revenue statistics...</div>
            ) : error ? (
              <div className="px-6 py-8 text-sm font-medium text-rose-600">{error}</div>
            ) : groupedShopSummaries.length === 0 ? (
              <div className="px-6 py-8 text-sm text-slate-500">No {billStatus.toLowerCase()} bills match the selected filters.</div>
            ) : (
              <>
                <div className="overflow-x-auto">
                  <table className="w-full min-w-0 table-fixed border-collapse text-left">
                    <colgroup>
                      <col className="w-[26%]" />
                      <col className="w-[18%]" />
                      <col className="w-[16%]" />
                      <col className="w-[18%]" />
                      <col className="w-[10%]" />
                      <col className="w-[12%]" />
                    </colgroup>
                    <thead className="bg-slate-100 text-xs font-semibold uppercase tracking-[0.16em] text-slate-500">
                      <tr>
                        <th className="px-4 py-3">Shop Name</th>
                        <th className="px-4 py-3">Location</th>
                        <th className="px-4 py-3">Latest Bill</th>
                        <th className="px-4 py-3">Latest Total Due</th>
                        <th className="px-4 py-3">Bills</th>
                        <th className="px-4 py-3">Action</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-slate-200 text-sm text-slate-700">
                      {paginatedSummaries.map((item) => {
                        return (
                          <tr key={item.key} className="align-top transition hover:bg-amber-50/60">
                            <td className="px-4 py-4 font-semibold text-slate-950">
                              <span className="block truncate" title={item.shopName}>
                                {item.shopName}
                              </span>
                            </td>
                            <td className="px-4 py-4">{item.rentalLocation}</td>
                            <td className="px-4 py-4">{item.latestMonth || "-"}</td>
                            <td className="px-4 py-4 font-bold text-slate-950">{formatCurrency(item.latestTotalDue)}</td>
                            <td className="px-4 py-4">
                              <span className="inline-flex rounded-full bg-slate-100 px-3 py-1 text-xs font-bold text-slate-700">
                                {item.bills.length} bill{item.bills.length > 1 ? "s" : ""}
                              </span>
                            </td>
                            <td className="px-4 py-4">
                              <button
                                type="button"
                                onClick={() => setSelectedShopBills(item)}
                                className="rounded-full bg-slate-950 px-4 py-2 text-xs font-semibold text-white transition hover:-translate-y-0.5"
                              >
                                View
                              </button>
                            </td>
                          </tr>
                        );
                      })}
                    </tbody>
                  </table>
                </div>

                <div className="flex flex-col gap-3 border-t border-slate-200 px-5 py-4 text-sm text-slate-600 sm:flex-row sm:items-center sm:justify-between sm:px-6">
                  <p>
                    Showing <span className="font-bold text-slate-950">{showingStart}</span>
                    {" - "}
                    <span className="font-bold text-slate-950">{showingEnd}</span>
                    {" of "}
                    <span className="font-bold text-slate-950">{groupedShopSummaries.length}</span> shops
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

      {selectedContract && (
        <div className="fixed inset-0 z-[70] flex items-center justify-center bg-slate-950/70 px-4 backdrop-blur-sm">
          <div className="max-h-[90vh] w-full max-w-4xl overflow-hidden rounded-[28px] bg-white shadow-[0_30px_120px_rgba(15,23,42,0.3)]">
            <div className="flex items-center justify-between border-b border-slate-200 px-5 py-4">
              <h3 className="text-lg font-black text-slate-950">Contract Image</h3>
              <button
                type="button"
                onClick={() => setSelectedContract(null)}
                className="rounded-full bg-slate-950 px-4 py-2 text-sm font-semibold text-white"
              >
                Close
              </button>
            </div>
            <div className="max-h-[78vh] overflow-auto bg-slate-100 p-4">
              <img
                src={selectedContract}
                alt="Contract"
                className="mx-auto max-h-[72vh] w-auto rounded-2xl object-contain shadow-lg"
              />
            </div>
          </div>
        </div>
      )}

      {selectedShopBills && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-950/70 px-4 py-6 backdrop-blur-sm">
          <div className="max-h-[92vh] w-full max-w-5xl overflow-auto rounded-[30px] bg-white shadow-[0_30px_120px_rgba(15,23,42,0.3)]">
            <div className="sticky top-0 z-10 flex items-center justify-between border-b border-slate-200 bg-white px-5 py-4">
              <div>
                <h3 className="text-xl font-black text-slate-950">Monthly Bills</h3>
                <p className="text-sm text-slate-500">
                  {selectedShopBills.shopName} | {selectedShopBills.rentalLocation}
                </p>
              </div>
              <button
                type="button"
                onClick={() => setSelectedShopBills(null)}
                className="rounded-full bg-slate-950 px-4 py-2 text-sm font-semibold text-white"
              >
                Close
              </button>
            </div>

            <div className="p-5">
              <div className="mb-5 grid gap-3 sm:grid-cols-3">
                <SummaryPill label="Shop Name" value={selectedShopBills.shopName} />
                <SummaryPill label="Location" value={selectedShopBills.rentalLocation} />
                <SummaryPill label="Total Bills" value={String(selectedShopBills.bills.length)} />
              </div>

              <div className="overflow-x-auto rounded-[24px] border border-slate-200">
                <table className="w-full min-w-0 table-fixed border-collapse text-left">
                  <colgroup>
                    <col className="w-[24%]" />
                    <col className="w-[22%]" />
                    <col className="w-[22%]" />
                    <col className="w-[16%]" />
                    <col className="w-[16%]" />
                  </colgroup>
                  <thead className="bg-slate-100 text-xs font-semibold uppercase tracking-[0.16em] text-slate-500">
                    <tr>
                      <th className="px-4 py-3">Billing Month</th>
                      <th className="px-4 py-3">Usage Month</th>
                      <th className="px-4 py-3">Total Due</th>
                      <th className="px-4 py-3">Status</th>
                      <th className="px-4 py-3">Action</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-200 text-sm text-slate-700">
                    {selectedShopBills.bills.map((bill) => {
                      const isPaid = String(bill.paymentStatus || "").toLowerCase() === "paid";

                      return (
                        <tr key={bill.id} className="align-top transition hover:bg-amber-50/60">
                          <td className="px-4 py-4 font-semibold text-slate-950">{bill.month || "-"}</td>
                          <td className="px-4 py-4">{bill.usageMonth || "-"}</td>
                          <td className="px-4 py-4 font-bold text-slate-950">{formatCurrency(bill.totalDue)}</td>
                          <td className="px-4 py-4">
                            <span className={`inline-flex rounded-full px-3 py-1 text-xs font-bold ${isPaid ? "bg-emerald-100 text-emerald-700" : "bg-amber-100 text-amber-700"}`}>
                              {isPaid ? "Paid" : "Unpaid"}
                            </span>
                          </td>
                          <td className="px-4 py-4">
                            <button
                              type="button"
                              onClick={() => setSelectedRevenueDetail(bill)}
                              className="rounded-full bg-slate-950 px-4 py-2 text-xs font-semibold text-white transition hover:-translate-y-0.5"
                            >
                              View
                            </button>
                          </td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        </div>
      )}

      {selectedRevenueDetail && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-950/70 px-4 py-6 backdrop-blur-sm">
          <div className="max-h-[92vh] w-full max-w-5xl overflow-auto rounded-[30px] bg-white shadow-[0_30px_120px_rgba(15,23,42,0.3)]">
            <div className="sticky top-0 z-10 flex items-center justify-between border-b border-slate-200 bg-white px-5 py-4">
              <div>
                <h3 className="text-xl font-black text-slate-950">Rental Bill Details</h3>
                <p className="text-sm text-slate-500">
                  {selectedRevenueDetail.shopName} | {selectedRevenueDetail.month}
                </p>
              </div>
              <button
                type="button"
                onClick={() => setSelectedRevenueDetail(null)}
                className="rounded-full bg-slate-950 px-4 py-2 text-sm font-semibold text-white"
              >
                Close
              </button>
            </div>

            <div className="space-y-5 p-5">
              <section className="rounded-[24px] border border-slate-200 bg-slate-50 p-4">
                <p className="text-xs font-semibold uppercase tracking-[0.22em] text-slate-400">Shop Rental</p>
                <h4 className="mt-2 text-2xl font-black text-slate-950">{selectedRevenueDetail.shopName}</h4>

                <div className="mt-5 grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
                  <Detail label="Location" value={selectedRevenueDetail.rentalLocation} />
                  <Detail label="Billing Month" value={selectedRevenueDetail.month} />
                  <Detail label="Usage Month" value={selectedRevenueDetail.usageMonth} />
                  <Detail label="Start Date" value={selectedRevenueDetail.leaseStartDate} />
                  <Detail label="Manager" value={selectedRevenueDetail.managerName} />
                  <Detail label="CCCD" value={selectedRevenueDetail.cccd} />
                  <Detail label="Lease Term" value={selectedRevenueDetail.leaseTermDays ? `${selectedRevenueDetail.leaseTermDays} days` : "-"} />
                  <Detail label="Status" value={selectedRevenueDetail.paymentStatus || "Unpaid"} />
                </div>
              </section>

              <section className="rounded-[24px] border border-amber-100 bg-amber-50 p-4">
                <p className="text-xs font-semibold uppercase tracking-[0.22em] text-amber-700">Usage and Fees</p>
                <div className="mt-5 grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
                  <Detail label="Electricity" value={selectedRevenueDetail.electricityUsage || "-"} />
                  <Detail label="Electricity Fee" value={formatCurrency(selectedRevenueDetail.electricityFee)} />
                  <Detail label="Water" value={selectedRevenueDetail.waterUsage || "-"} />
                  <Detail label="Water Fee" value={formatCurrency(selectedRevenueDetail.waterFee)} />
                  <Detail label="Service Fee" value={formatCurrency(selectedRevenueDetail.serviceFee)} />
                  <Detail label="Total Due" value={formatCurrency(selectedRevenueDetail.totalDue)} strong />
                  <Detail
                    label="Paid At"
                    value={selectedRevenueDetail.paidAtUtc ? new Date(selectedRevenueDetail.paidAtUtc).toLocaleString() : "-"}
                  />
                </div>
              </section>

              <section className="rounded-[24px] border border-slate-200 bg-slate-50 p-4">
                <p className="text-xs font-semibold uppercase tracking-[0.22em] text-slate-400">Actions</p>
                <div className="mt-4 flex flex-col gap-3 sm:flex-row">
                  <button
                    type="button"
                    disabled={!resolveContractUrl(selectedRevenueDetail.contractImage || selectedRevenueDetail.contractImages)}
                    onClick={() => {
                      setSelectedContract(resolveContractUrl(selectedRevenueDetail.contractImage || selectedRevenueDetail.contractImages));
                    }}
                    className="rounded-full bg-slate-950 px-5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5 disabled:cursor-not-allowed disabled:bg-slate-200 disabled:text-slate-400 disabled:hover:translate-y-0"
                  >
                    View Contract
                  </button>
                </div>
              </section>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

function Detail({ label, value, strong = false }) {
  return (
    <div className="rounded-[18px] bg-white px-4 py-3">
      <p className="text-xs font-semibold uppercase tracking-[0.18em] text-slate-400">{label}</p>
      <p className={`mt-1 break-words ${strong ? "text-lg font-black text-slate-950" : "font-bold text-slate-950"}`}>
        {value || "-"}
      </p>
    </div>
  );
}

function SummaryPill({ label, value }) {
  return (
    <div className="rounded-[18px] bg-slate-50 px-4 py-3">
      <p className="text-xs font-semibold uppercase tracking-[0.18em] text-slate-400">{label}</p>
      <p className="mt-1 break-words font-bold text-slate-950">{value || "-"}</p>
    </div>
  );
}
