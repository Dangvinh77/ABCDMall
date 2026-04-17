import { api } from "../../../core/api/api";

export type MovieStatus = "NowShowing" | "ComingSoon" | string;
export type SeatType = "regular" | "vip" | "couple";
export type SeatStatus = "available" | "held" | "booked";

export interface MovieCardModel {
  id: string;
  apiId?: string;
  title: string;
  genre: string;
  rating: number;
  duration: string;
  imageUrl: string;
  isComingSoon: boolean;
  ageRating: string;
}

export interface MovieDetailModel extends MovieCardModel {
  description: string;
  language: string;
  director: string;
  cast: string[];
  releaseDate?: string;
  trailerUrl?: string;
}

export interface PromotionModel {
  id: string;
  title: string;
  description: string;
  category: string;
  discountLabel: string;
  status: string;
  imageUrl: string;
  accentFrom: string;
  accentTo: string;
  condition: string;
  expiry: string;
}

export interface SnackComboModel {
  id: string;
  code: string;
  name: string;
  description: string;
  price: number;
  imageUrl?: string;
}

export interface ShowtimeLiteModel {
  showtimeId: string;
  cinemaId: string;
  cinemaName: string;
  cinemaAddress: string;
  hallType: string;
  time: string;
  businessDate: string;
  priceFrom: number;
  status: string;
  language: "sub" | "dub";
}

export interface CinemaShowtimeGroupModel {
  cinemaId: string;
  cinemaName: string;
  cinemaAddress: string;
  showtimes: ShowtimeLiteModel[];
}

export interface MovieShowtimesModel {
  movie: MovieCardModel;
  cinemaSchedules: CinemaShowtimeGroupModel[];
}

export interface ShowtimeDetailModel {
  showtimeId: string;
  movieId: string;
  movieTitle: string;
  moviePosterUrl?: string;
  cinemaId: string;
  cinemaName: string;
  hallId: string;
  hallName: string;
  hallType: string;
  businessDate: string;
  startAtUtc: string;
  language: "sub" | "dub";
  basePrice: number;
  status: string;
}

export interface SeatMapSeatModel {
  seatInventoryId: string;
  seatCode: string;
  row: string;
  col: number;
  seatType: SeatType;
  status: SeatStatus;
  price: number;
  coupleGroupCode?: string;
}

export interface SeatMapModel {
  showtimeId: string;
  hallId: string;
  hallType: string;
  seats: SeatMapSeatModel[];
}

export interface QuoteLineModel {
  type: string;
  code: string;
  label: string;
  amount: number;
}

export interface QuotePromotionModel {
  promotionId?: string;
  promotionCode: string;
  status: string;
  isEligible: boolean;
  message: string;
  discountAmount: number;
}

export interface BookingQuoteModel {
  showtimeId: string;
  seatSubtotal: number;
  serviceFeeTotal: number;
  comboSubtotal: number;
  discountTotal: number;
  grandTotal: number;
  promotion?: QuotePromotionModel | null;
  lines: QuoteLineModel[];
}

export interface BookingHoldSeatModel {
  seatInventoryId: string;
  seatCode: string;
  seatType: SeatType;
  price: number;
}

export interface BookingHoldModel {
  holdId: string;
  holdCode: string;
  showtimeId: string;
  status: string;
  expiresAtUtc: string;
  remainingSeconds: number;
  seatSubtotal: number;
  comboSubtotal: number;
  discountAmount: number;
  grandTotal: number;
  seats: BookingHoldSeatModel[];
}

interface MovieListItemResponseDto {
  movieId: string;
  title: string;
  slug: string;
  posterUrl?: string | null;
  durationMinutes: number;
  ratingLabel?: string | null;
  status: string;
  genres: string[];
}

interface MovieDetailResponseDto extends MovieListItemResponseDto {
  synopsis?: string | null;
  trailerUrl?: string | null;
  releaseDate?: string | null;
}

interface MovieHomeResponseDto {
  featuredMovies: MovieListItemResponseDto[];
  nowShowing: MovieListItemResponseDto[];
  comingSoon: MovieListItemResponseDto[];
}

