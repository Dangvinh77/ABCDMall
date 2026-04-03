import { useState, useEffect, useCallback, useMemo, type ReactNode } from 'react';
import { useNavigate, useParams, useSearchParams } from 'react-router-dom';
import {
  ArrowLeft,
  Clock,
  MapPin,
  Film,
  Ticket,
  ChevronRight,
  Mail,
  AlertCircle,
  Heart,
  Info,
} from 'lucide-react';
import { getMovieById, cinemasList } from '../data/movie';
import {
  SERVICE_FEE,
  HALL_NAMES,
  SNACK_COMBOS,
  getSnackComboById,
  getSeatPrice,
  SEAT_SURCHARGES,
  vnd,
  type SelectedSnackCombo,
  type SeatType,
} from '../data/booking';
import {
  evaluatePromotion,
  getDefaultBookingDate,
  getPromotionFinalTotal,
} from '../data/promotions';
import { Button } from '../component/ui/button';
import { Badge } from '../component/ui/badge';
type SeatStatus = 'available' | 'booked' | 'selected';

interface Seat {
  id: string;
  row: string;
  col: number;
  type: SeatType;
  status: SeatStatus;
}
const ROWS = ['A', 'B', 'C', 'D', 'E', 'F', 'G', 'H'];
const COLS = 12;
const AISLE_AFTER = 6;
const VIP_ROWS = new Set(['C', 'D', 'E']);
const COUPLE_ROW = 'H';

const BOOKED_SEATS = new Set([
  'A2', 'A3', 'A7', 'A8',
  'B5', 'B6', 'B11',
  'C3', 'C4', 'C9', 'C10',
  'D6', 'D7', 'D8',
  'E2', 'E3', 'E10', 'E11',
  'F5', 'F6',
  'G3', 'G8', 'G9',
  'H3', 'H4', 'H7', 'H8',
]);
function countdown(s: number) {
  const m = Math.floor(s / 60).toString().padStart(2, '0');
  const sec = (s % 60).toString().padStart(2, '0');
  return `${m}:${sec}`;
}

function buildSeats(): Seat[][] {
  return ROWS.map((row) =>
    Array.from({ length: COLS }, (_, i) => {
      const col = i + 1;
      const id = `${row}${col}`;
      const type: SeatType =
        row === COUPLE_ROW ? 'couple' : VIP_ROWS.has(row) ? 'vip' : 'regular';
      const status: SeatStatus = BOOKED_SEATS.has(id) ? 'booked' : 'available';
      return { id, row, col, type, status };
    })
  );
}
function SeatButton({
  seat,
  hallType,
  onClick,
}: {
  seat: Seat;
  hallType: string;
  onClick: () => void;
}) {
  const base =
    'relative flex items-center justify-center h-8 w-8 rounded-lg border text-[10px] font-semibold transition-all duration-150 select-none focus:outline-none';

  let variant: string;
  if (seat.status === 'booked') {
    variant =
      'bg-slate-800/30 border-slate-700/20 cursor-not-allowed opacity-20 text-slate-700';
  } else if (seat.status === 'selected') {
    if (seat.type === 'vip') {
      variant =
        'bg-gradient-to-b from-amber-400 to-amber-600 border-amber-300 shadow-lg shadow-amber-500/40 text-amber-950 scale-110 z-10';
    } else if (seat.type === 'couple') {
      variant =
        'bg-gradient-to-b from-rose-500 to-pink-600 border-rose-400 shadow-lg shadow-rose-500/40 text-white scale-110 z-10';
    } else {
      variant =
        'bg-gradient-to-b from-purple-500 to-pink-600 border-purple-400 shadow-lg shadow-purple-500/40 text-white scale-110 z-10';
    }
  } else {
    // available
    if (seat.type === 'vip') {
      variant =
        'bg-amber-950/60 border-amber-600/40 hover:border-amber-400 hover:bg-amber-900/60 hover:scale-110 text-amber-500 cursor-pointer';
    } else if (seat.type === 'couple') {
      variant =
        'bg-rose-950/60 border-rose-600/30 hover:border-rose-400 hover:bg-rose-900/50 hover:scale-110 text-rose-400 cursor-pointer';
    } else {
      variant =
        'bg-slate-700/50 border-slate-600/50 hover:border-purple-500 hover:bg-purple-900/30 hover:scale-110 text-slate-400 hover:text-purple-300 cursor-pointer';
    }
  }

  return (
    <button
      onClick={onClick}
      disabled={seat.status === 'booked'}
      className={`${base} ${variant}`}
      aria-label={`Seat ${seat.id}`}
      title={`${seat.id} - ${seat.type === 'regular' ? 'Regular' : seat.type === 'vip' ? 'VIP' : 'Couple'} - ${vnd(getSeatPrice(hallType, seat.type))}`}
    >
      {seat.col}
    </button>
  );
}
function LegendItem({
  boxClass,
  label,
  icon,
}: {
  boxClass: string;
  label: string;
  icon?: ReactNode;
}) {
  return (
    <div className="flex items-center gap-2">
      <div
        className={`h-7 w-7 shrink-0 rounded-lg border flex items-center justify-center text-[9px] font-bold ${boxClass}`}
      >
        {icon}
      </div>
      <span className="text-xs text-gray-400 whitespace-nowrap">{label}</span>
    </div>
  );
}
function InfoRow({
  icon,
  label,
  dim = false,
}: {
  icon: ReactNode;
  label: string;
  dim?: boolean;
}) {
  return (
    <div
      className={`flex items-start gap-1.5 ${dim ? 'text-[11px] text-gray-500' : 'text-xs text-gray-300'}`}
    >
      <span className="mt-px shrink-0">{icon}</span>
      <span className="line-clamp-1">{label}</span>
    </div>
  );
}
function getSelectedSnackCombos(comboQuantities: Record<string, number>): SelectedSnackCombo[] {
  return Object.entries(comboQuantities)
    .filter(([, quantity]) => quantity > 0)
    .map(([id, quantity]) => ({ id: id as SelectedSnackCombo['id'], quantity }));
}

