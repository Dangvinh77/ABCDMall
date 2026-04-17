import {
  fetchHomePageData,
  fetchMovieDetail,
  fetchMovieShowtimes,
  fetchPromotions,
  fetchSchedules,
  type MovieCardModel,
  type MovieDetailModel,
  type MovieShowtimesModel,
  type PromotionModel,
  type ShowtimeLiteModel,
} from './moviesApi';
import {
  allMovies,
  cinemasList,
  comingSoonMovies,
  nowShowingMovies,
  type Movie,
} from '../data/movie';
import { getScheduleDataForDate, type HallType, type Language, type MovieSchedule } from '../data/schedules';

export interface HomePromo {
  id: string;
  title: string;
  description: string;
  discount: string;
  color: string;
  imageUrl: string;
}

export type PromoFilterKey = 'all' | 'ticket' | 'combo' | 'member' | 'bank' | 'weekend';

export interface UiPromo {
  id: string;
  title: string;
  desc: string;
  badge: string;
  badgeColor: string;
  expiry: string;
  condition: string;
  category: PromoFilterKey[];
  img: string;
  accentFrom: string;
  accentTo: string;
  hot?: boolean;
}

const PROMO_ID_BY_CODE: Record<string, UiPromo['id']> = {
  WEEKEND: 'f1',
  MOMO30: 'f2',
  DATENIGHT: 'f3',
  VCB25: 'p4',
  GROUP20: 'p5',
  COMBOGOLD: 'p6',
  BIRTHDAY: 'p7',
  EARLYBIRD: 'p8',
};

const PROMO_BADGE_BY_CODE: Record<string, string> = {
  WEEKEND: 'Free combo',
  MOMO30: '30% OFF',
  DATENIGHT: 'Buy 1 Get 1',
  VCB25: '25% OFF',
  GROUP20: '20% off groups',
  COMBOGOLD: '-40% combo',
  BIRTHDAY: 'Free ticket',
  EARLYBIRD: '35% early bird',
};

const PROMO_BADGE_COLORS: Record<string, string> = {
  weekend: 'from-fuchsia-500 to-pink-600',
  bank: 'from-cyan-500 to-blue-600',
  ticket: 'from-violet-500 to-purple-700',
  combo: 'from-yellow-400 to-amber-600',
  member: 'from-fuchsia-400 to-pink-500',
};

const PROMO_CATEGORIES: Record<string, PromoFilterKey[]> = {
  weekend: ['weekend', 'combo'],
  bank: ['ticket', 'bank'],
  ticket: ['ticket'],
  combo: ['combo'],
  member: ['member'],
};

const HOME_PROMO_COLORS: Record<string, string> = {
  weekend: 'bg-gradient-to-br from-purple-600 to-purple-800',
  bank: 'bg-gradient-to-br from-cyan-600 to-blue-800',
  ticket: 'bg-gradient-to-br from-pink-600 to-pink-800',
  combo: 'bg-gradient-to-br from-amber-500 to-orange-700',
  member: 'bg-gradient-to-br from-cyan-600 to-cyan-800',
};

function normalizedTitle(value: string) {
  return value.toLowerCase().replace(/[^a-z0-9]+/g, ' ').trim();
}

function findFallbackMovie(movie: Pick<MovieCardModel, 'id' | 'title'>) {
  return (
    allMovies.find((item) => item.id === movie.id) ??
    allMovies.find((item) => normalizedTitle(item.title) === normalizedTitle(movie.title))
  );
}

function toUiMovie(movie: MovieCardModel | MovieDetailModel): Movie {
  const fallback = findFallbackMovie(movie);

  return {
    id: movie.id,
    title: movie.title,
    description:
      'description' in movie ? movie.description : fallback?.description ?? 'Synopsis is being updated.',
    genre: movie.genre,
    rating: movie.rating,
    duration: movie.duration,
    director: 'director' in movie ? movie.director : fallback?.director ?? 'Updating',
    cast: 'cast' in movie ? movie.cast : fallback?.cast ?? ['Updating'],
    language: 'language' in movie ? movie.language : fallback?.language ?? 'Updating',
    releaseDate: 'releaseDate' in movie ? movie.releaseDate : fallback?.releaseDate,
    imageUrl: movie.imageUrl,
    backdropUrl: fallback?.backdropUrl,
    isComingSoon: movie.isComingSoon,
    ageRating: movie.ageRating,
    cinemas: fallback?.cinemas ?? cinemasList,
  };
}

function cinemaCodeFromName(name: string) {
  const normalized = normalizedTitle(name);
  return (
    cinemasList.find((cinema) => normalized.includes(normalizedTitle(cinema.name.replace('ABCD Cinema - ', ''))))?.id ??
    cinemasList.find((cinema) => normalizedTitle(cinema.name) === normalized)?.id ??
    'abcd-mall'
  );
}

