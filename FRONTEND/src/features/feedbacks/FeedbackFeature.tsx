import { useState } from 'react';

export const FeedbackFeature = () => {
  const [isSubmitted, setIsSubmitted] = useState(false);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    // Giả lập call API gửi feedback
    setTimeout(() => setIsSubmitted(true), 500);
  };

  return (
    <div className="bg-slate-50 min-h-screen pt-32 pb-20">
      <div className="max-w-6xl mx-auto px-6">
        
        <div className="bg-white rounded-[2.5rem] shadow-2xl border border-gray-100 overflow-hidden flex flex-col lg:flex-row">
          
          {/* CỘT TRÁI: THÔNG TIN & HÌNH ẢNH */}
          <div className="lg:w-2/5 bg-gray-900 relative p-10 text-white flex flex-col justify-between overflow-hidden">
            {/* Hình nền mờ */}
            <div className="absolute inset-0 opacity-20">
              <img 
                src="https://images.unsplash.com/photo-1519567241046-7f154a4332f0?q=80&w=800&auto=format&fit=crop" 
                alt="Mall Interior" 
                className="w-full h-full object-cover"
              />
            </div>
            
            <div className="relative z-10">
              <h2 className="text-4xl font-black text-transparent bg-clip-text bg-gradient-to-r from-red-400 to-yellow-500 mb-4">
                Liên Hệ &<br/>Góp Ý
              </h2>
              <p className="text-gray-300 font-medium mb-10 leading-relaxed">
                Mọi ý kiến đóng góp của quý khách là thước đo giá trị nhất giúp ABCD Mall ngày càng hoàn thiện chất lượng dịch vụ.
              </p>

              <div className="space-y-6">
                <div className="flex items-start gap-4">
                  <div className="w-10 h-10 rounded-full bg-white/10 flex items-center justify-center shrink-0">📍</div>
                  <div>
                    <h4 className="font-bold text-red-400">Địa chỉ</h4>
                    <p className="text-sm text-gray-300 mt-1">123 Suburban Place,<br/>Mumbai, India</p>
                  </div>
                </div>
                <div className="flex items-start gap-4">
                  <div className="w-10 h-10 rounded-full bg-white/10 flex items-center justify-center shrink-0">📞</div>
                  <div>
                    <h4 className="font-bold text-red-400">Hotline CSKH</h4>
                    <p className="text-sm text-gray-300 mt-1">1800-ABCD-MALL<br/>(09:00 - 22:00)</p>
                  </div>
                </div>
                <div className="flex items-start gap-4">
                  <div className="w-10 h-10 rounded-full bg-white/10 flex items-center justify-center shrink-0">✉️</div>
                  <div>
                    <h4 className="font-bold text-red-400">Email</h4>
                    <p className="text-sm text-gray-300 mt-1">support@abcdmall.com</p>
                  </div>
                </div>
              </div>
            </div>
          </div>

          {/* CỘT PHẢI: FORM ĐIỀN THÔNG TIN */}
          <div className="lg:w-3/5 p-10 lg:p-14">
            {isSubmitted ? (
              <div className="h-full flex flex-col items-center justify-center text-center py-20 animate-fade-in-up">
                <div className="w-24 h-24 bg-green-100 text-green-500 rounded-full flex items-center justify-center text-5xl mb-6 shadow-inner">
                  ✓
                </div>
                <h3 className="text-3xl font-black text-gray-800 mb-4">Cảm ơn bạn!</h3>
                <p className="text-gray-500 text-lg max-w-sm">Phản hồi của bạn đã được gửi thành công đến ban quản lý ABCD Mall. Chúng tôi sẽ phản hồi bạn trong thời gian sớm nhất.</p>
                <button 
                  onClick={() => setIsSubmitted(false)}
                  className="mt-8 text-red-500 font-bold hover:underline"
                >
                  Gửi thêm ý kiến khác
                </button>
              </div>
            ) : (
              <form onSubmit={handleSubmit} className="space-y-6 animate-fade-in-up">
                <h3 className="text-2xl font-bold text-gray-800 mb-6">Biểu mẫu thông tin</h3>
                
                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                  <div className="space-y-2">
                    <label className="text-sm font-bold text-gray-600">Họ và Tên <span className="text-red-500">*</span></label>
                    <input required type="text" placeholder="Nguyễn Văn A" className="w-full px-5 py-3 rounded-xl border border-gray-200 focus:outline-none focus:border-red-500 focus:ring-1 focus:ring-red-200 transition-all bg-gray-50 hover:bg-white" />
                  </div>
                  <div className="space-y-2">
                    <label className="text-sm font-bold text-gray-600">Số điện thoại <span className="text-red-500">*</span></label>
                    <input required type="tel" placeholder="0909 xxx xxx" className="w-full px-5 py-3 rounded-xl border border-gray-200 focus:outline-none focus:border-red-500 focus:ring-1 focus:ring-red-200 transition-all bg-gray-50 hover:bg-white" />
                  </div>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                   <div className="space-y-2">
                    <label className="text-sm font-bold text-gray-600">Email</label>
                    <input type="email" placeholder="example@email.com" className="w-full px-5 py-3 rounded-xl border border-gray-200 focus:outline-none focus:border-red-500 focus:ring-1 focus:ring-red-200 transition-all bg-gray-50 hover:bg-white" />
                  </div>
                  <div className="space-y-2">
                    <label className="text-sm font-bold text-gray-600">Vấn đề liên quan <span className="text-red-500">*</span></label>
                    <select required className="w-full px-5 py-3 rounded-xl border border-gray-200 focus:outline-none focus:border-red-500 focus:ring-1 focus:ring-red-200 transition-all bg-gray-50 hover:bg-white text-gray-700 cursor-pointer appearance-none">
                      <option value="">-- Chọn danh mục --</option>
                      <option value="cs">Thái độ nhân viên / Dịch vụ CSKH</option>
                      <option value="clean">Vệ sinh cảnh quan</option>
                      <option value="security">An ninh / Trật tự</option>
                      <option value="facilities">Cơ sở vật chất / Bãi xe</option>
                      <option value="other">Góp ý khác</option>
                    </select>
                  </div>
                </div>

                <div className="space-y-2">
                  <label className="text-sm font-bold text-gray-600">Nội dung chi tiết <span className="text-red-500">*</span></label>
                  <textarea required rows={5} placeholder="Xin vui lòng mô tả chi tiết trải nghiệm của bạn..." className="w-full px-5 py-3 rounded-xl border border-gray-200 focus:outline-none focus:border-red-500 focus:ring-1 focus:ring-red-200 transition-all bg-gray-50 hover:bg-white resize-none"></textarea>
                </div>

                <button type="submit" className="w-full py-4 rounded-xl font-black text-white bg-gradient-to-r from-red-600 to-orange-500 hover:from-red-700 hover:to-orange-600 shadow-lg hover:shadow-xl transform hover:-translate-y-1 transition-all duration-300">
                  GỬI PHẢN HỒI
                </button>
              </form>
            )}
          </div>

        </div>
      </div>
    </div>
  );
};