import { Link } from 'react-router-dom';

export const Footer = () => {
  return (
    <footer className="bg-slate-900 text-gray-400 pt-16 pb-8 border-t-[6px] border-mall-primary">
      <div className="max-w-7xl mx-auto px-6 grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-12">
        
        {/* Cột 1: Thông tin chung */}
        <div className="space-y-6">
          <Link to="/" className="inline-block">
            <h3 className="text-3xl font-black text-white tracking-tight">
              <span className="text-transparent bg-clip-text bg-gradient-to-r from-yellow-400 to-orange-500">ABCD</span> Mall
            </h3>
          </Link>
          <p className="text-sm leading-relaxed">
            Trung tâm mua sắm, ẩm thực và giải trí hàng đầu với 250,000 sq ft (23,000 m2) trải nghiệm đẳng cấp dành cho bạn và gia đình.
          </p>
        </div>

        {/* Cột 2: Khám Phá */}
        <div>
          <h4 className="text-white font-bold text-lg mb-6 uppercase tracking-wider">Khám Phá</h4>
          <ul className="space-y-3 text-sm font-medium">
            <li><Link to="/brands" className="hover:text-white transition-colors flex items-center gap-2"><span className="text-mall-primary">▸</span> Cửa hàng nổi bật</Link></li>
            <li><Link to="/food-court" className="hover:text-white transition-colors flex items-center gap-2"><span className="text-mall-primary">▸</span> Khu ẩm thực</Link></li>
            <li><Link to="/movies" className="hover:text-white transition-colors flex items-center gap-2"><span className="text-mall-primary">▸</span> Rạp chiếu phim</Link></li>
            <li><Link to="/map" className="hover:text-white transition-colors flex items-center gap-2"><span className="text-mall-primary">▸</span> Sơ đồ tầng</Link></li>
          </ul>
        </div>

        {/* Cột 3: Hỗ Trợ */}
        <div>
          <h4 className="text-white font-bold text-lg mb-6 uppercase tracking-wider">Hỗ Trợ</h4>
          <ul className="space-y-3 text-sm font-medium">
            <li><Link to="/contact" className="hover:text-white transition-colors flex items-center gap-2"><span className="text-mall-primary">▸</span> Liên hệ (Contact Us)</Link></li>
            <li><Link to="/faq" className="hover:text-white transition-colors flex items-center gap-2"><span className="text-mall-primary">▸</span> Câu hỏi thường gặp</Link></li>
            <li><Link to="/feedback" className="hover:text-white transition-colors flex items-center gap-2"><span className="text-mall-primary">▸</span> Gửi phản hồi</Link></li>
          </ul>
        </div>

        {/* Cột 4: Liên Hệ & Social */}
        <div>
          <h4 className="text-white font-bold text-lg mb-6 uppercase tracking-wider">Liên Hệ</h4>
          <ul className="space-y-4 text-sm mb-8">
            <li className="flex items-start gap-3">
              <span className="text-red-500 text-lg">📍</span> 
              <span>123 Suburban Place,<br />Mumbai</span>
            </li>
            <li className="flex items-center gap-3">
              <span className="text-red-500 text-lg">📞</span> 
              <span>+91 1800-ABCD-MALL</span>
            </li>
            <li className="flex items-center gap-3">
              <span className="text-red-500 text-lg">✉️</span> 
              <span>support@abcdmall.com</span>
            </li>
          </ul>

          {/* Social Icons (SVG chuyên nghiệp thay vì Emoji) */}
          <div className="flex gap-4">
            <a href="#" className="w-10 h-10 rounded-full bg-slate-800 flex items-center justify-center hover:bg-blue-600 hover:text-white transition-all duration-300">
              <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 24 24">
                <path fillRule="evenodd" d="M22 12c0-5.523-4.477-10-10-10S2 6.477 2 12c0 4.991 3.657 9.128 8.438 9.878v-6.987h-2.54V12h2.54V9.797c0-2.506 1.492-3.89 3.777-3.89 1.094 0 2.238.195 2.238.195v2.46h-1.26c-1.243 0-1.63.771-1.63 1.562V12h2.773l-.443 2.89h-2.33v6.988C18.343 21.128 22 16.991 22 12z" clipRule="evenodd" />
              </svg>
            </a>
            <a href="#" className="w-10 h-10 rounded-full bg-slate-800 flex items-center justify-center hover:bg-pink-600 hover:text-white transition-all duration-300">
              <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 24 24">
                <path fillRule="evenodd" d="M12.315 2c2.43 0 2.784.013 3.808.06 1.064.049 1.791.218 2.427.465a4.902 4.902 0 011.772 1.153 4.902 4.902 0 011.153 1.772c.247.636.416 1.363.465 2.427.048 1.067.06 1.407.06 4.123v.08c0 2.643-.012 2.987-.06 4.043-.049 1.064-.218 1.791-.465 2.427a4.902 4.902 0 01-1.153 1.772 4.902 4.902 0 01-1.772 1.153c-.636.247-1.363.416-2.427.465-1.067.048-1.407.06-4.123.06h-.08c-2.643 0-2.987-.012-4.043-.06-1.064-.049-1.791-.218-2.427-.465a4.902 4.902 0 01-1.772-1.153 4.902 4.902 0 01-1.153-1.772c-.247-.636-.416-1.363-.465-2.427-.047-1.024-.06-1.379-.06-3.808v-.63c0-2.43.013-2.784.06-3.808.049-1.064.218-1.791.465-2.427a4.902 4.902 0 011.153-1.772A4.902 4.902 0 015.45 2.525c.636-.247 1.363-.416 2.427-.465C8.901 2.013 9.256 2 11.685 2h.63zm-.081 1.802h-.468c-2.456 0-2.784.011-3.807.058-.975.045-1.504.207-1.857.344-.467.182-.8.398-1.15.748-.35.35-.566.683-.748 1.15-.137.353-.3.882-.344 1.857-.047 1.023-.058 1.351-.058 3.807v.468c0 2.456.011 2.784.058 3.807.045.975.207 1.504.344 1.857.182.466.399.8.748 1.15.35.35.683.566 1.15.748.353.137.882.3 1.857.344 1.054.048 1.37.058 4.041.058h.08c2.597 0 2.917-.01 3.96-.058.976-.045 1.505-.207 1.858-.344.466-.182.8-.398 1.15-.748.35-.35.566-.683.748-1.15.137-.353.3-.882.344-1.857.048-1.055.058-1.37.058-4.041v-.08c0-2.597-.01-2.917-.058-3.96-.045-.976-.207-1.505-.344-1.858a3.097 3.097 0 00-.748-1.15 3.098 3.098 0 00-1.15-.748c-.353-.137-.882-.3-1.857-.344-1.023-.047-1.351-.058-3.807-.058zM12 6.865a5.135 5.135 0 110 10.27 5.135 5.135 0 010-10.27zm0 1.802a3.333 3.333 0 100 6.666 3.333 3.333 0 000-6.666zm5.338-3.205a1.2 1.2 0 110 2.4 1.2 1.2 0 010-2.4z" clipRule="evenodd" />
              </svg>
            </a>
            <a href="#" className="w-10 h-10 rounded-full bg-slate-800 flex items-center justify-center hover:bg-gray-700 hover:text-white transition-all duration-300">
              <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 24 24">
                <path d="M18.901 1.153h3.68l-8.04 9.19L24 22.846h-7.406l-5.8-7.584-6.638 7.584H.474l8.6-9.83L0 1.154h7.594l5.243 6.932ZM17.61 20.644h2.039L6.486 3.24H4.298Z" />
              </svg>
            </a>
          </div>
        </div>
      </div>

      {/* Copyright & Legal */}
      <div className="max-w-7xl mx-auto px-6 mt-16 pt-8 border-t border-slate-800 flex flex-col md:flex-row justify-between items-center text-xs font-medium">
        <p>&copy; 2026 ABCD Developers Pvt Ltd. All rights reserved.</p>
        <div className="flex gap-6 mt-4 md:mt-0">
          <Link to="/privacy" className="hover:text-white transition-colors">Privacy Policy</Link>
          <Link to="/terms" className="hover:text-white transition-colors">Terms of Service</Link>
        </div>
      </div>
    </footer>
  );
};