import { useState } from 'react';

export const ContactFeature = () => {
  const [isSubmitted, setIsSubmitted] = useState(false);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    // Giả lập độ trễ mạng khi gửi form
    setTimeout(() => setIsSubmitted(true), 600);
  };

  return (
    <div className="bg-slate-50 min-h-screen pt-32 pb-20">
      <div className="max-w-7xl mx-auto px-6">
        
        {/* Tiêu đề trang */}
        <div className="text-center mb-16">
          <h1 className="text-4xl md:text-5xl font-black uppercase text-transparent bg-clip-text bg-gradient-to-r from-red-600 to-orange-500 mb-4 tracking-tight">
            Contact Us
          </h1>
          <p className="text-gray-500 text-lg max-w-2xl mx-auto">
            The ABCD Mall team is always ready to listen and support you. Leave us a message or visit our operations headquarters.
          </p>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-10 mb-16">
          
          {/* CỘT TRÁI: THÔNG TIN LIÊN HỆ (1 cột) */}
          <div className="lg:col-span-1 space-y-6">
            
            {/* Card Địa chỉ (Aptech Ấn Độ) */}
            <div className="bg-white p-8 rounded-[2rem] shadow-sm border border-gray-100 hover:shadow-lg transition-shadow duration-300">
              <div className="w-12 h-12 bg-red-50 text-red-500 rounded-2xl flex items-center justify-center text-2xl mb-4">📍</div>
              <h3 className="font-black text-gray-800 text-xl mb-2">Operations Headquarters</h3>
              <p className="text-gray-500 text-sm leading-relaxed mb-3">
                <strong>Aptech Limited Headquarters</strong><br/>
                Aptech House Building, A-65, MIDC Industrial Area, Marol, Andheri East, Mumbai - 400093, Maharashtra, India.
              </p>
            </div>

            {/* Card Hotline */}
            <div className="bg-white p-8 rounded-[2rem] shadow-sm border border-gray-100 hover:shadow-lg transition-shadow duration-300">
              <div className="w-12 h-12 bg-orange-50 text-orange-500 rounded-2xl flex items-center justify-center text-2xl mb-4">📞</div>
              <h3 className="font-black text-gray-800 text-xl mb-2">Hotline</h3>
              <div className="text-gray-500 text-sm space-y-1">
                <p>Global customer support: <strong className="text-red-500">+91 1800-ABCD-MALL</strong></p>
                <p>Vietnam branch: <strong>+84 28 7300 8888</strong></p>
                <p className="text-xs text-gray-400 mt-2">*(Monday - Sunday: 09:30 - 22:00)*</p>
              </div>
            </div>

            {/* Card Email & MXH */}
            <div className="bg-white p-8 rounded-[2rem] shadow-sm border border-gray-100 hover:shadow-lg transition-shadow duration-300">
              <div className="w-12 h-12 bg-blue-50 text-blue-500 rounded-2xl flex items-center justify-center text-2xl mb-4">✉️</div>
              <h3 className="font-black text-gray-800 text-xl mb-2">Email & Partnerships</h3>
              <div className="text-gray-500 text-sm space-y-1 mb-4">
                <p>Customer support:<br/><strong>support@abcdmall.com</strong></p>
                <p>Brand partnerships:<br/><strong>partners@abcdmall.com</strong></p>
              </div>
            </div>

          </div>

          {/* CỘT PHẢI: FORM LIÊN HỆ (2 cột) */}
          <div className="lg:col-span-2 bg-white rounded-[2.5rem] shadow-xl border border-gray-100 p-8 md:p-12">
            <h2 className="text-3xl font-black text-gray-800 mb-8">Send a Message</h2>
            
            {isSubmitted ? (
              <div className="h-full min-h-[400px] flex flex-col items-center justify-center text-center animate-fade-in-up">
                <div className="w-24 h-24 bg-green-100 text-green-500 rounded-full flex items-center justify-center text-5xl mb-6 shadow-inner">
                  ✓
                </div>
                <h3 className="text-3xl font-black text-gray-800 mb-4">Message Sent Successfully!</h3>
                <p className="text-gray-500 text-lg max-w-md">
                  Thank you for reaching out. The ABCD Mall team will review your message and reply by email within 24 business hours.
                </p>
                <button 
                  onClick={() => setIsSubmitted(false)}
                  className="mt-8 text-red-500 font-bold hover:underline"
                >
                  Send another message
                </button>
              </div>
            ) : (
              <form onSubmit={handleSubmit} className="space-y-6 animate-fade-in-up">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                  <div className="space-y-2">
                    <label className="text-sm font-bold text-gray-600">Full Name <span className="text-red-500">*</span></label>
                    <input required type="text" placeholder="Enter your name" className="w-full px-5 py-4 rounded-2xl border border-gray-200 focus:outline-none focus:border-red-500 focus:ring-2 focus:ring-red-100 transition-all bg-gray-50 hover:bg-white" />
                  </div>
                  <div className="space-y-2">
                    <label className="text-sm font-bold text-gray-600">Email <span className="text-red-500">*</span></label>
                    <input required type="email" placeholder="example@email.com" className="w-full px-5 py-4 rounded-2xl border border-gray-200 focus:outline-none focus:border-red-500 focus:ring-2 focus:ring-red-100 transition-all bg-gray-50 hover:bg-white" />
                  </div>
                </div>

                <div className="space-y-2">
                  <label className="text-sm font-bold text-gray-600">Subject <span className="text-red-500">*</span></label>
                  <input required type="text" placeholder="What do you need help with?" className="w-full px-5 py-4 rounded-2xl border border-gray-200 focus:outline-none focus:border-red-500 focus:ring-2 focus:ring-red-100 transition-all bg-gray-50 hover:bg-white" />
                </div>

                <div className="space-y-2">
                  <label className="text-sm font-bold text-gray-600">Message Details <span className="text-red-500">*</span></label>
                  <textarea required rows={6} placeholder="Enter the details of your message..." className="w-full px-5 py-4 rounded-2xl border border-gray-200 focus:outline-none focus:border-red-500 focus:ring-2 focus:ring-red-100 transition-all bg-gray-50 hover:bg-white resize-none"></textarea>
                </div>

                <button type="submit" className="w-full md:w-auto px-10 py-4 rounded-2xl font-black text-white bg-gradient-to-r from-red-600 to-orange-500 hover:from-red-700 hover:to-orange-600 shadow-lg hover:shadow-xl transform hover:-translate-y-1 transition-all duration-300">
                  SEND MESSAGE
                </button>
              </form>
            )}
          </div>

        </div>

        {/* BẢN ĐỒ GOOGLE MAPS (Embed trụ sở Aptech Mumbai) */}
        <div className="w-full h-[500px] rounded-[2.5rem] overflow-hidden shadow-xl border-4 border-white relative bg-gray-200">
          {/* Iframe bản đồ hướng trực tiếp đến Aptech Ltd, Andheri East, Mumbai */}
          <iframe 
            src="https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d3769.940428514068!2d72.87229567605943!3d19.110255850928236!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x3be7c8163f91515b%3A0xc07a82c40c837bd5!2sAptech%20Limited!5e0!3m2!1svi!2s!4v1713192000000!5m2!1svi!2s" 
            width="100%" 
            height="100%" 
            style={{ border: 0 }} 
            allowFullScreen={false} 
            loading="lazy" 
            referrerPolicy="no-referrer-when-downgrade"
            title="Aptech India Headquarters Map"
          ></iframe>
        </div>

      </div>
    </div>
  );
};
