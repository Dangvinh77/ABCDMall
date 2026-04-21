import { useEffect, useState } from "react";
import { Badge } from "../../movies/component/ui/badge";
import { type MoviesAdminEmailLog, moviesAdminApi } from "../services/moviesAdminApi";

export function MoviesAdminEmailsPage() {
  const [logs, setLogs] = useState<MoviesAdminEmailLog[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [sendingBookingId, setSendingBookingId] = useState<string | null>(null);

  async function loadLogs() {
    try {
      setLoading(true);
      setError("");
      setLogs(await moviesAdminApi.getEmailLogs());
    } catch (err) {
      setError(err instanceof Error ? err.message : "Unable to load email logs.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void loadLogs();
  }, []);

  async function handleResend(bookingId: string) {
    try {
      setSendingBookingId(bookingId);
      setError("");
      await moviesAdminApi.resendTicketEmail(bookingId);
      await loadLogs();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Unable to resend ticket email.");
    } finally {
      setSendingBookingId(null);
    }
  }

  return (
    <div className="space-y-6">
      <section className="rounded-[2rem] border border-white/8 bg-[radial-gradient(circle_at_top_left,rgba(34,211,238,0.12),transparent_24%),linear-gradient(180deg,rgba(255,255,255,0.05),rgba(255,255,255,0.02))] p-6 shadow-[0_24px_80px_rgba(2,6,23,0.34)]">
        <div className="flex items-start justify-between gap-4">
          <div>
            <p className="text-xs font-semibold uppercase tracking-[0.28em] text-cyan-200/70">Emails</p>
            <h1 className="mt-2 text-3xl font-black uppercase tracking-[0.08em] text-white">Ticket email delivery</h1>
          </div>
          <Badge className="border border-fuchsia-400/20 bg-fuchsia-500/10 px-3 py-1.5 text-fuchsia-200">{logs.length} logs</Badge>
        </div>
      </section>
      {error ? <div className="rounded-3xl border border-red-500/20 bg-red-500/10 px-4 py-3 text-sm text-red-200">{error}</div> : null}
      <div className="overflow-hidden rounded-[2rem] border border-white/8 bg-white/[0.03] shadow-[0_24px_80px_rgba(2,6,23,0.34)]">
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-white/8 text-left text-sm">
            <thead className="bg-white/[0.04]"><tr>{["Booking", "Email", "Movie", "Delivery", "Outbox", "Error", "Actions"].map((c)=><th key={c} className="px-4 py-3 text-xs font-semibold uppercase tracking-[0.18em] text-gray-500">{c}</th>)}</tr></thead>
            <tbody className="divide-y divide-white/8">
              {loading ? <tr><td colSpan={7} className="px-4 py-6 text-gray-400">Loading email logs...</td></tr> : null}
              {!loading && logs.map((log) => (
                <tr key={log.bookingId} className="bg-white/[0.02]">
                  <td className="px-4 py-3 text-white">{log.bookingCode}</td>
                  <td className="px-4 py-3 text-gray-300">{log.customerEmail}</td>
                  <td className="px-4 py-3 text-gray-300">{log.movieTitle}</td>
                  <td className="px-4 py-3 text-gray-300">{log.deliveryStatus}</td>
                  <td className="px-4 py-3 text-gray-300">{log.outboxStatus} ({log.outboxRetryCount})</td>
                  <td className="px-4 py-3 text-gray-300">{log.emailSendError ?? log.outboxLastError ?? "-"}</td>
                  <td className="px-4 py-3">
                    <button className="text-cyan-200" disabled={sendingBookingId === log.bookingId} onClick={() => void handleResend(log.bookingId)}>
                      {sendingBookingId === log.bookingId ? "Sending..." : "Re-send"}
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
