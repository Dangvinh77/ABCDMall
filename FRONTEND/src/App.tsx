import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { HomePage } from './pages/home/HomePage';
import { Footer } from './core/layouts/Footer';

function App() {
  return (
    <Router>
      <div className="flex flex-col min-h-screen">
        {/* Phần nội dung chính (mở rộng linh hoạt để đẩy Footer xuống đáy) */}
        <div className="flex-grow">
          <Routes>
            <Route path="/" element={<HomePage />} />
            
            {/* Các trang giả lập */}
            <Route path="/shops" element={<div className="p-20 text-center text-2xl font-bold min-h-[50vh]">Trang Cửa hàng (Sắp ra mắt)</div>} />
            <Route path="/food-court" element={<div className="p-20 text-center text-2xl font-bold min-h-[50vh]">Trang Ẩm thực (Sắp ra mắt)</div>} />
            <Route path="/movies" element={<div className="p-20 text-center text-2xl font-bold min-h-[50vh]">Trang Đặt vé phim (Sắp ra mắt)</div>} />
            <Route path="/gallery" element={<div className="p-20 text-center text-2xl font-bold min-h-[50vh]">Trang Thư viện (Sắp ra mắt)</div>} />
            <Route path="/map" element={<div className="p-20 text-center text-2xl font-bold min-h-[50vh]">Trang Sơ đồ (Sắp ra mắt)</div>} />
            <Route path="/contact" element={<div className="p-20 text-center text-2xl font-bold min-h-[50vh]">Trang Liên hệ (Sắp ra mắt)</div>} />
          </Routes>
        </div>

        {/* Chân trang luôn hiển thị ở dưới cùng */}
        <Footer />
      </div>
    </Router>
  );
}

export default App;