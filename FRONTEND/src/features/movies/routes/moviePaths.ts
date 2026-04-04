export const moviePaths = {
  home: () => '/movies',
  promotions: () => '/movies/promotions',
  showtimes: () => '/movies/showtimes',
  detail: (movieId: string) => `/movies/${movieId}`,
  booking: (movieId: string) => `/movies/${movieId}/booking`,
  checkout: (movieId: string) => `/movies/${movieId}/checkout`,
}
