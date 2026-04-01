import { Clock, Star, Calendar } from 'lucide-react';
import { Button } from '../component/ui/button';
import { Badge } from '../component/ui/badge';

interface MovieCardProps {
  title: string;
  genre: string;
  rating: number;
  duration: string;
  releaseDate?: string;
  imageUrl: string;
  isComingSoon?: boolean;
  onClick?: () => void;
}

export function MovieCard({
  title,
  genre,
  rating,
  duration,
  releaseDate,
  imageUrl,
  isComingSoon = false,
  onClick,
}: MovieCardProps) {
  return (
    <div
      className="group relative cursor-pointer overflow-hidden rounded-[1.4rem] border border-white/10 bg-slate-950/70 shadow-[0_20px_40px_rgba(15,23,42,0.35)] transition-all duration-300 hover:-translate-y-2 hover:shadow-[0_30px_60px_rgba(168,85,247,0.22)]"
      style={{ transformStyle: 'preserve-3d' }}
      onClick={onClick}
    >
      <div className="absolute inset-0 rounded-[1.4rem] bg-gradient-to-br from-white/10 via-transparent to-transparent opacity-60" />
      <div className="pointer-events-none absolute -inset-px rounded-[1.4rem] border border-white/10 opacity-0 transition-opacity duration-300 group-hover:opacity-100" />
      {/* Movie Poster */}
      <div className="aspect-[2/3] overflow-hidden">
        <img
          src={imageUrl}
          alt={title}
          className="size-full object-cover transition-transform duration-300 group-hover:scale-110"
        />
        <div className="pointer-events-none absolute inset-y-0 left-[-80%] w-1/2 rotate-12 bg-gradient-to-r from-transparent via-white/25 to-transparent opacity-0 transition-opacity duration-300 group-hover:opacity-100 group-hover:animate-[cinema-shimmer_1.2s_ease]" />
      </div>

      {/* Overlay Gradient */}
      <div className="absolute inset-0 bg-gradient-to-t from-black/95 via-slate-950/40 to-transparent opacity-80 transition-opacity duration-300 group-hover:opacity-100" />
      <div className="absolute inset-x-0 top-0 h-24 bg-gradient-to-b from-black/45 to-transparent opacity-70" />

      {/* Content */}
      <div className="absolute inset-x-0 bottom-0 translate-y-3 p-4 text-white transition-transform duration-300 group-hover:translate-y-0">
        <div className="space-y-2">
          <div className="flex items-start justify-between gap-2">
            <h3 className="line-clamp-2 text-lg font-semibold leading-tight drop-shadow-[0_6px_18px_rgba(0,0,0,0.45)]">{title}</h3>
            <Badge variant="secondary" className="shrink-0 border border-yellow-200/50 bg-yellow-400/95 text-black shadow-[0_0_18px_rgba(250,204,21,0.35)]">
              <Star className="mr-1 size-3 fill-current" />
              {rating}
            </Badge>
          </div>

          <p className="text-sm text-gray-300">{genre}</p>

          <div className="flex items-center gap-3 text-xs text-gray-400">
            <span className="flex items-center gap-1">
              <Clock className="size-3" />
              {duration}
            </span>
            {releaseDate && (
              <span className="flex items-center gap-1">
                <Calendar className="size-3" />
                {releaseDate}
              </span>
            )}
          </div>

          {/* Action Button */}
          <div className="opacity-0 transition-opacity duration-300 group-hover:opacity-100">
            <Button
              className="mt-2 w-full rounded-xl bg-gradient-to-r from-purple-600 via-fuchsia-500 to-pink-500 shadow-[0_12px_24px_rgba(168,85,247,0.28)] hover:from-purple-700 hover:via-fuchsia-600 hover:to-pink-600"
              size="sm"
              onClick={(e) => {
                e.stopPropagation();
                onClick?.();
              }}
            >
              {isComingSoon ? 'Xem chi tiết' : 'Đặt vé ngay'}
            </Button>
          </div>
        </div>
      </div>

      {/* Coming Soon Badge */}
      {isComingSoon && (
        <div className="absolute right-2 top-2 [&_span]:border [&_span]:border-cyan-200/35 [&_span]:shadow-[0_0_20px_rgba(34,211,238,0.28)]">
          <Badge className="bg-gradient-to-r from-blue-600 to-cyan-600">Sắp chiếu</Badge>
        </div>
      )}
    </div>
  );
}
