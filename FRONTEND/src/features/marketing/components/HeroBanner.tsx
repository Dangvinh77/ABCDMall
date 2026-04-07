export const HeroBanner = () => {
  return (
    <div className="relative w-full h-[80vh] flex items-center justify-center overflow-hidden">
      {/* Background Image với hiệu ứng zoom chậm */}
      <div 
        className="absolute inset-0 bg-cover bg-center z-0 scale-105 animate-pulse-slow"
        style={{ backgroundImage: "url('https://images.unsplash.com/photo-1519567241046-7f570eee3ce6?q=80&w=2000&auto=format&fit=crop')" }}
      />
      {/* Lớp phủ Gradient để chữ nổi bật */}
      <div className="absolute inset-0 bg-gradient-to-r from-mall-dark/80 to-mall-primary/40 z-10" />

      {/* Nội dung trung tâm */}
      <div className="relative z-20 text-center px-4 opacity-0 animate-fade-in-up">
        <span className="inline-block py-1 px-3 rounded-full bg-mall-accent text-mall-dark font-bold text-sm mb-4">
          Grand Opening 2026
        </span>
        <h1 className="text-5xl md:text-7xl font-extrabold text-white mb-6 drop-shadow-lg">
          Welcome to <span className="text-transparent bg-clip-text bg-gradient-to-r from-mall-accent to-white">ABCD Mall</span>
        </h1>
        <p className="text-lg md:text-xl text-gray-200 mb-8 max-w-2xl mx-auto">
          Trải nghiệm không gian mua sắm, ẩm thực và giải trí đẳng cấp bậc nhất với diện tích 250,000 sq ft.
        </p>
      </div>
    </div>
  );
};