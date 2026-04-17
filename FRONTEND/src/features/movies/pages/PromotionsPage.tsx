import { useEffect, useMemo, useRef, useState, type ReactNode } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import {
  Ticket,
  ChevronRight,
  ArrowLeft,
  Clock,
  Zap,
  Mail,
  CreditCard,
  Sparkles,
  Tag,
  Film,
  Coffee,
  Star,
  Users,
  Calendar,
  ChevronDown,
  Check,
  ArrowRight,
  Flame,
  Shield,
  Gift,
  BadgePercent,
  Smartphone,
} from 'lucide-react';
import { Button } from '../component/ui/button';
import { moviePaths } from '../routes/moviePaths';
import { loadPromotionsUiData } from '../api/movieUiAdapter';
type FilterKey = 'all' | 'ticket' | 'combo' | 'member' | 'bank' | 'weekend';

interface Promo {
  id: string;
  title: string;
  desc: string;
  badge: string;
  badgeColor: string;
  expiry: string;
  condition: string;
  category: FilterKey[];
  img: string;
  accentFrom: string;
  accentTo: string;
  hot?: boolean;
}
const FEATURED: Promo[] = [
  {
    id: 'f1',
    title: 'Weekend special - Free popcorn combo',
    desc: 'Buy 2 tickets on Saturday or Sunday and receive 1 free large popcorn combo. Valid for selected weekend showtimes.',
    badge: 'Free combo',
    badgeColor: 'from-fuchsia-500 to-pink-600',
    expiry: '30/06/2026',
    condition: 'Valid on Sat-Sun, minimum 2 seats per order',
    category: ['weekend', 'combo'],
    img: 'https://images.unsplash.com/photo-1691480213129-106b2c7d1ee8?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxwb3Bjb3JuJTIwYnVja2V0JTIwY2luZW1hJTIwc25hY2tzJTIwdmlicmFudHxlbnwxfHx8fDE3NzUxMTMxMDl8MA&ixlib=rb-4.1.0&q=80&w=1080',
    accentFrom: '#a21caf',
    accentTo: '#db2777',
    hot: true,
  },
  {
    id: 'f2',
    title: 'Pay with MoMo - Get 30% Off',
    desc: 'Pay with the MoMo wallet for your ABCD Cinema booking and receive 30% off, up to 60,000 VND per transaction.',
    badge: '30% OFF',
    badgeColor: 'from-pink-500 to-rose-600',
    expiry: '15/05/2026',
    condition: 'Up to 60,000 VND, once per account per day',
    category: ['ticket', 'bank'],
    img: 'https://images.unsplash.com/photo-1623295080944-9ba74d587748?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxjcmVkaXQlMjBjYXJkJTIwcGF5bWVudCUyMGRpZ2l0YWwlMjB3YWxsZXQlMjBwdXJwbGV8ZW58MXx8fHwxNzc1MTEzMTA0fDA&ixlib=rb-4.1.0&q=80&w=1080',
    accentFrom: '#9333ea',
    accentTo: '#ec4899',
    hot: true,
  },
  {
    id: 'f3',
    title: 'Date Night - Buy 1 Get 1',
    desc: 'Choose a Couple Seat and receive 1 extra regular ticket for the same showtime. Perfect for weekend date nights.',
    badge: 'Buy 1 Get 1',
    badgeColor: 'from-amber-400 to-orange-500',
    expiry: '30/04/2026',
    condition: 'Valid for couple seats, not available in IMAX',
    category: ['ticket', 'weekend'],
    img: 'https://images.unsplash.com/photo-1665827488604-e224df2ddbf5?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxjb3VwbGUlMjB3YXRjaGluZyUyMG1vdmllJTIwZGF0ZSUyMG5pZ2h0fGVufDF8fHx8MTc3NTExMzEwNXww&ixlib=rb-4.1.0&q=80&w=1080',
    accentFrom: '#d97706',
    accentTo: '#ea580c',
  },
];
const ALL_PROMOS: Promo[] = [
  ...FEATURED,
  {
    id: 'p4',
    title: 'Vietcombank - 25% Off',
    desc: 'Vietcombank Visa and Mastercard holders receive 25% off the total order when booking at ABCD Cinema.',
    badge: '25% OFF',
    badgeColor: 'from-cyan-500 to-blue-600',
    expiry: '31/12/2026',
    condition: 'Up to 50,000 VND, valid Mon-Fri',
    category: ['bank', 'ticket'],
    img: 'https://images.unsplash.com/photo-1649251855096-f8beda8f6b24?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxiYW5rJTIwY2FyZCUyMGNhc2hiYWNrJTIwZ29sZCUyMHByZW1pdW18ZW58MXx8fHwxNzc1MTEzMTEzfDA&ixlib=rb-4.1.0&q=80&w=1080',
    accentFrom: '#0891b2',
    accentTo: '#2563eb',
  },
  {
    id: 'p5',
    title: 'Group booking - 20% off 5 tickets',
    desc: 'Book 5 tickets or more in one order and get 20% off the full booking. Great for movie nights with friends.',
    badge: '20% off groups',
    badgeColor: 'from-violet-500 to-purple-700',
    expiry: '30/06/2026',
    condition: 'Minimum 5 tickets, same showtime',
    category: ['ticket', 'member'],
    img: 'https://images.unsplash.com/photo-1681641092941-b1acee507ee0?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxmcmllbmRzJTIwbGF1Z2hpbmclMjB3ZWVrZW5kJTIwbmlnaHQlMjBvdXR8ZW58MXx8fHwxNzc1MTEzMTEzfDA&ixlib=rb-4.1.0&q=80&w=1080',
    accentFrom: '#7c3aed',
    accentTo: '#6d28d9',
  },
  {
    id: 'p6',
    title: 'Combo Gold - Popcorn + 2 Drinks + Snack',
    desc: 'Upgrade your movie night with Combo Gold: 1 XL buttery popcorn, 2 large drinks, and 1 snack for only 85,000 VND.',
    badge: '-40% combo',
    badgeColor: 'from-yellow-400 to-amber-600',
    expiry: '31/05/2026',
    condition: 'Add it during checkout, available every day',
    category: ['combo'],
    img: 'https://images.unsplash.com/photo-1563381013529-1c922c80ac8d?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxjaW5lbWElMjBwb3Bjb3JuJTIwdGlja2V0cyUyMG5lb24lMjBkYXJrfGVufDF8fHx8MTc3NTExMzA5OXww&ixlib=rb-4.1.0&q=80&w=1080',
    accentFrom: '#ca8a04',
    accentTo: '#d97706',
    hot: true,
  },
  {
    id: 'p7',
    title: 'ABCD Birthday Special - 1 Free Ticket',
    desc: 'During your birthday month, enter your birthday at checkout to receive 1 free ticket worth up to 120,000 VND.',
    badge: 'Free ticket',
    badgeColor: 'from-fuchsia-400 to-pink-500',
    expiry: 'Valid during your birthday month',
    condition: 'Enter your birthday at checkout, once per year',
    category: ['member'],
    img: 'https://images.unsplash.com/photo-1762028892696-1d6e9d8c294e?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxmaWxtJTIwcmVlbCUyMGNpbmVtYSUyMGdvbGRlbiUyMGxpZ2h0JTIwYm9rZWh8ZW58MXx8fHwxNzc1MTEzMTA0fDA&ixlib=rb-4.1.0&q=80&w=1080',
    accentFrom: '#c026d3',
    accentTo: '#db2777',
  },
  {
    id: 'p8',
    title: 'Early bird - 35% off from 9AM to 11AM',
    desc: 'All showtimes starting from 9:00 to before 11:00 receive 35% off ticket price. Perfect for guests who prefer early screenings.',
    badge: '35% early bird',
    badgeColor: 'from-sky-400 to-cyan-600',
    expiry: '31/12/2026',
    condition: 'Showtimes 09:00-10:59, every day',
    category: ['ticket'],
    img: 'https://images.unsplash.com/photo-1766425597359-08c8f7585ba4?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxtb3ZpZSUyMHRoZWF0ZXIlMjBzZWF0cyUyMGRhcmslMjBhdG1vc3BoZXJpY3xlbnwxfHx8fDE3NzUxMTMxMDB8MA&ixlib=rb-4.1.0&q=80&w=1080',
    accentFrom: '#0284c7',
    accentTo: '#0891b2',
  },
];
const FILTERS: { key: FilterKey; label: string; icon: ReactNode }[] = [
  { key: 'all', label: 'All', icon: <Sparkles className="size-3.5" /> },
  { key: 'ticket', label: 'Movie tickets', icon: <Ticket className="size-3.5" /> },
  { key: 'combo', label: 'Popcorn combos', icon: <Coffee className="size-3.5" /> },
  { key: 'member', label: 'Members', icon: <Star className="size-3.5" /> },
  { key: 'bank', label: 'Bank / Wallet', icon: <CreditCard className="size-3.5" /> },
  { key: 'weekend', label: 'Weekend', icon: <Calendar className="size-3.5" /> },
];
const PROMO_DETAIL_NOTES: Record<string, string[]> = {
  f1: [
    'Valid when the order includes at least 2 seats on Saturday or Sunday.',
    'The offer appears as soon as the selected showtime matches the conditions.',
    'At checkout, the system keeps the same promo and carries it into the payment summary.',
  ],
  f2: [
    'Seat selection only shows this offer as reserved or estimated.',
    'The 30% discount is confirmed only when MoMo is selected as the payment method.',
    'If you switch to another payment method, the total returns to normal and the discount row disappears.',
  ],
  f3: [
    'You need at least one eligible couple seat to activate the Date Night offer.',
    'If the showtime or hall type is not eligible, the summary explains why.',
    'Checkout verifies the offer again before deducting it from the total.',
  ],
  p4: [
    'Valid for Vietcombank card payments on weekdays.',
    'The summary keeps the selected offer, and checkout confirms it only with an eligible payment method.',
    'If you switch to another payment method, the promo becomes ineligible.',
  ],
  p5: [
    'Automatically activates when one order contains 5 seats or more.',
    'The seat selection summary updates as soon as the required seat count is reached.',
    'Checkout keeps the discount as long as the seat configuration does not change.',
  ],
  p6: [
    'This promo applies to Combo Gold and does not reduce the standard ticket price.',
    'The seat page shows it as reserved to remind the user.',
    'At checkout, the user must select Combo Gold for the promo to apply in full.',
  ],
  p7: [
    'This promotion is validated using the birthday entered at checkout.',
    'Seat selection still shows the chosen promo so users know it is being held.',
    'If the birthday is not eligible, the promo explains why it cannot be applied.',
  ],
  p8: [
    'Valid for showtimes starting between 09:00 and 10:59.',
    'The system can calculate the discount as early as the seat selection step.',
    'Checkout only needs to confirm it again, with no extra condition required.',
  ],
};
const FAQ_ITEMS = [
  {
    q: 'How do I use a promotion?',
    a: 'Choose your movie, showtime, and seats. Then enter your booking details and select the eligible payment method. The system will apply the offer automatically once all conditions are met.',
  },
  {
    q: 'Do I need an account to use promotions?',
    a: 'No. ABCD Cinema does not require account registration. Just enter your email during checkout and your e-ticket will be sent after successful payment.',
  },
  {
    q: 'Can I apply multiple promotions at once?',
    a: 'Each order can use only one promotion. The system keeps the offer that best matches your current booking conditions.',
  },
  {
    q: 'What if I do not receive my ticket email?',
    a: 'Please check your Spam or Junk folder. If the email still does not arrive after 15 minutes, contact 1900-ABCD or support@abcdcinema.vn for assistance.',
  },
  {
    q: 'Do promotions work for IMAX halls?',
    a: "Some offers do not apply to IMAX or 4DX halls. Please review each promotion's conditions before booking.",
  },
  {
    q: 'Can I refund a booking if I choose the wrong showtime?',
    a: 'Refunds are available within 30 minutes of a successful booking and at least 2 hours before showtime. Orders using promotions are handled according to the current refund policy.',
  },
];
function PromoCard({
  promo,
  featured = false,
  active = false,
  onDetail,
}: {
  promo: Promo;
  featured?: boolean;
  active?: boolean;
  onDetail?: (promo: Promo) => void;
}) {
  const navigate = useNavigate();
  return (
    <div
      className={`group relative flex flex-col overflow-hidden rounded-2xl border bg-[#0c0e1e] transition-all duration-300 hover:-translate-y-1 hover:border-white/[0.15] hover:shadow-2xl ${
        active
          ? 'border-fuchsia-400/60 shadow-2xl shadow-fuchsia-900/30 ring-1 ring-fuchsia-400/30'
          : 'border-white/[0.07]'
      }`}
      style={
        featured
          ? { boxShadow: `0 8px 40px -8px ${promo.accentFrom}55` }
          : undefined
      }
    >
      <div className={`relative overflow-hidden ${featured ? 'h-52' : 'h-40'}`}>
        <img
          src={promo.img}
          alt={promo.title}
          className="h-full w-full object-cover transition-transform duration-500 group-hover:scale-105"
        />
        <div
          className="absolute inset-0"
          style={{
            background: `linear-gradient(to bottom, ${promo.accentFrom}22 0%, #0c0e1e 100%)`,
          }}
        />
        <div className="absolute left-3 top-3">
          <span
            className="inline-flex items-center gap-1 rounded-full px-3 py-1 text-xs font-bold text-white shadow-lg"
            style={{ background: `linear-gradient(135deg, ${promo.accentFrom}, ${promo.accentTo})` }}
          >
            {promo.hot && <Flame className="size-3" />}
            {promo.badge}
          </span>
        </div>
        <div
          className="absolute bottom-0 left-0 right-0 h-px"
          style={{
            background: `linear-gradient(90deg, transparent, ${promo.accentFrom}99, ${promo.accentTo}99, transparent)`,
          }}
        />
      </div>

      <div className="flex flex-1 flex-col p-4 sm:p-5">
        <h3
          className={`mb-2 font-bold text-white leading-snug ${featured ? 'text-lg' : 'text-base'}`}
        >
          {promo.title}
        </h3>
        <p className="mb-4 flex-1 text-sm text-gray-400 leading-relaxed line-clamp-2">
          {promo.desc}
        </p>

        <div className="mb-4 space-y-1.5">
          <div className="flex items-center gap-2 text-xs text-gray-500">
            <Clock className="size-3.5 shrink-0 text-gray-600" />
            <span>Expires: <span className="text-gray-400">{promo.expiry}</span></span>
          </div>
          <div className="flex items-start gap-2 text-xs text-gray-500">
            <Tag className="size-3.5 shrink-0 mt-px text-gray-600" />
            <span className="text-gray-500">{promo.condition}</span>
          </div>
        </div>

        <div className="flex gap-2">
          <button
            onClick={() => onDetail?.(promo)}
            className="flex-1 rounded-xl border border-white/10 bg-white/[0.03] py-2.5 text-sm font-semibold text-white transition-all duration-200 hover:border-white/20 hover:bg-white/[0.06]"
          >
            Details
          </button>
          <button
            onClick={() => navigate(`${moviePaths.showtimes()}?promo=${promo.id}`)}
            className="group/btn flex flex-1 items-center justify-center gap-2 rounded-xl py-2.5 text-sm font-semibold text-white transition-all duration-200"
            style={{
              background: `linear-gradient(135deg, ${promo.accentFrom}cc, ${promo.accentTo}cc)`,
            }}
            onMouseEnter={(e) => {
              (e.currentTarget as HTMLButtonElement).style.background = `linear-gradient(135deg, ${promo.accentFrom}, ${promo.accentTo})`;
            }}
            onMouseLeave={(e) => {
              (e.currentTarget as HTMLButtonElement).style.background = `linear-gradient(135deg, ${promo.accentFrom}cc, ${promo.accentTo}cc)`;
            }}
          >
            Use now
            <ChevronRight className="size-4 transition-transform group-hover/btn:translate-x-0.5" />
          </button>
        </div>
      </div>
    </div>
  );
}

