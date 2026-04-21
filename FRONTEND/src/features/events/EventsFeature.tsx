import { useState, useEffect } from 'react';
// IMPORT THÊM useSearchParams
import { Link, useSearchParams } from 'react-router-dom'; 
import { eventsApi } from './api/eventsApi';
import type { EventDto } from './types/event.types';

export const EventsFeature = () => {
  // KHAI BÁO BỘ LỌC TỪ URL
  const [searchParams] = useSearchParams();
  const statusParam = searchParams.get('status') || undefined;

  const [events, setEvents] = useState<EventDto[]>([]);
  const [activeTab, setActiveTab] = useState<number>(0); 
  const [isLoading, setIsLoading] = useState(true);

  // Thay đổi tiêu đề tự động theo Navbar
  const pageTitle = statusParam === 'Ongoing' ? 'Sự Kiện Đang Diễn Ra' : statusParam === 'Upcoming' ? 'Sự Kiện Sắp Diễn Ra' : 'Tất Cả Sự Kiện';

  const mockEvents: EventDto[] = [
    {
      id: "e1", title: "Lễ Hội Đèn Lồng Trăng Rằm", description: "Cùng chiêm ngưỡng cây đèn lồng khổng lồ và rước đèn quanh trung tâm thương mại.", coverImageUrl: "https://images.unsplash.com/photo-1541727687969-ce40493cd847?q=80&w=800", startDate: "2026-08-15T00:00:00", endDate: "2026-08-20T00:00:00", location: "Sảnh Trung Tâm Tầng 1", eventType: "MallEvent", eventTypeId: 1, isHot: true, status: "Upcoming", statusId: 1, createdAt: "2026-04-19T00:00:00"
    },
    {
      id: "e2", title: "UNIQLO - Ra mắt BST Thu Đông", description: "Trải nghiệm không gian Pop-up độc quyền và nhận túi tote giới hạn.", coverImageUrl: "https://images.unsplash.com/photo-1441984904996-e0b6ba687e04?q=80&w=800", startDate: "2026-09-01T00:00:00", endDate: "2026-09-07T00:00:00", location: "Sảnh Sự Kiện Tầng 1", eventType: "BrandEvent", eventTypeId: 2, shopId: "uniqlo", shopName: "UNIQLO", isHot: false, status: "Ongoing", statusId: 2, createdAt: "2026-04-19T00:00:00"
    }
  ];

  // GỌI API LẠI MỖI KHI ĐỔI TAB HOẶC CLICK TỪ NAVBAR
  useEffect(() => {
    const fetchEvents = async () => {
      setIsLoading(true);
      try {
        // Truyền thêm statusParam xuống API
        const data = await eventsApi.getEvents(undefined, activeTab === 0 ? undefined : activeTab, statusParam);
        
        if (!data || data.length === 0) {
          // Xử lý Mock Data chạy offline
          let filteredMock = mockEvents;
          if (activeTab !== 0) filteredMock = filteredMock.filter(e => e.eventTypeId === activeTab);
          if (statusParam) filteredMock = filteredMock.filter(e => e.status.toLowerCase() === statusParam.toLowerCase());
          setEvents(filteredMock);
        } else {
          setEvents(data);
        }
      } catch (error) {
        console.error(error);
        setEvents([]);
      } finally {
        setIsLoading(false);
      }
    };
    
    fetchEvents();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }, [activeTab, statusParam]); // Lắng nghe sự thay đổi

  return (
    <div className="bg-slate-50 min-h-screen pt-32 pb-20">
      <div className="max-w-7xl mx-auto px-6">
        
        {/* HEADER ĐỘNG TIÊU ĐỀ */}
        <div className="text-center mb-12">
          <h1 className="text-4xl md:text-5xl font-black uppercase text-transparent bg-clip-text bg-gradient-to-r from-red-600 to-orange-500 mb-4 tracking-tight">
            {pageTitle}
          </h1>
          <p className="text-gray-500 text-lg max-w-2xl mx-auto font-medium">
            Cập nhật những chương trình ưu đãi, lễ hội và sự kiện hấp dẫn nhất.
          </p>
        </div>

        {/* TABS LỌC THEO LOẠI */}
        <div className="flex flex-wrap justify-center gap-4 mb-12">
          {[{ id: 0, label: 'Tất cả sự kiện' }, { id: 1, label: '🎪 Sự kiện Mall' }, { id: 2, label: '🛍️ Sự kiện Nhãn hàng' }].map(tab => (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id)}
              className={`px-8 py-3 rounded-full font-bold transition-all duration-300 shadow-sm ${
                activeTab === tab.id 
                  ? 'bg-gradient-to-r from-red-500 to-orange-500 text-white scale-105 shadow-lg' 
                  : 'bg-white text-gray-600 hover:text-red-500 border border-gray-200'
              }`}
            >
              {tab.label}
            </button>
          ))}
        </div>

        {/* LƯỚI SỰ KIỆN */}
        {isLoading ? (
          <div className="text-center py-20 text-2xl animate-pulse text-gray-400">Đang tải sự kiện...</div>
        ) : events.length > 0 ? (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
            {events.map((event, index) => {
              const dateObj = new Date(event.startDate);
              const formattedDate = `${dateObj.getDate()}/${dateObj.getMonth() + 1}/${dateObj.getFullYear()}`;
              
              return (
                <div key={event.id} className="bg-white rounded-[2rem] overflow-hidden shadow-sm hover:shadow-2xl hover:-translate-y-2 transition-all duration-300 group border border-gray-100 flex flex-col relative" style={{ animation: `fadeIn 0.5s ease-out ${index * 0.1}s both` }}>
                  
                  {/* BADGE HOT & STATUS */}
                  <div className="absolute top-4 left-4 z-10 flex gap-2">
                    {event.isHot && (
                      <span className="bg-red-500 text-white text-xs font-black px-3 py-1.5 rounded-full shadow-md uppercase tracking-wider animate-pulse">
                        🔥 HOT
                      </span>
                    )}
                    <span className="bg-slate-900/80 backdrop-blur-sm text-white text-xs font-bold px-3 py-1.5 rounded-full shadow-md uppercase">
                      {event.status === 'Ongoing' ? 'Đang diễn ra' : 'Sắp diễn ra'}
                    </span>
                  </div>

                  <div className="relative h-60 overflow-hidden">
                    <img src={event.coverImageUrl} alt={event.title} className="w-full h-full object-cover group-hover:scale-110 transition-transform duration-700" />
                    <div className="absolute inset-0 bg-gradient-to-t from-black/80 via-black/20 to-transparent opacity-80"></div>
                    
                    <div className="absolute bottom-4 left-4 text-white">
                       <p className="text-sm font-bold text-red-400">Từ {formattedDate}</p>
                       <p className="text-xs text-gray-300 flex items-center gap-1 mt-1">📍 {event.location}</p>
                    </div>
                  </div>

                  <div className="p-6 flex-1 flex flex-col">
                    {event.eventTypeId === 2 && event.shopName && (
                      <span className="text-xs font-bold text-orange-500 bg-orange-50 self-start px-2 py-1 rounded mb-3 border border-orange-100">
                        Thương hiệu: {event.shopName}
                      </span>
                    )}
                    
                    <h3 className="text-xl font-black text-gray-800 mb-3 group-hover:text-red-500 transition-colors line-clamp-2">
                      {event.title}
                    </h3>
                    <p className="text-gray-500 text-sm leading-relaxed font-medium mb-6 line-clamp-3">
                      {event.description}
                    </p>
                    
                    <div className="mt-auto">
                      {event.eventTypeId === 2 ? (
                        <Link to={event.shopId ? `/shops/${event.shopId}` : `/map`} className="block text-center w-full py-3 rounded-xl font-bold text-red-500 bg-red-50 hover:bg-red-500 hover:text-white transition-colors">
                          Đến trang cửa hàng &rarr;
                        </Link>
                      ) : (
                        <Link to="/map" className="block text-center w-full py-3 rounded-xl font-bold text-gray-700 bg-gray-100 hover:bg-gray-200 transition-colors">
                          Xem sơ đồ đường đi
                        </Link>
                      )}
                    </div>
                  </div>
                </div>
              )
            })}
          </div>
        ) : (
          <div className="text-center py-20 text-xl font-bold text-gray-400 border-2 border-dashed border-gray-200 rounded-[2rem]">
            Không tìm thấy sự kiện nào trong mục này.
          </div>
        )}

      </div>
      <style>{`@keyframes fadeIn { from { opacity: 0; transform: translateY(20px); } to { opacity: 1; transform: translateY(0); } }`}</style>
    </div>
  );
};