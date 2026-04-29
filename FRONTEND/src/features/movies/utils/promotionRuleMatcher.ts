import type { PromotionModel } from '../api/moviesApi';

const CINEMA_TIME_ZONE = 'Asia/Ho_Chi_Minh';

export function normalizePromotionRuleType(ruleType: string) {
  return ruleType.trim().toLowerCase();
}

function parseVietnamTimeParts(showtimeStartAtUtc?: string | null) {
  if (!showtimeStartAtUtc) {
    return null;
  }

  const parsed = new Date(showtimeStartAtUtc);
  if (Number.isNaN(parsed.getTime())) {
    return null;
  }

  const formatted = new Intl.DateTimeFormat('en-GB', {
    hour: '2-digit',
    minute: '2-digit',
    hour12: false,
    timeZone: CINEMA_TIME_ZONE,
  }).format(parsed);

  const [hourText, minuteText] = formatted.split(':');
  const hour = Number(hourText);
  const minute = Number(minuteText);

  if (!Number.isFinite(hour) || !Number.isFinite(minute)) {
    return null;
  }

  return { hour, minute };
}

function parseBusinessDateParts(businessDate?: string | null) {
  if (!businessDate) {
    return null;
  }

  const [yearText, monthText, dayText] = businessDate.split('-');
  const year = Number(yearText);
  const month = Number(monthText);
  const day = Number(dayText);

  if (!Number.isFinite(year) || !Number.isFinite(month) || !Number.isFinite(day)) {
    return null;
  }

  const date = new Date(Date.UTC(year, month - 1, day));
  return {
    year,
    month,
    day,
    dayOfWeek: date.getUTCDay(),
  };
}

function tryParseTimeWindow(ruleValue: string) {
  const [startText, endText] = ruleValue.split('-').map((segment) => segment.trim());
  if (!startText || !endText) {
    return null;
  }

  const [startHourText, startMinuteText = '0'] = startText.split(':');
  const [endHourText, endMinuteText = '0'] = endText.split(':');
  const startHour = Number(startHourText);
  const startMinute = Number(startMinuteText);
  const endHour = Number(endHourText);
  const endMinute = Number(endMinuteText);

  if (
    !Number.isFinite(startHour)
    || !Number.isFinite(startMinute)
    || !Number.isFinite(endHour)
    || !Number.isFinite(endMinute)
  ) {
    return null;
  }

  return {
    startHour,
    startMinute,
    endHour,
    endMinute,
  };
}

function isTimeWithinWindow(
  hour: number,
  minute: number,
  startHour: number,
  startMinute: number,
  endHour: number,
  endMinute: number,
) {
  const value = hour * 60 + minute;
  const start = startHour * 60 + startMinute;
  const end = endHour * 60 + endMinute;
  return value >= start && value < end;
}

export function matchesShowtimeRule(
  ruleValue: string,
  showtimeId?: string | null,
  showtimeStartAtUtc?: string | null,
) {
  const normalizedValue = ruleValue.trim();

  if (
    showtimeId
    && /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(normalizedValue)
  ) {
    return normalizedValue.toLowerCase() === showtimeId.toLowerCase();
  }

  const timeParts = parseVietnamTimeParts(showtimeStartAtUtc);
  if (!timeParts) {
    return false;
  }

  if (normalizedValue.toLowerCase() === 'morning') {
    return isTimeWithinWindow(timeParts.hour, timeParts.minute, 9, 0, 11, 0);
  }

  if (normalizedValue.toLowerCase() === 'afternoon') {
    return isTimeWithinWindow(timeParts.hour, timeParts.minute, 11, 0, 17, 0);
  }

  if (normalizedValue.toLowerCase() === 'evening') {
    return timeParts.hour * 60 + timeParts.minute >= 17 * 60;
  }

  const parsedWindow = tryParseTimeWindow(normalizedValue);
  if (!parsedWindow) {
    return false;
  }

  return isTimeWithinWindow(
    timeParts.hour,
    timeParts.minute,
    parsedWindow.startHour,
    parsedWindow.startMinute,
    parsedWindow.endHour,
    parsedWindow.endMinute,
  );
}

export function matchesBusinessDateRule(ruleValue: string, businessDate?: string | null) {
  const normalizedValue = ruleValue.trim();
  const businessDateParts = parseBusinessDateParts(businessDate);
  if (!businessDateParts) {
    return false;
  }

  if (normalizedValue.toLowerCase() === 'weekend') {
    return businessDateParts.dayOfWeek === 0 || businessDateParts.dayOfWeek === 6;
  }

  return normalizedValue === `${businessDateParts.year.toString().padStart(4, '0')}-${businessDateParts.month.toString().padStart(2, '0')}-${businessDateParts.day.toString().padStart(2, '0')}`;
}

export function matchesPromotionShowtimeContext(
  promotion: PromotionModel,
  context: {
    businessDate?: string | null;
    showtimeId?: string | null;
    showtimeStartAtUtc?: string | null;
  },
) {
  for (const rule of promotion.rules) {
    const ruleType = normalizePromotionRuleType(rule.ruleType);

    if (ruleType === 'showtime') {
      if (!matchesShowtimeRule(rule.ruleValue, context.showtimeId, context.showtimeStartAtUtc) && rule.isRequired) {
        return false;
      }
      continue;
    }

    if (ruleType === 'businessdate') {
      if (!matchesBusinessDateRule(rule.ruleValue, context.businessDate) && rule.isRequired) {
        return false;
      }
    }
  }

  return true;
}
