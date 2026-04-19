import { useState, useEffect, useRef, type CSSProperties, type ReactNode } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import {
  ArrowLeft,
  Film,
  Ticket,
  Clock,
  Star,
  MapPin,
  Calendar,
  ChevronRight,
  SlidersHorizontal,
  RotateCcw,
  Clapperboard,
  Globe,
  Shield,
} from 'lucide-react';
import { Button } from '../component/ui/button';
import {
  allCinemas,
  getScheduleDataForDate,
  getScheduleDates,
  formatScheduleDateParam,
  type HallType,
  type Language,
  type MovieSchedule,
  type Showtime,
} from '../data/schedules';
import { vnd } from '../data/booking';
import { moviePaths } from '../routes/moviePaths';
import { loadSchedulesUiData } from '../api/movieUiAdapter';
const HALL_CONFIGS: Record<HallType, { label: string; bg: string; text: string; border: string; glow: string }> = {
  '2D': {
    label: '2D',
    bg: 'bg-gray-700/70',
    text: 'text-gray-200',
    border: 'border-gray-600',
    glow: 'rgba(100,116,139,0.25)',
  },
  '3D': {
    label: '3D',
    bg: 'bg-blue-800/70',
    text: 'text-blue-200',
    border: 'border-blue-700',
    glow: 'rgba(59,130,246,0.25)',
  },
  IMAX: {
    label: 'IMAX',
    bg: 'bg-purple-800/70',
    text: 'text-purple-200',
    border: 'border-purple-600',
    glow: 'rgba(168,85,247,0.3)',
  },
  '4DX': {
    label: '4DX',
    bg: 'bg-orange-800/70',
    text: 'text-orange-200',
    border: 'border-orange-700',
    glow: 'rgba(249,115,22,0.25)',
  },
};

const LANG_CONFIGS: Record<Language, { label: string; bg: string; text: string }> = {
  sub: { label: 'Subtitled', bg: 'bg-cyan-900/60', text: 'text-cyan-400' },
  dub: { label: 'Dubbed', bg: 'bg-amber-900/60', text: 'text-amber-400' },
};

const AGE_RATING_COLORS: Record<string, string> = {
  P: 'bg-green-700',
  T13: 'bg-yellow-600',
  T16: 'bg-orange-600',
  T18: 'bg-red-600',
};

const MONTH_LABELS = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];

function getSeatStatus(st: Showtime): 'full' | 'nearly-full' | 'available' {
  if (st.availableSeats === 0) return 'full';
  if (st.availableSeats <= 10 || st.availableSeats / st.totalSeats < 0.12) return 'nearly-full';
  return 'available';
}

function HallBadge({ hallType }: { hallType: HallType }) {
  const cfg = HALL_CONFIGS[hallType];
  return (
    <span className={`rounded px-1.5 py-0.5 text-[10px] font-bold leading-none ${cfg.bg} ${cfg.text}`}>
      {cfg.label}
    </span>
  );
}

function LangBadge({ language }: { language: Language }) {
  const cfg = LANG_CONFIGS[language];
  return (
    <span className={`rounded px-1.5 py-0.5 text-[10px] leading-none ${cfg.bg} ${cfg.text}`}>
      {cfg.label}
    </span>
  );
}

function getUnavailableLabel(reason?: string) {
  const normalized = reason?.toLowerCase() ?? '';
  if (normalized.includes('ended')) return 'Ended';
  if (normalized.includes('started')) return 'Started';
  if (normalized.includes('closed')) return 'Closed';
  return 'Unavailable';
}

interface ShowtimeChipProps {
  showtime: Showtime;
  movieId: string;
  cinemaId: string;
  onBook: (movieId: string, cinemaId: string, time: string, hallType: HallType, showtimeId: string) => void;
}

