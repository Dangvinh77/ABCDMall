import { AppRoutes } from "./routes/AppRoutes";
import { Header } from "./core/layouts/Header"; // Đã import Header
import { Footer } from "./core/layouts/Footer";

function App() {
  return (
    <div className="flex flex-col min-h-screen relative">
      
      {/* THANH ĐIỀU HƯỚNG SẼ LUÔN NẰM TRÊN CÙNG */}
      <Header />

      {/* Content Area - Thêm pt-20 để chừa không gian cho Header (cao h-20) */}
      <div className="flex-grow pt-20">
        <AppRoutes /> 
      </div>

      <Footer />
    </div>
  );
}

export default App;