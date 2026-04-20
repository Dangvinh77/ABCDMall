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
  image: "",
  cccd: "",
};

const accountsPerPage = 10;

export default function UserManagement() {
  const role = localStorage.getItem("role") || "Guest";
  const isAdmin = role === "Admin";
  const [users, setUsers] = useState([]);
  const [searchTerm, setSearchTerm] = useState("");
  const [loading, setLoading] = useState(isAdmin);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [selectedViewUser, setSelectedViewUser] = useState(null);
  const [editingUser, setEditingUser] = useState(null);
  const [deletingUser, setDeletingUser] = useState(null);
  const [editForm, setEditForm] = useState(emptyEditForm);
  const [avatarFile, setAvatarFile] = useState(null);
  const [currentPage, setCurrentPage] = useState(1);

  const loadUsers = async () => {
    try {
      setLoading(true);
      setError("");
      const res = await api.get("/Auth/users");
      const activeUsers = (res.data || []).filter((user) => user.isActive !== false);
      setUsers(activeUsers);
      setSelectedViewUser((current) => (
        current ? activeUsers.find((user) => user.id === current.id) || null : null
      ));
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

  const totalPages = Math.max(1, Math.ceil(filteredUsers.length / accountsPerPage));
  const pageStartIndex = (currentPage - 1) * accountsPerPage;
  const pagedUsers = filteredUsers.slice(pageStartIndex, pageStartIndex + accountsPerPage);

  useEffect(() => {
    setCurrentPage(1);
  }, [searchTerm]);

  useEffect(() => {
    if (currentPage > totalPages) {
      setCurrentPage(totalPages);
    }
  }, [currentPage, totalPages]);

  const openViewModal = (user) => {
    setError("");
    setSuccess("");
    setSelectedViewUser(user);
  };

  const closeViewModal = () => {
    setSelectedViewUser(null);
    setError("");
  };

  const openEditModal = (user) => {
    setError("");
    setSuccess("");
    setEditingUser(user);
    setEditForm({
      email: user.email || "",
      fullName: user.fullName || "",
      shopName: user.shopName || "",
      address: user.address || "",
      image: user.image || "",
      cccd: user.cccd || "",
    });
    setAvatarFile(null);
  };

  const closeEditModal = () => {
    setEditingUser(null);
    setEditForm(emptyEditForm);
    setAvatarFile(null);
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

      const formData = new FormData();
      formData.append("email", editForm.email.trim());
      formData.append("fullName", editForm.fullName.trim());
      formData.append("shopName", editForm.shopName.trim());
      formData.append("address", editForm.address.trim());
      formData.append("image", editForm.image.trim());
      formData.append("cccd", editForm.cccd.trim());

      if (avatarFile) {
        formData.append("avatar", avatarFile);
      }

      await api.put(`/Auth/users/${editingUser.id}`, formData, {
        headers: { "Content-Type": "multipart/form-data" },
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
      setSelectedViewUser(null);
      setSuccess("User account set to inactive successfully.");
      await loadUsers();
    } catch (err) {
      setError(err.response?.data || "Unable to set user account inactive.");
    } finally {
      setSaving(false);
    }
  };

  if (!isAdmin) {
    return (
      <div className="min-h-screen bg-[linear-gradient(180deg,#fff8ef_0%,#fffdf8_42%,#f8fbff_100%)] px-4 pb-6 pt-24 text-slate-900 sm:px-6 sm:pt-28 lg:px-8">
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
      <div className="mx-auto flex min-h-screen w-full max-w-7xl flex-col px-4 pb-6 pt-24 sm:px-6 sm:pt-28 lg:px-8">
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
                {error && !selectedViewUser && !editingUser && !deletingUser && <p className="mt-2 text-sm font-medium text-rose-600">{error}</p>}
              </div>
              <input
                value={searchTerm}
                placeholder="Search by email, role, name, shop..."
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
                <table className="w-full table-fixed border-collapse text-left">
                  <colgroup>
                    <col className="w-[30%]" />
                    <col className="w-[12%]" />
                    <col className="w-[22%]" />
                    <col className="w-[24%]" />
                    <col className="w-[12%]" />
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
                    {pagedUsers.map((user) => (
                      <tr key={user.id} className="align-top transition hover:bg-amber-50/60">
                        <td className="px-4 py-4 font-semibold text-slate-950">{user.email}</td>
                        <td className="px-4 py-4">
                          <span className={`rounded-full px-3 py-1 text-xs font-bold ${user.role === "Admin" ? "bg-slate-950 text-white" : "bg-amber-100 text-amber-800"}`}>
                            {user.role}
                          </span>
                        </td>
                        <td className="px-4 py-4">{user.fullName || "-"}</td>
                        <td className="px-4 py-4">{user.shopName || "-"}</td>
                        <td className="px-4 py-4">
                          <button
                            type="button"
                            className="rounded-full bg-slate-950 px-4 py-2 text-xs font-semibold text-white transition hover:-translate-y-0.5"
                            onClick={() => openViewModal(user)}
                          >
                            View
                          </button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
                <div className="flex flex-col gap-3 border-t border-slate-200 px-5 py-4 text-sm text-slate-600 sm:flex-row sm:items-center sm:justify-between sm:px-6">
                  <p>
                    Showing{" "}
                    <span className="font-semibold text-slate-950">{pageStartIndex + 1}</span>
                    {" - "}
                    <span className="font-semibold text-slate-950">{Math.min(pageStartIndex + accountsPerPage, filteredUsers.length)}</span>
                    {" of "}
                    <span className="font-semibold text-slate-950">{filteredUsers.length}</span>
                    {" accounts"}
                  </p>
                  <div className="flex items-center gap-2">
                    <button
                      type="button"
                      disabled={currentPage === 1}
                      onClick={() => setCurrentPage((page) => Math.max(1, page - 1))}
                      className="rounded-full border border-slate-200 px-4 py-2 text-xs font-semibold text-slate-700 transition hover:bg-slate-100 disabled:cursor-not-allowed disabled:opacity-50"
                    >
                      Previous
                    </button>
                    <span className="rounded-full bg-slate-100 px-4 py-2 text-xs font-bold text-slate-700">
                      Page {currentPage} / {totalPages}
                    </span>
                    <button
                      type="button"
                      disabled={currentPage === totalPages}
                      onClick={() => setCurrentPage((page) => Math.min(totalPages, page + 1))}
                      className="rounded-full border border-slate-200 px-4 py-2 text-xs font-semibold text-slate-700 transition hover:bg-slate-100 disabled:cursor-not-allowed disabled:opacity-50"
                    >
                      Next
                    </button>
                  </div>
                </div>
              </div>
            )}
          </section>
        </main>
      </div>

      {selectedViewUser && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-950/70 px-4 py-6 backdrop-blur-sm">
          <div className="max-h-[92vh] w-full max-w-4xl overflow-auto rounded-[30px] bg-white shadow-[0_30px_120px_rgba(15,23,42,0.3)]">
            <div className="sticky top-0 z-10 flex items-center justify-between border-b border-slate-200 bg-white px-5 py-4">
              <div>
                <p className="text-xs font-semibold uppercase tracking-[0.24em] text-amber-700">Account Details</p>
                <h3 className="mt-1 text-xl font-black text-slate-950">{selectedViewUser.email}</h3>
              </div>
              <button type="button" onClick={closeViewModal} className="rounded-full bg-slate-950 px-4 py-2 text-sm font-semibold text-white">
                Close
              </button>
            </div>

            <div className="space-y-5 p-5">
              <section className="rounded-[24px] border border-slate-200 bg-slate-50 p-4">
                <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
                  <div>
                    <p className="text-xs font-semibold uppercase tracking-[0.22em] text-slate-400">Account Information</p>
                    <h4 className="mt-2 text-2xl font-black text-slate-950">{selectedViewUser.fullName || "Unnamed account"}</h4>
                  </div>
                  <div className="flex flex-wrap gap-2">
                    <span className={`inline-flex rounded-full px-3 py-1 text-xs font-bold ${selectedViewUser.role === "Admin" ? "bg-slate-950 text-white" : "bg-amber-100 text-amber-800"}`}>
                      {selectedViewUser.role}
                    </span>
                    <span className="inline-flex rounded-full bg-emerald-100 px-3 py-1 text-xs font-bold text-emerald-700">
                      Active
                    </span>
                  </div>
                </div>

                <div className="mt-5 grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
                  <div className="rounded-[18px] bg-white px-4 py-3">
                    <p className="text-xs font-semibold uppercase tracking-[0.18em] text-slate-400">Email</p>
                    <p className="mt-1 break-words font-bold text-slate-950">{selectedViewUser.email || "-"}</p>
                  </div>
                  <div className="rounded-[18px] bg-white px-4 py-3">
                    <p className="text-xs font-semibold uppercase tracking-[0.18em] text-slate-400">Shop</p>
                    <p className="mt-1 font-bold text-slate-950">{selectedViewUser.shopName || "-"}</p>
                  </div>
                  <div className="rounded-[18px] bg-white px-4 py-3">
                    <p className="text-xs font-semibold uppercase tracking-[0.18em] text-slate-400">CCCD</p>
                    <p className="mt-1 font-bold text-slate-950">{selectedViewUser.cccd || "-"}</p>
                  </div>
                  <div className="rounded-[18px] bg-white px-4 py-3">
                    <p className="text-xs font-semibold uppercase tracking-[0.18em] text-slate-400">Created</p>
                    <p className="mt-1 font-bold text-slate-950">{formatDate(selectedViewUser.createdAt)}</p>
                  </div>
                  <div className="rounded-[18px] bg-white px-4 py-3">
                    <p className="text-xs font-semibold uppercase tracking-[0.18em] text-slate-400">Updated</p>
                    <p className="mt-1 font-bold text-slate-950">{formatDate(selectedViewUser.updatedAt)}</p>
                  </div>
                </div>

                <div className="mt-3 rounded-[18px] bg-white px-4 py-3">
                  <p className="text-xs font-semibold uppercase tracking-[0.18em] text-slate-400">Address</p>
                  <p className="mt-1 font-bold text-slate-950">{selectedViewUser.address || "-"}</p>
                </div>
              </section>

              {selectedViewUser.role === "Admin" ? (
                <section className="rounded-[24px] border border-slate-200 bg-slate-50 p-4">
                  <p className="text-sm font-semibold text-slate-600">Admin accounts can only be viewed here.</p>
                </section>
              ) : (
                <section className="rounded-[24px] border border-amber-100 bg-amber-50 p-4">
                  <p className="text-xs font-semibold uppercase tracking-[0.22em] text-amber-700">Account Actions</p>
                  <h4 className="mt-2 text-xl font-black text-slate-950">Manage this manager account</h4>
                  <p className="mt-1 text-sm text-amber-800">
                    Inactive will set this account to inactive and hide it from Account List. Data will remain in the database.
                  </p>
                  <div className="mt-4 flex flex-col gap-3 sm:flex-row">
                    <button
                      type="button"
                      className="rounded-full bg-slate-950 px-5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5"
                      onClick={() => {
                        openEditModal(selectedViewUser);
                        setSelectedViewUser(null);
                      }}
                    >
                      Update Account
                    </button>
                    <button
                      type="button"
                      className="rounded-full bg-rose-500 px-5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5 hover:bg-rose-600"
                      onClick={() => {
                        setDeletingUser(selectedViewUser);
                        setSelectedViewUser(null);
                      }}
                    >
                      Inactive Account
                    </button>
                  </div>
                </section>
              )}

              <div className="space-y-1">
                {error && <p className="text-sm font-medium text-rose-600">{error}</p>}
                {success && <p className="text-sm font-medium text-emerald-600">{success}</p>}
              </div>
            </div>
          </div>
        </div>
      )}

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
              <div className="sm:col-span-2 rounded-[18px] border border-slate-200 bg-slate-50 px-4 py-3">
                <p className="text-xs font-semibold uppercase tracking-[0.18em] text-slate-400">System Info</p>
                <p className="mt-1 text-sm font-semibold text-slate-700">
                  Role: {editingUser.role} | Shop ID: {editingUser.shopId || "-"} | Created: {formatDate(editingUser.createdAt)}
                </p>
              </div>

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
              <label className="block sm:col-span-2">
                <span className="mb-2 block text-sm font-semibold text-slate-700">Avatar</span>
                <input
                  type="file"
                  accept="image/*"
                  className="w-full rounded-[16px] border border-slate-200 px-4 py-3 text-sm file:mr-4 file:rounded-full file:border-0 file:bg-slate-950 file:px-4 file:py-2 file:text-sm file:font-semibold file:text-white"
                  onChange={(e) => setAvatarFile(e.target.files?.[0] || null)}
                />
                <p className="mt-2 text-xs font-medium text-slate-500">
                  {avatarFile ? avatarFile.name : editingUser.image ? "Current avatar will be kept if no new file is selected." : "No avatar uploaded yet."}
                </p>
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
            <p className="text-xs font-semibold uppercase tracking-[0.24em] text-rose-600">Inactive Account</p>
            <h3 className="mt-2 text-2xl font-black text-slate-950">{deletingUser.email}</h3>
            <p className="mt-3 text-sm leading-6 text-slate-600">
              This will set the manager account to inactive, revoke active refresh tokens, and hide it from Account List. The database record will not be deleted.
            </p>
            {error && <p className="mt-4 text-sm font-medium text-rose-600">{error}</p>}
            <div className="mt-6 flex flex-col gap-3 sm:flex-row sm:justify-end">
              <button type="button" disabled={saving} onClick={() => setDeletingUser(null)} className="rounded-full border border-slate-200 px-5 py-3 text-sm font-semibold text-slate-700 transition hover:bg-slate-100 disabled:cursor-not-allowed disabled:opacity-50">
                Cancel
              </button>
              <button type="button" disabled={saving} onClick={handleDeleteUser} className="rounded-full bg-rose-500 px-5 py-3 text-sm font-semibold text-white transition hover:bg-rose-600 disabled:cursor-not-allowed disabled:opacity-50">
                {saving ? "Saving..." : "Inactive Account"}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
