// 1. Import các thư viện cần thiết
import { Routes, Route } from "react-router-dom";

// Imports từ tính năng Food Court & Directory
import FoodPage from "./features/food/pages/FoodPage";
import FoodDetailPage from "./features/food/pages/FoodDetailPage";
import { MapPage } from './features/directory/MapPage';

// Imports từ tính năng Shop & Core Layout
import { HomePage } from "./pages/home/HomePage";
import { Footer } from "./core/layouts/Footer";

function App() {
  return (
    /* Sử dụng Flexbox để tạo Sticky Footer: 
       min-h-screen đảm bảo layout luôn cao ít nhất bằng màn hình,
       flex-grow ở phần thân sẽ đẩy Footer xuống đáy.
    */
    <div className="flex flex-col min-h-screen">
      
      {/* Content Area */}
      <div className="flex-grow">
        <Routes>
          {/* Trang chủ */}
          <Route path="/" element={<HomePage />} />

          {/* Food Court Features */}
          <Route path="/food-court" element={<FoodPage />} />
          <Route path="/food/:slug" element={<FoodDetailPage />} />
          <Route path="/mall/:mall/:slug" element={<FoodDetailPage />} />

          {/* Directory & Maps */}
          <Route path="/map" element={<MapPage />} />

          {/* Các trang đang phát triển (Placeholders) */}
          <Route path="/shops" element={<Placeholder title="Trang Cửa hàng" />} />
          <Route path="/movies" element={<Placeholder title="Trang Đặt vé phim" />} />
          <Route path="/gallery" element={<Placeholder title="Trang Thư viện" />} />
          <Route path="/contact" element={<Placeholder title="Trang Liên hệ" />} />
        </Routes>
      </div>

      {/* Footer luôn hiển thị ở cuối trang */}
      <Footer />
    </div>
  );
}

const Placeholder = ({ title }: { title: string }) => (
  <div className="p-20 text-center text-2xl font-bold min-h-[50vh]">
    {title} (Sắp ra mắt)
  </div>
);

export default App;