import { InteractiveMap } from '../../features/directory/components/InteractiveMap';

export const MapPage = () => {
  return (
    // Thêm pt-24 (padding-top) để nội dung không bị dính vào Header/Menu trên cùng (nếu có)
    <main className="min-h-screen bg-mall-light pt-12 pb-20">
      <div className="text-center mb-8 animate-fade-in-up">
        <h1 className="text-4xl md:text-5xl font-extrabold text-mall-dark">
          Sơ Đồ Trung Tâm Thương Mại
        </h1>
        <p className="text-gray-500 mt-4 text-lg">
          Khám phá không gian 4 tầng đẳng cấp của ABCD Mall
        </p>
        <div className="w-24 h-1 bg-mall-primary mx-auto mt-6 rounded-full"></div>
      </div>
      
      {/* Gọi Component bản đồ đã làm ở bước trước ra đây */}
      <InteractiveMap />
    </main>
  );
};