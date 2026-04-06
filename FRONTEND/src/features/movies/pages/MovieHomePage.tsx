import { useEffect, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import {
  Film,
  Popcorn,
  Clock,
  CalendarDays,
  TrendingUp,
  Sparkles,
  ChevronRight,
  ChevronLeft,
  ShieldUser,
} from "lucide-react";
import { Button } from "../component/ui/button";
import { Badge } from "../component/ui/badge";
import { Tabs, TabsList, TabsTrigger } from "../component/ui/tabs";
import { MovieCard } from "./MovieCard";
import { PromoCard } from "./PromoCard";
import { nowShowingMovies, comingSoonMovies } from "../data/movie";
import { getDefaultBookingDate } from "../data/promotions";
import { moviePaths } from "../routes/moviePaths";

const promos = [
  {
    id: "f1",
    title: "Weekend special",
    description: "Extra savings for Saturday and Sunday showtimes",
    discount: "30% OFF",
    color: "bg-gradient-to-br from-purple-600 to-purple-800",
    imageUrl:
      "https://unsplash.com/photos/DUPLnKwg69w/download?force=true&w=1200",
  },
  {
    id: "p6",
    title: "Popcorn combo",
    description: "Save 50,000 VND on your snack combo",
    discount: "50K",
    color: "bg-gradient-to-br from-pink-600 to-pink-800",
    imageUrl:
      "https://unsplash.com/photos/wzAf9AAtVSI/download?force=true&w=1200",
  },
  {
    id: "p7",
    title: "Birthday gift",
    description: "Birthday reward unlocked during checkout",
    discount: "FREE",
    color: "bg-gradient-to-br from-cyan-600 to-cyan-800",
    imageUrl:
      "https://unsplash.com/photos/PxM8aeJbzvk/download?force=true&w=1200",
  },
];

export function MovieHomePage() {
  const [selectedCategory, setSelectedCategory] = useState("all");
  const [heroIndex, setHeroIndex] = useState(0);
  const [nowShowingIndex, setNowShowingIndex] = useState(
    nowShowingMovies.length,
  );
  const [comingSoonIndex, setComingSoonIndex] = useState(
    comingSoonMovies.length,
  );
  const [isNowShowingTransitionEnabled, setIsNowShowingTransitionEnabled] =
    useState(true);
  const [isComingSoonTransitionEnabled, setIsComingSoonTransitionEnabled] =
    useState(true);
  const navigate = useNavigate();
  const location = useLocation();
  const heroMovies = nowShowingMovies.slice(0, 3);
  const defaultBookingDate = getDefaultBookingDate();

  const nowShowingTrack = [
    ...nowShowingMovies,
    ...nowShowingMovies,
    ...nowShowingMovies,
  ];
  const comingSoonTrack = [
    ...comingSoonMovies,
    ...comingSoonMovies,
    ...comingSoonMovies,
  ];

  useEffect(() => {
    const intervalId = window.setInterval(() => {
      setHeroIndex((current) => (current + 1) % heroMovies.length);
    }, 5200);

    return () => window.clearInterval(intervalId);
  }, [heroMovies.length]);

  useEffect(() => {
    const intervalId = window.setInterval(() => {
      setNowShowingIndex((current) => current + 1);
    }, 7600);

    return () => window.clearInterval(intervalId);
  }, []);

  useEffect(() => {
    const intervalId = window.setInterval(() => {
      setComingSoonIndex((current) => current + 1);
    }, 8200);

    return () => window.clearInterval(intervalId);
  }, []);

  useEffect(() => {
    if (!isNowShowingTransitionEnabled) {
      const frameId = window.requestAnimationFrame(() =>
        setIsNowShowingTransitionEnabled(true),
      );
      return () => window.cancelAnimationFrame(frameId);
    }
  }, [isNowShowingTransitionEnabled]);

  useEffect(() => {
    if (!isComingSoonTransitionEnabled) {
      const frameId = window.requestAnimationFrame(() =>
        setIsComingSoonTransitionEnabled(true),
      );
      return () => window.cancelAnimationFrame(frameId);
    }
  }, [isComingSoonTransitionEnabled]);

  const handlePrevNowShowing = () => {
    setNowShowingIndex((current) => current - 1);
  };

  const handleNextNowShowing = () => {
    setNowShowingIndex((current) => current + 1);
  };

  const handlePrevComingSoon = () => {
    setComingSoonIndex((current) => current - 1);
  };

  const handleNextComingSoon = () => {
    setComingSoonIndex((current) => current + 1);
  };

  const handlePrevHero = () => {
    setHeroIndex(
      (current) => (current - 1 + heroMovies.length) % heroMovies.length,
    );
  };

  const handleNextHero = () => {
    setHeroIndex((current) => (current + 1) % heroMovies.length);
  };

  const handleOpenShowtimes = (movieId?: string) => {
    const params = new URLSearchParams(location.search);
    params.set("date", params.get("date") ?? defaultBookingDate);

    if (movieId) {
      params.set("movie", movieId);
    } else {
      params.delete("movie");
    }

    navigate(`${moviePaths.showtimes()}?${params.toString()}`);
  };

  const handleNowShowingTransitionEnd = () => {
    if (nowShowingIndex >= nowShowingMovies.length * 2) {
      setIsNowShowingTransitionEnabled(false);
      setNowShowingIndex(nowShowingIndex - nowShowingMovies.length);
    } else if (nowShowingIndex < nowShowingMovies.length) {
      setIsNowShowingTransitionEnabled(false);
      setNowShowingIndex(nowShowingIndex + nowShowingMovies.length);
    }
  };

  const handleComingSoonTransitionEnd = () => {
    if (comingSoonIndex >= comingSoonMovies.length * 2) {
      setIsComingSoonTransitionEnabled(false);
      setComingSoonIndex(comingSoonIndex - comingSoonMovies.length);
    } else if (comingSoonIndex < comingSoonMovies.length) {
      setIsComingSoonTransitionEnabled(false);
      setComingSoonIndex(comingSoonIndex + comingSoonMovies.length);
    }
  };

  return (
    <div className="relative min-h-screen overflow-hidden bg-[radial-gradient(circle_at_top,_rgba(139,92,246,0.2),_transparent_30%),radial-gradient(circle_at_80%_20%,_rgba(6,182,212,0.16),_transparent_24%),linear-gradient(to_bottom,_#030712,_#374151,_#111827)] pt-[76px]">
      <style>{`
        @keyframes cinema-float {
          0%, 100% { transform: translate3d(0, 0, 0) scale(1); }
          50% { transform: translate3d(0, -18px, 0) scale(1.04); }
        }
        @keyframes cinema-drift {
          0% { transform: translate3d(-2%, 0, 0) scale(1.02); }
          100% { transform: translate3d(2%, -3%, 0) scale(1.1); }
        }
        @keyframes cinema-reveal {
          0% { opacity: 0; transform: translateY(24px); }
          100% { opacity: 1; transform: translateY(0); }
        }
        @keyframes cinema-pulse {
          0%, 100% { box-shadow: 0 0 0 rgba(217, 70, 239, 0.18); }
          50% { box-shadow: 0 0 36px rgba(217, 70, 239, 0.36); }
        }
        @keyframes cinema-beam {
          0%, 100% { transform: translateX(-6%) rotate(-8deg); opacity: 0.2; }
          50% { transform: translateX(6%) rotate(-12deg); opacity: 0.42; }
        }
        @keyframes cinema-title-glow {
          0%, 100% { text-shadow: 0 0 24px rgba(192,38,211,0.2), 0 0 42px rgba(59,130,246,0.12); }
          50% { text-shadow: 0 0 36px rgba(192,38,211,0.34), 0 0 60px rgba(59,130,246,0.2); }
        }
        @keyframes cinema-marquee-glow {
          0%, 100% { text-shadow: 0 0 18px rgba(244,114,182,0.16), 0 0 32px rgba(34,211,238,0.12); }
          50% { text-shadow: 0 0 24px rgba(244,114,182,0.3), 0 0 48px rgba(34,211,238,0.2); }
        }
        @keyframes cinema-gradient-shift {
          0% { background-position: 0% 50%; filter: saturate(1.02) brightness(1); }
          50% { background-position: 100% 50%; filter: saturate(1.22) brightness(1.12); }
          100% { background-position: 0% 50%; filter: saturate(1.02) brightness(1); }
        }
        @keyframes cinema-section-glow {
          0%, 100% { text-shadow: 0 0 14px rgba(168,85,247,0.12), 0 0 28px rgba(34,211,238,0.08); }
          50% { text-shadow: 0 0 24px rgba(168,85,247,0.24), 0 0 40px rgba(34,211,238,0.16); }
        }
        @keyframes cinema-carousel-breathe {
          0%, 100% { filter: saturate(1) brightness(1); }
          50% { filter: saturate(1.04) brightness(1.02); }
        }
        @keyframes cinema-orb-drift {
          0%, 100% { transform: translate3d(0, 0, 0) scale(1); }
          50% { transform: translate3d(12px, -16px, 0) scale(1.06); }
        }
        @keyframes cinema-grain {
          0%, 100% { transform: translate(0, 0); }
          25% { transform: translate(-1%, 1%); }
          50% { transform: translate(1%, -1%); }
          75% { transform: translate(1%, 1%); }
        }
      `}</style>
      <div className="pointer-events-none absolute inset-0">
        <div
          className="absolute left-[-8rem] top-12 h-72 w-72 rounded-full bg-fuchsia-500/18 blur-3xl"
          style={{ animation: "cinema-float 11s ease-in-out infinite" }}
        />
        <div
          className="absolute right-[-6rem] top-40 h-80 w-80 rounded-full bg-cyan-400/14 blur-3xl"
          style={{
            animation: "cinema-float 14s ease-in-out infinite",
            animationDelay: "1.1s",
          }}
        />
        <div
          className="absolute bottom-40 left-1/3 h-64 w-64 rounded-full bg-violet-500/12 blur-3xl"
          style={{
            animation: "cinema-float 16s ease-in-out infinite",
            animationDelay: "1.8s",
          }}
        />
        <div
          className="absolute inset-0 opacity-[0.08] mix-blend-soft-light"
          style={{
            animation: "cinema-grain 8s steps(6) infinite",
            backgroundImage:
              "radial-gradient(circle at 20% 20%, rgba(255,255,255,0.8) 0 0.8px, transparent 1px), radial-gradient(circle at 80% 30%, rgba(255,255,255,0.55) 0 0.7px, transparent 1px), radial-gradient(circle at 40% 70%, rgba(255,255,255,0.45) 0 0.7px, transparent 1px)",
            backgroundSize: "180px 180px, 220px 220px, 260px 260px",
          }}
        />
      </div>
      {/* Header */}
      <header className="fixed inset-x-0 top-0 z-30 border-b border-white/10 bg-[#040816]/35 shadow-[0_10px_40px_rgba(3,7,18,0.28)] backdrop-blur-md">
        <div className="mx-auto flex max-w-7xl items-center justify-between px-4 py-3 sm:px-6 lg:px-8">
          <div className="flex items-center gap-3">
            <div className="flex size-11 items-center justify-center rounded-full bg-gradient-to-br from-violet-700 via-fuchsia-600 to-pink-500 shadow-lg shadow-fuchsia-950/35">
              <Film className="size-5 text-white" />
            </div>

            <div>
              <h1
                className="bg-[linear-gradient(90deg,#fff7ed_0%,#f472b6_18%,#ffffff_38%,#22d3ee_62%,#c084fc_82%,#fff7ed_100%)] bg-[length:260%_260%] bg-clip-text text-xl font-black uppercase leading-none tracking-[0.22em] text-transparent drop-shadow-[0_0_24px_rgba(244,114,182,0.28)] sm:text-[1.65rem]"
                style={{
                  animation:
                    "cinema-marquee-glow 4.6s ease-in-out infinite, cinema-gradient-shift 5.2s ease-in-out infinite",
                }}
              >
                ABCD Cinema
              </h1>
              <p className="mt-1 text-xs text-gray-400 sm:text-sm">
                Online movie booking made fast and easy
              </p>
            </div>
          </div>

          <nav className="flex items-center gap-2 sm:gap-4">
            <Button
              variant="ghost"
              onClick={() => handleOpenShowtimes()}
              className="hidden h-10 px-3 text-base font-semibold text-gray-200 hover:bg-white/20 hover:text-white md:inline-flex"
            >
              <Clock className="mr-2 size-4" />
              Showtimes
            </Button>
            <Button
              variant="ghost"
              className="hidden h-10 px-3 text-base font-semibold text-gray-200 hover:bg-white/20 hover:text-white md:inline-flex"
            >
              <Popcorn className="mr-2 size-4" />
              Snacks
            </Button>
            <Button
              variant="ghost"
              onClick={() => navigate(moviePaths.admin())}
              className="hidden h-10 px-3 text-base font-semibold text-gray-200 hover:bg-white/20 hover:text-white md:inline-flex"
            >
              <ShieldUser className="mr-2 size-4" />
              Admin
            </Button>
          </nav>
        </div>
      </header>

      {/* Hero Section */}
      <section className="relative min-h-[30rem] overflow-hidden md:min-h-[34rem]">
        {heroMovies.map((movie, index) => (
          <img
            key={movie.id}
            src={movie.imageUrl}
            alt={movie.title}
            className={`absolute inset-0 h-72 w-full object-cover object-center transition-all duration-[1400ms] ease-out sm:h-80 md:h-[34rem] ${
              index === heroIndex
                ? "scale-105 opacity-55"
                : "scale-110 opacity-0"
            }`}
          />
        ))}
        <div className="absolute inset-0 bg-[linear-gradient(90deg,rgba(2,6,23,0.9)_0%,rgba(2,6,23,0.72)_38%,rgba(15,23,42,0.42)_62%,rgba(2,6,23,0.82)_100%)]" />
        <div className="absolute inset-0 bg-[radial-gradient(circle_at_20%_20%,rgba(244,114,182,0.18),transparent_24%),radial-gradient(circle_at_80%_28%,rgba(34,211,238,0.16),transparent_24%),radial-gradient(circle_at_50%_80%,rgba(192,38,211,0.14),transparent_32%)]" />
        <div
          className="absolute left-[8%] top-[-10%] h-[140%] w-40 bg-gradient-to-b from-fuchsia-300/0 via-fuchsia-300/18 to-fuchsia-300/0 blur-3xl"
          style={{ animation: "cinema-beam 9s ease-in-out infinite" }}
        />
        <div
          className="absolute right-[12%] top-[-8%] h-[135%] w-32 bg-gradient-to-b from-cyan-200/0 via-cyan-200/16 to-cyan-200/0 blur-3xl"
          style={{
            animation: "cinema-beam 11s ease-in-out infinite",
            animationDelay: "1.1s",
          }}
        />
        <div
          className="absolute -left-12 top-10 h-40 w-40 rounded-full bg-fuchsia-400/20 blur-3xl"
          style={{ animation: "cinema-float 12s ease-in-out infinite" }}
        />
        <div
          className="absolute bottom-0 right-0 h-48 w-48 rounded-full bg-cyan-400/18 blur-3xl"
          style={{
            animation: "cinema-float 10s ease-in-out infinite",
            animationDelay: "0.8s",
          }}
        />
        <div
          className="absolute left-1/2 top-1/2 h-72 w-72 -translate-x-1/2 -translate-y-1/2 rounded-full bg-fuchsia-500/10 blur-3xl"
          style={{
            animation: "cinema-float 12s ease-in-out infinite",
            animationDelay: "0.4s",
          }}
        />
        <div className="absolute inset-0 bg-[linear-gradient(180deg,rgba(2,6,23,0.02)_0%,rgba(2,6,23,0.26)_65%,rgba(2,6,23,0.72)_100%)]" />
        <div className="absolute inset-x-0 bottom-0 h-32 bg-[radial-gradient(circle_at_center,_rgba(244,114,182,0.18),_transparent_65%)] blur-2xl" />
        <div className="absolute inset-0 flex items-center">
          <div className="container mx-auto px-4">
            <div
              className="hidden max-w-2xl space-y-4 sm:space-y-6"
              style={{ animation: "cinema-reveal 750ms ease-out both" }}
            >
              <Badge className="border border-yellow-300/50 bg-gradient-to-r from-yellow-500 to-orange-500 text-black shadow-[0_0_24px_rgba(249,115,22,0.28)]">
                <Sparkles className="mr-1 size-3" />
                Premium cinema experience
              </Badge>
              <div className="inline-flex items-center gap-3">
                <span className="h-px w-10 bg-gradient-to-r from-transparent to-fuchsia-300/80" />
                <span className="text-[10px] font-semibold uppercase tracking-[0.45em] text-fuchsia-100/70 sm:text-xs">
                  Cinema Experience
                </span>
              </div>
              <h2
                className="bg-[linear-gradient(90deg,#fff7ed_0%,#f472b6_16%,#ffffff_36%,#22d3ee_60%,#c084fc_82%,#fff7ed_100%)] bg-[length:260%_260%] bg-clip-text text-2xl font-black uppercase tracking-[0.08em] text-transparent drop-shadow-[0_0_24px_rgba(34,211,238,0.2)] sm:text-3xl md:text-4xl"
                style={{
                  animation:
                    "cinema-title-glow 4.2s ease-in-out infinite, cinema-gradient-shift 5s ease-in-out infinite",
                }}
              >
                Book movie tickets
                <br />
                <span className="inline-block bg-gradient-to-r from-fuchsia-300 via-violet-100 to-cyan-300 bg-clip-text text-transparent drop-shadow-[0_0_30px_rgba(192,38,211,0.28)]">
                  Made effortless
                </span>
              </h2>
              <p className="max-w-xl text-base text-gray-300/95 drop-shadow-[0_6px_24px_rgba(0,0,0,0.35)] sm:text-lg">
                ABCD Mall's booking system lets you choose your movie, showtime,
                and seats in just a few seconds
              </p>
              <div className="flex flex-wrap gap-3 sm:gap-4">
                <Button
                  size="lg"
                  onClick={() => handleOpenShowtimes()}
                  className="rounded-xl bg-gradient-to-r from-purple-600 via-fuchsia-500 to-pink-500 shadow-[0_0_30px_rgba(192,38,211,0.3)] hover:from-purple-700 hover:via-fuchsia-600 hover:to-pink-600"
                  style={{ animation: "cinema-pulse 3s ease-in-out infinite" }}
                >
                  Explore now
                  <ChevronRight className="ml-2 size-4" />
                </Button>
                <Button
                  size="lg"
                  variant="outline"
                  onClick={() => handleOpenShowtimes()}
                  className="rounded-xl border-white/20 bg-white/5 text-white backdrop-blur-sm hover:border-white/30 hover:bg-white/10"
                >
                  View showtimes
                </Button>
              </div>
            </div>
            <div
              className="max-w-2xl space-y-4 sm:space-y-6"
              style={{ animation: "cinema-reveal 750ms ease-out both" }}
            >
              <Badge className="border border-yellow-300/50 bg-gradient-to-r from-yellow-500 to-orange-500 text-black shadow-[0_0_24px_rgba(249,115,22,0.28)]">
                <Sparkles className="mr-1 size-3" />
                Featured this week
              </Badge>
              <div className="inline-flex items-center gap-3">
                <span className="h-px w-10 bg-gradient-to-r from-transparent to-fuchsia-300/80" />
                <span className="text-[10px] font-semibold uppercase tracking-[0.45em] text-fuchsia-100/70 sm:text-xs">
                  Featured Premiere
                </span>
              </div>
              <h2
                className="bg-[linear-gradient(90deg,#fff7ed_0%,#f472b6_16%,#ffffff_36%,#22d3ee_60%,#c084fc_82%,#fff7ed_100%)] bg-[length:260%_260%] bg-clip-text text-3xl font-black uppercase tracking-[0.08em] text-transparent drop-shadow-[0_0_24px_rgba(34,211,238,0.2)] sm:text-4xl md:text-5xl"
                style={{
                  animation:
                    "cinema-title-glow 4.2s ease-in-out infinite, cinema-gradient-shift 5s ease-in-out infinite",
                }}
              >
                {heroMovies[heroIndex].title}
              </h2>
              <div className="flex flex-wrap items-center gap-2 text-sm text-gray-200/90">
                <Badge className="border border-white/10 bg-white/10 text-white">
                  {heroMovies[heroIndex].genre}
                </Badge>
                <Badge className="border border-fuchsia-400/30 bg-fuchsia-500/10 text-fuchsia-100">
                  {heroMovies[heroIndex].duration}
                </Badge>
                <Badge className="border border-cyan-400/30 bg-cyan-500/10 text-cyan-100">
                  {heroMovies[heroIndex].ageRating}
                </Badge>
              </div>
              <p className="max-w-xl text-base text-gray-300/95 drop-shadow-[0_6px_24px_rgba(0,0,0,0.35)] sm:text-lg">
                {heroMovies[heroIndex].description}
              </p>
              <div className="flex flex-wrap items-center gap-3 text-sm text-gray-300/90">
                <span className="rounded-full border border-white/10 bg-white/8 px-3 py-1.5">
                  Director: {heroMovies[heroIndex].director}
                </span>
                <span className="rounded-full border border-white/10 bg-white/8 px-3 py-1.5">
                  Rating: {heroMovies[heroIndex].rating}/10
                </span>
              </div>
              <div className="flex flex-wrap items-center gap-3 sm:gap-4">
                <Button
                  size="lg"
                  variant="outline"
                  onClick={() => handleOpenShowtimes(heroMovies[heroIndex].id)}
                  className="group rounded-2xl border border-cyan-300/35 bg-[linear-gradient(135deg,rgba(34,211,238,0.22),rgba(168,85,247,0.14))] px-6 text-white shadow-[0_0_34px_rgba(34,211,238,0.16)] backdrop-blur-md hover:border-cyan-200/60 hover:bg-[linear-gradient(135deg,rgba(34,211,238,0.32),rgba(236,72,153,0.18))]"
                >
                  <CalendarDays className="mr-2 size-4 text-cyan-100 transition-transform duration-300 group-hover:-rotate-6" />
                  View showtimes
                  <ChevronRight className="ml-2 size-4 transition-transform duration-300 group-hover:translate-x-1" />
                </Button>
              </div>
            </div>
          </div>
        </div>
        <div className="absolute inset-x-0 bottom-4 z-10 sm:bottom-5 md:bottom-6">
          <Button
            type="button"
            variant="ghost"
            size="icon"
            className="absolute left-1 top-1/2 size-11 -translate-y-1/2 rounded-full bg-transparent text-white/45 hover:bg-white/8 hover:text-white/75 sm:left-2 sm:size-12 md:left-3 md:size-14"
            onClick={handlePrevHero}
          >
            <ChevronLeft className="size-6 sm:size-7 md:size-8" />
          </Button>
          <div className="flex items-center justify-center gap-2">
            {heroMovies.map((movie, index) => (
              <button
                key={movie.id}
                type="button"
                onClick={() => setHeroIndex(index)}
                aria-label={`Chuyen den banner ${index + 1}`}
                className={`h-2 rounded-full transition-all duration-300 ${
                  index === heroIndex
                    ? "w-9 bg-white/70"
                    : "w-2 bg-white/22 hover:bg-white/40"
                }`}
              />
            ))}
          </div>
          <Button
            type="button"
            variant="ghost"
            size="icon"
            className="absolute right-1 top-1/2 size-11 -translate-y-1/2 rounded-full bg-transparent text-white/45 hover:bg-white/8 hover:text-white/75 sm:right-2 sm:size-12 md:right-3 md:size-14"
            onClick={handleNextHero}
          >
            <ChevronRight className="size-6 sm:size-7 md:size-8" />
          </Button>
        </div>
      </section>

      {/* Promotions Section */}
      <section className="relative py-10 sm:py-12">
        <div className="pointer-events-none absolute inset-0">
          <div className="absolute inset-x-0 top-6 h-px bg-gradient-to-r from-transparent via-yellow-300/25 to-transparent" />
          <div
            className="absolute -left-16 top-8 h-52 w-52 rounded-full bg-yellow-400/10 blur-3xl"
            style={{ animation: "cinema-orb-drift 11s ease-in-out infinite" }}
          />
          <div
            className="absolute right-0 bottom-0 h-56 w-56 rounded-full bg-fuchsia-500/10 blur-3xl"
            style={{
              animation: "cinema-orb-drift 13s ease-in-out infinite",
              animationDelay: "0.8s",
            }}
          />
        </div>
        <div className="container mx-auto px-4">
          <div className="rounded-[2rem] border border-white/8 bg-[linear-gradient(180deg,rgba(255,255,255,0.03),rgba(255,255,255,0.015))] px-4 py-4 shadow-[0_18px_60px_rgba(2,6,23,0.22)] backdrop-blur-[2px] sm:px-6 sm:py-6">
            <div
              className="mb-6 flex items-center justify-between sm:mb-8"
              style={{
                animation: "cinema-reveal 700ms ease-out both",
                animationDelay: "120ms",
              }}
            >
              <div>
                <div className="mb-2 h-px w-14 bg-gradient-to-r from-yellow-400/80 to-transparent" />
                <h2
                  className="bg-gradient-to-r from-yellow-200 via-orange-100 to-pink-200 bg-clip-text text-2xl font-black uppercase tracking-[0.12em] text-transparent sm:text-3xl"
                  style={{
                    animation: "cinema-section-glow 5.8s ease-in-out infinite",
                  }}
                >
                  Offers for you
                </h2>
                <p className="text-sm text-gray-400 sm:text-base">
                  Save more with curated promotions and perks
                </p>
              </div>
              <Button
                variant="ghost"
                className="text-purple-400 hover:text-purple-300"
                onClick={() => navigate(moviePaths.promotions())}
              >
                View all
                <ChevronRight className="ml-1 size-4" />
              </Button>
            </div>

            <div
              className="grid gap-4 sm:gap-6 md:grid-cols-3"
              style={{
                animation: "cinema-reveal 800ms ease-out both",
                animationDelay: "180ms",
              }}
            >
              {promos.map((promo) => (
                <PromoCard
                  key={promo.id}
                  {...promo}
                  onClick={() =>
                    navigate(`${moviePaths.promotions()}?promo=${promo.id}`)
                  }
                />
              ))}
            </div>
          </div>
        </div>
      </section>

      {/* Now Showing Section */}
      <section id="now-showing-section" className="relative py-10 sm:py-12">
        <div className="pointer-events-none absolute inset-0">
          <div className="absolute inset-x-0 top-4 h-px bg-gradient-to-r from-transparent via-fuchsia-400/25 to-transparent" />
          <div
            className="absolute left-[-4rem] top-24 h-64 w-64 rounded-full bg-fuchsia-500/12 blur-3xl"
            style={{ animation: "cinema-orb-drift 12s ease-in-out infinite" }}
          />
          <div
            className="absolute right-[-3rem] top-10 h-60 w-60 rounded-full bg-cyan-400/10 blur-3xl"
            style={{
              animation: "cinema-orb-drift 14s ease-in-out infinite",
              animationDelay: "1s",
            }}
          />
          <div className="absolute inset-0 bg-[linear-gradient(180deg,transparent,rgba(255,255,255,0.015),transparent)]" />
        </div>
        <div className="container mx-auto px-4">
          <div className="rounded-[2rem] border border-white/8 bg-[radial-gradient(circle_at_top,rgba(168,85,247,0.08),transparent_30%),linear-gradient(180deg,rgba(255,255,255,0.03),rgba(255,255,255,0.012))] px-4 py-4 shadow-[0_22px_70px_rgba(2,6,23,0.24)] backdrop-blur-[2px] sm:px-6 sm:py-6">
            <div
              className="mb-6 flex flex-col gap-4 sm:mb-8 sm:flex-row sm:items-center sm:justify-between"
              style={{
                animation: "cinema-reveal 800ms ease-out both",
                animationDelay: "220ms",
              }}
            >
              <div className="flex items-center gap-3">
                <TrendingUp className="size-7 text-purple-500 sm:size-8" />
                <div>
                  <div className="mb-2 h-px w-14 bg-gradient-to-r from-fuchsia-400/80 to-transparent" />
                  <h2
                    className="bg-gradient-to-r from-fuchsia-200 via-violet-100 to-cyan-200 bg-clip-text text-2xl font-black uppercase tracking-[0.12em] text-transparent sm:text-3xl"
                    style={{
                      animation:
                        "cinema-section-glow 5.4s ease-in-out infinite",
                    }}
                  >
                    Now showing
                  </h2>
                  <p className="text-sm text-gray-400">
                    The hottest movies playing right now
                  </p>
                </div>
              </div>

              <div className="overflow-x-auto pb-1 text-gray-200">
                <Tabs
                  value={selectedCategory}
                  onValueChange={setSelectedCategory}
                >
                  <TabsList className="border border-white/10 bg-white/5 shadow-[0_12px_30px_rgba(15,23,42,0.28)] backdrop-blur-md">
                    <TabsTrigger value="all">All</TabsTrigger>
                    <TabsTrigger value="action">Action</TabsTrigger>
                    <TabsTrigger value="scifi">Sci-Fi</TabsTrigger>
                    <TabsTrigger value="horror">Horror</TabsTrigger>
                  </TabsList>
                </Tabs>
              </div>
            </div>

            <div
              className="mb-4 flex items-center justify-end gap-2"
              style={{
                animation: "cinema-reveal 900ms ease-out both",
                animationDelay: "260ms",
              }}
            >
              <Button
                type="button"
                variant="ghost"
                size="icon"
                className="rounded-full border border-white/10 bg-white/5 text-white backdrop-blur-sm hover:bg-white/10"
                onClick={handlePrevNowShowing}
              >
                <ChevronLeft className="size-4" />
              </Button>
              <Button
                type="button"
                variant="ghost"
                size="icon"
                className="rounded-full border border-white/10 bg-white/5 text-white backdrop-blur-sm hover:bg-white/10"
                onClick={handleNextNowShowing}
              >
                <ChevronRight className="size-4" />
              </Button>
            </div>

            <div
              className="overflow-hidden"
              style={{
                animation: "cinema-reveal 900ms ease-out both",
                animationDelay: "280ms",
              }}
            >
              <div
                className="[--gap:1rem] [--visible:1] sm:[--gap:1.5rem] sm:[--visible:2] lg:[--visible:4]"
                onTransitionEnd={handleNowShowingTransitionEnd}
                style={{
                  display: "grid",
                  gridAutoFlow: "column",
                  gridAutoColumns:
                    "calc((100% - (var(--visible) - 1) * var(--gap)) / var(--visible))",
                  gap: "var(--gap)",
                  transform: `translateX(calc(-${nowShowingIndex} * ((100% - (var(--visible) - 1) * var(--gap)) / var(--visible) + var(--gap))))`,
                  transition: isNowShowingTransitionEnabled
                    ? "transform 2400ms cubic-bezier(0.16, 1, 0.3, 1)"
                    : "none",
                  willChange: "transform",
                  animation:
                    "cinema-carousel-breathe 7.2s ease-in-out infinite",
                }}
              >
                {nowShowingTrack.map((movie, index) => (
                  <MovieCard
                    key={`${movie.id}-track-${index}`}
                    {...movie}
                    onClick={() =>
                      navigate(
                        `${moviePaths.detail(movie.id)}${location.search}`,
                      )
                    }
                    actionLabel="View showtimes"
                    actionOnClick={() => handleOpenShowtimes(movie.id)}
                  />
                ))}
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Coming Soon Section */}
      <section className="relative py-10 sm:py-12">
        <div className="pointer-events-none absolute inset-0">
          <div className="absolute inset-x-0 top-4 h-px bg-gradient-to-r from-transparent via-cyan-400/25 to-transparent" />
          <div
            className="absolute right-[-4rem] top-16 h-64 w-64 rounded-full bg-cyan-400/12 blur-3xl"
            style={{ animation: "cinema-orb-drift 12.5s ease-in-out infinite" }}
          />
          <div
            className="absolute left-[-3rem] bottom-0 h-56 w-56 rounded-full bg-fuchsia-500/10 blur-3xl"
            style={{
              animation: "cinema-orb-drift 15s ease-in-out infinite",
              animationDelay: "0.9s",
            }}
          />
          <div className="absolute inset-0 bg-[linear-gradient(180deg,transparent,rgba(255,255,255,0.012),transparent)]" />
        </div>
        <div className="container mx-auto px-4">
          <div className="rounded-[2rem] border border-white/8 bg-[radial-gradient(circle_at_top_right,rgba(34,211,238,0.08),transparent_28%),linear-gradient(180deg,rgba(255,255,255,0.03),rgba(255,255,255,0.012))] px-4 py-4 shadow-[0_22px_70px_rgba(2,6,23,0.24)] backdrop-blur-[2px] sm:px-6 sm:py-6">
            <div
              className="mb-6 flex items-center justify-between sm:mb-8"
              style={{
                animation: "cinema-reveal 850ms ease-out both",
                animationDelay: "320ms",
              }}
            >
              <div className="flex items-center gap-3">
                <Sparkles className="size-7 text-cyan-500 sm:size-8" />
                <div>
                  <div className="mb-2 h-px w-14 bg-gradient-to-r from-cyan-400/80 to-transparent" />
                  <h2
                    className="bg-gradient-to-r from-cyan-200 via-sky-100 to-fuchsia-200 bg-clip-text text-2xl font-black uppercase tracking-[0.12em] text-transparent sm:text-3xl"
                    style={{
                      animation:
                        "cinema-section-glow 6.1s ease-in-out infinite",
                    }}
                  >
                    Coming soon
                  </h2>
                  <p className="text-sm text-gray-400">
                    Do not miss the next big premieres
                  </p>
                </div>
              </div>
              <Button
                variant="ghost"
                className="text-cyan-400 hover:text-cyan-300"
              >
                Explore more
                <ChevronRight className="ml-1 size-4" />
              </Button>
            </div>

            <div
              className="mb-4 flex items-center justify-end gap-2"
              style={{
                animation: "cinema-reveal 930ms ease-out both",
                animationDelay: "360ms",
              }}
            >
              <Button
                type="button"
                variant="ghost"
                size="icon"
                className="rounded-full border border-white/10 bg-white/5 text-white backdrop-blur-sm hover:bg-white/10"
                onClick={handlePrevComingSoon}
              >
                <ChevronLeft className="size-4" />
              </Button>
              <Button
                type="button"
                variant="ghost"
                size="icon"
                className="rounded-full border border-white/10 bg-white/5 text-white backdrop-blur-sm hover:bg-white/10"
                onClick={handleNextComingSoon}
              >
                <ChevronRight className="size-4" />
              </Button>
            </div>

            <div
              className="overflow-hidden"
              style={{
                animation: "cinema-reveal 950ms ease-out both",
                animationDelay: "380ms",
              }}
            >
              <div
                className="[--gap:1rem] [--visible:1] sm:[--gap:1.5rem] sm:[--visible:2] lg:[--visible:3]"
                onTransitionEnd={handleComingSoonTransitionEnd}
                style={{
                  display: "grid",
                  gridAutoFlow: "column",
                  gridAutoColumns:
                    "calc((100% - (var(--visible) - 1) * var(--gap)) / var(--visible))",
                  gap: "var(--gap)",
                  transform: `translateX(calc(-${comingSoonIndex} * ((100% - (var(--visible) - 1) * var(--gap)) / var(--visible) + var(--gap))))`,
                  transition: isComingSoonTransitionEnabled
                    ? "transform 2400ms cubic-bezier(0.16, 1, 0.3, 1)"
                    : "none",
                  willChange: "transform",
                  animation:
                    "cinema-carousel-breathe 7.8s ease-in-out infinite",
                }}
              >
                {comingSoonTrack.map((movie, index) => (
                  <MovieCard
                    key={`${movie.id}-track-${index}`}
                    {...movie}
                    onClick={() =>
                      navigate(
                        `${moviePaths.detail(movie.id)}${location.search}`,
                      )
                    }
                    actionLabel={
                      movie.isComingSoon ? "View details" : "View showtimes"
                    }
                    actionOnClick={() =>
                      movie.isComingSoon
                        ? navigate(
                            `${moviePaths.detail(movie.id)}${location.search}`,
                          )
                        : handleOpenShowtimes(movie.id)
                    }
                  />
                ))}
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Footer */}
      <footer className="relative border-t border-white/10 bg-[linear-gradient(180deg,rgba(2,6,23,0.86),rgba(2,6,23,0.98))] py-10">
        <div className="container mx-auto px-4">
          <div className="overflow-hidden rounded-[2rem] border border-white/10 bg-white/[0.03] shadow-[0_24px_80px_rgba(2,6,23,0.45)] backdrop-blur-md">
            <div className="h-px bg-gradient-to-r from-transparent via-fuchsia-400/60 to-transparent" />
            <div className="grid gap-8 px-6 py-8 sm:grid-cols-2 md:grid-cols-4 lg:px-8">
              <div className="relative">
                <div className="mb-4 h-1 w-12 rounded-full bg-gradient-to-r from-fuchsia-500 to-cyan-400" />
                <h3 className="mb-4 text-sm font-semibold uppercase tracking-[0.22em] text-white/95">
                  About ABCD Cinema
                </h3>
                <p className="text-sm leading-7 text-gray-400">
                  ABCD Mall's online booking platform brings a smooth, stylish,
                  and convenient movie-going experience.
                </p>
              </div>
              <div className="relative">
                <div className="mb-4 h-1 w-12 rounded-full bg-white/10 sm:hidden" />
                <h3 className="mb-4 text-sm font-semibold uppercase tracking-[0.22em] text-white/95">
                  Quick links
                </h3>
                <ul className="space-y-3 text-sm text-gray-400">
                  <li>
                    <button
                      type="button"
                      onClick={() => handleOpenShowtimes()}
                      className="transition-colors hover:text-gray-200"
                    >
                      Showtimes
                    </button>
                  </li>
                  <li>Ticket prices</li>
                  <li>Promotions</li>
                  <li>News</li>
                </ul>
              </div>
              <div className="relative">
                <div className="mb-4 h-1 w-12 rounded-full bg-white/10 sm:hidden" />
                <h3 className="mb-4 text-sm font-semibold uppercase tracking-[0.22em] text-white/95">
                  Support
                </h3>
                <ul className="space-y-3 text-sm text-gray-400">
                  <li>FAQ</li>
                  <li>Policies</li>
                  <li>Terms</li>
                  <li>Contact</li>
                </ul>
              </div>
              <div className="relative">
                <div className="mb-4 h-1 w-12 rounded-full bg-white/10 sm:hidden" />
                <h3 className="mb-4 text-sm font-semibold uppercase tracking-[0.22em] text-white/95">
                  Contact
                </h3>
                <ul className="space-y-3 text-sm text-gray-400">
                  <li>Email: info@abcdcinema.vn</li>
                  <li>Hotline: 1900 xxxx</li>
                  <li>ABCD Mall, Ho Chi Minh City</li>
                </ul>
              </div>
            </div>
            <div className="border-t border-white/10 px-6 py-5 text-center text-sm text-gray-500 lg:px-8">
              &copy; 2026 ABCD Cinema. Online movie booking made fast and easy.
            </div>
          </div>
        </div>
      </footer>
    </div>
  );
}
