import { NavLink, Outlet } from 'react-router-dom';
import {
  ChartColumnBig,
  CircleDollarSign,
  Clapperboard,
  Film,
  LayoutDashboard,
  Mail,
  Megaphone,
  PieChart,
  ReceiptText,
  Settings,
  Shield,
  Ticket,
  Users,
} from 'lucide-react';
import { movieAdminPaths } from '../routes/movieAdminPaths';

const navItems = [
  { label: 'Dashboard', to: movieAdminPaths.dashboard(), icon: LayoutDashboard },
  { label: 'Movies', to: movieAdminPaths.movies(), icon: Film },
  { label: 'Showtimes', to: movieAdminPaths.showtimes(), icon: Clapperboard },
  { label: 'Seats', to: movieAdminPaths.seats(), icon: Ticket },
  { label: 'Bookings', to: movieAdminPaths.bookings(), icon: ReceiptText },
  { label: 'Revenue', to: movieAdminPaths.revenue(), icon: PieChart },
  { label: 'Payments', to: movieAdminPaths.payments(), icon: CircleDollarSign },
  { label: 'Emails', to: movieAdminPaths.emails(), icon: Mail },
  { label: 'Guests', to: movieAdminPaths.guests(), icon: Users },
  { label: 'Settings', to: movieAdminPaths.settings(), icon: Settings },
  { label: 'Promotions', to: movieAdminPaths.promotions(), icon: Megaphone },
  { label: 'Logs', to: movieAdminPaths.logs(), icon: ChartColumnBig },
  { label: 'Admin users', to: movieAdminPaths.users(), icon: Shield },
];

export function MoviesAdminShell() {
  const role = localStorage.getItem("role") ?? "Unknown";

  return (
    <div className="min-h-screen bg-[linear-gradient(180deg,#050816,#0b1220_38%,#030712)] text-white">
      <div className="pointer-events-none fixed inset-0">
        <div className="absolute left-[-6rem] top-12 h-72 w-72 rounded-full bg-fuchsia-500/8 blur-3xl" />
        <div className="absolute right-[-5rem] top-24 h-80 w-80 rounded-full bg-cyan-400/6 blur-3xl" />
        <div className="absolute bottom-0 left-1/3 h-64 w-64 rounded-full bg-violet-500/6 blur-3xl" />
      </div>

      <div className="relative mx-auto flex min-h-screen max-w-[1540px]">
        <aside className="hidden w-[292px] shrink-0 border-r border-white/8 bg-slate-950/60 px-5 py-6 backdrop-blur-xl lg:block">
          <div className="mb-8 flex items-center gap-3">
            <div className="flex size-12 items-center justify-center rounded-2xl bg-gradient-to-br from-violet-600 via-fuchsia-500 to-cyan-400 shadow-[0_0_28px_rgba(192,38,211,0.28)]">
              <Film className="size-5 text-white" />
            </div>
            <div>
              <p className="text-xs font-semibold uppercase tracking-[0.28em] text-fuchsia-200/70">
                Movies admin
              </p>
              <h1 className="text-lg font-black tracking-[0.08em] text-white">ABCD Cinema Ops</h1>
            </div>
          </div>

          <nav className="space-y-1.5">
            {navItems.map((item) => {
              const Icon = item.icon;
              return (
                <NavLink
                  key={item.to}
                  to={item.to}
                  end={item.to === movieAdminPaths.dashboard()}
                  className={({ isActive }) =>
                    [
                      'flex items-center gap-3 rounded-2xl border px-4 py-3 text-sm font-medium transition-all duration-200',
                      isActive
                        ? 'border-white/12 bg-white/[0.08] text-white shadow-[0_10px_24px_rgba(2,6,23,0.24)]'
                        : 'border-transparent text-gray-400 hover:border-white/8 hover:bg-white/[0.04] hover:text-white',
                    ].join(' ')
                  }
                >
                  <Icon className="size-4" />
                  <span>{item.label}</span>
                </NavLink>
              );
            })}
          </nav>
        </aside>

        <div className="flex min-h-screen flex-1 flex-col">
          <header className="sticky top-0 z-30 border-b border-white/8 bg-slate-950/60 px-4 py-4 backdrop-blur-xl sm:px-6 lg:px-8">
            <div className="flex items-center justify-between gap-4">
              <div>
                <p className="text-xs font-semibold uppercase tracking-[0.28em] text-cyan-200/70">
                  Movies admin
                </p>
                <h2 className="mt-1 text-xl font-black tracking-[0.06em] text-white">
                  Operations
                </h2>
              </div>

              <div className="flex items-center gap-3">
                <div className="rounded-full border border-white/10 bg-white/[0.04] px-4 py-2 text-sm font-semibold text-white">
                  {role}
                </div>
              </div>
            </div>
          </header>

          <main className="flex-1 px-4 py-6 sm:px-6 lg:px-8">
            <Outlet />
          </main>
        </div>
      </div>
    </div>
  );
}
