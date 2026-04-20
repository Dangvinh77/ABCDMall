import { useCallback, useEffect, useMemo, useState } from 'react';
import { useLocation, useNavigate, useParams, useSearchParams } from 'react-router-dom';
import { useForm, useWatch } from 'react-hook-form';
import {
  ArrowLeft,
  Clock,
  MapPin,
  Film,
  Ticket,
  CheckCircle2,
  Loader2,
  User,
  Mail,
  Phone,
  ShieldCheck,
  ChevronRight,
  Download,
  Home,
  CreditCard,
  Smartphone,
  QrCode,
  Building2,
  Star,
  Heart,
} from 'lucide-react';
import {
  SERVICE_FEE,
  HALL_NAMES,
  SEAT_LABELS,
  getSeatPrice,
  vnd,
  generateBookingCode,
  type PaymentMethod,
  type SeatType,
  type BookingState,
  type SelectedSnackCombo,
} from '../data/booking';
import { getDefaultBookingDate } from '../data/promotions';
import { Button } from '../component/ui/button';
import { Badge } from '../component/ui/badge';
import { moviePaths } from '../routes/moviePaths';
import {
  confirmBookingHold,
  fetchBookingHold,
  fetchPromotions,
  fetchSnackCombos,
  fetchShowtimeDetail,
  quoteBooking,
  releaseBookingHold,
  type BookingHoldModel,
  type BookingQuoteModel,
  type PromotionModel,
  type SnackComboModel,
  type ShowtimeDetailModel,
  resolvePromotionApiIdFromUiId,
} from '../api/moviesApi';
interface FormValues {
  fullName: string;
  email: string;
  phone: string;
  birthday?: string;
}

type Stage = 'form' | 'loading' | 'success';

interface ComboSummaryItem {
  id: string;
  name: string;
  price: number;
  quantity: number;
  lineTotal: number;
}
const PAYMENT_METHODS: {
  id: PaymentMethod;
  label: string;
  sub: string;
  icon: React.ReactNode;
  accent: string;
  ring: string;
  bg: string;
}[] = [
  {
    id: 'momo',
    label: 'MoMo',
    sub: 'Vi dien tu MoMo',
    icon: <Smartphone className="size-5" />,
    accent: 'text-pink-400',
    ring: 'ring-pink-500/40',
    bg: 'bg-pink-950/40',
  },
  {
    id: 'vnpay',
    label: 'VNPay QR',
    sub: 'Quét để thanh toán',
    icon: <QrCode className="size-5" />,
    accent: 'text-blue-400',
    ring: 'ring-blue-500/40',
    bg: 'bg-blue-950/40',
  },
  {
    id: 'visa',
    label: 'Visa / Mastercard',
    sub: 'Thẻ tín dụng quốc tế',
    icon: <CreditCard className="size-5" />,
    accent: 'text-cyan-400',
    ring: 'ring-cyan-500/40',
    bg: 'bg-cyan-950/30',
  },
  {
    id: 'atm',
    label: 'ATM / Internet Banking',
    sub: 'Thẻ nội địa',
    icon: <Building2 className="size-5" />,
    accent: 'text-emerald-400',
    ring: 'ring-emerald-500/40',
    bg: 'bg-emerald-950/30',
  },
];
const SEAT_CHIP: Record<SeatType, string> = {
  regular: 'bg-purple-950/60 text-purple-300 ring-purple-500/30',
  vip: 'bg-amber-950/60 text-amber-400 ring-amber-500/30',
  couple: 'bg-rose-950/60 text-rose-400 ring-rose-500/30',
};

const SEAT_ICON: Record<SeatType, React.ReactNode> = {
  regular: null,
  vip: <Star className="size-2.5 fill-current" />,
  couple: <Heart className="size-2.5 fill-current" />,
};

function mapHoldSeatType(value: string): SeatType {
  const normalized = value.toLowerCase();
  if (normalized.includes('vip')) return 'vip';
  if (normalized.includes('couple')) return 'couple';
  return 'regular';
}

function StepBar({ current }: { current: 1 | 2 | 3 }) {
  const steps = [
    { n: 1, label: 'Chọn phim' },
    { n: 2, label: 'Chọn ghế' },
    { n: 3, label: 'Thanh toán' },
  ];
  return (
    <div className="flex items-center gap-0">
      {steps.map((step, i) => {
        const done = step.n < current;
        const active = step.n === current;
        return (
          <div key={step.n} className="flex items-center">
            {/* Dot + label */}
            <div className="flex items-center gap-1.5">
              <div
                className={`flex h-5 w-5 items-center justify-center rounded-full text-[10px] font-bold transition-all ${
                  done
                    ? 'bg-purple-600 text-white'
                    : active
                      ? 'bg-gradient-to-br from-purple-500 to-pink-600 text-white shadow-md shadow-purple-500/30'
                      : 'bg-gray-800 text-gray-600'
                }`}
              >
                {done ? <CheckCircle2 className="size-3" /> : step.n}
              </div>
              <span
                className={`hidden text-[11px] font-medium sm:block ${
                  active
                    ? 'text-white'
                    : done
                      ? 'text-purple-400'
                      : 'text-gray-600'
                }`}
              >
                {step.label}
              </span>
            </div>
            {/* Connector */}
            {i < steps.length - 1 && (
              <div
                className={`mx-2 h-px w-8 sm:w-12 transition-colors ${
                  done ? 'bg-purple-600' : 'bg-gray-700'
                }`}
              />
            )}
          </div>
        );
      })}
    </div>
  );
}

