export const AmenitiesFeature = () => {
  // Danh sách dữ liệu Tiện ích (Lấy ý tưởng từ Gigamall)
  const amenities = [
    {
      id: 1,
      title: "Free Wi-Fi",
      description: "Guests can enjoy high-speed complimentary Wi-Fi across all public areas of ABCD Mall.",
      icon: "🛜",
      gradient: "from-blue-400 to-cyan-400",
      image: "https://images.unsplash.com/photo-1563986768609-322da13575f3?q=80&w=600&auto=format&fit=crop"
    },
    {
      id: 2,
      title: "Phone Charging",
      description: "Free mobile device charging stations are available so you can shop without worrying about running out of battery.",
      icon: "🔋",
      gradient: "from-green-400 to-emerald-400",
      image: "https://images.unsplash.com/photo-1583863788434-e58a36330cf0?q=80&w=600&auto=format&fit=crop"
    },
    {
      id: 3,
      title: "Strollers & Wheelchairs",
      description: "ABCD Mall provides complimentary baby strollers and wheelchairs at the information desk on Floor 1.",
      icon: "♿",
      gradient: "from-purple-500 to-indigo-500",
      image: "https://images.unsplash.com/photo-1506869640319-fea1a28a1158?q=80&w=600&auto=format&fit=crop"
    },
    {
      id: 4,
      title: "ATM Machines",
      description: "ATMs from major banks are conveniently located on Floor 1.",
      icon: "🏧",
      gradient: "from-yellow-400 to-orange-400",
      image: "https://images.unsplash.com/photo-1621961458348-f013d219b50c?q=80&w=600&auto=format&fit=crop"
    },
    {
      id: 5,
      title: "Smart Parking",
      description: "A spacious 7-level parking system with license plate recognition and smart parking guidance technology.",
      icon: "🅿️",
      gradient: "from-slate-600 to-slate-800",
      image: "https://images.unsplash.com/photo-1506521781263-d8422e82f27a?q=80&w=600&auto=format&fit=crop"
    },
    {
      id: 6,
      title: "Baby Care Room",
      description: "A private and comfortable space on Floor 2 with diaper-changing stations and baby bottle warmers for parents and infants.",
      icon: "🍼",
      gradient: "from-pink-400 to-rose-400",
      image: "https://images.unsplash.com/photo-1519689680058-324335c77eba?q=80&w=600&auto=format&fit=crop"
    }
  ];

  return (
    <div className="bg-slate-50 min-h-screen pt-32 pb-20">
      <div className="max-w-7xl mx-auto px-6">
        
        {/* HEADER */}
        <div className="text-center mb-16 animate-[fadeIn_0.8s_ease-out]">
          <h1 className="text-4xl md:text-5xl font-black uppercase text-transparent bg-clip-text bg-gradient-to-r from-red-600 to-orange-500 mb-4 tracking-tight">
            Amenities & Services
          </h1>
          <p className="text-gray-500 text-lg max-w-2xl mx-auto font-medium">
            ABCD Mall continuously improves its services to deliver the most complete and comfortable experience for every visitor.
          </p>
        </div>

        {/* GRID LAYOUT */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-10">
          {amenities.map((item, index) => (
            <div 
              key={item.id} 
              // Thêm delay animation mượt mà cho từng card
              className="bg-white rounded-[2rem] overflow-hidden shadow-sm hover:shadow-2xl hover:-translate-y-2 transition-all duration-500 group border border-gray-100 flex flex-col"
              style={{ animation: `fadeIn 0.8s ease-out ${index * 0.15}s both`, opacity: 0 }}
            >
              {/* IMAGE HEADER */}
              <div className="relative h-56 overflow-hidden">
                <img 
                  src={item.image} 
                  alt={item.title} 
                  className="w-full h-full object-cover group-hover:scale-110 transition-transform duration-700"
                />
                <div className="absolute inset-0 bg-black/20 group-hover:bg-black/10 transition-colors"></div>
                
                {/* ICON BADGE NỔI TRÊN ẢNH */}
                <div className={`absolute -bottom-6 right-6 w-16 h-16 rounded-2xl bg-gradient-to-br ${item.gradient} text-white flex items-center justify-center text-3xl shadow-lg border-4 border-white group-hover:scale-110 transition-transform duration-300 z-10`}>
                  {item.icon}
                </div>
              </div>

              {/* CONTENT */}
              <div className="p-8 pt-10 flex-1 flex flex-col">
                <h3 className="text-2xl font-black text-gray-800 mb-3 group-hover:text-red-500 transition-colors">
                  {item.title}
                </h3>
                <p className="text-gray-500 leading-relaxed font-medium">
                  {item.description}
                </p>
              </div>
            </div>
          ))}
        </div>

      </div>

      {/* Thêm style CSS keyframe trực tiếp vào component để chạy animation */}
      <style>{`
        @keyframes fadeIn {
          from { opacity: 0; transform: translateY(20px); }
          to { opacity: 1; transform: translateY(0); }
        }
      `}</style>
    </div>
  );
};
