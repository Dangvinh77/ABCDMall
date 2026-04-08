import { useState, useEffect } from 'react';
import { FloorPlan, MapLocation } from '../types/map.types';

// ==========================================
// 1. DỮ LIỆU GIẢ (MOCK DATA) ĐỂ TEST UI
// ==========================================
const MOCK_FLOORS: FloorPlan[] = [
  {
    id: "f1",
    floorLevel: "L1",
    description: "Tầng 1 - Thời trang Cao cấp & Mỹ phẩm",
    // Dùng một ảnh mặt bằng giả lập (bạn có thể thay bằng SVG thật sau này)
    blueprintImageUrl: "https://www.transparenttextures.com/patterns/blueprint.png",
    locations: [
      { shopId: "s1", shopName: "Dior Beauty", locationSlot: "L1-01", x_Coordinate: 25, y_Coordinate: 40, storefrontImageUrl: "https://images.unsplash.com/photo-1596462502278-27bfdc403348?q=80&w=800&auto=format&fit=crop" },
      { shopId: "s2", shopName: "Zara Flagship", locationSlot: "L1-05", x_Coordinate: 65, y_Coordinate: 55, storefrontImageUrl: "https://images.unsplash.com/photo-1441984904996-e0b6ba687e08?q=80&w=800&auto=format&fit=crop" },
      { shopId: "s3", shopName: "Highlands Coffee", locationSlot: "L1-12", x_Coordinate: 85, y_Coordinate: 20, storefrontImageUrl: "https://images.unsplash.com/photo-1554118811-1e0d58224f24?q=80&w=800&auto=format&fit=crop" }
    ]
  },
  {
    id: "f2",
    floorLevel: "L2",
    description: "Tầng 2 - Thể thao & Thời trang Nhanh",
    blueprintImageUrl: "https://www.transparenttextures.com/patterns/blueprint.png",
    locations: [
      { shopId: "s4", shopName: "Nike Mega Store", locationSlot: "L2-02", x_Coordinate: 40, y_Coordinate: 30, storefrontImageUrl: "https://images.unsplash.com/photo-1542291026-7eec264c27ff?q=80&w=800&auto=format&fit=crop" },
      { shopId: "s5", shopName: "Adidas Originals", locationSlot: "L2-08", x_Coordinate: 75, y_Coordinate: 65, storefrontImageUrl: "https://images.unsplash.com/photo-1518002171953-a080ee817e1f?q=80&w=800&auto=format&fit=crop" }
    ]
  },
  {
    id: "f4",
    floorLevel: "L4",
    description: "Tầng 4 - Khu Ẩm thực & Rạp chiếu phim",
    blueprintImageUrl: "https://www.transparenttextures.com/patterns/blueprint.png",
    locations: [
      { shopId: "s6", shopName: "Ocean Seafood Buffet", locationSlot: "L4-15", x_Coordinate: 30, y_Coordinate: 50, storefrontImageUrl: "https://images.unsplash.com/photo-1514933651103-005eec06c04b?q=80&w=800&auto=format&fit=crop" },
      { shopId: "s7", shopName: "CGV Cinemas", locationSlot: "L4-01", x_Coordinate: 80, y_Coordinate: 35, storefrontImageUrl: "https://images.unsplash.com/photo-1517604931442-7e0c8ed2963c?q=80&w=800&auto=format&fit=crop" }
    ]
  }
];

