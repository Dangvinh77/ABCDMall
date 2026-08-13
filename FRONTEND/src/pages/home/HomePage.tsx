import { HeroBanner } from '../../features/marketing/components/HeroBanner';
import { PromoCarousel } from '../../features/marketing/components/PromoCarousel';
import { QuickNav } from '../../features/marketing/components/QuickNav';

export const HomePage = () => {
  return (
    <main className="min-h-screen bg-mall-light">
      <HeroBanner />
      
      <PromoCarousel />

      {/* 2. Thẻ điều hướng nằm ngay dưới phần nút chấm tròn của Carousel */}
      <QuickNav />

      {/* 3. Phần giới thiệu thêm */}
      <section className="max-w-7xl mx-auto px-4 mt-16 pb-20 text-center">
        <h2 className="text-3xl font-extrabold text-mall-dark">Welcome to ABCD Mall</h2>
        <p className="text-gray-500 mt-4 text-lg">Discover a leading shopping and entertainment destination spanning more than 250,000 m2.</p>
        <div className="w-24 h-1 bg-mall-primary mx-auto mt-6 rounded-full"></div>
      </section>
    </main>
  );
};
