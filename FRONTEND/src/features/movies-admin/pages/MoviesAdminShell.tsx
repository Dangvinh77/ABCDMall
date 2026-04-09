import { NavLink, Outlet } from 'react-router-dom';
import {
  Bell,
  ChartColumnBig,
  CircleDollarSign,
  Clapperboard,
  Film,
  LayoutDashboard,
  Mail,
  Megaphone,
  ReceiptText,
  Settings,
  Shield,
  Ticket,
  Users,
} from 'lucide-react';
import { Button } from '../../movies/component/ui/button';
import { Badge } from '../../movies/component/ui/badge';
import { movieAdminPaths } from '../routes/movieAdminPaths';

const navItems = [
  { label: 'Dashboard', to: movieAdminPaths.dashboard(), icon: LayoutDashboard },
  { label: 'Movies', to: movieAdminPaths.movies(), icon: Film },
  { label: 'Showtimes', to: movieAdminPaths.showtimes(), icon: Clapperboard },
  { label: 'Seats', to: movieAdminPaths.seats(), icon: Ticket },
  { label: 'Bookings', to: movieAdminPaths.bookings(), icon: ReceiptText },
  { label: 'Payments', to: movieAdminPaths.payments(), icon: CircleDollarSign },
  { label: 'Emails', to: movieAdminPaths.emails(), icon: Mail },
  { label: 'Guests', to: movieAdminPaths.guests(), icon: Users },
  { label: 'Settings', to: movieAdminPaths.settings(), icon: Settings },
  { label: 'Promotions', to: movieAdminPaths.promotions(), icon: Megaphone },
  { label: 'Logs', to: movieAdminPaths.logs(), icon: ChartColumnBig },
  { label: 'Admin users', to: movieAdminPaths.users(), icon: Shield },
];

export function MoviesAdminShell() {
  return (
    <div className="min-h-screen bg-[linear-gradient(180deg,#040816,#0f172a_38%,#020617)] text-white">
      <div className="pointer-events-none fixed inset-0">
        <div className="absolute left-[-6rem] top-12 h-72 w-72 rounded-full bg-fuchsia-500/12 blur-3xl" />
        <div className="absolute right-[-5rem] top-24 h-80 w-80 rounded-full bg-cyan-400/10 blur-3xl" />
        <div className="absolute bottom-0 left-1/3 h-64 w-64 rounded-full bg-violet-500/10 blur-3xl" />
      </div>

      <div className="relative mx-auto flex min-h-screen max-w-[1680px]">
        <aside className="hidden w-76 shrink-0 border-r border-white/8 bg-slate-950/65 px-5 py-6 backdrop-blur-xl lg:block">
          <div className="mb-8 flex items-center gap-3">
            <div className="flex size-12 items-center justify-center rounded-2xl bg-gradient-to-br from-violet-600 via-fuchsia-500 to-cyan-400 shadow-[0_0_28px_rgba(192,38,211,0.28)]">
              <Film className="size-5 text-white" />
            </div>
            <div>
              <p className="text-xs font-semibold uppercase tracking-[0.28em] text-fuchsia-200/70">
                Movies admin
              </p>
              <h1 className="text-lg font-black tracking-[0.08em] text-white">ABCD Control Room</h1>
            </div>
          </div>

          <div className="mb-6 rounded-3xl border border-white/8 bg-white/[0.03] p-4 shadow-[0_18px_50px_rgba(2,6,23,0.3)]">
            <p className="text-xs font-semibold uppercase tracking-[0.26em] text-cyan-200/70">Today</p>
            <div className="mt-3 space-y-3">
              <div>
                <p className="text-2xl font-black text-white">384</p>
                <p className="text-sm text-gray-400">Guest bookings in progress</p>
              </div>
              <div className="flex items-center gap-2">
                <Badge className="border border-emerald-400/20 bg-emerald-500/10 text-emerald-300">
                  Stable
                </Badge>
                <span className="text-xs text-gray-500">Ticketing SLA within threshold</span>
              </div>
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
                        ? 'border-fuchsia-400/25 bg-[linear-gradient(135deg,rgba(168,85,247,0.24),rgba(34,211,238,0.12))] text-white shadow-[0_14px_34px_rgba(168,85,247,0.18)]'
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
                  Admin portal
                </p>
                <h2 className="mt-1 text-xl font-black tracking-[0.08em] text-white">
                  Manage movies operations
                </h2>
              </div>

              <div className="flex items-center gap-3">
                <Button
                  variant="outline"
                  className="hidden border-white/10 bg-white/[0.03] text-white hover:bg-white/[0.08] sm:inline-flex"
                >
                  <Bell className="mr-2 size-4" />
                  3 live alerts
                </Button>
                <div className="rounded-2xl border border-white/10 bg-white/[0.04] px-4 py-2 text-right">
                  <p className="text-xs text-gray-500">Signed in as</p>
                  <p className="text-sm font-semibold text-white">admin.mall</p>
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
