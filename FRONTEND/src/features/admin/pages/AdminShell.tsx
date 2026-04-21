import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import {
  LayoutDashboard,
  Users,
  Map,
  LogOut,
  Menu,
  X,
} from 'lucide-react';
import { useState } from 'react';
import { adminPaths } from '../routes/adminPaths';

const navItems = [
  { label: 'Dashboard', to: adminPaths.dashboard(), icon: LayoutDashboard },
  { label: 'Users', to: adminPaths.users(), icon: Users },
  { label: 'Slots', to: adminPaths.maps(), icon: Map },
];

export function AdminShell() {
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const navigate = useNavigate();

  const handleLogout = () => {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    navigate('/auth/login');
  };

  return (
    <div className="min-h-screen bg-[linear-gradient(180deg,#040816,#0f172a_38%,#020617)] text-white">
      {/* Background Effects */}
      <div className="pointer-events-none fixed inset-0">
        <div className="absolute left-[-6rem] top-12 h-72 w-72 rounded-full bg-violet-500/12 blur-3xl" />
        <div className="absolute right-[-5rem] top-24 h-80 w-80 rounded-full bg-cyan-400/10 blur-3xl" />
        <div className="absolute bottom-0 left-1/3 h-64 w-64 rounded-full bg-violet-500/10 blur-3xl" />
      </div>

      <div className="relative mx-auto flex min-h-screen max-w-[1680px]">
        {/* Mobile Menu Button */}
        <button
          onClick={() => setSidebarOpen(!sidebarOpen)}
          className="lg:hidden fixed top-6 left-6 z-40 rounded-lg bg-white/10 p-2 hover:bg-white/20"
        >
          {sidebarOpen ? <X className="size-6" /> : <Menu className="size-6" />}
        </button>

        {/* Sidebar */}
        <aside
          className={`${
            sidebarOpen ? 'translate-x-0' : '-translate-x-full'
          } lg:translate-x-0 transition-transform fixed lg:relative w-76 h-screen shrink-0 border-r border-white/8 bg-slate-950/65 px-5 py-6 backdrop-blur-xl z-30`}
        >
          <div className="mb-8 flex items-center gap-3">
            <div className="flex size-12 items-center justify-center rounded-2xl bg-gradient-to-br from-violet-600 via-fuchsia-500 to-cyan-400 shadow-[0_0_28px_rgba(192,38,211,0.28)]">
              <LayoutDashboard className="size-6 text-white" />
            </div>
            <div>
              <h2 className="font-bold text-white">ABCD Admin</h2>
              <p className="text-xs text-gray-400">Control Center</p>
            </div>
          </div>

          {/* Navigation */}
          <nav className="mb-8 space-y-2">
            {navItems.map((item) => {
              const Icon = item.icon;
              return (
                <NavLink
                  key={item.to}
                  to={item.to}
                  onClick={() => setSidebarOpen(false)}
                  className={({ isActive }) =>
                    `flex items-center gap-3 rounded-lg px-4 py-3 text-sm font-semibold transition-all ${
                      isActive
                        ? 'bg-violet-500/20 text-violet-200 shadow-[0_8px_16px_rgba(139,92,246,0.3)]'
                        : 'text-gray-300 hover:bg-white/10'
                    }`
                  }
                >
                  <Icon className="size-5 flex-shrink-0" />
                  <span>{item.label}</span>
                </NavLink>
              );
            })}
          </nav>

          {/* Divider */}
          <div className="mb-8 h-px bg-gradient-to-r from-transparent via-white/10 to-transparent" />

          {/* Logout Button */}
          <button
            onClick={handleLogout}
            className="flex w-full items-center gap-3 rounded-lg px-4 py-3 text-sm font-semibold text-gray-300 hover:bg-red-500/10 hover:text-red-300 transition-all"
          >
            <LogOut className="size-5" />
            <span>Logout</span>
          </button>

          {/* User Info */}
          <div className="mt-8 rounded-lg border border-white/8 bg-white/5 p-4 text-xs text-gray-400">
            <p className="font-semibold text-white">Admin Panel</p>
            <p className="mt-2">Manage users, assign slots, and oversee operations.</p>
          </div>
        </aside>

        {/* Main Content */}
        <main className="flex-1 overflow-auto">
          <div className="p-6 lg:p-8">
            <Outlet />
          </div>
        </main>
      </div>
    </div>
  );
}