function cinemaAddressFromCode(code: string, fallback: string) {
  return cinemasList.find((cinema) => cinema.id === code)?.address ?? fallback;
}

function toUiHallType(value: string): HallType {
  if (value === '3D' || value === 'IMAX' || value === '4DX') return value;
  return '2D';
}

function toUiLanguage(value: string): Language {
  return value === 'dub' ? 'dub' : 'sub';
}

function toUiShowtime(showtime: ShowtimeLiteModel) {
  return {
    id: showtime.showtimeId,
    time: showtime.time,
    hallType: toUiHallType(showtime.hallType),
    language: toUiLanguage(showtime.language),
    availableSeats: showtime.status.toLowerCase().includes('sold') ? 0 : 58,
    totalSeats: 60,
    priceFrom: showtime.priceFrom,
  };
}

function toUiMovieSchedule(schedule: MovieShowtimesModel): MovieSchedule {
  return {
    movie: toUiMovie(schedule.movie),
    cinemaSchedules: schedule.cinemaSchedules.map((cinema) => {
      const cinemaCode = cinemaCodeFromName(cinema.cinemaName);
      return {
        cinemaId: cinemaCode,
        cinemaName: cinema.cinemaName,
        cinemaAddress: cinemaAddressFromCode(cinemaCode, cinema.cinemaAddress),
        showtimes: cinema.showtimes.map(toUiShowtime),
      };
    }),
  };
}

function toHomePromo(promo: PromotionModel): HomePromo {
  return {
    id: PROMO_ID_BY_CODE[promo.discountLabel] ?? promo.id,
    title: promo.title,
    description: promo.description,
    discount: PROMO_BADGE_BY_CODE[promo.discountLabel] ?? promo.discountLabel,
    color: HOME_PROMO_COLORS[promo.category] ?? 'bg-gradient-to-br from-purple-600 to-purple-800',
    imageUrl: promo.imageUrl,
  };
}

export function toUiPromo(promo: PromotionModel): UiPromo {
  return {
    id: PROMO_ID_BY_CODE[promo.discountLabel] ?? promo.id,
    title: promo.title,
    desc: promo.description,
    badge: PROMO_BADGE_BY_CODE[promo.discountLabel] ?? promo.discountLabel,
    badgeColor: PROMO_BADGE_COLORS[promo.category] ?? 'from-purple-500 to-fuchsia-600',
    expiry: promo.expiry,
    condition: promo.condition,
    category: PROMO_CATEGORIES[promo.category] ?? ['ticket'],
    img: promo.imageUrl,
    accentFrom: promo.accentFrom,
    accentTo: promo.accentTo,
    hot: ['WEEKEND', 'MOMO30', 'COMBOGOLD'].includes(promo.discountLabel),
  };
}

export async function loadHomeUiData() {
  const home = await fetchHomePageData();

  return {
    nowShowingMovies: home.nowShowingMovies.map(toUiMovie),
    comingSoonMovies: home.comingSoonMovies.map(toUiMovie),
    promos: home.promotions.map(toHomePromo),
  };
}

export async function loadMovieDetailUiData(movieId: string, bookingDate: string) {
  const [movie, showtimes] = await Promise.all([
    fetchMovieDetail(movieId),
    fetchMovieShowtimes(movieId, bookingDate),
  ]);

  return {
    movie: toUiMovie(movie),
    movieSchedule: {
      movie: toUiMovie(movie),
      cinemaSchedules: showtimes.cinemas.map((cinema) => {
        const cinemaCode = cinemaCodeFromName(cinema.cinemaName);
        return {
          cinemaId: cinemaCode,
          cinemaName: cinema.cinemaName,
          cinemaAddress: cinemaAddressFromCode(cinemaCode, cinema.cinemaAddress),
          showtimes: cinema.showtimes.map(toUiShowtime),
        };
      }),
    } satisfies MovieSchedule,
  };
}

export async function loadSchedulesUiData(bookingDate: string, movieId?: string) {
  const schedules = await fetchSchedules({
    businessDate: bookingDate,
    movieId: movieId && movieId !== 'all' ? movieId : undefined,
  });

  return schedules.map(toUiMovieSchedule);
}

export async function loadPromotionsUiData() {
  const promotions = await fetchPromotions(true);
  return promotions.map(toUiPromo);
}

export const fallbackHomeUiData = {
  nowShowingMovies,
  comingSoonMovies,
  promos: [] as HomePromo[],
};

export function fallbackMovieDetailUiData(movieId: string | undefined, bookingDate: string) {
  const movie = movieId ? allMovies.find((item) => item.id === movieId) : undefined;
  const scheduleMovieId = movieId ? movieId.replace(/-(now|soon)-\d+$/, '') : undefined;
  const movieSchedule = scheduleMovieId
    ? getScheduleDataForDate(bookingDate).find((item) => item.movie.id === scheduleMovieId)
    : undefined;

  return { movie, movieSchedule };
}
