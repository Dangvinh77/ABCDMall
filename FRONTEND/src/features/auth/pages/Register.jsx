import { useState } from "react";
import { Link } from "react-router-dom";
import api from "../../../core/api/api";
import { AdminMapPickerModal } from "../../admin-map/components/AdminMapPickerModal";

export default function RegisterModern() {
  const [email, setEmail] = useState("");
  const [fullName, setFullName] = useState("");
  const [shopName, setShopName] = useState("");
  const [cccd, setCccd] = useState("");
  const [floor, setFloor] = useState("");
  const [locationSlot, setLocationSlot] = useState("");
  const [leaseStartDate, setLeaseStartDate] = useState("");
  const [leaseTermDays, setLeaseTermDays] = useState("");
  const [electricityFee, setElectricityFee] = useState("");
  const [waterFee, setWaterFee] = useState("");
  const [serviceFee, setServiceFee] = useState("");
  const [avatarFile, setAvatarFile] = useState(null);
  const [cccdFrontFile, setCccdFrontFile] = useState(null);
  const [cccdBackFile, setCccdBackFile] = useState(null);
  const [contractFile, setContractFile] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [isMapOpen, setIsMapOpen] = useState(false);
  const [mapLocationId, setMapLocationId] = useState(null);
  const [displaySlotName, setDisplaySlotName] = useState("");

  const handleRegister = async () => {
    try {
      setLoading(true);
      setError("");
      setSuccess("");

      const formData = new FormData();
      formData.append("email", email);
      formData.append("fullName", fullName);
      formData.append("shopName", shopName);
      formData.append("cccd", cccd);
      formData.append("floor", floor);
      formData.append("locationSlot", locationSlot);
      formData.append("leaseStartDate", leaseStartDate);
      formData.append("leaseTermDays", leaseTermDays);
      formData.append("electricityFee", electricityFee);
      formData.append("waterFee", waterFee);
      formData.append("serviceFee", serviceFee);

      if (mapLocationId !== null && mapLocationId !== undefined) {
        formData.append("mapLocationId", String(mapLocationId));
      }

      if (avatarFile) formData.append("avatar", avatarFile);
      if (cccdFrontFile) formData.append("cccdFrontImage", cccdFrontFile);
      if (cccdBackFile) formData.append("cccdBackImage", cccdBackFile);
      if (contractFile) formData.append("contractImage", contractFile);

      const res = await api.post("/Auth/register", formData, {
        headers: { "Content-Type": "multipart/form-data" },
      });

      setSuccess(
        res.data.emailSent
          ? "User created successfully. A one-time password and change-password link were sent to the manager email."
          : "User created successfully, but the one-time password email could not be sent.",
      );

      setEmail("");
      setFullName("");
      setShopName("");
      setCccd("");
      setFloor("");
      setLocationSlot("");
      setLeaseStartDate("");
      setLeaseTermDays("");
      setElectricityFee("");
      setWaterFee("");
      setServiceFee("");
      setAvatarFile(null);
      setCccdFrontFile(null);
      setCccdBackFile(null);
      setContractFile(null);
      setMapLocationId(null);
      setDisplaySlotName("");
    } catch (err) {
      setError(err.response?.data || "Server error");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-[linear-gradient(180deg,#fff8ef_0%,#fffdf8_42%,#f8fbff_100%)] text-slate-900">
      {isMapOpen && (
        <AdminMapPickerModal
          onClose={() => setIsMapOpen(false)}
          onSelectSlot={(locId, slot, floorLevel) => {
            setMapLocationId(locId);
            setLocationSlot(slot);
            setFloor(floorLevel);
            setDisplaySlotName(`${floorLevel} - Lot ${slot}`);
            setIsMapOpen(false);
          }}
        />
      )}

      <div className="mx-auto flex min-h-screen w-full max-w-7xl flex-col px-4 py-6 sm:px-6 lg:px-8">
        <header className="rounded-[28px] border border-white/70 bg-white/80 px-5 py-4 shadow-[0_20px_80px_rgba(15,23,42,0.08)] backdrop-blur-xl sm:px-6">
          <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
            <div>
              <div className="inline-flex items-center gap-2 rounded-full bg-amber-100 px-3 py-1 text-xs font-semibold uppercase tracking-[0.28em] text-amber-700">
                Register
              </div>
              <h1 className="mt-3 text-3xl font-black tracking-tight sm:text-4xl">
                Create a New Account
              </h1>
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
          <section className="relative overflow-hidden rounded-[34px] bg-slate-950 px-6 py-7 text-white shadow-[0_30px_120px_rgba(15,23,42,0.26)] sm:px-8 sm:py-9">
            <div className="absolute inset-0 bg-[radial-gradient(circle_at_top_left,_rgba(251,191,36,0.32),_transparent_28%),radial-gradient(circle_at_bottom_right,_rgba(14,165,233,0.24),_transparent_30%)]" />

            <div className="relative grid gap-6 lg:grid-cols-[1.05fr_0.95fr]">
              <div>
                <h2 className="text-4xl font-black leading-tight sm:text-5xl">Register</h2>
                <p className="mt-4 max-w-2xl text-sm leading-7 text-slate-300 sm:text-base">
                  Use this form to create a manager account and connect it to a shop profile.
                </p>
              </div>

              <div className="rounded-[28px] border border-white/10 bg-white/10 p-5 backdrop-blur-md">
                <div className="space-y-4">
                  {error && <div className="rounded-[20px] border border-red-400/20 bg-red-500/10 px-4 py-3 text-sm text-red-200">{error}</div>}
                  {success && <div className="rounded-[20px] border border-emerald-400/20 bg-emerald-500/10 px-4 py-3 text-sm text-emerald-200">{success}</div>}

                  <div className="rounded-[18px] border border-amber-300/30 bg-amber-500/10 p-4">
                    <label className="mb-2 block text-sm font-semibold text-amber-300">
                      Shop location on mall map
                    </label>
                    <div className="flex gap-2">
                      <input
                        readOnly
                        value={displaySlotName}
                        placeholder="No slot selected yet..."
                        className="w-full rounded-[14px] border border-white/10 bg-black/20 px-4 py-2 text-white outline-none"
                      />
                      <button
                        type="button"
                        onClick={() => setIsMapOpen(true)}
                        className="whitespace-nowrap rounded-[14px] bg-amber-400 px-4 py-2 text-sm font-bold text-slate-900 hover:bg-amber-300"
                      >
                        Open Map
                      </button>
                    </div>
                  </div>

                  <div>
                    <label className="mb-2 block text-sm font-semibold text-white/80">Full Name</label>
                    <input value={fullName} className="w-full rounded-[18px] border border-white/10 bg-black/10 px-4 py-3 text-white outline-none" onChange={(e) => setFullName(e.target.value)} />
                  </div>

                  <div>
                    <label className="mb-2 block text-sm font-semibold text-white/80">Email</label>
                    <input value={email} className="w-full rounded-[18px] border border-white/10 bg-black/10 px-4 py-3 text-white outline-none" onChange={(e) => setEmail(e.target.value)} />
                  </div>

                  <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                    <div>
                      <label className="mb-2 block text-sm font-semibold text-white/80">Shop Name</label>
                      <input value={shopName} className="w-full rounded-[18px] border border-white/10 bg-black/10 px-4 py-2 text-white outline-none" onChange={(e) => setShopName(e.target.value)} />
                    </div>
                    <div>
                      <label className="mb-2 block text-sm font-semibold text-white/80">CCCD</label>
                      <input value={cccd} className="w-full rounded-[18px] border border-white/10 bg-black/10 px-4 py-2 text-white outline-none" onChange={(e) => setCccd(e.target.value)} />
                    </div>
                  </div>

                  <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                    <div>
                      <label className="mb-2 block text-sm font-semibold text-white/80">Floor</label>
                      <input value={floor} readOnly placeholder="Selected from map" className="w-full rounded-[18px] border border-white/10 bg-black/20 px-4 py-2 text-white outline-none" />
                    </div>
                    <div>
                      <label className="mb-2 block text-sm font-semibold text-white/80">Location</label>
                      <input value={locationSlot} readOnly placeholder="Selected from map" className="w-full rounded-[18px] border border-white/10 bg-black/20 px-4 py-2 text-white outline-none" />
                    </div>
                  </div>

                  <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                    <div>
                      <label className="mb-2 block text-sm font-semibold text-white/80">Start Date</label>
                      <input type="date" value={leaseStartDate} className="w-full rounded-[18px] border border-white/10 bg-black/10 px-4 py-2 text-white outline-none" onChange={(e) => setLeaseStartDate(e.target.value)} />
                    </div>
                    <div>
                      <label className="mb-2 block text-sm font-semibold text-white/80">Lease Term (days)</label>
                      <input type="number" min="1" value={leaseTermDays} className="w-full rounded-[18px] border border-white/10 bg-black/10 px-4 py-2 text-white outline-none" onChange={(e) => setLeaseTermDays(e.target.value)} />
                    </div>
                  </div>

                  <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
                    <div>
                      <label className="mb-2 block text-sm font-semibold text-white/80">Electricity Fee</label>
                      <input type="number" min="1" value={electricityFee} className="w-full rounded-[18px] border border-white/10 bg-black/10 px-4 py-2 text-white outline-none" onChange={(e) => setElectricityFee(e.target.value)} />
                    </div>
                    <div>
                      <label className="mb-2 block text-sm font-semibold text-white/80">Water Fee</label>
                      <input type="number" min="1" value={waterFee} className="w-full rounded-[18px] border border-white/10 bg-black/10 px-4 py-2 text-white outline-none" onChange={(e) => setWaterFee(e.target.value)} />
                    </div>
                    <div>
                      <label className="mb-2 block text-sm font-semibold text-white/80">Fee</label>
                      <input type="number" min="1" value={serviceFee} className="w-full rounded-[18px] border border-white/10 bg-black/10 px-4 py-2 text-white outline-none" onChange={(e) => setServiceFee(e.target.value)} />
                    </div>
                  </div>

                  <div>
                    <label className="mb-2 block text-sm font-semibold text-white/80">Avatar</label>
                    <input type="file" accept="image/*" className="w-full rounded-[18px] border border-white/10 bg-black/10 px-4 py-2 text-white outline-none file:mr-4 file:rounded-full file:border-0 file:bg-white file:px-4 file:py-2 file:text-sm file:font-semibold file:text-slate-950" onChange={(e) => setAvatarFile(e.target.files?.[0] ?? null)} />
                  </div>

                  <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                    <div>
                      <label className="mb-2 block text-sm font-semibold text-white/80">CCCD Front Image</label>
                      <input type="file" accept="image/*" className="w-full rounded-[18px] border border-white/10 bg-black/10 px-4 py-2 text-white outline-none file:mr-4 file:rounded-full file:border-0 file:bg-white file:px-4 file:py-2 file:text-sm file:font-semibold file:text-slate-950" onChange={(e) => setCccdFrontFile(e.target.files?.[0] ?? null)} />
                    </div>
                    <div>
                      <label className="mb-2 block text-sm font-semibold text-white/80">CCCD Back Image</label>
                      <input type="file" accept="image/*" className="w-full rounded-[18px] border border-white/10 bg-black/10 px-4 py-2 text-white outline-none file:mr-4 file:rounded-full file:border-0 file:bg-white file:px-4 file:py-2 file:text-sm file:font-semibold file:text-slate-950" onChange={(e) => setCccdBackFile(e.target.files?.[0] ?? null)} />
                    </div>
                  </div>

                  <div>
                    <label className="mb-2 block text-sm font-semibold text-white/80">Contract Image</label>
                    <input type="file" accept="image/*" className="w-full rounded-[18px] border border-white/10 bg-black/10 px-4 py-2 text-white outline-none file:mr-4 file:rounded-full file:border-0 file:bg-white file:px-4 file:py-2 file:text-sm file:font-semibold file:text-slate-950" onChange={(e) => setContractFile(e.target.files?.[0] ?? null)} />
                  </div>

                  <div className="rounded-[18px] border border-emerald-300/30 bg-emerald-500/10 p-4 text-sm leading-6 text-emerald-100">
                    The system will generate a 6-digit one-time password and send it with a change-password link to the manager email. The link is valid for 24 hours.
                  </div>

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
