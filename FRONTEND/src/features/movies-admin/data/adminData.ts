import { allMovies, cinemasList, comingSoonMovies, nowShowingMovies } from '../../movies/data/movie';
import { getDefaultBookingDate } from '../../movies/data/promotions';
import { getScheduleDataForDate } from '../../movies/data/schedules';

export type AdminSectionId =
  | 'movies'
  | 'showtimes'
  | 'seats'
  | 'bookings'
  | 'payments'
  | 'emails'
  | 'guests'
  | 'settings'
  | 'promotions'
  | 'logs'
  | 'users';

export interface SectionMetric {
  label: string;
  value: string;
  hint: string;
}

export interface SectionTable {
  columns: string[];
  rows: string[][];
}

export interface SectionPanel {
  title: string;
  description: string;
  bullets: string[];
}

export interface AdminSectionContent {
  eyebrow: string;
  title: string;
  description: string;
  metrics: SectionMetric[];
  table: SectionTable;
  panels: SectionPanel[];
}

export interface DashboardStat {
  label: string;
  value: string;
  change: string;
  tone: 'violet' | 'emerald' | 'cyan' | 'amber' | 'rose';
}

export interface DashboardTopMovie {
  title: string;
  genre: string;
  tickets: number;
  revenue: string;
  occupancy: string;
}

export interface DashboardAlert {
  title: string;
  detail: string;
  severity: 'high' | 'medium' | 'low';
}

function vnd(value: number) {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value);
}

const bookingDate = getDefaultBookingDate();
const scheduleData = getScheduleDataForDate(bookingDate);
const totalShowtimes = scheduleData.reduce(
  (sum, movieSchedule) =>
    sum +
    movieSchedule.cinemaSchedules.reduce(
      (cinemaSum, cinemaSchedule) => cinemaSum + cinemaSchedule.showtimes.length,
      0,
    ),
  0,
);
const totalSeats = scheduleData.reduce(
  (sum, movieSchedule) =>
    sum +
    movieSchedule.cinemaSchedules.reduce(
      (cinemaSum, cinemaSchedule) =>
        cinemaSum +
        cinemaSchedule.showtimes.reduce((showtimeSum, showtime) => showtimeSum + showtime.totalSeats, 0),
      0,
    ),
  0,
);
const soldSeats = scheduleData.reduce(
  (sum, movieSchedule) =>
    sum +
    movieSchedule.cinemaSchedules.reduce(
      (cinemaSum, cinemaSchedule) =>
        cinemaSum +
        cinemaSchedule.showtimes.reduce(
          (showtimeSum, showtime) => showtimeSum + (showtime.totalSeats - showtime.availableSeats),
          0,
        ),
      0,
    ),
  0,
);
const occupancyRate = totalSeats === 0 ? 0 : Math.round((soldSeats / totalSeats) * 100);
const activeMovies = Array.from(new Set(scheduleData.map((item) => item.movie.id)));
const uniqueMovies = Array.from(new Map(allMovies.map((movie) => [movie.id, movie])).values());

const movieAdminRows = uniqueMovies.slice(0, 8).map((movie, index) => [
  movie.title,
  movie.genre,
  movie.duration,
  movie.cinemas?.length ? `${movie.cinemas.length} cinemas` : `${1 + (index % 3)} cinemas`,
  movie.isComingSoon ? 'Coming soon' : 'Now showing',
]);

const showtimeRows = scheduleData
  .slice(0, 8)
  .flatMap((movieSchedule) =>
    movieSchedule.cinemaSchedules.slice(0, 1).flatMap((cinemaSchedule) =>
      cinemaSchedule.showtimes.slice(0, 2).map((showtime) => [
        movieSchedule.movie.title,
        cinemaSchedule.cinemaName.replace('ABCD Cinema - ', ''),
        bookingDate,
        showtime.time,
        showtime.hallType,
        vnd(showtime.priceFrom),
        `${showtime.totalSeats - showtime.availableSeats}/${showtime.totalSeats}`,
      ]),
    ),
  )
  .slice(0, 8);

