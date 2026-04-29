import { Link } from "react-router-dom";

const adminSections = [
  {
    title: "Rental Areas",
    description: "Manage mall rental spaces, register tenants, cancel rentals, and update monthly usage.",
    href: "/rental-areas",
    accent: "bg-amber-300 text-slate-950 hover:bg-amber-200",
  },
  {
    title: "User Management",
    description: "Review registered accounts, update manager information, and delete manager accounts.",
    href: "/admin-management/users",
    accent: "bg-sky-300 text-slate-950 hover:bg-sky-200",
  },
  {
    title: "Revenue Statistics",
    description: "Track rental revenue by year, month, and location with detailed monthly bill records.",
    href: "/admin-management/revenue",
    accent: "bg-emerald-300 text-slate-950 hover:bg-emerald-200",
  },
  {
    title: "Bidding Control",
    description: "Review weekly homepage bids, submit the fixed movie ad, and run the Saturday and Monday simulation flows.",
    href: "/admin-management/bidding",
    accent: "bg-violet-300 text-slate-950 hover:bg-violet-200",
  },
  {
    title: "Event Management",
    description: "Create mall event campaigns and approve pending shop event submissions.",
    href: "/admin-management/events",
    accent: "bg-rose-300 text-slate-950 hover:bg-rose-200",
  },
  {
    title: "Create Manager",
    description: "Register a manager account, create the initial shop profile, and send email notification.",
    href: "/register",
    accent: "bg-slate-950 text-white hover:bg-slate-800",
  },
];

export default function AdminManagement() {
  const role = localStorage.getItem("role") || "Guest";
  const isAdmin = role === "Admin";

  if (!isAdmin) {
    return (
      <div className="min-h-screen bg-[linear-gradient(180deg,#fff8ef_0%,#fffdf8_42%,#f8fbff_100%)] px-4 py-6 text-slate-900 sm:px-6 lg:px-8">
        <div className="mx-auto max-w-3xl rounded-[28px] border border-slate-200 bg-white/90 p-6 shadow-[0_20px_80px_rgba(15,23,42,0.08)]">
          <p className="text-xs font-semibold uppercase tracking-[0.28em] text-amber-700">
            Admin Management
          </p>
          <h1 className="mt-3 text-3xl font-black">Admin access only</h1>
          <p className="mt-3 text-sm text-slate-600">
            This management area is only available for admin accounts.
          </p>
          <Link
            to="/dashboard"
            className="mt-6 inline-flex rounded-full bg-slate-950 px-5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5"
          >
            Back to Dashboard
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
                Admin Management
              </div>
              <h1 className="mt-3 text-3xl font-black tracking-tight sm:text-4xl">
                Mall Operations Center
              </h1>
              <p className="mt-2 max-w-2xl text-sm text-slate-600 sm:text-base">
                User management, revenue statistics, and rental workflows are grouped here for admin accounts.
              </p>
            </div>

            <Link
              to="/dashboard"
              className="inline-flex items-center justify-center rounded-full bg-slate-950 px-5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5"
            >
              Back to Dashboard
            </Link>
          </div>
        </header>

        <main className="mt-6 grid flex-1 gap-5 lg:grid-cols-[0.8fr_1.2fr]">
          <section className="rounded-[34px] bg-slate-950 p-6 text-white shadow-[0_30px_120px_rgba(15,23,42,0.26)] sm:p-8">
            <p className="text-sm font-semibold uppercase tracking-[0.35em] text-amber-300">
              Overview
            </p>
            <h2 className="mt-4 text-4xl font-black leading-tight sm:text-5xl">
              One place for admin work.
            </h2>
            <p className="mt-4 text-sm leading-7 text-slate-300 sm:text-base">
              Instead of separating user management and revenue into standalone dashboard buttons, this area keeps the admin tools together.
            </p>
            <div className="mt-8 rounded-[28px] border border-white/10 bg-white/10 p-5 backdrop-blur-md">
              <p className="text-sm uppercase tracking-[0.28em] text-white/55">Access</p>
              <p className="mt-3 text-4xl font-black">{role}</p>
              <p className="mt-2 text-sm text-slate-300">Full admin management scope</p>
            </div>
          </section>

          <section className="grid gap-4">
            {adminSections.map((section) => (
              <Link
                key={section.title}
                to={section.href}
                className="group rounded-[30px] border border-slate-200 bg-white/90 p-5 shadow-[0_24px_90px_rgba(15,23,42,0.08)] backdrop-blur-xl transition hover:-translate-y-1 hover:shadow-[0_30px_110px_rgba(15,23,42,0.14)]"
              >
                <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
                  <div>
                    <p className="text-xs font-semibold uppercase tracking-[0.24em] text-slate-400">
                      Admin Tool
                    </p>
                    <h3 className="mt-2 text-2xl font-black text-slate-950">{section.title}</h3>
                    <p className="mt-2 max-w-2xl text-sm leading-6 text-slate-600">{section.description}</p>
                  </div>
                  <span className={`inline-flex shrink-0 items-center justify-center rounded-full px-5 py-3 text-sm font-bold transition group-hover:-translate-y-0.5 ${section.accent}`}>
                    Open
                  </span>
                </div>
              </Link>
            ))}
          </section>
        </main>
      </div>
    </div>
  );
}
