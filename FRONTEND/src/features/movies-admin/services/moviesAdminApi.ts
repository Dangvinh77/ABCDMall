import { api, http } from "../../../core/api/api";

export interface MoviesAdminDashboardResponse {
  activeMovies: number;
  upcomingShowtimes: number;
  totalBookings: number;
  paidRevenue: number;
  upcomingShowtimesSnapshot: Array<{
    showtimeId: string;
    movieId: string;
    movieTitle: string;
    cinemaName: string;
    hallName: string;
    businessDate: string;
    startAtUtc: string;
    status: string;
  }>;
  recentBookings: Array<{
    bookingId: string;
    bookingCode: string;
    customerName: string;
    movieTitle: string;
    createdAtUtc: string;
    grandTotal: number;
    status: string;
    paymentStatus: string;
  }>;
  topMovies: Array<{
    movieId: string;
    movieTitle: string;
    bookedSeats: number;
    revenue: number;
  }>;
}

export interface MoviesAdminMovie {
  id: string;
  title: string;
  slug: string;
  synopsis?: string | null;
  durationMinutes: number;
  posterUrl?: string | null;
  trailerUrl?: string | null;
  releaseDate?: string | null;
  ratingLabel?: string | null;
  defaultLanguage: string;
  status: string;
  showtimeCount: number;
}

export interface MoviesAdminMovieUpsertRequest {
  title: string;
  slug?: string;
  synopsis?: string;
  durationMinutes: number;
  posterUrl?: string;
  trailerUrl?: string;
  releaseDate?: string;
  ratingLabel?: string;
  defaultLanguage: string;
  status: string;
}

export interface MoviesAdminShowtime {
  id: string;
  movieId: string;
  movieTitle: string;
  cinemaId: string;
  cinemaName: string;
  hallId: string;
  hallName: string;
  businessDate: string;
  startAtUtc: string;
  endAtUtc?: string | null;
  language: string;
  basePrice: number;
  status: string;
}

export interface MoviesAdminPromotionRule {
  ruleType: string;
  ruleValue: string;
  thresholdValue?: number | null;
  sortOrder: number;
  isRequired: boolean;
}

export interface MoviesAdminPromotion {
  id: string;
  code: string;
  name: string;
  description: string;
  category: string;
  status: string;
  validFromUtc?: string | null;
  validToUtc?: string | null;
  percentageValue?: number | null;
  flatDiscountValue?: number | null;
  maximumDiscountAmount?: number | null;
  minimumSpendAmount?: number | null;
  maxRedemptions?: number | null;
  maxRedemptionsPerCustomer?: number | null;
  isAutoApplied: boolean;
  imageUrl?: string | null;
  badgeText?: string | null;
  accentFrom?: string | null;
  accentTo?: string | null;
  displayCondition?: string | null;
  isFeatured: boolean;
  displayPriority: number;
  ruleCount: number;
  redemptionCount: number;
}

export interface MoviesAdminPromotionDetail extends MoviesAdminPromotion {
  metadataJson?: string | null;
  rules: MoviesAdminPromotionRule[];
}

export interface MoviesAdminPromotionUpsertRequest {
  code: string;
  name: string;
  description: string;
  category: string;
  status: string;
  validFromUtc?: string;
  validToUtc?: string;
  percentageValue?: number;
  flatDiscountValue?: number;
  maximumDiscountAmount?: number;
  minimumSpendAmount?: number;
  maxRedemptions?: number;
  maxRedemptionsPerCustomer?: number;
  isAutoApplied: boolean;
  imageUrl?: string;
  badgeText?: string;
  accentFrom?: string;
  accentTo?: string;
  displayCondition?: string;
  isFeatured: boolean;
  displayPriority: number;
  metadataJson?: string;
  rules: MoviesAdminPromotionRule[];
}

export interface MoviesAdminShowtimeUpsertRequest {
  movieId: string;
  cinemaId: string;
  hallId: string;
  businessDate: string;
  startAtUtc: string;
  basePrice: number;
  language: string;
  status: string;
}

export interface MoviesAdminBooking {
  id: string;
  bookingCode: string;
  showtimeId: string;
  movieTitle: string;
  cinemaName: string;
  showtimeStartAtUtc: string;
  customerName: string;
  customerEmail: string;
  customerPhoneNumber: string;
  grandTotal: number;
  currency: string;
  status: string;
  paymentStatus: string;
  createdAtUtc: string;
}