function getComboSummary(combos: SelectedSnackCombo[], snackCombos: SnackComboModel[]) {
  const comboLookup = new Map(
    snackCombos.map((combo) => [
      combo.code,
      {
        id: combo.code,
        name: combo.name,
        price: combo.price,
      },
    ]),
  );

  return combos
    .map((selectedCombo) => {
      const combo = comboLookup.get(selectedCombo.id);
      if (!combo) return null;
      return {
        ...combo,
        quantity: selectedCombo.quantity,
        lineTotal: combo.price * selectedCombo.quantity,
      };
    })
    .filter((combo): combo is ComboSummaryItem => combo !== null);
}

function InputField({
  label,
  error,
  icon,
  children,
}: {
  label: string;
  error?: string;
  icon: React.ReactNode;
  children: React.ReactNode;
}) {
  return (
    <div className="space-y-1.5">
      <label className="flex items-center gap-1.5 text-sm font-medium text-gray-300">
        {icon}
        {label}
      </label>
      {children}
      {error && (
        <p className="flex items-center gap-1 text-xs text-red-400">
          <span className="inline-block h-1 w-1 rounded-full bg-red-400" />
          {error}
        </p>
      )}
    </div>
  );
}
export function CheckoutPage() {
  useParams<{ movieId: string }>();
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const location = useLocation();
  const bookingState = location.state as BookingState | null;

  const showtime = searchParams.get('showtime') ?? '19:30';
  const showtimeId = searchParams.get('showtimeId');
  const hallType = searchParams.get('hallType') ?? '2D';
  const promoId = searchParams.get('promo');
  const bookingDate = searchParams.get('date') ?? bookingState?.bookingDate ?? getDefaultBookingDate();
  const holdId = searchParams.get('holdId') ?? bookingState?.holdId;

  const [apiHold, setApiHold] = useState<BookingHoldModel | null>(null);
  const [apiQuote, setApiQuote] = useState<BookingQuoteModel | null>(null);
  const [apiPromotions, setApiPromotions] = useState<PromotionModel[]>([]);
  const [apiSnackCombos, setApiSnackCombos] = useState<SnackComboModel[]>([]);
  const holdSeats = useMemo(
    () =>
      apiHold?.seats.map((seat) => ({
        id: seat.seatCode,
        type: mapHoldSeatType(seat.seatType),
        seatInventoryId: seat.seatInventoryId,
      })) ?? null,
    [apiHold]
  );
  const seats = holdSeats ?? bookingState?.seats ?? [];
  const localSubtotal = useMemo(
    () => seats.reduce((sum, seat) => sum + getSeatPrice(hallType, seat.type), 0),
    [hallType, seats]
  );
  const localServiceFee = seats.length * SERVICE_FEE;
  const selectedCombos = useMemo(() => bookingState?.combos ?? [], [bookingState?.combos]);
  const comboSummary = useMemo(
    () => getComboSummary(selectedCombos, apiSnackCombos),
    [apiSnackCombos, selectedCombos]
  );
  const comboSubtotal = useMemo(
    () => comboSummary.reduce((sum, combo) => sum + combo.lineTotal, 0),
    [comboSummary]
  );
  const subtotal = apiQuote?.seatSubtotal ?? apiHold?.seatSubtotal ?? bookingState?.subtotal ?? localSubtotal;
  const serviceFee = apiQuote?.serviceFeeTotal ?? bookingState?.serviceFee ?? localServiceFee;

  const [stage, setStage] = useState<Stage>('form');
  const [paymentMethod, setPaymentMethod] = useState<PaymentMethod>('momo');
  const [bookingCode] = useState(generateBookingCode);
  const [apiShowtime, setApiShowtime] = useState<ShowtimeDetailModel | null>(null);

  const {
    register,
    handleSubmit,
    getValues,
    control,
    formState: { errors },
  } = useForm<FormValues>({ mode: 'onBlur' });

  const birthday = useWatch({ control, name: 'birthday' });
  const total = apiQuote?.grandTotal ?? apiHold?.grandTotal ?? bookingState?.total ?? subtotal + serviceFee + comboSubtotal;
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
  const displayPosterUrl = apiShowtime?.moviePosterUrl;
  const displayMovieTitle = apiShowtime?.movieTitle ?? 'Unknown movie';
  const displayCinemaName = apiShowtime?.cinemaName ?? 'Unknown cinema';
  const displayHallName = apiShowtime?.hallName ?? HALL_NAMES[hallType] ?? hallType;

  useEffect(() => {
    if (!showtimeId) return;

    let active = true;
    const currentShowtimeId = showtimeId;

    async function loadShowtimeFromApi() {
      try {
        // API FETCH NOTE:
        // Checkout keeps the original form UI, but refreshes cinema/hall context from GET /api/showtimes/{id}.
        const detail = await fetchShowtimeDetail(currentShowtimeId);
        if (active) {
          setApiShowtime(detail);
        }
      } catch (error) {
        if (active) {
          setApiShowtime(null);
        }
        console.warn("Checkout showtime API failed.", error);
      }
    }

    void loadShowtimeFromApi();

    return () => {
      active = false;
    };
  }, [showtimeId]);

  useEffect(() => {
    if (!holdId) return;

    let active = true;
    const currentHoldId = holdId;

    async function loadBookingHoldFromApi() {
      try {
        const hold = await fetchBookingHold(currentHoldId);
        if (active) {
          setApiHold(hold);
        }
      } catch (error) {
        if (active) {
          setApiHold(null);
        }
        console.warn("Booking hold API failed.", error);
      }
    }

    void loadBookingHoldFromApi();

    return () => {
      active = false;
    };
  }, [holdId]);

  useEffect(() => {
    let active = true;

    async function loadCheckoutSeedData() {
      try {
        const [promotions, snackCombos] = await Promise.all([
          fetchPromotions(true),
          fetchSnackCombos(),
        ]);

        if (active) {
          setApiPromotions(promotions);
          setApiSnackCombos(snackCombos);
        }
      } catch (error) {
        console.warn("Checkout seed API failed; combo and promo details may be incomplete.", error);
      }
    }

    void loadCheckoutSeedData();

    return () => {
      active = false;
    };
  }, []);

  useEffect(() => {
    if (!showtimeId || seats.some((seat) => !seat.seatInventoryId)) {
      setApiQuote(null);
      return;
    }

    let active = true;
    const currentShowtimeId = showtimeId;

    async function refreshCheckoutQuote() {
      try {
        const comboLookup = new Map(apiSnackCombos.map((combo) => [combo.code, combo.id]));
        const quote = await quoteBooking({
          showtimeId: currentShowtimeId,
          seatInventoryIds: seats.map((seat) => seat.seatInventoryId as string),
          snackCombos: selectedCombos
            .map((combo) => {
              const comboId = comboLookup.get(combo.id);
              if (!comboId) {
                return null;
              }

              return {
                comboId,
                quantity: combo.quantity,
              };
            })
            .filter((combo): combo is { comboId: string; quantity: number } => combo !== null),
          promotionId: resolvePromotionApiIdFromUiId(apiPromotions, promoId),
          paymentProvider: paymentMethod,
          birthday: birthday ?? undefined,
        });

        if (active) {
          setApiQuote(quote);
        }
      } catch (error) {
        if (active) {
          setApiQuote(null);
        }
        console.warn("Checkout quote API failed.", error);
      }
    }

    void refreshCheckoutQuote();

    return () => {
      active = false;
    };
  }, [apiPromotions, apiSnackCombos, birthday, paymentMethod, promoId, seats, selectedCombos, showtimeId]);

  const selectedPromotion = useMemo(() => {
    const promotionApiId = resolvePromotionApiIdFromUiId(apiPromotions, promoId);
    return apiPromotions.find((promotion) => promotion.id === promotionApiId) ?? null;
  }, [apiPromotions, promoId]);
  const promoBadgeClass = apiQuote?.promotion
    ? apiQuote.promotion.isEligible
      ? 'bg-emerald-600/20 text-emerald-300 ring-1 ring-emerald-500/30'
      : apiQuote.promotion.status.toLowerCase().includes('ineligible')
        ? 'bg-red-600/20 text-red-300 ring-1 ring-red-500/30'
        : 'bg-fuchsia-600/20 text-fuchsia-200 ring-1 ring-fuchsia-500/30'
    : '';
  const promoBadgeLabel = apiQuote?.promotion
    ? apiQuote.promotion.isEligible
      ? 'Applied'
      : apiQuote.promotion.status.toLowerCase().includes('ineligible')
        ? 'Not eligible'
        : 'Reserved'
    : '';

  const backToSeats = useCallback(async () => {
    if (holdId) {
      try {
        await releaseBookingHold(holdId);
      } catch (error) {
        console.warn("Release booking hold API failed; navigating back to seats anyway.", error);
      }
    }

    navigate(-1);
  }, [holdId, navigate]);

  const onSubmit = async () => {
    setStage('loading');
    try {
      if (!holdId) {
        window.alert("Booking hold was not found. Please choose your seats again.");
        setStage('form');
        return;
      }

      // DAY5 TEST-ONLY CONFIRM FLOW:
      // Tạm gọi endpoint confirm hold để test ghế chuyển sang Booked.
      // Khi có booking/payment hoàn chỉnh, thay bằng API đặt vé/thanh toán thật.
      await confirmBookingHold(holdId);
      setTimeout(() => setStage('success'), 1800);
    } catch (error) {
      console.warn("Confirm booking hold API failed; booking was not completed.", error);
      window.alert("Unable to confirm this booking. Please choose your seats again.");
      setStage('form');
    }
  };
  const groupedSeats = (['regular', 'vip', 'couple'] as SeatType[])
    .map((type) => ({ type, seats: seats.filter((s) => s.type === type) }))
    .filter((g) => g.seats.length > 0);
  // SUCCESS SCREEN
  if (stage === 'success') {
    return (
      <div className="min-h-screen bg-[#07091a] text-white">
        {/* Header */}
        <header className="border-b border-white/[0.06] bg-[#07091a]/95 backdrop-blur-2xl">
          <div className="container mx-auto flex h-14 items-center justify-between px-4">
            <div className="flex items-center gap-2">
              <div className="rounded-full bg-gradient-to-br from-purple-600 to-pink-600 p-1.5">
                <Film className="size-4 text-white" />
              </div>
              <span className="font-bold tracking-tight">ABCD Cinema</span>
            </div>
            <Badge className="bg-emerald-600/20 text-emerald-400 ring-1 ring-emerald-500/30">
              <CheckCircle2 className="mr-1.5 size-3.5" />
              Đặt vé thành công
            </Badge>
          </div>
        </header>

        <main className="container mx-auto max-w-2xl px-4 py-12 sm:py-16">
          {/* Success hero */}
          <div className="mb-8 flex flex-col items-center text-center">
            {/* Animated checkmark */}
            <div className="relative mb-6">
              <div className="absolute inset-0 animate-ping rounded-full bg-emerald-500/20" />
              <div className="relative flex h-20 w-20 items-center justify-center rounded-full bg-gradient-to-br from-emerald-500 to-teal-600 shadow-2xl shadow-emerald-500/30">
                <CheckCircle2 className="size-10 text-white" strokeWidth={2.5} />
              </div>
            </div>
            <h1 className="mb-2 text-3xl font-bold text-white sm:text-4xl">
              Đặt vé thành công!
            </h1>
            <p className="text-gray-400">
              Vé của bạn đã được gửi đến email. Vui lòng kiểm tra hộp thư.
            </p>
          </div>

          {/* Booking code */}
          <div className="mb-6 rounded-2xl border border-purple-500/20 bg-gradient-to-br from-purple-950/50 to-pink-950/30 p-5 text-center shadow-xl ring-1 ring-inset ring-white/[0.04]">
            <p className="mb-1.5 text-xs font-semibold uppercase tracking-widest text-gray-500">
              Mã đặt vé
            </p>
            <p className="font-mono text-2xl font-bold tracking-[0.15em] text-white sm:text-3xl">
              {bookingCode}
            </p>
            <p className="mt-1.5 text-xs text-gray-500">
              Vui lòng lưu mã này để hỗ trợ hoặc kiểm tra tại quầy
            </p>
          </div>

          {/* Ticket card */}
          <div className="mb-8 overflow-hidden rounded-2xl border border-white/[0.06] bg-gradient-to-b from-slate-900/90 to-[#0e1128] shadow-2xl">
            <div className="flex gap-4 p-5">
              {displayPosterUrl && (
                <img
                  src={displayPosterUrl}
                  alt={displayMovieTitle}
                  className="h-24 w-16 shrink-0 rounded-xl object-cover shadow-lg ring-1 ring-white/10"
                />
              )}
              <div className="flex flex-col justify-center gap-2">
                <h2 className="text-lg font-bold text-white">
                  {displayMovieTitle}
                </h2>
                <div className="space-y-1">
                  <p className="flex items-center gap-1.5 text-xs text-gray-400">
                    <MapPin className="size-3 text-purple-400" />
                    {displayCinemaName}
                  </p>
                  <p className="flex items-center gap-1.5 text-xs text-gray-400">
                    <Clock className="size-3 text-pink-400" />
                    {showtime} &nbsp;-&nbsp; {bookingDateLabel}
                  </p>
                  <p className="flex items-center gap-1.5 text-xs text-gray-400">
                    <Film className="size-3 text-cyan-400" />
                    {displayHallName}
                  </p>
                </div>
              </div>
            </div>

            {/* Perforation divider */}
            <div className="relative flex items-center">
              <div className="h-px flex-1 border-t border-dashed border-white/10" />
              <div className="absolute -left-3 h-6 w-6 rounded-full bg-[#07091a]" />
              <div className="absolute -right-3 h-6 w-6 rounded-full bg-[#07091a]" />
            </div>
            <div className="grid gap-4 p-5 sm:grid-cols-2">
              <div>
                <p className="mb-2 text-[10px] font-semibold uppercase tracking-widest text-gray-600">
                  Seat
                </p>
                <div className="flex flex-wrap gap-1.5">
                  {seats.map((s) => (
                    <span
                      key={s.id}
                      className={`flex items-center gap-1 rounded-lg px-2 py-1 text-xs font-semibold ring-1 ${SEAT_CHIP[s.type]}`}
                    >
                      {SEAT_ICON[s.type]}
                      {s.id}
                    </span>
                  ))}
                </div>
              </div>
              <div>
                <p className="mb-2 text-[10px] font-semibold uppercase tracking-widest text-gray-600">
                  Guest
                </p>
                <p className="text-sm font-semibold text-white">
                  {getValues('fullName') || 'Nguyen Van A'}
                </p>
                <p className="text-xs text-gray-400">
                  {getValues('email') || 'example@email.com'}
                </p>
                <p className="text-xs text-gray-400">
                  {getValues('phone') || '0901 234 567'}
                </p>
              </div>
              <div className="sm:col-span-2">
                <div className="flex items-center justify-between rounded-xl bg-white/[0.04] px-4 py-2.5">
                  <span className="text-sm font-semibold text-white">Tổng thanh toán</span>
                  <span className="bg-gradient-to-r from-purple-300 to-pink-300 bg-clip-text font-bold text-transparent">
                    {vnd(total)}
                  </span>
                </div>
              </div>
            </div>
          </div>

          {/* Actions */}
          <div className="flex flex-col gap-3 sm:flex-row">
            <Button
              size="lg"
              className="flex-1 bg-gradient-to-r from-purple-600 to-pink-600 font-semibold hover:from-purple-500 hover:to-pink-500 shadow-lg shadow-purple-900/30"
            >
              <Download className="mr-2 size-5" />
              Tải vé điện tử
            </Button>
            <Button
              size="lg"
              variant="outline"
              onClick={() => navigate(moviePaths.home())}
              className="flex-1 border-white/10 text-white hover:bg-white/[0.06]"
            >
              <Home className="mr-2 size-5" />
              Về trang chủ
            </Button>
          </div>

          <p className="mt-5 flex items-center justify-center gap-1.5 text-center text-xs text-gray-600">
            <ShieldCheck className="size-3.5 text-emerald-600" />
            Giao dich duoc bao mat boi ABCD Cinema
          </p>
        </main>
      </div>
    );
  }
  // LOADING SCREEN
  if (stage === 'loading') {
    return (
      <div className="flex min-h-screen flex-col items-center justify-center gap-5 bg-[#07091a]">
        <div className="relative">
          <div className="h-20 w-20 rounded-full border-4 border-purple-500/20" />
          <Loader2 className="absolute inset-0 m-auto size-10 animate-spin text-purple-500" />
        </div>
        <div className="text-center">
          <p className="text-lg font-semibold text-white">Đang xử lý đặt vé...</p>
          <p className="mt-1 text-sm text-gray-500">Vui lòng không đóng trình duyệt</p>
        </div>
        <div className="h-1 w-48 overflow-hidden rounded-full bg-gray-800">
          <div className="animate-loading-bar h-full rounded-full bg-gradient-to-r from-purple-600 to-pink-600" />
        </div>
        <style>{`
          @keyframes loading-bar {
            0% { width: 0%; margin-left: 0; }
            50% { width: 60%; margin-left: 20%; }
            100% { width: 0%; margin-left: 100%; }
          }
          .animate-loading-bar { animation: loading-bar 1.2s ease-in-out infinite; }
        `}</style>
      </div>
    );
  }
  // FORM SCREEN
  return (
    <div className="min-h-screen bg-[#07091a] text-white">
      <header className="sticky top-0 z-50 border-b border-white/[0.06] bg-[#07091a]/95 backdrop-blur-2xl">
        <div className="container mx-auto flex h-14 items-center justify-between px-4">
          <button
            onClick={backToSeats}
            className="flex items-center gap-1.5 rounded-xl px-3 py-2 text-sm text-gray-400 transition-all hover:bg-white/[0.06] hover:text-white"
          >
            <ArrowLeft className="size-4" />
            <span className="hidden sm:inline">Quay lại</span>
          </button>

          <div className="flex items-center gap-2">
            <div className="rounded-full bg-gradient-to-br from-purple-600 to-pink-600 p-1.5">
              <Film className="size-4 text-white" />
            </div>
            <span className="hidden font-bold tracking-tight sm:block">ABCD Cinema</span>
          </div>

          <StepBar current={3} />
        </div>
      </header>
      <main className="container mx-auto px-3 py-6 sm:px-4 lg:flex lg:items-start lg:gap-7 xl:gap-10">
        <div className="min-w-0 flex-1 space-y-5">
          <div className="overflow-hidden rounded-2xl border border-white/[0.06] bg-gradient-to-b from-slate-900/90 to-[#0e1128] shadow-xl ring-1 ring-inset ring-white/[0.04]">
            {/* Card header */}
            <div className="flex items-center gap-3 border-b border-white/[0.06] px-5 py-4">
              <div className="flex h-8 w-8 items-center justify-center rounded-full bg-purple-600/20 ring-1 ring-purple-500/30">
                <User className="size-4 text-purple-400" />
              </div>
              <div>
                <h2 className="font-bold text-white">Guest details</h2>
                <p className="text-xs text-gray-500">Your ticket will be sent to the email below</p>
              </div>
            </div>

            <div className="p-5">
              <form onSubmit={handleSubmit(onSubmit)} id="checkout-form">
                <div className="grid gap-5 sm:grid-cols-2">
                  <div className="sm:col-span-2">
                    <InputField
                      label="Ho va ten"
                      error={errors.fullName?.message}
                      icon={<User className="size-3.5 text-gray-500" />}
                    >
                      <input
                        {...register('fullName', {
                          required: 'Please enter your full name',
                          minLength: { value: 2, message: 'Toi thieu 2 ky tu' },
                        })}
                        placeholder="Nguyen Van A"
                        className={`w-full rounded-xl border bg-white/[0.04] px-4 py-3 text-sm text-white placeholder-gray-600 outline-none transition-all focus:border-purple-500 focus:ring-1 focus:ring-purple-500/30 ${
                          errors.fullName
                            ? 'border-red-500/60 bg-red-950/10'
                            : 'border-white/[0.08] hover:border-white/[0.15]'
                        }`}
                      />
                    </InputField>
                  </div>

                  {/* Email */}
                  <InputField
                    label="Email"
                    error={errors.email?.message}
                    icon={<Mail className="size-3.5 text-gray-500" />}
                  >
                    <input
                      {...register('email', {
                        required: 'Please enter your email',
                        pattern: {
                          value: /^[^\s@]+@[^\s@]+\.[^\s@]+$/,
                          message: 'Invalid email address',
                        },
                      })}
                      type="email"
                      placeholder="example@email.com"
                      className={`w-full rounded-xl border bg-white/[0.04] px-4 py-3 text-sm text-white placeholder-gray-600 outline-none transition-all focus:border-purple-500 focus:ring-1 focus:ring-purple-500/30 ${
                        errors.email
                          ? 'border-red-500/60 bg-red-950/10'
                          : 'border-white/[0.08] hover:border-white/[0.15]'
                      }`}
                    />
                  </InputField>

                  {/* Phone */}
                  <InputField
                    label="Phone number"
                    error={errors.phone?.message}
                    icon={<Phone className="size-3.5 text-gray-500" />}
                  >
                    <input
                      {...register('phone', {
                        required: 'Please enter your phone number',
                        pattern: {
                          value: /^(0|\+84)[3-9][0-9]{8}$/,
                          message: 'Invalid phone number',
                        },
                      })}
                      type="tel"
                      placeholder="0901 234 567"
                      className={`w-full rounded-xl border bg-white/[0.04] px-4 py-3 text-sm text-white placeholder-gray-600 outline-none transition-all focus:border-purple-500 focus:ring-1 focus:ring-purple-500/30 ${
                        errors.phone
                          ? 'border-red-500/60 bg-red-950/10'
                          : 'border-white/[0.08] hover:border-white/[0.15]'
                      }`}
                    />
                  </InputField>

                  {promoId === 'p7' && (
                    <InputField
                      label="Birthday"
                      error={errors.birthday?.message}
                      icon={<Heart className="size-3.5 text-gray-500" />}
                    >
                      <input
                        {...register('birthday', {
                          required: 'Enter your birthday to verify the birthday offer',
                        })}
                        type="date"
                        className={`w-full rounded-xl border bg-white/[0.04] px-4 py-3 text-sm text-white outline-none transition-all focus:border-purple-500 focus:ring-1 focus:ring-purple-500/30 ${
                          errors.birthday
                            ? 'border-red-500/60 bg-red-950/10'
                            : 'border-white/[0.08] hover:border-white/[0.15]'
                        }`}
                      />
                    </InputField>
                  )}
                </div>
              </form>
            </div>
          </div>
          <div className="overflow-hidden rounded-2xl border border-white/[0.06] bg-gradient-to-b from-slate-900/90 to-[#0e1128] shadow-xl ring-1 ring-inset ring-white/[0.04]">
            {/* Card header */}
            <div className="flex items-center gap-3 border-b border-white/[0.06] px-5 py-4">
              <div className="flex h-8 w-8 items-center justify-center rounded-full bg-pink-600/20 ring-1 ring-pink-500/30">
                <CreditCard className="size-4 text-pink-400" />
              </div>
              <div>
                <h2 className="font-bold text-white">Payment method</h2>
                <p className="text-xs text-gray-500">Choose a payment option</p>
              </div>
            </div>

            <div className="p-5">
              <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
                {PAYMENT_METHODS.map((pm) => {
                  const active = paymentMethod === pm.id;
                  return (
                    <button
                      key={pm.id}
                      type="button"
                      onClick={() => setPaymentMethod(pm.id)}
                      className={`relative flex items-center gap-3 rounded-xl border p-4 text-left transition-all duration-200 focus:outline-none ${
                        active
                          ? `${pm.bg} border-transparent ring-1 ${pm.ring} shadow-md`
                          : 'border-white/[0.07] bg-white/[0.02] hover:border-white/[0.14] hover:bg-white/[0.04]'
                      }`}
                    >
                      {/* Icon */}
                      <div
                        className={`flex h-10 w-10 shrink-0 items-center justify-center rounded-xl transition-colors ${
                          active ? pm.accent + ' bg-white/10' : 'text-gray-500 bg-white/[0.04]'
                        }`}
                      >
                        {pm.icon}
                      </div>
                      {/* Text */}
                      <div className="min-w-0">
                        <p
                          className={`text-sm font-semibold transition-colors ${
                            active ? 'text-white' : 'text-gray-300'
                          }`}
                        >
                          {pm.label}
                        </p>
                        <p className="text-xs text-gray-500">{pm.sub}</p>
                      </div>
                      {/* Active radio dot */}
                      <div
                        className={`absolute right-4 top-1/2 h-4 w-4 -translate-y-1/2 rounded-full border-2 transition-all ${
                          active
                            ? `border-purple-400 bg-purple-500 shadow-sm shadow-purple-500/40`
                            : 'border-gray-600 bg-transparent'
                        }`}
                      />
                    </button>
                  );
                })}
              </div>

              {/* Security note */}
              <div className="mt-4 flex items-center gap-2 rounded-xl bg-white/[0.02] px-3 py-2.5">
                <ShieldCheck className="size-4 shrink-0 text-emerald-500" />
                <p className="text-xs text-gray-500">
                  Payment information is protected with 256-bit SSL encryption. ABCD Cinema does not store your card details.
                </p>
              </div>

              {selectedCombos.length > 0 && (
                <div className="mt-4 rounded-2xl border border-amber-500/20 bg-amber-950/20 p-4 ring-1 ring-amber-500/10">
                  <p className="text-sm font-semibold text-white">Selected snack combos</p>
                  <div className="mt-3 space-y-2">
                    {comboSummary.map((combo) => (
                      <div key={combo.id} className="flex items-center justify-between text-sm">
                        <div>
                          <p className="text-amber-100">{combo.name}</p>
                          <p className="text-xs text-amber-100/60">
                            {combo.quantity} x {vnd(combo.price)}
                          </p>
                        </div>
                        <span className="font-medium text-amber-300">{vnd(combo.lineTotal)}</span>
                      </div>
                    ))}
                  </div>
                  <p className="mt-3 text-xs text-amber-100/70">
                    Snack selections were chosen in the seat selection step and are locked into this order summary.
                  </p>
                </div>
              )}
            </div>
          </div>

          {selectedPromotion && apiQuote?.promotion && (
            <div className="overflow-hidden rounded-2xl border border-fuchsia-500/20 bg-gradient-to-b from-fuchsia-950/30 to-[#0e1128] shadow-xl ring-1 ring-inset ring-fuchsia-500/10">
              <div className="flex items-center justify-between border-b border-white/[0.06] px-5 py-4">
                <div>
                  <h2 className="font-bold text-white">Reserved promotion</h2>
                  <p className="text-xs text-gray-500">The offer will be verified again before payment is confirmed</p>
                </div>
                <Badge className={`text-xs ${promoBadgeClass}`}>{promoBadgeLabel}</Badge>
              </div>

              <div className="space-y-3 px-5 py-4">
                <div>
                  <p className="text-sm font-semibold text-white">{selectedPromotion.title}</p>
                  <p className="mt-1 text-xs leading-relaxed text-fuchsia-100/75">{apiQuote.promotion.message}</p>
                </div>

                {!apiQuote.promotion.isEligible && apiQuote.promotion.discountAmount > 0 && (
                  <div className="flex items-center justify-between rounded-xl bg-white/[0.03] px-3 py-2 text-xs ring-1 ring-white/[0.05]">
                    <span className="text-gray-400">Estimated savings</span>
                    <span className="font-semibold text-fuchsia-200">-{vnd(apiQuote.promotion.discountAmount)}</span>
                  </div>
                )}
              </div>
            </div>
          )}

          
          <div className="flex flex-col-reverse gap-3 sm:flex-row sm:justify-between">
            <Button
              type="button"
              variant="outline"
              size="lg"
              onClick={backToSeats}
              className="border-white/10 text-gray-300 hover:bg-white/[0.05] hover:text-white"
            >
              <ArrowLeft className="mr-2 size-4" />
              Back to seats
            </Button>

            <Button
              type="submit"
              form="checkout-form"
              size="lg"
              className="bg-gradient-to-r from-purple-600 to-pink-600 font-semibold hover:from-purple-500 hover:to-pink-500 shadow-lg shadow-purple-900/30 sm:min-w-[220px]"
            >
              <Ticket className="mr-2 size-5" />
              Confirm booking
              <ChevronRight className="ml-2 size-4" />
            </Button>
          </div>
        </div>

       
        <div className="mt-5 shrink-0 lg:mt-0 lg:w-[340px] xl:w-[380px] lg:sticky lg:top-[4.5rem]">
          <div className="overflow-hidden rounded-2xl border border-white/[0.06] bg-gradient-to-b from-slate-900/90 to-[#0e1128] shadow-2xl ring-1 ring-inset ring-white/[0.04]">

            {/* Movie row */}
            <div className="flex gap-4 p-5">
              {displayPosterUrl ? (
                <img
                  src={displayPosterUrl}
                  alt={displayMovieTitle}
                  className="h-24 w-16 shrink-0 rounded-xl object-cover shadow-lg ring-1 ring-white/10"
                />
              ) : (
                <div className="h-24 w-16 shrink-0 rounded-xl bg-slate-800 ring-1 ring-white/10" />
              )}
              <div className="flex min-w-0 flex-col justify-center gap-2">
                <h3 className="line-clamp-2 font-bold text-white">
                  {displayMovieTitle}
                </h3>
                <div className="space-y-1">
                  <p className="flex items-center gap-1.5 text-xs text-gray-400">
                    <MapPin className="size-3 shrink-0 text-purple-400" />
                    {displayCinemaName}
                  </p>
                  <p className="flex items-center gap-1.5 text-xs text-gray-400">
                    <Clock className="size-3 shrink-0 text-pink-400" />
                    {showtime} &nbsp;-&nbsp; {bookingDateLabel}
                  </p>
                  <p className="flex items-center gap-1.5 text-xs text-gray-400">
                    <Film className="size-3 shrink-0 text-cyan-400" />
                    {displayHallName}
                  </p>
                </div>
              </div>
            </div>

            {/* Divider */}
            <div className="mx-5 h-px bg-white/[0.05]" />

            {/* Seats section */}
            <div className="px-5 py-4">
              <p className="mb-3 text-[10px] font-semibold uppercase tracking-widest text-gray-500">
                Selected seats ({seats.length} seats)
              </p>
              <div className="space-y-2.5">
                {groupedSeats.map(({ type, seats: ts }) => (
                  <div key={type} className="flex items-start justify-between gap-3">
                    <div className="flex items-center gap-2">
                      <div className={`flex h-5 w-5 shrink-0 items-center justify-center rounded-md ring-1 text-[9px] ${SEAT_CHIP[type]}`}>
                        {SEAT_ICON[type] ?? null}
                      </div>
                      <div>
                        <p className="text-xs font-medium text-gray-300">
                          {SEAT_LABELS[type]}
                        </p>
                        <div className="mt-0.5 flex flex-wrap gap-1">
                          {ts.map((s) => (
                            <span
                              key={s.id}
                              className={`rounded px-1.5 py-0.5 text-[10px] font-semibold ring-1 ${SEAT_CHIP[s.type]}`}
                            >
                              {s.id}
                            </span>
                          ))}
                        </div>
                      </div>
                    </div>
                    <div className="shrink-0 text-right">
                      <p className="text-xs text-gray-400">
                        {vnd(getSeatPrice(hallType, type))} x {ts.length}
                      </p>
                      <p className="text-sm font-semibold text-white">
                        {vnd(getSeatPrice(hallType, type) * ts.length)}
                      </p>
                    </div>
                  </div>
                ))}
              </div>
            </div>

            {comboSummary.length > 0 && (
              <div className="border-t border-white/[0.05] px-5 py-4">
                <p className="mb-3 text-[10px] font-semibold uppercase tracking-widest text-gray-500">
                  Snack combos ({comboSummary.reduce((sum, combo) => sum + combo.quantity, 0)} items)
                </p>
                <div className="space-y-2.5">
                  {comboSummary.map((combo) => (
                    <div key={combo.id} className="flex items-start justify-between gap-3">
                      <div>
                        <p className="text-xs font-medium text-gray-300">{combo.name}</p>
                        <p className="mt-0.5 text-[11px] text-gray-500">
                          {combo.quantity} x {vnd(combo.price)}
                        </p>
                      </div>
                      <p className="text-sm font-semibold text-amber-300">{vnd(combo.lineTotal)}</p>
                    </div>
                  ))}
                </div>
              </div>
            )}

            {selectedPromotion && apiQuote?.promotion && (
              <div className="border-t border-white/[0.05] px-5 py-4">
                <div className="rounded-2xl border border-fuchsia-500/20 bg-fuchsia-950/20 p-4 ring-1 ring-fuchsia-500/10">
                  <div className="flex items-start justify-between gap-3">
                    <div>
                      <p className="text-[10px] font-semibold uppercase tracking-widest text-fuchsia-300/80">
                        Promotion
                      </p>
                      <p className="mt-1 text-sm font-semibold text-white">{selectedPromotion.title}</p>
                      <p className="mt-1 text-xs text-fuchsia-100/75">{apiQuote.promotion.message}</p>
                    </div>
                    <Badge className={`text-xs ${promoBadgeClass}`}>{promoBadgeLabel}</Badge>
                  </div>
                </div>
              </div>
            )}

            {/* Price breakdown */}
            <div className="border-t border-white/[0.05] px-5 py-4 space-y-2.5">
              <div className="flex justify-between text-sm">
                <span className="text-gray-400">Subtotal</span>
                <span className="text-white">{vnd(subtotal)}</span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-gray-400">Service fee x {seats.length}</span>
                <span className="text-white">{vnd(serviceFee)}</span>
              </div>
              {comboSubtotal > 0 && (
                <div className="flex justify-between text-sm">
                  <span className="text-gray-400">Snack combos</span>
                  <span className="text-white">{vnd(comboSubtotal)}</span>
                </div>
              )}
              {(apiQuote?.discountTotal ?? 0) > 0 && (
                <div className="flex justify-between text-sm">
                  <span className="text-emerald-300">Promotion discount</span>
                  <span className="text-emerald-300">-{vnd(apiQuote?.discountTotal ?? 0)}</span>
                </div>
              )}
            </div>

            {/* Total */}
            <div className="border-t border-white/[0.05] px-5 py-4">
              <div className="flex items-center justify-between rounded-2xl bg-gradient-to-r from-purple-950/60 to-pink-950/40 px-4 py-3.5 ring-1 ring-purple-500/20">
                <span className="font-semibold text-white">Total</span>
                <span className="bg-gradient-to-r from-purple-300 to-pink-300 bg-clip-text text-xl font-bold text-transparent">
                  {vnd(total)}
                </span>
              </div>
            </div>

            {/* Email notice */}
            <div className="flex items-start gap-2 border-t border-white/[0.05] px-5 py-4">
              <Mail className="mt-0.5 size-3.5 shrink-0 text-gray-600" />
              <p className="text-[11px] leading-relaxed text-gray-500">
                Your e-ticket will be sent by email after successful payment.
                Please check your Spam folder as well.
              </p>
            </div>
          </div>
        </div>
      </main>

      {/* Mobile: bottom continue bar */}
      <div className="fixed inset-x-0 bottom-0 z-40 border-t border-white/[0.06] bg-[#07091a]/95 px-4 py-3 backdrop-blur-2xl lg:hidden">
        <div className="flex items-center justify-between gap-3">
          <div>
            <p className="text-xs text-gray-500">{seats.length} seats - Total</p>
            <p className="font-bold text-white">{vnd(total)}</p>
          </div>
          <Button
            type="submit"
            form="checkout-form"
            className="shrink-0 bg-gradient-to-r from-purple-600 to-pink-600 font-semibold hover:from-purple-500 hover:to-pink-500 shadow-lg shadow-purple-900/30"
          >
            Confirm
            <ChevronRight className="ml-1.5 size-4" />
          </Button>
        </div>
      </div>
      <div className="h-20 lg:hidden" />
    </div>
  );
}