function ShowtimeChip({ showtime: st, movieId, cinemaId, onBook }: ShowtimeChipProps) {
  const status = getSeatStatus(st);
  const isFull = status === 'full';
  const isUnavailable = !st.isBookable;
  const isDisabled = isFull || isUnavailable;
  const isNearlyFull = status === 'nearly-full';
  const cfg = HALL_CONFIGS[st.hallType];

  return (
    <button
      disabled={isDisabled}
      title={st.bookingUnavailableReason}
      onClick={() => {
        if (isDisabled) return;
        onBook(movieId, cinemaId, st.time, st.hallType, st.id);
      }}
      style={
        !isDisabled
          ? { '--chip-glow': cfg.glow } as CSSProperties
          : undefined
      }
      className={[
        'group relative flex min-w-[82px] flex-col items-center rounded-xl border px-3 py-2.5 text-center transition-all duration-200',
        isDisabled
          ? 'cursor-not-allowed border-gray-700/30 bg-gray-800/20 opacity-40'
          : isNearlyFull
          ? 'cursor-pointer border-amber-600/35 bg-amber-950/20 hover:border-amber-500/60 hover:bg-amber-900/25 hover:shadow-[0_0_16px_rgba(217,119,6,0.18)]'
          : `cursor-pointer border-white/[0.08] bg-white/[0.04] hover:border-white/20 hover:bg-white/[0.08] hover:shadow-[0_0_18px_var(--chip-glow)]`,
      ].join(' ')}
    >
      <span
        className={`text-base font-black leading-none tracking-tight ${
          isDisabled ? 'text-gray-600 line-through' : 'text-white'
        }`}
      >
        {st.time}
      </span>

      <div className="mt-1.5 flex flex-wrap items-center justify-center gap-1">
        <HallBadge hallType={st.hallType} />
        <LangBadge language={st.language} />
      </div>

      <div className="mt-1.5">
        {isUnavailable ? (
          <span className="text-[10px] text-gray-600">{getUnavailableLabel(st.bookingUnavailableReason)}</span>
        ) : isFull ? (
          <span className="text-[10px] text-gray-600">Sold out</span>
        ) : isNearlyFull ? (
          <span className="text-[10px] font-semibold text-amber-500">Almost full</span>
        ) : (
          <span className="text-[10px] text-gray-500">{vnd(st.priceFrom)}</span>
        )}
      </div>
    </button>
  );
}

interface CinemaGroupProps {
  cinemaSchedule: MovieSchedule['cinemaSchedules'][number];
  movieId: string;
  onBook: (movieId: string, cinemaId: string, time: string, hallType: HallType, showtimeId: string) => void;
}

function CinemaGroup({ cinemaSchedule: cs, movieId, onBook }: CinemaGroupProps) {
  return (
    <div className="py-4 first:pt-0">
      {/* Cinema name + address */}
      <div className="mb-3 flex flex-wrap items-baseline gap-x-2 gap-y-0.5">
        <span className="text-sm font-semibold text-gray-200">{cs.cinemaName}</span>
        <span className="hidden items-center gap-1 text-xs text-gray-600 sm:flex">
          <MapPin className="size-2.5" />
          {cs.cinemaAddress}
        </span>
      </div>
      {/* Showtime chips */}
      <div className="flex flex-wrap gap-2">
        {cs.showtimes.map((st) => (
          <ShowtimeChip
            key={st.id}
            showtime={st}
            movieId={movieId}
            cinemaId={cs.cinemaId}
            onBook={onBook}
          />
        ))}
      </div>
    </div>
  );
}

interface MovieScheduleBlockProps {
  ms: MovieSchedule;
  onBook: (movieId: string, cinemaId: string, time: string, hallType: HallType, showtimeId: string) => void;
  onMovieClick: (movieId: string) => void;
}

