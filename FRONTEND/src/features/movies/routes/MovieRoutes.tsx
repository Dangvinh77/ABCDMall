import { Navigate, Route, Routes } from 'react-router-dom'
import { MovieHomePage } from '../pages/MovieHomePage'
import { MovieDetailPage } from '../pages/MovieDetailPage'
import { PromotionsPage } from '../pages/PromotionsPage'
import { SchedulePage } from '../pages/SchedulesPage'
import { SeatSelectionPage } from '../pages/SeatSelectionPage'
import { CheckoutPage } from '../pages/CheckOutPage'
import { MoviePaymentSuccessPage } from '../pages/MoviePaymentSuccessPage'
import { MoviePaymentCancelPage } from '../pages/MoviePaymentCancelPage'

export function MoviesRoutes() {
  return (
    <Routes>
      <Route index element={<MovieHomePage />} />
      <Route path="promotions" element={<PromotionsPage />} />
      <Route path="showtimes" element={<SchedulePage />} />
      <Route path=":movieId" element={<MovieDetailPage />} />
      <Route path=":movieId/booking" element={<SeatSelectionPage />} />
      <Route path=":movieId/checkout" element={<CheckoutPage />} />
      <Route path="payment/success" element={<MoviePaymentSuccessPage />} />
      <Route path="payment/cancel" element={<MoviePaymentCancelPage />} />
      <Route path="*" element={<Navigate to="/movies" replace />} />
    </Routes>
  )
}
