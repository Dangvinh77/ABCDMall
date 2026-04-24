import { useState, useEffect, useCallback, useMemo, useRef, type ReactNode } from 'react';
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
import {
  SERVICE_FEE,
  HALL_NAMES,
  getSeatPrice,
  SEAT_SURCHARGES,
  vnd,
  type SelectedSnackCombo,
  type SeatType,
} from '../data/booking';
import {
  getDefaultBookingDate,
} from '../data/promotions';
import { Button } from '../component/ui/button';
import { Badge } from '../component/ui/badge';
import { moviePaths } from '../routes/moviePaths';
import {
  createBookingHold,
  fetchMovieDetail,
  fetchPromotions,
  fetchSeatMap,
  fetchSnackCombos,
  fetchShowtimeDetail,
  quoteBooking,
  releaseBookingHold,
  resolvePromotionApiIdFromUiId,
  type BookingHoldModel,
  type BookingQuoteModel,
  type MovieDetailModel,
  type PromotionModel,
  type PromotionRuleModel,
  type SnackComboModel,
  type ShowtimeDetailModel,
} from '../api/moviesApi';
type SeatStatus = 'available' | 'held' | 'booked' | 'selected';

const HELD_STATUS_CLASSES =
  'bg-blue-700/45 border-blue-400 text-blue-100 ring-1 ring-inset ring-blue-300/70 shadow-[0_0_0_1px_rgba(59,130,246,0.22)]';

interface ComboOption {
  id: string;
  apiId?: string;
  name: string;
  description: string;
  price: number;
  originalPrice?: number;
}

interface Seat {
  id: string;
  seatInventoryId?: string;
  coupleGroupCode?: string;
  row: string;
  col: number;
  type: SeatType;
  status: SeatStatus;
}
const COLS = 12;
const AISLE_AFTER = 6;
const COUPLE_ROW = 'H';
const CINEMA_TIME_ZONE = 'Asia/Ho_Chi_Minh';

function countdown(s: number) {
  const m = Math.floor(s / 60).toString().padStart(2, '0');
  const sec = (s % 60).toString().padStart(2, '0');
  return `${m}:${sec}`;
}

function getRemainingSeconds(expiresAtUtc: string) {
  return Math.max(0, Math.floor((new Date(expiresAtUtc).getTime() - Date.now()) / 1000));
}

function findLinkedCoupleSeats(clicked: Seat, allSeats: Seat[]): Seat[] {
  if (clicked.type !== 'couple') {
    return [clicked];
  }

  if (clicked.coupleGroupCode) {
    const linkedSeats = allSeats
      .filter(
        (seat) =>
          seat.type === 'couple' && seat.coupleGroupCode === clicked.coupleGroupCode,
      )
      .sort((left, right) => left.col - right.col);

    if (linkedSeats.length > 0) {
      return linkedSeats;
    }
  }

  if (clicked.row !== COUPLE_ROW) {
    return [clicked];
  }

  const pairStartCol = clicked.col % 2 === 0 ? clicked.col - 1 : clicked.col;
  const linkedSeats = allSeats
    .filter(
      (seat) =>
        seat.row === clicked.row &&
        seat.type === 'couple' &&
        seat.col >= pairStartCol &&
        seat.col < pairStartCol + 2,
    )
    .sort((left, right) => left.col - right.col);

  return linkedSeats.length > 0 ? linkedSeats : [clicked];
}

function mapHoldSeatIdsToUiSeatIds(hold: BookingHoldModel, allSeats: Seat[]): string[] {
  const holdSeatInventoryIds = new Set(
    (hold.seats ?? []).map((seat) => seat.seatInventoryId).filter(Boolean),
  );

  return allSeats
    .filter(
      (seat) => seat.seatInventoryId && holdSeatInventoryIds.has(seat.seatInventoryId),
    )
    .map((seat) => seat.id);
}

function buildSeatsFromApi(seatMap: Awaited<ReturnType<typeof fetchSeatMap>>): Seat[][] {
  const rows = new Map<string, Seat[]>();

  seatMap.seats.forEach((seat) => {
    const nextSeat: Seat = {
      id: seat.seatCode,
      seatInventoryId: seat.seatInventoryId,
      coupleGroupCode: seat.coupleGroupCode,
      row: seat.row,
      col: seat.col,
      type: seat.seatType,
      status: seat.status,
    };

    rows.set(seat.row, [...(rows.get(seat.row) ?? []), nextSeat]);
  });

  return Array.from(rows.entries())
    .sort(([left], [right]) => left.localeCompare(right))
    .map(([, rowSeats]) => rowSeats.sort((left, right) => left.col - right.col));
}

function findComboOption(comboOptions: ComboOption[], comboId: string) {
  return comboOptions.find((combo) => combo.id === comboId) ?? null;
}

