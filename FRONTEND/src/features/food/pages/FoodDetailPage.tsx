import { useParams } from "react-router-dom";
import { useEffect, useState } from "react";
import { getFoodBySlug } from "../api/foodApi";

export default function FoodDetailPage() {
  const { slug } = useParams();
  const [food, setFood] = useState<any>(null);

  useEffect(() => {
    if (slug) {
      getFoodBySlug(slug).then((res) => {
  setFood(res);
      }
      );
    }
  }, [slug]);

  if (!food) return <div className="p-10">Loading...</div>;

  return (
    <div className="bg-gray-50 min-h-screen">

      {/* BANNER */}
      <div
        className="h-[250px] bg-cover bg-center"
        style={{ backgroundImage: `url(${food.imageUrl})` }}
      >
        <div className="h-full bg-black/40 flex items-end px-16 pb-6">
          <h1 className="text-white text-3xl font-bold">
            {food.name}
          </h1>
        </div>
      </div>

      {/* CONTENT */}
      <div className="px-16 py-10 grid md:grid-cols-3 gap-10">

        <div className="md:col-span-2">
          <h2 className="text-xl font-bold mb-4">
            Giới thiệu
          </h2>

          <p className="text-gray-600 whitespace-pre-line">
            {food.description}
          </p>
        </div>

        <div className="bg-white p-6 rounded-xl shadow">
          <img
            src={food.imageUrl}
            className="w-full h-40 object-cover rounded mb-4"
          />
          <p>📍 Vincom Plaza</p>
          <p>⭐ 4.5</p>
        </div>

      </div>
    </div>
  );
}