export function SeatSelectionPage() {
  const { movieId } = useParams<{ movieId: string }>();
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();

  const cinemaId = searchParams.get('cinema') ?? 'abcd-mall';
  const showtime = searchParams.get('showtime') ?? '19:30';
  const hallType = searchParams.get('hallType') ?? '2D';
  const promoId = searchParams.get('promo');
  const bookingDate = searchParams.get('date') ?? getDefaultBookingDate();

  const movie = movieId ? getMovieById(movieId) : undefined;
  const cinema = cinemasList.find((c) => c.id === cinemaId);

  const [seats, setSeats] = useState<Seat[][]>(buildSeats);
  const [timeLeft, setTimeLeft] = useState(10 * 60);
  const [comboQuantities, setComboQuantities] = useState<Record<string, number>>({});

  useEffect(() => {
    if (timeLeft <= 0) return;
    const id = setInterval(() => setTimeLeft((t) => t - 1), 1000);
    return () => clearInterval(id);
  }, [timeLeft]);

  const toggleSeat = useCallback((clicked: Seat) => {
    if (clicked.status === 'booked') return;

    setSeats((prev) => {
      if (clicked.type === 'couple') {
        const pairCol = clicked.col % 2 === 0 ? clicked.col - 1 : clicked.col + 1;
        const flat = prev.flat();
        const partner = flat.find((s) => s.row === clicked.row && s.col === pairCol);
        const anySelected =
          clicked.status === 'selected' || partner?.status === 'selected';
        const next: SeatStatus = anySelected ? 'available' : 'selected';
        return prev.map((row) =>
          row.map((s) =>
            s.row === clicked.row &&
            (s.col === clicked.col || s.col === pairCol) &&
            s.status !== 'booked'
              ? { ...s, status: next }
              : s
          )
        );
      }
      return prev.map((row) =>
        row.map((s) =>
          s.id === clicked.id
            ? { ...s, status: s.status === 'selected' ? 'available' : 'selected' }
            : s
        )
      );
    });
  }, []);

  const selected = useMemo(() => seats.flat().filter((s) => s.status === 'selected'), [seats]);
  const subtotal = useMemo(
    () => selected.reduce((sum, seat) => sum + getSeatPrice(hallType, seat.type), 0),
    [hallType, selected]
  );
  const selectedSnackCombos = useMemo(
    () => getSelectedSnackCombos(comboQuantities),
    [comboQuantities]
  );
  const comboSubtotal = useMemo(
    () =>
      selectedSnackCombos.reduce((sum, selection) => {
        const combo = getSnackComboById(selection.id);
        return combo ? sum + combo.price * selection.quantity : sum;
      }, 0),
    [selectedSnackCombos]
  );
  const comboCount = useMemo(
    () => selectedSnackCombos.reduce((sum, combo) => sum + combo.quantity, 0),
    [selectedSnackCombos]
  );
  const serviceFee = selected.length * SERVICE_FEE;
  const isLow = timeLeft < 120;
  const bookingDateLabel = useMemo(
    () =>
      new Date(`${bookingDate}T00:00:00`).toLocaleDateString('en-US', {
        weekday: 'long',
        month: 'long',
        day: 'numeric',
        year: 'numeric',
      }),
    [bookingDate]
  );
  const promoEvaluation = useMemo(
    () =>
      evaluatePromotion({
        promoId,
        seats: selected.map((seat) => ({ id: seat.id, type: seat.type })),
        subtotal,
        serviceFee,
        hallType,
        showtime,
        bookingDate,
        snackCombos: selectedSnackCombos,
      }),
    [bookingDate, hallType, promoId, selected, selectedSnackCombos, serviceFee, showtime, subtotal]
  );
  const promoTotals = useMemo(
    () => getPromotionFinalTotal(subtotal + serviceFee, promoEvaluation, selectedSnackCombos),
    [promoEvaluation, selectedSnackCombos, serviceFee, subtotal]
  );

  const updateComboQuantity = useCallback((comboId: string, nextQuantity: number) => {
    setComboQuantities((prev) => {
      if (nextQuantity <= 0) {
        const { [comboId]: _removed, ...rest } = prev;
        return rest;
      }
      return { ...prev, [comboId]: Math.min(nextQuantity, 9) };
    });
  }, []);

  // Navigate to checkout with booking state
  const goToCheckout = () => {
    if (selected.length === 0) return;
    const params = new URLSearchParams({
      cinema: cinemaId,
      showtime,
      hallType,
      date: bookingDate,
    });

    if (promoId) {
      params.set('promo', promoId);
    }

    navigate(
      `/movie/${movieId}/checkout?${params.toString()}`,
      {
        state: {
          seats: selected.map((s) => ({ id: s.id, type: s.type })),
          subtotal,
          serviceFee,
          total: promoTotals.total,
          comboSubtotal,
          combos: selectedSnackCombos,
          promoId,
          bookingDate,
        },
      }
    );
  };

  // Couple row pair rendering
  const renderCoupleRow = (row: Seat[]) => {
    const left = row.slice(0, AISLE_AFTER);
    const right = row.slice(AISLE_AFTER);

    const pairs = (side: Seat[]) => {
      const result: Seat[][] = [];
      for (let i = 0; i < side.length; i += 2) {
        result.push([side[i], side[i + 1]].filter(Boolean));
      }
      return result;
    };

    const renderPairs = (pairList: Seat[][]) =>
      pairList.map((pair) => {
        const isAnySelected = pair.some((s) => s.status === 'selected');
        const isAllBooked = pair.every((s) => s.status === 'booked');
        return (
          <div
            key={pair[0]?.id}
            className={`flex gap-px rounded-xl p-0.5 ring-1 transition-all duration-200 ${
              isAnySelected
                ? 'ring-rose-500/70 bg-rose-950/40'
                : isAllBooked
                  ? 'ring-slate-700/20 bg-transparent'
                  : 'ring-rose-700/20 hover:ring-rose-500/50 bg-transparent'
            }`}
          >
            {pair.map((seat) => (
              <SeatButton
                key={seat.id}
                seat={seat}
                hallType={hallType}
                onClick={() => toggleSeat(seat)}
              />
            ))}
          </div>
        );
      });

    return (
      <div className="flex items-center gap-1.5">
        {renderPairs(pairs(left))}
        <div className="w-5 shrink-0" />
        {renderPairs(pairs(right))}
      </div>
    );
  };

  if (!movie || !cinema) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-[#07091a]">
        <div className="text-center">
          <h2 className="mb-4 text-2xl font-bold text-white">Booking details not found</h2>
          <Button
            onClick={() => navigate('/')}
            className="bg-gradient-to-r from-purple-600 to-pink-600"
          >
            Go to homepage
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-[#07091a] text-white">
      <header className="sticky top-0 z-50 border-b border-white/[0.06] bg-[#07091a]/95 backdrop-blur-2xl">
        <div className="container mx-auto flex h-14 items-center justify-between px-4">
          <button
            onClick={() => navigate(`/movie/${movieId}?${new URLSearchParams({ ...(promoId ? { promo: promoId } : {}), date: bookingDate }).toString()}`)}
            className="flex items-center gap-1.5 rounded-xl px-3 py-2 text-sm text-gray-400 transition-all hover:bg-white/[0.06] hover:text-white"
          >
            <ArrowLeft className="size-4" />
            <span className="hidden sm:inline">Back</span>
          </button>

          <div className="flex items-center gap-2">
            <div className="rounded-full bg-gradient-to-br from-purple-600 to-pink-600 p-1.5">
              <Film className="size-4 text-white" />
            </div>
            <span className="font-bold tracking-tight">ABCD Cinema</span>
          </div>

          {/* Countdown */}
          <div
            className={`flex items-center gap-1.5 rounded-full px-3 py-1.5 font-mono text-sm font-semibold ring-1 transition-all ${
              isLow
                ? 'bg-red-950/60 text-red-400 ring-red-500/40 animate-pulse'
                : 'bg-purple-950/50 text-purple-300 ring-purple-500/20'
            }`}
          >
            <Clock className="size-3.5" />
            {countdown(timeLeft)}
          </div>
        </div>
      </header>
      <main className="container mx-auto px-3 py-5 sm:px-4 sm:py-6 lg:flex lg:items-start lg:gap-6 xl:gap-8">
        <div className="flex-1 space-y-4 min-w-0">

          {/* Booking Info Card */}
          <div className="relative overflow-hidden rounded-2xl border border-purple-500/10 bg-gradient-to-br from-slate-900/90 to-[#0e1128] p-4 sm:p-5 shadow-xl ring-1 ring-inset ring-white/[0.04]">
            {/* Ambient glow */}
            <div className="pointer-events-none absolute -top-16 left-1/2 h-32 w-64 -translate-x-1/2 rounded-full bg-purple-600/10 blur-3xl" />

            <div className="flex gap-4">
              {/* Poster */}
              <div className="relative shrink-0">
                <img
                  src={movie.imageUrl}
                  alt={movie.title}
                  className="h-24 w-16 rounded-xl object-cover shadow-xl ring-1 ring-white/10 sm:h-28 sm:w-20"
                />
                <Badge className="absolute -right-1.5 -top-1.5 bg-gradient-to-r from-purple-600 to-pink-600 px-1.5 py-0.5 text-[10px]">
                  {hallType}
                </Badge>
              </div>

              {/* Details */}
              <div className="flex min-w-0 flex-1 flex-col justify-center gap-2">
                <h2 className="text-base font-bold text-white line-clamp-1 sm:text-lg">
                  {movie.title}
                </h2>
                <div className="space-y-1.5">
                  <InfoRow
                    icon={<MapPin className="size-3.5 text-purple-400" />}
                    label={cinema.name}
                  />
                  <InfoRow
                    icon={<Clock className="size-3.5 text-pink-400" />}
                    label={`${showtime} - ${bookingDateLabel}`}
                  />
                  <InfoRow
                    icon={<Film className="size-3.5 text-cyan-400" />}
                    label={HALL_NAMES[hallType] ?? hallType}
                  />
                  <InfoRow
                    icon={<MapPin className="size-3 text-gray-600" />}
                    label={cinema.address}
                    dim
                  />
                </div>
              </div>
            </div>

            {/* Low time warning */}
            {isLow && (
              <div className="mt-4 flex items-center gap-2 rounded-xl bg-red-950/50 px-3 py-2.5 text-sm text-red-400 ring-1 ring-red-500/20">
                <AlertCircle className="size-4 shrink-0" />
                Seats will be held for another{' '}
                <span className="font-bold">{countdown(timeLeft)}</span>. Please complete your booking soon!
              </div>
            )}
          </div>
          <div className="relative overflow-hidden rounded-2xl border border-white/[0.05] bg-gradient-to-b from-slate-900/70 to-[#090c1e] shadow-2xl">
            <div className="p-4 sm:p-6">
              {/* Seat hold notice */}
              <div className="mb-6 flex items-center justify-center gap-2 text-xs text-gray-500">
                <Info className="size-3.5 text-purple-400/60" />
                Seats are currently being held for showtime{' '}
                <span className="font-semibold text-purple-400">{showtime}</span>
              </div>

              {/* Screen indicator */}
              <div className="mb-10 flex flex-col items-center">
                <div
                  className="h-1.5 w-3/4 max-w-xs rounded-full"
                  style={{
                    background:
                      'linear-gradient(90deg, transparent 0%, rgba(139,92,246,0.6) 20%, rgba(236,72,153,0.55) 50%, rgba(139,92,246,0.6) 80%, transparent 100%)',
                    boxShadow:
                      '0 0 18px rgba(139,92,246,0.45), 0 0 40px rgba(139,92,246,0.15)',
                  }}
                />
                <div
                  className="mt-0.5 h-px w-4/5 max-w-sm"
                  style={{
                    background:
                      'linear-gradient(90deg, transparent, rgba(139,92,246,0.15) 35%, rgba(139,92,246,0.15) 65%, transparent)',
                  }}
                />
                <p className="mt-2.5 text-[10px] font-semibold uppercase tracking-[0.45em] text-gray-600">
                  Screen
                </p>
                {/* Perspective fan */}
                <div
                  className="mt-2 h-6 w-full max-w-md rounded-sm"
                  style={{
                    background:
                      'linear-gradient(to bottom, rgba(139,92,246,0.05) 0%, transparent 100%)',
                  }}
                />
              </div>

              {/* Scroll wrapper for small screens */}
              <div className="overflow-x-auto">
                <div className="inline-block min-w-[540px] w-full">
                  {/* Column header */}
                  <div className="mb-3 flex items-center gap-2">
                    <div className="w-7 shrink-0" /> {/* row label spacer */}
                    <div className="flex flex-1 items-center justify-center gap-1.5">
                      {Array.from({ length: AISLE_AFTER }, (_, i) => (
                        <div
                          key={i}
                          className="flex h-8 w-8 items-center justify-center text-[9px] text-gray-600"
                        >
                          {i + 1}
                        </div>
                      ))}
                      <div className="w-5" />
                      {Array.from({ length: COLS - AISLE_AFTER }, (_, i) => (
                        <div
                          key={i + AISLE_AFTER}
                          className="flex h-8 w-8 items-center justify-center text-[9px] text-gray-600"
                        >
                          {i + AISLE_AFTER + 1}
                        </div>
                      ))}
                    </div>
                    <div className="w-7 shrink-0" />
                  </div>

                  {/* Seat rows */}
                  <div className="space-y-2">
                    {seats.map((row) => {
                      const label = row[0].row;
                      const isCouple = label === COUPLE_ROW;
                      return (
                        <div key={label} className="flex items-center gap-2">
                          {/* Left label */}
                          <span className="w-7 shrink-0 text-right text-[11px] font-medium text-gray-500">
                            {label}
                          </span>

                          {/* Seats */}
                          <div className="flex flex-1 items-center justify-center">
                            {isCouple ? (
                              renderCoupleRow(row)
                            ) : (
                              <div className="flex items-center gap-1.5">
                                {row.slice(0, AISLE_AFTER).map((seat) => (
                                  <SeatButton
                                    key={seat.id}
                                    seat={seat}
                                    hallType={hallType}
                                    onClick={() => toggleSeat(seat)}
                                  />
                                ))}
                                <div className="w-5 shrink-0" />
                                {row.slice(AISLE_AFTER).map((seat) => (
                                  <SeatButton
                                    key={seat.id}
                                    seat={seat}
                                    hallType={hallType}
                                    onClick={() => toggleSeat(seat)}
                                  />
                                ))}
                              </div>
                            )}
                          </div>

                          {/* Right label */}
                          <span className="w-7 shrink-0 text-left text-[11px] font-medium text-gray-500">
                            {label}
                          </span>
                        </div>
                      );
                    })}
                  </div>
                </div>
              </div>
              <div className="mt-8 border-t border-white/[0.05] pt-6">
                <div className="flex flex-wrap items-center justify-center gap-x-5 gap-y-3">
                  <LegendItem
                    boxClass="bg-slate-700/50 border-slate-600/50 text-slate-500"
                    label="Available"
                  />
                  <LegendItem
                    boxClass="bg-gradient-to-b from-purple-500 to-pink-600 border-purple-400 shadow-md shadow-purple-500/30"
                    label="Selected"
                  />
                  <LegendItem
                    boxClass="bg-slate-800/30 border-slate-700/20 opacity-30"
                    label="Booked"
                  />
                  <LegendItem
                    boxClass="bg-amber-950/70 border-amber-600/40 text-amber-400"
                    label={`VIP (+${Math.round(SEAT_SURCHARGES.vip / 1000)}K)`}
                    icon={<span>*</span>}
                  />
                  <LegendItem
                    boxClass="bg-rose-950/60 border-rose-600/30 text-rose-400"
                    label={`Couple seat (+${Math.round(SEAT_SURCHARGES.couple / 1000)}K)`}
                    icon={<Heart className="size-3 fill-current" />}
                  />
                </div>
              </div>
            </div>
          </div>
        </div>
        <div className="mt-4 shrink-0 lg:mt-0 lg:w-[320px] xl:w-[360px] lg:sticky lg:top-[4.5rem]">
          <div className="relative overflow-hidden rounded-2xl border border-white/[0.06] bg-gradient-to-b from-slate-900/90 to-[#0e1128] p-5 shadow-2xl ring-1 ring-inset ring-white/[0.04]">
            {/* Top glow */}
            <div className="pointer-events-none absolute -top-10 right-0 h-24 w-32 rounded-full bg-pink-600/10 blur-2xl" />

            {/* Title */}
            <div className="mb-4 flex items-center justify-between">
              <h3 className="font-bold text-white">Booking summary</h3>
              <Badge className="bg-purple-600/20 text-purple-300 ring-1 ring-purple-500/30 text-xs">
                {selected.length} seats
              </Badge>
            </div>

            {/* Movie snapshot */}
            <div className="mb-4 space-y-1.5 rounded-xl bg-white/[0.04] p-3 ring-1 ring-white/[0.04]">
              <p className="font-semibold text-white line-clamp-1 text-sm">{movie.title}</p>
              <div className="space-y-1">
                <p className="flex items-center gap-1.5 text-xs text-gray-400">
                  <MapPin className="size-3 shrink-0 text-purple-400" />
                  {cinema.name}
                </p>
                <p className="flex items-center gap-1.5 text-xs text-gray-400">
                  <Clock className="size-3 shrink-0 text-pink-400" />
                  {showtime} &nbsp;-&nbsp; {bookingDateLabel}
                </p>
                <p className="flex items-center gap-1.5 text-xs text-gray-400">
                  <Film className="size-3 shrink-0 text-cyan-400" />
                  {HALL_NAMES[hallType] ?? hallType}
                </p>
              </div>
            </div>

            {/* Selected seats */}
            {selected.length > 0 ? (
              <div className="mb-4 space-y-2">
                <p className="text-[10px] font-semibold uppercase tracking-wider text-gray-500">
                  Selected seats
                </p>
                <div className="flex flex-wrap gap-1.5">
                  {selected.map((s) => (
                    <span
                      key={s.id}
                      className={`rounded-lg px-2 py-1 text-xs font-semibold ring-1 ${
                        s.type === 'vip'
                          ? 'bg-amber-950/60 text-amber-400 ring-amber-500/30'
                          : s.type === 'couple'
                            ? 'bg-rose-950/60 text-rose-400 ring-rose-500/30'
                            : 'bg-purple-950/60 text-purple-300 ring-purple-500/30'
                      }`}
                    >
                      {s.id}
                    </span>
                  ))}
                </div>
              </div>
            ) : (
              <div className="mb-4 rounded-xl border border-dashed border-gray-700/60 py-6 text-center">
                <Ticket className="mx-auto mb-2 size-8 text-gray-700" />
                <p className="text-sm text-gray-500">No seats selected yet</p>
                <p className="mt-0.5 text-[11px] text-gray-600">
                  Tap the seating map to choose your seats
                </p>
              </div>
            )}

            {selectedSnackCombos.length > 0 && (
              <div className="mb-4 space-y-2">
                <p className="text-[10px] font-semibold uppercase tracking-wider text-gray-500">
                  Selected combos
                </p>
                <div className="space-y-2">
                  {selectedSnackCombos.map((selection) => {
                    const combo = getSnackComboById(selection.id);
                    if (!combo) return null;
                    return (
                      <div
                        key={selection.id}
                        className="flex items-center justify-between rounded-xl bg-white/[0.03] px-3 py-2 text-sm ring-1 ring-white/[0.05]"
                      >
                        <div>
                          <p className="font-medium text-white">{combo.name}</p>
                          <p className="text-xs text-gray-500">
                            {selection.quantity} x {vnd(combo.price)}
                          </p>
                        </div>
                        <span className="font-medium text-amber-300">
                          {vnd(combo.price * selection.quantity)}
                        </span>
                      </div>
                    );
                  })}
                </div>
              </div>
            )}

            {promoEvaluation && (
              <div className="mb-4 rounded-2xl border border-fuchsia-500/20 bg-fuchsia-950/20 p-4 ring-1 ring-fuchsia-500/10">
                <div className="flex items-start justify-between gap-3">
                  <div>
                    <p className="text-[10px] font-semibold uppercase tracking-widest text-fuchsia-300/80">
                      Selected promotion
                    </p>
                    <p className="mt-1 text-sm font-semibold text-white">{promoEvaluation.promo.title}</p>
                    <p className="mt-1 text-xs text-fuchsia-100/75">{promoEvaluation.message}</p>
                    {promoEvaluation.bonusLabel && (
                      <p className="mt-2 text-xs font-medium text-amber-300">{promoEvaluation.bonusLabel}</p>
                    )}
                  </div>
                  <Badge
                    className={`text-xs ${
                      promoEvaluation.status === 'applied'
                        ? 'bg-emerald-600/20 text-emerald-300 ring-1 ring-emerald-500/30'
                        : promoEvaluation.status === 'ineligible'
                          ? 'bg-red-600/20 text-red-300 ring-1 ring-red-500/30'
                          : 'bg-fuchsia-600/20 text-fuchsia-200 ring-1 ring-fuchsia-500/30'
                    }`}
                  >
                    {promoEvaluation.status === 'applied'
                      ? 'Applied'
                      : promoEvaluation.status === 'ineligible'
                        ? 'Not eligible'
                        : 'Reserved'}
                  </Badge>
                </div>
              </div>
            )}

            {/* Price breakdown */}
            {selected.length > 0 && (
              <div className="mb-4 space-y-2 border-t border-white/[0.06] pt-4">
                {(['regular', 'vip', 'couple'] as SeatType[]).map((type) => {
                  const ts = selected.filter((s) => s.type === type);
                  if (!ts.length) return null;
                  const label =
                    type === 'regular'
                      ? 'Regular seat'
                      : type === 'vip'
                        ? 'VIP seat'
                        : 'Couple seat';
                  return (
                    <div key={type} className="flex items-center justify-between text-sm">
                      <span className="text-gray-400">
                        {label} x {ts.length}
                      </span>
                      <span className="font-medium text-white">
                        {vnd(getSeatPrice(hallType, type) * ts.length)}
                      </span>
                    </div>
                  );
                })}
                <div className="flex items-center justify-between text-sm">
                  <span className="text-gray-400">Service fee x {selected.length}</span>
                  <span className="font-medium text-white">{vnd(serviceFee)}</span>
                </div>
                {selectedSnackCombos.map((selection) => {
                  const combo = getSnackComboById(selection.id);
                  if (!combo) return null;
                  return (
                    <div key={selection.id} className="flex items-center justify-between text-sm">
                      <span className="text-gray-400">
                        {combo.name} x {selection.quantity}
                      </span>
                      <span className="font-medium text-white">
                        {vnd(combo.price * selection.quantity)}
                      </span>
                    </div>
                  );
                })}
                {promoEvaluation && promoEvaluation.estimatedDiscount > 0 && (
                  <div className="flex items-center justify-between text-sm">
                    <span className="text-fuchsia-200/80">
                      {promoEvaluation.status === 'applied' ? 'Discount' : 'Estimated savings'}
                    </span>
                    <span className="font-medium text-fuchsia-200">
                      -{vnd(promoEvaluation.status === 'applied' ? promoEvaluation.discount : promoEvaluation.estimatedDiscount)}
                    </span>
                  </div>
                )}
              </div>
            )}

            {/* Total */}
            <div className="mb-5 flex items-center justify-between rounded-2xl bg-gradient-to-r from-purple-950/60 to-pink-950/40 px-4 py-3.5 ring-1 ring-purple-500/20">
              <span className="font-semibold text-white">Total</span>
              <span className="bg-gradient-to-r from-purple-300 to-pink-300 bg-clip-text text-xl font-bold text-transparent">
                {vnd(promoTotals.total)}
              </span>
            </div>

            {/* CTA */}
            <Button
              disabled={selected.length === 0}
              onClick={goToCheckout}
              className="h-12 w-full bg-gradient-to-r from-purple-600 to-pink-600 text-base font-semibold hover:from-purple-500 hover:to-pink-500 disabled:cursor-not-allowed disabled:opacity-40 shadow-lg shadow-purple-900/30"
              size="lg"
            >
              <Ticket className="mr-2 size-5" />
              Continue
              <ChevronRight className="ml-2 size-5" />
            </Button>

            {/* Email notice */}
            <p className="mt-3 flex items-start justify-center gap-1.5 text-center text-[11px] text-gray-500">
              <Mail className="mt-px size-3 shrink-0 text-gray-600" />
              Your e-ticket will be sent by email after successful payment
            </p>
          </div>
        </div>
      </main>

      <section className="container mx-auto px-3 pb-6 sm:px-4 sm:pb-8">
        <div className="relative overflow-hidden rounded-2xl border border-amber-500/10 bg-gradient-to-br from-slate-900/90 to-[#120d1d] p-4 sm:p-5 shadow-xl ring-1 ring-inset ring-white/[0.04]">
          <div className="pointer-events-none absolute -top-14 right-0 h-28 w-40 rounded-full bg-amber-500/10 blur-3xl" />
          <div className="mb-5 flex items-center justify-between gap-3">
            <div>
              <p className="text-[10px] font-semibold uppercase tracking-[0.35em] text-amber-300/70">
                Snacks & drinks
              </p>
              <h3 className="mt-2 text-lg font-bold text-white">Add popcorn combos</h3>
              <p className="mt-1 text-sm text-gray-400">
                Choose your snacks now so snack promotions can be applied before checkout.
              </p>
            </div>
            <Badge className="bg-amber-500/15 text-amber-300 ring-1 ring-amber-500/25">
              {comboCount} items
            </Badge>
          </div>

          <div className="space-y-3">
            {SNACK_COMBOS.map((combo) => {
              const quantity = comboQuantities[combo.id] ?? 0;
              return (
                <div
                  key={combo.id}
                  className="rounded-2xl border border-white/[0.06] bg-white/[0.03] p-4 ring-1 ring-white/[0.03]"
                >
                  <div className="flex items-start justify-between gap-4">
                    <div className="min-w-0">
                      <div className="flex flex-wrap items-center gap-2">
                        <p className="text-sm font-semibold text-white">{combo.name}</p>
                        {combo.promoPrice && (
                          <Badge className="bg-emerald-500/15 text-emerald-300 ring-1 ring-emerald-500/25">
                            Promo price
                          </Badge>
                        )}
                      </div>
                      <p className="mt-1 text-xs leading-relaxed text-gray-400">{combo.description}</p>
                      <div className="mt-3 flex items-center gap-2 text-sm">
                        <span className="font-semibold text-amber-300">
                          {vnd(combo.promoPrice ?? combo.price)}
                        </span>
                        {combo.promoPrice && (
                          <span className="text-xs text-gray-500 line-through">
                            {vnd(combo.price)}
                          </span>
                        )}
                      </div>
                    </div>

                    <div className="flex shrink-0 items-center gap-2 rounded-full border border-white/[0.08] bg-[#0b1024] px-2 py-1">
                      <button
                        type="button"
                        onClick={() => updateComboQuantity(combo.id, quantity - 1)}
                        className="flex h-8 w-8 items-center justify-center rounded-full text-lg text-gray-300 transition hover:bg-white/[0.06] hover:text-white"
                        aria-label={`Decrease ${combo.name}`}
                      >
                        -
                      </button>
                      <span className="w-6 text-center text-sm font-semibold text-white">{quantity}</span>
                      <button
                        type="button"
                        onClick={() => updateComboQuantity(combo.id, quantity + 1)}
                        className="flex h-8 w-8 items-center justify-center rounded-full text-lg text-gray-300 transition hover:bg-white/[0.06] hover:text-white"
                        aria-label={`Increase ${combo.name}`}
                      >
                        +
                      </button>
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      </section>
      <div className="fixed inset-x-0 bottom-0 z-40 border-t border-white/[0.06] bg-[#07091a]/95 px-4 py-3 backdrop-blur-2xl lg:hidden">
        <div className="flex items-center justify-between gap-3">
          <div className="min-w-0">
            {selected.length > 0 ? (
              <>
                <p className="truncate text-xs text-gray-400">
                  {selected.map((s) => s.id).join(', ')}
                </p>
                <p className="text-lg font-bold text-white">{vnd(promoTotals.total)}</p>
                {comboCount > 0 && (
                  <p className="text-[11px] text-amber-300">{comboCount} combo item(s) included</p>
                )}
              </>
            ) : (
              <p className="text-sm text-gray-500">No seats selected</p>
            )}
          </div>
          <Button
            disabled={selected.length === 0}
            onClick={goToCheckout}
            className="shrink-0 bg-gradient-to-r from-purple-600 to-pink-600 hover:from-purple-500 hover:to-pink-500 disabled:opacity-40 shadow-lg shadow-purple-900/30"
          >
            Continue
            <ChevronRight className="ml-1.5 size-4" />
          </Button>
        </div>
      </div>

      {/* Spacer for mobile sticky bar */}
      <div className="h-20 lg:hidden" />
    </div>
  );
}



