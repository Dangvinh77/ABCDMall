import {
  PRICES,
  getSnackComboById,
  type PaymentMethod,
  type SeatType,
  type SelectedSnackCombo,
} from './booking';

export type PromotionId = 'f1' | 'f2' | 'f3' | 'p4' | 'p5' | 'p6' | 'p7' | 'p8';
export type PromotionStatus = 'pending' | 'eligible' | 'applied' | 'ineligible';

export interface PromotionDefinition {
  id: PromotionId;
  title: string;
  description: string;
}

export interface PromotionContext {
  promoId?: string | null;
  seats: Array<{ id: string; type: SeatType }>;
  subtotal: number;
  serviceFee: number;
  hallType: string;
  showtime: string;
  bookingDate: string;
  paymentMethod?: PaymentMethod;
  birthday?: string;
  snackCombos?: SelectedSnackCombo[];
}

export interface PromotionEvaluation {
  promo: PromotionDefinition;
  status: PromotionStatus;
  discount: number;
  estimatedDiscount: number;
  message: string;
  bonusLabel?: string;
}

const PROMOTIONS: Record<PromotionId, PromotionDefinition> = {
  f1: {
    id: 'f1',
    title: 'Weekend Tickets - Free Popcorn Combo',
    description: 'Available on Saturday and Sunday for orders with at least 2 tickets.',
  },
  f2: {
    id: 'f2',
    title: 'Pay with MoMo - Get 30% Off',
    description: 'Save 30% on ticket value, up to 60,000 VND, when paying with MoMo.',
  },
  f3: {
    id: 'f3',
    title: 'Date Night - Buy 1 Get 1',
    description: 'Choose a couple seat and receive 1 regular ticket for the same showtime.',
  },
  p4: {
    id: 'p4',
    title: 'Vietcombank - 25% Off',
    description: 'Enjoy 25% off on weekday bookings, up to 50,000 VND.',
  },
  p5: {
    id: 'p5',
    title: 'Group Booking - 20% Off 5 Tickets',
    description: 'Get 20% off when booking 5 tickets or more in one order.',
  },
  p6: {
    id: 'p6',
    title: 'Combo Gold - Special Snack Price',
    description: 'Add Combo Gold to unlock the special 85,000 VND promotional price.',
  },
  p7: {
    id: 'p7',
    title: 'Birthday Special - 1 Free Ticket',
    description: 'Enter your birthday at checkout to claim a free regular ticket during your birthday month.',
  },
  p8: {
    id: 'p8',
    title: 'Early Bird - 35% Off',
    description: 'Valid for showtimes starting from 09:00 to before 11:00.',
  },
};

const COMBO_GOLD_BASE_PRICE = 142_000;
export const COMBO_GOLD_PROMO_PRICE = 85_000;

function toDate(value: string) {
  return new Date(`${value}T00:00:00`);
}

function padDatePart(value: number) {
  return String(value).padStart(2, '0');
}

function isWeekend(bookingDate: string) {
  const day = toDate(bookingDate).getDay();
  return day === 0 || day === 6;
}

function isWeekday(bookingDate: string) {
  return !isWeekend(bookingDate);
}

function isMorningShowtime(showtime: string) {
  const [hour] = showtime.split(':').map(Number);
  return Number.isFinite(hour) && hour >= 9 && hour < 11;
}

function hasCoupleSeat(seats: Array<{ type: SeatType }>) {
  return seats.some((seat) => seat.type === 'couple');
}

function discountPercent(subtotal: number, percent: number, maxDiscount?: number) {
  const raw = Math.round(subtotal * percent);
  return typeof maxDiscount === 'number' ? Math.min(raw, maxDiscount) : raw;
}

function birthdayMatchesMonth(birthday?: string, bookingDate?: string) {
  if (!birthday || !bookingDate) return false;
  return toDate(birthday).getMonth() === toDate(bookingDate).getMonth();
}

function getSnackComboQuantity(snackCombos: SelectedSnackCombo[] | undefined, comboId: string) {
  return snackCombos?.find((combo) => combo.id === comboId)?.quantity ?? 0;
}

export function getPromotionById(promoId?: string | null) {
  if (!promoId) return null;
  return PROMOTIONS[promoId as PromotionId] ?? null;
}

export function getDefaultBookingDate() {
  const now = new Date();
  return `${now.getFullYear()}-${padDatePart(now.getMonth() + 1)}-${padDatePart(now.getDate())}`;
}

