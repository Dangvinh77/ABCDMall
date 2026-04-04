
// import { useFood } from "../hooks/useFood";
// import FoodCard from "../components/FoodCard";
// import { useState } from "react";
// import FoodDetailModal from "../components/FoodDetailModal";

// export default function FoodPage() {
//   const { foods } = useFood();

//   const [keyword, setKeyword] = useState("");
//   const [selectedFood, setSelectedFood] = useState<any>(null);

//   // 🔥 realtime filter
//   const filteredFoods = foods.filter((f) =>
//     f.name.toLowerCase().includes(keyword.toLowerCase())
//   );

//   return (
    
// <div className="bg-gray-100 min-h-screen">

//      {/* HERO */}
//       <div className="h-56 bg-[url('https://images.unsplash.com/photo-1504674900247-0877df9cc836')] bg-cover bg-center flex items-center px-16">
//        <h1 className="text-4xl font-bold text-white drop-shadow-lg">
//           FOOD COURT
//         </h1>
//        </div>
//       {/* SEARCH */}
//       <div className="bg-white shadow-md -mt-10 mx-16 p-6 rounded-xl flex gap-4 items-center">
//         <input
//           value={keyword}
//           onChange={(e) => setKeyword(e.target.value)}
//           placeholder="Tìm món ăn..."
//           className="flex-1 border px-4 py-3 rounded-lg focus:outline-none"
//         />
//       </div>

//       {/* GRID */}
//       <div className="px-16 py-10 grid grid-cols-2 md:grid-cols-3 lg:grid-cols-5 gap-6">
//         {filteredFoods.map((food) => (
//           <FoodCard
//             key={food.id}
//             food={food}
//             onClick={() => setSelectedFood(food)}
//           />
//         ))}
//       </div>

//       {/* MODAL */}
//       {selectedFood && (
//         <FoodDetailModal
//           food={selectedFood}
//           onClose={() => setSelectedFood(null)}
//         />
//       )}
//     </div>
//   );
// }


import { useFood } from "../hooks/useFood";
import FoodCard from "../components/FoodCard";
import { useState } from "react";

export default function FoodPage() {
  const { foods } = useFood();
  const [keyword, setKeyword] = useState("");

  const filteredFoods = (foods || []).filter((f) =>
    f.name?.toLowerCase().includes(keyword.toLowerCase())
  );

  return (
    <div className="bg-gray-50 min-h-screen">
 <div className="h-56 bg-[url('https://images.unsplash.com/photo-1504674900247-0877df9cc836')] bg-cover bg-center flex items-center px-16">
        <h1 className="text-4xl font-bold text-white drop-shadow-lg">
          FOOD COURT
        </h1>
       </div>
      {/* HEADER */}
      {/* <div className="bg-white shadow-sm py-6 px-16">
        <h1 className="text-3xl font-bold">🍔 Food Court</h1>
      </div> */}

      {/* SEARCH */}
      <div className="px-16 mt-6">
        <input
          value={keyword}
          onChange={(e) => setKeyword(e.target.value)}
          placeholder="🔍 Tìm món..."
          className="w-full p-4 rounded-xl border"
        />
      </div>

      {/* GRID */}
      <div className="px-16 py-10 grid grid-cols-2 md:grid-cols-3 lg:grid-cols-5 gap-8">
        {filteredFoods.map((food) => (
          <FoodCard key={food.id} food={food} />
        ))}
      </div>

      {filteredFoods.length === 0 && (
        <p className="text-center text-gray-500">
          Không tìm thấy món 😢
        </p>
      )}
    </div>
  );
}