export interface MoviesAdminBookingDetail {
  id: string;
  bookingCode: string;
  showtimeId: string;
  movieTitle: string;
  cinemaName: string;
  hallName: string;
  businessDate: string;
  startAtUtc: string;
  customerName: string;
  customerEmail: string;
  customerPhoneNumber: string;
  seatSubtotal: number;
  comboSubtotal: number;
  serviceFee: number;
  discountAmount: number;
  grandTotal: number;
  currency: string;
  status: string;
  paymentStatus: string;
  providerTransactionId?: string | null;
  failureReason?: string | null;
  createdAtUtc: string;
  items: Array<{
    itemType: string;
    itemCode: string;
    description: string;
    quantity: number;
    unitPrice: number;
    lineTotal: number;
  }>;
}

export interface MoviesAdminLookups {
  movies: Array<{ id: string; name: string }>;
  cinemas: Array<{ id: string; name: string }>;
  halls: Array<{ id: string; cinemaId: string; name: string }>;
}

export interface MoviesAdminUser {
  id?: string;
  email: string;
  role: string;
  fullName?: string | null;
  address?: string | null;
  cccd?: string | null;
  createdAt?: string | null;
}

export interface MoviesAdminUserUpsertRequest {
  email: string;
  password?: string;
  fullName: string;
  address?: string;
  cccd?: string;
}

export interface MoviesAdminUserCreateRequest extends MoviesAdminUserUpsertRequest {
  password: string;
}

export interface MoviesAdminUserUpdateRequest {
  email: string;
  fullName: string;
  address?: string;
  cccd?: string;
}

export interface MoviesAdminPayment {
  id: string;
  bookingId: string;
  bookingCode: string;
  movieTitle: string;
  customerEmail: string;
  provider: string;
  status: string;
  amount: number;
  currency: string;
  providerTransactionId?: string | null;
  createdAtUtc: string;
  updatedAtUtc: string;
  completedAtUtc?: string | null;
}

export interface MoviesAdminPaymentDetail extends MoviesAdminPayment {
  failureReason?: string | null;
  callbackPayloadJson?: string | null;
}

export interface MoviesAdminEmailLog {
  bookingId: string;
  bookingCode: string;
  customerEmail: string;
  movieTitle: string;
  deliveryStatus: string;
  pdfFileName?: string | null;
  issuedAtUtc: string;
  emailSentAtUtc?: string | null;
  emailSendError?: string | null;
  outboxStatus: string;
  outboxRetryCount: number;
  outboxLastError?: string | null;
}

export interface MoviesAdminRevenueReport {
  dateFromUtc?: string | null;
  dateToUtc?: string | null;
  totalPaidRevenue: number;
  totalBookings: number;
  successfulPayments: number;
  failedPayments: number;
  byMovie: Array<{ label: string; revenue: number; bookingCount: number }>;
  byCinema: Array<{ label: string; revenue: number; bookingCount: number }>;
  byProvider: Array<{ label: string; revenue: number; bookingCount: number }>;
}

