// 1. Chỉ import Routes và Route (Bỏ BrowserRouter)
import { Routes, Route } from "react-router-dom";

// Imports từ nhánh Minh/foodcourt
import FoodPage from "./features/food/pages/FoodPage";
import FoodDetailPage from "./features/food/pages/FoodDetailPage";
import { MapPage } from './features/directory/MapPage';

// Imports từ nhánh fe-shop
import { HomePage } from "./pages/home/HomePage";
import { Footer } from "./core/layouts/Footer";

function App() {
  return (
    // 2. Xóa thẻ <Router> bao quanh, chỉ giữ lại cấu trúc Layout
    <div className="flex flex-col min-h-screen">
      {/* Phần nội dung chính (mở rộng linh hoạt để đẩy Footer xuống đáy) */}
      <div className="flex-grow">
        <Routes>
          {/* Trang chủ */}
          <Route path="/" element={<HomePage />} />

          {/* Các Routes thuộc tính năng Food Court */}
          <Route path="/food-court" element={<FoodPage />} />
          <Route path="/food/:slug" element={<FoodDetailPage />} />
          <Route path="/mall/:mall/:slug" element={<FoodDetailPage />} />

          {/* Các trang giả lập khác */}
          <Route path="/shops" element={<div className="p-20 text-center text-2xl font-bold min-h-[50vh]">Trang Cửa hàng (Sắp ra mắt)</div>} />
          <Route path="/movies" element={<div className="p-20 text-center text-2xl font-bold min-h-[50vh]">Trang Đặt vé phim (Sắp ra mắt)</div>} />
          <Route path="/gallery" element={<div className="p-20 text-center text-2xl font-bold min-h-[50vh]">Trang Thư viện (Sắp ra mắt)</div>} />
          <Route path="/map" element={<MapPage />} />
          <Route path="/contact" element={<div className="p-20 text-center text-2xl font-bold min-h-[50vh]">Trang Liên hệ (Sắp ra mắt)</div>} />
        </Routes>
      </div>

      {/* Chân trang luôn hiển thị ở dưới cùng */}
      <Footer />
    </div>
  );
}

export default App;