const seatRows = [
  ['Hall A - IMAX', 'ABCD Mall', '240', '42 VIP / 198 Regular', 'Live booking'],
  ['Hall B - 3D', 'District 1', '180', '24 VIP / 156 Regular', 'Syncing'],
  ['Hall C - 4DX', 'Binh Duong', '120', '18 VIP / 102 Regular', 'Maintenance check'],
  ['Hall D - Couple', 'Go Vap', '160', '20 Couple / 36 VIP / 104 Regular', 'Ready'],
];

const bookingRows = [
  ['ABCD-20260406-1001', 'linh.nguyen@gmail.com', 'Cosmic Odyssey - 19:30', 'D5, D6', 'Paid'],
  ['ABCD-20260406-1002', 'guest.hcm@mail.com', 'Shadow War - 17:00', 'F8, F9, F10', 'Pending'],
  ['ABCD-20260406-1003', 'phuongtran@outlook.com', 'Five Sacred Peaks - 20:00', 'C3, C4', 'Cancelled'],
  ['ABCD-20260406-1004', 'minhdo@gmail.com', 'The Old Building Secret - 21:45', 'E7, E8', 'Paid'],
];

const paymentRows = [
  ['PAY-20260406-8891', 'ABCD-20260406-1001', 'MoMo', vnd(318000), 'Success'],
  ['PAY-20260406-8892', 'ABCD-20260406-1002', 'VNPay', vnd(435000), 'Pending'],
  ['PAY-20260406-8893', 'ABCD-20260406-1003', 'Visa', vnd(210000), 'Failed'],
  ['PAY-20260406-8894', 'ABCD-20260406-1004', 'Stripe', vnd(280000), 'Success'],
];

const emailRows = [
  ['ETK-1001', 'linh.nguyen@gmail.com', 'Ticket delivered', '1st attempt', '10:21'],
  ['ETK-1002', 'guest.hcm@mail.com', 'Queued resend', 'SMTP retry', '10:34'],
  ['ETK-1003', 'phuongtran@outlook.com', 'Failed', 'Mailbox rejected', '10:36'],
  ['ETK-1004', 'minhdo@gmail.com', 'Ticket delivered', 'PDF attached', '10:48'],
];

const guestRows = [
  ['linh.nguyen@gmail.com', '12 orders', '7 movies', 'Trusted'],
  ['guest.hcm@mail.com', '5 orders', '4 movies', 'Needs follow-up'],
  ['phuongtran@outlook.com', '2 orders', '2 movies', 'Refund issued'],
  ['minhdo@gmail.com', '9 orders', '6 movies', 'Frequent buyer'],
];

const promotionRows = [
  ['WEEKEND30', '30% off tickets', '2,048 uses', '30/06/2026', 'Active'],
  ['MOMO30', '30% with MoMo', '1,344 uses', '15/05/2026', 'Active'],
  ['GROUP20', '20% for 5 tickets', '318 uses', '30/06/2026', 'Scheduled'],
  ['BIRTHDAYFREE', '1 free ticket', '126 uses', 'Ongoing', 'Active'],
];

const logRows = [
  ['booking-service', 'Order reserved for 10 minutes', 'Info', '10:20:31'],
  ['payment-callback', 'VNPay callback delayed > 30s', 'Warning', '10:34:12'],
  ['ticket-mailer', 'SMTP rejected recipient domain', 'Error', '10:36:27'],
  ['seat-locker', 'Recovered expired seat lock', 'Info', '10:42:55'],
];

const userRows = [
  ['admin.mall', 'Super admin', 'Full access', 'Active'],
  ['movie.ops', 'Movie operator', 'Movies + Showtimes', 'Active'],
  ['support.cs', 'Customer support', 'Bookings + Emails', 'Active'],
  ['finance.audit', 'Finance', 'Payments + Reports', 'Invite pending'],
];

export const dashboardStats: DashboardStat[] = [
  { label: 'Revenue today', value: vnd(128400000), change: '+12.4% vs yesterday', tone: 'violet' },
  { label: 'Tickets sold', value: `${soldSeats.toLocaleString('en-US')}`, change: `${activeMovies.length} active movies`, tone: 'emerald' },
  { label: 'Live showtimes', value: `${totalShowtimes}`, change: '18 sessions in progress', tone: 'cyan' },
  { label: 'Seat occupancy', value: `${occupancyRate}%`, change: 'Peak after 18:00', tone: 'amber' },
  { label: 'Payment issues', value: '7', change: '3 pending callbacks', tone: 'rose' },
];

