import { Gift, Percent } from 'lucide-react';
import { Button } from '../component/ui/button';

interface PromoCardProps {
  title: string;
  description: string;
  discount: string;
  color: string;
  imageUrl: string;
  onClick?: () => void;
}

export function PromoCard({
  title,
  description,
  discount,
  color,
  imageUrl,
  onClick,
}: PromoCardProps) {
  return (
    <div
      role={onClick ? 'button' : undefined}
      tabIndex={onClick ? 0 : undefined}
      onClick={onClick}
      onKeyDown={
        onClick
          ? (event) => {
              if (event.key === 'Enter' || event.key === ' ') {
                event.preventDefault();
                onClick();
              }
            }
          : undefined
      }
      className={`group relative overflow-hidden rounded-[1.5rem] border border-white/10 p-6 shadow-[0_20px_40px_rgba(15,23,42,0.3)] transition-all duration-300 hover:-translate-y-2 hover:shadow-[0_28px_60px_rgba(76,29,149,0.28)] ${color}`}
    >
      <img
        src={imageUrl}
        alt={title}
        className="absolute inset-0 size-full object-cover transition-transform duration-500 group-hover:scale-105"
      />
      <div className="absolute inset-0 bg-[linear-gradient(180deg,rgba(3,7,18,0.2),rgba(3,7,18,0.78))]" />
      <div className="absolute inset-0 bg-[linear-gradient(135deg,rgba(255,255,255,0.18),transparent_35%,transparent_65%,rgba(255,255,255,0.08))]" />
      <div className="absolute -right-8 -top-8 size-32 rounded-full bg-white/10 blur-xl" />
      <div className="absolute -bottom-8 -left-8 size-32 rounded-full bg-white/10 blur-xl" />
      <div className="pointer-events-none absolute inset-y-0 left-[-70%] w-1/2 rotate-12 bg-gradient-to-r from-transparent via-white/30 to-transparent opacity-0 transition-opacity duration-300 group-hover:opacity-100 group-hover:animate-[cinema-shimmer_1.25s_ease]" />

      <div className="relative z-10 flex items-start gap-4">
        <div className="rounded-full border border-white/20 bg-white/20 p-3 shadow-[0_0_24px_rgba(255,255,255,0.12)] backdrop-blur-sm">
          {discount.includes('%') ? (
            <Percent className="size-6 text-white" />
          ) : (
            <Gift className="size-6 text-white" />
          )}
        </div>

        <div className="flex-1 space-y-2">
          <h3 className="font-semibold text-white">{title}</h3>
          <p className="text-sm text-white/80">{description}</p>
          <div className="flex items-center justify-between">
            <span className="text-2xl font-bold text-white drop-shadow-[0_10px_24px_rgba(0,0,0,0.2)]">
              {discount}
            </span>
            <Button
              size="sm"
              variant="secondary"
              onClick={(event) => {
                event.stopPropagation();
                onClick?.();
              }}
              className="rounded-xl bg-white text-gray-900 shadow-[0_10px_24px_rgba(255,255,255,0.18)] hover:bg-gray-100"
            >
              Details
            </Button>
          </div>
        </div>
      </div>
    </div>
  );
}