function mergeSeatMapWithSelection(
  previousSeats: Seat[][],
  nextSeats: Seat[][],
  conflictedSeatIds: Set<string>,
): Seat[][] {
  const selectedSeatIds = new Set(
    previousSeats.flat().filter((seat) => seat.status === 'selected').map((seat) => seat.id),
  );

  return nextSeats.map((row) =>
    row.map((seat) => {
      if (conflictedSeatIds.has(seat.id)) {
        return seat.status === 'available'
          ? { ...seat, status: 'held' as const }
          : seat;
      }

      return seat.status === 'available' && selectedSeatIds.has(seat.id)
        ? { ...seat, status: 'selected' as const }
        : seat;
    }),
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
      'bg-slate-900 border-slate-500/70 cursor-not-allowed text-slate-200';
  } else if (seat.status === 'held') {
    variant = `${HELD_STATUS_CLASSES} cursor-not-allowed`;
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
        'bg-amber-950/60 border-amber-600/40 hover:border-amber-400 hover:bg-amber-900/60 text-amber-500 cursor-pointer';
    } else if (seat.type === 'couple') {
      variant =
        'bg-rose-950/60 border-rose-600/30 hover:border-rose-400 hover:bg-rose-900/50 text-rose-400 cursor-pointer';
    } else {
      variant =
        'bg-slate-700/50 border-slate-600/50 hover:border-purple-500 hover:bg-purple-900/30 text-slate-400 hover:text-purple-300 cursor-pointer';
    }
  }

  return (
    <button
      onClick={onClick}
      disabled={seat.status === 'booked' || seat.status === 'held'}
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
  description,
  icon,
}: {
  boxClass: string;
  label: string;
  description?: string;
  icon?: ReactNode;
}) {
  return (
    <div className="flex min-w-[150px] items-center gap-3 rounded-xl border border-white/[0.05] bg-white/[0.02] px-3 py-2">
      <div
        className={`h-7 w-7 shrink-0 rounded-lg border flex items-center justify-center text-[9px] font-bold ${boxClass}`}
      >
        {icon}
      </div>
      <div className="min-w-0">
        <p className="text-xs font-medium text-gray-200">{label}</p>
        {description ? (
          <p className="mt-0.5 text-[11px] leading-4 text-gray-500">{description}</p>
        ) : null}
      </div>
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

function formatCinemaTime(value?: string | null) {
  if (!value) {
    return null;
  }

  const parsed = new Date(value);
  if (Number.isNaN(parsed.getTime())) {
    return null;
  }

  return parsed.toLocaleTimeString('en-GB', {
    hour: '2-digit',
    minute: '2-digit',
    timeZone: CINEMA_TIME_ZONE,
  });
}

function normalizePromotionRuleType(ruleType: string) {
  return ruleType.trim().toLowerCase();
}

function isShowtimeSpecificRule(rule: PromotionRuleModel) {
  const normalizedType = normalizePromotionRuleType(rule.ruleType);
  return normalizedType === 'showtime' || normalizedType === 'businessdate';
}

function formatPromotionRule(rule: PromotionRuleModel) {
  const normalizedType = normalizePromotionRuleType(rule.ruleType);

  switch (normalizedType) {
    case 'showtime':
      if (rule.ruleValue.toLowerCase() === 'morning') {
        return 'Morning show only (09:00 - before 11:00)';
      }
      return `Only for this showtime window: ${rule.ruleValue}`;
    case 'businessdate':
      if (rule.ruleValue.toLowerCase() === 'weekend') {
        return 'Only applies on Saturday and Sunday';
      }
      return `Only applies on ${rule.ruleValue}`;
    case 'seatcount': {
      const seatCount = rule.thresholdValue ?? Number(rule.ruleValue || '0') ?? 0;
      return `Minimum ${seatCount} ticket${seatCount === 1 ? '' : 's'}`;
    }
    case 'seattype':
      return `Requires ${rule.ruleValue.toLowerCase()} seat selection`;
    case 'paymentprovider':
      return `Pay with ${rule.ruleValue}`;
    case 'combo':
      return 'Requires the matching snack combo in this order';
    case 'birthdaymonth':
      return 'Valid during your birthday month';
    case 'minimumspend':
      return `Minimum spend ${vnd(rule.thresholdValue ?? Number(rule.ruleValue || '0') ?? 0)}`;
    case 'couponcode':
      return `Use code ${rule.ruleValue}`;
    default:
      return rule.ruleValue;
  }
}

interface PromotionApplyState {
  canApplyNow: boolean;
  message: string;
  ctaLabel: string;
}

function getRequiredSeatCount(rule: PromotionRuleModel) {
  if (typeof rule.thresholdValue === 'number' && !Number.isNaN(rule.thresholdValue)) {
    return rule.thresholdValue;
  }

  const parsedValue = Number(rule.ruleValue);
  return Number.isFinite(parsedValue) ? parsedValue : 0;
}

function getPromotionApplyState(params: {
  promotion: PromotionModel;
  selectedSeats: Seat[];
  selectedSnackCombos: SelectedSnackCombo[];
  comboOptions: ComboOption[];
  bookingDate: string;
  showtime: string;
  seatSubtotal: number;
  comboSubtotal: number;
}): PromotionApplyState {
  const {
    promotion,
    selectedSeats,
    selectedSnackCombos,
    comboOptions,
    bookingDate,
    showtime,
    seatSubtotal,
    comboSubtotal,
  } = params;

  if (selectedSeats.length === 0) {
    return {
      canApplyNow: false,
      message: 'Choose your seats first to apply this promotion.',
      ctaLabel: 'Choose seats first',
    };
  }

  if (
    typeof promotion.minimumSpendAmount === 'number'
    && seatSubtotal + comboSubtotal < promotion.minimumSpendAmount
  ) {
    return {
      canApplyNow: false,
      message: `Minimum spend ${vnd(promotion.minimumSpendAmount)} is required.`,
      ctaLabel: 'Need higher subtotal',
    };
  }

  const seatTypes = new Set(selectedSeats.map((seat) => seat.type.toLowerCase()));
  const comboMatches = new Set(
    selectedSnackCombos.flatMap((selectedCombo) => {
      const combo = findComboOption(comboOptions, selectedCombo.id);
      return [selectedCombo.id.toLowerCase(), combo?.apiId?.toLowerCase()].filter(
        (value): value is string => Boolean(value),
      );
    }),
  );

  for (const rule of promotion.rules) {
    const ruleType = normalizePromotionRuleType(rule.ruleType);

    switch (ruleType) {
      case 'seatcount': {
        const requiredSeatCount = getRequiredSeatCount(rule);
        if (selectedSeats.length < requiredSeatCount) {
          return {
            canApplyNow: false,
            message: `Select at least ${requiredSeatCount} seats to unlock this offer.`,
            ctaLabel: `Need ${requiredSeatCount} seats`,
          };
        }
        break;
      }
      case 'seattype':
        if (!seatTypes.has(rule.ruleValue.toLowerCase())) {
          return {
            canApplyNow: false,
            message: `This offer requires ${rule.ruleValue.toLowerCase()} seats in the order.`,
            ctaLabel: `Need ${rule.ruleValue} seats`,
          };
        }
        break;
      case 'combo': {
        if (!comboMatches.has(rule.ruleValue.toLowerCase())) {
          return {
            canApplyNow: false,
            message: 'Add the required snack combo first, then apply this promotion.',
            ctaLabel: 'Need matching combo',
          };
        }
        break;
      }
      case 'paymentprovider':
        return {
          canApplyNow: false,
          message: `This offer is applied after you choose ${rule.ruleValue} at checkout.`,
          ctaLabel: 'Apply at checkout',
        };
      case 'couponcode':
        return {
          canApplyNow: false,
          message: `This promotion requires coupon code ${rule.ruleValue}.`,
          ctaLabel: 'Coupon required',
        };
      case 'birthdaymonth':
        if (rule.ruleValue.trim().toLowerCase() !== 'currentmonth') {
          const parsedMonth = Number(rule.ruleValue);
          const bookingMonth = new Date(`${bookingDate}T00:00:00`).getMonth() + 1;
          if (Number.isFinite(parsedMonth) && parsedMonth !== bookingMonth) {
            return {
              canApplyNow: false,
              message: 'This birthday promotion is not available for the selected booking month.',
              ctaLabel: 'Month not eligible',
            };
          }
        }
        break;
      case 'showtime': {
        if (rule.ruleValue.trim().toLowerCase() === 'morning') {
          const showtimeHour = Number(showtime.split(':')[0]);
          if (!Number.isFinite(showtimeHour) || showtimeHour < 9 || showtimeHour >= 11) {
            return {
              canApplyNow: false,
              message: 'This offer is only valid for showtimes between 09:00 and 10:59.',
              ctaLabel: 'Wrong showtime',
            };
          }
        }
        break;
      }
      default:
        break;
    }
  }

  return {
    canApplyNow: true,
    message: 'All current booking conditions are satisfied. You can apply this promotion now.',
    ctaLabel: 'Apply now',
  };
}

function SeatHoldCountdown({ expiresAtUtc }: { expiresAtUtc: string }) {
  const [remainingSeconds, setRemainingSeconds] = useState(() => getRemainingSeconds(expiresAtUtc));

  useEffect(() => {
    setRemainingSeconds(getRemainingSeconds(expiresAtUtc));

    const intervalId = window.setInterval(() => {
      setRemainingSeconds(getRemainingSeconds(expiresAtUtc));
    }, 1000);

    return () => window.clearInterval(intervalId);
  }, [expiresAtUtc]);

  const isLow = remainingSeconds < 120;

  return (
    <span
      className={`ml-1 font-mono text-[10px] ${isLow ? 'text-red-400 animate-pulse' : 'text-gray-400'}`}
    >
      {countdown(remainingSeconds)}
    </span>
  );
}

export function SeatSelectionPage() {
  const { movieId } = useParams<{ movieId: string }>();
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();

  const cinemaId = searchParams.get('cinema') ?? 'abcd-mall';
  const showtime = searchParams.get('showtime') ?? '19:30';
  const showtimeId = searchParams.get('showtimeId');
  const hallType = searchParams.get('hallType') ?? '2D';
  const promoId = searchParams.get('promo');
  const bookingDate = searchParams.get('date') ?? getDefaultBookingDate();
  const [appliedPromotionId, setAppliedPromotionId] = useState<string | null>(promoId);
  const [seats, setSeats] = useState<Seat[][]>([]);
  const [comboQuantities, setComboQuantities] = useState<Record<string, number>>({});
  const [apiSnackCombos, setApiSnackCombos] = useState<SnackComboModel[]>([]);
  const [apiPromotions, setApiPromotions] = useState<PromotionModel[]>([]);
  const [seatMapPromotions, setSeatMapPromotions] = useState<PromotionModel[]>([]);
  const [apiMovie, setApiMovie] = useState<MovieDetailModel | null>(null);
  const [apiShowtime, setApiShowtime] = useState<ShowtimeDetailModel | null>(null);
  const [seatMapUnavailableReason, setSeatMapUnavailableReason] = useState<string | null>(null);
  const [apiQuote, setApiQuote] = useState<BookingQuoteModel | null>(null);
  const [isSeatMapLoading, setIsSeatMapLoading] = useState(Boolean(showtimeId));
  const [isShowtimeLoading, setIsShowtimeLoading] = useState(Boolean(showtimeId));
  const [selectedSeatHolds, setSelectedSeatHolds] = useState<Record<string, BookingHoldModel>>({});
  const conflictedSeatIdsRef = useRef<Set<string>>(new Set());

  const refreshSeatMap = useCallback(async (options?: { showLoading?: boolean }) => {
    if (!showtimeId) {
      setSeats([]);
      setSeatMapPromotions([]);
      setIsSeatMapLoading(false);
      return false;
    }

    const showLoading = options?.showLoading ?? true;
    if (showLoading) {
      setIsSeatMapLoading(true);
    }

    try {
      const seatMap = await fetchSeatMap(showtimeId);
      setSeatMapPromotions(seatMap.promotions ?? []);
      setSeatMapUnavailableReason(seatMap.isBookable ? null : (seatMap.bookingUnavailableReason ?? 'This showtime is not available for booking.'));
      if (seatMap.seats.length > 0) {
        const nextSeats = buildSeatsFromApi(seatMap);
        setSeats((previousSeats) =>
          mergeSeatMapWithSelection(previousSeats, nextSeats, conflictedSeatIdsRef.current),
        );
        conflictedSeatIdsRef.current = new Set(
          Array.from(conflictedSeatIdsRef.current).filter((seatId) =>
            nextSeats.some((row) => row.some((seat) => seat.id === seatId && seat.status !== 'available')),
          ),
        );
      } else {
        setSeats([]);
      }
      return true;
    } catch (error) {
      setSeats([]);
      setSeatMapPromotions([]);
      console.warn('Seat map API failed.', error);
      return false;
    } finally {
      if (showLoading) {
        setIsSeatMapLoading(false);
      }
    }
  }, [showtimeId]);

  useEffect(() => {
    void refreshSeatMap({ showLoading: true });
  }, [refreshSeatMap]);

  useEffect(() => {
    if (!showtimeId) {
      setIsShowtimeLoading(false);
      return;
    }

    let active = true;
    const currentShowtimeId = showtimeId;
    setIsShowtimeLoading(true);

    async function loadShowtimeContext() {
      try {
        const detail = await fetchShowtimeDetail(currentShowtimeId);
        if (!active) return;

        setApiShowtime(detail);

        try {
          const movieDetail = await fetchMovieDetail(detail.movieId);
          if (active) {
            setApiMovie(movieDetail);
          }
        } catch (error) {
          console.warn('Movie detail API failed on seat page.', error);
        }
      } catch (error) {
        if (active) {
          setApiShowtime(null);
          setApiMovie(null);
        }
        console.warn('Showtime detail API failed on seat page.', error);
      } finally {
        if (active) {
          setIsShowtimeLoading(false);
        }
      }
    }

    void loadShowtimeContext();

    return () => {
      active = false;
    };
  }, [showtimeId]);

  useEffect(() => {
    let active = true;

    async function loadBookingSeedDataFromApi() {
      try {
        const [combos, promotions] = await Promise.all([
          fetchSnackCombos(),
          fetchPromotions(true),
        ]);

        if (active) {
          setApiSnackCombos(combos);
          setApiPromotions(promotions);
        }
      } catch (error) {
        if (active) {
          setApiSnackCombos([]);
          setApiPromotions([]);
        }
        console.warn('Booking seed API failed.', error);
      }
    }

    void loadBookingSeedDataFromApi();

    return () => {
      active = false;
    };
  }, []);

  const toggleSeat = useCallback(async (clicked: Seat) => {
    if (clicked.status === 'booked' || clicked.status === 'held') return;

    const allSeats = seats.flat();

    if (clicked.status === 'selected') {
      const existingHold = selectedSeatHolds[clicked.id];
      const linkedSeatIds = existingHold
        ? mapHoldSeatIdsToUiSeatIds(existingHold, allSeats)
        : [clicked.id];

      if (existingHold) {
        try {
          await releaseBookingHold(existingHold.holdId);
        } catch (error) {
          console.warn('Release booking hold API failed on seat page.', error);
          return;
        }
      }

      setSelectedSeatHolds((previous) => {
        const next = { ...previous };
        for (const seatId of linkedSeatIds) {
          delete next[seatId];
        }
        return next;
      });

      setSeats((prev) =>
        prev.map((row) =>
          row.map((seat) =>
            linkedSeatIds.includes(seat.id) ? { ...seat, status: 'available' as const } : seat,
          ),
        ),
      );
      return;
    }

    const linkedSeats = findLinkedCoupleSeats(clicked, allSeats);
    const seatInventoryIds = linkedSeats
      .map((seat) => seat.seatInventoryId)
      .filter((seatInventoryId): seatInventoryId is string => Boolean(seatInventoryId));

    if (
      !showtimeId ||
      seatInventoryIds.length === 0 ||
      seatInventoryIds.length !== linkedSeats.length ||
      linkedSeats.some((seat) => seat.status !== 'available')
    ) {
      return;
    }

    try {
      const hold = await createBookingHold({
        showtimeId,
        seatInventoryIds,
        snackCombos: [],
      });

      const heldSeatIds = mapHoldSeatIdsToUiSeatIds(hold, allSeats);

      setSelectedSeatHolds((previous) => {
        const next = { ...previous };
        for (const seatId of heldSeatIds) {
          next[seatId] = hold;
        }
        return next;
      });

      setSeats((prev) =>
        prev.map((row) =>
          row.map((seat) =>
            heldSeatIds.includes(seat.id) ? { ...seat, status: 'selected' as const } : seat,
          ),
        ),
      );
    } catch (error) {
      console.warn('Create booking hold API failed on seat page.', error);
    }
  }, [seats, selectedSeatHolds, showtimeId]);

  const selected = useMemo(() => seats.flat().filter((s) => s.status === 'selected'), [seats]);
  const subtotal = useMemo(
    () => selected.reduce((sum, seat) => sum + getSeatPrice(hallType, seat.type), 0),
    [hallType, selected]
  );
  const selectedSnackCombos = useMemo(
    () => getSelectedSnackCombos(comboQuantities),
    [comboQuantities]
  );
  const comboOptions = useMemo<ComboOption[]>(
    () =>
      apiSnackCombos.map((combo) => ({
        id: combo.code,
        apiId: combo.id,
        name: combo.name,
        description: combo.description,
        price: combo.price,
      })),
    [apiSnackCombos],
  );
  const comboSubtotal = useMemo(
    () =>
      selectedSnackCombos.reduce((sum, selection) => {
        const combo = findComboOption(comboOptions, selection.id);
        return combo ? sum + combo.price * selection.quantity : sum;
      }, 0),
    [comboOptions, selectedSnackCombos]
  );
  const comboCount = useMemo(
    () => selectedSnackCombos.reduce((sum, combo) => sum + combo.quantity, 0),
    [selectedSnackCombos]
  );
  const serviceFee = apiQuote?.serviceFeeTotal ?? selected.length * SERVICE_FEE;
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
  const showtimeApplicablePromotions = useMemo(
    () =>
      [...seatMapPromotions].sort((left, right) => {
        if (left.isFeatured !== right.isFeatured) {
          return left.isFeatured ? -1 : 1;
        }

        return left.displayPriority - right.displayPriority;
      }),
    [seatMapPromotions],
  );
  const selectedPromotion = useMemo(() => {
    const promotionApiId = resolvePromotionApiIdFromUiId(apiPromotions, appliedPromotionId);
    return showtimeApplicablePromotions.find((promotion) => promotion.id === promotionApiId)
      ?? apiPromotions.find((promotion) => promotion.id === promotionApiId)
      ?? null;
  }, [apiPromotions, appliedPromotionId, showtimeApplicablePromotions]);
  const displayMovie = apiMovie;
  const displayMovieTitle = apiMovie?.title ?? apiShowtime?.movieTitle ?? 'Unknown movie';
  const displayPosterUrl = apiMovie?.imageUrl ?? apiShowtime?.moviePosterUrl;
  const displayCinemaName = apiShowtime?.cinemaName ?? 'Unknown cinema';
  const displayCinemaAddress = apiShowtime?.cinemaName ?? 'Unknown address';
  const displayHallName =
    apiShowtime?.hallName ??
    (showtimeId && isShowtimeLoading ? null : (HALL_NAMES[hallType] ?? hallType));
  const displayShowtime = formatCinemaTime(apiShowtime?.startAtUtc) ?? showtime;
  const showtimeUnavailableReason =
    apiShowtime?.isBookable === false
      ? apiShowtime.bookingUnavailableReason ?? 'This showtime is not available for booking.'
      : seatMapUnavailableReason;
  const isShowtimeBookable = !showtimeUnavailableReason;
  const total = useMemo(
    () => apiQuote?.grandTotal ?? subtotal + serviceFee + comboSubtotal,
    [apiQuote?.grandTotal, comboSubtotal, serviceFee, subtotal],
  );
  const promotionApplyStates = useMemo(
    () =>
      Object.fromEntries(
        showtimeApplicablePromotions.map((promotion) => [
          promotion.id,
          getPromotionApplyState({
            promotion,
            selectedSeats: selected,
            selectedSnackCombos,
            comboOptions,
            bookingDate,
            showtime,
            seatSubtotal: subtotal,
            comboSubtotal,
          }),
        ]),
      ) as Record<string, PromotionApplyState>,
    [bookingDate, comboOptions, comboSubtotal, selected, selectedSnackCombos, showtime, showtimeApplicablePromotions, subtotal],
  );

  useEffect(() => {
    if (!isShowtimeBookable || !showtimeId || selected.length === 0 || selected.some((seat) => !seat.seatInventoryId)) {
      setApiQuote(null);
      return;
    }

    let active = true;
    const currentShowtimeId = showtimeId;

    async function refreshQuote() {
      try {
        const snackCombos = selectedSnackCombos
          .map((selection) => {
            const combo = findComboOption(comboOptions, selection.id);
            if (!combo?.apiId) {
              return null;
            }

            return {
              comboId: combo.apiId,
              quantity: selection.quantity,
            };
          })
          .filter((combo): combo is { comboId: string; quantity: number } => combo !== null);

        const quote = await quoteBooking({
          showtimeId: currentShowtimeId,
          seatInventoryIds: selected.map((seat) => seat.seatInventoryId as string),
          snackCombos,
          promotionId: resolvePromotionApiIdFromUiId(apiPromotions, appliedPromotionId),
        });

        if (active) {
          setApiQuote(quote);
        }
      } catch (error) {
        if (active) {
          setApiQuote(null);
        }
        console.warn('Booking quote API failed on seat page.', error);
      }
    }

    void refreshQuote();

    return () => {
      active = false;
    };
  }, [apiPromotions, appliedPromotionId, comboOptions, isShowtimeBookable, selected, selectedSnackCombos, showtimeId]);

  const applyPromotion = useCallback((promotionId: string) => {
    setAppliedPromotionId(promotionId);
  }, []);

  const removeAppliedPromotion = useCallback(() => {
    setAppliedPromotionId(null);
  }, []);

  // Reconcile selected seats with backend hold expiry without rerendering the full page every second.
  useEffect(() => {
    if (Object.keys(selectedSeatHolds).length === 0) {
      return;
    }

    const intervalId = window.setInterval(() => {
      const expiredSeatIds = Object.entries(selectedSeatHolds)
        .filter(([, hold]) => getRemainingSeconds(hold.expiresAtUtc) <= 0)
        .map(([seatId]) => seatId);

      if (expiredSeatIds.length === 0) {
        return;
      }

      setSelectedSeatHolds((previous) => {
        const next = { ...previous };
        for (const seatId of expiredSeatIds) {
          delete next[seatId];
        }
        return next;
      });

      setSeats((previous) =>
        previous.map((row) =>
          row.map((seat) =>
            expiredSeatIds.includes(seat.id) ? { ...seat, status: 'available' as const } : seat,
          ),
        ),
      );

      void refreshSeatMap({ showLoading: false });
    }, 1000);

    return () => window.clearInterval(intervalId);
  }, [refreshSeatMap, selectedSeatHolds]);

  const updateComboQuantity = useCallback((comboId: string, nextQuantity: number) => {
    setComboQuantities((prev) => {
      if (nextQuantity <= 0) {
        const next = { ...prev };
        delete next[comboId];
        return next;
      }
      return { ...prev, [comboId]: Math.min(nextQuantity, 9) };
    });
  }, []);

  // Navigate to checkout with booking state
  const goToCheckout = async () => {
    if (selected.length === 0) return;

    if (!showtimeId || selected.some((seat) => !seat.seatInventoryId)) {
      window.alert('This showtime is not ready for online booking yet. Please choose a live showtime again.');
      return;
    }

    if (!isShowtimeBookable) {
      window.alert(showtimeUnavailableReason ?? 'This showtime is not available for booking.');
      return;
    }

    const params = new URLSearchParams({
      cinema: cinemaId,
      showtime,
      hallType,
      date: bookingDate,
      showtimeId,
    });

    if (appliedPromotionId) {
      params.set('promo', appliedPromotionId);
    }

    // Gather holdIds from per-seat holds already created
    const holdIds = Array.from(new Set(Object.values(selectedSeatHolds)
      .map((hold) => hold.holdId)
      .filter(Boolean)));

    if (holdIds.length === 0) {
      window.alert('Seat holds are not available. Please select seats again.');
      return;
    }

    navigate(
     `${moviePaths.checkout(movieId ?? '')}?${params.toString()}`,
      {
        state: {
          seats: selected.map((s) => {
            const hold = selectedSeatHolds[s.id];
            return {
              id: s.id,
              type: s.type,
              seatInventoryId: s.seatInventoryId,
              hold: hold ? {
                holdId: hold.holdId,
                holdCode: hold.holdCode,
                expiresAtUtc: hold.expiresAtUtc,
                remainingSeconds: getRemainingSeconds(hold.expiresAtUtc),
              } : undefined,
            };
          }),
          subtotal: apiQuote?.seatSubtotal ?? subtotal,
          serviceFee,
          total,
          comboSubtotal,
          discountAmount: apiQuote?.discountTotal ?? 0,
          combos: selectedSnackCombos,
          promoId: appliedPromotionId,
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
        const isAnyHeld = pair.some((s) => s.status === 'held');
        return (
          <div
            key={pair[0]?.id}
            className={`flex gap-px rounded-xl p-0.5 ring-1 transition-all duration-200 ${
              isAnySelected
                ? 'ring-rose-500/70 bg-rose-950/40'
                : isAnyHeld
                  ? 'ring-blue-400/60 bg-blue-950/25'
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

  if (!displayMovie && !apiShowtime) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-[#07091a]">
        <div className="text-center">
          <h2 className="mb-4 text-2xl font-bold text-white">Booking details not found</h2>
          <Button
            onClick={() => navigate(moviePaths.home())}
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
      <header className="relative z-40 border-b border-white/[0.06] bg-[#07091a]/95 backdrop-blur-2xl">
        <div className="container mx-auto flex h-14 items-center justify-between px-4">
          <button
            onClick={() => navigate(`${moviePaths.detail(movieId ?? '')}?${new URLSearchParams({ ...(appliedPromotionId ? { promo: appliedPromotionId } : {}), date: bookingDate }).toString()}`)}
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

          <div />
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
                  src={displayPosterUrl}
                  alt={displayMovieTitle}
                  className="h-24 w-16 rounded-xl object-cover shadow-xl ring-1 ring-white/10 sm:h-28 sm:w-20"
                />
                <Badge className="absolute -right-1.5 -top-1.5 bg-gradient-to-r from-purple-600 to-pink-600 px-1.5 py-0.5 text-[10px]">
                  {hallType}
                </Badge>
              </div>

              {/* Details */}
              <div className="flex min-w-0 flex-1 flex-col justify-center gap-2">
                <h2 className="text-base font-bold text-white line-clamp-1 sm:text-lg">
                  {displayMovieTitle}
                </h2>
                <div className="space-y-1.5">
                  <InfoRow
                    icon={<MapPin className="size-3.5 text-purple-400" />}
                    label={displayCinemaName}
                  />
                  <InfoRow
                    icon={<Clock className="size-3.5 text-pink-400" />}
                    label={`${displayShowtime} - ${bookingDateLabel}`}
                  />
                  <InfoRow
                    icon={<Film className="size-3.5 text-cyan-400" />}
                    label={displayHallName ?? 'Loading hall information...'}
                  />
                  <InfoRow
                    icon={<MapPin className="size-3 text-gray-600" />}
                    label={displayCinemaAddress}
                    dim
                  />
                </div>
              </div>
            </div>

            {!isShowtimeBookable && (
              <div className="mt-4 flex items-center gap-2 rounded-xl bg-amber-950/40 px-3 py-2.5 text-sm text-amber-300 ring-1 ring-amber-500/20">
                <AlertCircle className="size-4 shrink-0" />
                {showtimeUnavailableReason}
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
              {isSeatMapLoading ? (
                <div className="rounded-2xl border border-white/[0.05] bg-white/[0.02] px-4 py-10 text-center">
                  <div className="mx-auto h-10 w-10 animate-spin rounded-full border-2 border-blue-400/40 border-t-blue-300" />
                  <p className="mt-4 text-sm font-medium text-white">Loading live seat map...</p>
                  <p className="mt-1 text-xs text-gray-500">Waiting for the current hall layout and seat availability.</p>
                </div>
              ) : (
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
              )}
              {showtimeApplicablePromotions.length > 0 ? (
                <div className="mt-6 rounded-2xl border border-fuchsia-500/15 bg-fuchsia-950/10 p-4 ring-1 ring-fuchsia-500/10">
                  <div className="flex flex-wrap items-start justify-between gap-3">
                    <div>
                      <p className="text-[10px] font-semibold uppercase tracking-[0.35em] text-fuchsia-300/70">
                        Promotions
                      </p>
                      <h3 className="mt-2 text-base font-semibold text-white">
                        Offers available for this showtime
                      </h3>
                      <p className="mt-1 text-sm text-fuchsia-100/70">
                        Seat-specific conditions are shown first, then the remaining booking rules.
                      </p>
                    </div>
                    <Badge className="bg-fuchsia-500/15 text-fuchsia-200 ring-1 ring-fuchsia-500/25">
                      {showtimeApplicablePromotions.length} offers
                    </Badge>
                  </div>

                  <div className="mt-4 grid gap-3 xl:grid-cols-2">
                    {showtimeApplicablePromotions.map((promotion) => {
                      const showtimeRules = promotion.rules.filter(isShowtimeSpecificRule);
                      const generalRules = promotion.rules.filter((rule) => !isShowtimeSpecificRule(rule));
                      const isSelectedForBooking = selectedPromotion?.id === promotion.id;
                      const applyState = promotionApplyStates[promotion.id];

                      return (
                        <div
                          key={promotion.id}
                          className={`rounded-2xl border p-4 ring-1 ${
                            isSelectedForBooking
                              ? 'border-fuchsia-400/40 bg-fuchsia-500/10 ring-fuchsia-400/20'
                              : 'border-white/[0.06] bg-white/[0.03] ring-white/[0.04]'
                          }`}
                        >
                          <div className="flex items-start justify-between gap-3">
                            <div>
                              <p className="text-sm font-semibold text-white">{promotion.title}</p>
                              <p className="mt-1 text-xs leading-relaxed text-gray-400">
                                {promotion.description}
                              </p>
                            </div>
                            <Badge
                              className={
                                isSelectedForBooking
                                  ? 'bg-emerald-500/15 text-emerald-200 ring-1 ring-emerald-500/25'
                                  : 'bg-fuchsia-500/15 text-fuchsia-200 ring-1 ring-fuchsia-500/25'
                              }
                            >
                              {isSelectedForBooking ? 'Selected' : promotion.discountLabel}
                            </Badge>
                          </div>

                          <p className="mt-3 text-xs text-fuchsia-100/75">{promotion.condition}</p>
                          <p className="mt-2 text-xs text-gray-400">{applyState.message}</p>

                          {showtimeRules.length > 0 ? (
                            <div className="mt-4">
                              <p className="text-[10px] font-semibold uppercase tracking-[0.28em] text-fuchsia-300/75">
                                Showtime conditions
                              </p>
                              <div className="mt-2 flex flex-wrap gap-2">
                                {showtimeRules.map((rule) => (
                                  <span
                                    key={rule.id}
                                    className="rounded-full border border-fuchsia-400/20 bg-fuchsia-500/10 px-3 py-1 text-[11px] font-medium text-fuchsia-100"
                                  >
                                    {formatPromotionRule(rule)}
                                  </span>
                                ))}
                              </div>
                            </div>
                          ) : null}

                          {generalRules.length > 0 ? (
                            <div className="mt-4">
                              <p className="text-[10px] font-semibold uppercase tracking-[0.28em] text-gray-500">
                                Other conditions
                              </p>
                              <div className="mt-2 flex flex-wrap gap-2">
                                {generalRules.map((rule) => (
                                  <span
                                    key={rule.id}
                                    className="rounded-full border border-white/[0.08] bg-white/[0.04] px-3 py-1 text-[11px] font-medium text-gray-300"
                                  >
                                    {formatPromotionRule(rule)}
                                  </span>
                                ))}
                              </div>
                            </div>
                          ) : null}

                          <div className="mt-4 flex items-center justify-end gap-2">
                            {isSelectedForBooking ? (
                              <Button
                                type="button"
                                variant="outline"
                                size="sm"
                                onClick={removeAppliedPromotion}
                                className="border-fuchsia-400/30 text-fuchsia-100 hover:bg-fuchsia-500/10"
                              >
                                Remove
                              </Button>
                            ) : (
                              <Button
                                type="button"
                                size="sm"
                                disabled={!applyState.canApplyNow}
                                onClick={() => applyPromotion(promotion.id)}
                                className="bg-gradient-to-r from-fuchsia-600 to-pink-600 text-white hover:from-fuchsia-500 hover:to-pink-500 disabled:cursor-not-allowed disabled:opacity-40"
                              >
                                {applyState.ctaLabel}
                              </Button>
                            )}
                          </div>
                        </div>
                      );
                    })}
                  </div>
                </div>
              ) : null}

              <div className="mt-8 border-t border-white/[0.05] pt-6">
                <div className="grid gap-4 lg:grid-cols-2">
                  <div className="rounded-2xl border border-white/[0.05] bg-white/[0.02] p-4">
                    <p className="text-[11px] font-semibold uppercase tracking-[0.28em] text-gray-500">
                      Seat status
                    </p>
                    <div className="mt-3 flex flex-wrap gap-3">
                      <LegendItem
                        boxClass="bg-slate-700/50 border-slate-600/50 text-slate-500"
                        label="Available"
                        description="Can be selected now"
                      />
                      <LegendItem
                        boxClass="bg-gradient-to-b from-purple-500 to-pink-600 border-purple-400 shadow-md shadow-purple-500/30"
                        label="Selected"
                        description="Included in your order"
                      />
                      <LegendItem
                        boxClass="bg-slate-900 border-slate-500/70 text-slate-200"
                        label="Booked"
                        description="Already sold"
                      />
                      <LegendItem
                        boxClass={HELD_STATUS_CLASSES}
                        label="Held"
                        description="Temporarily locked"
                      />
                    </div>
                  </div>
                  <div className="rounded-2xl border border-white/[0.05] bg-white/[0.02] p-4">
                    <p className="text-[11px] font-semibold uppercase tracking-[0.28em] text-gray-500">
                      Seat type
                    </p>
                    <div className="mt-3 flex flex-wrap gap-3">
                      <LegendItem
                        boxClass="bg-amber-950/70 border-amber-600/40 text-amber-400"
                        label={`VIP (+${Math.round(SEAT_SURCHARGES.vip / 1000)}K)`}
                        description="Premium row surcharge"
                        icon={<span>*</span>}
                      />
                      <LegendItem
                        boxClass="bg-rose-950/60 border-rose-600/30 text-rose-400"
                        label={`Couple seat (+${Math.round(SEAT_SURCHARGES.couple / 1000)}K)`}
                        description="Booked as a linked pair"
                        icon={<Heart className="size-3 fill-current" />}
                      />
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
        <div className="mt-4 shrink-0 lg:mt-0 lg:w-[320px] xl:w-[360px]">
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
              <p className="font-semibold text-white line-clamp-1 text-sm">{displayMovieTitle}</p>
              <div className="space-y-1">
                <p className="flex items-center gap-1.5 text-xs text-gray-400">
                  <MapPin className="size-3 shrink-0 text-purple-400" />
                  {displayCinemaName}
                </p>
                <p className="flex items-center gap-1.5 text-xs text-gray-400">
                  <Clock className="size-3 shrink-0 text-pink-400" />
                  {displayShowtime} &nbsp;-&nbsp; {bookingDateLabel}
                </p>
                <p className="flex items-center gap-1.5 text-xs text-gray-400">
                  <Film className="size-3 shrink-0 text-cyan-400" />
                  {displayHallName ?? 'Loading hall information...'}
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
                  {selected.map((s) => {
                    const hold = selectedSeatHolds[s.id];
                    return (
                      <div
                        key={s.id}
                        className={`flex items-center gap-1.5 rounded-lg px-2 py-1 text-xs font-semibold ring-1 ${
                          s.type === 'vip'
                            ? 'bg-amber-950/60 text-amber-400 ring-amber-500/30'
                            : s.type === 'couple'
                              ? 'bg-rose-950/60 text-rose-400 ring-rose-500/30'
                              : 'bg-purple-950/60 text-purple-300 ring-purple-500/30'
                        }`}
                      >
                        <span>{s.id}</span>
                        {hold ? <SeatHoldCountdown expiresAtUtc={hold.expiresAtUtc} /> : null}
                      </div>
                    );
                  })}
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
                    const combo = findComboOption(comboOptions, selection.id);
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

            {selectedPromotion && apiQuote?.promotion ? (
              <div className="mb-4 rounded-2xl border border-fuchsia-500/20 bg-fuchsia-950/20 p-4 ring-1 ring-fuchsia-500/10">
                <div className="flex items-start justify-between gap-3">
                  <div>
                    <p className="text-[10px] font-semibold uppercase tracking-widest text-fuchsia-300/80">
                      Selected promotion
                    </p>
                    <p className="mt-1 text-sm font-semibold text-white">{selectedPromotion.title}</p>
                    <p className="mt-1 text-xs text-fuchsia-100/75">{apiQuote.promotion.message}</p>
                  </div>
                  <Badge
                    className={`text-xs ${
                      apiQuote.promotion.isEligible
                        ? 'bg-emerald-600/20 text-emerald-300 ring-1 ring-emerald-500/30'
                        : apiQuote.promotion.status.toLowerCase().includes('ineligible')
                          ? 'bg-red-600/20 text-red-300 ring-1 ring-red-500/30'
                          : 'bg-fuchsia-600/20 text-fuchsia-200 ring-1 ring-fuchsia-500/30'
                    }`}
                  >
                    {apiQuote.promotion.isEligible
                      ? 'Applied'
                      : apiQuote.promotion.status.toLowerCase().includes('ineligible')
                        ? 'Not eligible'
                        : 'Reserved'}
                  </Badge>
                </div>
              </div>
            ) : null}

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
                  const combo = findComboOption(comboOptions, selection.id);
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
                {(apiQuote?.discountTotal ?? 0) > 0 ? (
                  <div className="flex items-center justify-between text-sm">
                    <span className="text-fuchsia-200/80">Promotion discount</span>
                    <span className="font-medium text-fuchsia-200">
                      -{vnd(apiQuote?.discountTotal ?? 0)}
                    </span>
                  </div>
                ) : null}
              </div>
            )}

            {/* Total */}
            <div className="mb-5 flex items-center justify-between rounded-2xl bg-gradient-to-r from-purple-950/60 to-pink-950/40 px-4 py-3.5 ring-1 ring-purple-500/20">
              <span className="font-semibold text-white">Total</span>
              <span className="bg-gradient-to-r from-purple-300 to-pink-300 bg-clip-text text-xl font-bold text-transparent">
                {vnd(total)}
              </span>
            </div>

            {/* CTA */}
            <Button
              disabled={selected.length === 0 || !isShowtimeBookable}
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
            {comboOptions.map((combo) => {
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
                        {combo.originalPrice && (
                          <Badge className="bg-emerald-500/15 text-emerald-300 ring-1 ring-emerald-500/25">
                            Promo price
                          </Badge>
                        )}
                      </div>
                      <p className="mt-1 text-xs leading-relaxed text-gray-400">{combo.description}</p>
                      <div className="mt-3 flex items-center gap-2 text-sm">
                        <span className="font-semibold text-amber-300">
                          {vnd(combo.price)}
                        </span>
                        {combo.originalPrice && (
                          <span className="text-xs text-gray-500 line-through">
                            {vnd(combo.originalPrice)}
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
                <p className="text-lg font-bold text-white">{vnd(total)}</p>
                {comboCount > 0 && (
                  <p className="text-[11px] text-amber-300">{comboCount} combo item(s) included</p>
                )}
              </>
            ) : (
              <p className="text-sm text-gray-500">No seats selected</p>
            )}
          </div>
          <Button
            disabled={selected.length === 0 || !isShowtimeBookable}
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



