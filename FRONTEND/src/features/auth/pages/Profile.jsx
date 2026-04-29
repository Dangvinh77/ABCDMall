import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import api from "../../../core/api/api";
import { logoutUser } from "../services/auth";

const initialProfileForm = {
  fullName: "",
  address: "",
  cccd: "",
};

const API_ORIGIN = "http://localhost:5184";

const formatUtcDateTime = (value) => {
  if (!value) {
    return "-";
  }

  const text = String(value);
  const normalized = /Z$|[+-]\d{2}:\d{2}$/.test(text) ? text : `${text}Z`;
  const date = new Date(normalized);

  return Number.isNaN(date.getTime()) ? "-" : date.toLocaleString();
};

export default function Profile() {
  const [user, setUser] = useState(null);
  const [profileForm, setProfileForm] = useState(initialProfileForm);
  const [history, setHistory] = useState([]);
  const [pendingRequests, setPendingRequests] = useState([]);
  const [profileLoading, setProfileLoading] = useState(true);
  const [historyLoading, setHistoryLoading] = useState(true);
  const [pendingRequestsLoading, setPendingRequestsLoading] = useState(true);
  const [profileSaving, setProfileSaving] = useState(false);
  const [profileError, setProfileError] = useState("");
  const [profileSuccess, setProfileSuccess] = useState("");
  const [showEditProfile, setShowEditProfile] = useState(false);
  const [cccdFrontImageFile, setCccdFrontImageFile] = useState(null);
  const [cccdBackImageFile, setCccdBackImageFile] = useState(null);
  const [currentPassword, setCurrentPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [passwordLoading, setPasswordLoading] = useState(false);
  const [passwordError, setPasswordError] = useState("");
  const [passwordSuccess, setPasswordSuccess] = useState("");
  const [showChangePassword, setShowChangePassword] = useState(false);
  const [historyFieldFilter, setHistoryFieldFilter] = useState("all");
  const navigate = useNavigate();

  const syncProfileForm = (profile) => {
    setProfileForm({
      fullName: profile?.fullName || "",
      address: profile?.address || "",
      cccd: profile?.cccd || "",
    });
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

  const loadPendingRequests = async () => {
    const res = await api.get("/Auth/profile-update-requests/me?status=Pending");
    setPendingRequests(res.data || []);
  };

  useEffect(() => {
    const loadData = async () => {
      try {
        setProfileLoading(true);
        setHistoryLoading(true);
        setPendingRequestsLoading(true);
        await Promise.all([loadProfile(), loadHistory(), loadPendingRequests()]);
      } catch {
        alert("Unauthorized");
      } finally {
        setProfileLoading(false);
        setHistoryLoading(false);
        setPendingRequestsLoading(false);
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

      if (cccdFrontImageFile) {
        formData.append("cccdFrontImage", cccdFrontImageFile);
      }

      if (cccdBackImageFile) {
        formData.append("cccdBackImage", cccdBackImageFile);
      }

      const res = await api.put("/Auth/updateprofile", formData, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      });
      setUser(res.data.profile);
      syncProfileForm(res.data.profile);
      setCccdFrontImageFile(null);
      setCccdBackImageFile(null);
      setProfileSuccess(res.data.message || "Profile update request submitted for admin approval.");
      setShowEditProfile(false);
      await Promise.all([loadHistory(), loadPendingRequests()]);
    } catch (err) {
      setProfileError(err.response?.data || "Unable to update profile.");
    } finally {
      setProfileSaving(false);
    }
  };

  const handleResetPassword = async () => {
    try {
      setPasswordLoading(true);
      setPasswordError("");
      setPasswordSuccess("");

      if (currentPassword === newPassword) {
        setPasswordError("The new password must be different from the current password.");
        return;
      }

      await api.post("/Auth/resetpassword", {
        currentPassword,
        newPassword,
      });

      setPasswordSuccess("Password reset successful.");
      setCurrentPassword("");
      setNewPassword("");
      setShowChangePassword(false);
    } catch (err) {
      setPasswordError(err.response?.data || "Unable to reset password.");
    } finally {
      setPasswordLoading(false);
    }
  };

  const handleLogout = async () => {
    await logoutUser();
    navigate("/login");
  };

  const renderHistoryValue = (value, isImage = false) => {
    if (!value) {
      return "-";
    }

    if (!isImage) {
      return (
        <span className="block truncate" title={value}>
          {value}
        </span>
      );
    }

    return (
      <a
        href={resolveAvatarUrl(value)}
        target="_blank"
        rel="noreferrer"
        className="font-semibold text-amber-700 underline-offset-4 hover:underline"
      >
        View image
      </a>
    );
  };

  const historyRows = history
    .flatMap((item) => [
      {
        key: `${item.id}-fullName`,
        fieldKey: "fullName",
        label: "Full Name",
        previous: item.previousFullName || "-",
        updated: item.updatedFullName || "-",
        email: item.email,
        updatedAt: item.updatedAt,
      },
      {
        key: `${item.id}-address`,
        fieldKey: "address",
        label: "Address",
        previous: item.previousAddress || "-",
        updated: item.updatedAddress || "-",
        email: item.email,
        updatedAt: item.updatedAt,
      },
      {
        key: `${item.id}-cccd`,
        fieldKey: "cccd",
        label: "CCCD",
        previous: item.previousCCCD || "-",
        updated: item.updatedCCCD || "-",
        email: item.email,
        updatedAt: item.updatedAt,
      },
      {
        key: `${item.id}-cccdFrontImage`,
        fieldKey: "cccdFrontImage",
        label: "CCCD Front Image",
        previous: item.previousCccdFrontImage || "",
        updated: item.updatedCccdFrontImage || "",
        email: item.email,
        updatedAt: item.updatedAt,
        isImage: true,
      },
      {
        key: `${item.id}-cccdBackImage`,
        fieldKey: "cccdBackImage",
        label: "CCCD Back Image",
        previous: item.previousCccdBackImage || "",
        updated: item.updatedCccdBackImage || "",
        email: item.email,
        updatedAt: item.updatedAt,
        isImage: true,
      },
    ])
    .filter((row) => row.previous !== row.updated)
    .filter((row) => historyFieldFilter === "all" || row.fieldKey === historyFieldFilter);

  return (
    <div className="min-h-screen bg-[linear-gradient(180deg,#fff8ef_0%,#fffdf8_42%,#f8fbff_100%)] text-slate-900">
      <div className="mx-auto flex min-h-screen w-full max-w-7xl flex-col px-4 pb-6 pt-28 sm:px-6 lg:px-8">
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
                      <p className="text-xs uppercase tracking-[0.24em] text-white/55">CCCD Images</p>
                      <div className="mt-3 grid gap-3 sm:grid-cols-2">
                        {user?.cccdFrontImage ? (
                          <a href={resolveAvatarUrl(user.cccdFrontImage)} target="_blank" rel="noreferrer" className="block overflow-hidden rounded-[16px] border border-white/10 bg-white/5">
                            <img src={resolveAvatarUrl(user.cccdFrontImage)} alt="CCCD front" className="h-28 w-full object-cover" />
                            <span className="block px-3 py-2 text-xs font-semibold text-slate-300">Front side</span>
                          </a>
                        ) : (
                          <div className="rounded-[16px] border border-white/10 bg-white/5 px-3 py-8 text-center text-xs font-semibold text-slate-400">
                            Front side not uploaded
                          </div>
                        )}

                        {user?.cccdBackImage ? (
                          <a href={resolveAvatarUrl(user.cccdBackImage)} target="_blank" rel="noreferrer" className="block overflow-hidden rounded-[16px] border border-white/10 bg-white/5">
                            <img src={resolveAvatarUrl(user.cccdBackImage)} alt="CCCD back" className="h-28 w-full object-cover" />
                            <span className="block px-3 py-2 text-xs font-semibold text-slate-300">Back side</span>
                          </a>
                        ) : (
                          <div className="rounded-[16px] border border-white/10 bg-white/5 px-3 py-8 text-center text-xs font-semibold text-slate-400">
                            Back side not uploaded
                          </div>
                        )}
                      </div>
                    </div>

                    <div className="rounded-[22px] border border-white/10 bg-black/10 px-4 py-4">
                      <button
                        type="button"
                        onClick={() => {
                          setShowEditProfile(true);
                          setProfileError("");
                          setProfileSuccess("");
                          syncProfileForm(user);
                          setCccdFrontImageFile(null);
                          setCccdBackImageFile(null);
                        }}
                        className="w-full rounded-[16px] bg-white py-3 text-sm font-semibold text-slate-950 transition hover:-translate-y-0.5"
                      >
                        Edit Profile
                      </button>
                    </div>

                    <div className="rounded-[22px] border border-white/10 bg-black/10 px-4 py-4">
                      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
                        <div>
                          <p className="text-xs uppercase tracking-[0.24em] text-white/55">Password</p>
                          <p className="mt-2 text-lg font-semibold text-white">*******</p>
                        </div>
                        <button
                          type="button"
                          onClick={() => {
                            setShowChangePassword(true);
                            setPasswordError("");
                            setPasswordSuccess("");
                          }}
                          className="rounded-[16px] bg-white px-5 py-3 text-sm font-semibold text-slate-950 transition hover:-translate-y-0.5"
                        >
                          Change Password
                        </button>
                      </div>
                    </div>
                  </div>
                )}
              </div>
            </div>
          </section>

          <section className="rounded-[28px] border border-amber-100 bg-amber-50/80 p-5 shadow-[0_20px_80px_rgba(15,23,42,0.06)] backdrop-blur-xl">
            <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
              <div>
                <p className="text-xs font-semibold uppercase tracking-[0.24em] text-amber-700">
                  Pending Approval
                </p>
                <h3 className="mt-2 text-2xl font-black text-slate-900">Profile Update Request</h3>
                <p className="mt-1 text-sm text-slate-600">
                  Profile changes will appear in history after admin approval.
                </p>
              </div>
              <span className="w-fit rounded-full bg-white px-3 py-1 text-xs font-bold text-slate-700">
                {pendingRequests.length} pending
              </span>
            </div>

            {pendingRequestsLoading ? (
              <div className="mt-5 rounded-[20px] bg-white px-4 py-5 text-sm text-slate-500">
                Loading pending requests...
              </div>
            ) : pendingRequests.length === 0 ? (
              <div className="mt-5 rounded-[20px] bg-white px-4 py-5 text-sm text-slate-500">
                No profile update request is waiting for approval.
              </div>
            ) : (
              <div className="mt-5 grid gap-3">
                {pendingRequests.map((request) => (
                  <div key={request.id} className="rounded-[22px] border border-amber-100 bg-white p-4">
                    <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
                      <div>
                        <p className="text-sm font-bold text-slate-950">{request.email}</p>
                        <p className="mt-1 text-xs text-slate-500">
                          Requested: {formatUtcDateTime(request.requestedAt)}
                        </p>
                      </div>
                      <span className="w-fit rounded-full bg-amber-100 px-3 py-1 text-xs font-bold text-amber-800">
                        {request.status}
                      </span>
                    </div>

                    <div className="mt-4 grid gap-3 text-sm sm:grid-cols-2 lg:grid-cols-3">
                      <div className="rounded-[16px] bg-slate-50 p-3">
                        <p className="text-xs font-bold uppercase tracking-[0.16em] text-slate-400">Full Name</p>
                        <p className="mt-1 line-through decoration-rose-300">{request.currentFullName || "-"}</p>
                        <p className="mt-1 font-semibold text-slate-950">{request.requestedFullName || "-"}</p>
                      </div>
                      <div className="rounded-[16px] bg-slate-50 p-3">
                        <p className="text-xs font-bold uppercase tracking-[0.16em] text-slate-400">Address</p>
                        <p className="mt-1 line-through decoration-rose-300">{request.currentAddress || "-"}</p>
                        <p className="mt-1 font-semibold text-slate-950">{request.requestedAddress || "-"}</p>
                      </div>
                      <div className="rounded-[16px] bg-slate-50 p-3">
                        <p className="text-xs font-bold uppercase tracking-[0.16em] text-slate-400">CCCD</p>
                        <p className="mt-1 line-through decoration-rose-300">{request.currentCCCD || "-"}</p>
                        <p className="mt-1 font-semibold text-slate-950">{request.requestedCCCD || "-"}</p>
                      </div>
                      <div className="rounded-[16px] bg-slate-50 p-3">
                        <p className="text-xs font-bold uppercase tracking-[0.16em] text-slate-400">CCCD Front Image</p>
                        <p className="mt-2">{renderHistoryValue(request.requestedCccdFrontImage, true)}</p>
                      </div>
                      <div className="rounded-[16px] bg-slate-50 p-3">
                        <p className="text-xs font-bold uppercase tracking-[0.16em] text-slate-400">CCCD Back Image</p>
                        <p className="mt-2">{renderHistoryValue(request.requestedCccdBackImage, true)}</p>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </section>

          <section className="rounded-[28px] border border-slate-200 bg-white/90 p-5 shadow-[0_20px_80px_rgba(15,23,42,0.08)] backdrop-blur-xl">
            <div className="flex items-center justify-between gap-3">
              <div>
                <p className="text-xs font-semibold uppercase tracking-[0.24em] text-slate-400">
                  History
                </p>
                <h3 className="mt-2 text-2xl font-black text-slate-900">Profile Update History</h3>
                <p className="mt-1 text-sm text-slate-500">
                  Recent changes saved after admin approval.
                </p>
              </div>
              <select
                value={historyFieldFilter}
                onChange={(e) => setHistoryFieldFilter(e.target.value)}
                className="rounded-full border border-slate-200 bg-white px-4 py-2 text-sm font-medium text-slate-700 outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100"
              >
                <option value="all">All fields</option>
                <option value="fullName">Full Name</option>
                <option value="address">Address</option>
                <option value="cccd">CCCD</option>
                <option value="cccdFrontImage">CCCD Front Image</option>
                <option value="cccdBackImage">CCCD Back Image</option>
              </select>
            </div>

            <div className="mt-5 overflow-x-auto rounded-[22px] border border-slate-200">
              {historyLoading ? (
                <div className="px-4 py-6 text-sm text-slate-500">Loading history...</div>
              ) : historyRows.length === 0 ? (
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
                    {historyRows.map((row) => (
                      <tr key={row.key} className="align-top">
                        <td className="px-4 py-4 font-semibold text-slate-900">
                          {row.label}
                        </td>
                        <td className="px-4 py-4">
                          {renderHistoryValue(row.previous, row.isImage)}
                        </td>
                        <td className="px-4 py-4">
                          {renderHistoryValue(row.updated, row.isImage)}
                        </td>
                        <td className="px-4 py-4">
                          <span className="block truncate" title={row.email}>
                            {row.email}
                          </span>
                        </td>
                        <td className="px-4 py-4 whitespace-nowrap">
                          {formatUtcDateTime(row.updatedAt)}
                        </td>
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
              <button
                type="button"
                onClick={() => {
                  setShowEditProfile(false);
                  setProfileError("");
                  setProfileSuccess("");
                  syncProfileForm(user);
                  setCccdFrontImageFile(null);
                  setCccdBackImageFile(null);
                }}
                className="rounded-full bg-slate-950 px-4 py-2 text-sm font-semibold text-white"
              >
                Close
              </button>
            </div>

            <div className="space-y-4 p-5">
              {profileError && (
                <div className="rounded-[16px] border border-rose-200 bg-rose-50 px-4 py-3 text-sm font-medium text-rose-600">
                  {profileError}
                </div>
              )}

              <input
                value={profileForm.fullName}
                placeholder="Full name"
                className="w-full rounded-[16px] border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100"
                onChange={(e) => handleProfileInputChange("fullName", e.target.value)}
              />

              <input
                value={profileForm.address}
                placeholder="Address"
                className="w-full rounded-[16px] border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100"
                onChange={(e) => handleProfileInputChange("address", e.target.value)}
              />

              <input
                value={profileForm.cccd}
                placeholder="CCCD"
                className="w-full rounded-[16px] border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100"
                onChange={(e) => handleProfileInputChange("cccd", e.target.value)}
              />

              <div className="grid gap-4 sm:grid-cols-2">
                <label className="rounded-[16px] border border-slate-200 bg-slate-50 px-4 py-3 text-sm text-slate-700">
                  <span className="block text-xs font-semibold uppercase tracking-[0.18em] text-slate-400">
                    CCCD Front Image
                  </span>
                  <input
                    type="file"
                    accept="image/*"
                    className="mt-2 w-full text-xs file:mr-3 file:rounded-full file:border-0 file:bg-slate-950 file:px-3 file:py-2 file:text-xs file:font-semibold file:text-white"
                    onChange={(e) => setCccdFrontImageFile(e.target.files?.[0] || null)}
                  />
                </label>

                <label className="rounded-[16px] border border-slate-200 bg-slate-50 px-4 py-3 text-sm text-slate-700">
                  <span className="block text-xs font-semibold uppercase tracking-[0.18em] text-slate-400">
                    CCCD Back Image
                  </span>
                  <input
                    type="file"
                    accept="image/*"
                    className="mt-2 w-full text-xs file:mr-3 file:rounded-full file:border-0 file:bg-slate-950 file:px-3 file:py-2 file:text-xs file:font-semibold file:text-white"
                    onChange={(e) => setCccdBackImageFile(e.target.files?.[0] || null)}
                  />
                </label>
              </div>
            </div>

            <div className="flex flex-col gap-3 border-t border-slate-200 px-5 py-4 sm:flex-row sm:justify-end">
              <button
                type="button"
                onClick={() => {
                  setShowEditProfile(false);
                  setProfileError("");
                  setProfileSuccess("");
                  syncProfileForm(user);
                  setCccdFrontImageFile(null);
                  setCccdBackImageFile(null);
                }}
                className="rounded-full border border-slate-200 px-5 py-3 text-sm font-semibold text-slate-700 transition hover:bg-slate-100"
              >
                Cancel
              </button>
              <button
                type="button"
                onClick={handleUpdateProfile}
                disabled={profileSaving}
                className="rounded-full bg-slate-950 px-5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5 disabled:cursor-not-allowed disabled:opacity-50"
              >
                {profileSaving ? "Submitting..." : "Submit Request"}
              </button>
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
                <h3 className="mt-1 text-xl font-black text-slate-950">Update account password</h3>
              </div>
              <button
                type="button"
                onClick={() => {
                  setShowChangePassword(false);
                  setCurrentPassword("");
                  setNewPassword("");
                  setPasswordError("");
                  setPasswordSuccess("");
                }}
                className="rounded-full bg-slate-950 px-4 py-2 text-sm font-semibold text-white"
              >
                Close
              </button>
            </div>

            <div className="space-y-4 p-5">
              {passwordError && (
                <div className="rounded-[16px] border border-rose-200 bg-rose-50 px-4 py-3 text-sm font-medium text-rose-600">
                  {passwordError}
                </div>
              )}

              {passwordSuccess && (
                <div className="rounded-[16px] border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm font-medium text-emerald-600">
                  {passwordSuccess}
                </div>
              )}

              <input
                type="password"
                value={currentPassword}
                placeholder="Current password"
                className="w-full rounded-[16px] border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100"
                onChange={(e) => setCurrentPassword(e.target.value)}
              />

              <input
                type="password"
                value={newPassword}
                placeholder="New password"
                className="w-full rounded-[16px] border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100"
                onChange={(e) => setNewPassword(e.target.value)}
              />
            </div>

            <div className="flex flex-col gap-3 border-t border-slate-200 px-5 py-4 sm:flex-row sm:justify-end">
              <button
                type="button"
                onClick={() => {
                  setShowChangePassword(false);
                  setCurrentPassword("");
                  setNewPassword("");
                  setPasswordError("");
                  setPasswordSuccess("");
                }}
                className="rounded-full border border-slate-200 px-5 py-3 text-sm font-semibold text-slate-700 transition hover:bg-slate-100"
              >
                Cancel
              </button>
              <button
                type="button"
                onClick={handleResetPassword}
                disabled={passwordLoading}
                className="rounded-full bg-slate-950 px-5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5 disabled:cursor-not-allowed disabled:opacity-50"
              >
                {passwordLoading ? "Saving..." : "Save Password"}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
