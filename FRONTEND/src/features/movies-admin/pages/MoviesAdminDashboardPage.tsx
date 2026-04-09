import { AlertTriangle, ArrowUpRight, CircleAlert, CircleCheckBig } from 'lucide-react';
import { Badge } from '../../movies/component/ui/badge';
import {
  dashboardAlerts,
  dashboardRevenueBars,
  dashboardStats,
  dashboardTopMovies,
} from '../data/adminData';

const toneMap = {
  violet: 'from-violet-500/30 to-fuchsia-500/10 text-fuchsia-100 border-fuchsia-400/20',
  emerald: 'from-emerald-500/24 to-emerald-500/8 text-emerald-100 border-emerald-400/20',
  cyan: 'from-cyan-500/24 to-sky-500/8 text-cyan-100 border-cyan-400/20',
  amber: 'from-amber-500/26 to-orange-500/8 text-amber-100 border-amber-400/20',
  rose: 'from-rose-500/26 to-pink-500/8 text-rose-100 border-rose-400/20',
};

const severityMap = {
  high: {
    icon: AlertTriangle,
    badge: 'border-rose-400/20 bg-rose-500/10 text-rose-300',
  },
  medium: {
    icon: CircleAlert,
    badge: 'border-amber-400/20 bg-amber-500/10 text-amber-300',
  },
  low: {
    icon: CircleCheckBig,
    badge: 'border-emerald-400/20 bg-emerald-500/10 text-emerald-300',
  },
};