interface MovieShowtimesResponseDto {
  movieId: string;
  title: string;
  slug: string;
  posterUrl?: string | null;
  dates: Array<{
    businessDate: string;
    cinemas: Array<{
      cinemaId: string;
      cinemaCode: string;
      cinemaName: string;
      showtimes: Array<{
        showtimeId: string;
        movieId: string;
        cinemaId: string;
        hallId: string;
        hallType: string;
        businessDate: string;
        startAtUtc: string;
        endAtUtc?: string | null;
        language: string;
        basePrice: number;
        status: string;
      }>;
    }>;
  }>;
}

interface ShowtimeListItemDto {
  showtimeId: string;
  movieId: string;
  movieTitle: string;
  cinemaId: string;
  cinemaName: string;
  hallId: string;
  hallName: string;
  hallType: string;
  businessDate: string;
  startAtUtc: string;
  language: string;
  basePrice: number;
  status: string;
}

interface ShowtimeDetailResponseDto {
  showtimeId: string;
  movieId: string;
  movieTitle: string;
  movieSlug: string;
  moviePosterUrl?: string | null;
  cinemaId: string;
  cinemaCode: string;
  cinemaName: string;
  hallId: string;
  hallCode: string;
  hallName: string;
  hallType: string;
  businessDate: string;
  startAtUtc: string;
  endAtUtc?: string | null;
  language: string;
  basePrice: number;
  status: string;
}

interface SeatMapResponseDto {
  showtimeId: string;
  hallId: string;
  hallType: string;
  seats: Array<{
    seatInventoryId: string;
    seatCode: string;
    row: string;
    col: number;
    seatType: string;
    status: string;
    price: number;
    coupleGroupCode?: string | null;
  }>;
}

interface PromotionResponseDto {
  id: string;
  code: string;
  name: string;
  description: string;
  category: string;
  status: string;
  validFromUtc?: string | null;
  validToUtc?: string | null;
  isAutoApplied: boolean;
}

interface SnackComboResponseDto {
  id: string;
  code: string;
  name: string;
  description: string;
  price: number;
  imageUrl?: string | null;
  isActive: boolean;
}

interface BookingQuoteResponseDto {
  showtimeId: string;
  seatSubtotal: number;
  serviceFeeTotal: number;
  comboSubtotal: number;
  discountTotal: number;
  grandTotal: number;
  promotion?: QuotePromotionModel | null;
  lines: QuoteLineModel[];
}

interface BookingHoldResponseDto {
  holdId: string;
  holdCode: string;
  showtimeId: string;
  status: string;
  expiresAtUtc: string;
  remainingSeconds: number;
  seatSubtotal: number;
  comboSubtotal: number;
  discountAmount: number;
  grandTotal: number;
  seats: Array<{
    seatInventoryId: string;
    seatCode: string;
    seatType: string;
    unitPrice: number;
  }>;
}

export interface QuoteRequestPayload {
  showtimeId: string;
  seatInventoryIds: string[];
  snackCombos: Array<{
    comboId: string;
    quantity: number;
  }>;
  promotionId?: string | null;
  paymentProvider?: string | null;
  birthday?: string | null;
}

export interface CreateBookingHoldPayload extends QuoteRequestPayload {
  sessionId?: string | null;
  guestCustomerId?: string | null;
}

const DEFAULT_POSTER =
  "https://images.unsplash.com/photo-1489599849927-2ee91cede3ba?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&q=80&w=1080";

const PROMO_IMAGES: Record<string, string> = {
  weekend: "https://images.unsplash.com/photo-1691480213129-106b2c7d1ee8?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&q=80&w=1080",
  combo: "https://images.unsplash.com/photo-1563381013529-1c922c80ac8d?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&q=80&w=1080",
  member: "https://images.unsplash.com/photo-1762028892696-1d6e9d8c294e?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&q=80&w=1080",
  bank: "https://images.unsplash.com/photo-1623295080944-9ba74d587748?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&q=80&w=1080",
  ticket: "https://images.unsplash.com/photo-1766425597359-08c8f7585ba4?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&q=80&w=1080",
  all: "https://images.unsplash.com/photo-1762028892696-1d6e9d8c294e?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&q=80&w=1080",
};

