import { useState, useMemo } from 'react';
import { useSearchParams, Link } from 'react-router-dom';
import { mockBrandsData } from './api/brandsMockData';
import { getImageUrl } from "@/core/utils/image";

export const BrandsFeature = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const currentCategory = searchParams.get('category') || 'all';
  const [searchTerm, setSearchTerm] = useState('');
  const [activeLetter, setActiveLetter] = useState<string | null>(null);
  const [activeFloor, setActiveFloor] = useState<string | null>(null);

  const categories = [
    { name: 'All', slug: 'all' },
    { name: 'Fashion', slug: 'thoi-trang' },
    { name: 'Jewelry & Accessories', slug: 'phu-kien' },
    { name: 'Dining', slug: 'am-thuc' },
    { name: 'Education', slug: 'giao-duc' },
    { name: 'Entertainment', slug: 'giai-tri' },
  ];

  const diningCategories = new Set(['am-thuc', 'foods']);
  const foodCourtBrandSlugs = new Set([
    'gogi-house',
    'kichi-kichi',
    'texas-chicken',
    'sushix',
    'starbucks',
    'dookki',
    'pizza-company',
  ]);
  const foodSlugMap: Record<string, string> = {
    'pizza-company': 'the-pizza-company',
    'dookki': 'dookki-vietnam',
  };
  const alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".split("");
  const floors = [
    { value: "Tầng 1", label: "Floor 1" },
    { value: "Tầng 2", label: "Floor 2" },
    { value: "Tầng 3", label: "Floor 3" },
    { value: "Tầng 4", label: "Floor 4" },
  ];

  const getFloorLabel = (floor: string) =>
    floors.find((item) => item.value === floor)?.label ?? floor;

  const filteredBrands = useMemo(() => {
    return mockBrandsData.filter(brand => {
      const matchCategory =
        currentCategory === 'all' ||
        brand.category === currentCategory ||
        (currentCategory === 'am-thuc' && diningCategories.has(brand.category));
      const matchSearch = brand.name.toLowerCase().includes(searchTerm.toLowerCase());
      const matchLetter = !activeLetter || brand.name.toUpperCase().startsWith(activeLetter);
      const matchFloor = !activeFloor || brand.floor === activeFloor;
      return matchCategory && matchSearch && matchLetter && matchFloor;
    });
  }, [currentCategory, searchTerm, activeLetter, activeFloor]);

  return (
    <div className="min-h-screen bg-slate-50 pt-10 pb-20">
      <div className="max-w-7xl mx-auto px-6">
        
        {/* HEADER */}
        <div className="text-center mb-12">
          <h1 className="text-4xl md:text-5xl font-black uppercase text-transparent bg-clip-text bg-gradient-to-r from-red-600 to-orange-500 mb-6 tracking-tight">
            {categories.find(c => c.slug === currentCategory)?.name || 'Brands'}
          </h1>
          
          <div className="flex flex-col md:flex-row justify-center items-center gap-4 max-w-3xl mx-auto">
            <div className="relative w-full md:w-1/2">
              <input 
                type="text" 
                placeholder="Search brand names..."
                className="w-full pl-6 pr-12 py-3.5 rounded-full border-2 border-transparent shadow-md focus:outline-none focus:border-red-400 transition-all"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
              />
              <span className="absolute right-5 top-1/2 -translate-y-1/2 text-gray-400 text-lg">🔍</span>
            </div>
            
            <select 
              className="w-full md:w-1/3 px-6 py-3.5 rounded-full border-2 border-transparent shadow-md bg-white font-bold text-gray-700 cursor-pointer focus:outline-none focus:border-red-400 transition-all appearance-none"
              value={currentCategory}
              onChange={(e) => setSearchParams({ category: e.target.value })}
            >
              {categories.map(cat => (
                <option key={cat.slug} value={cat.slug}>{cat.name}</option>
              ))}
            </select>
          </div>
        </div>

        <div className="flex flex-col lg:flex-row gap-8">
          
          {/* SIDEBAR */}
          <div className="lg:w-64 shrink-0 flex flex-col gap-6">
            {/* Alphabet Box */}
            <div className="bg-white rounded-[2rem] p-5 shadow-lg border border-gray-100">
              <div className="flex items-center gap-2 mb-4 px-2">
                <div className="w-2 h-6 bg-gradient-to-b from-red-500 to-orange-400 rounded-full"></div>
                <h3 className="font-black text-gray-800 uppercase tracking-wide">Alphabet</h3>
              </div>
              <div className="grid grid-cols-5 gap-2">
                <button 
                  onClick={() => setActiveLetter(null)}
                  className={`col-span-5 py-2 text-sm font-bold rounded-xl transition-all ${!activeLetter ? 'bg-gradient-to-r from-red-500 to-orange-500 text-white shadow-md' : 'bg-gray-50 text-gray-500 hover:bg-gray-100'}`}
                >
                  All
                </button>
                {alphabet.map(letter => (
                  <button 
                    key={letter}
                    onClick={() => setActiveLetter(letter)}
                    className={`aspect-square flex items-center justify-center text-sm font-bold rounded-xl transition-all ${activeLetter === letter ? 'bg-gradient-to-br from-red-500 to-orange-500 text-white shadow-md scale-110' : 'bg-gray-50 text-gray-600 hover:bg-red-50 hover:text-red-500'}`}
                  >
                    {letter}
                  </button>
                ))}
              </div>
            </div>

            {/* Floor Box */}
            <div className="bg-white rounded-[2rem] p-5 shadow-lg border border-gray-100">
               <div className="flex items-center gap-2 mb-4 px-2">
                <div className="w-2 h-6 bg-gradient-to-b from-red-500 to-orange-400 rounded-full"></div>
                <h3 className="font-black text-gray-800 uppercase tracking-wide">Floor</h3>
              </div>
              <div className="flex flex-col gap-2">
                <button 
                  onClick={() => setActiveFloor(null)}
                  className={`py-3 px-4 text-sm font-bold rounded-xl transition-all text-left ${!activeFloor ? 'bg-gradient-to-r from-red-500 to-orange-500 text-white shadow-md' : 'bg-gray-50 text-gray-500 hover:bg-gray-100'}`}
                >
                  All floors
                </button>
                {floors.map(floor => (
                  <button 
                    key={floor.value}
                    onClick={() => setActiveFloor(floor.value)}
                    className={`py-3 px-4 text-sm font-bold rounded-xl transition-all text-left flex justify-between items-center ${activeFloor === floor.value ? 'bg-gradient-to-r from-red-500 to-orange-500 text-white shadow-md translate-x-1' : 'bg-gray-50 text-gray-600 hover:bg-red-50 hover:text-red-500'}`}
                  >
                    <span>{floor.label}</span>
                    <span className="text-[10px] opacity-70">📍</span>
                  </button>
                ))}
              </div>
            </div>
          </div>

          {/* MAIN CONTENT */}
          <div className="flex-1">
            {filteredBrands.length > 0 ? (
              <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6">
                {filteredBrands.map(brand => {
                  const foodRouteSlug = foodCourtBrandSlugs.has(brand.slug)
                    ? foodSlugMap[brand.slug] ?? brand.slug
                    : null;
                  const destination = foodRouteSlug
                    ? `/food-court/${foodRouteSlug}`
                    : `/shops/${brand.slug}`;

                  return (
                    <Link 
                      to={destination}
                      key={brand.id}
                      className="bg-white rounded-[2rem] border border-gray-100 shadow-sm hover:shadow-2xl hover:-translate-y-2 transition-all duration-300 group flex flex-col items-center p-6 text-center relative overflow-hidden"
                    >
                      <div className="absolute top-0 left-0 w-full h-1 bg-gradient-to-r from-red-500 to-orange-400 opacity-0 group-hover:opacity-100 transition-opacity"></div>
                      
                      <div className="w-24 h-24 md:w-32 md:h-32 mb-4 flex items-center justify-center p-2">
                        <img 
                          src={getImageUrl(brand.logoUrl)}
                          alt={brand.name} 
                          className="max-w-full max-h-full object-contain group-hover:scale-110 transition-transform duration-500" 
                        />
                      </div>
                      <h3 className="font-black text-gray-800 uppercase tracking-wide group-hover:text-red-500 transition-colors">{brand.name}</h3>
                      <p className="text-xs font-bold text-gray-400 mt-2 bg-gray-50 px-3 py-1 rounded-full">📍 {getFloorLabel(brand.floor)}</p>
                    </Link>
                  );
                })}
              </div>
            ) : (
              <div className="bg-white rounded-[2rem] border-2 border-dashed border-gray-200 p-20 text-center flex flex-col items-center">
                <span className="text-6xl mb-4">🏬</span>
                <h3 className="text-2xl font-bold text-gray-400">No matching brands found!</h3>
                <button 
                  onClick={() => {setSearchTerm(''); setActiveFloor(null); setActiveLetter(null);}}
                  className="mt-6 px-8 py-3 bg-red-50 text-red-500 font-bold rounded-full hover:bg-red-100 transition-colors shadow-sm"
                >
                  Clear filters
                </button>
              </div>
            )}
          </div>

        </div>
      </div>
    </div>
  );
};