import { useNavigate } from "react-router-dom";
import { logoutUser } from "../services/auth";

export default function DashboardMall() {
  const role = localStorage.getItem("role") || "Guest";
  const isAdmin = role === "Admin";
  const isManager = role === "Manager";
  const navigate = useNavigate();

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
                Dashboard
              </div>
              <h1 className="mt-3 text-3xl font-black tracking-tight sm:text-4xl">
                System Overview
              </h1>
              <p className="mt-2 max-w-2xl text-sm text-slate-600 sm:text-base">
                This dashboard keeps the content minimal and the layout clear for account information and core actions.
              </p>
            </div>

            <div className="grid gap-3 sm:grid-cols-2 lg:w-[320px]">
              <div className="rounded-3xl bg-slate-950 px-4 py-4 text-white shadow-lg">
                <p className="text-xs uppercase tracking-[0.25em] text-white/60">Current Role</p>
                <p className="mt-2 text-2xl font-bold">{role}</p>
              </div>
              <div className="rounded-3xl border border-slate-200 bg-white px-4 py-4">
                <p className="text-xs uppercase tracking-[0.25em] text-slate-400">Status</p>
                <p className="mt-2 text-2xl font-bold text-emerald-600">Online</p>
              </div>
              <button
                type="button"
                onClick={handleLogout}
                className="sm:col-span-2 rounded-3xl bg-rose-500 px-4 py-4 text-sm font-semibold text-white shadow-lg transition hover:-translate-y-0.5 hover:bg-rose-600"
              >
                Logout
              </button>
            </div>
          </div>
        </header>
 
        <main className="mt-6 flex-1">
          <section className="relative overflow-hidden rounded-[34px] bg-slate-950 px-6 py-7 text-white shadow-[0_30px_120px_rgba(15,23,42,0.26)] sm:px-8 sm:py-9">
            <div className="absolute inset-0 bg-[radial-gradient(circle_at_top_left,_rgba(251,191,36,0.32),_transparent_28%),radial-gradient(circle_at_bottom_right,_rgba(14,165,233,0.24),_transparent_30%)]" />
            <div className="absolute right-[-40px] top-[-30px] h-40 w-40 rounded-full bg-amber-300/20 blur-3xl" />
            <div className="absolute bottom-[-50px] left-[-20px] h-44 w-44 rounded-full bg-cyan-400/15 blur-3xl" />

            <div className="relative grid gap-6 xl:grid-cols-[1.2fr_0.9fr]">
              <div>
                <p className="text-sm font-semibold uppercase tracking-[0.35em] text-amber-300">
                  Overview
                </p>
                <h2 className="mt-4 max-w-xl text-4xl font-black leading-tight sm:text-5xl">
                  Welcome back.
                </h2>
                <p className="mt-4 max-w-2xl text-sm leading-7 text-slate-300 sm:text-base">
                  This overview gives you quick access to the core system areas such as profile, role, and user management.
                </p>

                <div className="mt-6 flex flex-wrap gap-3">
                  <a
                    href="/profile"
                    className="inline-flex items-center justify-center rounded-full bg-white px-5 py-3 text-sm font-semibold text-slate-950 transition hover:-translate-y-0.5"
                  >
                    Open Profile
                  </a>
                  {isManager && (
                    <>
                      <a
                        href="/shop-info"
                        className="inline-flex items-center justify-center rounded-full border border-amber-300/40 bg-amber-300 px-5 py-3 text-sm font-semibold text-slate-950 transition hover:-translate-y-0.5 hover:bg-amber-200"
                      >
                        Shop Info
                      </a>
                      <a
                        href="/manager-shops"
                        className="inline-flex items-center justify-center rounded-full border border-white/20 bg-white/10 px-5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5 hover:bg-white/20"
                      >
                        Manage My Shop
                      </a>
                    </>
                  )}
                  {isAdmin && (
                    <a
                      href="/admin-management"
                      className="inline-flex items-center justify-center rounded-full border border-amber-300/40 bg-amber-300 px-5 py-3 text-sm font-semibold text-slate-950 transition hover:-translate-y-0.5 hover:bg-amber-200"
                    >
                      Admin Management
                    </a>
                  )}
                </div>
              </div>

              <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-1">
                <div className="rounded-[28px] border border-white/10 bg-white/10 p-5 backdrop-blur-md">
                  <p className="text-sm uppercase tracking-[0.28em] text-white/55">Role</p>
                  <p className="mt-3 text-4xl font-black">{role}</p>
                  <p className="mt-2 text-sm text-slate-300">Your current permission level</p>
                </div>
                <div className="rounded-[28px] border border-white/10 bg-gradient-to-br from-amber-300/20 to-orange-400/10 p-5">
                  <p className="text-sm uppercase tracking-[0.28em] text-amber-200">Access</p>
                  <p className="mt-3 text-4xl font-black">{isAdmin ? "Full" : "Basic"}</p>
                  <p className="mt-2 text-sm text-slate-300">Role-based access scope</p>
                </div>
              </div>
            </div>
          </section>
        </main>
      </div>
    </div>
  );
}
