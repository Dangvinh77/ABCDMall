import { useEffect, useState } from "react";
import { Badge } from "../../movies/component/ui/badge";
import {
  type MoviesAdminUser,
  type MoviesAdminUserUpsertRequest,
  moviesAdminApi,
} from "../services/moviesAdminApi";

const emptyForm: MoviesAdminUserUpsertRequest = {
  email: "",
  password: "",
  fullName: "",
  address: "",
  cccd: "",
};

export function MoviesAdminUsersPage() {
  const [users, setUsers] = useState<MoviesAdminUser[]>([]);
  const [form, setForm] = useState<MoviesAdminUserUpsertRequest>(emptyForm);
  const [editingUserId, setEditingUserId] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");

  async function loadUsers() {
    try {
      setLoading(true);
      setError("");
      setUsers(await moviesAdminApi.getMoviesAdmins());
    } catch (err) {
      setError(err instanceof Error ? err.message : "Unable to load MoviesAdmin users.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void loadUsers();
  }, []);

  function resetForm() {
    setForm(emptyForm);
    setEditingUserId(null);
  }

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();

    try {
      setSubmitting(true);
      setError("");

      if (editingUserId) {
        await moviesAdminApi.updateMoviesAdmin(editingUserId, form);
      } else {
        await moviesAdminApi.createMoviesAdmin(form);
      }

      resetForm();
      await loadUsers();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Unable to save MoviesAdmin user.");
    } finally {
      setSubmitting(false);
    }
  }

  function handleEdit(user: MoviesAdminUser) {
    setEditingUserId(user.id ?? null);
    setForm({
      email: user.email,
      password: "",
      fullName: user.fullName ?? "",
      address: user.address ?? "",
      cccd: user.cccd ?? "",
    });
  }

  async function handleDelete(userId?: string) {
    if (!userId) return;

    try {
      setError("");
      await moviesAdminApi.deleteMoviesAdmin(userId);
      if (editingUserId === userId) {
        resetForm();
      }
      await loadUsers();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Unable to delete MoviesAdmin user.");
    }
  }

  return (
    <div className="space-y-6">
      <section className="rounded-[2rem] border border-white/8 bg-[radial-gradient(circle_at_top_left,rgba(34,211,238,0.12),transparent_24%),linear-gradient(180deg,rgba(255,255,255,0.05),rgba(255,255,255,0.02))] p-6 shadow-[0_24px_80px_rgba(2,6,23,0.34)]">
        <div className="flex items-start justify-between gap-4">
          <div>
            <p className="text-xs font-semibold uppercase tracking-[0.28em] text-cyan-200/70">Admin users</p>
            <h1 className="mt-2 text-3xl font-black uppercase tracking-[0.08em] text-white">MoviesAdmin accounts</h1>
          </div>
          <Badge className="border border-fuchsia-400/20 bg-fuchsia-500/10 px-3 py-1.5 text-fuchsia-200">
            {users.length} accounts
          </Badge>
        </div>
      </section>

      {error ? <div className="rounded-3xl border border-red-500/20 bg-red-500/10 px-4 py-3 text-sm text-red-200">{error}</div> : null}

      <section className="grid gap-6 xl:grid-cols-[360px_minmax(0,1fr)]">
        <form onSubmit={handleSubmit} className="space-y-4 rounded-[2rem] border border-white/8 bg-white/[0.03] p-6 shadow-[0_24px_80px_rgba(2,6,23,0.34)]">
          <h2 className="text-lg font-black uppercase tracking-[0.08em] text-white">{editingUserId ? "Edit user" : "Create user"}</h2>
          <input value={form.email} onChange={(e) => setForm((c) => ({ ...c, email: e.target.value }))} placeholder="Email" className="w-full rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white" />
          <input value={form.password ?? ""} onChange={(e) => setForm((c) => ({ ...c, password: e.target.value }))} placeholder={editingUserId ? "New password (optional)" : "Password"} className="w-full rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white" />
          <input value={form.fullName} onChange={(e) => setForm((c) => ({ ...c, fullName: e.target.value }))} placeholder="Full name" className="w-full rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white" />
          <input value={form.address ?? ""} onChange={(e) => setForm((c) => ({ ...c, address: e.target.value }))} placeholder="Address" className="w-full rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white" />
          <input value={form.cccd ?? ""} onChange={(e) => setForm((c) => ({ ...c, cccd: e.target.value }))} placeholder="CCCD" className="w-full rounded-2xl border border-white/10 bg-slate-950/40 px-4 py-3 text-sm text-white" />
          <div className="flex gap-3">
            <button type="submit" disabled={submitting} className="flex-1 rounded-2xl bg-cyan-500 px-4 py-3 text-sm font-semibold text-slate-950 disabled:opacity-50">{submitting ? "Saving..." : editingUserId ? "Update" : "Create"}</button>
            {editingUserId ? <button type="button" onClick={resetForm} className="rounded-2xl border border-white/10 px-4 py-3 text-sm text-white">Reset</button> : null}
          </div>
        </form>

        <div className="overflow-hidden rounded-[2rem] border border-white/8 bg-white/[0.03] shadow-[0_24px_80px_rgba(2,6,23,0.34)]">
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-white/8 text-left text-sm">
              <thead className="bg-white/[0.04]">
                <tr>{["Email", "Name", "Role", "CCCD", "Actions"].map((column) => <th key={column} className="px-4 py-3 text-xs font-semibold uppercase tracking-[0.18em] text-gray-500">{column}</th>)}</tr>
              </thead>
              <tbody className="divide-y divide-white/8">
                {loading ? <tr><td colSpan={5} className="px-4 py-6 text-gray-400">Loading accounts...</td></tr> : null}
                {!loading && users.length === 0 ? <tr><td colSpan={5} className="px-4 py-6 text-gray-400">No accounts found.</td></tr> : null}
                {users.map((user) => (
                  <tr key={user.id ?? user.email} className="bg-white/[0.02]">
                    <td className="px-4 py-3 text-white">{user.email}</td>
                    <td className="px-4 py-3 text-gray-300">{user.fullName ?? "-"}</td>
                    <td className="px-4 py-3 text-gray-300">{user.role}</td>
                    <td className="px-4 py-3 text-gray-300">{user.cccd ?? "-"}</td>
                    <td className="px-4 py-3">
                      <div className="flex gap-2">
                        <button className="text-cyan-200" onClick={() => handleEdit(user)}>Edit</button>
                        <button className="text-rose-200" onClick={() => void handleDelete(user.id)}>Delete</button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </section>
    </div>
  );
}
