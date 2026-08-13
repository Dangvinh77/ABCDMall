import { Badge } from '../../movies/component/ui/badge';
import type { AdminSectionId } from '../data/adminData';
import { adminSections } from '../data/adminData';

export function MoviesAdminSectionPage({ sectionId }: { sectionId: AdminSectionId }) {
  const section = adminSections[sectionId];

  return (
    <div className="space-y-6">
      <section className="rounded-[2rem] border border-white/8 bg-[radial-gradient(circle_at_top_left,rgba(34,211,238,0.12),transparent_24%),linear-gradient(180deg,rgba(255,255,255,0.05),rgba(255,255,255,0.02))] p-6 shadow-[0_24px_80px_rgba(2,6,23,0.34)]">
        <div className="flex flex-wrap items-start justify-between gap-4">
          <div>
            <p className="text-xs font-semibold uppercase tracking-[0.28em] text-cyan-200/70">
              {section.eyebrow}
            </p>
            <h1 className="mt-2 text-3xl font-black uppercase tracking-[0.08em] text-white">
              {section.title}
            </h1>
            <p className="mt-3 max-w-3xl text-sm leading-7 text-gray-400">{section.description}</p>
          </div>
          <Badge className="border border-fuchsia-400/20 bg-fuchsia-500/10 px-3 py-1.5 text-fuchsia-200">
            {section.metrics.length} metrics
          </Badge>
        </div>

        <div className="mt-6 grid gap-4 md:grid-cols-2 xl:grid-cols-4">
          {section.metrics.map((metric) => (
            <div
              key={metric.label}
              className="rounded-3xl border border-white/8 bg-[linear-gradient(180deg,rgba(255,255,255,0.05),rgba(255,255,255,0.025))] p-4"
            >
              <p className="text-sm text-gray-400">{metric.label}</p>
              <p className="mt-3 text-3xl font-black tracking-tight text-white">{metric.value}</p>
              <p className="mt-2 text-xs text-gray-500">{metric.hint}</p>
            </div>
          ))}
        </div>
      </section>

      <section className="grid gap-6 xl:grid-cols-[minmax(0,1.2fr)_minmax(340px,0.8fr)]">
        <div className="rounded-[2rem] border border-white/8 bg-white/[0.03] p-6 shadow-[0_24px_80px_rgba(2,6,23,0.34)]">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-xs font-semibold uppercase tracking-[0.28em] text-fuchsia-200/70">
                Operational view
              </p>
              <h2 className="mt-2 text-xl font-black uppercase tracking-[0.08em] text-white">
                Current records
              </h2>
            </div>
            <Badge className="border border-white/10 bg-white/[0.04] text-gray-200">
              {section.table.rows.length} rows shown
            </Badge>
          </div>

          <div className="mt-6 overflow-hidden rounded-3xl border border-white/8">
            <div className="overflow-x-auto">
              <table className="min-w-full divide-y divide-white/8 text-left text-sm">
                <thead className="bg-white/[0.04]">
                  <tr>
                    {section.table.columns.map((column) => (
                      <th
                        key={column}
                        className="px-4 py-3 text-xs font-semibold uppercase tracking-[0.18em] text-gray-500"
                      >
                        {column}
                      </th>
                    ))}
                  </tr>
                </thead>
                <tbody className="divide-y divide-white/8">
                  {section.table.rows.map((row, rowIndex) => (
                    <tr key={`${sectionId}-${rowIndex}`} className="bg-white/[0.02]">
                      {row.map((cell, cellIndex) => (
                        <td key={`${sectionId}-${rowIndex}-${cellIndex}`} className="px-4 py-3 text-gray-200">
                          {cell}
                        </td>
                      ))}
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>

        <div className="space-y-6">
          {section.panels.map((panel) => (
            <div
              key={panel.title}
              className="rounded-[2rem] border border-white/8 bg-[linear-gradient(180deg,rgba(255,255,255,0.04),rgba(255,255,255,0.02))] p-6 shadow-[0_24px_80px_rgba(2,6,23,0.34)]"
            >
              <p className="text-xs font-semibold uppercase tracking-[0.28em] text-cyan-200/70">
                Overview
              </p>
              <h3 className="mt-2 text-xl font-black uppercase tracking-[0.08em] text-white">
                {panel.title}
              </h3>
              <p className="mt-3 text-sm leading-7 text-gray-400">{panel.description}</p>
              <div className="mt-4 space-y-3">
                {panel.bullets.map((bullet) => (
                  <div
                    key={bullet}
                    className="rounded-2xl border border-white/8 bg-white/[0.02] px-4 py-3 text-sm leading-6 text-gray-200"
                  >
                    {bullet}
                  </div>
                ))}
              </div>
            </div>
          ))}
        </div>
      </section>
    </div>
  );
}
