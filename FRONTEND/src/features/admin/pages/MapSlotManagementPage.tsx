import { useState, useEffect } from 'react';
import { Loader2, AlertCircle } from 'lucide-react';
import {
  FloorPlanAdmin,
  MapLocationAdmin,
  getAdminFloorPlan,
  assignSlot,
  releaseSlot,
} from '../services/adminApi';

export function MapSlotManagementPage() {
  const [selectedFloor, setSelectedFloor] = useState<FloorPlanAdmin | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [selectedSlot, setSelectedSlot] = useState<MapLocationAdmin | null>(null);
  const [showAssignModal, setShowAssignModal] = useState(false);
  const [assignShopId, setAssignShopId] = useState('');
  const [actionLoading, setActionLoading] = useState(false);
  const token = localStorage.getItem('accessToken') || '';

  const floors = ['1', '2', '3', '4', '5'];

  useEffect(() => {
    if (selectedFloor) {
      loadFloorPlan(selectedFloor.floorLevel);
    }
  }, [selectedFloor]);

  const loadFloorPlan = async (floorLevel: string) => {
    try {
      setLoading(true);
      setError(null);
      const data = await getAdminFloorPlan(token, floorLevel);
      setSelectedFloor(data);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load floor plan');
    } finally {
      setLoading(false);
    }
  };

  const handleAssignSlot = async () => {
    if (!selectedSlot || !assignShopId.trim()) return;

    try {
      setActionLoading(true);
      setError(null);
      await assignSlot(token, selectedSlot.id, assignShopId);
      
      // Reload floor plan
      if (selectedFloor) {
        await loadFloorPlan(selectedFloor.floorLevel);
      }
      
      setShowAssignModal(false);
      setAssignShopId('');
      setSelectedSlot(null);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to assign slot');
    } finally {
      setActionLoading(false);
    }
  };

  const handleReleaseSlot = async (slot: MapLocationAdmin) => {
    if (!window.confirm(`Release slot ${slot.locationSlot}?`)) return;

    try {
      setActionLoading(true);
      setError(null);
      await releaseSlot(token, slot.id);
      
      // Reload floor plan
      if (selectedFloor) {
        await loadFloorPlan(selectedFloor.floorLevel);
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to release slot');
    } finally {
      setActionLoading(false);
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Available':
        return 'bg-gray-500/20 text-gray-300 border-gray-500/30';
      case 'Reserved':
        return 'bg-yellow-500/20 text-yellow-300 border-yellow-500/30';
      case 'ComingSoon':
        return 'bg-blue-500/20 text-blue-300 border-blue-500/30';
      case 'Active':
        return 'bg-green-500/20 text-green-300 border-green-500/30';
      default:
        return 'bg-gray-500/20 text-gray-300 border-gray-500/30';
    }
  };

  const getStatusBgColor = (status: string) => {
    switch (status) {
      case 'Available':
        return 'bg-gray-600/50 hover:bg-gray-600/60';
      case 'Reserved':
        return 'bg-yellow-600/50 hover:bg-yellow-600/60';
      case 'ComingSoon':
        return 'bg-blue-600/50 hover:bg-blue-600/60';
      case 'Active':
        return 'bg-green-600/50 hover:bg-green-600/60';
      default:
        return 'bg-gray-600/50 hover:bg-gray-600/60';
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="rounded-[2rem] border border-white/8 bg-gradient-to-br from-cyan-500/20 to-blue-500/10 p-6">
        <div className="flex items-start justify-between gap-4">
          <div>
            <p className="text-xs font-semibold uppercase tracking-widest text-cyan-200/70">
              Map Management
            </p>
            <h1 className="mt-2 text-3xl font-bold text-white">Slot Management</h1>
            <p className="mt-2 text-sm text-gray-400">
              Assign and manage shop slots across the mall floors.
            </p>
          </div>
        </div>
      </div>

      {/* Error Alert */}
      {error && (
        <div className="rounded-lg border border-red-500/30 bg-red-500/10 p-4 flex items-start gap-3">
          <AlertCircle className="size-5 text-red-400 flex-shrink-0 mt-0.5" />
          <div>
            <p className="font-semibold text-red-200">Error</p>
            <p className="text-sm text-red-300">{error}</p>
          </div>
        </div>
      )}

      <div className="grid gap-6 lg:grid-cols-[300px_1fr]">
        {/* Floor Selector */}
        <div className="space-y-3">
          <h3 className="text-sm font-semibold uppercase tracking-wider text-gray-300">
            Select Floor
          </h3>
          <div className="space-y-2">
            {floors.map((floor) => (
              <button
                key={floor}
                onClick={() => setSelectedFloor({ floorLevel: floor } as FloorPlanAdmin)}
                className={`w-full rounded-lg px-4 py-3 text-left font-semibold transition-colors ${
                  selectedFloor?.floorLevel === floor
                    ? 'bg-cyan-500/20 text-cyan-300 border border-cyan-500/30'
                    : 'bg-slate-900/50 text-gray-300 border border-white/10 hover:bg-slate-900'
                }`}
              >
                Floor {floor}
              </button>
            ))}
          </div>
        </div>

        {/* Floor Plan Viewer */}
        <div className="space-y-4">
          {loading ? (
            <div className="rounded-[2rem] border border-white/8 bg-slate-950/65 h-96 flex items-center justify-center">
              <Loader2 className="size-8 animate-spin text-cyan-400" />
            </div>
          ) : selectedFloor ? (
            <>
              <div className="rounded-[2rem] border border-white/8 bg-slate-950/65 p-6">
                <h2 className="mb-4 text-xl font-bold text-white">Floor {selectedFloor.floorLevel}</h2>
                {selectedFloor.blueprintImageUrl && (
                  <img
                    src={selectedFloor.blueprintImageUrl}
                    alt={`Floor ${selectedFloor.floorLevel}`}
                    className="mb-6 max-h-64 w-full rounded-lg object-cover"
                  />
                )}
                <p className="text-sm text-gray-400">{selectedFloor.description}</p>
              </div>

              {/* Slots Grid */}
              <div className="rounded-[2rem] border border-white/8 bg-slate-950/65 p-6">
                <h3 className="mb-4 text-lg font-semibold text-white">Available Slots</h3>
                <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
                  {selectedFloor.locations.map((slot) => (
                    <div
                      key={slot.id}
                      className={`rounded-lg border p-4 transition-all cursor-pointer ${
                        selectedSlot?.id === slot.id
                          ? 'border-cyan-500/50 bg-cyan-500/10'
                          : 'border-white/10 hover:border-white/20'
                      } ${getStatusBgColor(slot.status)}`}
                      onClick={() => setSelectedSlot(selectedSlot?.id === slot.id ? null : slot)}
                    >
                      <div className="flex items-start justify-between gap-2 mb-3">
                        <div className="flex-1 min-w-0">
                          <p className="font-semibold text-white truncate">
                            {slot.locationSlot}
                          </p>
                          <p className="text-xs text-gray-400 truncate">{slot.shopName || 'Empty'}</p>
                        </div>
                        <span
                          className={`shrink-0 inline-block rounded-full px-2 py-1 text-xs font-semibold border ${getStatusColor(
                            slot.status
                          )}`}
                        >
                          {slot.status}
                        </span>
                      </div>

                      {selectedSlot?.id === slot.id && (
                        <div className="space-y-3 border-t border-white/10 pt-3">
                          {slot.status === 'Available' ? (
                            <button
                              onClick={(e) => {
                                e.stopPropagation();
                                setShowAssignModal(true);
                              }}
                              className="w-full rounded-lg bg-green-600 px-3 py-2 text-xs font-semibold text-white hover:bg-green-700 transition-colors"
                            >
                              Assign Slot
                            </button>
                          ) : slot.status === 'Reserved' || slot.status === 'Active' ? (
                            <button
                              onClick={(e) => {
                                e.stopPropagation();
                                handleReleaseSlot(slot);
                              }}
                              disabled={actionLoading}
                              className="w-full rounded-lg bg-red-600 px-3 py-2 text-xs font-semibold text-white hover:bg-red-700 disabled:opacity-50 transition-colors"
                            >
                              {actionLoading ? 'Releasing...' : 'Release Slot'}
                            </button>
                          ) : null}
                          {slot.shopInfoId && (
                            <p className="text-xs text-gray-400">
                              Shop ID: <span className="text-gray-300 font-mono">{slot.shopInfoId}</span>
                            </p>
                          )}
                        </div>
                      )}
                    </div>
                  ))}
                </div>
              </div>
            </>
          ) : (
            <div className="rounded-[2rem] border border-white/8 bg-slate-950/65 h-96 flex items-center justify-center">
              <p className="text-gray-400">Select a floor to view slots</p>
            </div>
          )}
        </div>
      </div>

      {/* Assign Modal */}
      {showAssignModal && selectedSlot && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
          <div className="rounded-[2rem] border border-white/8 bg-slate-950 p-6 max-w-md w-full mx-4 space-y-4">
            <h3 className="text-xl font-bold text-white">Assign Slot to Manager</h3>
            <p className="text-sm text-gray-400">
              Slot: <span className="font-semibold text-white">{selectedSlot.locationSlot}</span>
            </p>

            <div>
              <label className="block text-sm font-semibold text-gray-300 mb-2">
                Manager Shop ID (ShopInfo ID)
              </label>
              <input
                type="text"
                value={assignShopId}
                onChange={(e) => setAssignShopId(e.target.value)}
                placeholder="Paste manager's ShopInfo ID"
                className="w-full rounded-lg border border-white/10 bg-slate-900 px-4 py-2 text-white placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-green-500"
              />
              <p className="mt-2 text-xs text-gray-500">
                You'll get this ID after creating the manager account.
              </p>
            </div>

            <div className="flex gap-3 pt-4">
              <button
                onClick={handleAssignSlot}
                disabled={actionLoading || !assignShopId.trim()}
                className="flex-1 flex items-center justify-center gap-2 rounded-lg bg-green-600 px-4 py-2 font-semibold text-white hover:bg-green-700 disabled:opacity-50 transition-colors"
              >
                {actionLoading && <Loader2 className="size-4 animate-spin" />}
                Assign
              </button>
              <button
                onClick={() => {
                  setShowAssignModal(false);
                  setAssignShopId('');
                }}
                disabled={actionLoading}
                className="flex-1 rounded-lg border border-white/10 px-4 py-2 font-semibold text-gray-300 hover:bg-white/5 disabled:opacity-50 transition-colors"
              >
                Cancel
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
