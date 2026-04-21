import { api } from "../../../core/api/api";

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
  getBookings: (params?: { status?: string }) => api.get<MoviesAdminBooking[]>("/movies/admin/bookings", params),
  getBookingById: (bookingId: string) => api.get<MoviesAdminBookingDetail>(`/movies/admin/bookings/${bookingId}`),
  getMoviesAdmins: () => api.get<MoviesAdminUser[]>("/Auth/movies-admins"),
  createMoviesAdmin: (payload: MoviesAdminUserUpsertRequest) =>
    api.post<{ message: string }, MoviesAdminUserUpsertRequest>("/Auth/movies-admins", {
      ...payload,
      role: "MoviesAdmin",
      shopName: null,
    } as MoviesAdminUserUpsertRequest & { role: string; shopName: null }),
  updateMoviesAdmin: (userId: string, payload: MoviesAdminUserUpsertRequest) =>
    api.put<{ message: string }, MoviesAdminUserUpsertRequest>(`/Auth/movies-admins/${userId}`, {
      ...payload,
      role: "MoviesAdmin",
      shopName: null,
    } as MoviesAdminUserUpsertRequest & { role: string; shopName: null }),
  deleteMoviesAdmin: (userId: string) => api.delete<void>(`/Auth/movies-admins/${userId}`),
  getPayments: (params?: { status?: string; provider?: string }) =>
    api.get<MoviesAdminPayment[]>("/movies/admin/payments", params),
  getPaymentById: (paymentId: string) => api.get<MoviesAdminPaymentDetail>(`/movies/admin/payments/${paymentId}`),
  getEmailLogs: () => api.get<MoviesAdminEmailLog[]>("/movies/admin/emails"),
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
