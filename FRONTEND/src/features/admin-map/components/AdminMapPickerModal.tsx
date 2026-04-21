import { useState, useEffect } from 'react';
import type { FloorPlan} from '../../directory/types/map.types';
import { mapApi } from '../../directory/api/mapApi';

interface Props {
  onClose: () => void;
  onSelectSlot: (locationId: number, locationSlot: string) => void;
}

export const AdminMapPickerModal = ({ onClose, onSelectSlot }: Props) => {
  const [floors, setFloors] = useState<FloorPlan[]>([]);
  const [activeFloor, setActiveFloor] = useState<FloorPlan | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const API_BASE = 'http://localhost:5184';

  useEffect(() => {
    const fetchAdminFloors = async () => {
      try {
        const data = await mapApi.getAdminFloors();
        setFloors(data);
        if (data.length > 0) setActiveFloor(data[0]);
      } catch (err) {
        setError("Lỗi tải bản đồ Admin.");
      } finally {
        setLoading(false);
      }
    };
    fetchAdminFloors();
  }, []);

  if (loading) return <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 text-white">Đang tải bản đồ...</div>;
  if (error) return <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 text-red-500">{error}</div>;
  if (!activeFloor) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/80 p-4 backdrop-blur-sm">
      <div className="flex w-full max-w-5xl flex-col overflow-hidden rounded-[2rem] bg-white shadow-2xl">
        
        <div className="flex items-center justify-between border-b px-6 py-4">
          <h2 className="text-xl font-black text-slate-800">Chọn Vị Trí Gian Hàng</h2>
          <button onClick={onClose} className="text-2xl font-bold text-gray-400 hover:text-red-500">&times;</button>
        </div>

        <div className="flex flex-col md:flex-row h-[70vh]">
          {/* CỘT TRÁI: DANH SÁCH TẦNG */}
          <div className="w-full md:w-1/4 border-r bg-gray-50 p-4 flex flex-col gap-2 overflow-y-auto">
            {floors.map(floor => (
              <button
                key={floor.id}
                onClick={() => setActiveFloor(floor)}
                className={`py-3 px-4 rounded-xl font-bold text-left transition-all ${
                  activeFloor.id === floor.id ? 'bg-amber-100 text-amber-700 border-2 border-amber-300' : 'bg-white border-2 border-transparent text-gray-600 hover:bg-gray-100'
                }`}
              >
                {floor.floorLevel}
              </button>
            ))}
            
            <div className="mt-auto pt-6 border-t border-gray-200">
               <h3 className="text-xs font-bold text-gray-400 uppercase mb-3">Chú giải:</h3>
               <div className="flex items-center gap-2 text-sm text-gray-600 mb-2">
                 <div className="w-4 h-4 rounded-full bg-slate-300 border-2 border-white shadow-sm"></div> Trống (Available)
               </div>
               <div className="flex items-center gap-2 text-sm text-gray-600">
                 <div className="w-4 h-4 rounded-full bg-red-500 border-2 border-white shadow-sm"></div> Đã thuê (Occupied)
               </div>
            </div>
          </div>

          {/* CỘT PHẢI: BẢN ĐỒ */}
          <div className="relative flex-1 bg-slate-100 overflow-hidden flex items-center justify-center p-6">
             <div className="relative w-full max-w-lg aspect-[3/4] bg-white rounded-2xl shadow-inner border">
                <img
                  src={`${API_BASE}${activeFloor.blueprintImageUrl}`}
                  alt={`Sơ đồ ${activeFloor.floorLevel}`}
                  className="absolute inset-0 w-full h-full object-contain"
                />

                {activeFloor.locations.map((loc) => {
                  const isAvailable = loc.status === "Available";
                  
                  return (
                    <button
                      key={loc.id}
                      onClick={() => {
                        if (isAvailable) {
                           onSelectSlot(loc.id, `${activeFloor.floorLevel} - Lô ${loc.locationSlot}`);
                        } else {
                           alert(`Lô ${loc.locationSlot} đã có người thuê (${loc.shopName || 'Reserved'}).`);
                        }
                      }}
                      style={{ top: `${loc.y}%`, left: `${loc.x}%` }}
                      className="absolute z-30 -translate-x-1/2 -translate-y-1/2 group"
                    >
                      <div
                        className={`rounded-full border-2 border-white shadow-md flex items-center justify-center transition-transform duration-200 w-6 h-6
                        ${isAvailable ? 'bg-slate-300 hover:bg-amber-400 hover:scale-150 animate-pulse' : 'bg-red-500 opacity-50 cursor-not-allowed'}`}
                      />
                      {/* Tooltip */}
                      <span className="absolute bottom-full left-1/2 -translate-x-1/2 mb-2 px-2 py-1 bg-gray-800 text-white text-xs font-bold rounded opacity-0 group-hover:opacity-100 pointer-events-none whitespace-nowrap">
                        Lô {loc.locationSlot} {isAvailable ? '(Trống)' : '(Đã thuê)'}
                      </span>
                    </button>
                  );
                })}
             </div>
          </div>
        </div>
      </div>
    </div>
  );
};