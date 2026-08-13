import React, { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import api from "../../../core/api/api";

const emptyEditForm = {
  email: "",
  fullName: "",
  shopName: "",
  address: "",
  cccd: "",
};

export default function UserManagement() {
  const role = localStorage.getItem("role") || "Guest";
  const isAdmin = role === "Admin";
  const [users, setUsers] = useState([]);
  const [profileRequests, setProfileRequests] = useState([]);
  const [activeTab, setActiveTab] = useState("accounts");
  const [searchTerm, setSearchTerm] = useState("");
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [selectedUser, setSelectedUser] = useState(null);
  const [editingUser, setEditingUser] = useState(null);
  const [editForm, setEditForm] = useState(emptyEditForm);
  const [debugOtp, setDebugOtp] = useState(null);
  const [debugOtpLoading, setDebugOtpLoading] = useState(false);

  const loadUsers = async () => {
    const res = await api.get("/Auth/users");
    setUsers(res || []);
  };

  const loadProfileRequests = async () => {
    const res = await api.get("/Auth/profile-update-requests?status=Pending");
    setProfileRequests(res || []);
  };

  const reloadAll = async () => {
    await Promise.all([loadUsers(), loadProfileRequests()]);
  };

  useEffect(() => {
    if (!isAdmin) {
      setLoading(false);
      return;
    }

    const run = async () => {
      try {
        setLoading(true);
        setError("");
        await reloadAll();
      } catch (err) {
        setError(err?.message || "Unable to load user management data.");
      } finally {
        setLoading(false);
      }
    };

    run();
  }, [isAdmin]);

  const visibleUsers = useMemo(() => {
    const source = activeTab === "inactive"
      ? users.filter((user) => user.isActive === false)
      : users.filter((user) => user.isActive !== false);
    const keyword = searchTerm.trim().toLowerCase();

    if (!keyword) {
      return source;
    }

    return source.filter((user) =>
      [user.email, user.role, user.fullName, user.shopName, user.cccd]
        .filter(Boolean)
        .some((value) => String(value).toLowerCase().includes(keyword)),
    );
  }, [activeTab, searchTerm, users]);

  const openEdit = (user) => {
    setEditingUser(user);
    setEditForm({
      email: user.email || "",
      fullName: user.fullName || "",
      shopName: user.shopName || "",
      address: user.address || "",
      cccd: user.cccd || "",
    });
  };

  const handleRevealInitialOtp = async (user) => {
    try {
      setDebugOtpLoading(true);
      setError("");
      setSuccess("");
      const res = await api.post("/Auth/debug/otp", {
        userId: user.id,
        regenerateInitialPasswordOtp: true,
      });
      setDebugOtp(res || null);
      setSuccess("Initial password OTP regenerated successfully.");
    } catch (err) {
      setDebugOtp(null);
      setError(err?.message || "Unable to reveal initial password OTP.");
    } finally {
      setDebugOtpLoading(false);
    }
  };

  const handleUpdateUser = async () => {
    try {
      setSaving(true);
      setError("");
      setSuccess("");
      await api.put(`/Auth/users/${editingUser.id}`, {
        email: editForm.email.trim(),
        fullName: editForm.fullName.trim(),
        shopName: editForm.shopName.trim(),
        address: editForm.address.trim(),
        cccd: editForm.cccd.trim(),
      });
      setEditingUser(null);
      setSuccess("User account updated successfully.");
      await loadUsers();
    } catch (err) {
      setError(err?.message || "Unable to update user account.");
    } finally {
      setSaving(false);
    }
  };

  const handleDeactivate = async (user) => {
    try {
      setSaving(true);
      setError("");
      setSuccess("");
      await api.delete(`/Auth/users/${user.id}`);
      setSelectedUser(null);
      setSuccess("User account set to inactive successfully.");
      await loadUsers();
    } catch (err) {
      setError(err?.message || "Unable to set user inactive.");
    } finally {
      setSaving(false);
    }
  };

  const handleActivate = async (user) => {
    try {
      setSaving(true);
      setError("");
      setSuccess("");
      await api.post(`/Auth/users/${user.id}/activate`, {});
      setSelectedUser(null);
      setSuccess("User account activated successfully.");
      await loadUsers();
    } catch (err) {
      setError(err?.message || "Unable to activate user account.");
    } finally {
      setSaving(false);
    }
  };

  const handleResendInitialPassword = async (user) => {
    try {
      setSaving(true);
      setError("");
      setSuccess("");
      await api.post(`/Auth/users/${user.id}/resend-initial-password`, {});
      setSuccess("Password setup link resent successfully.");
      await loadUsers();
    } catch (err) {
      setError(err?.message || "Unable to resend password setup link.");
    } finally {
      setSaving(false);
    }
  };

  const handleProfileRequestDecision = async (requestId, action) => {
    try {
      setSaving(true);
      setError("");
      setSuccess("");
      await api.post(`/Auth/profile-update-requests/${requestId}/${action}`, {});
      setSuccess(action === "approve" ? "Profile update request approved successfully." : "Profile update request rejected successfully.");
      await reloadAll();
    } catch (err) {
      setError(err?.message || "Unable to review profile update request.");
    } finally {
      setSaving(false);
    }
  };

  if (!isAdmin) {
    return (
      <div className="min-h-screen bg-[linear-gradient(180deg,#fff8ef_0%,#fffdf8_42%,#f8fbff_100%)] px-4 py-6 text-slate-900 sm:px-6 lg:px-8">
        <div className="mx-auto max-w-3xl rounded-[28px] border border-slate-200 bg-white/90 p-6 shadow-[0_20px_80px_rgba(15,23,42,0.08)]">
          <h1 className="text-3xl font-black">Admin access only</h1>
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
                User Management
              </div>
              <h1 className="mt-3 text-3xl font-black tracking-tight sm:text-4xl">Registered Users</h1>
            </div>
            <div className="flex flex-wrap gap-3">
              <Link to="/register" className="inline-flex items-center justify-center rounded-full bg-amber-300 px-5 py-3 text-sm font-semibold text-slate-950 transition hover:-translate-y-0.5 hover:bg-amber-200">
                Create Manager
              </Link>
              <Link to="/admin-management" className="inline-flex items-center justify-center rounded-full bg-slate-950 px-5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5">
                Back to Admin Management
              </Link>
            </div>
          </div>
        </header>

        <main className="mt-6 flex flex-1 flex-col gap-5">
          <section className="grid gap-4 md:grid-cols-2">
            <div className="rounded-[30px] border border-slate-200 bg-slate-950 px-5 py-5 text-white shadow-[0_24px_90px_rgba(15,23,42,0.12)] sm:px-6">
              <p className="text-xs font-semibold uppercase tracking-[0.24em] text-white/55">Total</p>
              <p className="mt-2 text-4xl font-black">{users.length}</p>
            </div>
            <div className="rounded-[30px] border border-amber-200 bg-amber-100 px-5 py-5 text-slate-950 shadow-[0_24px_90px_rgba(245,158,11,0.14)] sm:px-6">
              <p className="text-xs font-semibold uppercase tracking-[0.24em] text-amber-700">Pending Approvals</p>
              <p className="mt-2 text-4xl font-black">{profileRequests.length}</p>
            </div>
          </section>

          <section className="overflow-hidden rounded-[30px] border border-slate-200 bg-white/90 shadow-[0_24px_90px_rgba(15,23,42,0.08)]">
            <div className="border-b border-slate-200 px-5 py-4 sm:px-6">
              <div className="flex flex-wrap gap-2">
                <button type="button" onClick={() => setActiveTab("accounts")} className={`rounded-full px-4 py-2 text-xs font-bold transition ${activeTab === "accounts" ? "bg-slate-950 text-white" : "bg-slate-100 text-slate-600"}`}>Account List</button>
                <button type="button" onClick={() => setActiveTab("inactive")} className={`rounded-full px-4 py-2 text-xs font-bold transition ${activeTab === "inactive" ? "bg-slate-950 text-white" : "bg-slate-100 text-slate-600"}`}>Inactive Accounts</button>
                <button type="button" onClick={() => setActiveTab("approvals")} className={`rounded-full px-4 py-2 text-xs font-bold transition ${activeTab === "approvals" ? "bg-slate-950 text-white" : "bg-slate-100 text-slate-600"}`}>Profile Approval</button>
              </div>

              {activeTab !== "approvals" && (
                <input
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  placeholder="Search by email, role, name, shop..."
                  className="mt-4 w-full rounded-full border border-slate-200 bg-white px-5 py-3 text-sm outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100 sm:max-w-md"
                />
              )}

              {success && <p className="mt-3 text-sm font-medium text-emerald-600">{success}</p>}
              {error && <p className="mt-3 text-sm font-medium text-rose-600">{error}</p>}
            </div>

            {loading ? (
              <div className="px-6 py-8 text-sm text-slate-500">Loading registered users...</div>
            ) : activeTab === "approvals" ? (
              <div className="space-y-3 bg-amber-50/40 px-5 py-4 sm:px-6">
                {profileRequests.length === 0 ? (
                  <p className="text-sm text-slate-500">No manager profile updates are waiting for approval.</p>
                ) : (
                  profileRequests.map((request) => (
                    <div key={request.id} className="rounded-[22px] border border-amber-100 bg-white p-4">
                      <div className="flex flex-col gap-3 lg:flex-row lg:items-start lg:justify-between">
                        <div className="min-w-0 flex-1">
                          <p className="break-all text-sm font-bold text-slate-950">{request.email}</p>
                          <div className="mt-3 grid gap-2 text-sm md:grid-cols-3">
                            <div className="rounded-[16px] bg-slate-50 p-3"><p className="text-xs font-bold uppercase tracking-[0.16em] text-slate-400">Full Name</p><p className="mt-1 line-through decoration-rose-300">{request.currentFullName || "-"}</p><p className="mt-1 font-semibold text-slate-950">{request.requestedFullName || "-"}</p></div>
                            <div className="rounded-[16px] bg-slate-50 p-3"><p className="text-xs font-bold uppercase tracking-[0.16em] text-slate-400">Address</p><p className="mt-1 line-through decoration-rose-300">{request.currentAddress || "-"}</p><p className="mt-1 font-semibold text-slate-950">{request.requestedAddress || "-"}</p></div>
                            <div className="rounded-[16px] bg-slate-50 p-3"><p className="text-xs font-bold uppercase tracking-[0.16em] text-slate-400">CCCD</p><p className="mt-1 line-through decoration-rose-300">{request.currentCCCD || "-"}</p><p className="mt-1 font-semibold text-slate-950">{request.requestedCCCD || "-"}</p></div>
                          </div>
                        </div>
                        <div className="flex gap-2">
                          <button type="button" disabled={saving} onClick={() => handleProfileRequestDecision(request.id, "approve")} className="rounded-full bg-emerald-500 px-4 py-2 text-xs font-semibold text-white transition hover:bg-emerald-600 disabled:opacity-50">Approve</button>
                          <button type="button" disabled={saving} onClick={() => handleProfileRequestDecision(request.id, "reject")} className="rounded-full bg-rose-500 px-4 py-2 text-xs font-semibold text-white transition hover:bg-rose-600 disabled:opacity-50">Reject</button>
                        </div>
                      </div>
                    </div>
                  ))
                )}
              </div>
            ) : visibleUsers.length === 0 ? (
              <div className="px-6 py-8 text-sm text-slate-500">No users found.</div>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full min-w-[1120px] table-auto border-collapse text-left">
                  <colgroup>
                    <col className="w-[32%]" />
                    <col className="w-[14%]" />
                    <col className="w-[24%]" />
                    <col className="w-[20%]" />
                    <col className="w-[10%]" />
                  </colgroup>
                  <thead className="bg-slate-100 text-xs font-semibold uppercase tracking-[0.16em] text-slate-500">
                    <tr>
                      <th className="px-4 py-3">Email</th>
                      <th className="px-4 py-3">Role</th>
                      <th className="px-4 py-3">Full Name</th>
                      <th className="px-4 py-3">Shop</th>
                      <th className="px-4 py-3">Action</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-200 text-sm text-slate-700">
                    {visibleUsers.map((user) => (
                      <tr key={user.id} className="transition hover:bg-amber-50/60">
                        <td className="break-all px-4 py-4 font-semibold text-slate-950">{user.email}</td>
                        <td className="whitespace-nowrap px-4 py-4">{user.role}</td>
                        <td className="px-4 py-4">{user.fullName || "-"}</td>
                        <td className="px-4 py-4">{user.shopName || "-"}</td>
                        <td className="whitespace-nowrap px-4 py-4">
                          <button type="button" onClick={() => { setSelectedUser(user); setDebugOtp(null); }} className="rounded-full bg-slate-950 px-4 py-2 text-xs font-semibold text-white transition hover:-translate-y-0.5">View</button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </section>
        </main>
      </div>

      {selectedUser && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-950/70 px-4 py-6 backdrop-blur-sm">
          <div className="w-full max-w-2xl rounded-[30px] bg-white p-5 shadow-[0_30px_120px_rgba(15,23,42,0.3)]">
            <div className="flex items-center justify-between">
              <h3 className="text-xl font-black text-slate-950">{selectedUser.email}</h3>
              <button type="button" onClick={() => { setSelectedUser(null); setDebugOtp(null); }} className="rounded-full bg-slate-950 px-4 py-2 text-sm font-semibold text-white">Close</button>
            </div>

            <div className="mt-4 grid gap-3 sm:grid-cols-2">
              <div className="rounded-[18px] bg-slate-50 px-4 py-3"><p className="text-xs font-semibold uppercase tracking-[0.18em] text-slate-400">Role</p><p className="mt-1 font-bold text-slate-950">{selectedUser.role}</p></div>
              <div className="rounded-[18px] bg-slate-50 px-4 py-3"><p className="text-xs font-semibold uppercase tracking-[0.18em] text-slate-400">Shop</p><p className="mt-1 font-bold text-slate-950">{selectedUser.shopName || "-"}</p></div>
              <div className="rounded-[18px] bg-slate-50 px-4 py-3"><p className="text-xs font-semibold uppercase tracking-[0.18em] text-slate-400">CCCD</p><p className="mt-1 font-bold text-slate-950">{selectedUser.cccd || "-"}</p></div>
              <div className="rounded-[18px] bg-slate-50 px-4 py-3"><p className="text-xs font-semibold uppercase tracking-[0.18em] text-slate-400">Status</p><p className="mt-1 font-bold text-slate-950">{selectedUser.isActive === false ? "Inactive" : "Active"}</p></div>
            </div>

            {selectedUser.role !== "Admin" && (
              <div className="mt-5 flex flex-wrap gap-3">
                {selectedUser.isActive === false ? (
                  <button type="button" disabled={saving} onClick={() => handleActivate(selectedUser)} className="rounded-full bg-emerald-500 px-5 py-3 text-sm font-semibold text-white transition hover:bg-emerald-600 disabled:opacity-50">Activate Account</button>
                ) : (
                  <>
                    <button type="button" onClick={() => openEdit(selectedUser)} className="rounded-full bg-slate-950 px-5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5">Update Account</button>
                    {selectedUser.mustChangePassword && (
                      <>
                        <button type="button" disabled={saving} onClick={() => handleResendInitialPassword(selectedUser)} className="rounded-full bg-amber-400 px-5 py-3 text-sm font-semibold text-slate-950 transition hover:bg-amber-300 disabled:opacity-50">Resend Password Link</button>
                        <button type="button" disabled={debugOtpLoading} onClick={() => handleRevealInitialOtp(selectedUser)} className="rounded-full bg-sky-100 px-5 py-3 text-sm font-semibold text-sky-950 transition hover:bg-sky-200 disabled:opacity-50">
                          {debugOtpLoading ? "Revealing OTP..." : "Reveal New Initial OTP"}
                        </button>
                      </>
                    )}
                    <button type="button" disabled={saving} onClick={() => handleDeactivate(selectedUser)} className="rounded-full bg-rose-500 px-5 py-3 text-sm font-semibold text-white transition hover:bg-rose-600 disabled:opacity-50">Inactive Account</button>
                  </>
                )}
              </div>
            )}

            {debugOtp && (
              <div className="mt-5 rounded-[22px] border border-sky-200 bg-sky-50 px-4 py-4">
                <p className="text-xs font-bold uppercase tracking-[0.18em] text-sky-700">Development Debug OTP</p>
                <div className="mt-3 grid gap-3 sm:grid-cols-2">
                  <div className="rounded-[16px] bg-white px-4 py-3">
                    <p className="text-xs font-semibold uppercase tracking-[0.16em] text-slate-400">Initial OTP</p>
                    <p className="mt-1 break-all font-mono text-sm font-bold text-slate-950">{debugOtp.initialPasswordOtp || "-"}</p>
                  </div>
                  <div className="rounded-[16px] bg-white px-4 py-3">
                    <p className="text-xs font-semibold uppercase tracking-[0.16em] text-slate-400">Expires At</p>
                    <p className="mt-1 break-all text-sm font-bold text-slate-950">{debugOtp.initialPasswordExpiresAt || "-"}</p>
                  </div>
                  <div className="rounded-[16px] bg-white px-4 py-3">
                    <p className="text-xs font-semibold uppercase tracking-[0.16em] text-slate-400">Password Setup Token</p>
                    <p className="mt-1 break-all font-mono text-sm font-bold text-slate-950">{debugOtp.passwordSetupToken || "-"}</p>
                  </div>
                  <div className="rounded-[16px] bg-white px-4 py-3">
                    <p className="text-xs font-semibold uppercase tracking-[0.16em] text-slate-400">Change Password URL</p>
                    <p className="mt-1 break-all text-sm font-bold text-slate-950">{debugOtp.changePasswordUrl || "-"}</p>
                  </div>
                </div>
              </div>
            )}
          </div>
        </div>
      )}

      {editingUser && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-950/70 px-4 py-6 backdrop-blur-sm">
          <div className="w-full max-w-2xl rounded-[30px] bg-white p-5 shadow-[0_30px_120px_rgba(15,23,42,0.3)]">
            <div className="flex items-center justify-between">
              <h3 className="text-xl font-black text-slate-950">Update Account</h3>
              <button type="button" onClick={() => setEditingUser(null)} className="rounded-full bg-slate-950 px-4 py-2 text-sm font-semibold text-white">Close</button>
            </div>
            <div className="mt-4 grid gap-4 sm:grid-cols-2">
              <label className="block text-sm font-semibold text-slate-700">Email<input value={editForm.email} onChange={(e) => setEditForm((current) => ({ ...current, email: e.target.value }))} className="mt-2 w-full rounded-[16px] border border-slate-200 px-4 py-3 text-sm outline-none" /></label>
              <label className="block text-sm font-semibold text-slate-700">Full Name<input value={editForm.fullName} onChange={(e) => setEditForm((current) => ({ ...current, fullName: e.target.value }))} className="mt-2 w-full rounded-[16px] border border-slate-200 px-4 py-3 text-sm outline-none" /></label>
              <label className="block text-sm font-semibold text-slate-700">Shop Name<input value={editForm.shopName} onChange={(e) => setEditForm((current) => ({ ...current, shopName: e.target.value }))} className="mt-2 w-full rounded-[16px] border border-slate-200 px-4 py-3 text-sm outline-none" /></label>
              <label className="block text-sm font-semibold text-slate-700">CCCD<input value={editForm.cccd} onChange={(e) => setEditForm((current) => ({ ...current, cccd: e.target.value }))} className="mt-2 w-full rounded-[16px] border border-slate-200 px-4 py-3 text-sm outline-none" /></label>
              <label className="block text-sm font-semibold text-slate-700 sm:col-span-2">Address<input value={editForm.address} onChange={(e) => setEditForm((current) => ({ ...current, address: e.target.value }))} className="mt-2 w-full rounded-[16px] border border-slate-200 px-4 py-3 text-sm outline-none" /></label>
            </div>
            <div className="mt-5 flex justify-end">
              <button type="button" disabled={saving} onClick={handleUpdateUser} className="rounded-full bg-slate-950 px-5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5 disabled:opacity-50">Save Changes</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
