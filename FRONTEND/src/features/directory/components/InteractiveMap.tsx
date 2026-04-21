import { useState } from 'react';
import type { MapLocation } from '../types/map.types';
import { useMap } from '../hooks/useMap';

export const InteractiveMap = () => {
  const { floors, activeFloor, switchFloor, loading, error } = useMap();
  const [selectedPin, setSelectedPin] = useState<MapLocation | null>(null);
  const [imgLoaded, setImgLoaded] = useState(false);

  // State lưu toạ độ vừa click (Dành cho Dev Mode)
  const [devCoords, setDevCoords] = useState<{ x: number; y: number } | null>(null);

  const API_BASE = 'http://localhost:5184';
  const getFloorLabel = (label: string) => label.replace("Tầng", "Floor");
  const displayDescription = activeFloor ? activeFloor.description.replace("Tầng", "Floor") : "";

  // Hàm tính toán toạ độ khi click
  const handleMapClick = (e: React.MouseEvent<HTMLDivElement>) => {
    const rect = e.currentTarget.getBoundingClientRect();
    const x = ((e.clientX - rect.left) / rect.width) * 100;
    const y = ((e.clientY - rect.top) / rect.height) * 100;

    const roundedX = Math.round(x * 100) / 100;
    const roundedY = Math.round(y * 100) / 100;

    setDevCoords({ x: roundedX, y: roundedY });
    console.log(`Toạ độ mới: X = ${roundedX}, Y = ${roundedY}`);
  };

  if (loading) return (
    <div className="flex items-center justify-center min-h-[400px]">
      <div className="text-center text-gray-400">
        <div className="text-4xl mb-4 animate-bounce">🗺️</div>
        <p className="font-semibold">Loading map...</p>
      </div>
    </div>
  );

  if (error) return (
    <div className="flex items-center justify-center min-h-[400px] text-red-400">
      <p>{error}</p>
    </div>
  );

  if (!activeFloor) return null;

  const handleFloorSwitch = (floor: typeof activeFloor) => {
    if (floor.id === activeFloor.id) return;
    setSelectedPin(null);
    setImgLoaded(false);
    setDevCoords(null);
    switchFloor(floor);
  };

  return (
    // SỬA TẠI ĐÂY: Xóa max-w-7xl, đổi thành w-full và cho lề px rộng ra để bung full màn hình
    <div className="w-full px-4 lg:px-8 xl:px-12 py-12 mx-auto">
      {/* THANH CHỌN TẦNG */}
      <div className="flex justify-center flex-wrap gap-3 mb-10">
            {floors.map((floor) => (
          <button
            key={floor.id}
            onClick={() => handleFloorSwitch(floor)}
            className={`px-7 py-2.5 rounded-2xl font-bold text-base transition-all duration-300 ${activeFloor.id === floor.id
              ? 'bg-mall-primary text-white shadow-[0_4px_18px_rgba(255,65,108,0.4)] scale-105'
              : 'bg-white text-gray-500 border-2 border-gray-100 hover:border-mall-primary/40 hover:text-mall-primary'
              }`}
          >
            {getFloorLabel(floor.floorLevel)}
          </button>
        ))}
      </div>

      {/* NỘI DUNG CHÍNH */}
      <div className="flex flex-col lg:flex-row gap-8">

        {/* CỘT TRÁI: BẢN ĐỒ - TĂNG LÊN CHIẾM 3/4 (75%) */}
        <div className="w-full lg:w-3/4 bg-white rounded-[2rem] p-4 shadow-2xl border border-gray-100">
          <div className="mb-3 px-2 flex justify-between items-center">
            <div>
              <h2 className="text-lg font-extrabold text-mall-dark">{displayDescription}</h2>
              <p className="text-sm text-gray-400">{activeFloor.locations.length} stores</p>
            </div>

            {/* Box hiển thị tọa độ góc phải */}
            {devCoords && (
              <div className="bg-gray-800 text-green-400 px-4 py-2 rounded-xl font-mono text-sm shadow-inner">
                LAST CLICK: X: {devCoords.x}, Y: {devCoords.y}
              </div>
            )}
          </div>

          {/* Khung map — Tỷ lệ 3:4 */}
          <div
            className="relative w-full rounded-[1.5rem] overflow-hidden border border-gray-100 cursor-crosshair bg-slate-50"
            style={{ aspectRatio: '3 / 4' }}
            onClick={handleMapClick}
          >
            <img
              key={activeFloor.id}
              src={`${API_BASE}${activeFloor.blueprintImageUrl}`}
              alt={`Map of ${getFloorLabel(activeFloor.floorLevel)}`}
              className={`absolute inset-0 w-full h-full object-contain transition-opacity duration-500 ${imgLoaded ? 'opacity-100' : 'opacity-0'}`}
              onLoad={() => setImgLoaded(true)}
            />

            {/* Render Pins */}
            {activeFloor.locations.map((loc) => {
              const isActive = selectedPin?.id === loc.id;
              const isCinema = loc.shopUrl === '/movies';

              return (
                <button
                  key={loc.id}
                  onClick={(e) => {
                    e.stopPropagation();
                    setSelectedPin(loc);
                  }}
                  style={{ top: `${loc.y}%`, left: `${loc.x}%` }}
                  className="absolute z-30 -translate-x-1/2 -translate-y-1/2 group"
                >
                  <div className={`rounded-full border-2 shadow-lg flex items-center justify-center transition-all duration-300
                    ${isCinema ? 'w-10 h-10 text-base bg-gray-800 text-white' : 'w-6 h-6'}
                    ${isActive
                      ? 'bg-yellow-400 border-white scale-150'
                      : isCinema
                        ? 'border-white hover:scale-110'
                        : 'bg-red-500 border-white hover:scale-125'
                    }`}
                  >
                    {isCinema && '🎬'}
                  </div>

                  {/* Tooltip khi hover (Tùy chọn) */}
                  {!isActive && (
                    <span className="absolute bottom-full left-1/2 -translate-x-1/2 mb-2 px-3 py-1 bg-gray-800 text-white text-xs font-bold rounded-lg opacity-0 group-hover:opacity-100 transition-opacity whitespace-nowrap pointer-events-none shadow-lg">
                      {loc.shopName}
                    </span>
                  )}
                </button>
              );
            })}
          </div>
        </div>

        {/* CỘT PHẢI: THÔNG TIN - THU LẠI 1/4 (25%) & THÊM STICKY */}
        <div className="w-full lg:w-1/4 sticky top-8 self-start transition-all duration-300">
          {selectedPin ? (
            <div className="bg-white rounded-[2rem] p-6 shadow-2xl border border-gray-100 animate-fade-in-up">

              {/* Ảnh cover của Shop (nếu có) */}
              {selectedPin.storefrontImageUrl ? (
                <div className="rounded-[1.5rem] overflow-hidden aspect-[4/3] mb-5 shadow-md">
                  {/* <img
                      src={selectedPin.storefrontImageUrl}
                      alt={selectedPin.shopName}
                      className="w-full h-full object-cover"
                    /> */}

                  <img
                    src={`${API_BASE}${selectedPin.storefrontImageUrl}`}
                    alt={selectedPin.shopName}
                    className="w-full h-full object-contain"
                  />
                </div>
              ) : (
                <div className="rounded-[1.5rem] aspect-[4/3] mb-5 bg-gradient-to-br from-red-50 to-orange-50 flex items-center justify-center border border-gray-100">
                  <span className="text-5xl">{selectedPin.shopUrl === '/movies' ? '🍿' : '🏪'}</span>
                </div>
              )}

              <div className="text-center px-2">
                <span className="inline-block bg-red-50 text-red-500 text-xs font-extrabold px-3 py-1 rounded-full mb-3">
                  📍 Unit {selectedPin.locationSlot}
                </span>

                <h3 className="text-2xl font-black text-gray-800 mb-2">{selectedPin.shopName}</h3>
                <p className="text-gray-400 text-sm mb-5">
                  {getFloorLabel(activeFloor.floorLevel)}
                </p>

                {/* Hiện toạ độ Dev (Bạn có thể xóa phần này khi deploy thật) */}
                <div className="mb-6 p-3 bg-gray-50 rounded-xl font-mono text-xs text-gray-500 border border-gray-100 text-left">
                  <strong>Pin Coordinates:</strong><br />
                  X: {selectedPin.x}<br />
                  Y: {selectedPin.y}
                </div>

                {/* Nút Xem Gian Hàng */}
                <a
                  href={selectedPin.shopUrl}
                  className="block w-full py-3.5 bg-gradient-to-r from-red-500 to-orange-500 text-white rounded-2xl font-bold text-sm hover:shadow-lg hover:-translate-y-0.5 transition-all duration-300 text-center"
                >
                  View Store &rarr;
                </a>
              </div>
            </div>
          ) : (
            <div className="bg-gray-50 rounded-[2rem] border-2 border-dashed border-gray-200 min-h-[300px] flex flex-col items-center justify-center p-8 text-center">
              <div className="w-16 h-16 bg-white rounded-full flex items-center justify-center text-3xl mb-5 shadow-sm animate-bounce">
                📌
              </div>
              <h3 className="text-lg font-bold text-gray-500 mb-2">No store selected</h3>
              <p className="text-gray-400 text-sm">Click a pin on the map to view its details.</p>
            </div>
          )}
        </div>

      </div>
    </div>
  );
};