function MovieScheduleBlock({ ms, onBook, onMovieClick }: MovieScheduleBlockProps) {
  const { movie, cinemaSchedules } = ms;
  const totalShowtimes = cinemaSchedules.reduce((acc, cs) => acc + cs.showtimes.length, 0);
  const availableShowtimes = cinemaSchedules.reduce(
    (acc, cs) => acc + cs.showtimes.filter((st) => st.isBookable && getSeatStatus(st) !== 'full').length,
    0,
  );

  return (
    <div className="overflow-hidden rounded-2xl border border-white/[0.06] bg-gradient-to-b from-gray-900/60 to-gray-950/80 transition-all duration-300 hover:border-white/10">
      {/* Movie header */}
      <div className="flex gap-4 border-b border-white/[0.05] p-4 sm:gap-5 sm:p-5">
        {/* Poster */}
        <button
          onClick={() => onMovieClick(movie.id)}
          className="group relative shrink-0 overflow-hidden rounded-xl transition-transform duration-200 hover:scale-[1.03]"
          style={{ width: 68, height: 100 }}
        >
          <img
            src={movie.imageUrl}
            alt={movie.title}
            className="h-full w-full object-cover"
          />
          <div className="absolute inset-0 bg-gradient-to-t from-black/50 to-transparent opacity-0 transition-opacity group-hover:opacity-100" />
          {/* Age rating */}
          <span
            className={`absolute left-1 top-1 rounded px-1 py-0.5 text-[9px] font-bold text-white ${AGE_RATING_COLORS[movie.ageRating] ?? 'bg-gray-700'}`}
          >
            {movie.ageRating}
          </span>
        </button>

        {/* Meta */}
        <div className="flex min-w-0 flex-1 flex-col justify-center gap-1">
          <button
            onClick={() => onMovieClick(movie.id)}
            className="group flex items-start gap-1 text-left"
          >
            <h3 className="line-clamp-2 text-base font-bold text-white transition-colors group-hover:text-purple-300 sm:text-lg">
              {movie.title}
            </h3>
            <ChevronRight className="mt-0.5 size-4 shrink-0 text-gray-600 opacity-0 transition-all group-hover:translate-x-0.5 group-hover:text-purple-400 group-hover:opacity-100" />
          </button>

          <div className="flex flex-wrap items-center gap-x-2 gap-y-1 text-xs text-gray-500">
            <span className="text-gray-400">{movie.genre}</span>
            <span className="text-gray-700">&bull;</span>
            <span className="flex items-center gap-1">
              <Clock className="size-3" />
              {movie.duration}
            </span>
            <span className="text-gray-700">&bull;</span>
            <span className="flex items-center gap-1">
              <Globe className="size-3" />
              <span className="hidden sm:inline">{movie.language}</span>
              <span className="sm:hidden">{movie.language.split(' ')[0]}</span>
            </span>
          </div>

          <div className="flex items-center gap-3">
            {/* Star rating */}
            <span className="flex items-center gap-1">
              <Star className="size-3.5 fill-yellow-400 text-yellow-400" />
              <span className="text-sm font-bold text-yellow-400">{movie.rating}</span>
              <span className="text-xs text-gray-600">/10</span>
            </span>

            {/* Showtime count */}
            <span className="flex items-center gap-1 rounded-full border border-white/[0.06] bg-white/[0.04] px-2 py-0.5 text-[11px] text-gray-400">
              <Ticket className="size-3 text-purple-500" />
              {availableShowtimes}/{totalShowtimes} showtimes available
            </span>
          </div>
        </div>
      </div>

      {/* Schedule content */}
      <div className="divide-y divide-white/[0.04] px-4 sm:px-5">
        {cinemaSchedules.map((cs) => (
          <CinemaGroup
            key={cs.cinemaId}
            cinemaSchedule={cs}
            movieId={movie.id}
            onBook={onBook}
          />
        ))}
      </div>
    </div>
  );
}

