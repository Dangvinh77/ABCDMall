import React, { useEffect, useMemo, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import api from "../../../core/api/api";
import { logoutUser } from "../services/auth";

const initialProfileForm = {
  fullName: "",
  address: "",
  cccd: "",
};

export default function Profile() {
  const [user, setUser] = useState(null);
  const [profileForm, setProfileForm] = useState(initialProfileForm);
  const [history, setHistory] = useState([]);
  const [pendingRequests, setPendingRequests] = useState([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [profileError, setProfileError] = useState("");
  const [profileSuccess, setProfileSuccess] = useState("");
  const [showEditProfile, setShowEditProfile] = useState(false);
  const [cccdFrontImageFile, setCccdFrontImageFile] = useState(null);
  const [cccdBackImageFile, setCccdBackImageFile] = useState(null);
  const [currentPassword, setCurrentPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [otp, setOtp] = useState("");
  const [otpRequested, setOtpRequested] = useState(false);
  const [passwordLoading, setPasswordLoading] = useState(false);
  const [passwordError, setPasswordError] = useState("");
  const [passwordSuccess, setPasswordSuccess] = useState("");
  const [showChangePassword, setShowChangePassword] = useState(false);
  const navigate = useNavigate();

  const syncProfileForm = (profile) => {
    setProfileForm({
      fullName: profile?.fullName || "",
      address: profile?.address || "",
      cccd: profile?.cccd || "",
    });
  };

  const loadProfile = async () => {
    const res = await api.get("/Auth/getprofile");
    setUser(res);
    syncProfileForm(res);
  };

  const loadHistory = async () => {
    const res = await api.get("/Auth/profile-update-history");
    setHistory(res || []);
  };

  const loadPendingRequests = async () => {
    const res = await api.get("/Auth/profile-update-requests/me?status=Pending");
    setPendingRequests(res || []);
  };

  useEffect(() => {
    const loadData = async () => {
      try {
        setLoading(true);
        await Promise.all([loadProfile(), loadHistory(), loadPendingRequests()]);
      } catch {
        setProfileError("Unable to load profile information.");
      } finally {
        setLoading(false);
      }
    };

    loadData();
  }, []);

  const historyRows = useMemo(
    () =>
      history.flatMap((item) =>
        [
          { key: `${item.id}-fullName`, label: "Full Name", previous: item.previousFullName, updated: item.updatedFullName, updatedAt: item.updatedAt },
          { key: `${item.id}-address`, label: "Address", previous: item.previousAddress, updated: item.updatedAddress, updatedAt: item.updatedAt },
          { key: `${item.id}-cccd`, label: "CCCD", previous: item.previousCCCD, updated: item.updatedCCCD, updatedAt: item.updatedAt },
        ].filter((row) => row.previous !== row.updated),
      ),
    [history],
  );

  const handleUpdateProfile = async () => {
    try {
      setSaving(true);
      setProfileError("");
      setProfileSuccess("");

      const formData = new FormData();
      formData.append("fullName", profileForm.fullName);
      formData.append("address", profileForm.address);
      formData.append("cccd", profileForm.cccd);
      if (cccdFrontImageFile) formData.append("cccdFrontImage", cccdFrontImageFile);
      if (cccdBackImageFile) formData.append("cccdBackImage", cccdBackImageFile);

      const res = await api.put("/Auth/updateprofile", formData, {
        headers: { "Content-Type": "multipart/form-data" },
      });

      setUser(res.profile);
      syncProfileForm(res.profile);
      setProfileSuccess(res.message || "Profile update request submitted for admin approval.");
      setShowEditProfile(false);
      setCccdFrontImageFile(null);
      setCccdBackImageFile(null);
      await Promise.all([loadHistory(), loadPendingRequests()]);
    } catch (err) {
      setProfileError(err?.message || "Unable to update profile.");
    } finally {
      setSaving(false);
    }
  };

  const handleRequestResetPasswordOtp = async () => {
    try {
      setPasswordLoading(true);
      setPasswordError("");
      setPasswordSuccess("");
      await api.post("/Auth/resetpassword/request-otp", { currentPassword, newPassword });
      setOtpRequested(true);
      setPasswordSuccess("The OTP has been sent to your email.");
    } catch (err) {
      setPasswordError(err?.message || "Unable to send OTP.");
    } finally {
      setPasswordLoading(false);
    }
  };

  const handleConfirmResetPasswordOtp = async () => {
    try {
      setPasswordLoading(true);
      setPasswordError("");
      setPasswordSuccess("");
      await api.post("/Auth/resetpassword/confirm-otp", { otp });
      setPasswordSuccess("Password reset successful.");
      setCurrentPassword("");
      setNewPassword("");
      setOtp("");
      setOtpRequested(false);
      setShowChangePassword(false);
    } catch (err) {
      setPasswordError(err?.message || "Invalid OTP.");
    } finally {
      setPasswordLoading(false);
    }
  };

  const handleLogout = async () => {
    await logoutUser();
    navigate("/login");
  };

  return (
    <div className="min-h-screen bg-[linear-gradient(180deg,#fff8ef_0%,#fffdf8_42%,#f8fbff_100%)] text-slate-900">
      <div className="mx-auto flex min-h-screen w-full max-w-7xl flex-col px-4 py-6 sm:px-6 lg:px-8">
        <header className="rounded-[28px] border border-white/70 bg-white/80 px-5 py-4 shadow-[0_20px_80px_rgba(15,23,42,0.08)] backdrop-blur-xl sm:px-6">
          <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
            <div>
              <div className="inline-flex items-center gap-2 rounded-full bg-amber-100 px-3 py-1 text-xs font-semibold uppercase tracking-[0.28em] text-amber-700">
                Profile
              </div>
              <h1 className="mt-3 text-3xl font-black tracking-tight sm:text-4xl">Account Information</h1>
            </div>
            <div className="flex flex-wrap gap-3">
              <Link to="/dashboard" className="inline-flex items-center justify-center rounded-full bg-slate-950 px-5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5">Back to Dashboard</Link>
              <button type="button" onClick={handleLogout} className="inline-flex items-center justify-center rounded-full bg-rose-500 px-5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5 hover:bg-rose-600">Logout</button>
            </div>
          </div>
        </header>

        <main className="mt-6 flex-1 space-y-6">
          <section className="rounded-[30px] bg-slate-950 p-6 text-white shadow-[0_30px_120px_rgba(15,23,42,0.26)]">
            {loading ? (
              <p className="text-sm text-slate-300">Loading...</p>
            ) : (
              <div className="grid gap-5 lg:grid-cols-[1fr_1.1fr]">
                <div>
                  <p className="text-sm font-semibold uppercase tracking-[0.35em] text-amber-300">Overview</p>
                  <h2 className="mt-4 text-4xl font-black leading-tight sm:text-5xl">{user?.fullName || user?.email || "Profile"}</h2>
                  <div className="mt-6 space-y-3">
                    <div className="rounded-[20px] bg-white/10 px-4 py-3"><p className="text-xs uppercase tracking-[0.24em] text-white/55">Email</p><p className="mt-2 text-lg font-semibold text-white">{user?.email || "-"}</p></div>
                    <div className="rounded-[20px] bg-white/10 px-4 py-3"><p className="text-xs uppercase tracking-[0.24em] text-white/55">Role</p><p className="mt-2 text-lg font-semibold text-amber-300">{user?.role || "-"}</p></div>
                    <div className="rounded-[20px] bg-white/10 px-4 py-3"><p className="text-xs uppercase tracking-[0.24em] text-white/55">Address</p><p className="mt-2 text-lg font-semibold text-white">{user?.address || "-"}</p></div>
                    <div className="rounded-[20px] bg-white/10 px-4 py-3"><p className="text-xs uppercase tracking-[0.24em] text-white/55">CCCD</p><p className="mt-2 text-lg font-semibold text-white">{user?.cccd || "-"}</p></div>
                  </div>
                </div>

                <div className="space-y-4 rounded-[28px] border border-white/10 bg-white/10 p-5">
                  {profileError && <div className="rounded-[16px] border border-red-400/20 bg-red-500/10 px-3 py-2 text-sm text-red-200">{profileError}</div>}
                  {profileSuccess && <div className="rounded-[16px] border border-emerald-400/20 bg-emerald-500/10 px-3 py-2 text-sm text-emerald-200">{profileSuccess}</div>}

                  <button type="button" onClick={() => { setShowEditProfile(true); syncProfileForm(user); }} className="w-full rounded-[16px] bg-white py-3 text-sm font-semibold text-slate-950 transition hover:-translate-y-0.5">Edit Profile</button>
                  <button type="button" onClick={() => setShowChangePassword(true)} className="w-full rounded-[16px] border border-white/15 bg-white/5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5">Change Password</button>

                  <div className="rounded-[20px] border border-amber-300/20 bg-amber-500/10 px-4 py-4">
                    <div className="flex items-center justify-between gap-3">
                      <div>
                        <p className="text-xs uppercase tracking-[0.24em] text-amber-200">Pending Approval</p>
                        <p className="mt-2 text-lg font-semibold text-white">Profile Update Requests</p>
                      </div>
                      <span className="rounded-full bg-white/10 px-3 py-1 text-xs font-bold text-white">{pendingRequests.length}</span>
                    </div>
                    <div className="mt-4 space-y-3">
                      {pendingRequests.length === 0 ? (
                        <p className="text-sm text-slate-300">No profile update request is waiting for approval.</p>
                      ) : (
                        pendingRequests.map((request) => (
                          <div key={request.id} className="rounded-[18px] bg-white/10 px-4 py-3">
                            <p className="text-sm font-semibold text-white">{request.status}</p>
                            <p className="mt-1 text-xs text-slate-300">{request.requestedFullName || request.requestedCCCD || "Pending update"}</p>
                          </div>
                        ))
                      )}
                    </div>
                  </div>
                </div>
              </div>
            )}
          </section>

          <section className="rounded-[28px] border border-slate-200 bg-white/90 p-5 shadow-[0_20px_80px_rgba(15,23,42,0.08)]">
            <h3 className="text-2xl font-black text-slate-900">Profile Update History</h3>
            <div className="mt-5 overflow-x-auto rounded-[22px] border border-slate-200">
              {historyRows.length === 0 ? (
                <div className="px-4 py-6 text-sm text-slate-500">No profile updates have been recorded yet.</div>
              ) : (
                <table className="w-full min-w-[760px] table-fixed border-collapse text-left">
                  <thead className="bg-slate-100 text-xs font-semibold uppercase tracking-[0.18em] text-slate-500">
                    <tr>
                      <th className="px-4 py-3">Field</th>
                      <th className="px-4 py-3">Previous Value</th>
                      <th className="px-4 py-3">Updated Value</th>
                      <th className="px-4 py-3">Updated At</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-200 text-sm text-slate-700">
                    {historyRows.map((row) => (
                      <tr key={row.key}>
                        <td className="px-4 py-4 font-semibold text-slate-900">{row.label}</td>
                        <td className="px-4 py-4">{row.previous || "-"}</td>
                        <td className="px-4 py-4">{row.updated || "-"}</td>
                        <td className="px-4 py-4">{row.updatedAt ? new Date(row.updatedAt).toLocaleString() : "-"}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              )}
            </div>
          </section>
        </main>
      </div>

      {showEditProfile && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-950/70 px-4 py-6 backdrop-blur-sm">
          <div className="w-full max-w-2xl overflow-hidden rounded-[30px] bg-white shadow-[0_30px_120px_rgba(15,23,42,0.3)]">
            <div className="flex items-center justify-between border-b border-slate-200 px-5 py-4">
              <div>
                <p className="text-xs font-semibold uppercase tracking-[0.24em] text-amber-700">Edit Profile</p>
                <h3 className="mt-1 text-xl font-black text-slate-950">Submit profile update request</h3>
              </div>
              <button type="button" onClick={() => setShowEditProfile(false)} className="rounded-full bg-slate-950 px-4 py-2 text-sm font-semibold text-white">Close</button>
            </div>

            <div className="space-y-4 p-5">
              <label className="block text-sm font-semibold text-slate-700">Full name
                <input value={profileForm.fullName} placeholder="Full name" onChange={(e) => setProfileForm((current) => ({ ...current, fullName: e.target.value }))} className="mt-2 w-full rounded-[16px] border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100" />
              </label>
              <label className="block text-sm font-semibold text-slate-700">Address
                <input value={profileForm.address} placeholder="Address" onChange={(e) => setProfileForm((current) => ({ ...current, address: e.target.value }))} className="mt-2 w-full rounded-[16px] border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100" />
              </label>
              <label className="block text-sm font-semibold text-slate-700">CCCD
                <input value={profileForm.cccd} placeholder="CCCD" onChange={(e) => setProfileForm((current) => ({ ...current, cccd: e.target.value }))} className="mt-2 w-full rounded-[16px] border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100" />
              </label>
              <div className="grid gap-4 sm:grid-cols-2">
                <label className="block text-sm font-semibold text-slate-700">CCCD Front Image
                  <input type="file" accept="image/*" aria-label="CCCD Front Image" onChange={(e) => setCccdFrontImageFile(e.target.files?.[0] || null)} className="mt-2 w-full rounded-[16px] border border-slate-200 px-4 py-3 text-sm file:mr-3 file:rounded-full file:border-0 file:bg-slate-950 file:px-3 file:py-2 file:text-xs file:font-semibold file:text-white" />
                </label>
                <label className="block text-sm font-semibold text-slate-700">CCCD Back Image
                  <input type="file" accept="image/*" aria-label="CCCD Back Image" onChange={(e) => setCccdBackImageFile(e.target.files?.[0] || null)} className="mt-2 w-full rounded-[16px] border border-slate-200 px-4 py-3 text-sm file:mr-3 file:rounded-full file:border-0 file:bg-slate-950 file:px-3 file:py-2 file:text-xs file:font-semibold file:text-white" />
                </label>
              </div>
            </div>

            <div className="flex flex-col gap-3 border-t border-slate-200 px-5 py-4 sm:flex-row sm:justify-end">
              <button type="button" onClick={() => setShowEditProfile(false)} className="rounded-full border border-slate-200 px-5 py-3 text-sm font-semibold text-slate-700 transition hover:bg-slate-100">Cancel</button>
              <button type="button" onClick={handleUpdateProfile} disabled={saving} className="rounded-full bg-slate-950 px-5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5 disabled:opacity-50">{saving ? "Submitting..." : "Submit Request"}</button>
            </div>
          </div>
        </div>
      )}

      {showChangePassword && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-950/70 px-4 py-6 backdrop-blur-sm">
          <div className="w-full max-w-xl overflow-hidden rounded-[30px] bg-white shadow-[0_30px_120px_rgba(15,23,42,0.3)]">
            <div className="flex items-center justify-between border-b border-slate-200 px-5 py-4">
              <div>
                <p className="text-xs font-semibold uppercase tracking-[0.24em] text-amber-700">Change Password</p>
                <h3 className="mt-1 text-xl font-black text-slate-950">Reset with OTP</h3>
              </div>
              <button type="button" onClick={() => setShowChangePassword(false)} className="rounded-full bg-slate-950 px-4 py-2 text-sm font-semibold text-white">Close</button>
            </div>

            <div className="space-y-4 p-5">
              {passwordError && <div className="rounded-[16px] border border-rose-200 bg-rose-50 px-4 py-3 text-sm font-medium text-rose-600">{passwordError}</div>}
              {passwordSuccess && <div className="rounded-[16px] border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm font-medium text-emerald-600">{passwordSuccess}</div>}

              <input type="password" value={currentPassword} placeholder="Current password" onChange={(e) => setCurrentPassword(e.target.value)} className="w-full rounded-[16px] border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100" />
              <input type="password" value={newPassword} placeholder="New password" onChange={(e) => setNewPassword(e.target.value)} className="w-full rounded-[16px] border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100" />
              {otpRequested && <input type="text" value={otp} placeholder="Enter OTP" onChange={(e) => setOtp(e.target.value)} className="w-full rounded-[16px] border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100" />}
            </div>

            <div className="flex flex-col gap-3 border-t border-slate-200 px-5 py-4 sm:flex-row sm:justify-end">
              {!otpRequested ? (
                <button type="button" onClick={handleRequestResetPasswordOtp} disabled={passwordLoading} className="rounded-full bg-slate-950 px-5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5 disabled:opacity-50">{passwordLoading ? "Sending..." : "Send OTP"}</button>
              ) : (
                <button type="button" onClick={handleConfirmResetPasswordOtp} disabled={passwordLoading} className="rounded-full bg-slate-950 px-5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5 disabled:opacity-50">{passwordLoading ? "Verifying..." : "Confirm OTP"}</button>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
