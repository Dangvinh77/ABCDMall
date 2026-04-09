import type { Movie } from './movie';
import { nowShowingMovies, comingSoonMovies } from './movie';
import { getDefaultBookingDate } from './promotions';
export type HallType = '2D' | '3D' | 'IMAX' | '4DX';
export type Language = 'sub' | 'dub';

export interface Showtime {
  id: string;
  time: string;
  hallType: HallType;
  language: Language;
  availableSeats: number;
  totalSeats: number;
  priceFrom: number; // VND
}

export interface CinemaSchedule {
  cinemaId: string;
  cinemaName: string;
  cinemaAddress: string;
  showtimes: Showtime[];
}

export interface MovieSchedule {
  movie: Movie;
  cinemaSchedules: CinemaSchedule[];
}

export interface ScheduleDateOption {
  idx: number;
  date: Date;
  dayLabel: string;
  day: number;
  month: number;
  fullLabel: string;
}

function s(
  id: string,
  time: string,
  hallType: HallType,
  language: Language,
  availableSeats: number,
  totalSeats: number,
  priceFrom: number,
): Showtime {
  return { id, time, hallType, language, availableSeats, totalSeats, priceFrom };
}
const extraMovies: Movie[] = [
  {
    id: 'cuoc-chien-ngam',
    title: 'Shadow War',
    description:
      'Veteran detective Marcus Black takes on a murder case that spirals into a covert war between international crime syndicates and the institution that once trained him.',
    genre: 'Action, Crime',
    rating: 8.1,
    duration: '128 min',
    director: 'Matt Reeves',
    cast: ['Ryan Gosling', 'Ana de Armas', 'John David Washington'],
    language: 'English with Vietnamese subtitles',
    ageRating: 'T16',
    imageUrl:
      'https://images.unsplash.com/photo-1765510296004-614b6cc204da?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxhY3Rpb24lMjBtb3ZpZSUyMHBvc3RlcnxlbnwxfHx8fDE3NzQ1NjY1NjF8MA&ixlib=rb-4.1.0&q=80&w=1080',
    cinemas: [],
  },
  {
    id: 'ngu-hanh-son',
    title: 'Five Sacred Peaks',
    description:
      'Five young guardians inherit the power of sacred stones and must learn to control their gifts before an ancient darkness rises again.',
    genre: 'Fantasy, Action',
    rating: 7.9,
    duration: '143 min',
    director: 'Ly Hai',
    cast: ['Tran Thanh', 'Ngo Thanh Van', 'Kieu Minh Tuan'],
    language: 'Vietnamese',
    ageRating: 'T13',
    imageUrl:
      'https://images.unsplash.com/photo-1761948245703-cbf27a3e7502?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxmYW50YXN5JTIwYWR2ZW50dXJlJTIwbW92aWV8ZW58MXx8fHwxNzc0NTk4NzcwfDA&ixlib=rb-4.1.0&q=80&w=1080',
    cinemas: [],
  },
];
export const scheduleMovies: Movie[] = [
  ...nowShowingMovies,
  { ...comingSoonMovies[0], isComingSoon: false },
  { ...comingSoonMovies[1], isComingSoon: false },
  { ...comingSoonMovies[2], isComingSoon: false },
  ...extraMovies,
];
const cinemas = {
  abcdMall: {
    cinemaId: 'abcd-mall',
    cinemaName: 'ABCD Cinema - ABCD Mall',
    cinemaAddress: 'Level 5, ABCD Mall, 123 Le Van Viet Street, Ho Chi Minh City',
  },
  quan1: {
    cinemaId: 'quan-1',
    cinemaName: 'ABCD Cinema - District 1',
    cinemaAddress: '456 Nguyen Hue Boulevard, District 1, Ho Chi Minh City',
  },
  binhDuong: {
    cinemaId: 'binh-duong',
    cinemaName: 'ABCD Cinema - Binh Duong',
    cinemaAddress: 'Level 3, Aeon Mall Binh Duong, Thuan An, Binh Duong',
  },
  goVap: {
    cinemaId: 'go-vap',
    cinemaName: 'ABCD Cinema - Go Vap',
    cinemaAddress: '789 Phan Van Tri Street, Go Vap, Ho Chi Minh City',
  },
} as const;
const DEFAULT_SCHEDULE_DATE = getDefaultBookingDate();
const WEEKDAY_LABELS = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];