export function evaluatePromotion(context: PromotionContext): PromotionEvaluation | null {
  const promo = getPromotionById(context.promoId);

  if (!promo) return null;

  const seatCount = context.seats.length;
  const result: PromotionEvaluation = {
    promo,
    status: 'pending',
    discount: 0,
    estimatedDiscount: 0,
    message: 'Choose an eligible movie and showtime to activate this offer.',
  };

  switch (promo.id) {
    case 'f1':
      if (!isWeekend(context.bookingDate)) {
        result.status = 'ineligible';
        result.message = 'This offer is only valid for Saturday and Sunday showtimes.';
        return result;
      }
      if (seatCount < 2) {
        result.status = 'pending';
        result.message = 'Select at least 2 tickets to receive the free popcorn combo.';
        return result;
      }
      result.status = 'applied';
      result.message = 'You will receive 1 free large popcorn combo.';
      result.bonusLabel = 'Free large popcorn combo';
      return result;

    case 'f2':
      result.estimatedDiscount = discountPercent(context.subtotal, 0.3, 60_000);
      if (seatCount === 0) {
        result.message = 'Choose your seats first to estimate the MoMo discount.';
        return result;
      }
      if (context.paymentMethod !== 'momo') {
        result.status = 'pending';
        result.message = 'Pay with MoMo at checkout to receive 30% off.';
        return result;
      }
      result.status = 'applied';
      result.discount = result.estimatedDiscount;
      result.message = '30% MoMo discount has been applied.';
      return result;

    case 'f3':
      result.estimatedDiscount = PRICES.regular;
      if (context.hallType === 'IMAX') {
        result.status = 'ineligible';
        result.message = 'This offer is not available for IMAX halls.';
        return result;
      }
      if (!hasCoupleSeat(context.seats)) {
        result.status = 'pending';
        result.message = 'Choose at least 1 couple seat to unlock the free regular ticket.';
        return result;
      }
      result.status = 'applied';
      result.discount = PRICES.regular;
      result.message = 'A free regular ticket has been added to your Date Night booking.';
      return result;

    case 'p4':
      result.estimatedDiscount = discountPercent(context.subtotal, 0.25, 50_000);
      if (!isWeekday(context.bookingDate)) {
        result.status = 'ineligible';
        result.message = 'This offer is only available from Monday to Friday.';
        return result;
      }
      if (!context.paymentMethod || !['visa', 'atm'].includes(context.paymentMethod)) {
        result.status = 'pending';
        result.message = 'Choose Visa/Mastercard or ATM/Internet Banking to receive 25% off.';
        return result;
      }
      result.status = 'applied';
      result.discount = result.estimatedDiscount;
      result.message = 'The 25% bank payment discount has been applied.';
      return result;

    case 'p5':
      result.estimatedDiscount = discountPercent(context.subtotal, 0.2);
      if (seatCount < 5) {
        result.status = 'pending';
        result.message = 'Book at least 5 tickets in one order to unlock this group offer.';
        return result;
      }
      result.status = 'applied';
      result.discount = result.estimatedDiscount;
      result.message = 'The 20% group booking discount has been applied.';
      return result;

    case 'p6':
      result.estimatedDiscount = COMBO_GOLD_BASE_PRICE - COMBO_GOLD_PROMO_PRICE;
      if (getSnackComboQuantity(context.snackCombos, 'combo-gold') === 0) {
        result.status = 'pending';
        result.message = 'Add at least 1 Combo Gold in the snack section to receive the special 85,000 VND price.';
        return result;
      }
      result.status = 'applied';
      result.discount = result.estimatedDiscount;
      result.message = 'Combo Gold special pricing has been applied.';
      result.bonusLabel = 'Combo Gold for 85,000 VND';
      return result;

    case 'p7':
      result.estimatedDiscount = Math.min(PRICES.regular, 120_000);
      if (!context.birthday) {
        result.status = 'pending';
        result.message = 'Enter your birthday at checkout so we can verify this offer.';
        return result;
      }
      if (!birthdayMatchesMonth(context.birthday, context.bookingDate)) {
        result.status = 'ineligible';
        result.message = 'This offer is only valid during your birthday month.';
        return result;
      }
      result.status = 'applied';
      result.discount = result.estimatedDiscount;
      result.message = 'Your free birthday ticket has been applied.';
      return result;

    case 'p8':
      result.estimatedDiscount = discountPercent(context.subtotal, 0.35);
      if (!isMorningShowtime(context.showtime)) {
        result.status = 'ineligible';
        result.message = 'This offer is only valid for showtimes between 09:00 and 10:59.';
        return result;
      }
      result.status = 'applied';
      result.discount = result.estimatedDiscount;
      result.message = 'The 35% early bird discount has been applied.';
      return result;

    default:
      result.status = 'ineligible';
      result.message = 'This promotion is not supported yet.';
      return result;
  }
}

export function getPromotionFinalTotal(
  baseTotal: number,
  evaluation: PromotionEvaluation | null,
  snackCombos: SelectedSnackCombo[] = []
) {
  const comboBase = snackCombos.reduce((sum, selectedCombo) => {
    const combo = getSnackComboById(selectedCombo.id);
    if (!combo) return sum;
    return sum + (combo.originalPrice ?? combo.price) * selectedCombo.quantity;
  }, 0);
  const comboCost = snackCombos.reduce((sum, selectedCombo) => {
    const combo = getSnackComboById(selectedCombo.id);
    if (!combo) return sum;
    return sum + combo.price * selectedCombo.quantity;
  }, 0);
  const discount = evaluation?.discount ?? 0;

  return {
    comboBase,
    comboCost,
    discount,
    total: baseTotal + comboCost - discount,
  };
}
