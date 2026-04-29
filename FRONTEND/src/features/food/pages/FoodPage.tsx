import { useFood } from "../hooks/useFood";
import { useState, useMemo } from "react";
import { useNavigate } from "react-router-dom";
import { getImageUrl } from "../../../core/utils/image";

export default function FoodPage() {
  const { foods, loading, error } = useFood();
  const navigate = useNavigate();
  const [keyword, setKeyword] = useState("");

  // Sử dụng useMemo để tối ưu hiệu năng khi filter
  const filteredFoods = useMemo(() => {
    return (foods || []).filter((f) =>
      f.name?.toLowerCase().includes(keyword.toLowerCase())
    );
  }, [foods, keyword]);

  return (
    <div className="bg-gray-50 min-h-screen">
      {/* HERO SECTION */}
      <div className="relative h-[380px] overflow-hidden">
        <img
          src="https://images.unsplash.com/photo-1504674900247-0877df9cc836"
          className="absolute inset-0 w-full h-full object-cover scale-105"
          alt="Food Court Hero"
        />
        <div className="absolute inset-0 bg-black/60" />
        <div className="relative z-10 h-full flex flex-col items-center justify-center text-center text-white px-4">
          <h1 className="text-5xl font-bold text-yellow-400 drop-shadow-md">
            Khu Ăn Uống
          </h1>
          <p className="mt-3 text-lg opacity-90">
            Nhiều cửa hàng ẩm thực đa dạng, tất cả ở một điểm đến duy nhất.
          </p>
        </div>
      </div>

      {/* TRẠNG THÁI ERROR */}
      {error ? (
        <div className="px-6 md:px-16 mt-16 pb-16">
          <div className="bg-white rounded-2xl border-2 border-red-100 p-12 text-center flex flex-col items-center gap-4 shadow-sm">
            <span className="text-6xl">⚠️</span>
            <h3 className="text-2xl font-bold text-red-600">Lỗi tải dữ liệu!</h3>
            <p className="text-gray-600 max-w-md">{error}</p>
            <button 
              onClick={() => window.location.reload()}
              className="mt-4 px-8 py-2 bg-red-500 text-white font-bold rounded-full hover:bg-red-600 transition-all shadow-lg active:scale-95"
            >
              Thử lại
            </button>
          </div>
        </div>
      ) : loading ? (
        /* TRẠNG THÁI LOADING */
        <div className="px-6 md:px-16 mt-16 pb-16 text-center">
          <div className="flex flex-col items-center gap-4">
            <div className="w-12 h-12 border-4 border-gray-200 border-t-red-500 rounded-full animate-spin"></div>
            <p className="text-gray-400 text-lg font-medium">Đang tải menu...</p>
          </div>
        </div>
      ) : (
        <>
          {/* THANH TÌM KIẾM (FLOATING) */}
          <div className="px-6 md:px-20 relative z-20 -mt-10">
            <div className="bg-white rounded-2xl shadow-xl p-2 max-w-2xl mx-auto border border-gray-100">
              <div className="flex items-center px-4">
                <span className="text-xl">🔍</span>
                <input
                  value={keyword}
                  onChange={(e) => setKeyword(e.target.value)}
                  placeholder="Tìm mon ăn..."
                  className="w-full p-4 outline-none text-gray-700 bg-transparent"
                />
              </div>
            </div>
          </div>

          {/* SECTION ƯU ĐÃI */}
          <div className="px-6 md:px-16 mt-14">
            <h2 className="text-red-500 text-2xl font-bold mb-6 flex items-center gap-2">
              <span className="bg-red-500 w-2 h-8 rounded-full"></span>
              Giảm Giá Đặc Biệt
            </h2>
            <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-6 flex flex-col md:flex-row justify-between items-start md:items-center hover:shadow-md transition gap-4">
              <div>
                <p className="font-bold text-xl text-gray-800">Cơm trưa lẻ</p>
                <p className="text-gray-500">Cơm lẻ Babushka và Boba Bella</p>
              </div>
              <div className="bg-gray-50 px-4 py-2 rounded-lg">
                <p className="text-gray-600 font-medium text-sm">
                  📅 Mỗi ngày | 11:30 - 16:00
                </p>
              </div>
            </div>
          </div>

          {/* SECTION GIỚI THIỆU */}
          <div className="px-6 md:px-16 mt-16">
            <h2 className="text-red-500 text-2xl font-bold mb-4 flex items-center gap-2">
              <span className="bg-red-500 w-2 h-8 rounded-full"></span>
              Về Khu Ăn Uống
            </h2>
            <p className="text-gray-600 max-w-3xl text-lg leading-relaxed">
              Một loạt đa dạng các nền ẩm thực quốc tế hội tụ tại Khu Ăn Uống.
              Tại đây, bạn có thể thưởng thức nhiều món ăn ngon trong một không gian hiện đại và thoải mái.
            </p>
          </div>

          {/* SECTION MENU */}
          <div className="px-6 md:px-16 mt-16 pb-20">
            <h2 className="text-red-500 text-2xl font-bold mb-8 flex items-center gap-2">
              <span className="bg-red-500 w-2 h-8 rounded-full"></span>
              Thực Đơn Của Chúng Tôi
            </h2>

            {filteredFoods.length > 0 ? (
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-2 gap-6">
                {filteredFoods.map((food) => (
                  <div
                    key={food.id}
                    onClick={() => navigate(`/food/${food.slug}`)}
                    className="group flex items-center gap-5 p-4 rounded-2xl border border-gray-100
                               bg-white hover:shadow-xl hover:-translate-y-1 
                               transition-all duration-300 cursor-pointer"
                  >
                    {/* IMAGE CONTAINER */}
                    <div className="w-24 h-24 bg-gray-50 rounded-xl flex-shrink-0 flex items-center justify-center overflow-hidden border border-gray-50">
                      <img
                        src={getImageUrl(food.imageUrl)}
                        alt={food.name}
                        className="w-full h-full object-cover group-hover:scale-110 transition duration-500"
                        onError={(e) => {
                          (e.target as HTMLImageElement).src = "https://via.placeholder.com/150?text=No+Image";
                        }}
                      />
                    </div>

                    {/* INFO */}
                    <div className="flex-1">
                      <h3 className="font-bold text-gray-800 text-lg group-hover:text-red-600 transition">
                        {food.name}
                      </h3>
                      <p className="text-sm text-gray-500 mt-2 flex items-center gap-1">
                        Xem chi tiết <span className="group-hover:translate-x-2 transition-transform">→</span>
                      </p>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              /* TRẠNG THÁI KHÔNG TÌM THẤY MÓN */
              <div className="bg-white rounded-2xl border-2 border-dashed border-gray-200 p-16 text-center flex flex-col items-center gap-4">
                <span className="text-6xl opacity-50">🍽️</span>
                <p className="text-gray-500 text-xl font-medium">Không tìm thấy mon ăn phù hợp với "{keyword}"</p>
                <button 
                  onClick={() => setKeyword("")}
                  className="text-red-500 hover:underline"
                >
                  Xóa tìm kiếm
                </button>
              </div>
            )}
          </div>
        </>
      )}
    </div>
  );
}