function parseScheduleDate(dateParam: string) {
  const [year, month, day] = dateParam.split('-').map(Number);
  return new Date(year, month - 1, day);
}

export function formatScheduleDateParam(date: Date) {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

export function getScheduleDates(count = 7, startDate = DEFAULT_SCHEDULE_DATE): ScheduleDateOption[] {
  const today = parseScheduleDate(startDate);
  return Array.from({ length: count }, (_, idx) => {
    const date = new Date(today);
    date.setDate(today.getDate() + idx);

    return {
      idx,
      date,
      dayLabel: idx === 0 ? 'Today' : idx === 1 ? 'Tomorrow' : WEEKDAY_LABELS[date.getDay()],
      day: date.getDate(),
      month: date.getMonth(),
      fullLabel:
        idx === 0
          ? `Today, ${date.getDate()}/${date.getMonth() + 1}`
          : idx === 1
            ? `Tomorrow, ${date.getDate()}/${date.getMonth() + 1}`
            : `${WEEKDAY_LABELS[date.getDay()]}, ${date.getDate()}/${date.getMonth() + 1}`,
    };
  });
}

function getDayOffset(dateParam: string) {
  const targetDate = parseScheduleDate(dateParam);
  const baseDate = parseScheduleDate(DEFAULT_SCHEDULE_DATE);
  const diffMs = targetDate.getTime() - baseDate.getTime();
  return Math.round(diffMs / 86_400_000);
}

function shiftTime(time: string, deltaMinutes: number) {
  const [hours, minutes] = time.split(':').map(Number);
  const totalMinutes = (hours * 60 + minutes + deltaMinutes + 24 * 60) % (24 * 60);
  const nextHours = String(Math.floor(totalMinutes / 60)).padStart(2, '0');
  const nextMinutes = String(totalMinutes % 60).padStart(2, '0');
  return `${nextHours}:${nextMinutes}`;
}

function clamp(value: number, min: number, max: number) {
  return Math.max(min, Math.min(max, value));
}

function buildDailyShowtime(
  showtime: Showtime,
  dateParam: string,
  movieIndex: number,
  cinemaIndex: number,
  showtimeIndex: number,
): Showtime {
  const dayOffset = getDayOffset(dateParam);
  const variationSeed = dayOffset * 17 + movieIndex * 11 + cinemaIndex * 7 + showtimeIndex * 5;
  const timeShift = ((dayOffset + movieIndex + cinemaIndex + showtimeIndex) % 3 - 1) * 15;
  const seatsDelta = (variationSeed % 41) - 20;
  const availableSeats = clamp(showtime.availableSeats + seatsDelta, 0, showtime.totalSeats);

  return {
    ...showtime,
    id: `${showtime.id}-${dateParam.replace(/-/g, '')}`,
    time: shiftTime(showtime.time, timeShift),
    availableSeats:
      (dayOffset + cinemaIndex + showtimeIndex) % 9 === 0
        ? 0
        : availableSeats,
  };
}

const baseScheduleData: MovieSchedule[] = [
  {
    movie: scheduleMovies[0],
    cinemaSchedules: [
      {
        ...cinemas.abcdMall,
        showtimes: [
          s('htv-am-0930', '09:30', 'IMAX', 'sub', 210, 240, 140_000),
          s('htv-am-1200', '12:00', '3D', 'sub', 95, 180, 120_000),
          s('htv-am-1430', '14:30', 'IMAX', 'sub', 118, 240, 140_000),
          s('htv-am-1700', '17:00', '2D', 'dub', 152, 200, 85_000),
          s('htv-am-1930', '19:30', 'IMAX', 'sub', 9, 240, 140_000),
          s('htv-am-2200', '22:00', '3D', 'sub', 145, 180, 120_000),
        ],
      },
      {
        ...cinemas.quan1,
        showtimes: [
          s('htv-q1-1000', '10:00', '3D', 'sub', 88, 150, 120_000),
          s('htv-q1-1315', '13:15', '2D', 'sub', 115, 180, 85_000),
          s('htv-q1-1600', '16:00', '3D', 'dub', 42, 150, 120_000),
          s('htv-q1-1900', '19:00', '2D', 'sub', 6, 180, 85_000),
          s('htv-q1-2145', '21:45', '3D', 'sub', 130, 150, 120_000),
        ],
      },
      {
        ...cinemas.binhDuong,
        showtimes: [
          s('htv-bd-0845', '08:45', '2D', 'sub', 175, 200, 85_000),
          s('htv-bd-1130', '11:30', '4DX', 'sub', 78, 120, 160_000),
          s('htv-bd-1400', '14:00', '2D', 'sub', 133, 200, 85_000),
          s('htv-bd-1645', '16:45', '4DX', 'sub', 4, 120, 160_000),
          s('htv-bd-1915', '19:15', '2D', 'dub', 88, 200, 85_000),
          s('htv-bd-2130', '21:30', '4DX', 'dub', 0, 120, 160_000),
        ],
      },
      {
        ...cinemas.goVap,
        showtimes: [
          s('htv-gv-1030', '10:30', '3D', 'sub', 100, 150, 120_000),
          s('htv-gv-1300', '13:00', '2D', 'sub', 140, 180, 85_000),
          s('htv-gv-1530', '15:30', '3D', 'dub', 62, 150, 120_000),
          s('htv-gv-1800', '18:00', '2D', 'sub', 0, 180, 85_000),
          s('htv-gv-2030', '20:30', '3D', 'sub', 73, 150, 120_000),
        ],
      },
    ],
  },
  {
    movie: scheduleMovies[1],
    cinemaSchedules: [
      {
        ...cinemas.abcdMall,
        showtimes: [
          s('batnc-am-1000', '10:00', '2D', 'sub', 155, 200, 85_000),
          s('batnc-am-1330', '13:30', '3D', 'sub', 88, 180, 120_000),
          s('batnc-am-1600', '16:00', '2D', 'dub', 112, 200, 85_000),
          s('batnc-am-1900', '19:00', '3D', 'sub', 12, 180, 120_000),
          s('batnc-am-2130', '21:30', '2D', 'sub', 168, 200, 85_000),
        ],
      },
      {
        ...cinemas.quan1,
        showtimes: [
          s('batnc-q1-1100', '11:00', '2D', 'sub', 128, 180, 85_000),
          s('batnc-q1-1400', '14:00', '3D', 'sub', 48, 150, 120_000),
          s('batnc-q1-1700', '17:00', '2D', 'dub', 98, 180, 85_000),
          s('batnc-q1-2000', '20:00', '3D', 'sub', 18, 150, 120_000),
          s('batnc-q1-2230', '22:30', '2D', 'sub', 155, 180, 85_000),
        ],
      },
      {
        ...cinemas.goVap,
        showtimes: [
          s('batnc-gv-1200', '12:00', '2D', 'sub', 140, 180, 85_000),
          s('batnc-gv-1500', '15:00', '3D', 'sub', 75, 150, 120_000),
          s('batnc-gv-1830', '18:30', '2D', 'sub', 0, 180, 85_000),
          s('batnc-gv-2100', '21:00', '3D', 'dub', 108, 150, 120_000),
        ],
      },
    ],
  },
  {
    movie: scheduleMovies[2],
    cinemaSchedules: [
      {
        ...cinemas.abcdMall,
        showtimes: [
          s('nvbkt-am-0930', '09:30', '2D', 'sub', 188, 200, 85_000),
          s('nvbkt-am-1200', '12:00', 'IMAX', 'sub', 52, 240, 140_000),
          s('nvbkt-am-1430', '14:30', '3D', 'dub', 105, 180, 120_000),
          s('nvbkt-am-1700', '17:00', 'IMAX', 'sub', 5, 240, 140_000),
          s('nvbkt-am-1930', '19:30', '3D', 'sub', 30, 180, 120_000),
          s('nvbkt-am-2200', '22:00', '2D', 'sub', 145, 200, 85_000),
        ],
      },
      {
        ...cinemas.quan1,
        showtimes: [
          s('nvbkt-q1-1000', '10:00', '2D', 'sub', 160, 180, 85_000),
          s('nvbkt-q1-1315', '13:15', '3D', 'sub', 72, 150, 120_000),
          s('nvbkt-q1-1600', '16:00', '2D', 'dub', 94, 180, 85_000),
          s('nvbkt-q1-1900', '19:00', '3D', 'sub', 8, 150, 120_000),
          s('nvbkt-q1-2145', '21:45', '2D', 'sub', 148, 180, 85_000),
        ],
      },
      {
        ...cinemas.binhDuong,
        showtimes: [
          s('nvbkt-bd-0845', '08:45', '4DX', 'sub', 98, 120, 160_000),
          s('nvbkt-bd-1130', '11:30', '2D', 'sub', 170, 200, 85_000),
          s('nvbkt-bd-1400', '14:00', '4DX', 'sub', 7, 120, 160_000),
          s('nvbkt-bd-1645', '16:45', '2D', 'dub', 130, 200, 85_000),
          s('nvbkt-bd-1915', '19:15', '4DX', 'sub', 0, 120, 160_000),
          s('nvbkt-bd-2130', '21:30', '2D', 'sub', 85, 200, 85_000),
        ],
      },
      {
        ...cinemas.goVap,
        showtimes: [
          s('nvbkt-gv-1030', '10:30', '2D', 'sub', 142, 180, 85_000),
          s('nvbkt-gv-1300', '13:00', '3D', 'sub', 55, 150, 120_000),
          s('nvbkt-gv-1530', '15:30', '2D', 'dub', 100, 180, 85_000),
          s('nvbkt-gv-1800', '18:00', '3D', 'sub', 12, 150, 120_000),
          s('nvbkt-gv-2030', '20:30', '2D', 'sub', 165, 180, 85_000),
        ],
      },
    ],
  },
  {
    movie: scheduleMovies[3],
    cinemaSchedules: [
      {
        ...cinemas.abcdMall,
        showtimes: [
          s('ctjhh-am-1100', '11:00', '2D', 'sub', 172, 200, 85_000),
          s('ctjhh-am-1400', '14:00', '2D', 'dub', 95, 200, 85_000),
          s('ctjhh-am-1700', '17:00', '2D', 'sub', 145, 200, 85_000),
          s('ctjhh-am-2000', '20:00', '2D', 'sub', 60, 200, 85_000),
        ],
      },
      {
        ...cinemas.binhDuong,
        showtimes: [
          s('ctjhh-bd-1000', '10:00', '2D', 'sub', 185, 200, 85_000),
          s('ctjhh-bd-1300', '13:00', '2D', 'dub', 110, 200, 85_000),
          s('ctjhh-bd-1600', '16:00', '2D', 'sub', 75, 200, 85_000),
          s('ctjhh-bd-1900', '19:00', '2D', 'sub', 20, 200, 85_000),
        ],
      },
    ],
  },
  {
    movie: scheduleMovies[4],
    cinemaSchedules: [
      {
        ...cinemas.abcdMall,
        showtimes: [
          s('vqtt-am-1000', '10:00', 'IMAX', 'sub', 215, 240, 140_000),
          s('vqtt-am-1330', '13:30', '3D', 'sub', 135, 180, 120_000),
          s('vqtt-am-1700', '17:00', 'IMAX', 'sub', 55, 240, 140_000),
          s('vqtt-am-2030', '20:30', '3D', 'sub', 11, 180, 120_000),
        ],
      },
      {
        ...cinemas.quan1,
        showtimes: [
          s('vqtt-q1-1100', '11:00', '3D', 'sub', 120, 150, 120_000),
          s('vqtt-q1-1415', '14:15', '2D', 'sub', 148, 180, 85_000),
          s('vqtt-q1-1800', '18:00', '3D', 'sub', 38, 150, 120_000),
          s('vqtt-q1-2115', '21:15', '2D', 'sub', 130, 180, 85_000),
        ],
      },
      {
        ...cinemas.goVap,
        showtimes: [
          s('vqtt-gv-1230', '12:30', '3D', 'sub', 105, 150, 120_000),
          s('vqtt-gv-1600', '16:00', '2D', 'dub', 158, 180, 85_000),
          s('vqtt-gv-1930', '19:30', '3D', 'sub', 22, 150, 120_000),
        ],
      },
    ],
  },
  {
    movie: scheduleMovies[5],
    cinemaSchedules: [
      {
        ...cinemas.abcdMall,
        showtimes: [
          s('amdk-am-1130', '11:30', '3D', 'sub', 148, 180, 120_000),
          s('amdk-am-1430', '14:30', '2D', 'sub', 95, 200, 85_000),
          s('amdk-am-1900', '19:00', '3D', 'sub', 6, 180, 120_000),
          s('amdk-am-2200', '22:00', '2D', 'sub', 155, 200, 85_000),
        ],
      },
      {
        ...cinemas.goVap,
        showtimes: [
          s('amdk-gv-1300', '13:00', '2D', 'sub', 130, 180, 85_000),
          s('amdk-gv-1630', '16:30', '3D', 'sub', 48, 150, 120_000),
          s('amdk-gv-2000', '20:00', '2D', 'dub', 0, 180, 85_000),
          s('amdk-gv-2230', '22:30', '3D', 'sub', 115, 150, 120_000),
        ],
      },
    ],
  },
  {
    movie: scheduleMovies[6],
    cinemaSchedules: [
      {
        ...cinemas.binhDuong,
        showtimes: [
          s('hpbn-bd-1000', '10:00', '2D', 'sub', 190, 200, 85_000),
          s('hpbn-bd-1230', '12:30', '2D', 'dub', 140, 200, 85_000),
          s('hpbn-bd-1500', '15:00', '2D', 'sub', 105, 200, 85_000),
          s('hpbn-bd-1730', '17:30', '2D', 'sub', 25, 200, 85_000),
          s('hpbn-bd-2000', '20:00', '2D', 'dub', 170, 200, 85_000),
        ],
      },
      {
        ...cinemas.goVap,
        showtimes: [
          s('hpbn-gv-1100', '11:00', '2D', 'sub', 165, 180, 85_000),
          s('hpbn-gv-1400', '14:00', '2D', 'dub', 88, 180, 85_000),
          s('hpbn-gv-1630', '16:30', '2D', 'sub', 142, 180, 85_000),
          s('hpbn-gv-1900', '19:00', '2D', 'sub', 45, 180, 85_000),
        ],
      },
    ],
  },
  {
    movie: scheduleMovies[7],
    cinemaSchedules: [
      {
        ...cinemas.abcdMall,
        showtimes: [
          s('ccn-am-1000', '10:00', '3D', 'sub', 158, 180, 120_000),
          s('ccn-am-1300', '13:00', '2D', 'sub', 120, 200, 85_000),
          s('ccn-am-1600', '16:00', '3D', 'dub', 68, 180, 120_000),
          s('ccn-am-1900', '19:00', '2D', 'sub', 14, 200, 85_000),
          s('ccn-am-2145', '21:45', '3D', 'sub', 140, 180, 120_000),
        ],
      },
      {
        ...cinemas.quan1,
        showtimes: [
          s('ccn-q1-1115', '11:15', '2D', 'sub', 142, 180, 85_000),
          s('ccn-q1-1415', '14:15', '3D', 'sub', 78, 150, 120_000),
          s('ccn-q1-1730', '17:30', '2D', 'dub', 108, 180, 85_000),
          s('ccn-q1-2030', '20:30', '3D', 'sub', 18, 150, 120_000),
        ],
      },
      {
        ...cinemas.goVap,
        showtimes: [
          s('ccn-gv-1200', '12:00', '2D', 'sub', 135, 180, 85_000),
          s('ccn-gv-1500', '15:00', '3D', 'sub', 62, 150, 120_000),
          s('ccn-gv-1800', '18:00', '2D', 'sub', 0, 180, 85_000),
          s('ccn-gv-2100', '21:00', '2D', 'dub', 118, 180, 85_000),
        ],
      },
    ],
  },
  {
    movie: scheduleMovies[8],
    cinemaSchedules: [
      {
        ...cinemas.abcdMall,
        showtimes: [
          s('nhs-am-0930', '09:30', '2D', 'sub', 180, 200, 85_000),
          s('nhs-am-1230', '12:30', '3D', 'sub', 105, 180, 120_000),
          s('nhs-am-1530', '15:30', '2D', 'sub', 72, 200, 85_000),
          s('nhs-am-1830', '18:30', '3D', 'sub', 8, 180, 120_000),
          s('nhs-am-2100', '21:00', '2D', 'sub', 148, 200, 85_000),
        ],
      },
      {
        ...cinemas.binhDuong,
        showtimes: [
          s('nhs-bd-1000', '10:00', '2D', 'sub', 195, 200, 85_000),
          s('nhs-bd-1300', '13:00', '2D', 'sub', 112, 200, 85_000),
          s('nhs-bd-1600', '16:00', '4DX', 'sub', 45, 120, 160_000),
          s('nhs-bd-1900', '19:00', '2D', 'sub', 22, 200, 85_000),
          s('nhs-bd-2130', '21:30', '4DX', 'sub', 88, 120, 160_000),
        ],
      },
      {
        ...cinemas.goVap,
        showtimes: [
          s('nhs-gv-1100', '11:00', '2D', 'sub', 158, 180, 85_000),
          s('nhs-gv-1400', '14:00', '3D', 'sub', 72, 150, 120_000),
          s('nhs-gv-1700', '17:00', '2D', 'dub', 94, 180, 85_000),
          s('nhs-gv-2000', '20:00', '3D', 'sub', 16, 150, 120_000),
        ],
      },
    ],
  },
];

export function getScheduleDataForDate(dateParam: string): MovieSchedule[] {
  return baseScheduleData.map((movieSchedule, movieIndex) => ({
    ...movieSchedule,
    cinemaSchedules: movieSchedule.cinemaSchedules.map((cinemaSchedule, cinemaIndex) => {
      const dailyShowtimes = cinemaSchedule.showtimes
        .filter((_, showtimeIndex, showtimes) => {
          if (showtimes.length <= 3) return true;
          const dayOffset = getDayOffset(dateParam);
          return (dayOffset + movieIndex + cinemaIndex + showtimeIndex) % 5 !== 0;
        })
        .map((showtime, showtimeIndex) =>
          buildDailyShowtime(showtime, dateParam, movieIndex, cinemaIndex, showtimeIndex),
        );

      return {
        ...cinemaSchedule,
        showtimes: dailyShowtimes,
      };
    }),
  }));
}

export const scheduleData = getScheduleDataForDate(DEFAULT_SCHEDULE_DATE);
export const allCinemas = [
  { id: 'all', name: 'All cinemas' },
  { id: 'abcd-mall', name: 'ABCD Mall' },
  { id: 'quan-1', name: 'District 1' },
  { id: 'binh-duong', name: 'Binh Duong' },
  { id: 'go-vap', name: 'Go Vap' },
];



