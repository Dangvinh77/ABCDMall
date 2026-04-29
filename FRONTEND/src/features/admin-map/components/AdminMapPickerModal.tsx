import { useEffect, useState } from "react";
import type { FloorPlan } from "../../directory/types/map.types";
import { mapApi } from "../../directory/api/mapApi";

interface Props {
  onClose: () => void;
  onSelectSlot: (locationId: number, locationSlot: string, floorLevel: string) => void;
}

export const AdminMapPickerModal = ({ onClose, onSelectSlot }: Props) => {
  const [floors, setFloors] = useState<FloorPlan[]>([]);
  const [activeFloor, setActiveFloor] = useState<FloorPlan | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const API_BASE = "http://localhost:5184";

  useEffect(() => {
    const fetchAdminFloors = async () => {
      try {
        const data = await mapApi.getAdminFloors();
        setFloors(data);
        if (data.length > 0) {
          setActiveFloor(data[0]);
        }
      } catch {
        setError("Unable to load admin mall map.");
      } finally {
        setLoading(false);
      }
    };

    fetchAdminFloors();
  }, []);

  if (loading) {
    return <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 text-white">Loading mall map...</div>;
  }

  if (error) {
    return <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 text-red-300">{error}</div>;
  }

  if (!activeFloor) {
    return null;
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/80 p-4 backdrop-blur-sm">
      <div className="flex h-[70vh] w-full max-w-5xl flex-col overflow-hidden rounded-[2rem] bg-white shadow-2xl md:h-[75vh]">
        <div className="flex items-center justify-between border-b px-6 py-4">
          <h2 className="text-xl font-black text-slate-800">Select Rental Slot</h2>
          <button onClick={onClose} className="text-2xl font-bold text-gray-400 hover:text-red-500">
            &times;
          </button>
        </div>

        <div className="flex h-full flex-col md:flex-row">
          <div className="flex w-full flex-col gap-2 overflow-y-auto border-r bg-gray-50 p-4 md:w-1/4">
            {floors.map((floor) => (
              <button
                key={floor.id}
                onClick={() => setActiveFloor(floor)}
                className={`rounded-xl px-4 py-3 text-left font-bold transition-all ${
                  activeFloor.id === floor.id
                    ? "border-2 border-amber-300 bg-amber-100 text-amber-700"
                    : "border-2 border-transparent bg-white text-gray-600 hover:bg-gray-100"
                }`}
              >
                {floor.floorLevel}
              </button>
            ))}

            <div className="mt-auto border-t border-gray-200 pt-6">
              <h3 className="mb-3 text-xs font-bold uppercase text-gray-400">Legend</h3>
              <div className="mb-2 flex items-center gap-2 text-sm text-gray-600">
                <div className="h-4 w-4 rounded-full border-2 border-white bg-slate-300 shadow-sm" />
                Available
              </div>
              <div className="flex items-center gap-2 text-sm text-gray-600">
                <div className="h-4 w-4 rounded-full border-2 border-white bg-red-500 shadow-sm" />
                Occupied
              </div>
            </div>
          </div>

          <div className="relative flex flex-1 items-center justify-center overflow-hidden bg-slate-100 p-6">
            <div className="relative aspect-[3/4] w-full max-w-lg rounded-2xl border bg-white shadow-inner">
              <img
                src={`${API_BASE}${activeFloor.blueprintImageUrl}`}
                alt={`Blueprint ${activeFloor.floorLevel}`}
                className="absolute inset-0 h-full w-full object-contain"
              />

              {activeFloor.locations.map((loc) => {
                const isAvailable = loc.status === "Available";
                const occupiedLabel = loc.shopName?.trim()
                  ? loc.shopName.trim()
                  : loc.shopInfoId
                    ? "Reserved"
                    : "Occupied";

                return (
                  <button
                    key={loc.id}
                    onClick={() => {
                      if (isAvailable) {
                        onSelectSlot(loc.id, loc.locationSlot, activeFloor.floorLevel);
                      } else {
                        alert(`Slot ${loc.locationSlot} is already occupied (${occupiedLabel}).`);
                      }
                    }}
                    style={{ top: `${loc.y}%`, left: `${loc.x}%` }}
                    className="group absolute z-30 -translate-x-1/2 -translate-y-1/2"
                  >
                    <div
                      className={`flex h-6 w-6 items-center justify-center rounded-full border-2 border-white shadow-md transition-transform duration-200 ${
                        isAvailable
                          ? "animate-pulse bg-slate-300 hover:scale-150 hover:bg-amber-400"
                          : "cursor-not-allowed bg-red-500 opacity-50"
                      }`}
                    />
                    <span className="pointer-events-none absolute bottom-full left-1/2 mb-2 -translate-x-1/2 whitespace-nowrap rounded bg-gray-800 px-2 py-1 text-xs font-bold text-white opacity-0 group-hover:opacity-100">
                      Slot {loc.locationSlot} {isAvailable ? "(Available)" : `(${occupiedLabel})`}
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