function getPrimaryFilter(promo: Promo): FilterKey {
  const priority: FilterKey[] = ['ticket', 'combo', 'member', 'bank', 'weekend'];
  return priority.find((key) => promo.category.includes(key)) ?? 'all';
}

function FAQItem({ q, a }: { q: string; a: string }) {
  const [open, setOpen] = useState(false);
  return (
    <div className="overflow-hidden rounded-xl border border-white/[0.06] bg-white/[0.02] transition-all duration-200 hover:border-white/[0.1]">
      <button
        onClick={() => setOpen((v) => !v)}
        className="flex w-full items-center justify-between gap-4 px-5 py-4 text-left"
      >
        <span className="text-sm font-semibold text-white leading-snug pr-2">{q}</span>
        <ChevronDown
          className={`size-4 shrink-0 text-gray-500 transition-transform duration-300 ${open ? 'rotate-180 text-purple-400' : ''}`}
        />
      </button>
      {open && (
        <div className="border-t border-white/[0.05] px-5 py-4">
          <p className="text-sm text-gray-400 leading-relaxed">{a}</p>
        </div>
      )}
    </div>
  );
}
export function PromotionsPage() {
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();
  const [activeFilter, setActiveFilter] = useState<FilterKey>('all');
  const [allPromos, setAllPromos] = useState<Promo[]>(ALL_PROMOS);
  const gridRef = useRef<HTMLDivElement>(null);
  const detailRef = useRef<HTMLDivElement>(null);
  const selectedPromoId = searchParams.get('promo');
  const selectedPromo = useMemo(
    () => allPromos.find((promo) => promo.id === selectedPromoId) ?? null,
    [allPromos, selectedPromoId]
  );
  const featuredPromos = allPromos.slice(0, 3);
  const effectiveFilter =
    selectedPromo && activeFilter !== 'all' && !selectedPromo.category.includes(activeFilter)
      ? getPrimaryFilter(selectedPromo)
      : activeFilter;

  const filteredPromos =
    effectiveFilter === 'all'
      ? allPromos
      : allPromos.filter((p) => p.category.includes(effectiveFilter));

  useEffect(() => {
    let active = true;

    async function loadPromotionsFromApi() {
      try {
        // API FETCH NOTE:
        // Promotions keep the original marketing page UI; this only replaces the promo list with API data.
        const data = await loadPromotionsUiData();
        if (active && data.length > 0) {
          setAllPromos(data);
        }
      } catch (error) {
        console.warn("Promotions API failed; using bundled fallback data.", error);
      }
    }

    void loadPromotionsFromApi();

    return () => {
      active = false;
    };
  }, []);

  useEffect(() => {
    if (!selectedPromo) return;

    const timeoutId = window.setTimeout(() => {
      detailRef.current?.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }, 80);

    return () => window.clearTimeout(timeoutId);
  }, [selectedPromo]);

  const scrollToGrid = () => {
    gridRef.current?.scrollIntoView({ behavior: 'smooth', block: 'start' });
  };

  const openPromoDetail = (promo: Promo) => {
    const next = new URLSearchParams(searchParams);
    next.set('promo', promo.id);
    setSearchParams(next);
    setActiveFilter(getPrimaryFilter(promo));
  };

  const clearPromoDetail = () => {
    const next = new URLSearchParams(searchParams);
    next.delete('promo');
    setSearchParams(next);
  };

  return (
    <div className="min-h-screen bg-[#060818] text-white overflow-x-hidden">
      <nav className="sticky top-0 z-50 border-b border-white/[0.05] bg-[#060818]/90 backdrop-blur-2xl">
        <div className="container mx-auto flex h-14 items-center justify-between px-4 sm:px-6">
          <button
            onClick={() => navigate(moviePaths.home())}
            className="flex items-center gap-2 group"
          >
            <div className="flex h-8 w-8 items-center justify-center rounded-full bg-gradient-to-br from-purple-600 to-pink-600 shadow-lg shadow-purple-900/30">
              <Film className="size-4 text-white" />
            </div>
            <span className="font-bold tracking-tight text-white">ABCD Cinema</span>
          </button>

          <div className="flex items-center gap-2">
            <span className="hidden text-xs text-gray-500 sm:block">Promotions page</span>
            <Button
              size="sm"
              onClick={() => navigate(moviePaths.showtimes())}
              className="bg-gradient-to-r from-purple-600 to-pink-600 hover:from-purple-500 hover:to-pink-500 shadow-md shadow-purple-900/30"
            >
              <Ticket className="mr-1.5 size-3.5" />
              Book tickets
            </Button>
          </div>
        </div>
      </nav>
      <section className="relative min-h-[88vh] flex items-center overflow-hidden">
        {/* Background cinematic image */}
        <div className="absolute inset-0">
          <img
            src="https://images.unsplash.com/photo-1766425597359-08c8f7585ba4?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Nzg4Nzd8MHwxfHNlYXJjaHwxfHxtb3ZpZSUyMHRoZWF0ZXIlMjBzZWF0cyUyMGRhcmslMjBhdG1vc3BoZXJpY3xlbnwxfHx8fDE3NzUxMTMxMDB8MA&ixlib=rb-4.1.0&q=80&w=1080"
            alt="Cinema"
            className="h-full w-full object-cover opacity-25"
          />
          {/* Deep gradient overlay */}
          <div className="absolute inset-0 bg-gradient-to-b from-[#060818]/60 via-[#060818]/40 to-[#060818]" />
          <div className="absolute inset-0 bg-gradient-to-r from-[#060818]/80 via-transparent to-[#060818]/60" />
        </div>

        {/* Ambient orbs */}
        <div className="pointer-events-none absolute left-1/4 top-1/3 h-72 w-72 -translate-x-1/2 -translate-y-1/2 rounded-full bg-purple-700/20 blur-[100px]" />
        <div className="pointer-events-none absolute right-1/4 top-1/2 h-64 w-64 rounded-full bg-fuchsia-600/15 blur-[80px]" />
        <div className="pointer-events-none absolute bottom-1/4 left-1/2 h-48 w-96 -translate-x-1/2 rounded-full bg-pink-600/10 blur-[80px]" />

        {/* Floating ticket decorations */}
        <div className="pointer-events-none absolute right-10 top-24 hidden rotate-12 opacity-10 lg:block">
          <Ticket className="size-24 text-purple-300" />
        </div>
        <div className="pointer-events-none absolute bottom-32 right-24 hidden -rotate-6 opacity-[0.07] lg:block">
          <Ticket className="size-16 text-pink-300" />
        </div>

        {/* Content */}
        <div className="container relative mx-auto px-4 py-24 sm:px-6 sm:py-32">
          <div className="max-w-3xl">
            {/* Eyebrow */}
            <div className="mb-6 inline-flex items-center gap-2 rounded-full border border-fuchsia-500/30 bg-fuchsia-950/40 px-4 py-2 backdrop-blur-sm">
              <BadgePercent className="size-4 text-fuchsia-400" />
              <span className="text-sm font-medium text-fuchsia-300">Exclusive offers &bull; Updated weekly</span>
            </div>

            {/* Main heading */}
            <h1 className="mb-5 text-5xl font-black leading-[1.05] tracking-tight text-white sm:text-6xl lg:text-7xl">
              Promotions{' '}
              <span
                className="inline-block"
                style={{
                  background: 'linear-gradient(135deg, #e879f9 0%, #f472b6 40%, #fb923c 100%)',
                  WebkitBackgroundClip: 'text',
                  WebkitTextFillColor: 'transparent',
                  backgroundClip: 'text',
                }}
              >
                for you
              </span>
            </h1>

            {/* Subheading */}
            <p className="mb-8 max-w-xl text-lg text-gray-300 leading-relaxed sm:text-xl">
              Discover weekly offers on tickets, combos, and special gifts -{' '}
              <span className="text-white font-medium">no sign-up required, no account needed</span>.
            </p>

            {/* Stats strip */}
            <div className="mb-10 flex flex-wrap gap-6">
              {[
                { val: '30%', label: 'Max savings', color: 'text-fuchsia-400' },
                { val: '8+', label: 'Active deals', color: 'text-cyan-400' },
                { val: '5 cinemas', label: 'Nationwide network', color: 'text-amber-400' },
              ].map((s) => (
                <div key={s.label}>
                  <p className={`text-3xl font-black ${s.color}`}>{s.val}</p>
                  <p className="text-xs text-gray-500">{s.label}</p>
                </div>
              ))}
            </div>

            {/* CTAs */}
            <div className="flex flex-wrap gap-3">
              <Button
                size="lg"
                onClick={() => navigate(moviePaths.showtimes())}
                className="h-12 bg-gradient-to-r from-purple-600 via-fuchsia-600 to-pink-600 px-7 text-base font-bold shadow-2xl shadow-purple-900/50 hover:from-purple-500 hover:via-fuchsia-500 hover:to-pink-500"
              >
                <Film className="mr-2 size-5" />
                View showtimes
                <ArrowRight className="ml-2 size-4" />
              </Button>
              <Button
                size="lg"
                variant="outline"
                onClick={scrollToGrid}
                className="h-12 border-white/15 px-7 text-base font-semibold text-white backdrop-blur-sm hover:bg-white/[0.08] hover:border-white/25"
              >
                View all offers
              </Button>
            </div>
          </div>
        </div>

        {/* Bottom fade */}
        <div className="absolute bottom-0 left-0 right-0 h-24 bg-gradient-to-t from-[#060818] to-transparent" />
      </section>
      <section className="relative py-20 sm:py-24">
        <div className="container mx-auto px-4 sm:px-6">
          {/* Section header */}
          <div className="mb-12 flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
            <div>
              <div className="mb-3 flex items-center gap-2">
                <div className="h-px w-8 bg-gradient-to-r from-fuchsia-500 to-pink-500" />
                <span className="text-xs font-bold uppercase tracking-widest text-fuchsia-400">
                  Featured this week
                </span>
              </div>
              <h2 className="text-3xl font-black text-white sm:text-4xl">
                Top{' '}
                <span className="bg-gradient-to-r from-fuchsia-400 to-pink-400 bg-clip-text text-transparent">
                  offers
                </span>
              </h2>
              <p className="mt-2 text-gray-500">Choose yours before it expires</p>
            </div>

            <div className="flex items-center gap-2 rounded-2xl border border-red-500/20 bg-red-950/20 px-4 py-3">
              <div className="flex h-2 w-2 animate-pulse rounded-full bg-red-500" />
              <span className="text-sm font-semibold text-red-400">Live now</span>
            </div>
          </div>

          {/* 3 featured cards */}
          <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
            {featuredPromos.map((promo) => (
              <PromoCard
                key={promo.id}
                promo={promo}
                featured
                active={selectedPromo?.id === promo.id}
                onDetail={openPromoDetail}
              />
            ))}
          </div>
        </div>
      </section>

      {selectedPromo && (
        <section ref={detailRef} className="py-4 sm:py-6 scroll-mt-20">
          <div className="container mx-auto px-4 sm:px-6">
            <div className="overflow-hidden rounded-[2rem] border border-fuchsia-400/20 bg-[linear-gradient(135deg,rgba(17,24,39,0.96),rgba(9,9,23,0.96))] shadow-[0_24px_80px_rgba(76,29,149,0.22)]">
              <div className="grid gap-0 lg:grid-cols-[1.05fr,0.95fr]">
                <div className="relative min-h-[300px] overflow-hidden">
                  <img
                    src={selectedPromo.img}
                    alt={selectedPromo.title}
                    className="absolute inset-0 h-full w-full object-cover"
                  />
                  <div className="absolute inset-0 bg-gradient-to-r from-[#060818] via-[#060818]/88 to-transparent" />
                  <div className="absolute inset-0 bg-gradient-to-t from-[#060818] via-transparent to-transparent" />
                  <div className="relative z-10 flex h-full flex-col justify-end p-6 sm:p-8">
                    <div className="mb-4 flex flex-wrap items-center gap-3">
                      <span
                        className="inline-flex items-center gap-1 rounded-full px-3 py-1 text-xs font-bold text-white shadow-lg"
                        style={{
                          background: `linear-gradient(135deg, ${selectedPromo.accentFrom}, ${selectedPromo.accentTo})`,
                        }}
                      >
                        {selectedPromo.hot && <Flame className="size-3" />}
                        {selectedPromo.badge}
                      </span>
                      <span className="rounded-full border border-white/10 bg-white/5 px-3 py-1 text-xs font-medium text-gray-300">
                        Expires: {selectedPromo.expiry}
                      </span>
                    </div>
                    <h2 className="max-w-2xl text-3xl font-black leading-tight text-white sm:text-4xl">
                      {selectedPromo.title}
                    </h2>
                    <p className="mt-3 max-w-2xl text-sm leading-7 text-gray-300 sm:text-base">
                      {selectedPromo.desc}
                    </p>
                  </div>
                </div>

                <div className="flex flex-col justify-between p-6 sm:p-8">
                  <div>
                    <div className="mb-5 inline-flex items-center gap-2 rounded-full border border-cyan-400/15 bg-cyan-400/10 px-3 py-1 text-xs font-semibold uppercase tracking-[0.2em] text-cyan-300">
                      <Sparkles className="size-3.5" />
                      Promotion detail
                    </div>

                    <div className="space-y-4">
                      <div className="rounded-2xl border border-white/8 bg-white/[0.03] p-4">
                        <p className="text-xs font-semibold uppercase tracking-[0.18em] text-gray-500">
                          Dieu kien ap dung
                        </p>
                        <p className="mt-2 text-sm leading-6 text-gray-200">
                          {selectedPromo.condition}
                        </p>
                      </div>

                      <div className="rounded-2xl border border-white/8 bg-white/[0.03] p-4">
                        <p className="text-xs font-semibold uppercase tracking-[0.18em] text-gray-500">
                          Booking flow logic
                        </p>
                        <div className="mt-3 space-y-3">
                          {PROMO_DETAIL_NOTES[selectedPromo.id]?.map((note) => (
                            <div key={note} className="flex items-start gap-3">
                              <div
                                className="mt-1 h-2 w-2 shrink-0 rounded-full"
                                style={{
                                  background: `linear-gradient(135deg, ${selectedPromo.accentFrom}, ${selectedPromo.accentTo})`,
                                }}
                              />
                              <p className="text-sm leading-6 text-gray-300">{note}</p>
                            </div>
                          ))}
                        </div>
                      </div>
                    </div>
                  </div>

                  <div className="mt-6 flex flex-wrap gap-3">
                    <Button
                      size="lg"
                      onClick={() => navigate(`${moviePaths.showtimes()}?promo=${selectedPromo.id}`)}
                      className="h-12 px-6 text-sm font-bold text-white shadow-xl"
                      style={{
                        background: `linear-gradient(135deg, ${selectedPromo.accentFrom}, ${selectedPromo.accentTo})`,
                      }}
                    >
                      Use now
                      <ChevronRight className="ml-2 size-4" />
                    </Button>
                    <Button
                      size="lg"
                      variant="outline"
                      onClick={clearPromoDetail}
                      className="h-12 border-white/12 px-6 text-sm font-semibold text-white hover:bg-white/[0.05]"
                    >
                      <ArrowLeft className="mr-2 size-4" />
                      Ve danh sach
                    </Button>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </section>
      )}
      <section className="py-14 sm:py-16">
        <div className="container mx-auto px-4 sm:px-6">
          <div className="relative overflow-hidden rounded-3xl border border-white/[0.07] p-8 sm:p-10">
            {/* Background */}
            <div className="absolute inset-0 bg-gradient-to-br from-[#0f0c2a] via-[#110d20] to-[#0a0d1f]" />
            <div className="pointer-events-none absolute -right-20 -top-20 h-64 w-64 rounded-full bg-purple-800/20 blur-3xl" />
            <div className="pointer-events-none absolute -bottom-16 left-1/3 h-48 w-48 rounded-full bg-fuchsia-700/15 blur-2xl" />

            <div className="relative">
              <div className="mb-8 text-center">
                <h2 className="text-2xl font-black text-white sm:text-3xl">
                  Why choose{' '}
                  <span className="bg-gradient-to-r from-purple-400 to-fuchsia-400 bg-clip-text text-transparent">
                    ABCD Cinema?
                  </span>
                </h2>
                <p className="mt-2 text-sm text-gray-500">
                  Book fast, receive tickets instantly, and skip account creation
                </p>
              </div>

              <div className="grid grid-cols-2 gap-6 sm:grid-cols-4">
                {[
                  {
                    icon: <Zap className="size-6" />,
                    title: 'Instant activation',
                    desc: 'Offers apply automatically when conditions are met',
                    color: 'text-yellow-400',
                    glow: 'bg-yellow-500/10',
                    ring: 'ring-yellow-500/20',
                  },
                  {
                    icon: <Shield className="size-6" />,
                    title: 'No account needed',
                    desc: 'Fast guest checkout',
                    color: 'text-emerald-400',
                    glow: 'bg-emerald-500/10',
                    ring: 'ring-emerald-500/20',
                  },
                  {
                    icon: <Mail className="size-6" />,
                    title: 'Tickets sent by email',
                    desc: 'Receive your e-ticket right after payment',
                    color: 'text-cyan-400',
                    glow: 'bg-cyan-500/10',
                    ring: 'ring-cyan-500/20',
                  },
                  {
                    icon: <Smartphone className="size-6" />,
                    title: 'Multiple payment methods',
                    desc: 'MoMo, VNPay, bank cards',
                    color: 'text-purple-400',
                    glow: 'bg-purple-500/10',
                    ring: 'ring-purple-500/20',
                  },
                ].map((item) => (
                  <div key={item.title} className="flex flex-col items-center gap-3 text-center">
                    <div
                      className={`flex h-14 w-14 items-center justify-center rounded-2xl ${item.glow} ${item.color} ring-1 ${item.ring}`}
                    >
                      {item.icon}
                    </div>
                    <div>
                      <p className="font-bold text-white text-sm sm:text-base">{item.title}</p>
                      <p className="mt-0.5 text-xs text-gray-500 leading-snug">{item.desc}</p>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          </div>
        </div>
      </section>
      <section ref={gridRef} className="py-16 sm:py-20 scroll-mt-16">
        <div className="container mx-auto px-4 sm:px-6">
          {/* Section header */}
          <div className="mb-10">
            <div className="mb-3 flex items-center gap-2">
              <div className="h-px w-8 bg-gradient-to-r from-cyan-500 to-purple-500" />
              <span className="text-xs font-bold uppercase tracking-widest text-cyan-400">
                All offers
              </span>
            </div>
            <h2 className="text-3xl font-black text-white sm:text-4xl">
              All{' '}
              <span className="bg-gradient-to-r from-cyan-400 to-purple-400 bg-clip-text text-transparent">
                promotions
              </span>
            </h2>
          </div>

          {/* Filter tabs */}
          <div className="mb-8 flex flex-wrap gap-2">
            {FILTERS.map((f) => {
              const active = effectiveFilter === f.key;
              return (
                <button
                  key={f.key}
                  onClick={() => setActiveFilter(f.key)}
                  className={`flex items-center gap-1.5 rounded-full border px-4 py-2 text-sm font-semibold transition-all duration-200 ${
                    active
                      ? 'border-purple-500/60 bg-purple-600/20 text-purple-300 shadow-md shadow-purple-900/30'
                      : 'border-white/[0.08] bg-white/[0.03] text-gray-400 hover:border-white/[0.16] hover:text-white'
                  }`}
                >
                  <span className={active ? 'text-purple-400' : 'text-gray-600'}>{f.icon}</span>
                  {f.label}
                  {active && (
                    <span className="flex h-4 w-4 items-center justify-center rounded-full bg-purple-500/30 text-[10px] font-bold text-purple-300">
                      {filteredPromos.length}
                    </span>
                  )}
                </button>
              );
            })}
          </div>

          {/* Grid */}
          {filteredPromos.length > 0 ? (
            <div className="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
              {filteredPromos.map((promo) => (
                <PromoCard
                  key={promo.id}
                  promo={promo}
                  active={selectedPromo?.id === promo.id}
                  onDetail={openPromoDetail}
                />
              ))}
            </div>
          ) : (
            <div className="flex flex-col items-center justify-center rounded-2xl border border-white/[0.06] bg-white/[0.02] py-20 text-center">
              <Gift className="mb-4 size-12 text-gray-700" />
              <p className="text-lg font-semibold text-gray-500">No offers in this category yet</p>
              <p className="mt-1 text-sm text-gray-600">Try browsing all offers</p>
              <button
                onClick={() => setActiveFilter('all')}
                className="mt-4 text-sm font-semibold text-purple-400 hover:text-purple-300"
              >
                View all
              </button>
            </div>
          )}
        </div>
      </section>
      <section className="py-16 sm:py-20">
        <div className="container mx-auto px-4 sm:px-6">
          <div className="mb-12 text-center">
            <div className="mb-3 inline-flex items-center gap-2">
              <div className="h-px w-8 bg-gradient-to-r from-amber-500 to-orange-500" />
              <span className="text-xs font-bold uppercase tracking-widest text-amber-400">
                Extremely simple
              </span>
              <div className="h-px w-8 bg-gradient-to-l from-amber-500 to-orange-500" />
            </div>
            <h2 className="text-3xl font-black text-white sm:text-4xl">
              Book tickets & unlock offers{' '}
              <span className="bg-gradient-to-r from-amber-400 to-orange-400 bg-clip-text text-transparent">
                in 3 steps
              </span>
            </h2>
          </div>

          <div className="relative grid grid-cols-1 gap-6 sm:grid-cols-3">
            {/* Connector line (desktop) */}
            <div
              className="pointer-events-none absolute top-10 left-[calc(16.67%+1rem)] right-[calc(16.67%+1rem)] hidden h-px sm:block"
              style={{
                background:
                  'linear-gradient(90deg, transparent, #a855f755, #ec489955, #a855f755, transparent)',
              }}
            />

            {[
              {
                step: '01',
                icon: <Film className="size-7" />,
                title: 'Choose movie & showtime',
                desc: 'Browse showtimes, pick your movie, and choose the session that fits your schedule.',
                color: 'from-purple-600 to-fuchsia-600',
                shadow: 'shadow-purple-900/40',
              },
              {
                step: '02',
                icon: <Ticket className="size-7" />,
                title: 'Choose seats & enter details',
                desc: 'Pick your preferred seats and enter your email to receive the ticket. No account required.',
                color: 'from-fuchsia-600 to-pink-600',
                shadow: 'shadow-fuchsia-900/40',
              },
              {
                step: '03',
                icon: <CreditCard className="size-7" />,
                title: 'Pay & receive tickets',
                desc: 'Choose your payment method, let the offer apply automatically, and receive your ticket by email instantly.',
                color: 'from-pink-600 to-rose-600',
                shadow: 'shadow-pink-900/40',
              },
            ].map((s) => (
              <div
                key={s.step}
                className="relative flex flex-col items-center rounded-2xl border border-white/[0.06] bg-gradient-to-b from-white/[0.04] to-transparent p-6 text-center sm:p-8"
              >
                {/* Step number */}
                <div className="mb-1 text-xs font-black tracking-widest text-gray-700">{s.step}</div>
                {/* Icon */}
                <div
                  className={`mb-5 flex h-16 w-16 items-center justify-center rounded-2xl bg-gradient-to-br ${s.color} shadow-xl ${s.shadow} text-white`}
                >
                  {s.icon}
                </div>
                <h3 className="mb-2 font-bold text-white">{s.title}</h3>
                <p className="text-sm text-gray-500 leading-relaxed">{s.desc}</p>
                {/* Check */}
                <div className="mt-5 flex h-6 w-6 items-center justify-center rounded-full bg-emerald-500/10 ring-1 ring-emerald-500/30">
                  <Check className="size-3.5 text-emerald-400" />
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>

      
      <section className="py-16 sm:py-20">
        <div className="container mx-auto px-4 sm:px-6">
          <div className="mx-auto max-w-3xl">
            {/* Section header */}
            <div className="mb-10 text-center">
              <div className="mb-3 flex items-center justify-center gap-2">
                <div className="h-px w-8 bg-gradient-to-r from-sky-500 to-cyan-500" />
                <span className="text-xs font-bold uppercase tracking-widest text-sky-400">
                  Frequently asked questions
                </span>
                <div className="h-px w-8 bg-gradient-to-l from-sky-500 to-cyan-500" />
              </div>
              <h2 className="text-3xl font-black text-white sm:text-4xl">
                How to use{' '}
                <span className="bg-gradient-to-r from-sky-400 to-cyan-400 bg-clip-text text-transparent">
                  offers
                </span>
              </h2>
              <p className="mt-3 text-gray-500 text-sm">
                Everything you need to know about promotions and booking at ABCD Cinema
              </p>
            </div>

            {/* Accordion */}
            <div className="space-y-3">
              {FAQ_ITEMS.map((item) => (
                <FAQItem key={item.q} q={item.q} a={item.a} />
              ))}
            </div>

            {/* Contact strip */}
            <div className="mt-8 flex flex-col items-center gap-3 rounded-2xl border border-white/[0.06] bg-white/[0.02] px-6 py-5 text-center sm:flex-row sm:text-left">
              <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-purple-600/20 ring-1 ring-purple-500/30">
                <Users className="size-5 text-purple-400" />
              </div>
              <div className="flex-1">
                <p className="text-sm font-semibold text-white">Still have questions?</p>
                <p className="text-xs text-gray-500">
                  Contact hotline{' '}
                  <span className="font-semibold text-purple-400">1900-ABCD</span> or email{' '}
                  <span className="font-semibold text-purple-400">support@abcdcinema.vn</span>
                </p>
              </div>
              <button className="shrink-0 rounded-xl bg-purple-600/20 px-4 py-2 text-sm font-semibold text-purple-300 ring-1 ring-purple-500/30 hover:bg-purple-600/30 transition-colors">
                Contact us
              </button>
            </div>
          </div>
        </div>
      </section>
      <section className="relative overflow-hidden py-24 sm:py-32">
        {/* Background */}
        <div className="absolute inset-0 bg-gradient-to-br from-[#0d0520] via-[#110828] to-[#070918]" />
        {/* Ambient glows */}
        <div className="pointer-events-none absolute left-1/4 top-0 h-72 w-72 rounded-full bg-purple-700/25 blur-[100px]" />
        <div className="pointer-events-none absolute right-1/4 bottom-0 h-64 w-64 rounded-full bg-fuchsia-600/20 blur-[80px]" />
        <div className="pointer-events-none absolute left-1/2 top-1/2 h-96 w-96 -translate-x-1/2 -translate-y-1/2 rounded-full bg-pink-600/10 blur-[120px]" />

        {/* Decorative ticket */}
        <div className="pointer-events-none absolute right-16 top-16 hidden opacity-[0.06] lg:block">
          <Ticket className="size-32 text-purple-300" strokeWidth={1} />
        </div>
        <div className="pointer-events-none absolute bottom-20 left-20 hidden opacity-[0.04] lg:block">
          <Film className="size-24 text-pink-300" strokeWidth={1} />
        </div>

        <div className="container relative mx-auto px-4 text-center sm:px-6">
          {/* Badge */}
          <div className="mb-6 inline-flex items-center gap-2 rounded-full border border-purple-500/30 bg-purple-950/40 px-5 py-2.5 backdrop-blur-sm">
            <Sparkles className="size-4 text-purple-400" />
            <span className="text-sm font-semibold text-purple-300">
              Great movies - Better deals - Fast booking
            </span>
          </div>

          {/* Headline */}
          <h2 className="mb-4 text-4xl font-black leading-[1.05] text-white sm:text-5xl lg:text-6xl">
            Pick a great movie,{' '}
            <span
              style={{
                background: 'linear-gradient(135deg, #e879f9 0%, #f472b6 45%, #fb923c 100%)',
                WebkitBackgroundClip: 'text',
                WebkitTextFillColor: 'transparent',
                backgroundClip: 'text',
              }}
            >
              grab the best offer,
            </span>
            <br className="hidden sm:block" />
            {' '}and book in minutes.
          </h2>

          <p className="mx-auto mb-10 max-w-xl text-lg text-gray-400 leading-relaxed">
            Thousands of moviegoers book through ABCD Cinema every week. No friction,
            no account, no waiting.
          </p>

          {/* CTA buttons */}
          <div className="flex flex-col items-center justify-center gap-3 sm:flex-row">
            <Button
              size="lg"
              onClick={() => navigate(moviePaths.showtimes())}
              className="h-14 min-w-[220px] bg-gradient-to-r from-purple-600 via-fuchsia-600 to-pink-600 px-10 text-base font-bold shadow-2xl shadow-purple-900/60 hover:from-purple-500 hover:via-fuchsia-500 hover:to-pink-500 transition-all duration-300 hover:shadow-purple-800/60"
            >
              <Ticket className="mr-2 size-5" />
              Book now
              <ArrowRight className="ml-2 size-5" />
            </Button>
            <Button
              size="lg"
              variant="outline"
              onClick={scrollToGrid}
              className="h-14 border-white/[0.12] px-8 text-base text-white hover:bg-white/[0.06] hover:border-white/25"
            >
              See more offers
            </Button>
          </div>

          {/* Trust signals */}
          <div className="mt-10 flex flex-wrap items-center justify-center gap-x-8 gap-y-3">
            {[
              { icon: <Shield className="size-3.5" />, text: '256-bit SSL security' },
              { icon: <Mail className="size-3.5" />, text: 'Instant e-ticket delivery' },
              { icon: <Check className="size-3.5" />, text: 'No account needed' },
              { icon: <Zap className="size-3.5" />, text: 'Book in 3 minutes' },
            ].map((t) => (
              <div key={t.text} className="flex items-center gap-1.5 text-xs text-gray-500">
                <span className="text-emerald-500">{t.icon}</span>
                {t.text}
              </div>
            ))}
          </div>
        </div>
      </section>

     
      <footer className="border-t border-white/[0.05] bg-[#060818] py-8">
        <div className="container mx-auto px-4 sm:px-6">
          <div className="flex flex-col items-center gap-4 sm:flex-row sm:justify-between">
            <div className="flex items-center gap-2">
              <div className="flex h-7 w-7 items-center justify-center rounded-full bg-gradient-to-br from-purple-600 to-pink-600">
                <Film className="size-3.5 text-white" />
              </div>
              <span className="font-bold text-white">ABCD Cinema</span>
              <span className="text-gray-600">&bull;</span>
              <span className="text-xs text-gray-600">ABCD Mall</span>
            </div>
            <p className="text-xs text-gray-600">
              &copy; 2026 ABCD Cinema. Online ticket platform &bull; No login required.
            </p>
          </div>
        </div>
      </footer>
    </div>
  );
}