export function MoviesAdminDashboardPage() {
  return (
    <div className="space-y-6">
      <section className="grid gap-4 xl:grid-cols-[minmax(0,1.55fr)_minmax(320px,0.95fr)]">
        <div className="rounded-[2rem] border border-white/8 bg-[radial-gradient(circle_at_top_left,rgba(168,85,247,0.16),transparent_30%),linear-gradient(180deg,rgba(255,255,255,0.05),rgba(255,255,255,0.02))] p-6 shadow-[0_24px_80px_rgba(2,6,23,0.34)]">
          <div className="flex flex-wrap items-start justify-between gap-4">
            <div>
              <p className="text-xs font-semibold uppercase tracking-[0.28em] text-fuchsia-200/70">
                Dashboard
              </p>
              <h1 className="mt-2 text-3xl font-black uppercase tracking-[0.08em] text-white">
                Cinema operations overview
              </h1>
              <p className="mt-3 max-w-2xl text-sm leading-7 text-gray-400">
                A live snapshot of sales performance, booking flow health, occupancy and the issues
                that matter most when running a guest-based movie platform.
              </p>
            </div>

            <Badge className="border border-cyan-400/20 bg-cyan-500/10 px-3 py-1.5 text-cyan-200">
              Last sync: just now
            </Badge>
          </div>

          <div className="mt-6 grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
            {dashboardStats.map((stat) => (
              <div
                key={stat.label}
                className={`rounded-3xl border bg-gradient-to-br p-4 shadow-[0_18px_40px_rgba(15,23,42,0.24)] ${toneMap[stat.tone]}`}
              >
                <p className="text-sm text-white/70">{stat.label}</p>
                <p className="mt-3 text-3xl font-black tracking-tight text-white">{stat.value}</p>
                <p className="mt-2 flex items-center gap-1.5 text-xs text-white/65">
                  <ArrowUpRight className="size-3.5" />
                  {stat.change}
                </p>
              </div>
            ))}
          </div>
        </div>

        <div className="rounded-[2rem] border border-white/8 bg-[linear-gradient(180deg,rgba(255,255,255,0.05),rgba(255,255,255,0.02))] p-6 shadow-[0_24px_80px_rgba(2,6,23,0.34)]">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-xs font-semibold uppercase tracking-[0.28em] text-cyan-200/70">
                Revenue
              </p>
              <h2 className="mt-2 text-xl font-black uppercase tracking-[0.08em] text-white">
                Weekly trend
              </h2>
            </div>
            <Badge className="border border-fuchsia-400/20 bg-fuchsia-500/10 text-fuchsia-200">
              7-day view
            </Badge>
          </div>

          <div className="mt-8 flex h-64 items-end gap-3">
            {dashboardRevenueBars.map((bar) => (
              <div key={bar.label} className="flex flex-1 flex-col items-center justify-end gap-3">
                <div
                  className="w-full rounded-t-3xl bg-[linear-gradient(180deg,rgba(34,211,238,0.92),rgba(168,85,247,0.88))] shadow-[0_16px_30px_rgba(34,211,238,0.16)]"
                  style={{ height: `${bar.value}%` }}
                />
                <span className="text-xs font-semibold uppercase tracking-[0.18em] text-gray-500">
                  {bar.label}
                </span>
              </div>
            ))}
          </div>
        </div>
      </section>

      <section className="grid gap-6 xl:grid-cols-[minmax(0,1.15fr)_minmax(360px,0.85fr)]">
        <div className="rounded-[2rem] border border-white/8 bg-white/[0.03] p-6 shadow-[0_24px_80px_rgba(2,6,23,0.34)]">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-xs font-semibold uppercase tracking-[0.28em] text-cyan-200/70">
                Top movies
              </p>
              <h2 className="mt-2 text-xl font-black uppercase tracking-[0.08em] text-white">
                Best sellers
              </h2>
            </div>
            <Badge className="border border-emerald-400/20 bg-emerald-500/10 text-emerald-200">
              Based on sold seats
            </Badge>
          </div>

          <div className="mt-6 space-y-4">
            {dashboardTopMovies.map((movie, index) => (
              <div
                key={movie.title}
                className="grid gap-3 rounded-3xl border border-white/8 bg-[linear-gradient(180deg,rgba(255,255,255,0.04),rgba(255,255,255,0.02))] p-4 md:grid-cols-[56px_minmax(0,1fr)_140px_140px]"
              >
                <div className="flex size-14 items-center justify-center rounded-2xl bg-gradient-to-br from-violet-600/80 to-fuchsia-500/80 text-xl font-black text-white">
                  {index + 1}
                </div>
                <div>
                  <p className="text-lg font-semibold text-white">{movie.title}</p>
                  <p className="mt-1 text-sm text-gray-400">{movie.genre}</p>
                </div>
                <div>
                  <p className="text-xs uppercase tracking-[0.18em] text-gray-500">Tickets</p>
                  <p className="mt-2 text-lg font-bold text-cyan-200">{movie.tickets}</p>
                </div>
                <div>
                  <p className="text-xs uppercase tracking-[0.18em] text-gray-500">Revenue / occupancy</p>
                  <p className="mt-2 text-lg font-bold text-white">{movie.revenue}</p>
                  <p className="text-sm text-emerald-300">{movie.occupancy}</p>
                </div>
              </div>
            ))}
          </div>
        </div>

        <div className="rounded-[2rem] border border-white/8 bg-white/[0.03] p-6 shadow-[0_24px_80px_rgba(2,6,23,0.34)]">
          <div>
            <p className="text-xs font-semibold uppercase tracking-[0.28em] text-fuchsia-200/70">
              Alerts
            </p>
            <h2 className="mt-2 text-xl font-black uppercase tracking-[0.08em] text-white">
              Live operations feed
            </h2>
          </div>

          <div className="mt-6 space-y-4">
            {dashboardAlerts.map((alert) => {
              const Icon = severityMap[alert.severity].icon;
              return (
                <div
                  key={alert.title}
                  className="rounded-3xl border border-white/8 bg-[linear-gradient(180deg,rgba(255,255,255,0.04),rgba(255,255,255,0.02))] p-4"
                >
                  <div className="flex items-start gap-3">
                    <div className="mt-1 flex size-10 items-center justify-center rounded-2xl bg-white/[0.05]">
                      <Icon className="size-5 text-white" />
                    </div>
                    <div className="min-w-0 flex-1">
                      <div className="flex flex-wrap items-center justify-between gap-3">
                        <p className="font-semibold text-white">{alert.title}</p>
                        <Badge className={severityMap[alert.severity].badge}>{alert.severity}</Badge>
                      </div>
                      <p className="mt-2 text-sm leading-6 text-gray-400">{alert.detail}</p>
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      </section>
    </div>
  );
}