const PROMO_ACCENTS: Record<string, { from: string; to: string }> = {
  weekend: { from: "#a21caf", to: "#db2777" },
  combo: { from: "#ca8a04", to: "#d97706" },
  member: { from: "#c026d3", to: "#db2777" },
  bank: { from: "#0891b2", to: "#2563eb" },
  ticket: { from: "#9333ea", to: "#ec4899" },
  all: { from: "#7c3aed", to: "#6d28d9" },
};

const PROMO_UI_ID_BY_CODE: Record<string, string> = {
  WEEKEND: "f1",
  MOMO30: "f2",
  DATENIGHT: "f3",
  VCB25: "p4",
  GROUP20: "p5",
  COMBOGOLD: "p6",
  BIRTHDAY: "p7",
  EARLYBIRD: "p8",
};

export function resolvePromotionApiIdFromUiId(promotions: PromotionModel[], promoId?: string | null) {
  if (!promoId) {
    return undefined;
  }

  const matched = promotions.find(
    (promotion) =>
      promotion.id === promoId ||
      (PROMO_UI_ID_BY_CODE[promotion.discountLabel.toUpperCase()] ?? promotion.discountLabel) === promoId,
  );

  return matched?.id;
}

function formatDuration(minutes: number) {
  return `${minutes} min`;
}

function parseRating(label?: string | null) {
  if (!label) {
    return 8.5;
  }

  const parsed = Number.parseFloat(label.replace(/[^\d.]/g, ""));
  return Number.isFinite(parsed) ? parsed : 8.5;
}

function mapAgeRating(label?: string | null) {
  if (!label) {
    return "P";
  }

  const normalized = label.toUpperCase();
  if (normalized.includes("18")) return "T18";
  if (normalized.includes("16")) return "T16";
  if (normalized.includes("13")) return "T13";
  return "P";
}

function mapLanguage(value?: string | null): "sub" | "dub" {
  return value?.toLowerCase().includes("dub") ? "dub" : "sub";
}

function mapSeatType(value: string): SeatType {
  const normalized = value.toLowerCase();
  if (normalized.includes("vip")) return "vip";
  if (normalized.includes("couple")) return "couple";
  return "regular";
}

function mapSeatStatus(value: string): SeatStatus {
  const normalized = value.toLowerCase();
  if (normalized.includes("book")) return "booked";
  if (normalized.includes("hold")) return "held";
  return "available";
}

function formatTime(value: string) {
  return new Date(value).toLocaleTimeString("en-GB", {
    hour: "2-digit",
    minute: "2-digit",
  });
}

function formatReleaseDate(value?: string | null) {
  if (!value) {
    return undefined;
  }

  const parsed = new Date(value);
  if (Number.isNaN(parsed.getTime())) {
    return value;
  }

  return parsed.toLocaleDateString("en-GB");
}

function formatExpiry(value?: string | null) {
  if (!value) {
    return "While stocks last";
  }

  return new Date(value).toLocaleDateString("en-GB");
}

function mapMovieCard(dto: MovieListItemResponseDto): MovieCardModel {
  return {
    id: dto.slug || dto.movieId,
    apiId: dto.movieId,
    title: dto.title,
    genre: dto.genres.join(", "),
    rating: parseRating(dto.ratingLabel),
    duration: formatDuration(dto.durationMinutes),
    imageUrl: dto.posterUrl ?? DEFAULT_POSTER,
    isComingSoon: dto.status.toLowerCase().includes("coming"),
    ageRating: mapAgeRating(dto.ratingLabel),
  };
}

function mapPromotion(dto: PromotionResponseDto): PromotionModel {
  const category = dto.category || "all";
  const accent = PROMO_ACCENTS[category] ?? PROMO_ACCENTS.all;

  return {
    id: dto.id,
    title: dto.name,
    description: dto.description,
    category,
    discountLabel: dto.code,
    status: dto.status,
    imageUrl: PROMO_IMAGES[category] ?? PROMO_IMAGES.all,
    accentFrom: accent.from,
    accentTo: accent.to,
    condition: dto.isAutoApplied ? "Applied automatically when eligible" : "Select this offer before checkout",
    expiry: formatExpiry(dto.validToUtc),
  };
}

