import { useState } from 'react';
import { Link } from 'react-router-dom';

export const FaqFeature = () => {
  const [activeTab, setActiveTab] = useState<string>('shopping');
  const [activeQuestion, setActiveQuestion] = useState<number | null>(null);

  const categories = [
    { id: 'shopping', name: '🛍️ Mua sắm & Dịch vụ' },
    { id: 'amenities', name: '🚗 Tiện ích & Gửi xe' },
    { id: 'cinema', name: '🎬 Rạp chiếu phim' },
    { id: 'other', name: '💡 Quy định khác' }
  ];

  const faqs: Record<string, { q: string; a: string }[]> = {
    shopping: [
      { q: 'Giờ mở cửa hoạt động của ABCD Mall là khi nào?', a: 'ABCD Mall mở cửa phục vụ quý khách từ 09:30 đến 22:00 tất cả các ngày trong tuần, bao gồm cả Lễ và Chủ Nhật. Riêng khu vực Rạp chiếu phim CGV và một số nhà hàng có thể đóng cửa muộn hơn.' },
      { q: 'Làm thế nào để tôi xuất hóa đơn GTGT (VAT)?', a: 'Quý khách vui lòng yêu cầu xuất hóa đơn VAT trực tiếp tại quầy thanh toán của gian hàng ngay tại thời điểm mua sắm. Mall không hỗ trợ xuất hóa đơn gộp cho nhiều gian hàng khác nhau.' },
      { q: 'Mall có chương trình thẻ thành viên không?', a: 'Có! Quý khách có thể đăng ký thẻ ABCD Member ngay tại Quầy Thông Tin (Tầng 1) hoặc qua ứng dụng di động để tích điểm và nhận các ưu đãi độc quyền từ hơn 200 thương hiệu.' }
    ],
    amenities: [
      { q: 'Bãi gửi xe của Mall nằm ở đâu và phí gửi xe là bao nhiêu?', a: 'ABCD Mall có bãi đậu xe máy tại Tầng hầm B1, B2 và bãi đậu xe ô tô tại Tầng 3M, Tầng 5. Phí gửi xe máy là 5,000đ/4h đầu, Ô tô là 30,000đ/4h đầu.' },
      { q: 'Mall có cung cấp xe lăn cho người lớn tuổi/người khuyết tật không?', a: 'ABCD Mall cung cấp DỊCH VỤ MƯỢN XE LĂN MIỄN PHÍ. Quý khách vui lòng liên hệ Quầy Thông Tin tại Tầng 1 và xuất trình CCCD/CMND để được hỗ trợ.' },
      { q: 'Mall có phòng chăm sóc trẻ em (Baby Room) không?', a: 'Có. Phòng chăm sóc trẻ em được bố trí tại Tầng 2 và Tầng 3, trang bị đầy đủ bàn thay tã, máy nước nóng lạnh và không gian cho con bú riêng tư.' }
    ],
    cinema: [
      { q: 'Tôi có thể mua vé xem phim online qua website này không?', a: 'Hoàn toàn được! Bạn có thể truy cập mục "Rạp Phim" trên thanh menu để chọn phim, chọn suất chiếu, chọn ghế ngồi và thanh toán trực tuyến dễ dàng.' },
      { q: 'Trẻ em dưới bao nhiêu tuổi thì được miễn phí vé xem phim?', a: 'Theo quy định của rạp chiếu phim, trẻ em có chiều cao dưới 90cm sẽ được miễn phí vé khi ngồi chung ghế với người lớn đi kèm.' }
    ],
    other: [
      { q: 'Tôi có được mang thú cưng (chó, mèo) vào Mall không?', a: 'Rất tiếc, để đảm bảo an toàn và vệ sinh chung, ABCD Mall hiện chưa cho phép mang thú cưng vào khu vực trung tâm thương mại (ngoại trừ chó dẫn đường cho người khiếm thị).' },
      { q: 'Tôi để quên đồ tại Mall, tôi cần liên hệ ai?', a: 'Nếu thất lạc tài sản, quý khách vui lòng đến ngay Quầy Thông Tin (Tầng 1) hoặc gọi Hotline 1800-ABCD-MALL để được bộ phận An ninh hỗ trợ kiểm tra và tìm kiếm.' }
    ]
  };

  const toggleQuestion = (index: number) => {
    setActiveQuestion(activeQuestion === index ? null : index);
  };

  return (
    <div className="bg-slate-50 min-h-screen pt-32 pb-20">
      <div className="max-w-4xl mx-auto px-6">
        
        <div className="text-center mb-12">
          <h1 className="text-4xl md:text-5xl font-black uppercase text-transparent bg-clip-text bg-gradient-to-r from-red-600 to-orange-500 mb-4 tracking-tight">
            Câu Hỏi Thường Gặp
          </h1>
          <p className="text-gray-500 text-lg">Chúng tôi luôn sẵn sàng giải đáp mọi thắc mắc của bạn để mang lại trải nghiệm tuyệt vời nhất tại ABCD Mall.</p>
        </div>

        {/* Tab Categories */}
        <div className="flex flex-wrap justify-center gap-3 mb-10">
          {categories.map(cat => (
            <button
              key={cat.id}
              onClick={() => { setActiveTab(cat.id); setActiveQuestion(null); }}
              className={`px-6 py-3 rounded-full font-bold transition-all duration-300 shadow-sm ${
                activeTab === cat.id 
                  ? 'bg-gradient-to-r from-red-500 to-orange-500 text-white scale-105' 
                  : 'bg-white text-gray-600 hover:text-red-500 border border-gray-200'
              }`}
            >
              {cat.name}
            </button>
          ))}
        </div>

        {/* FAQ Accordion */}
        <div className="bg-white rounded-[2rem] shadow-xl border border-gray-100 p-6 md:p-10">
          <div className="space-y-4">
            {faqs[activeTab].map((faq, index) => (
              <div key={index} className="border border-gray-100 rounded-2xl overflow-hidden transition-all duration-300">
                <button
                  onClick={() => toggleQuestion(index)}
                  className={`w-full text-left px-6 py-5 flex justify-between items-center transition-colors ${
                    activeQuestion === index ? 'bg-red-50' : 'bg-white hover:bg-gray-50'
                  }`}
                >
                  <span className={`font-bold text-lg pr-4 ${activeQuestion === index ? 'text-red-600' : 'text-gray-800'}`}>
                    {faq.q}
                  </span>
                  <span className={`text-2xl transition-transform duration-300 ${activeQuestion === index ? 'rotate-180 text-red-500' : 'text-gray-400'}`}>
                    ↓
                  </span>
                </button>
                
                <div 
                  className={`overflow-hidden transition-all duration-300 ease-in-out ${
                    activeQuestion === index ? 'max-h-96 opacity-100' : 'max-h-0 opacity-0'
                  }`}
                >
                  <div className="px-6 pb-5 pt-2 text-gray-600 leading-relaxed border-t border-red-100">
                    {faq.a}
                  </div>
                </div>
              </div>
            ))}
          </div>

          {/* Call to action */}
          <div className="mt-12 pt-8 border-t border-gray-200 text-center">
            <p className="text-gray-500 mb-4">Bạn không tìm thấy câu trả lời mình cần?</p>
            <Link to="/feedback" className="inline-block bg-gray-900 text-white font-bold px-8 py-3 rounded-full hover:bg-red-600 transition-colors shadow-md">
              Gửi Phản Hồi Cho Chúng Tôi
            </Link>
          </div>
        </div>

      </div>
    </div>
  );
};