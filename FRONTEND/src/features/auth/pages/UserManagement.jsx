import { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import api from "../../../core/api/api";

const formatDate = (value) => {
  if (!value) {
    return "-";
  }

  const date = new Date(value);
  return Number.isNaN(date.getTime())
    ? "-"
    : new Intl.DateTimeFormat("en-GB", {
        day: "2-digit",
        month: "short",
        year: "numeric",
      }).format(date);
};

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
  const [searchTerm, setSearchTerm] = useState("");
  const [loading, setLoading] = useState(isAdmin);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [editingUser, setEditingUser] = useState(null);
  const [deletingUser, setDeletingUser] = useState(null);
  const [editForm, setEditForm] = useState(emptyEditForm);

  const loadUsers = async () => {
    try {
      setLoading(true);
      setError("");
      const res = await api.get("/Auth/users");
      setUsers(res.data || []);
    } catch (err) {
      setError(err.response?.data || "Unable to load registered users.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (!isAdmin) {
      return;
    }

    loadUsers();
  }, [isAdmin]);

  const filteredUsers = useMemo(() => {
    const keyword = searchTerm.trim().toLowerCase();

    if (!keyword) {
      return users;
    }

    return users.filter((user) =>
      [
        user.email,
        user.role,
        user.fullName,
        user.shopName,
        user.address,
        user.cccd,
      ]
        .filter(Boolean)
        .some((value) => String(value).toLowerCase().includes(keyword))
    );
  }, [searchTerm, users]);

  const stats = useMemo(
    () =>
      users.reduce(
        (result, user) => ({
          total: result.total + 1,
          admin: result.admin + (user.role === "Admin" ? 1 : 0),
          manager: result.manager + (user.role === "Manager" ? 1 : 0),
        }),
        { total: 0, admin: 0, manager: 0 }
      ),
    [users]
  );

  const openEditModal = (user) => {
    setError("");
    setSuccess("");
    setEditingUser(user);
    setEditForm({
      email: user.email || "",
      fullName: user.fullName || "",
      shopName: user.shopName || "",
      address: user.address || "",
      cccd: user.cccd || "",
    });
  };

  const closeEditModal = () => {
    setEditingUser(null);
    setEditForm(emptyEditForm);
  };

  const handleEditChange = (field, value) => {
    setEditForm((current) => ({
      ...current,
      [field]: value,
    }));
  };

  const handleUpdateUser = async () => {
    const requiredFields = [
      editForm.email,
      editForm.fullName,
      editForm.shopName,
      editForm.cccd,
    ];

    if (requiredFields.some((value) => !String(value).trim())) {
      setError("Please complete email, full name, shop name, and CCCD.");
      return;
    }

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

      closeEditModal();
      setSuccess("User account updated successfully.");
      await loadUsers();
    } catch (err) {
      setError(err.response?.data || "Unable to update user account.");
    } finally {
      setSaving(false);
    }
  };

  const handleDeleteUser = async () => {
    try {
      setSaving(true);
      setError("");
      setSuccess("");

      await api.delete(`/Auth/users/${deletingUser.id}`);

      setDeletingUser(null);
      setSuccess("User account deleted successfully.");
      await loadUsers();
    } catch (err) {
      setError(err.response?.data || "Unable to delete user account.");
    } finally {
      setSaving(false);
    }
  };

  if (!isAdmin) {
    return (
      <div className="min-h-screen bg-[linear-gradient(180deg,#fff8ef_0%,#fffdf8_42%,#f8fbff_100%)] px-4 py-6 text-slate-900 sm:px-6 lg:px-8">
        <div className="mx-auto max-w-3xl rounded-[28px] border border-slate-200 bg-white/90 p-6 shadow-[0_20px_80px_rgba(15,23,42,0.08)]">
          <p className="text-xs font-semibold uppercase tracking-[0.28em] text-amber-700">
            User Management
          </p>
          <h1 className="mt-3 text-3xl font-black">Admin access only</h1>
          <p className="mt-3 text-sm text-slate-600">
            Registered user management is only available for admin accounts.
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
                User Management
              </div>
              <h1 className="mt-3 text-3xl font-black tracking-tight sm:text-4xl">
                Registered Users
              </h1>
              <p className="mt-2 max-w-2xl text-sm text-slate-600 sm:text-base">
                Review admin and manager accounts that have been created in the system.
              </p>
            </div>

            <div className="flex flex-wrap gap-3">
              <Link
                to="/register"
                className="inline-flex items-center justify-center rounded-full bg-amber-300 px-5 py-3 text-sm font-semibold text-slate-950 transition hover:-translate-y-0.5 hover:bg-amber-200"
              >
                Create Manager
              </Link>
              <Link
                to="/admin-management"
                className="inline-flex items-center justify-center rounded-full bg-slate-950 px-5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5"
              >
                Back to Admin Management
              </Link>
            </div>
          </div>
        </header>

        <main className="mt-6 grid flex-1 gap-5 lg:grid-cols-[280px_1fr]">
          <aside className="rounded-[30px] border border-slate-200 bg-white/90 p-4 shadow-[0_24px_90px_rgba(15,23,42,0.08)] backdrop-blur-xl">
            <p className="px-2 text-xs font-semibold uppercase tracking-[0.24em] text-slate-400">
              User Summary
            </p>
            <div className="mt-4 grid gap-3">
              <div className="rounded-[24px] bg-slate-950 px-5 py-5 text-white">
                <p className="text-xs font-semibold uppercase tracking-[0.24em] text-white/55">Total</p>
                <p className="mt-2 text-4xl font-black">{stats.total}</p>
              </div>
              <div className="rounded-[24px] bg-amber-100 px-5 py-5 text-slate-950">
                <p className="text-xs font-semibold uppercase tracking-[0.24em] text-amber-700">Managers</p>
                <p className="mt-2 text-3xl font-black">{stats.manager}</p>
              </div>
              <div className="rounded-[24px] bg-slate-100 px-5 py-5 text-slate-950">
                <p className="text-xs font-semibold uppercase tracking-[0.24em] text-slate-500">Admins</p>
                <p className="mt-2 text-3xl font-black">{stats.admin}</p>
              </div>
            </div>
          </aside>

          <section className="overflow-hidden rounded-[30px] border border-slate-200 bg-white/90 shadow-[0_24px_90px_rgba(15,23,42,0.08)] backdrop-blur-xl">
            <div className="flex flex-col gap-3 border-b border-slate-200 px-5 py-4 sm:flex-row sm:items-center sm:justify-between sm:px-6">
              <div>
                <p className="text-xs font-semibold uppercase tracking-[0.24em] text-slate-400">
                  Directory
                </p>
                <h2 className="mt-2 text-2xl font-black text-slate-950">Account List</h2>
                {success && <p className="mt-2 text-sm font-medium text-emerald-600">{success}</p>}
                {error && !editingUser && !deletingUser && <p className="mt-2 text-sm font-medium text-rose-600">{error}</p>}
              </div>
              <input
                value={searchTerm}
                placeholder="Search by email, name, shop, role, CCCD..."
                className="w-full rounded-full border border-slate-200 bg-white px-5 py-3 text-sm outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100 sm:max-w-md"
                onChange={(e) => setSearchTerm(e.target.value)}
              />
            </div>

            {loading ? (
              <div className="px-6 py-8 text-sm text-slate-500">Loading registered users...</div>
            ) : error ? (
              <div className="px-6 py-8 text-sm font-medium text-rose-600">{error}</div>
            ) : filteredUsers.length === 0 ? (
              <div className="px-6 py-8 text-sm text-slate-500">No users found.</div>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full min-w-[1320px] table-fixed border-collapse text-left">
                  <colgroup>
                    <col className="w-[16%]" />
                    <col className="w-[8%]" />
                    <col className="w-[13%]" />
                    <col className="w-[13%]" />
                    <col className="w-[11%]" />
                    <col className="w-[13%]" />
                    <col className="w-[7%]" />
                    <col className="w-[9%]" />
                    <col className="w-[10%]" />
                  </colgroup>
                  <thead className="bg-slate-100 text-xs font-semibold uppercase tracking-[0.16em] text-slate-500">
                    <tr>
                      <th className="px-4 py-3">Email</th>
                      <th className="px-4 py-3">Role</th>
                      <th className="px-4 py-3">Full Name</th>
                      <th className="px-4 py-3">Shop</th>
                      <th className="px-4 py-3">CCCD</th>
                      <th className="px-4 py-3">Address</th>
                      <th className="px-4 py-3">Failed</th>
                      <th className="px-4 py-3">Created</th>
                      <th className="px-4 py-3">Action</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-200 text-sm text-slate-700">
                    {filteredUsers.map((user) => (
                      <tr key={user.id} className="align-top transition hover:bg-amber-50/60">
                        <td className="px-4 py-4 font-semibold text-slate-950">{user.email}</td>
                        <td className="px-4 py-4">
                          <span className={`rounded-full px-3 py-1 text-xs font-bold ${user.role === "Admin" ? "bg-slate-950 text-white" : "bg-amber-100 text-amber-800"}`}>
                            {user.role}
                          </span>
                        </td>
                        <td className="px-4 py-4">{user.fullName || "-"}</td>
                        <td className="px-4 py-4">{user.shopName || "-"}</td>
                        <td className="px-4 py-4">{user.cccd || "-"}</td>
                        <td className="px-4 py-4">{user.address || "-"}</td>
                        <td className="px-4 py-4">{user.failedLoginAttempts || 0}</td>
                        <td className="px-4 py-4">{formatDate(user.createdAt)}</td>
                        <td className="px-4 py-4">
                          {user.role === "Admin" ? (
                            <span className="text-xs font-semibold text-slate-400">No action</span>
                          ) : (
                            <div className="flex items-center gap-2 whitespace-nowrap">
                              <button
                                type="button"
                                className="rounded-full bg-slate-950 px-3 py-2 text-xs font-semibold text-white transition hover:-translate-y-0.5"
                                onClick={() => openEditModal(user)}
                              >
                                Update
                              </button>
                              <button
                                type="button"
                                className="rounded-full bg-rose-500 px-3 py-2 text-xs font-semibold text-white transition hover:-translate-y-0.5 hover:bg-rose-600"
                                onClick={() => setDeletingUser(user)}
                              >
                                Delete
                              </button>
                            </div>
                          )}
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

      {editingUser && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-950/70 px-4 py-6 backdrop-blur-sm">
          <div className="w-full max-w-3xl overflow-hidden rounded-[30px] bg-white shadow-[0_30px_120px_rgba(15,23,42,0.3)]">
            <div className="flex items-center justify-between border-b border-slate-200 px-5 py-4">
              <div>
                <p className="text-xs font-semibold uppercase tracking-[0.24em] text-amber-700">Update Account</p>
                <h3 className="mt-1 text-xl font-black text-slate-950">{editingUser.email}</h3>
              </div>
              <button type="button" onClick={closeEditModal} className="rounded-full bg-slate-950 px-4 py-2 text-sm font-semibold text-white">
                Close
              </button>
            </div>

            <div className="grid gap-4 p-5 sm:grid-cols-2">
              <label className="block">
                <span className="mb-2 block text-sm font-semibold text-slate-700">Email</span>
                <input value={editForm.email} className="w-full rounded-[16px] border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100" onChange={(e) => handleEditChange("email", e.target.value)} />
              </label>
              <label className="block">
                <span className="mb-2 block text-sm font-semibold text-slate-700">Full Name</span>
                <input value={editForm.fullName} className="w-full rounded-[16px] border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100" onChange={(e) => handleEditChange("fullName", e.target.value)} />
              </label>
              <label className="block">
                <span className="mb-2 block text-sm font-semibold text-slate-700">Shop Name</span>
                <input value={editForm.shopName} className="w-full rounded-[16px] border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100" onChange={(e) => handleEditChange("shopName", e.target.value)} />
              </label>
              <label className="block">
                <span className="mb-2 block text-sm font-semibold text-slate-700">CCCD</span>
                <input value={editForm.cccd} className="w-full rounded-[16px] border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100" onChange={(e) => handleEditChange("cccd", e.target.value)} />
              </label>
              <label className="block sm:col-span-2">
                <span className="mb-2 block text-sm font-semibold text-slate-700">Address</span>
                <input value={editForm.address} className="w-full rounded-[16px] border border-slate-200 px-4 py-3 text-sm outline-none transition focus:border-amber-400 focus:ring-4 focus:ring-amber-100" onChange={(e) => handleEditChange("address", e.target.value)} />
              </label>
            </div>

            <div className="flex flex-col gap-3 border-t border-slate-200 px-5 py-4 sm:flex-row sm:items-center sm:justify-between">
              <div className="space-y-1">
                {error && <p className="text-sm font-medium text-rose-600">{error}</p>}
                {success && <p className="text-sm font-medium text-emerald-600">{success}</p>}
              </div>
              <button type="button" disabled={saving} onClick={handleUpdateUser} className="rounded-full bg-slate-950 px-5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5 disabled:cursor-not-allowed disabled:opacity-50">
                {saving ? "Saving..." : "Save Changes"}
              </button>
            </div>
          </div>
        </div>
      )}

      {deletingUser && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-950/70 px-4 py-6 backdrop-blur-sm">
          <div className="w-full max-w-md rounded-[30px] bg-white p-5 shadow-[0_30px_120px_rgba(15,23,42,0.3)]">
            <p className="text-xs font-semibold uppercase tracking-[0.24em] text-rose-600">Delete Account</p>
            <h3 className="mt-2 text-2xl font-black text-slate-950">{deletingUser.email}</h3>
            <p className="mt-3 text-sm leading-6 text-slate-600">
              This will delete the manager account and revoke related auth records. Admin accounts cannot be deleted here.
            </p>
            {error && <p className="mt-4 text-sm font-medium text-rose-600">{error}</p>}
            <div className="mt-6 flex flex-col gap-3 sm:flex-row sm:justify-end">
              <button type="button" disabled={saving} onClick={() => setDeletingUser(null)} className="rounded-full border border-slate-200 px-5 py-3 text-sm font-semibold text-slate-700 transition hover:bg-slate-100 disabled:cursor-not-allowed disabled:opacity-50">
                Cancel
              </button>
              <button type="button" disabled={saving} onClick={handleDeleteUser} className="rounded-full bg-rose-500 px-5 py-3 text-sm font-semibold text-white transition hover:bg-rose-600 disabled:cursor-not-allowed disabled:opacity-50">
                {saving ? "Deleting..." : "Delete Account"}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
