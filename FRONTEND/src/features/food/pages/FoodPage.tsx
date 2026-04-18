// import { useFood } from "../hooks/useFood";
// import FoodCard from "../components/FoodCard";
// import { useState } from "react";
// import { uploadImage } from "../../../core/api/api";

// export default function FoodPage() {
//   const { foods } = useFood();
//   const [keyword, setKeyword] = useState("");

//   const filteredFoods = (foods || []).filter((f) =>
//     f.name?.toLowerCase().includes(keyword.toLowerCase())
//   );
//   const [file, setFile] = useState<File | null>(null);
//   const [imageUrl, setImageUrl] = useState("");
//   const handleUpload = async (e: any) => {
//     const file = e.target.files[0];

//     const res = await uploadImage(file);

//     setImageUrl(res.imageUrl);
//   };

//   return (
//     <div className="bg-gray-50 min-h-screen">
//       <div className="h-100 bg-[url('https://images.unsplash.com/photo-1504674900247-0877df9cc836')] bg-cover bg-center flex items-center px-10">
//         <h1 className="text-4xl font-bold text-white drop-shadow-lg">
//           FOOD COURT
//         </h1>
//       </div>

//       {/* SEARCH */}
//       {/* <div className="px-16 mt-4">
//         <input type="file" onChange={handleUpload} />
//       </div> */}
//       {imageUrl && (
//         <div className="px-16 mt-4">
//           <img
//             src={imageUrl}
//             className="h-32 rounded shadow"
//           />
//         </div>
//       )}
//       <div className="px-16 mt-6">
//         <input
//           value={keyword}
//           onChange={(e) => setKeyword(e.target.value)}
//           placeholder="🔍 Find dishes..."
//           className="w-full p-4 rounded-xl border"
//         />
//       </div>

//       {/* GRID */}
//       <div className="px-16 py-10 grid grid-cols-2 md:grid-cols-3 lg:grid-cols-5 gap-8">
//         {filteredFoods.map((food) => (
//           <FoodCard key={food.id} food={food} />
//         ))}
//       </div>

//       {filteredFoods.length === 0 && (
//         <p className="text-center text-gray-500">
//           No dishes found ! 
//         </p>
//       )}
//     </div>
//   );
// }


import { useFood } from "../hooks/useFood";
import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { getImageUrl } from "../../../core/utils/image";