function groupShowtimesByMovieWithLookup(showtimes: ShowtimeListItemDto[], movieLookup: Map<string, MovieCardModel>) {
  const grouped = new Map<string, MovieShowtimesModel>();

  for (const item of showtimes) {
    const movie = movieLookup.get(item.movieId);
    const movieKey = movie?.id ?? item.movieId;
    const movieEntry =
      grouped.get(movieKey) ??
      {
        movie: movie ?? {
          id: item.movieId,
          apiId: item.movieId,
          title: item.movieTitle,
          genre: "Movie",
          rating: 8.5,
          duration: "Updating",
          imageUrl: DEFAULT_POSTER,
          isComingSoon: false,
          ageRating: "P",
        },
        cinemaSchedules: [],
      };

    let cinemaGroup = movieEntry.cinemaSchedules.find(
      (cinema) => cinema.cinemaId === item.cinemaId,
    );

    if (!cinemaGroup) {
      cinemaGroup = {
        cinemaId: item.cinemaId,
        cinemaName: item.cinemaName,
        cinemaAddress: item.hallName,
        showtimes: [],
      };
      movieEntry.cinemaSchedules.push(cinemaGroup);
    }

    cinemaGroup.showtimes.push({
      showtimeId: item.showtimeId,
      cinemaId: item.cinemaId,
      cinemaName: item.cinemaName,
      cinemaAddress: item.hallName,
      hallType: item.hallType,
      time: formatTime(item.startAtUtc),
      businessDate: item.businessDate,
      priceFrom: item.basePrice,
      status: item.status,
      language: mapLanguage(item.language),
    });

    grouped.set(movieKey, movieEntry);
  }

  return Array.from(grouped.values());
}

export async function fetchHomePageData() {
  // API FETCH NOTE:
  // Movies home needs both movie catalog data and promotion data, so we request them in parallel.
  // Backend endpoints: GET /api/movies/home and GET /api/movies-promotions?activeOnly=true.
  const [home, promotions] = await Promise.all([
    api.get<MovieHomeResponseDto>("/movies/home"),
    api.get<PromotionResponseDto[]>("/movies-promotions", { activeOnly: true }),
  ]);

  return {
    featuredMovies: home.featuredMovies.map(mapMovieCard),
    nowShowingMovies: home.nowShowing.map(mapMovieCard),
    comingSoonMovies: home.comingSoon.map(mapMovieCard),
    promotions: promotions.slice(0, 3).map(mapPromotion),
  };
}

export async function fetchMovieDetail(movieId: string) {
  // API FETCH NOTE:
  // movieId comes from the URL route /movies/:movieId and maps to GET /api/movies/{movieId}.
  const apiMovieId = await resolveMovieApiId(movieId);
  const response = await api.get<MovieDetailResponseDto>(`/movies/${apiMovieId}`);

  return {
    id: response.slug || response.movieId,
    apiId: response.movieId,
    title: response.title,
    genre: response.genres.join(", "),
    rating: parseRating(response.ratingLabel),
    duration: formatDuration(response.durationMinutes),
    imageUrl: response.posterUrl ?? DEFAULT_POSTER,
    isComingSoon: response.status.toLowerCase().includes("coming"),
    ageRating: mapAgeRating(response.ratingLabel),
    description: response.synopsis ?? "Synopsis is being updated.",
    language: "Updating",
    director: "Updating",
    cast: ["Updating"],
    releaseDate: formatReleaseDate(response.releaseDate),
    trailerUrl: response.trailerUrl ?? undefined,
  } satisfies MovieDetailModel;
}

