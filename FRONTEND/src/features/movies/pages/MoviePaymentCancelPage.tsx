import { Link, useSearchParams } from 'react-router-dom';
import { ArrowLeft, CircleX, CreditCard, Film, Ticket } from 'lucide-react';
import { Button } from '../component/ui/button';
import { Badge } from '../component/ui/badge';
import { moviePaths } from '../routes/moviePaths';

export function MoviePaymentCancelPage() {
  const [searchParams] = useSearchParams();
  const bookingCode = searchParams.get('bookingCode') ?? '';
  const holdId = searchParams.get('holdId') ?? '';

  return (
    <div className="min-h-screen bg-[#07091a] px-4 py-10 text-white sm:px-6">
      <div className="mx-auto max-w-2xl overflow-hidden rounded-3xl border border-white/[0.08] bg-gradient-to-b from-slate-900/90 to-[#0e1128] shadow-2xl ring-1 ring-inset ring-white/[0.04]">
        <div className="flex items-center justify-between border-b border-white/[0.06] px-6 py-5">
          <div className="flex items-center gap-3">
            <div className="rounded-full bg-gradient-to-br from-purple-600 to-pink-600 p-2">
              <Film className="size-5 text-white" />
            </div>
            <div>
              <p className="text-sm font-semibold text-white">ABCD Cinema</p>
              <p className="text-xs text-gray-500">Stripe checkout cancelled</p>
            </div>
          </div>
          <Badge className="bg-red-600/20 text-red-300 ring-1 ring-red-500/30">
            <CircleX className="mr-1.5 size-3.5" />
            Payment cancelled
          </Badge>
        </div>

        <div className="px-6 py-8 text-center">
          <div className="mx-auto mb-5 flex h-16 w-16 items-center justify-center rounded-full bg-red-600/15 ring-1 ring-red-500/20">
            <CircleX className="size-8 text-red-300" />
          </div>
          <h1 className="text-3xl font-bold text-white">Payment was not completed</h1>
          <p className="mt-3 text-sm text-gray-400">
            Stripe returned the customer to the site before a successful charge. The booking remains unpaid.
          </p>

          {(bookingCode || holdId) && (
            <div className="mt-6 grid gap-4 rounded-2xl bg-white/[0.03] p-4 text-left ring-1 ring-white/[0.05] sm:grid-cols-2">
              <div>
                <p className="text-xs font-semibold uppercase tracking-[0.2em] text-gray-500">Booking code</p>
                <p className="mt-2 font-mono text-lg font-semibold text-white">{bookingCode || 'Not available'}</p>
              </div>
              <div>
                <p className="text-xs font-semibold uppercase tracking-[0.2em] text-gray-500">Hold reference</p>
                <p className="mt-2 break-all text-sm text-gray-300">{holdId || 'Not available'}</p>
              </div>
            </div>
          )}

          <div className="mt-6 rounded-2xl border border-amber-500/20 bg-amber-950/20 px-4 py-4 text-left text-sm text-amber-100">
            The hold may stay active for a short time while Stripe closes the session. If the session expires, the backend webhook
            will release the hold automatically.
          </div>
        </div>

        <div className="flex flex-col gap-3 border-t border-white/[0.06] px-6 py-5 sm:flex-row">
          <Button asChild className="bg-gradient-to-r from-purple-600 to-pink-600 hover:from-purple-500 hover:to-pink-500">
            <Link to={moviePaths.home()}>
              <Ticket className="mr-2 size-4" />
              Choose another showtime
            </Link>
          </Button>
          <Button asChild variant="outline" className="border-white/10 text-white hover:bg-white/[0.05]">
            <Link to={moviePaths.home()}>
              <ArrowLeft className="mr-2 size-4" />
              Back to movies
            </Link>
          </Button>
        </div>

        <div className="border-t border-white/[0.06] px-6 py-4 text-xs text-gray-500">
          <p className="flex items-center gap-2">
            <CreditCard className="size-4 text-cyan-400" />
            When Stripe is the active provider, the real payment result will only be trusted after webhook verification.
          </p>
        </div>
      </div>
    </div>
  );
}
