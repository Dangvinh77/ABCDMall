import { CircleHelp, Clock, Film, ShieldUser } from "lucide-react";
import { Button } from "./ui/button";

interface MovieHeaderProps {
  onShowtimes: () => void;
  onSupport: () => void;
  onAdmin: () => void;
}

export function MovieHeader({
  onShowtimes,
  onSupport,
  onAdmin,
}: MovieHeaderProps) {
  return (
    <header className="relative z-30 border-b border-white/10 bg-[#040816]/35 shadow-[0_10px_40px_rgba(3,7,18,0.28)] backdrop-blur-md">
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
            onClick={onShowtimes}
            className="hidden h-10 px-3 text-base font-semibold text-gray-200 hover:bg-white/20 hover:text-white md:inline-flex"
          >
            <Clock className="mr-2 size-4" />
            Showtimes
          </Button>
          <Button
            variant="ghost"
            onClick={onSupport}
            className="hidden h-10 px-3 text-base font-semibold text-gray-200 hover:bg-white/20 hover:text-white md:inline-flex"
          >
            <CircleHelp className="mr-2 size-4" />
            Support
          </Button>
          <Button
            variant="ghost"
            onClick={onAdmin}
            className="hidden h-10 px-3 text-base font-semibold text-gray-200 hover:bg-white/20 hover:text-white md:inline-flex"
          >
            <ShieldUser className="mr-2 size-4" />
            Admin
          </Button>
        </nav>
      </div>
    </header>
  );
}
