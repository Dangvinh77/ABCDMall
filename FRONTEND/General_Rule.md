## ⚙️ Thiết lập Routes trong App.tsx

Ví dụ: movies chỉ là một feature trong ABCD Mall, nên nên tách theo kiểu:

```bash
// src/features/movies/routes/MoviesRoutes.tsx
import { Navigate, Route, Routes } from 'react-router-dom'
import { MovieHomePage } from '../pages/MovieHomePage'
import { MovieDetailPage } from '../pages/MovieDetailPage'
import { PromotionsPage } from '../pages/PromotionsPage'
import { SchedulePage } from '../pages/SchedulesPage'
import { SeatSelectionPage } from '../pages/SeatSelectionPage'
import { CheckoutPage } from '../pages/CheckOutPage'

export function MoviesRoutes() {
  return (
    <Routes>
      <Route index element={<MovieHomePage />} />
      <Route path="promotions" element={<PromotionsPage />} />
      <Route path="showtimes" element={<SchedulePage />} />
      <Route path=":movieId" element={<MovieDetailPage />} />
      <Route path=":movieId/booking" element={<SeatSelectionPage />} />
      <Route path=":movieId/checkout" element={<CheckoutPage />} />
      <Route path="*" element={<Navigate to="/movies" replace />} />
    </Routes>
  )
}
```

### 💠 src/routes ngoài root thì nên giữ để làm gì ?

Là nơi khai báo route cấp app như AppRoutes.tsx
Nối các feature route lại với nhau
Xử lý route chung như 404, redirect /, layout shell của toàn mall
Ví dụ:
```bash
// src/routes/AppRoutes.tsx
import { Routes, Route, Navigate } from 'react-router-dom'
import { MoviesRoutes } from '../features/movies/routes/MoviesRoutes'
import { ShopsRoutes } from '../features/shops/routes/ShopsRoutes'

export function AppRoutes() {
  return (
    <Routes>
      <Route path="/" element={<Navigate to="/movies" replace />} />
      <Route path="/movies/*" element={<MoviesRoutes />} />
      <Route path="/shops/*" element={<ShopsRoutes />} />
      <Route path="*" element={<div>404</div>} />
    </Routes>
  )
```
### 💠 App giữ route cấp cao của toàn mall.
Mỗi feature như **movies, shops, auth** tự có file route riêng.
**App.tsx** chỉ mount route entry của từng feature.
Ví dụ kiến trúc:
```bash
// src/App.tsx
import { AppRoutes } from './routes/AppRoutes'

function App() {
  return <AppRoutes />
}

export default App
```

