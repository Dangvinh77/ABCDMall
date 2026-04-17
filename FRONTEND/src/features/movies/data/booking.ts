export type SeatType = 'regular' | 'vip' | 'couple';
export type PaymentMethod = 'momo' | 'vnpay' | 'visa' | 'atm';
export type SnackComboId = string;
export type HallType = '2D' | '3D' | 'IMAX' | '4DX';

export const PRICES: Record<SeatType, number> = {
  regular: 85_000,
  vip: 120_000,
  couple: 95_000,
};

export const HALL_BASE_PRICES: Record<HallType, number> = {
  '2D': 85_000,
  '3D': 120_000,
  IMAX: 140_000,
  '4DX': 160_000,
};

export const SEAT_SURCHARGES: Record<SeatType, number> = {
  regular: 0,
  vip: 35_000,
  couple: 10_000,
};

export const SERVICE_FEE = 10_000; // per ticket

export const HALL_NAMES: Record<string, string> = {
  '2D': '2D Hall - Room 01',
  '3D': '3D Hall - Room 02',
  IMAX: 'IMAX Premium Hall',
  '4DX': '4DX Experience Hall',
};

export const SEAT_LABELS: Record<SeatType, string> = {
  regular: 'Regular Seat',
  vip: 'VIP Seat',
  couple: 'Couple Seat',
};

export interface SnackCombo {
  id: SnackComboId;
  name: string;
  description: string;
  price: number;
  originalPrice?: number;
  promoPrice?: number;
}

export const SNACK_COMBOS: SnackCombo[] = [
  {
    id: 'combo-solo',
    name: 'Solo Popcorn Combo',
    description: '1 large popcorn and 1 soft drink.',
    price: 89_000,
  },
  {
    id: 'combo-double',
    name: 'Double Movie Combo',
    description: '2 large popcorns and 2 soft drinks for sharing.',
    price: 159_000,
  },
  {
    id: 'combo-gold',
    name: 'Combo Gold',
    description: '1 caramel popcorn, 2 drinks and 1 snack tray.',
    price: 142_000,
    promoPrice: 85_000,
  },
];

export interface SelectedSeat {
  id: string;
  type: SeatType;
  seatInventoryId?: string;
}

export interface SelectedSnackCombo {
  id: string;
  quantity: number;
}

export interface BookingState {
  holdId?: string;
  holdCode?: string;
  holdExpiresAtUtc?: string;
  holdRemainingSeconds?: number;
  seats: SelectedSeat[];
  subtotal: number;
  serviceFee: number;
  total: number;
  comboSubtotal?: number;
  discountAmount?: number;
  combos?: SelectedSnackCombo[];
  promoId?: string | null;
  bookingDate?: string;
}

export function getSnackComboById(comboId: string) {
  return SNACK_COMBOS.find((combo) => combo.id === comboId) ?? null;
}

export function isHallType(value: string): value is HallType {
  return value === '2D' || value === '3D' || value === 'IMAX' || value === '4DX';
}

export function normalizeHallType(value: string): HallType {
  return isHallType(value) ? value : '2D';
}

export function getShowtimeStartingPrice(hallType: string) {
  return HALL_BASE_PRICES[normalizeHallType(hallType)];
}

export function getSeatPrice(hallType: string, seatType: SeatType) {
  return getShowtimeStartingPrice(hallType) + SEAT_SURCHARGES[seatType];
}

export function vnd(n: number) {
  return `${n.toLocaleString('en-US')} VND`;
}

export function generateBookingCode() {
  const chars = 'ABCDEFGHJKLMNPQRSTUVWXYZ23456789';
  const seg = (len: number) =>
    Array.from({ length: len }, () => chars[Math.floor(Math.random() * chars.length)]).join('');
  return `ABCD-${seg(4)}-${seg(4)}`;
}