export default function FoodPage() {
  const { foods } = useFood();
  const navigate = useNavigate();

  const [keyword, setKeyword] = useState("");

  const filteredFoods = (foods || []).filter((f) =>
    f.name?.toLowerCase().includes(keyword.toLowerCase())
  );


//   return (
//     <div className="bg-gray-50 min-h-screen">

// {/* HERO */}
// <div className="relative h-[350px] overflow-hidden mb-16">
//   <img
//     src="https://images.unsplash.com/photo-1504674900247-0877df9cc836"
//     className="absolute inset-0 w-full h-full object-cover"
//   />
//   <div className="absolute inset-0 bg-black/50" />

//   <div className="relative z-10 h-full flex flex-col items-center justify-center text-center text-white">
//     <h1 className="text-5xl font-bold text-yellow-400">
//       Food Court
//     </h1>
//     <p className="mt-2 text-sm">
//       Sáu (6) quầy ẩm thực khác nhau chỉ trong một điểm đến
//     </p>
//   </div>
// </div>

// {/* SEARCH FIX */}
// <div className="px-16 -mt-20 relative z-20">
//   <div className="bg-white rounded-2xl shadow-xl p-2 max-w-4xl mx-auto">
    
//     <div className="flex items-center px-3">
//       <span className="mr-2 text-gray-400">🔍</span>

//       <input
//         value={keyword}
//         onChange={(e) => setKeyword(e.target.value)}
//         placeholder="Find dishes..."
//         className="w-full p-4 outline-none rounded-2xl"
//       />
//     </div>

//   </div>
// </div>

//       {/* ƯU ĐÃI */}
//       <div className="px-16 mt-10">
//         <h2 className="text-red-500 text-2xl font-bold mb-4">
//           Ưu Đãi Hấp Dẫn
//         </h2>

//         <div className="bg-white rounded-xl shadow p-4 flex justify-between items-center">
//           <div>
//             <p className="font-semibold">Bữa trưa gọi món</p>
//             <p className="text-gray-500 text-sm">
//               Babushka and Boba Bella A la carte
//             </p>
//           </div>
//           <p className="text-gray-400 text-sm">
//             Hàng ngày | 11:30 - 16:00
//           </p>
//         </div>
//       </div>

//       {/* GIỚI THIỆU */}
//       <div className="px-16 mt-12">
//         <h2 className="text-red-500 text-2xl font-bold mb-4">
//           Về Food Court
//         </h2>

//         <p className="text-gray-600 max-w-3xl leading-7">
//           Đa dạng các hương vị ẩm thực quốc tế tập trung tại Food Court.
//           Bạn có thể thưởng thức nhiều món ăn hấp dẫn trong một không gian hiện đại và tiện nghi.
//         </p>
//       </div>

//       {/* MENU */}
//       <div className="px-16 mt-12">
//         <h2 className="text-red-500 text-2xl font-bold mb-6">
//           Thực Đơn
//         </h2>

//         <div className="grid md:grid-cols-2 gap-6">

//           {filteredFoods.map((food) => (
//             <div
//               key={food.id}
//               onClick={() => navigate(`/food/${food.slug}`)}
//               className="flex items-center gap-4 p-4 rounded-2xl 
//                          border hover:shadow-md hover:bg-gray-50 
//                          cursor-pointer transition"
//             >
//               {/* IMAGE */}
//               <div className="w-20 h-20 bg-gray-100 rounded-xl flex items-center justify-center overflow-hidden">
//                 <img
//                // src={food.imageUrl}
//                // src={`http://localhost:5184${food.imageUrl}?t=${Date.now()}`}
//                src={`${getImageUrl(food.imageUrl)}?t=${Date.now()}`}
//                   className="max-w-full max-h-full object-contain"
//                 />
//               </div>

//               {/* INFO */}
//               <div className="flex-1">
//                 <h3 className="font-semibold text-gray-800">
//                   {food.name}
//                 </h3>
//                 <p className="text-sm text-gray-500">
//                   More Details →
//                 </p>
//               </div>

             
//             </div>
//           ))}

//         </div>
//       </div>

//       {/* EMPTY */}
//       {filteredFoods.length === 0 && (
//         <p className="text-center text-gray-500 mt-10">
//           No dishes found!
//         </p>
//       )}
//     </div>
//   );
// }

return (
  <div className="bg-gray-50 min-h-screen">

    {/* HERO */}
    <div className="relative h-[380px] overflow-hidden">
      <img
        src="https://images.unsplash.com/photo-1504674900247-0877df9cc836"
        className="absolute inset-0 w-full h-full object-cover scale-105"
      />

      <div className="absolute inset-0 bg-black/60" />

      <div className="relative z-10 h-full flex flex-col items-center justify-center text-center text-white px-4">
        <h1 className="text-5xl font-bold text-yellow-400 drop-shadow">
          Food Court
        </h1>
        <p className="mt-3 text-sm opacity-90">
          More different culinary stalls, all in a single destination.
        </p>
      </div>
    </div>

    {/* SEARCH FLOAT */}
    <div className="px-16 md:px-20 relative z-20">
      <div className="bg-white rounded-2xl shadow-xl p-2 max-w-1xl mx-auto">
        <div className="flex items-center px-4">
          <span className="mr-2 text-gray-400">🔍</span>

          <input
            value={keyword}
            onChange={(e) => setKeyword(e.target.value)}
            placeholder="Find dishes..."
            className="w-full p-4 outline-none text-gray-700"
          />
        </div>
      </div>
    </div>

    {/* ƯU ĐÃI */}
    <div className="px-6 md:px-16 mt-14">
      <h2 className="text-red-500 text-2xl font-bold mb-6">
        Special Discount
      </h2>

      <div className="bg-white rounded-2xl shadow-md p-5 flex justify-between items-center hover:shadow-lg transition">
        <div>
          <p className="font-semibold text-gray-800">
            A-la-carte lunch
          </p>
          <p className="text-gray-500 text-sm">
            Babushka and Boba Bella A la carte
          </p>
        </div>

        <p className="text-gray-400 text-sm">
          Daily | 11:30 - 16:00
        </p>
      </div>
    </div>

    {/* GIỚI THIỆU */}
    <div className="px-6 md:px-16 mt-16">
      <h2 className="text-red-500 text-2xl font-bold mb-4">
        About Food Court
      </h2>

      <p className="text-gray-600 max-w-1xl leading-7">
        A diverse array of international cuisines comes together at the Food Court.
        Here, you can enjoy a variety of delicious dishes in a modern and comfortable setting.
      </p>
    </div>

    {/* MENU */}
    <div className="px-6 md:px-16 mt-16 pb-16">
      <h2 className="text-red-500 text-2xl font-bold mb-8">
        Menu
      </h2>

      <div className="grid md:grid-cols-2 gap-6">

        {filteredFoods.map((food) => (
          <div
            key={food.id}
            onClick={() => navigate(`/food/${food.slug}`)}
            className="group flex items-center gap-5 p-5 rounded-2xl border 
                       bg-white hover:shadow-xl hover:-translate-y-1 
                       transition cursor-pointer"
          >

            {/* IMAGE */}
            <div className="w-20 h-20 bg-gray-100 rounded-xl flex items-center justify-center overflow-hidden">
              <img
                src={`${getImageUrl(food.imageUrl)}?t=${Date.now()}`}
                
                className="max-w-full max-h-full object-contain 
                           group-hover:scale-110 transition"
              />
            </div>

            {/* INFO */}
            <div className="flex-1">
              <h3 className="font-semibold text-gray-800 text-lg">
                {food.name}
              </h3>

              <p className="text-sm text-gray-500 mt-1 group-hover:text-red-500 transition">
                More Details →
              </p>
            </div>

          </div>
        ))}

      </div>
    </div>

    {/* EMPTY */}
    {filteredFoods.length === 0 && (
      <p className="text-center text-gray-500 mt-10">
        No dishes found!
      </p>
    )}
  </div>
);}