function EmptySchedule({ onReset }: { onReset: () => void }) {
  return (
    <div className="flex flex-col items-center justify-center rounded-2xl border border-white/[0.06] bg-white/[0.02] py-24 text-center">
      <div className="mb-5 flex h-20 w-20 items-center justify-center rounded-full bg-gray-800/60 ring-1 ring-white/[0.07]">
        <Clapperboard className="size-10 text-gray-600" />
      </div>
      <p className="text-xl font-bold text-gray-400">No matching showtimes</p>
      <p className="mt-2 max-w-xs text-sm text-gray-600">
        Try another date, cinema, or hall format to see more showtimes.
      </p>
      <button
        onClick={onReset}
        className="mt-6 flex items-center gap-2 rounded-xl border border-white/[0.1] bg-white/[0.05] px-5 py-2.5 text-sm font-semibold text-gray-300 transition-all hover:bg-white/[0.1] hover:text-white"
      >
        <RotateCcw className="size-4" />
        Reset filters
      </button>
    </div>
  );
}
function FilterChip({
  active,
  onClick,
  children,
  accentColor = 'purple',
}: {
  active: boolean;
  onClick: () => void;
  children: ReactNode;
  accentColor?: 'purple' | 'blue' | 'orange' | 'cyan';
}) {
  const accentMap = {
    purple: active
      ? 'border-purple-500/50 bg-purple-600/20 text-purple-300'
      : 'border-white/[0.08] bg-white/[0.03] text-gray-400 hover:border-white/15 hover:text-gray-200',
    blue: active
      ? 'border-blue-500/50 bg-blue-700/20 text-blue-300'
      : 'border-white/[0.08] bg-white/[0.03] text-gray-400 hover:border-white/15 hover:text-gray-200',
    orange: active
      ? 'border-orange-500/50 bg-orange-800/20 text-orange-300'
      : 'border-white/[0.08] bg-white/[0.03] text-gray-400 hover:border-white/15 hover:text-gray-200',
    cyan: active
      ? 'border-cyan-500/50 bg-cyan-800/20 text-cyan-300'
      : 'border-white/[0.08] bg-white/[0.03] text-gray-400 hover:border-white/15 hover:text-gray-200',
  };

  return (
    <button
      onClick={onClick}
      className={`whitespace-nowrap rounded-full border px-3.5 py-1.5 text-sm font-semibold transition-all duration-150 ${accentMap[accentColor]}`}
    >
      {children}
    </button>
  );
}
export function SchedulePage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const dates = getScheduleDates(7);
  const initialMovieId = searchParams.get('movie') ?? 'all';
  const initialDate = searchParams.get('date');
  const initialCinema = searchParams.get('cinema') ?? 'all';
  const selectedPromoId = searchParams.get('promo');
  const [selectedDateIdx, setSelectedDateIdx] = useState(() => {
    const matchedIndex = initialDate
      ? dates.findIndex((item) => formatScheduleDateParam(item.date) === initialDate)
      : -1;
    return matchedIndex >= 0 ? matchedIndex : 0;
  });
  const [selectedCinema, setSelectedCinema] = useState(initialCinema);
  const [selectedMovieId, setSelectedMovieId] = useState(initialMovieId);
  const [activeHallTypes, setActiveHallTypes] = useState<Set<HallType>>(new Set());
  const [activeLanguages, setActiveLanguages] = useState<Set<Language>>(new Set());
  const [showStickyBar, setShowStickyBar] = useState(false);
  const [apiScheduleData, setApiScheduleData] = useState<MovieSchedule[]>(() =>
    getScheduleDataForDate(formatScheduleDateParam(dates[selectedDateIdx].date)),
  );

  const heroRef = useRef<HTMLDivElement>(null);
  useEffect(() => {
    const handleScroll = () => {
      if (heroRef.current) {
        const bottom = heroRef.current.getBoundingClientRect().bottom;
        setShowStickyBar(bottom < 64);
      }
    };
    window.addEventListener('scroll', handleScroll, { passive: true });
    return () => window.removeEventListener('scroll', handleScroll);
  }, []);
  const currentDate = dates[selectedDateIdx];
  const selectedDate = formatScheduleDateParam(currentDate.date);
  const dailyScheduleData = apiScheduleData;

  useEffect(() => {
    let active = true;

    async function loadSchedulesFromApi() {
      try {
        // Keep the existing schedule UI, but source the cards from API-backed showtimes.
        const data = await loadSchedulesUiData(selectedDate, selectedMovieId);
        if (!active) return;

        if (data.length > 0) {
          setApiScheduleData(data);
          return;
        }

        setApiScheduleData(getScheduleDataForDate(selectedDate));
      } catch (error) {
        if (active) {
          setApiScheduleData(getScheduleDataForDate(selectedDate));
        }
        console.warn('Schedules API failed; using bundled fallback data.', error);
      }
    }

    void loadSchedulesFromApi();

    return () => {
      active = false;
    };
  }, [selectedDate, selectedMovieId]);

  const filteredSchedules = dailyScheduleData
    .map((ms) => ({
      ...ms,
      cinemaSchedules: ms.cinemaSchedules
        .filter((cs) => selectedCinema === 'all' || cs.cinemaId === selectedCinema)
        .map((cs) => ({
          ...cs,
          showtimes: cs.showtimes.filter((st) => {
            if (activeHallTypes.size > 0 && !activeHallTypes.has(st.hallType)) return false;
            if (activeLanguages.size > 0 && !activeLanguages.has(st.language)) return false;
            return true;
          }),
        }))
        .filter((cs) => cs.showtimes.length > 0),
    }))
    .filter((ms) => selectedMovieId === 'all' || ms.movie.id === selectedMovieId)
    .filter((ms) => ms.cinemaSchedules.length > 0);

  const totalMovies = filteredSchedules.length;
  const totalShowtimes = filteredSchedules.reduce(
    (acc, ms) => acc + ms.cinemaSchedules.reduce((a, cs) => a + cs.showtimes.length, 0),
    0,
  );

  const hasFilters =
    selectedMovieId !== 'all' || selectedCinema !== 'all' || activeHallTypes.size > 0 || activeLanguages.size > 0;
  function toggleHallType(ht: HallType) {
    setActiveHallTypes((prev) => {
      const next = new Set(prev);
      if (next.has(ht)) next.delete(ht);
      else next.add(ht);
      return next;
    });
  }

  function toggleLanguage(lang: Language) {
    setActiveLanguages((prev) => {
      const next = new Set(prev);
      if (next.has(lang)) next.delete(lang);
      else next.add(lang);
      return next;
    });
  }

  function resetFilters() {
    setSelectedMovieId('all');
    setSelectedCinema('all');
    setActiveHallTypes(new Set());
    setActiveLanguages(new Set());
  }

  function handleBook(movieId: string, cinemaId: string, time: string, hallType: HallType, showtimeId: string) {
    const promoQuery = selectedPromoId ? `&promo=${encodeURIComponent(selectedPromoId)}` : '';
    navigate(
      `${moviePaths.booking(movieId)}?cinema=${cinemaId}&showtime=${encodeURIComponent(time)}&hallType=${encodeURIComponent(hallType)}&date=${selectedDate}&showtimeId=${encodeURIComponent(showtimeId)}${promoQuery}`,
    );
  }

  function handleMovieClick(movieId: string) {
    const promoQuery = selectedPromoId ? `&promo=${encodeURIComponent(selectedPromoId)}` : '';
    navigate(`${moviePaths.detail(movieId)}?date=${selectedDate}${promoQuery}`);
  }

  const selectedCinemaLabel = allCinemas.find((c) => c.id === selectedCinema)?.name ?? 'All cinemas';

  return (
    <div className="min-h-screen bg-gradient-to-b from-gray-950 via-gray-900 to-gray-950 text-white">
      <header className="sticky top-0 z-50 border-b border-gray-800 bg-gray-950/90 backdrop-blur-lg">
        <div className="container mx-auto px-4 py-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <Button
                variant="ghost"
                size="sm"
                onClick={() => navigate(moviePaths.home())}
                className="text-gray-300 hover:text-white"
              >
                <ArrowLeft className="mr-1.5 size-4" />
                <span className="hidden sm:inline">Home</span>
              </Button>
              <div className="h-5 w-px bg-gray-700" />
              <div className="flex items-center gap-2">
                <div className="rounded-full bg-gradient-to-br from-purple-600 to-pink-600 p-1.5">
                  <Film className="size-4 text-white" />
                </div>
                <span className="font-bold text-white">ABCD Cinema</span>
              </div>
              <div className="hidden items-center gap-1.5 rounded-full border border-white/10 bg-white/[0.04] px-3 py-1 sm:flex">
                <Calendar className="size-3.5 text-purple-400" />
                <span className="text-xs text-gray-400">Showtimes</span>
              </div>
            </div>

            <Button
              onClick={() => navigate(moviePaths.home())}
              className="bg-gradient-to-r from-purple-600 to-pink-600 hover:from-purple-700 hover:to-pink-700"
            >
              <Ticket className="mr-1.5 size-4" />
              <span className="hidden sm:inline">Book tickets</span>
            </Button>
          </div>
        </div>
      </header>
      <div
        className="fixed left-0 right-0 z-40 transition-transform duration-300"
        style={{
          top: 65,
          transform: showStickyBar ? 'translateY(0)' : 'translateY(-110%)',
        }}
      >
        <div className="border-b border-gray-800/80 bg-gray-950/95 backdrop-blur-xl">
          <div className="container mx-auto flex items-center justify-between px-4 py-2.5">
            <div className="flex items-center gap-2 overflow-hidden text-sm">
              <Calendar className="size-3.5 shrink-0 text-purple-400" />
              <span className="font-semibold text-gray-200">{currentDate.fullLabel}</span>
              <span className="text-gray-700">&bull;</span>
              <span className="hidden text-gray-400 sm:inline">{selectedCinemaLabel}</span>
              <span className="hidden text-gray-700 sm:inline">&bull;</span>
              <span className="text-purple-400">
                {totalMovies} movies
              </span>
              <span className="text-gray-700">&bull;</span>
              <span className="text-gray-400">{totalShowtimes} showtimes</span>
            </div>
            <div className="flex shrink-0 items-center gap-2">
              {hasFilters && (
                <button
                  onClick={resetFilters}
                  className="flex items-center gap-1.5 rounded-full border border-white/[0.08] px-3 py-1 text-xs text-gray-400 transition-colors hover:border-white/15 hover:text-white"
                >
                  <RotateCcw className="size-3" />
                  Reset filters
                </button>
              )}
            </div>
          </div>
        </div>
      </div>
      <section ref={heroRef} className="relative overflow-hidden pb-8 pt-10 sm:pb-10 sm:pt-14">
        <div className="pointer-events-none absolute left-1/4 top-0 h-64 w-64 -translate-x-1/2 rounded-full bg-purple-900/20 blur-3xl" />
        <div className="pointer-events-none absolute right-1/4 top-8 h-56 w-56 rounded-full bg-pink-900/15 blur-3xl" />
        <div className="pointer-events-none absolute bottom-0 left-1/2 h-40 w-96 -translate-x-1/2 rounded-full bg-indigo-900/15 blur-2xl" />

        <div className="container relative mx-auto px-4">
          <div className="mb-4 flex items-center gap-2">
            <div className="h-px w-7 bg-gradient-to-r from-purple-500 to-pink-500" />
            <span className="text-xs font-bold uppercase tracking-widest text-purple-400">
              ABCD Cinema &bull; {MONTH_LABELS[currentDate.date.getMonth()]} {currentDate.date.getFullYear()}
            </span>
          </div>

          <div className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
            <div>
              <h1 className="text-4xl font-black tracking-tight text-white sm:text-5xl">
                Movie{' '}
                <span
                  style={{
                    background: 'linear-gradient(135deg, #a855f7 0%, #ec4899 60%, #f97316 100%)',
                    WebkitBackgroundClip: 'text',
                    WebkitTextFillColor: 'transparent',
                    backgroundClip: 'text',
                  }}
                >
                  Showtimes
                </span>
              </h1>
              <p className="mt-2 text-gray-400">
                Browse by date, cinema, and format, then pick a showtime and book instantly.
              </p>
            </div>

            <div className="flex shrink-0 items-center gap-5 rounded-2xl border border-white/[0.06] bg-white/[0.03] px-5 py-3">
              {[
                { val: totalMovies, label: 'Movies', color: 'text-purple-400' },
                { val: totalShowtimes, label: 'Showtimes', color: 'text-cyan-400' },
                { val: allCinemas.length - 1, label: 'Cinemas', color: 'text-pink-400' },
              ].map((s) => (
                <div key={s.label} className="text-center">
                  <p className={`text-2xl font-black ${s.color}`}>{s.val}</p>
                  <p className="text-[11px] text-gray-600">{s.label}</p>
                </div>
              ))}
            </div>
          </div>
        </div>
      </section>
      <section className="sticky top-16 z-30 border-b border-gray-800/70 bg-gray-950/95 backdrop-blur-xl">
        <div className="container mx-auto px-4">
          <div
            className="flex gap-2 overflow-x-auto py-3 scrollbar-hide"
            style={{ WebkitOverflowScrolling: 'touch' }}
          >
            {dates.map((d) => {
              const isActive = selectedDateIdx === d.idx;
              return (
                <button
                  key={d.idx}
                  onClick={() => setSelectedDateIdx(d.idx)}
                  className={[
                    'flex shrink-0 flex-col items-center rounded-xl border px-4 py-2 transition-all duration-150',
                    isActive
                      ? 'border-purple-500/60 bg-gradient-to-b from-purple-600/25 to-purple-900/20 shadow-[0_0_16px_rgba(168,85,247,0.2)]'
                      : 'border-white/[0.07] bg-white/[0.03] hover:border-white/15 hover:bg-white/[0.06]',
                  ].join(' ')}
                >
                  <span
                    className={`text-[11px] font-semibold uppercase leading-none ${isActive ? 'text-purple-300' : 'text-gray-500'}`}
                  >
                    {d.dayLabel}
                  </span>
                  <span className={`mt-0.5 text-xl font-black leading-none ${isActive ? 'text-white' : 'text-gray-300'}`}>
                    {d.day}
                  </span>
                  <span className={`text-[11px] leading-none ${isActive ? 'text-purple-400' : 'text-gray-600'}`}>
                    {MONTH_LABELS[d.month]}
                  </span>
                </button>
              );
            })}
          </div>
          <div className="flex flex-wrap items-center gap-2 py-2.5">
            <span className="flex shrink-0 items-center gap-1.5 text-xs text-gray-600">
              <SlidersHorizontal className="size-3.5" />
              <span className="hidden sm:inline">Filters:</span>
            </span>

            {allCinemas.map((c) => (
              <FilterChip
                key={c.id}
                active={selectedCinema === c.id}
                onClick={() => setSelectedCinema(c.id)}
                accentColor="purple"
              >
                {c.name === 'All cinemas' ? (
                  <span className="flex items-center gap-1">
                    <MapPin className="size-3" />
                    {c.name}
                  </span>
                ) : (
                  c.name
                )}
              </FilterChip>
            ))}

            <div className="h-5 w-px bg-gray-700/60" />

            {(['2D', '3D', 'IMAX', '4DX'] as HallType[]).map((ht) => (
              <FilterChip
                key={ht}
                active={activeHallTypes.has(ht)}
                onClick={() => toggleHallType(ht)}
                accentColor={ht === '3D' ? 'blue' : ht === '4DX' ? 'orange' : 'purple'}
              >
                {ht}
              </FilterChip>
            ))}

            <div className="h-5 w-px bg-gray-700/60" />

            <FilterChip
              active={activeLanguages.has('sub')}
              onClick={() => toggleLanguage('sub')}
              accentColor="cyan"
            >
              Subtitled
            </FilterChip>
            <FilterChip
              active={activeLanguages.has('dub')}
              onClick={() => toggleLanguage('dub')}
              accentColor="orange"
            >
              Dubbed
            </FilterChip>

            {hasFilters && (
              <button
                onClick={resetFilters}
                className="flex items-center gap-1.5 rounded-full border border-white/[0.08] px-3 py-1.5 text-sm text-gray-500 transition-all hover:border-white/15 hover:text-gray-300"
              >
                <RotateCcw className="size-3.5" />
                Reset
              </button>
            )}
          </div>
        </div>
      </section>
      <main className="container mx-auto px-4 py-8 sm:py-10">
        {hasFilters && totalMovies > 0 && (
          <div className="mb-6 flex items-center gap-2 rounded-xl border border-purple-500/20 bg-purple-950/20 px-4 py-3">
            <Shield className="size-4 shrink-0 text-purple-400" />
            <p className="text-sm text-gray-300">
              Showing <span className="font-bold text-purple-300">{totalMovies} movies</span> with{' '}
              <span className="font-bold text-purple-300">{totalShowtimes} showtimes</span>{' '}
              matching the selected filters
            </p>
          </div>
        )}

        <div className="mb-6 flex items-center gap-3">
          <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-xl bg-gradient-to-br from-purple-600 to-pink-600 shadow-lg shadow-purple-900/40">
            <Calendar className="size-4 text-white" />
          </div>
          <div>
            <h2 className="text-lg font-black text-white">{currentDate.fullLabel}</h2>
            <p className="text-xs text-gray-500">
              {totalMovies} movies now showing &bull; {totalShowtimes} showtimes
            </p>
          </div>
        </div>

        {filteredSchedules.length === 0 ? (
          <EmptySchedule onReset={resetFilters} />
        ) : (
          <div className="space-y-5">
            {filteredSchedules.map((ms) => (
              <MovieScheduleBlock
                key={ms.movie.id}
                ms={ms}
                onBook={handleBook}
                onMovieClick={handleMovieClick}
              />
            ))}
          </div>
        )}

        {filteredSchedules.length > 0 && (
          <div className="mt-10 flex flex-wrap items-center justify-center gap-5 border-t border-white/[0.05] pt-8">
            <p className="w-full text-center text-xs font-semibold uppercase tracking-widest text-gray-700">
              Legend
            </p>
            {[
              { label: 'Many seats left', className: 'border-white/10 bg-white/[0.06] text-white' },
              { label: 'Almost full', className: 'border-amber-600/35 bg-amber-950/20 text-amber-400' },
              { label: 'Sold out', className: 'border-gray-700/30 bg-gray-800/20 text-gray-600 opacity-50' },
            ].map((item) => (
              <div key={item.label} className="flex items-center gap-2">
                <div className={`rounded-lg border px-3 py-1 text-sm font-bold ${item.className}`}>
                  19:30
                </div>
                <span className="text-xs text-gray-500">{item.label}</span>
              </div>
            ))}
            <div className="flex items-center gap-2">
              <HallBadge hallType="IMAX" />
              <span className="text-xs text-gray-500">Hall format</span>
            </div>
            <div className="flex items-center gap-2">
              <LangBadge language="sub" />
              <LangBadge language="dub" />
              <span className="text-xs text-gray-500">Language</span>
            </div>
          </div>
        )}
      </main>
      <footer className="border-t border-gray-800 bg-gray-950 py-8">
        <div className="container mx-auto px-4">
          <div className="flex flex-col items-center gap-4 sm:flex-row sm:justify-between">
            <div className="flex items-center gap-2">
              <div className="rounded-full bg-gradient-to-br from-purple-600 to-pink-600 p-1.5">
                <Film className="size-4 text-white" />
              </div>
              <span className="font-bold text-white">ABCD Cinema</span>
              <span className="text-gray-600">&bull;</span>
              <span className="text-sm text-gray-600">ABCD Mall</span>
            </div>
            <div className="flex items-center gap-4 text-sm text-gray-600">
              <button onClick={() => navigate(moviePaths.promotions())} className="transition-colors hover:text-gray-300">
                Promotions
              </button>
              <button onClick={() => navigate(moviePaths.home())} className="transition-colors hover:text-gray-300">
                Home
              </button>
              <span>&copy; 2026 ABCD Cinema</span>
            </div>
          </div>
        </div>
      </footer>
    </div>
  );
}




