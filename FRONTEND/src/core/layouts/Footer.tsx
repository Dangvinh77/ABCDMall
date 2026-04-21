import { Link } from 'react-router-dom';

export const Footer = () => {
  return (
    <footer className="bg-mall-dark text-gray-300 pt-16 pb-8 border-t-4 border-mall-primary">
      <div className="max-w-7xl mx-auto px-4 grid grid-cols-1 md:grid-cols-4 gap-10">
        
        {/* Cột 1: Thông tin chung */}
        <div className="space-y-4">
          <h3 className="text-2xl font-extrabold text-white">
            <span className="text-mall-accent">ABCD</span> Mall
          </h3>
          <p className="text-sm leading-relaxed">
            A leading shopping, dining, and entertainment destination with more than 250,000 m2 of premium experiences for you and your family.
          </p>
        </div>

        {/* Cột 2: Liên kết nhanh */}
        <div>
          <h4 className="text-white font-bold text-lg mb-4">Explore</h4>
          <ul className="space-y-2 text-sm">
            <li><Link to="/shops" className="hover:text-mall-accent transition-colors">Featured Stores</Link></li>
            <li><Link to="/food-court" className="hover:text-mall-accent transition-colors">Food Court</Link></li>
            <li><Link to="/movies" className="hover:text-mall-accent transition-colors">Cinema</Link></li>
            <li><Link to="/map" className="hover:text-mall-accent transition-colors">Floor Map</Link></li>
          </ul>
        </div>

        {/* Cột 3: Hỗ trợ khách hàng */}
        <div>
          <h4 className="text-white font-bold text-lg mb-4">Support</h4>
          <ul className="space-y-2 text-sm">
            <li><Link to="/contact" className="hover:text-mall-accent transition-colors">Contact Us</Link></li>
            <li><Link to="/faq" className="hover:text-mall-accent transition-colors">Frequently Asked Questions</Link></li>
            <li><Link to="/feedback" className="hover:text-mall-accent transition-colors">Send Feedback</Link></li>
          </ul>
        </div>

        {/* Cột 4: Thông tin liên hệ & Social */}
        <div>
          <h4 className="text-white font-bold text-lg mb-4">Contact</h4>
          <ul className="space-y-2 text-sm mb-6">
            <li>📍 123 Suburban Place, Mumbai</li>
            <li>📞 +91 1800-ABCD-MALL</li>
            <li>✉️ support@abcdmall.com</li>
          </ul>
          {/* Nút Social Media mộc mạc không cần cài thư viện icon */}
          <div className="flex space-x-4 text-xl">
            <a href="#" className="w-10 h-10 bg-gray-700 rounded-full flex items-center justify-center hover:bg-blue-600 transition-colors">📘</a>
            <a href="#" className="w-10 h-10 bg-gray-700 rounded-full flex items-center justify-center hover:bg-pink-600 transition-colors">📸</a>
            <a href="#" className="w-10 h-10 bg-gray-700 rounded-full flex items-center justify-center hover:bg-blue-400 transition-colors">🐦</a>
          </div>
        </div>
      </div>

      <div className="max-w-7xl mx-auto px-4 mt-12 pt-8 border-t border-gray-700 text-center text-sm flex flex-col md:flex-row justify-between items-center">
        <p>&copy; 2026 ABCD Developers Pvt Ltd. All rights reserved.</p>
        <div className="space-x-4 mt-4 md:mt-0">
          <Link to="/privacy" className="hover:text-white">Privacy Policy</Link>
          <Link to="/terms" className="hover:text-white">Terms of Service</Link>
        </div>
      </div>
    </footer>
  );
};
