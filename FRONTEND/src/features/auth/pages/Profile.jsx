import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import api from "../../../core/api/api";
import { logoutUser } from "../services/auth";

const initialProfileForm = {
  fullName: "",
  address: "",
  image: "",
  cccd: "",
};

const API_ORIGIN = "http://localhost:5184";

export default function Profile() {
  const [user, setUser] = useState(null);
  const [profileForm, setProfileForm] = useState(initialProfileForm);
  const [history, setHistory] = useState([]);
  const [profileLoading, setProfileLoading] = useState(true);
  const [historyLoading, setHistoryLoading] = useState(true);
  const [profileSaving, setProfileSaving] = useState(false);
  const [profileError, setProfileError] = useState("");
  const [profileSuccess, setProfileSuccess] = useState("");
  const [showEditProfile, setShowEditProfile] = useState(false);
  const [avatarFile, setAvatarFile] = useState(null);
  const [currentPassword, setCurrentPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [otp, setOtp] = useState("");
  const [passwordLoading, setPasswordLoading] = useState(false);
  const [passwordError, setPasswordError] = useState("");
  const [passwordSuccess, setPasswordSuccess] = useState("");
  const [showChangePassword, setShowChangePassword] = useState(false);
  const [otpRequested, setOtpRequested] = useState(false);
  const navigate = useNavigate();

  const syncProfileForm = (profile) => {
    setProfileForm({
      fullName: profile?.fullName || "",
      address: profile?.address || "",
      image: profile?.image || "",
      cccd: profile?.cccd || "",
    });
    setAvatarFile(null);
  };

  const resolveAvatarUrl = (imagePath) => {
    if (!imagePath) {
      return "";
    }

    const normalizedPath = imagePath.replace(/\\/g, "/").trim();

    if (normalizedPath.startsWith("http://") || normalizedPath.startsWith("https://")) {
      return normalizedPath;
    }

    const relativePath = normalizedPath.startsWith("/")
      ? normalizedPath
      : `/${normalizedPath}`;

    return `${API_ORIGIN}${relativePath}`;
  };

  const loadProfile = async () => {
    const res = await api.get("/Auth/getprofile");
    setUser(res.data);
    syncProfileForm(res.data);
  };

  const loadHistory = async () => {
    const res = await api.get("/Auth/profile-update-history");
    setHistory(res.data);
  };

  useEffect(() => {
    const loadData = async () => {
      try {
        setProfileLoading(true);
        setHistoryLoading(true);
        await Promise.all([loadProfile(), loadHistory()]);
      } catch {
        alert("Unauthorized");
      } finally {
        setProfileLoading(false);
        setHistoryLoading(false);
      }
    };

    loadData();
  }, []);

  const handleProfileInputChange = (field, value) => {
    setProfileForm((prev) => ({
      ...prev,
      [field]: value,
    }));
  };

  const handleUpdateProfile = async () => {
    try {
      setProfileSaving(true);
      setProfileError("");
      setProfileSuccess("");

      const formData = new FormData();
      formData.append("fullName", profileForm.fullName);
      formData.append("address", profileForm.address);
      formData.append("cccd", profileForm.cccd);

      if (avatarFile) {
        formData.append("avatar", avatarFile);
      }

      const res = await api.put("/Auth/updateprofile", formData, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      });
      setUser(res.data.profile);
      syncProfileForm(res.data.profile);
      setProfileSuccess(res.data.message || "Profile updated successfully.");
      setShowEditProfile(false);
      await loadHistory();
    } catch (err) {
      setProfileError(err.response?.data || "Unable to update profile.");
    } finally {
      setProfileSaving(false);
    }
  };

  const handleRequestResetPasswordOtp = async () => {
    try {
      setPasswordLoading(true);
      setPasswordError("");
      setPasswordSuccess("");

      await api.post("/Auth/resetpassword/request-otp", {
        currentPassword,
        newPassword,
      });

      setOtpRequested(true);
      setPasswordSuccess("The OTP has been sent to your email.");
    } catch (err) {
      setPasswordError(err.response?.data || "Unable to send OTP.");
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
      setPasswordError(err.response?.data || "Invalid OTP.");
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
              <h1 className="mt-3 text-3xl font-black tracking-tight sm:text-4xl">
                Account Information
              </h1>
              <p className="mt-2 max-w-2xl text-sm text-slate-600 sm:text-base">
                View and manage the basic information of the account currently signed in to the system.
              </p>
            </div>

            <div className="flex flex-wrap gap-3">
              <Link
                to="/dashboard"
                className="inline-flex items-center justify-center rounded-full bg-slate-950 px-5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5"
              >
                Back to Dashboard
              </Link>
              <button
                type="button"
                onClick={handleLogout}
                className="inline-flex items-center justify-center rounded-full bg-rose-500 px-5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5 hover:bg-rose-600"
              >
                Logout
              </button>
            </div>
          </div>
        </header>

        <main className="mt-6 flex-1 space-y-6">
          <section className="relative overflow-hidden rounded-[34px] bg-slate-950 px-6 py-7 text-white shadow-[0_30px_120px_rgba(15,23,42,0.26)] sm:px-8 sm:py-9">
            <div className="absolute inset-0 bg-[radial-gradient(circle_at_top_left,_rgba(251,191,36,0.32),_transparent_28%),radial-gradient(circle_at_bottom_right,_rgba(14,165,233,0.24),_transparent_30%)]" />
            <div className="absolute right-[-40px] top-[-30px] h-40 w-40 rounded-full bg-amber-300/20 blur-3xl" />
            <div className="absolute bottom-[-50px] left-[-20px] h-44 w-44 rounded-full bg-cyan-400/15 blur-3xl" />

            <div className="relative grid gap-6 lg:grid-cols-[1.1fr_0.9fr]">
              <div>
                <p className="text-sm font-semibold uppercase tracking-[0.35em] text-amber-300">
                  Overview
                </p>
                <h2 className="mt-4 text-4xl font-black leading-tight sm:text-5xl">
                  Profile
                </h2>
                <p className="mt-4 max-w-2xl text-sm leading-7 text-slate-300 sm:text-base">
                  The profile currently displays these fields: Email, Role, Full Name,
                  Address, and CCCD. Avatar is shown in the image section.
                </p>

                <div className="mt-6 flex h-72 items-center justify-center overflow-hidden rounded-[28px] border border-white/10 bg-white/10">
                  {profileLoading ? (
                    <div className="flex items-center justify-center text-sm text-slate-300">
                      Loading...
                    </div>
                  ) : user?.image ? (
                    <img
                      src={resolveAvatarUrl(user.image)}
                      alt="Avatar"
                      className="h-40 w-40 rounded-full border-4 border-white/20 object-cover shadow-lg"
                    />
                  ) : (
                    <div className="flex h-40 w-40 items-center justify-center rounded-full border-4 border-white/20 bg-white/10 text-6xl font-black text-white shadow-lg">
                      {(user?.fullName || user?.email || "U").charAt(0).toUpperCase()}
                    </div>
                  )}
                </div>
              </div>

              <div className="rounded-[28px] border border-white/10 bg-white/10 p-5 backdrop-blur-md">
                {profileLoading ? (
                  <p className="text-sm text-slate-300">Loading...</p>
                ) : (
                  <div className="space-y-4">
                    {profileError && (
                      <div className="rounded-[16px] border border-red-400/20 bg-red-500/10 px-3 py-2 text-sm text-red-200">
                        {profileError}
                      </div>
                    )}

                    {profileSuccess && (
                      <div className="rounded-[16px] border border-emerald-400/20 bg-emerald-500/10 px-3 py-2 text-sm text-emerald-200">
                        {profileSuccess}
                      </div>
                    )}

                    <div className="rounded-[22px] border border-white/10 bg-black/10 px-4 py-4">
                      <p className="text-xs uppercase tracking-[0.24em] text-white/55">Email</p>
                      <p className="mt-2 break-all text-lg font-semibold text-white">
                        {user?.email}
                      </p>
                    </div>

                    <div className="rounded-[22px] border border-white/10 bg-black/10 px-4 py-4">
                      <p className="text-xs uppercase tracking-[0.24em] text-white/55">Role</p>
                      <p className="mt-2 text-lg font-semibold text-amber-300">
                        {user?.role}
                      </p>
                    </div>

                    <div className="rounded-[22px] border border-white/10 bg-black/10 px-4 py-4">
                      <p className="text-xs uppercase tracking-[0.24em] text-white/55">Full Name</p>
                      <p className="mt-2 text-lg font-semibold text-white">
                        {user?.fullName || "-"}
                      </p>
                    </div>

                    <div className="rounded-[22px] border border-white/10 bg-black/10 px-4 py-4">
                      <p className="text-xs uppercase tracking-[0.24em] text-white/55">Address</p>
                      <p className="mt-2 text-lg font-semibold text-white">
                        {user?.address || "-"}
                      </p>
                    </div>

                    <div className="rounded-[22px] border border-white/10 bg-black/10 px-4 py-4">
                      <p className="text-xs uppercase tracking-[0.24em] text-white/55">CCCD</p>
                      <p className="mt-2 text-lg font-semibold text-white">
                        {user?.cccd || "-"}
                      </p>
                    </div>

                    <div className="rounded-[22px] border border-white/10 bg-black/10 px-4 py-4">
                      {!showEditProfile ? (
                        <button
                          type="button"
                          onClick={() => {
                            setShowEditProfile(true);
                            setProfileError("");
                            setProfileSuccess("");
                            syncProfileForm(user);
                          }}
                          className="w-full rounded-[16px] bg-white py-3 text-sm font-semibold text-slate-950 transition hover:-translate-y-0.5"
                        >
                          Edit Profile
                        </button>
                      ) : (
                        <div className="space-y-3">
                          <input
                            value={profileForm.fullName}
                            placeholder="Full name"
                            className="w-full rounded-[16px] border border-white/10 bg-white/5 px-4 py-3 text-white outline-none transition placeholder:text-slate-400 focus:border-amber-300 focus:ring-4 focus:ring-amber-200/10"
                            onChange={(e) => handleProfileInputChange("fullName", e.target.value)}
                          />

                          <input
                            value={profileForm.address}
                            placeholder="Address"
                            className="w-full rounded-[16px] border border-white/10 bg-white/5 px-4 py-3 text-white outline-none transition placeholder:text-slate-400 focus:border-amber-300 focus:ring-4 focus:ring-amber-200/10"
                            onChange={(e) => handleProfileInputChange("address", e.target.value)}
                          />

                          <input
                            type="file"
                            accept="image/*"
                            className="w-full rounded-[16px] border border-white/10 bg-white/5 px-4 py-3 text-sm text-white outline-none file:mr-4 file:rounded-full file:border-0 file:bg-white file:px-4 file:py-2 file:text-sm file:font-semibold file:text-slate-950"
                            onChange={(e) => setAvatarFile(e.target.files?.[0] || null)}
                          />

                          <p className="text-xs text-slate-400">
                            Current avatar path: {profileForm.image || "No avatar uploaded"}
                          </p>

                          <input
                            value={profileForm.cccd}
                            placeholder="CCCD"
                            className="w-full rounded-[16px] border border-white/10 bg-white/5 px-4 py-3 text-white outline-none transition placeholder:text-slate-400 focus:border-amber-300 focus:ring-4 focus:ring-amber-200/10"
                            onChange={(e) => handleProfileInputChange("cccd", e.target.value)}
                          />

                          <div className="flex gap-3">
                            <button
                              type="button"
                              onClick={handleUpdateProfile}
                              disabled={profileSaving}
                              className="flex-1 rounded-[16px] bg-white py-3 text-sm font-semibold text-slate-950 transition hover:-translate-y-0.5 disabled:cursor-not-allowed disabled:opacity-50"
                            >
                              {profileSaving ? "Saving..." : "Save Profile"}
                            </button>

                            <button
                              type="button"
                              onClick={() => {
                                setShowEditProfile(false);
                                setProfileError("");
                                setProfileSuccess("");
                                syncProfileForm(user);
                              }}
                              className="flex-1 rounded-[16px] border border-white/15 bg-white/5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5"
                            >
                              Cancel
                            </button>
                          </div>
                        </div>
                      )}
                    </div>

                    <div className="rounded-[22px] border border-white/10 bg-black/10 px-4 py-4">
                      {!showChangePassword ? (
                        <button
                          type="button"
                          onClick={() => {
                            setShowChangePassword(true);
                            setPasswordError("");
                            setPasswordSuccess("");
                          }}
                          className="w-full rounded-[16px] bg-white py-3 text-sm font-semibold text-slate-950 transition hover:-translate-y-0.5"
                        >
                          Change Password
                        </button>
                      ) : (
                        <div className="space-y-3">
                          {passwordError && (
                            <div className="rounded-[16px] border border-red-400/20 bg-red-500/10 px-3 py-2 text-sm text-red-200">
                              {passwordError}
                            </div>
                          )}

                          {passwordSuccess && (
                            <div className="rounded-[16px] border border-emerald-400/20 bg-emerald-500/10 px-3 py-2 text-sm text-emerald-200">
                              {passwordSuccess}
                            </div>
                          )}

                          <input
                            type="password"
                            value={currentPassword}
                            placeholder="Current password"
                            className="w-full rounded-[16px] border border-white/10 bg-white/5 px-4 py-3 text-white outline-none transition placeholder:text-slate-400 focus:border-amber-300 focus:ring-4 focus:ring-amber-200/10"
                            onChange={(e) => setCurrentPassword(e.target.value)}
                          />

                          <input
                            type="password"
                            value={newPassword}
                            placeholder="New password"
                            className="w-full rounded-[16px] border border-white/10 bg-white/5 px-4 py-3 text-white outline-none transition placeholder:text-slate-400 focus:border-amber-300 focus:ring-4 focus:ring-amber-200/10"
                            onChange={(e) => setNewPassword(e.target.value)}
                          />

                          {!otpRequested ? (
                            <div className="flex gap-3">
                              <button
                                type="button"
                                onClick={handleRequestResetPasswordOtp}
                                disabled={passwordLoading}
                                className="flex-1 rounded-[16px] bg-white py-3 text-sm font-semibold text-slate-950 transition hover:-translate-y-0.5 disabled:cursor-not-allowed disabled:opacity-50"
                              >
                                {passwordLoading ? "Sending..." : "Send OTP"}
                              </button>

                              <button
                                type="button"
                                onClick={() => {
                                  setShowChangePassword(false);
                                  setOtpRequested(false);
                                  setCurrentPassword("");
                                  setNewPassword("");
                                  setOtp("");
                                  setPasswordError("");
                                  setPasswordSuccess("");
                                }}
                                className="flex-1 rounded-[16px] border border-white/15 bg-white/5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5"
                              >
                                Cancel
                              </button>
                            </div>
                          ) : (
                            <>
                              <input
                                type="text"
                                value={otp}
                                inputMode="numeric"
                                maxLength={6}
                                placeholder="Enter the 6-digit OTP"
                                className="w-full rounded-[16px] border border-white/10 bg-white/5 px-4 py-3 text-white outline-none transition placeholder:text-slate-400 focus:border-amber-300 focus:ring-4 focus:ring-amber-200/10"
                                onChange={(e) => setOtp(e.target.value.replace(/\D/g, "").slice(0, 6))}
                              />

                              <div className="flex gap-3">
                                <button
                                  type="button"
                                  onClick={handleConfirmResetPasswordOtp}
                                  disabled={passwordLoading}
                                  className="flex-1 rounded-[16px] bg-white py-3 text-sm font-semibold text-slate-950 transition hover:-translate-y-0.5 disabled:cursor-not-allowed disabled:opacity-50"
                                >
                                  {passwordLoading ? "Verifying..." : "Confirm OTP"}
                                </button>

                                <button
                                  type="button"
                                  onClick={() => {
                                    setOtpRequested(false);
                                    setOtp("");
                                    setPasswordError("");
                                    setPasswordSuccess("");
                                  }}
                                  className="flex-1 rounded-[16px] border border-white/15 bg-white/5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5"
                                >
                                  Back
                                </button>
                              </div>
                            </>
                          )}
                        </div>
                      )}
                    </div>
                  </div>
                )}
              </div>
            </div>
          </section>

          <section className="rounded-[28px] border border-slate-200 bg-white/90 p-5 shadow-[0_20px_80px_rgba(15,23,42,0.08)] backdrop-blur-xl">
            <div className="flex items-center justify-between gap-3">
              <div>
                <p className="text-xs font-semibold uppercase tracking-[0.24em] text-slate-400">
                  History
                </p>
                <h3 className="mt-2 text-2xl font-black text-slate-900">Profile Update History</h3>
                <p className="mt-1 text-sm text-slate-500">
                  Recent changes saved whenever your profile information is updated.
                </p>
              </div>
            </div>

            <div className="mt-5 overflow-x-auto rounded-[22px] border border-slate-200">
              {historyLoading ? (
                <div className="px-4 py-6 text-sm text-slate-500">Loading history...</div>
              ) : history.length === 0 ? (
                <div className="px-4 py-6 text-sm text-slate-500">No profile updates have been recorded yet.</div>
              ) : (
                <table className="w-full min-w-[920px] table-fixed border-collapse text-left">
                  <colgroup>
                    <col className="w-[16%]" />
                    <col className="w-[26%]" />
                    <col className="w-[26%]" />
                    <col className="w-[18%]" />
                    <col className="w-[14%]" />
                  </colgroup>
                  <thead className="bg-slate-100 text-xs font-semibold uppercase tracking-[0.18em] text-slate-500">
                    <tr>
                      <th className="px-4 py-3">Field</th>
                      <th className="px-4 py-3">Previous Value</th>
                      <th className="px-4 py-3">Updated Value</th>
                      <th className="px-4 py-3">Email</th>
                      <th className="px-4 py-3">Updated At</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-200 text-sm text-slate-700">
                    {history.flatMap((item) => {
                      const rows = [
                        {
                          key: `${item.id}-fullName`,
                          label: "Full Name",
                          previous: item.previousFullName || "-",
                          updated: item.updatedFullName || "-",
                        },
                        {
                          key: `${item.id}-address`,
                          label: "Address",
                          previous: item.previousAddress || "-",
                          updated: item.updatedAddress || "-",
                        },
                        {
                          key: `${item.id}-image`,
                          label: "Image (Avatar)",
                          previous: item.previousImage || "-",
                          updated: item.updatedImage || "-",
                        },
                        {
                          key: `${item.id}-cccd`,
                          label: "CCCD",
                          previous: item.previousCCCD || "-",
                          updated: item.updatedCCCD || "-",
                        },
                      ].filter((row) => row.previous !== row.updated);

                      return rows.map((row) => (
                        <tr key={row.key} className="align-top">
                          <td className="px-4 py-4 font-semibold text-slate-900">
                            {row.label}
                          </td>
                          <td className="px-4 py-4">
                            <span className="block truncate" title={row.previous}>
                              {row.previous}
                            </span>
                          </td>
                          <td className="px-4 py-4">
                            <span className="block truncate" title={row.updated}>
                              {row.updated}
                            </span>
                          </td>
                          <td className="px-4 py-4">
                            <span className="block truncate" title={item.email}>
                              {item.email}
                            </span>
                          </td>
                          <td className="px-4 py-4 whitespace-nowrap">
                            {new Date(item.updatedAt).toLocaleString()}
                          </td>
                        </tr>
                      ));
                    })}
                  </tbody>
                </table>
              )}
            </div>
          </section>
        </main>
      </div>
    </div>
  );
}
