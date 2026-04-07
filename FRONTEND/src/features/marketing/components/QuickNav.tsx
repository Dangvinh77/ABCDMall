import { Link } from 'react-router-dom';

const navItems = [
  { title: "Shopping Centers", icon: "🛍️", color: "bg-blue-100 text-blue-600", path: "/shops" },
  { title: "Food Courts", icon: "🍔", color: "bg-orange-100 text-orange-600", path: "/food-court" },
  { title: "Gallery & Events", icon: "✨", color: "bg-purple-100 text-purple-600", path: "/gallery" },
  { title: "Mall Directory", icon: "🗺️", color: "bg-green-100 text-green-600", path: "/map" },
];

export const QuickNav = () => {
  return (
    // THAY ĐỔI Ở ĐÂY: Dùng mt-8 thay vì -mt-16
    <div className="max-w-6xl mx-auto px-4 mt-8 mb-16 relative z-30">
      <div className="grid grid-cols-2 md:grid-cols-4 gap-6">
        {navItems.map((item, index) => (
          <Link 
            key={index} 
            to={item.path}
            className="bg-white rounded-[2rem] p-8 shadow-xl hover:shadow-2xl hover:-translate-y-3 border border-gray-100 transition-all duration-300 flex flex-col items-center justify-center group"
          >
            <div className={`w-20 h-20 rounded-full flex items-center justify-center text-4xl mb-6 group-hover:scale-110 transition-transform ${item.color}`}>
              {item.icon}
            </div>
            <h3 className="font-extrabold text-lg text-mall-dark">{item.title}</h3>
          </Link>
        ))}
      </div>
    </div>
  );
};