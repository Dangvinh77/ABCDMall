import { BrowserRouter as Router, Routes, Route } from "react-router-dom";

// Imports từ nhánh Minh/foodcourt
import FoodPage from "./features/food/pages/FoodPage";
import FoodDetailPage from "./features/food/pages/FoodDetailPage";

// Imports từ nhánh fe-shop
import { HomePage } from "./pages/home/HomePage";
import { Footer } from "./core/layouts/Footer";

function App() {
  return (
    //<Router>
      <div className="flex flex-col min-h-screen">
        {/* Phần nội dung chính: Chứa tất cả các Routes */}
        <div className="flex-grow">
          <Routes>
            {/* Trang chủ */}
            <Route path="/" element={<HomePage />} />

            {/* Các Routes thuộc tính năng Food Court (đã gộp) */}
            <Route path="/food-court" element={<FoodPage />} />
            <Route path="/food/:slug" element={<FoodDetailPage />} />
            <Route path="/mall/:mall/:slug" element={<FoodDetailPage />} />

            {/* Các trang khác đang phát triển */}
            <Route path="/shops" element={<div className="p-20 text-center text-2xl font-bold min-h-[50vh]">Trang Cửa hàng (Sắp ra mắt)</div>} />
            <Route path="/movies" element={<div className="p-20 text-center text-2xl font-bold min-h-[50vh]">Trang Đặt vé phim (Sắp ra mắt)</div>} />
            <Route path="/gallery" element={<div className="p-20 text-center text-2xl font-bold min-h-[50vh]">Trang Thư viện (Sắp ra mắt)</div>} />
            <Route path="/map" element={<div className="p-20 text-center text-2xl font-bold min-h-[50vh]">Trang Sơ đồ (Sắp ra mắt)</div>} />
            <Route path="/contact" element={<div className="p-20 text-center text-2xl font-bold min-h-[50vh]">Trang Liên hệ (Sắp ra mắt)</div>} />
          </Routes>
        </div>

        {/* Chân trang luôn hiển thị ở dưới cùng nhờ vào flex-grow ở trên */}
        <Footer />
      </div>
  //  </Router>
  );
}

export default App;