import { Gift, Percent } from 'lucide-react';
import { Button } from '../component/ui/button';

interface PromoCardProps {
  title: string;
  description: string;
  discount: string;
  color: string;
}

export function PromoCard({ title, description, discount, color }: PromoCardProps) {
  return (
    <div
      className={`group relative overflow-hidden rounded-xl p-6 shadow-lg transition-all hover:scale-105 hover:shadow-2xl ${color}`}
    >
      <div className="absolute -right-8 -top-8 size-32 rounded-full bg-white/10" />
      <div className="absolute -bottom-8 -left-8 size-32 rounded-full bg-white/10" />

      <div className="relative z-10 flex items-start gap-4">
        <div className="rounded-full bg-white/20 p-3">
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
            <span className="text-2xl font-bold text-white">{discount}</span>
            <Button
              size="sm"
              variant="secondary"
              className="bg-white text-gray-900 hover:bg-gray-100"
            >
              Chi tiết
            </Button>
          </div>
        </div>
      </div>
    </div>
  );
}