export const dashboardRevenueBars = [
  { label: 'Mon', value: 58 },
  { label: 'Tue', value: 64 },
  { label: 'Wed', value: 72 },
  { label: 'Thu', value: 69 },
  { label: 'Fri', value: 88 },
  { label: 'Sat', value: 96 },
  { label: 'Sun', value: 91 },
];

export const dashboardTopMovies: DashboardTopMovie[] = nowShowingMovies.slice(0, 4).map((movie, index) => ({
  title: movie.title,
  genre: movie.genre,
  tickets: 420 + index * 96,
  revenue: vnd(54000000 + index * 8300000),
  occupancy: `${78 + index * 4}%`,
}));

export const dashboardAlerts: DashboardAlert[] = [
  {
    title: 'Pending VNPay callbacks',
    detail: '3 bookings are waiting for callback reconciliation before ticket issue.',
    severity: 'high',
  },
  {
    title: 'Showtime nearly sold out',
    detail: 'Cosmic Odyssey at 19:30 IMAX has less than 10 seats left.',
    severity: 'medium',
  },
  {
    title: 'Email delivery retry',
    detail: '2 failed ticket emails are queued for resend with fallback SMTP.',
    severity: 'low',
  },
];

export const adminSections: Record<AdminSectionId, AdminSectionContent> = {
  movies: {
    eyebrow: 'Movies catalog',
    title: 'Manage movies, posters, trailers and release status',
    description:
      'Control the public movie library that feeds the customer-facing movies experience, including artwork, runtime, genres and release readiness.',
    metrics: [
      { label: 'Now showing', value: `${nowShowingMovies.length}`, hint: 'Synced with home page carousel' },
      { label: 'Coming soon', value: `${comingSoonMovies.length}`, hint: 'Visible in teaser slots' },
      { label: 'Localized titles', value: '14', hint: 'EN + VN naming coverage' },
      { label: 'Missing trailers', value: '2', hint: 'Ready for content QA' },
    ],
    table: { columns: ['Movie', 'Genre', 'Duration', 'Coverage', 'Status'], rows: movieAdminRows },
    panels: [
      {
        title: 'Recommended actions',
        description: 'The catalog still has a few gaps that will affect the storefront.',
        bullets: [
          'Attach trailer URLs for the two upcoming titles before launch.',
          'Normalize genre tags so home page filters can work correctly.',
          'Review duplicated source movie ids before wiring backend CRUD.',
        ],
      },
      {
        title: 'Expected admin actions',
        description: 'This section should support the typical content workflow for movies.',
        bullets: [
          'Create, edit and archive movies.',
          'Upload poster, backdrop and trailer assets.',
          'Set release status: now showing, coming soon, hidden.',
        ],
      },
    ],
  },
  showtimes: {
    eyebrow: 'Showtime operations',
    title: 'Configure schedule slots, halls, pricing and seat inventory',
    description:
      'This is the operational core of the cinema flow. Admins need fast visibility into pricing, halls, occupancy and schedule conflicts.',
    metrics: [
      { label: 'Today showtimes', value: `${totalShowtimes}`, hint: `${cinemasList.length} cinemas in rotation` },
      { label: 'Avg occupancy', value: `${occupancyRate}%`, hint: 'Across all public sessions' },
      { label: 'Peak sessions', value: '22', hint: '18:00 - 21:30' },
      { label: 'Sold out', value: '5', hint: 'Immediate demand signal' },
    ],
    table: {
      columns: ['Movie', 'Cinema', 'Date', 'Time', 'Hall', 'Base price', 'Sold seats'],
      rows: showtimeRows,
    },
    panels: [
      {
        title: 'Pricing strategy',
        description: 'High and low demand windows can be layered on top of the current model.',
        bullets: [
          'Apply peak pricing from Friday evening to Sunday night.',
          'Allow hall-based overrides for IMAX and 4DX.',
          'Add seat-type multipliers for VIP and couple seats.',
        ],
      },
      {
        title: 'Future extensions',
        description: 'The UI is ready for more advanced showtime rules.',
        bullets: [
          'Conflict detection when one hall is double-booked.',
          'Bulk create schedules for weekly release runs.',
          'Pause ticket sales without deleting the showtime.',
        ],
      },
    ],
  },
  seats: {
    eyebrow: 'Seat layouts',
    title: 'Manage hall maps, seat classes and booking states',
    description:
      'Seat layout data must stay aligned with the guest booking page so operators can understand inventory, VIP allocation and couple-seat rules.',
    metrics: [
      { label: 'Managed halls', value: '4', hint: 'Across 4 cinema branches' },
      { label: 'VIP seats', value: '120', hint: 'Premium inventory' },
      { label: 'Couple seats', value: '40', hint: 'Booked in linked pairs' },
      { label: 'Blocked seats', value: '12', hint: 'Maintenance or outages' },
    ],
    table: { columns: ['Hall', 'Branch', 'Capacity', 'Seat mix', 'Status'], rows: seatRows },
    panels: [
      {
        title: 'Layout controls',
        description: 'Admins should be able to visualize and edit room topology.',
        bullets: [
          'Change seat types: regular, VIP, couple.',
          'Mark seats as blocked, reserved or available.',
          'Preview how the customer seat map will render.',
        ],
      },
      {
        title: 'Operational checks',
        description: 'Seat integrity matters for reliable bookings.',
        bullets: [
          'Lock both seats when a couple seat is selected.',
          'Keep unavailable seats hidden from customer checkout.',
          'Track hall-level maintenance windows.',
        ],
      },
    ],
  },
  bookings: {
    eyebrow: 'Orders and ticketing',
    title: 'Monitor guest bookings, resend tickets and manage exceptions',
    description:
      'Because customers checkout as guests, bookings management is the main support console for payment disputes, ticket recovery and refund handling.',
    metrics: [
      { label: 'Orders today', value: '384', hint: 'Guest checkout only' },
      { label: 'Pending payment', value: '19', hint: 'Need callback verification' },
      { label: 'Cancelled', value: '11', hint: 'Auto release after timeout' },
      { label: 'Resend requests', value: '7', hint: 'Ticket email support queue' },
    ],
    table: { columns: ['Booking code', 'Guest email', 'Showtime', 'Seats', 'Status'], rows: bookingRows },
    panels: [
      {
        title: 'Critical workflows',
        description: 'These are the top admin operations for a guest-based cinema flow.',
        bullets: [
          'Open booking detail with payment and email history.',
          'Resend ticket email and regenerate PDF attachments.',
          'Issue refund or force-cancel when support approves.',
        ],
      },
      {
        title: 'Support notes',
        description: 'Agents need context without asking users to log in.',
        bullets: [
          'Search by booking code or email address.',
          'Display exact seats, hall, payment status and resend attempts.',
          'Keep an audit trail for manual booking actions.',
        ],
      },
    ],
  },
  payments: {
    eyebrow: 'Payment operations',
    title: 'Track transaction results, callbacks and reconciliation issues',
    description:
      'This section focuses on payment gateways and debugging edge cases where customers have paid but tickets are still missing or delayed.',
    metrics: [
      { label: 'Successful today', value: '357', hint: '92.9% conversion' },
      { label: 'Pending callbacks', value: '3', hint: 'Monitor within 5 minutes' },
      { label: 'Failed charges', value: '9', hint: 'Requires investigation' },
      { label: 'Gateways active', value: '4', hint: 'MoMo, VNPay, Visa, Stripe' },
    ],
    table: { columns: ['Payment id', 'Booking', 'Method', 'Amount', 'Status'], rows: paymentRows },
    panels: [
      {
        title: 'Gateway debugging',
        description: 'Operators need payment-specific logs on the same screen.',
        bullets: [
          'Persist raw callback payloads from payment providers.',
          'Mark transactions that succeeded but did not issue tickets.',
          'Support manual reconciliation with booking records.',
        ],
      },
      {
        title: 'Risk checks',
        description: 'Useful controls before wiring real gateways.',
        bullets: [
          'Detect duplicate callback events.',
          'Flag mismatched amounts and stale pending payments.',
          'Store retry counts for callback polling or webhooks.',
        ],
      },
    ],
  },
  emails: {
    eyebrow: 'Email and ticket delivery',
    title: 'Observe ticket email health, resend failures and regenerate documents',
    description:
      'Guest checkout depends on successful email delivery, so this page needs full visibility into ticket sends, retries and PDF generation.',
    metrics: [
      { label: 'Sent today', value: '341', hint: 'Ticket delivery events' },
      { label: 'Failed sends', value: '4', hint: 'SMTP or recipient issues' },
      { label: 'Queued retries', value: '2', hint: 'Will resend automatically' },
      { label: 'PDF regenerated', value: '6', hint: 'Support-driven actions' },
    ],
    table: {
      columns: ['Ticket id', 'Recipient', 'Delivery state', 'Attempt', 'Last event'],
      rows: emailRows,
    },
    panels: [
      {
        title: 'Core actions',
        description: 'Operators should be able to recover ticket delivery quickly.',
        bullets: [
          'Resend email with one click.',
          'Preview the HTML/email payload that was sent.',
          'Download or regenerate the attached ticket PDF.',
        ],
      },
      {
        title: 'Monitoring',
        description: 'Delivery logs should explain why the user did not receive the ticket.',
        bullets: [
          'Differentiate SMTP failure from mailbox rejection.',
          'Capture template version and attachment build status.',
          'Surface delayed sends older than five minutes.',
        ],
      },
    ],
  },
  guests: {
    eyebrow: 'Guest customers',
    title: 'Support guest buyers by email history, booking patterns and risk signals',
    description:
      'Even without account login, the admin still needs a lightweight customer lens to support repeat buyers and spot suspicious activity.',
    metrics: [
      { label: 'Unique buyers', value: '1,284', hint: 'Last 30 days' },
      { label: 'Repeat buyers', value: '37%', hint: 'More than one booking' },
      { label: 'Flagged emails', value: '6', hint: 'Chargeback or spam patterns' },
      { label: 'Avg order value', value: vnd(286000), hint: 'Guest checkout basket' },
    ],
    table: { columns: ['Email', 'Orders', 'Movies watched', 'Support state'], rows: guestRows },
    panels: [
      {
        title: 'Support use cases',
        description: 'This view helps support agents answer users quickly.',
        bullets: [
          'Find all bookings by email address.',
          'See refund history and delivery failures for the same guest.',
          'Spot frequent complaints that indicate systemic issues.',
        ],
      },
      {
        title: 'Fraud screening',
        description: 'Lightweight monitoring is enough at the FE stage.',
        bullets: [
          'Flag many failed payments on the same email or IP later in BE.',
          'Watch repeated high-value bookings followed by refund requests.',
          'Keep a notes column for manual review outcomes.',
        ],
      },
    ],
  },
  settings: {
    eyebrow: 'System settings',
    title: 'Control default prices, gateway credentials, branding and operational templates',
    description:
      'This section centralizes business rules and branding settings that affect customer checkout, ticket delivery and admin behavior.',
    metrics: [
      { label: 'Default 2D price', value: vnd(85000), hint: 'Base weekday ticket' },
      { label: 'SMTP profile', value: 'ABCD Mailer', hint: 'Transactional sender' },
      { label: 'Brand version', value: 'Spring 2026', hint: 'Header + email assets' },
      { label: 'Payment configs', value: '4 active', hint: 'Ready for credential rotation' },
    ],
    table: {
      columns: ['Setting group', 'Current value', 'Last updated', 'Owner'],
      rows: [
        ['Ticket pricing', '2D 85k / 3D 120k / IMAX 140k', '2026-04-02', 'movie.ops'],
        ['SMTP sender', 'tickets@abcdcinema.vn', '2026-03-30', 'admin.mall'],
        ['Email template', 'Cinematic purple v3', '2026-04-01', 'support.cs'],
        ['Brand assets', 'Logo + footer pack', '2026-03-28', 'admin.mall'],
      ],
    },
    panels: [
      {
        title: 'Expected controls',
        description: 'Admin FE should expose the most important configuration groups.',
        bullets: [
          'Default ticket price and dynamic pricing toggles.',
          'Payment gateways and callback endpoints.',
          'SMTP credentials, sender name and email templates.',
        ],
      },
      {
        title: 'Brand governance',
        description: 'The cinema branding should remain consistent everywhere.',
        bullets: [
          'Preview logo, colors and ticket assets before publishing.',
          'Version email templates for rollback.',
          'Track who changed each setting.',
        ],
      },
    ],
  },
  promotions: {
    eyebrow: 'Promotions engine',
    title: 'Build discount campaigns, usage limits and customer incentive rules',
    description:
      'Promotions must map directly to the guest checkout journey, including payment-method offers, ticket discounts and combo campaigns.',
    metrics: [
      { label: 'Active campaigns', value: '4', hint: 'Visible in storefront' },
      { label: 'Scheduled promos', value: '2', hint: 'Queued for launch' },
      { label: 'Total redemptions', value: '3,836', hint: 'Across all active campaigns' },
      { label: 'Best performer', value: 'MOMO30', hint: 'Highest applied usage' },
    ],
    table: { columns: ['Code', 'Offer', 'Usage', 'Valid until', 'State'], rows: promotionRows },
    panels: [
      {
        title: 'Rule system',
        description: 'A robust admin FE needs condition-driven promotion controls.',
        bullets: [
          'Support percentage and flat amount discounts.',
          'Limit by usage count, date window and eligible payment methods.',
          'Show preview messaging for pending vs applied promotions.',
        ],
      },
      {
        title: 'Operational notes',
        description: 'These campaigns should align with the current guest flow.',
        bullets: [
          'Weekend and early-bird offers depend on showtime metadata.',
          'Birthday offers depend on checkout-entered birthday.',
          'Combo promotions depend on snack selections before payment.',
        ],
      },
    ],
  },
  logs: {
    eyebrow: 'Logs and monitoring',
    title: 'Review booking, payment and system logs to debug live incidents',
    description:
      'This page gives operators a cross-cutting view into failures so support and ops can understand what happened without reading backend consoles.',
    metrics: [
      { label: 'Errors last 24h', value: '14', hint: 'Across email and payment flows' },
      { label: 'Warnings', value: '23', hint: 'Mostly delayed callbacks' },
      { label: 'Recovered jobs', value: '11', hint: 'Auto retries succeeded' },
      { label: 'SLA risk', value: '2 incidents', hint: 'Watch delivery latency' },
    ],
    table: { columns: ['Service', 'Message', 'Level', 'Timestamp'], rows: logRows },
    panels: [
      {
        title: 'Why this matters',
        description: 'This is the first place support will check when users report issues.',
        bullets: [
          'Trace booking creation, payment callbacks and email delivery in one timeline.',
          'Correlate failures by booking code or payment id.',
          'Highlight unresolved incidents clearly in the UI.',
        ],
      },
      {
        title: 'Suggested later integrations',
        description: 'The FE can be upgraded with richer monitoring once BE is ready.',
        bullets: [
          'External error tracking like Sentry or Datadog.',
          'Realtime stream for callback failures and stuck jobs.',
          'Saved filters for support and finance teams.',
        ],
      },
    ],
  },
  users: {
    eyebrow: 'Admin users and permissions',
    title: 'Manage operator access, roles and responsibility boundaries',
    description:
      'If multiple staff members use the portal, the admin interface should expose clear permissions for movie ops, finance and support.',
    metrics: [
      { label: 'Admins', value: '4', hint: 'Including support and finance' },
      { label: 'Pending invites', value: '1', hint: 'Awaiting first login' },
      { label: 'Custom roles', value: '3', hint: 'Ops, support, finance' },
      { label: 'Last audit', value: '2026-04-05', hint: 'Role review completed' },
    ],
    table: { columns: ['User', 'Role', 'Permissions', 'State'], rows: userRows },
    panels: [
      {
        title: 'Role boundaries',
        description: 'Use permission scopes that align with your real operational teams.',
        bullets: [
          'Movie operators manage catalog and showtimes.',
          'Support handles bookings, emails and customer recovery.',
          'Finance reviews payments, refunds and reconciliation.',
        ],
      },
      {
        title: 'Security baseline',
        description: 'These controls are enough for the FE prototype.',
        bullets: [
          'Invite-only admin creation.',
          'Role labels on every user row.',
          'Audit-ready wording for permission assignments.',
        ],
      },
    ],
  },
};
