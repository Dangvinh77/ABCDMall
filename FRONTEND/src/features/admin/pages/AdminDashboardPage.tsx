import { BarChart3, Users, Map, ArrowRight } from 'lucide-react';
import { Link } from 'react-router-dom';
import { adminPaths } from '../routes/adminPaths';

const adminModules = [
  {
    title: 'User Management',
    description: 'Create and manage manager accounts, assign roles and permissions.',
    icon: Users,
    to: adminPaths.users(),
    color: 'from-violet-500/30 to-fuchsia-500/10 border-fuchsia-400/20',
    iconColor: 'text-violet-400',
  },
  {
    title: 'Slot Management',
    description: 'Assign and manage shop slots across mall floors.',
    icon: Map,
    to: adminPaths.maps(),
    color: 'from-cyan-500/30 to-blue-500/10 border-cyan-400/20',
    iconColor: 'text-cyan-400',
  },
];

export function AdminDashboardPage() {
  return (
    <div className="space-y-6">
      {/* Welcome Header */}
      <div className="rounded-[2rem] border border-white/8 bg-[radial-gradient(circle_at_top_left,rgba(168,85,247,0.16),transparent_30%),linear-gradient(180deg,rgba(255,255,255,0.05),rgba(255,255,255,0.02))] p-6 shadow-[0_24px_80px_rgba(2,6,23,0.34)]">
        <div>
          <p className="text-xs font-semibold uppercase tracking-[0.28em] text-violet-200/70">
            Administration
          </p>
          <h1 className="mt-2 text-3xl font-black uppercase tracking-[0.08em] text-white">
            Admin Control Center
          </h1>
          <p className="mt-3 max-w-2xl text-sm leading-7 text-gray-400">
            Manage users, assign shop slots, and oversee mall operations from this centralized dashboard.
          </p>
        </div>
      </div>

      {/* Quick Stats */}
      <div className="grid gap-4 md:grid-cols-3">
        <div className="rounded-[1.5rem] border border-white/8 bg-gradient-to-br from-violet-500/10 to-fuchsia-500/5 p-6">
          <div className="flex items-start justify-between">
            <div>
              <p className="text-xs font-semibold uppercase tracking-wider text-gray-400">
                Total Users
              </p>
              <p className="mt-3 text-2xl font-bold text-white">—</p>
              <p className="mt-1 text-xs text-gray-500">Across all roles</p>
            </div>
            <Users className="size-8 text-violet-400/50" />
          </div>
        </div>

        <div className="rounded-[1.5rem] border border-white/8 bg-gradient-to-br from-cyan-500/10 to-blue-500/5 p-6">
          <div className="flex items-start justify-between">
            <div>
              <p className="text-xs font-semibold uppercase tracking-wider text-gray-400">
                Occupied Slots
              </p>
              <p className="mt-3 text-2xl font-bold text-white">—</p>
              <p className="mt-1 text-xs text-gray-500">Across all floors</p>
            </div>
            <Map className="size-8 text-cyan-400/50" />
          </div>
        </div>

        <div className="rounded-[1.5rem] border border-white/8 bg-gradient-to-br from-green-500/10 to-emerald-500/5 p-6">
          <div className="flex items-start justify-between">
            <div>
              <p className="text-xs font-semibold uppercase tracking-wider text-gray-400">
                Available Slots
              </p>
              <p className="mt-3 text-2xl font-bold text-white">—</p>
              <p className="mt-1 text-xs text-gray-500">Ready to assign</p>
            </div>
            <BarChart3 className="size-8 text-green-400/50" />
          </div>
        </div>
      </div>

      {/* Main Modules */}
      <div>
        <h2 className="mb-4 text-lg font-bold text-white">Management Modules</h2>
        <div className="grid gap-4 md:grid-cols-2">
          {adminModules.map((module) => {
            const Icon = module.icon;
            return (
              <Link
                key={module.to}
                to={module.to}
                className="group rounded-[2rem] border border-white/8 bg-gradient-to-br p-6 hover:border-white/20 transition-all"
                style={{
                  backgroundImage: `linear-gradient(135deg, var(--tw-gradient-stops))`,
                }}
              >
                <div className="flex items-start justify-between gap-4">
                  <div>
                    <div className={`mb-4 inline-flex rounded-xl bg-white/5 p-3 ${module.iconColor}`}>
                      <Icon className="size-6" />
                    </div>
                    <h3 className="text-lg font-bold text-white group-hover:text-white transition-colors">
                      {module.title}
                    </h3>
                    <p className="mt-2 text-sm text-gray-400">{module.description}</p>
                  </div>
                  <ArrowRight className="size-5 text-gray-500 group-hover:text-white group-hover:translate-x-1 transition-all" />
                </div>
              </Link>
            );
          })}
        </div>
      </div>

      {/* Info Section */}
      <div className="rounded-[2rem] border border-white/8 bg-slate-950/65 p-6 space-y-4">
        <h3 className="text-lg font-bold text-white">Quick Guide</h3>
        <ul className="space-y-3 text-sm text-gray-400">
          <li className="flex gap-3">
            <span className="text-green-400">→</span>
            <span>Use <strong className="text-white">User Management</strong> to create new manager accounts and manage existing users</span>
          </li>
          <li className="flex gap-3">
            <span className="text-green-400">→</span>
            <span>Use <strong className="text-white">Slot Management</strong> to assign/release shop slots on the mall floors</span>
          </li>
          <li className="flex gap-3">
            <span className="text-green-400">→</span>
            <span>When creating a manager account, you can optionally assign a slot immediately</span>
          </li>
          <li className="flex gap-3">
            <span className="text-green-400">→</span>
            <span>You can also assign slots later from the Slot Management page</span>
          </li>
        </ul>
      </div>
    </div>
  );
}
