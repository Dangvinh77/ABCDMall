import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import api from "../../../core/api/api";

const API_ORIGIN = "http://localhost:5184";

const formatCurrency = (value) =>
  new Intl.NumberFormat("vi-VN", {
    style: "currency",
    currency: "VND",
    maximumFractionDigits: 0,
  }).format(value);

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

export default function ShopInfo() {
  const role = localStorage.getItem("role") || "Guest";
  const isManager = role === "Manager";
  const [shopRentals, setShopRentals] = useState([]);
  const [loading, setLoading] = useState(isManager);
  const [error, setError] = useState("");
  const [selectedContract, setSelectedContract] = useState(null);

  useEffect(() => {
    if (!isManager) {
      return;
    }

    const loadShopInfos = async () => {
      try {
        setLoading(true);
        setError("");
        const res = await api.get("/ShopInfo");
        setShopRentals(res.data);
      } catch (err) {
        setError(err.response?.data || "Unable to load shop rental information.");
      } finally {
        setLoading(false);
      }
    };

    loadShopInfos();
  }, [isManager]);

  if (!isManager) {
    return (
      <div className="min-h-screen bg-[linear-gradient(180deg,#fff8ef_0%,#fffdf8_42%,#f8fbff_100%)] px-4 pb-6 pt-28 text-slate-900 sm:px-6 lg:px-8">
        <div className="mx-auto max-w-3xl rounded-[28px] border border-slate-200 bg-white/90 p-6 shadow-[0_20px_80px_rgba(15,23,42,0.08)]">
          <p className="text-xs font-semibold uppercase tracking-[0.28em] text-amber-700">
            Shop Info
          </p>
          <h1 className="mt-3 text-3xl font-black">Manager access only</h1>
          <p className="mt-3 text-sm text-slate-600">
            This rental information is only available for manager accounts.
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

  return (
    <div className="min-h-screen bg-[linear-gradient(180deg,#fff8ef_0%,#fffdf8_42%,#f8fbff_100%)] text-slate-900">
      <div className="mx-auto flex min-h-screen w-full max-w-7xl flex-col px-4 pb-6 pt-28 sm:px-6 lg:px-8">
        <header className="rounded-[28px] border border-white/70 bg-white/80 px-5 py-4 shadow-[0_20px_80px_rgba(15,23,42,0.08)] backdrop-blur-xl sm:px-6">
          <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
            <div>
              <div className="inline-flex items-center gap-2 rounded-full bg-amber-100 px-3 py-1 text-xs font-semibold uppercase tracking-[0.28em] text-amber-700">
                Shop Info
              </div>
              <h1 className="mt-3 text-3xl font-black tracking-tight sm:text-4xl">
                Rental Payment Overview
              </h1>
              <p className="mt-2 max-w-2xl text-sm text-slate-600 sm:text-base">
                Monthly rental, utility, fee, and contract details for the manager shop.
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

        <main className="mt-6 flex-1">
          <section className="overflow-hidden rounded-[30px] border border-slate-200 bg-white/90 shadow-[0_24px_90px_rgba(15,23,42,0.08)] backdrop-blur-xl">
            <div className="border-b border-slate-200 px-5 py-4 sm:px-6">
              <p className="text-xs font-semibold uppercase tracking-[0.24em] text-slate-400">
                Last 2 Months
              </p>
              <h2 className="mt-2 text-2xl font-black text-slate-950">Shop Rental Information</h2>
            </div>

            {loading ? (
              <div className="px-6 py-8 text-sm text-slate-500">Loading shop rental information...</div>
            ) : error ? (
              <div className="px-6 py-8 text-sm font-medium text-rose-600">{error}</div>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full min-w-[1560px] table-fixed border-collapse text-left">
                  <colgroup>
                    <col className="w-[8%]" />
                    <col className="w-[8%]" />
                    <col className="w-[8%]" />
                    <col className="w-[8%]" />
                    <col className="w-[9%]" />
                    <col className="w-[8%]" />
                    <col className="w-[9%]" />
                    <col className="w-[7%]" />
                    <col className="w-[8%]" />
                    <col className="w-[8%]" />
                    <col className="w-[8%]" />
                    <col className="w-[9%]" />
                    <col className="w-[6%]" />
                  </colgroup>
                  <thead className="bg-slate-100 text-xs font-semibold uppercase tracking-[0.16em] text-slate-500">
                    <tr>
                      <th className="px-4 py-3">Shop Name</th>
                      <th className="px-4 py-3">Location</th>
                      <th className="px-4 py-3">Billing Month</th>
                      <th className="px-4 py-3">Usage Month</th>
                      <th className="px-4 py-3">Start Date</th>
                      <th className="px-4 py-3">Electricity</th>
                      <th className="px-4 py-3">Electricity Fee</th>
                      <th className="px-4 py-3">Water</th>
                      <th className="px-4 py-3">Water Fee</th>
                      <th className="px-4 py-3">Fee</th>
                      <th className="px-4 py-3">Lease Term</th>
                      <th className="px-4 py-3">Total Due</th>
                      <th className="px-4 py-3">Contract</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-200 text-sm text-slate-700">
                    {shopRentals.map((item) => {
                      const contractUrl = resolveContractUrl(item.contractImage);

                      return (
                        <tr key={item.id} className="align-top transition hover:bg-amber-50/60">
                          <td className="px-4 py-4 font-semibold text-slate-950">{item.shopName}</td>
                          <td className="px-4 py-4">{item.rentalLocation}</td>
                          <td className="px-4 py-4">{item.month}</td>
                          <td className="px-4 py-4">{item.usageMonth}</td>
                          <td className="px-4 py-4">{item.leaseStartDate}</td>
                          <td className="px-4 py-4">{item.electricityUsage}</td>
                          <td className="px-4 py-4">{formatCurrency(item.electricityFee)}</td>
                          <td className="px-4 py-4">{item.waterUsage}</td>
                          <td className="px-4 py-4">{formatCurrency(item.waterFee)}</td>
                          <td className="px-4 py-4">{formatCurrency(item.serviceFee)}</td>
                          <td className="px-4 py-4">{item.leaseTermDays} days</td>
                          <td className="px-4 py-4 font-bold text-slate-950">{formatCurrency(item.totalDue)}</td>
                          <td className="px-4 py-4">
                            <button
                              type="button"
                              disabled={!contractUrl}
                              onClick={() => setSelectedContract(contractUrl)}
                              className="rounded-full bg-slate-950 px-4 py-2 text-xs font-semibold text-white transition hover:-translate-y-0.5 disabled:cursor-not-allowed disabled:bg-slate-200 disabled:text-slate-400 disabled:hover:translate-y-0"
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
            )}
          </section>
        </main>
      </div>

      {selectedContract && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-950/70 px-4 backdrop-blur-sm">
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
    </div>
  );
}
