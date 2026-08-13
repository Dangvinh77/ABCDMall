import { InteractiveMap } from './components/InteractiveMap';

export function MapPage() {
  return (
    <div className="min-h-screen bg-mall-light">
      <div className="max-w-7xl mx-auto px-4 pt-10 pb-4">
        <h1 className="text-3xl font-black text-mall-dark mb-1">ABCD Mall Directory</h1>
        <p className="text-gray-400 mb-2">Explore more than 70 stores across 4 floors</p>
      </div>
      <InteractiveMap />
    </div>
  );
}
