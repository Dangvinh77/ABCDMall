import { useEffect, useMemo, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { CheckCircle2, Clock3, Film, Loader2, Mail, MapPin, RefreshCw, ShieldCheck, Ticket } from 'lucide-react';
import { Button } from '../component/ui/button';
import { Badge } from '../component/ui/badge';
import { fetchBookingDetail, fetchShowtimeDetail, type BookingDetailModel, type ShowtimeDetailModel } from '../api/moviesApi';
import { moviePaths } from '../routes/moviePaths';
import { vnd } from '../data/booking';

function getErrorMessage(error: unknown) {
  return error instanceof Error ? error.message : 'Unable to load payment confirmation.';
}

export function MoviePaymentSuccessPage() {
  const [searchParams] = useSearchParams();
  const bookingCode = searchParams.get('bookingCode') ?? '';
  const sessionId = searchParams.get('session_id') ?? '';
  const [booking, setBooking] = useState<BookingDetailModel | null>(null);
  const [showtime, setShowtime] = useState<ShowtimeDetailModel | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const isConfirmed = booking?.status.toLowerCase() === 'confirmed';

  useEffect(() => {
    if (!bookingCode) {
      setLoading(false);
      setError('Missing booking code in Stripe return URL.');
      return;
    }

    let active = true;
    let timerId: number | undefined;

    async function loadBooking(isBackgroundRefresh: boolean) {
      if (!active) {
        return;
      }

      if (!isBackgroundRefresh) {
        setLoading(true);
      }

      try {
        const bookingDetail = await fetchBookingDetail(bookingCode);
        if (!active) {
          return;
        }

        setBooking(bookingDetail);
        setError(null);

        if (!showtime || showtime.showtimeId !== bookingDetail.showtimeId) {
          const showtimeDetail = await fetchShowtimeDetail(bookingDetail.showtimeId);
          if (active) {
            setShowtime(showtimeDetail);
          }
        }
      } catch (loadError) {
        if (active) {
          setError(getErrorMessage(loadError));
        }
      } finally {
        if (active) {
          setLoading(false);
        }
      }
    }

    void loadBooking(false);

    timerId = window.setInterval(() => {
      if (!isConfirmed) {
        void loadBooking(true);
      }
    }, 3000);

    return () => {
      active = false;
      if (timerId) {
        window.clearInterval(timerId);
      }
    };
  }, [bookingCode, isConfirmed, showtime]);

  const statusBadge = useMemo(() => {
    if (isConfirmed) {
      return (
        <Badge className="bg-emerald-600/20 text-emerald-300 ring-1 ring-emerald-500/30">
          <CheckCircle2 className="mr-1.5 size-3.5" />
          Payment confirmed
        </Badge>
      );
    }

    return (
      <Badge className="bg-amber-600/20 text-amber-300 ring-1 ring-amber-500/30">
        <Clock3 className="mr-1.5 size-3.5" />
        Waiting for webhook
      </Badge>
    );
  }, [isConfirmed]);

  return (
    <div className="min-h-screen bg-[#07091a] px-4 py-10 text-white sm:px-6">
      <div className="mx-auto max-w-3xl">
        <div className="mb-6 flex items-center justify-between gap-3">
          <div className="flex items-center gap-3">
            <div className="rounded-full bg-gradient-to-br from-purple-600 to-pink-600 p-2">
              <Film className="size-5 text-white" />
            </div>
            <div>
              <p className="text-sm font-semibold text-white">ABCD Cinema</p>
              <p className="text-xs text-gray-500">Stripe return page</p>
            </div>
          </div>
          {statusBadge}
        </div>

        <div className="overflow-hidden rounded-3xl border border-white/[0.08] bg-gradient-to-b from-slate-900/90 to-[#0e1128] shadow-2xl ring-1 ring-inset ring-white/[0.04]">
          <div className="border-b border-white/[0.06] px-6 py-6 text-center">
            <div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-gradient-to-br from-emerald-500 to-teal-600 shadow-xl shadow-emerald-500/25">
              {isConfirmed ? (
                <CheckCircle2 className="size-8 text-white" strokeWidth={2.5} />
              ) : (
                <Loader2 className="size-8 animate-spin text-white" />
              )}
            </div>
            <h1 className="text-3xl font-bold text-white">
              {isConfirmed ? 'Payment successful' : 'Waiting for Stripe confirmation'}
            </h1>
            <p className="mt-2 text-sm text-gray-400">
              {isConfirmed
                ? 'Your booking is confirmed and the ticket email will be sent by the backend.'
                : 'The redirect is complete. We are waiting for the Stripe webhook to finish confirming your booking.'}
            </p>
          </div>

          <div className="space-y-6 px-6 py-6">
            {loading ? (
              <div className="flex items-center justify-center gap-3 rounded-2xl bg-white/[0.03] px-4 py-6 text-sm text-gray-300 ring-1 ring-white/[0.05]">
                <Loader2 className="size-4 animate-spin text-purple-400" />
                Loading booking details...
              </div>
            ) : error ? (
              <div className="rounded-2xl border border-red-500/20 bg-red-950/20 px-4 py-4 text-sm text-red-200">
                {error}
              </div>
            ) : (
              <>
                <div className="grid gap-4 md:grid-cols-2">
                  <div className="rounded-2xl bg-white/[0.03] p-4 ring-1 ring-white/[0.05]">
                    <p className="text-xs font-semibold uppercase tracking-[0.2em] text-gray-500">Booking code</p>
                    <p className="mt-2 font-mono text-2xl font-bold tracking-[0.15em] text-white">{booking?.bookingCode}</p>
                    {sessionId ? (
                      <p className="mt-2 text-xs text-gray-500">Stripe session: {sessionId}</p>
                    ) : null}
                  </div>
                  <div className="rounded-2xl bg-white/[0.03] p-4 ring-1 ring-white/[0.05]">
                    <p className="text-xs font-semibold uppercase tracking-[0.2em] text-gray-500">Amount</p>
                    <p className="mt-2 text-2xl font-bold text-white">{vnd(booking?.grandTotal ?? 0)}</p>
                    <p className="mt-2 text-xs text-gray-500">Status: {booking?.status ?? 'Unknown'}</p>
                  </div>
                </div>

                <div className="grid gap-4 md:grid-cols-2">
                  <div className="rounded-2xl bg-white/[0.03] p-4 ring-1 ring-white/[0.05]">
                    <p className="mb-3 text-xs font-semibold uppercase tracking-[0.2em] text-gray-500">Customer</p>
                    <div className="space-y-2 text-sm text-gray-300">
                      <p>{booking?.customerName}</p>
                      <p className="flex items-center gap-2">
                        <Mail className="size-4 text-purple-400" />
                        {booking?.customerEmail}
                      </p>
                    </div>
                  </div>
                  <div className="rounded-2xl bg-white/[0.03] p-4 ring-1 ring-white/[0.05]">
                    <p className="mb-3 text-xs font-semibold uppercase tracking-[0.2em] text-gray-500">Showtime</p>
                    <div className="space-y-2 text-sm text-gray-300">
                      <p className="font-medium text-white">{showtime?.movieTitle ?? 'Updating movie info...'}</p>
                      <p className="flex items-center gap-2">
                        <MapPin className="size-4 text-pink-400" />
                        {showtime?.cinemaName ?? 'Updating cinema info...'}
                      </p>
                      <p className="flex items-center gap-2">
                        <Ticket className="size-4 text-cyan-400" />
                        {showtime?.hallName ?? 'Updating hall info...'}
                      </p>
                    </div>
                  </div>
                </div>

                {!isConfirmed ? (
                  <div className="rounded-2xl border border-amber-500/20 bg-amber-950/20 px-4 py-4 text-sm text-amber-100">
                    Do not repay this booking. Keep this page open for a few seconds while the webhook reaches
                    `POST /api/payments/webhooks/stripe`.
                  </div>
                ) : (
                  <div className="rounded-2xl border border-emerald-500/20 bg-emerald-950/20 px-4 py-4 text-sm text-emerald-100">
                    Booking confirmed. The ticket email job can now use the confirmed payment record to send the PDF ticket.
                  </div>
                )}
              </>
            )}
          </div>

          <div className="flex flex-col gap-3 border-t border-white/[0.06] px-6 py-5 sm:flex-row">
            <Button
              type="button"
              variant="outline"
              onClick={() => {
                if (bookingCode) {
                  void fetchBookingDetail(bookingCode).then(setBooking).catch((refreshError) => setError(getErrorMessage(refreshError)));
                }
              }}
              className="border-white/10 text-white hover:bg-white/[0.05]"
            >
              <RefreshCw className="mr-2 size-4" />
              Refresh status
            </Button>
            <Button asChild className="bg-gradient-to-r from-purple-600 to-pink-600 hover:from-purple-500 hover:to-pink-500">
              <Link to={moviePaths.home()}>Back to movies</Link>
            </Button>
          </div>
        </div>

        <p className="mt-5 flex items-center justify-center gap-2 text-center text-xs text-gray-500">
          <ShieldCheck className="size-4 text-emerald-500" />
          Payment confirmation is finalized by backend webhook verification, not by the frontend redirect.
        </p>
      </div>
    </div>
  );
}
