import React, { useState } from "react";
import { Link } from "react-router-dom";
import api from "../../../core/api/api";

export default function Register() {
  const [form, setForm] = useState({
    email: "",
    fullName: "",
    shopName: "",
    cccd: "",
    floor: "",
    locationSlot: "",
    leaseStartDate: "",
    leaseTermDays: "",
    electricityFee: "",
    waterFee: "",
    serviceFee: "",
  });
  const [files, setFiles] = useState({
    avatar: null,
    cccdFrontImage: null,
    cccdBackImage: null,
    contractImage: null,
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  const setValue = (key, value) => setForm((current) => ({ ...current, [key]: value }));
  const setFile = (key, file) => setFiles((current) => ({ ...current, [key]: file }));

  const handleRegister = async () => {
    try {
      setLoading(true);
      setError("");
      setSuccess("");

      const formData = new FormData();
      Object.entries(form).forEach(([key, value]) => formData.append(key, value));
      if (files.avatar) formData.append("avatar", files.avatar);
      if (files.cccdFrontImage) formData.append("cccdFrontImage", files.cccdFrontImage);
      if (files.cccdBackImage) formData.append("cccdBackImage", files.cccdBackImage);
      if (files.contractImage) formData.append("contractImage", files.contractImage);

      const res = await api.post("/Auth/register", formData);
      setSuccess(
        res.emailSent
          ? "User created successfully. A one-time password and change-password link were sent to the manager email."
          : "User created successfully, but the one-time password email could not be sent.",
      );
      setForm({
        email: "",
        fullName: "",
        shopName: "",
        cccd: "",
        floor: "",
        locationSlot: "",
        leaseStartDate: "",
        leaseTermDays: "",
        electricityFee: "",
        waterFee: "",
        serviceFee: "",
      });
      setFiles({
        avatar: null,
        cccdFrontImage: null,
        cccdBackImage: null,
        contractImage: null,
      });
        } catch (err) {
            setError(err?.message || "Server error");
        } finally {
            setLoading(false);
        }
  };

  return (
    <div className="min-h-screen bg-[linear-gradient(180deg,#fff8ef_0%,#fffdf8_42%,#f8fbff_100%)] text-slate-900">
      <div className="mx-auto flex min-h-screen w-full max-w-7xl flex-col px-4 py-6 sm:px-6 lg:px-8">
        <header className="rounded-[28px] border border-white/70 bg-white/80 px-5 py-4 shadow-[0_20px_80px_rgba(15,23,42,0.08)] backdrop-blur-xl sm:px-6">
          <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
            <div>
              <div className="inline-flex items-center gap-2 rounded-full bg-amber-100 px-3 py-1 text-xs font-semibold uppercase tracking-[0.28em] text-amber-700">
                Register
              </div>
              <h1 className="mt-3 text-3xl font-black tracking-tight sm:text-4xl">Create Manager Account</h1>
            </div>
            <Link to="/dashboard" className="inline-flex items-center justify-center rounded-full bg-slate-950 px-5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5">
              Back to Dashboard
            </Link>
          </div>
        </header>

        <main className="mt-6 flex-1">
          <section className="rounded-[32px] bg-slate-950 px-6 py-7 text-white shadow-[0_30px_120px_rgba(15,23,42,0.26)] sm:px-8 sm:py-9">
            <div className="grid gap-6 lg:grid-cols-[1fr_1.1fr]">
              <div>
                <p className="text-sm font-semibold uppercase tracking-[0.35em] text-amber-300">Onboarding</p>
                <h2 className="mt-4 text-4xl font-black leading-tight sm:text-5xl">Manager Registration</h2>
              </div>

              <div className="rounded-[28px] border border-white/10 bg-white/10 p-5 backdrop-blur-md">
                <div className="space-y-4">
                  {error && <div className="rounded-[18px] border border-red-400/20 bg-red-500/10 px-4 py-3 text-sm text-red-200">{error}</div>}
                  {success && <div className="rounded-[18px] border border-emerald-400/20 bg-emerald-500/10 px-4 py-3 text-sm text-emerald-200">{success}</div>}

                  <label className="block text-sm font-semibold text-white/80">Full Name
                    <input aria-label="Full Name" value={form.fullName} onChange={(e) => setValue("fullName", e.target.value)} className="mt-2 w-full rounded-[16px] border border-white/10 bg-black/10 px-4 py-3 text-white outline-none" />
                  </label>
                  <label className="block text-sm font-semibold text-white/80">Email
                    <input aria-label="Email" value={form.email} onChange={(e) => setValue("email", e.target.value)} className="mt-2 w-full rounded-[16px] border border-white/10 bg-black/10 px-4 py-3 text-white outline-none" />
                  </label>

                  <div className="grid gap-4 sm:grid-cols-2">
                    <label className="block text-sm font-semibold text-white/80">Shop Name
                      <input aria-label="Shop Name" value={form.shopName} onChange={(e) => setValue("shopName", e.target.value)} className="mt-2 w-full rounded-[16px] border border-white/10 bg-black/10 px-4 py-3 text-white outline-none" />
                    </label>
                    <label className="block text-sm font-semibold text-white/80">CCCD
                      <input aria-label="CCCD" value={form.cccd} onChange={(e) => setValue("cccd", e.target.value)} className="mt-2 w-full rounded-[16px] border border-white/10 bg-black/10 px-4 py-3 text-white outline-none" />
                    </label>
                  </div>

                  <div className="grid gap-4 sm:grid-cols-2">
                    <label className="block text-sm font-semibold text-white/80">Floor
                      <input aria-label="Floor" value={form.floor} onChange={(e) => setValue("floor", e.target.value)} className="mt-2 w-full rounded-[16px] border border-white/10 bg-black/10 px-4 py-3 text-white outline-none" />
                    </label>
                    <label className="block text-sm font-semibold text-white/80">Location Slot
                      <input aria-label="Location Slot" value={form.locationSlot} onChange={(e) => setValue("locationSlot", e.target.value)} className="mt-2 w-full rounded-[16px] border border-white/10 bg-black/10 px-4 py-3 text-white outline-none" />
                    </label>
                  </div>

                  <div className="grid gap-4 sm:grid-cols-2">
                    <label className="block text-sm font-semibold text-white/80">Lease Start Date
                      <input aria-label="Lease Start Date" type="date" value={form.leaseStartDate} onChange={(e) => setValue("leaseStartDate", e.target.value)} className="mt-2 w-full rounded-[16px] border border-white/10 bg-black/10 px-4 py-3 text-white outline-none" />
                    </label>
                    <label className="block text-sm font-semibold text-white/80">Lease Term Days
                      <input aria-label="Lease Term Days" type="number" value={form.leaseTermDays} onChange={(e) => setValue("leaseTermDays", e.target.value)} className="mt-2 w-full rounded-[16px] border border-white/10 bg-black/10 px-4 py-3 text-white outline-none" />
                    </label>
                  </div>

                  <div className="grid gap-4 sm:grid-cols-3">
                    <label className="block text-sm font-semibold text-white/80">Electricity Fee
                      <input aria-label="Electricity Fee" type="number" value={form.electricityFee} onChange={(e) => setValue("electricityFee", e.target.value)} className="mt-2 w-full rounded-[16px] border border-white/10 bg-black/10 px-4 py-3 text-white outline-none" />
                    </label>
                    <label className="block text-sm font-semibold text-white/80">Water Fee
                      <input aria-label="Water Fee" type="number" value={form.waterFee} onChange={(e) => setValue("waterFee", e.target.value)} className="mt-2 w-full rounded-[16px] border border-white/10 bg-black/10 px-4 py-3 text-white outline-none" />
                    </label>
                    <label className="block text-sm font-semibold text-white/80">Service Fee
                      <input aria-label="Service Fee" type="number" value={form.serviceFee} onChange={(e) => setValue("serviceFee", e.target.value)} className="mt-2 w-full rounded-[16px] border border-white/10 bg-black/10 px-4 py-3 text-white outline-none" />
                    </label>
                  </div>

                  <label className="block text-sm font-semibold text-white/80">Avatar
                    <input type="file" accept="image/*" onChange={(e) => setFile("avatar", e.target.files?.[0] ?? null)} className="mt-2 w-full rounded-[16px] border border-white/10 bg-black/10 px-4 py-3 text-white file:mr-4 file:rounded-full file:border-0 file:bg-white file:px-4 file:py-2 file:text-sm file:font-semibold file:text-slate-950" />
                  </label>
                  <div className="grid gap-4 sm:grid-cols-2">
                    <label className="block text-sm font-semibold text-white/80">CCCD Front Image
                      <input type="file" accept="image/*" onChange={(e) => setFile("cccdFrontImage", e.target.files?.[0] ?? null)} className="mt-2 w-full rounded-[16px] border border-white/10 bg-black/10 px-4 py-3 text-white file:mr-4 file:rounded-full file:border-0 file:bg-white file:px-4 file:py-2 file:text-sm file:font-semibold file:text-slate-950" />
                    </label>
                    <label className="block text-sm font-semibold text-white/80">CCCD Back Image
                      <input type="file" accept="image/*" onChange={(e) => setFile("cccdBackImage", e.target.files?.[0] ?? null)} className="mt-2 w-full rounded-[16px] border border-white/10 bg-black/10 px-4 py-3 text-white file:mr-4 file:rounded-full file:border-0 file:bg-white file:px-4 file:py-2 file:text-sm file:font-semibold file:text-slate-950" />
                    </label>
                  </div>
                  <label className="block text-sm font-semibold text-white/80">Contract Image
                    <input type="file" accept="image/*" onChange={(e) => setFile("contractImage", e.target.files?.[0] ?? null)} className="mt-2 w-full rounded-[16px] border border-white/10 bg-black/10 px-4 py-3 text-white file:mr-4 file:rounded-full file:border-0 file:bg-white file:px-4 file:py-2 file:text-sm file:font-semibold file:text-slate-950" />
                  </label>

                  <button onClick={handleRegister} disabled={loading} className="w-full rounded-[18px] bg-white py-3 text-sm font-semibold text-slate-950 transition hover:-translate-y-0.5 disabled:opacity-50">
                    {loading ? "Creating..." : "Create User"}
                  </button>
                </div>
              </div>
            </div>
          </section>
        </main>
      </div>
    </div>
  );
}
