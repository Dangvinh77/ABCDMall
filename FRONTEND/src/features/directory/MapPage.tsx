import { InteractiveMap } from './components/InteractiveMap';

export function MapPage() {
  return (
    <div className="min-h-screen bg-mall-light">
      <div className="max-w-7xl mx-auto px-4 pt-10 pb-4">
        <h1 className="text-3xl font-black text-mall-dark mb-1">Sơ đồ ABCD Mall</h1>
        <p className="text-gray-400 mb-2">Khám phá hơn 70 cửa hàng trên 4 tầng</p>
      </div>
      <InteractiveMap />
    </div>
  );
}