export async function fetchMovieShowtimes(movieId: string, businessDate: string) {
  // API FETCH NOTE:
  // Query params are passed as the second argument; api.get turns this into
  // /api/movies/{movieId}/showtimes?businessDate=yyyy-mm-dd.
  const apiMovieId = await resolveMovieApiId(movieId);
  const response = await api.get<MovieShowtimesResponseDto>(`/movies/${apiMovieId}/showtimes`, {
    businessDate,
  });

  const dateGroup =
    response.dates.find(
      (
        group: MovieShowtimesResponseDto["dates"][number],
      ) => group.businessDate === businessDate,
    ) ?? response.dates[0];

  return {
    movieId: response.slug || response.movieId,
    apiMovieId: response.movieId,
    title: response.title,
    posterUrl: response.posterUrl ?? DEFAULT_POSTER,
    cinemas: (dateGroup?.cinemas ?? []).map(
      (cinema: MovieShowtimesResponseDto["dates"][number]["cinemas"][number]) => ({
        cinemaId: cinema.cinemaId,
        cinemaName: cinema.cinemaName,
        cinemaAddress: cinema.cinemaCode,
        showtimes: cinema.showtimes.map(
          (
            showtime: MovieShowtimesResponseDto["dates"][number]["cinemas"][number]["showtimes"][number],
          ) => ({
            showtimeId: showtime.showtimeId,
            cinemaId: cinema.cinemaId,
            cinemaName: cinema.cinemaName,
            cinemaAddress: cinema.cinemaCode,
            hallType: showtime.hallType,
            time: formatTime(showtime.startAtUtc),
            businessDate: showtime.businessDate,
            priceFrom: showtime.basePrice,
            status: showtime.status,
            language: mapLanguage(showtime.language),
          }),
        ),
      }),
    ),
  };
}

export async function fetchSchedules(params: {
  movieId?: string;
  cinemaId?: string;
  businessDate: string;
  hallType?: string;
  language?: string;
}) {
  // API FETCH NOTE:
  // The schedule page calls GET /api/showtimes with filters, then groups the flat backend list by movie.
  const [movieList, apiMovieId] = await Promise.all([
    fetchMovieList(),
    params.movieId ? resolveMovieApiId(params.movieId) : Promise.resolve(undefined),
  ]);
  const response = await api.get<ShowtimeListItemDto[]>("/showtimes", {
    ...params,
    movieId: apiMovieId,
  });
  const movieLookup = new Map<string, MovieCardModel>();
  movieList.forEach((movie) => {
    if (movie.apiId) {
      movieLookup.set(movie.apiId, movie);
    }
  });
  return groupShowtimesByMovieWithLookup(response, movieLookup);
}

export async function fetchMovieList(status?: string) {
  // API FETCH NOTE:
  // Optional status becomes /api/movies?status=NowShowing or /api/movies?status=ComingSoon.
  const response = await api.get<MovieListItemResponseDto[]>("/movies", status ? { status } : undefined);
  return response.map(mapMovieCard);
}

async function resolveMovieApiId(movieId: string) {
  if (/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(movieId)) {
    return movieId;
  }

  const movies = await fetchMovieList();
  const matched = movies.find((movie) => movie.id === movieId);
  return matched?.apiId ?? movieId;
}

export async function fetchPromotions(activeOnly = true) {
  // API FETCH NOTE:
  // Promotions are fetched from backend, then mapped to UI colors/images used by the promotions page.
  const response = await api.get<PromotionResponseDto[]>("/movies-promotions", { activeOnly });
  return response.map(mapPromotion);
}

export async function fetchSnackCombos() {
  // API FETCH NOTE:
  // Snack combos are loaded before seat checkout so the quote API can price selected combos.
  const response = await api.get<SnackComboResponseDto[]>("/snack-combos");
  return response
    .filter((combo: SnackComboResponseDto) => combo.isActive)
    .map((combo: SnackComboResponseDto) => ({
      id: combo.id,
      code: combo.code,
      name: combo.name,
      description: combo.description,
      price: combo.price,
      imageUrl: combo.imageUrl ?? undefined,
    }));
}

export async function fetchShowtimeDetail(showtimeId: string) {
  // API FETCH NOTE:
  // Seat selection and checkout both use this to recover showtime context from GET /api/showtimes/{showtimeId}.
  const response = await api.get<ShowtimeDetailResponseDto>(`/showtimes/${showtimeId}`);

  return {
    showtimeId: response.showtimeId,
    movieId: response.movieId,
    movieTitle: response.movieTitle,
    moviePosterUrl: response.moviePosterUrl ?? DEFAULT_POSTER,
    cinemaId: response.cinemaId,
    cinemaName: response.cinemaName,
    hallId: response.hallId,
    hallName: response.hallName,
    hallType: response.hallType,
    businessDate: response.businessDate,
    startAtUtc: response.startAtUtc,
    language: mapLanguage(response.language),
    basePrice: response.basePrice,
    status: response.status,
  } satisfies ShowtimeDetailModel;
}