// ==========================================
// 2. COMPONENT CHÍNH
// ==========================================
export const InteractiveMap = () => {
  const [activeFloor, setActiveFloor] = useState<FloorPlan | null>(null);
  const [selectedPin, setSelectedPin] = useState<MapLocation | null>(null);
  const [isChangingFloor, setIsChangingFloor] = useState(false);

  // Khởi tạo dữ liệu (Dùng Mock Data thay vì gọi API)
  useEffect(() => {
    setActiveFloor(MOCK_FLOORS[0]); // Mặc định chọn L1
  }, []);

  // Hàm xử lý khi bấm đổi tầng
  const handleFloorChange = (floor: FloorPlan) => {
    if (activeFloor?.id === floor.id) return;
    
    setIsChangingFloor(true);
    setSelectedPin(null); // Tắt popup cũ
    
    // Giả lập độ trễ API 0.5s để thấy hiệu ứng loading
    setTimeout(() => {
      setActiveFloor(floor);
      setIsChangingFloor(false);
    }, 500);
  };

  if (!activeFloor) return null;

  return (
    <div className="max-w-7xl mx-auto px-4 py-12">
      
      {/* KHU VỰC 1: THANH ĐIỀU HƯỚNG TẦNG (Floor Selector) */}
      <div className="flex justify-center flex-wrap gap-4 mb-10">
        {MOCK_FLOORS.map((floor) => (
          <button
            key={floor.id}
            onClick={() => handleFloorChange(floor)}
            className={`px-8 py-3 rounded-2xl font-extrabold text-lg transition-all duration-300 ${
              activeFloor.id === floor.id
                ? "bg-mall-primary text-white shadow-[0_5px_20px_rgba(255,65,108,0.4)] scale-110"
                : "bg-white text-gray-500 border-2 border-gray-100 hover:border-mall-primary/40 hover:text-mall-primary"
            }`}
          >
            {floor.floorLevel}
          </button>
        ))}
      </div>

      {/* KHU VỰC 2: GIAO DIỆN BẢN ĐỒ & CHI TIẾT */}
      <div className="flex flex-col lg:flex-row gap-8">
        
        {/* CỘT TRÁI: BẢN ĐỒ 2D */}
        <div className="w-full lg:w-2/3 bg-white rounded-[2.5rem] p-4 lg:p-6 shadow-2xl border border-gray-100 relative">
          
          {/* Nhãn tên tầng góc trái */}
          <div className="absolute top-8 left-8 z-20 bg-white/90 backdrop-blur-md px-6 py-3 rounded-2xl shadow-lg border border-gray-100">
            <h2 className="text-xl font-extrabold text-mall-dark">{activeFloor.description}</h2>
          </div>

          {/* Khung chứa Map có hiệu ứng mờ khi chuyển tầng */}
          <div className={`relative w-full aspect-[4/3] bg-blue-50/50 rounded-[2rem] overflow-hidden border-2 border-dashed border-gray-200 transition-opacity duration-500 ${isChangingFloor ? 'opacity-30' : 'opacity-100'}`}>
            
            {/* Ảnh nền mặt bằng (Blueprint) */}
            <img 
              src={activeFloor.blueprintImageUrl} 
              alt="Mặt bằng tầng" 
              className="absolute inset-0 w-full h-full object-cover opacity-40 mix-blend-multiply"
            />
            
            {/* Render các đốm ghim (Pins) */}
            {!isChangingFloor && activeFloor.locations.map((loc) => {
              const isActive = selectedPin?.shopId === loc.shopId;
              return (
                <button
                  key={loc.shopId}
                  onClick={() => setSelectedPin(loc)}
                  style={{ top: `${loc.y_Coordinate}%`, left: `${loc.x_Coordinate}%` }}
                  className="absolute z-30 group -ml-4 -mt-4" // Căn giữa pin
                >
                  {/* Pin Icon */}
                  <div className={`w-8 h-8 rounded-full border-4 shadow-xl transition-all duration-300 flex items-center justify-center
                    ${isActive 
                      ? "bg-mall-accent border-white scale-125" 
                      : "bg-mall-primary border-white hover:scale-110 hover:bg-mall-secondary"}`}
                  >
                    {/* Vòng tròn nhấp nháy cho Pin đang chọn */}
                    {isActive && <span className="absolute inset-0 rounded-full bg-mall-accent animate-ping opacity-75"></span>}
                  </div>

                  {/* Tooltip hiện tên khi hover (ẩn khi đang active để nhường chỗ cho panel phải) */}
                  {!isActive && (
                    <span className="absolute bottom-full left-1/2 transform -translate-x-1/2 mb-2 px-3 py-1 bg-mall-dark text-white text-xs font-bold rounded-lg opacity-0 group-hover:opacity-100 transition-opacity whitespace-nowrap pointer-events-none shadow-lg">
                      {loc.locationSlot}: {loc.shopName}
                    </span>
                  )}
                </button>
              );
            })}
          </div>
        </div>

        {/* CỘT PHẢI: BẢNG THÔNG TIN CỬA HÀNG (PANEL) */}
        <div className="w-full lg:w-1/3">
          {selectedPin ? (
            // Card hiển thị thông tin
            <div className="bg-white rounded-[2.5rem] p-6 shadow-2xl border border-gray-100 h-full animate-fade-in-up">
              <div className="relative rounded-[2rem] overflow-hidden aspect-[4/3] mb-6 shadow-md group">
                <img 
                  src={selectedPin.storefrontImageUrl} 
                  alt={selectedPin.shopName} 
                  className="w-full h-full object-cover transition-transform duration-700 group-hover:scale-110" 
                />
                <div className="absolute top-4 right-4 bg-white/90 backdrop-blur-sm px-4 py-1.5 rounded-full text-sm font-extrabold text-mall-dark shadow-sm">
                  📍 Lô: {selectedPin.locationSlot}
                </div>
              </div>
              
              <div className="px-2 text-center">
                <h3 className="text-3xl font-black text-mall-dark mb-4">{selectedPin.shopName}</h3>
                <p className="text-gray-500 mb-8 leading-relaxed">
                  Trải nghiệm không gian mua sắm và dịch vụ tuyệt vời tại cửa hàng. Nằm tại vị trí đắc địa của {activeFloor.floorLevel}.
                </p>
                <button className="w-full py-4 bg-gradient-to-r from-mall-primary to-mall-secondary text-white rounded-2xl font-bold text-lg hover:shadow-lg hover:-translate-y-1 transition-all duration-300">
                  Xem Gian Hàng
                </button>
              </div>
            </div>
          ) : (
            // Trạng thái trống khi chưa chọn Pin
            <div className="bg-gray-50 rounded-[2.5rem] border-2 border-dashed border-gray-200 h-full min-h-[400px] flex flex-col items-center justify-center p-8 text-center transition-all">
              <div className="w-20 h-20 bg-gray-200 rounded-full flex items-center justify-center text-4xl mb-6 animate-bounce">
                🗺️
              </div>
              <h3 className="text-xl font-bold text-gray-400 mb-2">Chưa chọn khu vực</h3>
              <p className="text-gray-400">Hãy chạm vào một điểm ghim trên bản đồ để xem chi tiết cửa hàng nhé.</p>
            </div>
          )}
        </div>

      </div>
    </div>
  );
};