export const moviesAdminApi = {
  getDashboard: () => api.get<MoviesAdminDashboardResponse>("/movies/admin/dashboard"),
  getLookups: () => api.get<MoviesAdminLookups>("/movies/admin/lookups"),
  getMovies: () => api.get<MoviesAdminMovie[]>("/movies/admin/movies"),
  getPromotions: (params?: { status?: string; query?: string; activeOnly?: boolean }) =>
    api.get<MoviesAdminPromotion[]>("/movies/admin/promotions", params),
  getPromotionById: (promotionId: string) =>
    api.get<MoviesAdminPromotionDetail>(`/movies/admin/promotions/${promotionId}`),
  createPromotion: (payload: MoviesAdminPromotionUpsertRequest) =>
    api.post<MoviesAdminPromotionDetail, MoviesAdminPromotionUpsertRequest>("/movies/admin/promotions", payload),
  updatePromotion: (promotionId: string, payload: MoviesAdminPromotionUpsertRequest) =>
    api.put<MoviesAdminPromotionDetail, MoviesAdminPromotionUpsertRequest>(`/movies/admin/promotions/${promotionId}`, payload),
  deletePromotion: (promotionId: string) => api.delete<void>(`/movies/admin/promotions/${promotionId}`),
  uploadPromotionImage: async (file: File): Promise<{ imageUrl: string }> => {
    try {
      const formData = new FormData();
      formData.append("file", file);
      const response = await http.post<{ imageUrl: string }>("/movies/admin/promotions/upload-image", formData);
      return response.data;
    } catch (error) {
      if (error instanceof Error) {
        throw error;
      }

      throw new Error("Unable to upload promotion image.");
    }
  },
  createMovie: (payload: MoviesAdminMovieUpsertRequest) =>
    api.post<MoviesAdminMovie, MoviesAdminMovieUpsertRequest>("/movies/admin/movies", payload),
  updateMovie: (movieId: string, payload: MoviesAdminMovieUpsertRequest) =>
    api.put<MoviesAdminMovie, MoviesAdminMovieUpsertRequest>(`/movies/admin/movies/${movieId}`, payload),
  deleteMovie: (movieId: string) => api.delete<void>(`/movies/admin/movies/${movieId}`),
  getShowtimes: (params?: { movieId?: string; businessDate?: string }) =>
    api.get<MoviesAdminShowtime[]>("/movies/admin/showtimes", params),
  createShowtime: (payload: MoviesAdminShowtimeUpsertRequest) =>
    api.post<MoviesAdminShowtime, MoviesAdminShowtimeUpsertRequest>("/movies/admin/showtimes", payload),
  updateShowtime: (showtimeId: string, payload: MoviesAdminShowtimeUpsertRequest) =>
    api.put<MoviesAdminShowtime, MoviesAdminShowtimeUpsertRequest>(`/movies/admin/showtimes/${showtimeId}`, payload),
  deleteShowtime: (showtimeId: string) => api.delete<void>(`/movies/admin/showtimes/${showtimeId}`),
  getBookings: (params?: {
    status?: string;
    paymentStatus?: string;
    movieId?: string;
    cinemaId?: string;
    query?: string;
    dateFromUtc?: string;
    dateToUtc?: string;
  }) => api.get<MoviesAdminBooking[]>("/movies/admin/bookings", params),
  getBookingById: (bookingId: string) => api.get<MoviesAdminBookingDetail>(`/movies/admin/bookings/${bookingId}`),
  getMoviesAdmins: () => api.get<MoviesAdminUser[]>("/Auth/movies-admins"),
  createMoviesAdmin: (payload: MoviesAdminUserCreateRequest) =>
    api.post<{ message: string }, MoviesAdminUserCreateRequest>("/Auth/movies-admins", {
      ...payload,
      role: "MoviesAdmin",
      shopName: null,
    } as MoviesAdminUserCreateRequest & { role: string; shopName: null }),
  updateMoviesAdmin: (userId: string, payload: MoviesAdminUserUpdateRequest) =>
    api.put<{ message: string }, MoviesAdminUserUpdateRequest>(`/Auth/movies-admins/${userId}`, {
      ...payload,
      role: "MoviesAdmin",
      shopName: null,
    } as MoviesAdminUserUpdateRequest & { role: string; shopName: null }),
  deleteMoviesAdmin: (userId: string) => api.delete<void>(`/Auth/movies-admins/${userId}`),
  getPayments: (params?: {
    status?: string;
    provider?: string;
    movieId?: string;
    cinemaId?: string;
    query?: string;
    dateFromUtc?: string;
    dateToUtc?: string;
  }) =>
    api.get<MoviesAdminPayment[]>("/movies/admin/payments", params),
  getPaymentById: (paymentId: string) => api.get<MoviesAdminPaymentDetail>(`/movies/admin/payments/${paymentId}`),
  getEmailLogs: (params?: { query?: string; deliveryStatus?: string; outboxStatus?: string }) =>
    api.get<MoviesAdminEmailLog[]>("/movies/admin/emails", params),
  resendTicketEmail: (bookingId: string) => api.post<{ message: string }>(`/movies/admin/emails/${bookingId}/resend`),
  getRevenueReport: (params?: {
    dateFromUtc?: string;
    dateToUtc?: string;
    movieId?: string;
    cinemaId?: string;
    provider?: string;
    paymentStatus?: string;
  }) => api.get<MoviesAdminRevenueReport>("/movies/admin/revenue", params),
};