export async function fetchSeatMap(showtimeId: string) {
  // API FETCH NOTE:
  // This is the live seat map fetch. The response includes seatInventoryId values needed by quoteBooking().
  const response = await api.get<SeatMapResponseDto>(`/showtimes/${showtimeId}/seat-map`);
  return {
    showtimeId: response.showtimeId,
    hallId: response.hallId,
    hallType: response.hallType,
    seats: response.seats.map((seat: SeatMapResponseDto["seats"][number]) => ({
      seatInventoryId: seat.seatInventoryId,
      seatCode: seat.seatCode,
      row: seat.row,
      col: seat.col,
      seatType: mapSeatType(seat.seatType),
      status: mapSeatStatus(seat.status),
      price: seat.price,
      coupleGroupCode: seat.coupleGroupCode ?? undefined,
    })),
  } satisfies SeatMapModel;
}

export async function quoteBooking(payload: QuoteRequestPayload) {
  // API FETCH NOTE:
  // This is the important pricing call: selected seats, combos, promo and payment provider are sent to
  // POST /api/bookings/quote. Backend recalculates totals so the UI does not trust client-side prices.
  const response = await api.post<BookingQuoteResponseDto, QuoteRequestPayload>("/bookings/quote", {
    showtimeId: payload.showtimeId,
    seatInventoryIds: payload.seatInventoryIds,
    snackCombos: payload.snackCombos,
    promotionId: payload.promotionId ?? undefined,
    paymentProvider: payload.paymentProvider ?? undefined,
    birthday: payload.birthday ?? undefined,
  });

  return {
    showtimeId: response.showtimeId,
    seatSubtotal: response.seatSubtotal,
    serviceFeeTotal: response.serviceFeeTotal,
    comboSubtotal: response.comboSubtotal,
    discountTotal: response.discountTotal,
    grandTotal: response.grandTotal,
    promotion: response.promotion,
    lines: response.lines,
  } satisfies BookingQuoteModel;
}

function mapBookingHold(response: BookingHoldResponseDto) {
  return {
    holdId: response.holdId,
    holdCode: response.holdCode,
    showtimeId: response.showtimeId,
    status: response.status,
    expiresAtUtc: response.expiresAtUtc,
    remainingSeconds: response.remainingSeconds,
    seatSubtotal: response.seatSubtotal,
    comboSubtotal: response.comboSubtotal,
    discountAmount: response.discountAmount,
    grandTotal: response.grandTotal,
    seats: response.seats.map((seat) => ({
      seatInventoryId: seat.seatInventoryId,
      seatCode: seat.seatCode,
      seatType: mapSeatType(seat.seatType),
      price: seat.unitPrice,
    })),
  } satisfies BookingHoldModel;
}

export async function createBookingHold(payload: CreateBookingHoldPayload) {
  const response = await api.post<BookingHoldResponseDto, CreateBookingHoldPayload>("/bookings/hold", {
    showtimeId: payload.showtimeId,
    seatInventoryIds: payload.seatInventoryIds,
    snackCombos: payload.snackCombos,
    promotionId: payload.promotionId ?? undefined,
    paymentProvider: payload.paymentProvider ?? undefined,
    birthday: payload.birthday ?? undefined,
    sessionId: payload.sessionId ?? undefined,
    guestCustomerId: payload.guestCustomerId ?? undefined,
  });

  return mapBookingHold(response);
}

export async function fetchBookingHold(holdId: string) {
  const response = await api.get<BookingHoldResponseDto>(`/bookings/holds/${holdId}`);
  return mapBookingHold(response);
}

export async function confirmBookingHold(holdId: string) {
  // DAY5 TEST-ONLY CONFIRM FLOW:
  // API này chỉ phục vụ test hold -> booked. Khi backend có booking/payment thật, đổi sang endpoint confirm chính thức.
  const response = await api.post<BookingHoldResponseDto>(`/bookings/holds/${holdId}/confirm`);
  return mapBookingHold(response);
}

export async function releaseBookingHold(holdId: string) {
  await api.delete(`/bookings/holds/${holdId}`);
}
