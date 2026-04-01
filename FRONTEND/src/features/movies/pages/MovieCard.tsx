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
      className="group relative cursor-pointer overflow-hidden rounded-xl bg-gray-900 shadow-lg transition-all hover:scale-105 hover:shadow-2xl"
      onClick={onClick}
    >
      {/* Movie Poster */}
      <div className="aspect-[2/3] overflow-hidden">
        <img
          src={imageUrl}
          alt={title}
          className="size-full object-cover transition-transform duration-500 group-hover:scale-110"
        />
      </div>

      {/* Overlay Gradient */}
      <div className="absolute inset-0 bg-gradient-to-t from-black/90 via-black/50 to-transparent opacity-0 transition-opacity duration-300 group-hover:opacity-100" />

      {/* Content */}
      <div className="absolute inset-x-0 bottom-0 p-4 text-white transition-transform duration-300 group-hover:translate-y-0">
        <div className="space-y-2">
          <div className="flex items-start justify-between gap-2">
            <h3 className="line-clamp-2 text-lg font-semibold leading-tight">{title}</h3>
            <Badge variant="secondary" className="shrink-0 bg-yellow-500 text-black">
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
              className="mt-2 w-full bg-gradient-to-r from-purple-600 to-pink-600 hover:from-purple-700 hover:to-pink-700"
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
        <div className="absolute right-2 top-2">
          <Badge className="bg-gradient-to-r from-blue-600 to-cyan-600">Sắp chiếu</Badge>
        </div>
      )}
    </div>
  );
}
