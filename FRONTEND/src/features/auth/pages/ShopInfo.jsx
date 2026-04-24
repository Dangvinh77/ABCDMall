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
  const [rentalInfo, setRentalInfo] = useState(null);
  const [shopRentals, setShopRentals] = useState([]);
  const [loading, setLoading] = useState(isManager);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [payingBillId, setPayingBillId] = useState("");
  const [selectedContract, setSelectedContract] = useState(null);
  const [selectedRental, setSelectedRental] = useState(null);

  const loadShopInfos = async () => {
    try {
      setLoading(true);
      setError("");
      const [rentalInfoRes, billsRes] = await Promise.all([
        api.get("/ShopInfo/rental-information"),
        api.get("/ShopInfo"),
      ]);
      setRentalInfo(rentalInfoRes.data);
      setShopRentals(billsRes.data || []);
    } catch (err) {
      setError(err.response?.data || "Unable to load shop rental information.");
    } finally {
      setLoading(false);
    }
  };

  const handlePayBill = async (billId) => {
    try {
      setPayingBillId(billId);
      setError("");
      setSuccess("");
      const res = await api.post(`/RentalPayments/${billId}/checkout-session`);
      if (res.data?.checkoutUrl) {
        window.location.href = res.data.checkoutUrl;
        return;
      }

      setError("Unable to start online payment.");
    } catch (err) {
      setError(err.response?.data || "Unable to start online payment.");
    } finally {
      setPayingBillId("");
    }
  };

  useEffect(() => {
    if (!isManager) {
      return;
    }

    const query = new URLSearchParams(window.location.search);
    const payment = query.get("payment");
    if (payment === "success") {
      setSuccess("Payment completed. The bill status will update after Stripe confirms the payment.");
    } else if (payment === "cancel") {
      setError("Payment was cancelled. You can try again from the unpaid bill row.");
    }

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
          <section className="mb-6 overflow-hidden rounded-[30px] border border-slate-200 bg-white/90 shadow-[0_24px_90px_rgba(15,23,42,0.08)] backdrop-blur-xl">
            <div className="border-b border-slate-200 px-5 py-4 sm:px-6">
              <p className="text-xs font-semibold uppercase tracking-[0.24em] text-slate-400">
                Rental Information
              </p>
              <h2 className="mt-2 text-2xl font-black text-slate-950">Current Rental Information</h2>
            </div>

            {loading ? (
              <div className="px-6 py-8 text-sm text-slate-500">Loading rental information...</div>
            ) : error ? (
              <div className="px-6 py-8 text-sm font-medium text-rose-600">{error}</div>
            ) : !rentalInfo ? (
              <div className="px-6 py-8 text-sm text-slate-500">No rental information found for this manager account.</div>
            ) : (
              <div className="space-y-5 p-5">
                <section className="rounded-[24px] border border-slate-200 bg-slate-50 p-4">
                  <p className="text-xs font-semibold uppercase tracking-[0.22em] text-slate-400">Shop Rental</p>
                  <h4 className="mt-2 text-2xl font-black text-slate-950">{rentalInfo.shopName}</h4>

                  <div className="mt-5 grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
                    <Detail label="Location" value={rentalInfo.rentalLocation} />
                    <Detail label="Floor" value={rentalInfo.floor} />
                    <Detail label="Start Date" value={rentalInfo.leaseStartDate} />
                    <Detail label="Manager" value={rentalInfo.managerName} />
                    <Detail label="CCCD" value={rentalInfo.cccd} />
                    <Detail label="Electricity Fee" value={formatCurrency(rentalInfo.electricityFee)} />
                    <Detail label="Water Fee" value={formatCurrency(rentalInfo.waterFee)} />
                    <Detail label="Service Fee" value={formatCurrency(rentalInfo.serviceFee)} />
                    <Detail label="Lease Term" value={rentalInfo.leaseTermDays ? `${rentalInfo.leaseTermDays} days` : "-"} />
                  </div>
                </section>

                <section className="rounded-[24px] border border-slate-200 bg-slate-50 p-4">
                  <p className="text-xs font-semibold uppercase tracking-[0.22em] text-slate-400">Contract</p>
                  <div className="mt-4 flex flex-col gap-3 sm:flex-row">
                    <button
                      type="button"
                      disabled={!resolveContractUrl(rentalInfo.contractImage || rentalInfo.contractImages)}
                      onClick={() => setSelectedContract(resolveContractUrl(rentalInfo.contractImage || rentalInfo.contractImages))}
                      className="rounded-full bg-slate-950 px-5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5 disabled:cursor-not-allowed disabled:bg-slate-200 disabled:text-slate-400 disabled:hover:translate-y-0"
                    >
                      View Contract
                    </button>
                  </div>
                </section>
              </div>
            )}
          </section>

          <section className="overflow-hidden rounded-[30px] border border-slate-200 bg-white/90 shadow-[0_24px_90px_rgba(15,23,42,0.08)] backdrop-blur-xl">
            <div className="border-b border-slate-200 px-5 py-4 sm:px-6">
              <p className="text-xs font-semibold uppercase tracking-[0.24em] text-slate-400">
                Monthly Bills
              </p>
              <h2 className="mt-2 text-2xl font-black text-slate-950">Monthly Rental Bills</h2>
            </div>

            {loading ? (
              <div className="px-6 py-8 text-sm text-slate-500">Loading shop rental information...</div>
            ) : error ? (
              <div className="px-6 py-8 text-sm font-medium text-rose-600">{error}</div>
            ) : shopRentals.length === 0 ? (
              <>
                {success && <div className="border-b border-emerald-100 bg-emerald-50 px-6 py-3 text-sm font-semibold text-emerald-700">{success}</div>}
                <div className="px-6 py-8 text-sm text-slate-500">
                  No monthly bill has been generated yet. Bills will appear here after the admin updates monthly usage.
                </div>
              </>
            ) : (
              <>
                {success && <div className="border-b border-emerald-100 bg-emerald-50 px-6 py-3 text-sm font-semibold text-emerald-700">{success}</div>}
                <div className="overflow-x-auto">
                <table className="w-full min-w-0 table-fixed border-collapse text-left">
                  <colgroup>
                    <col className="w-[22%]" />
                    <col className="w-[18%]" />
                    <col className="w-[16%]" />
                    <col className="w-[18%]" />
                    <col className="w-[12%]" />
                    <col className="w-[14%]" />
                  </colgroup>
                  <thead className="bg-slate-100 text-xs font-semibold uppercase tracking-[0.16em] text-slate-500">
                    <tr>
                      <th className="px-4 py-3">Shop Name</th>
                      <th className="px-4 py-3">Location</th>
                      <th className="px-4 py-3">Billing Month</th>
                      <th className="px-4 py-3">Total Due</th>
                      <th className="px-4 py-3">Status</th>
                      <th className="px-4 py-3">Action</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-200 text-sm text-slate-700">
                    {shopRentals.map((item) => {
                      const isPaid = String(item.paymentStatus || "").toLowerCase() === "paid";

                      return (
                        <tr key={item.id} className="align-top transition hover:bg-amber-50/60">
                          <td className="px-4 py-4 font-semibold text-slate-950">
                            <span className="block truncate" title={item.shopName}>
                              {item.shopName}
                            </span>
                          </td>
                          <td className="px-4 py-4">{item.rentalLocation}</td>
                          <td className="px-4 py-4">{item.month}</td>
                          <td className="px-4 py-4 font-bold text-slate-950">{formatCurrency(item.totalDue)}</td>
                          <td className="px-4 py-4">
                            <span className={`inline-flex rounded-full px-3 py-1 text-xs font-bold ${isPaid ? "bg-emerald-100 text-emerald-700" : "bg-amber-100 text-amber-700"}`}>
                              {isPaid ? "Paid" : "Unpaid"}
                            </span>
                          </td>
                          <td className="px-4 py-4">
                            <button
                              type="button"
                              onClick={() => setSelectedRental(item)}
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

      {selectedRental && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-950/70 px-4 py-6 backdrop-blur-sm">
          <div className="max-h-[92vh] w-full max-w-5xl overflow-auto rounded-[30px] bg-white shadow-[0_30px_120px_rgba(15,23,42,0.3)]">
            <div className="sticky top-0 z-10 flex items-center justify-between border-b border-slate-200 bg-white px-5 py-4">
              <div>
                <h3 className="text-xl font-black text-slate-950">Rental Bill Details</h3>
                <p className="text-sm text-slate-500">
                  {selectedRental.shopName} | {selectedRental.month}
                </p>
              </div>
              <button
                type="button"
                onClick={() => setSelectedRental(null)}
                className="rounded-full bg-slate-950 px-4 py-2 text-sm font-semibold text-white"
              >
                Close
              </button>
            </div>

            <div className="space-y-5 p-5">
              <section className="rounded-[24px] border border-slate-200 bg-slate-50 p-4">
                <p className="text-xs font-semibold uppercase tracking-[0.22em] text-slate-400">Shop Rental</p>
                <h4 className="mt-2 text-2xl font-black text-slate-950">{selectedRental.shopName}</h4>

                <div className="mt-5 grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
                  <Detail label="Location" value={selectedRental.rentalLocation} />
                  <Detail label="Billing Month" value={selectedRental.month} />
                  <Detail label="Usage Month" value={selectedRental.usageMonth} />
                  <Detail label="Start Date" value={selectedRental.leaseStartDate} />
                  <Detail label="Manager" value={selectedRental.managerName} />
                  <Detail label="CCCD" value={selectedRental.cccd} />
                  <Detail label="Lease Term" value={`${selectedRental.leaseTermDays} days`} />
                  <Detail label="Status" value={selectedRental.paymentStatus || "Unpaid"} />
                </div>
              </section>

              <section className="rounded-[24px] border border-amber-100 bg-amber-50 p-4">
                <p className="text-xs font-semibold uppercase tracking-[0.22em] text-amber-700">Usage and Fees</p>
                <div className="mt-5 grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
                  <Detail label="Electricity" value={selectedRental.electricityUsage || "-"} />
                  <Detail label="Electricity Fee" value={formatCurrency(selectedRental.electricityFee)} />
                  <Detail label="Water" value={selectedRental.waterUsage || "-"} />
                  <Detail label="Water Fee" value={formatCurrency(selectedRental.waterFee)} />
                  <Detail label="Service Fee" value={formatCurrency(selectedRental.serviceFee)} />
                  <Detail label="Total Due" value={formatCurrency(selectedRental.totalDue)} strong />
                  <Detail label="Paid At" value={selectedRental.paidAtUtc ? new Date(selectedRental.paidAtUtc).toLocaleString() : "-"} />
                </div>
              </section>

              <section className="rounded-[24px] border border-slate-200 bg-slate-50 p-4">
                <p className="text-xs font-semibold uppercase tracking-[0.22em] text-slate-400">Actions</p>
                <div className="mt-4 flex flex-col gap-3 sm:flex-row">
                  <button
                    type="button"
                    disabled={!resolveContractUrl(selectedRental.contractImage || selectedRental.contractImages)}
                    onClick={() => {
                      setSelectedContract(resolveContractUrl(selectedRental.contractImage || selectedRental.contractImages));
                    }}
                    className="rounded-full bg-slate-950 px-5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5 disabled:cursor-not-allowed disabled:bg-slate-200 disabled:text-slate-400 disabled:hover:translate-y-0"
                  >
                    View Contract
                  </button>

                  {String(selectedRental.paymentStatus || "").toLowerCase() === "paid" ? (
                    <span className="inline-flex items-center rounded-full bg-emerald-100 px-5 py-3 text-sm font-bold text-emerald-700">
                      Payment Completed
                    </span>
                  ) : (
                    <button
                      type="button"
                      disabled={payingBillId === selectedRental.id}
                      onClick={() => handlePayBill(selectedRental.id)}
                      className="rounded-full bg-amber-300 px-5 py-3 text-sm font-bold text-slate-950 transition hover:-translate-y-0.5 hover:bg-amber-200 disabled:cursor-not-allowed disabled:opacity-50"
                    >
                      {payingBillId === selectedRental.id ? "Opening..." : "Pay Now"}
                    </button>
                  )}
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
