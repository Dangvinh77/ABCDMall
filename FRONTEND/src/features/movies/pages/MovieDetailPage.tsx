import { useEffect, useState } from 'react';
import { useLocation, useNavigate, useParams, useSearchParams } from 'react-router-dom';
import {
  ArrowLeft,
  Star,
  Clock,
  Calendar,
  Film,
  Users,
  Globe,
  Shield,
  MapPin,
  ChevronRight,
  Ticket,
  Play,
} from 'lucide-react';
import { vnd } from '../data/booking';
import { getScheduleDates, formatScheduleDateParam, type MovieSchedule } from '../data/schedules';
import { getDefaultBookingDate } from '../data/promotions';
import { Button } from '../component/ui/button';
import { Badge } from '../component/ui/badge';
import { moviePaths } from '../routes/moviePaths';
import { loadMovieDetailUiData } from '../api/movieUiAdapter';
import type { Movie } from '../data/movie';

const HALL_TYPE_COLORS: Record<string, string> = {
  '2D': 'bg-gray-700 text-gray-200',
  '3D': 'bg-blue-700 text-blue-100',
  IMAX: 'bg-purple-700 text-purple-100',
  '4DX': 'bg-orange-700 text-orange-100',
};

export function MovieDetailPage() {
  const { movieId } = useParams<{ movieId: string }>();
  const navigate = useNavigate();
  const location = useLocation();
  const [searchParams] = useSearchParams();
  const availableDates = getScheduleDates(7);
  const initialBookingDate = searchParams.get('date') ?? getDefaultBookingDate();
  const bookingDate =
    availableDates.find((dateOption) => formatScheduleDateParam(dateOption.date) === initialBookingDate)
      ? initialBookingDate
      : formatScheduleDateParam(availableDates[0].date);
  const [apiMovie, setApiMovie] = useState<Movie | undefined>();
  const [apiMovieSchedule, setApiMovieSchedule] = useState<MovieSchedule | undefined>();
  const [isLoading, setIsLoading] = useState(Boolean(movieId));
  const movie = apiMovie;
  const movieSchedule = apiMovieSchedule;
  const scheduleMovieId = movie?.id.replace(/-(now|soon)-\d+$/, '') ?? movieId?.replace(/-(now|soon)-\d+$/, '');
  const bookingDateLabel = new Date(`${bookingDate}T00:00:00`).toLocaleDateString('en-US', {
    weekday: 'long',
    month: 'long',
    day: 'numeric',
    year: 'numeric',
  });

  const buildBookingUrl = (cinemaId: string, showtime: string, hallType: string, showtimeId?: string) => {
    const params = new URLSearchParams(location.search);
    params.set('cinema', cinemaId);
    params.set('showtime', showtime);
    params.set('hallType', hallType);
    params.set('date', bookingDate);
    if (showtimeId) {
      params.set('showtimeId', showtimeId);
    }

    return `${moviePaths.booking(movieId ?? '')}?${params.toString()}`;
  };

  const buildShowtimesUrl = () => {
    const params = new URLSearchParams(location.search);
    params.set('movie', scheduleMovieId ?? movieId ?? '');
    params.set('date', bookingDate);

    return `${moviePaths.showtimes()}?${params.toString()}`;
  };

  const buildMovieDateUrl = (dateParam: string) => {
    const params = new URLSearchParams(location.search);
    params.set('date', dateParam);

    return `${moviePaths.detail(movieId ?? '')}?${params.toString()}`;
  };

  useEffect(() => {
    if (!movieId) return;

    let active = true;
    const currentMovieId = movieId;

    async function loadMovieFromApi() {
      if (active) {
        setIsLoading(true);
      }

      try {
        // API FETCH NOTE:
        // Detail keeps the original visual layout but swaps the movie/showtime objects with API-backed data.
        const data = await loadMovieDetailUiData(currentMovieId, bookingDate);
        if (!active) return;

        setApiMovie(data.movie);
        setApiMovieSchedule(data.movieSchedule);
      } catch (error) {
        if (active) {
          setApiMovie(undefined);
          setApiMovieSchedule(undefined);
        }
        console.warn('Movie detail API failed.', error);
      } finally {
        if (active) {
          setIsLoading(false);
        }
      }
    }

    void loadMovieFromApi();

    return () => {
      active = false;
    };
  }, [bookingDate, movieId]);

  if (isLoading) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-gray-950">
        <div className="text-center text-gray-300">Loading movie details...</div>
      </div>
    );
  }

  if (!movie) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-gray-950">
        <div className="text-center">
          <Film className="mx-auto mb-4 size-16 text-gray-600" />
          <h2 className="mb-2 text-2xl font-bold text-white">Movie not found</h2>
          <p className="mb-6 text-gray-400">
            The movie you are looking for does not exist or has been removed.
          </p>
          <Button
            onClick={() => navigate(moviePaths.home())}
            className="bg-gradient-to-r from-purple-600 to-pink-600 hover:from-purple-700 hover:to-pink-700"
          >
            <ArrowLeft className="mr-2 size-4" />
            Back to homepage
          </Button>
        </div>
      </div>
    );
  }

  const ageRatingColors: Record<string, string> = {
    P: 'bg-green-600',
    T13: 'bg-yellow-600',
    T16: 'bg-orange-600',
    T18: 'bg-red-600',
  };

  return (
    <div className="min-h-screen bg-gradient-to-b from-gray-950 via-gray-900 to-gray-950">
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
                <ArrowLeft className="mr-2 size-4" />
                <span className="hidden sm:inline">Back</span>
              </Button>
              <div className="h-5 w-px bg-gray-700" />
              <div className="flex items-center gap-2">
                <div className="rounded-full bg-gradient-to-br from-purple-600 to-pink-600 p-1.5">
                  <Film className="size-4 text-white" />
                </div>
                <span className="font-bold text-white">ABCD Cinema</span>
              </div>
            </div>
          </div>
        </div>
      </header>

      <div className="relative z-0 h-72 overflow-hidden sm:h-96 md:h-[480px]">
        <img src={movie.imageUrl} alt={movie.title} className="size-full object-cover object-center" />
        <div className="absolute inset-0 bg-gradient-to-t from-gray-950 via-gray-950/60 to-transparent" />
        <div className="absolute inset-0 bg-gradient-to-r from-gray-950/70 via-transparent to-transparent" />

        <div className="absolute inset-0 flex items-center justify-center">
          <button className="group flex size-16 items-center justify-center rounded-full border-2 border-white/40 bg-white/10 backdrop-blur-sm transition-all hover:scale-110 hover:border-white/80 hover:bg-white/20 sm:size-20">
            <Play className="size-7 translate-x-0.5 fill-white text-white sm:size-9" />
          </button>
        </div>
      </div>

      <div className="relative z-10 container mx-auto px-4 pb-16">
        <div className="relative z-10 -mt-32 grid gap-8 md:-mt-40 lg:grid-cols-[300px_1fr] lg:-mt-52 xl:grid-cols-[340px_1fr]">
          <div className="flex justify-center lg:justify-start">
            <div className="relative w-48 overflow-hidden rounded-2xl shadow-2xl ring-2 ring-purple-500/30 sm:w-56 md:w-64 lg:w-full">
              <img src={movie.imageUrl} alt={movie.title} className="aspect-[2/3] w-full object-cover" />
              <div className="absolute left-3 top-3">
                <span
                  className={`rounded-md px-2 py-1 text-xs font-bold text-white ${ageRatingColors[movie.ageRating] ?? 'bg-gray-600'}`}
                >
                  {movie.ageRating}
                </span>
              </div>
              {movie.isComingSoon && (
                <div className="absolute inset-x-0 bottom-0 bg-gradient-to-t from-blue-900/90 to-transparent p-4 text-center">
                  <span className="text-sm font-semibold text-blue-300">Coming soon</span>
                  <p className="text-xs text-blue-200">{movie.releaseDate}</p>
                </div>
              )}
            </div>
          </div>

          <div className="space-y-6 pt-4 lg:pt-8">
            <div className="space-y-3">
              {movie.isComingSoon && (
                <Badge className="bg-gradient-to-r from-blue-600 to-cyan-600 text-white">
                  Coming soon &bull; {movie.releaseDate}
                </Badge>
              )}
              <h1 className="text-3xl font-bold text-white sm:text-4xl md:text-5xl">{movie.title}</h1>

              <div className="flex flex-wrap items-center gap-4">
                <div className="flex items-center gap-2 rounded-full bg-yellow-500/10 px-3 py-1.5 ring-1 ring-yellow-500/30">
                  <Star className="size-5 fill-yellow-400 text-yellow-400" />
                  <span className="font-bold text-yellow-400">{movie.rating}</span>
                  <span className="text-sm text-yellow-400/70">/10</span>
                </div>
                <div className="flex flex-wrap gap-2">
                  {movie.genre.split(', ').map((g) => (
                    <Badge
                      key={g}
                      variant="secondary"
                      className="bg-gray-800 text-gray-200 hover:bg-gray-700"
                    >
                      {g}
                    </Badge>
                  ))}
                </div>
              </div>
            </div>

            <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
              <div className="rounded-xl bg-gray-800/60 p-3 text-center ring-1 ring-gray-700/50">
                <Clock className="mx-auto mb-1 size-5 text-purple-400" />
                <p className="text-xs text-gray-400">Duration</p>
                <p className="text-sm font-semibold text-white">{movie.duration}</p>
              </div>
              <div className="rounded-xl bg-gray-800/60 p-3 text-center ring-1 ring-gray-700/50">
                <Globe className="mx-auto mb-1 size-5 text-pink-400" />
                <p className="text-xs text-gray-400">Language</p>
                <p className="text-sm font-semibold text-white">{movie.language}</p>
              </div>
              <div className="rounded-xl bg-gray-800/60 p-3 text-center ring-1 ring-gray-700/50">
                <Shield className="mx-auto mb-1 size-5 text-orange-400" />
                <p className="text-xs text-gray-400">Age rating</p>
                <p className="text-sm font-semibold text-white">{movie.ageRating}</p>
              </div>
              {movie.releaseDate && (
                <div className="rounded-xl bg-gray-800/60 p-3 text-center ring-1 ring-gray-700/50">
                  <Calendar className="mx-auto mb-1 size-5 text-cyan-400" />
                  <p className="text-xs text-gray-400">Release date</p>
                  <p className="text-sm font-semibold text-white">{movie.releaseDate}</p>
                </div>
              )}
            </div>

            <div className="space-y-2">
              <h2 className="text-lg font-semibold text-white">Synopsis</h2>
              <p className="leading-relaxed text-gray-300">{movie.description}</p>
            </div>

            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <div className="flex items-center gap-2 text-gray-400">
                  <Film className="size-4" />
                  <span className="text-sm font-medium">Director</span>
                </div>
                <p className="font-semibold text-white">{movie.director}</p>
              </div>
              <div className="space-y-2">
                <div className="flex items-center gap-2 text-gray-400">
                  <Users className="size-4" />
                  <span className="text-sm font-medium">Cast</span>
                </div>
                <p className="font-semibold text-white">{movie.cast.join(', ')}</p>
              </div>
            </div>

            {!movie.isComingSoon && (
              <Button
                size="lg"
                onClick={() => navigate(buildShowtimesUrl())}
                className="w-full bg-gradient-to-r from-purple-600 to-pink-600 hover:from-purple-700 hover:to-pink-700 sm:w-auto"
              >
                <Ticket className="mr-2 size-5" />
                Book now
                <ChevronRight className="ml-2 size-4" />
              </Button>
            )}
            {movie.isComingSoon && (
              <Button
                size="lg"
                variant="outline"
                onClick={() => navigate(buildShowtimesUrl())}
                className="w-full border-purple-500/50 text-purple-300 hover:border-purple-400 hover:bg-purple-500/10 sm:w-auto"
              >
                <Calendar className="mr-2 size-5" />
                Remind me at release
              </Button>
            )}
          </div>
        </div>

        {!movie.isComingSoon && movieSchedule && movieSchedule.cinemaSchedules.length > 0 && (
          <div className="mt-12 space-y-6">
            <div className="flex items-center gap-4">
              <div className="h-px flex-1 bg-gradient-to-r from-transparent via-gray-700 to-transparent" />
              <div className="flex items-center gap-2 text-gray-400">
                <MapPin className="size-4 text-purple-400" />
                <span className="text-sm font-medium uppercase tracking-wider">Showtimes</span>
              </div>
              <div className="h-px flex-1 bg-gradient-to-r from-transparent via-gray-700 to-transparent" />
            </div>

            <div className="flex gap-2 overflow-x-auto pb-1 scrollbar-hide">
              {availableDates.map((dateOption) => {
                const dateParam = formatScheduleDateParam(dateOption.date);
                const isActive = dateParam === bookingDate;

                return (
                  <button
                    key={dateParam}
                    onClick={() => navigate(buildMovieDateUrl(dateParam))}
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
                      {dateOption.dayLabel}
                    </span>
                    <span className={`mt-0.5 text-xl font-black leading-none ${isActive ? 'text-white' : 'text-gray-300'}`}>
                      {dateOption.day}
                    </span>
                    <span className={`text-[11px] leading-none ${isActive ? 'text-purple-400' : 'text-gray-600'}`}>
                      {dateOption.date.toLocaleDateString('en-US', { month: 'short' })}
                    </span>
                  </button>
                );
              })}
            </div>

            <div className="space-y-4">
              {movieSchedule.cinemaSchedules.map((cinema) => {
                const hallTypes = [...new Set(cinema.showtimes.map((showtime) => showtime.hallType))];
                const firstAvailableShowtime = cinema.showtimes.find((showtime) => showtime.availableSeats > 0);

                return (
                  <div
                    key={cinema.cinemaId}
                    className="overflow-hidden rounded-2xl bg-gray-800/50 ring-1 ring-gray-700/50 transition-all hover:bg-gray-800/80 hover:ring-purple-500/30"
                  >
                    <div className="border-b border-gray-700/50 p-4 sm:p-5">
                      <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
                        <div className="space-y-1">
                          <div className="flex items-center gap-2">
                            <div className="size-2 rounded-full bg-gradient-to-r from-purple-500 to-pink-500" />
                            <h3 className="font-bold text-white">{cinema.cinemaName}</h3>
                          </div>
                          <p className="flex items-start gap-1.5 text-sm text-gray-400">
                            <MapPin className="mt-0.5 size-3.5 shrink-0 text-gray-500" />
                            {cinema.cinemaAddress}
                          </p>
                        </div>
                        <div className="flex flex-wrap gap-1.5 sm:shrink-0">
                          {hallTypes.map((type) => (
                            <span
                              key={type}
                              className={`rounded-md px-2 py-0.5 text-xs font-bold ${HALL_TYPE_COLORS[type] ?? 'bg-gray-700 text-gray-200'}`}
                            >
                              {type}
                            </span>
                          ))}
                        </div>
                      </div>
                    </div>

                    <div className="p-4 sm:p-5">
                      <p className="mb-3 text-xs font-medium uppercase tracking-wider text-gray-500">
                        {bookingDateLabel}
                      </p>
                      <div className="flex flex-wrap gap-2">
                        {cinema.showtimes.map((showtime) => (
                          <button
                            key={showtime.id}
                            disabled={showtime.availableSeats === 0}
                            onClick={() =>
                              navigate(buildBookingUrl(cinema.cinemaId, showtime.time, showtime.hallType, showtime.id))
                            }
                            className="group relative overflow-hidden rounded-xl border border-gray-600 bg-gray-900/50 px-4 py-2 text-left text-sm font-semibold text-white transition-all hover:border-purple-500/70 hover:bg-purple-500/10 hover:text-purple-300 focus:outline-none focus:ring-2 focus:ring-purple-500/50 disabled:cursor-not-allowed disabled:opacity-40"
                          >
                            <div className="flex items-center gap-2">
                              <span>{showtime.time}</span>
                              <span
                                className={`rounded px-1.5 py-0.5 text-[10px] font-bold ${HALL_TYPE_COLORS[showtime.hallType] ?? 'bg-gray-700 text-gray-200'}`}
                              >
                                {showtime.hallType}
                              </span>
                            </div>
                            <p className="mt-1 text-[11px] font-medium text-gray-400">
                              From {vnd(showtime.priceFrom)}
                            </p>
                          </button>
                        ))}
                      </div>
                    </div>

                    <div className="border-t border-gray-700/50 px-4 py-3 sm:px-5">
                      <Button
                        className="w-full bg-gradient-to-r from-purple-600 to-pink-600 hover:from-purple-700 hover:to-pink-700 sm:w-auto"
                        size="sm"
                        disabled={!firstAvailableShowtime}
                        onClick={() =>
                          firstAvailableShowtime &&
                          navigate(
                            buildBookingUrl(
                              cinema.cinemaId,
                              firstAvailableShowtime.time,
                              firstAvailableShowtime.hallType,
                              firstAvailableShowtime.id,
                            ),
                          )
                        }
                      >
                        <Ticket className="mr-2 size-4" />
                        Choose showtime & book
                        <ChevronRight className="ml-1 size-3.5" />
                      </Button>
                    </div>
                  </div>
                );
              })}
            </div>
          </div>
        )}
      </div>

      <footer className="border-t border-gray-800 bg-gray-950 py-6">
        <div className="container mx-auto px-4 text-center text-sm text-gray-500">
          &copy; 2026 ABCD Cinema. Online ticket booking made fast and simple.
        </div>
      </footer>
    </div>
  );
}

