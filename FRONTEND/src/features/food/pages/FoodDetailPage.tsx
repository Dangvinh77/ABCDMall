// import { useParams } from "react-router-dom";
// import { useEffect, useState } from "react";
// import { getFoodBySlug } from "../api/foodApi";

// export default function FoodDetailPage() {
//   const { slug } = useParams();
//   const [food, setFood] = useState<any>(null);

//   useEffect(() => {
//     if (slug) {
//       getFoodBySlug(slug).then((res) => {
//   setFood(res);
//       }
//       );
//     }
//   }, [slug]);

//   useEffect(() => {
//   if (food) {
//     document.title = food.name + " | ABCD Mall";

//     const meta = document.querySelector("meta[name='description']");
//     if (meta) {
//       meta.setAttribute("content", food.description);
//     }
//   }
// }, [food]);

//   if (!food) return <div className="p-10">Loading...</div>;

//   return (
//     <div className="bg-gray-50 min-h-screen">

//       {/* BANNER */}
//       <div
//         className="h-[250px] bg-cover bg-center"
//         style={{ backgroundImage: `url(${food.imageUrl})` }}
//       >
//         <div className="h-full bg-black/40 flex items-end px-16 pb-6">
//           <h1 className="text-white text-3xl font-bold">
//             {food.name}
//           </h1>
//         </div>
//       </div>

//       {/* CONTENT */}
//       <div className="px-16 py-10 grid md:grid-cols-3 gap-10">

//         <div className="md:col-span-2">
//           <h2 className="text-xl font-bold mb-4">
//             Giới thiệu
//           </h2>

//           <p className="text-gray-600 whitespace-pre-line">
//             {food.description}
//           </p>
//         </div>

//         <div className="bg-white p-6 rounded-xl shadow">
//           <img
//             src={food.imageUrl}
//             className="w-full h-40 object-cover rounded mb-4"
//           />
//           <p>📍 ABCD Mall</p>
//           <p>⭐ 4.5</p>
//         </div>

//       </div>
//     </div>
//   );
// }

import { useParams } from "react-router-dom";
import { useEffect, useState } from "react";
import { getFoodBySlug } from "../api/foodApi";
import { getImageUrl } from "../../../core/utils/image";

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

  useEffect(() => {
  if (food) {
    document.title = food.name + " | ABCD Mall";

    const meta = document.querySelector("meta[name='description']");
    if (meta) {
      meta.setAttribute("content", food.description);
    }
  }
}, [food]);

  if (!food) return <div className="p-10">Loading...</div>;

return (
  <div className="bg-gray-50 min-h-screen">

    {/* HERO BANNER */}
{/* HERO BANNER FIX */}
<div className="relative h-[320px] bg-gray-200 overflow-hidden">

  {/* BACKGROUND (BLUR CHO ĐẸP) */}
  <img
   //src={food.imageUrl}
    //src={`http://localhost:5184${food.imageUrl}?t=${Date.now()}`}
    src={`${getImageUrl(food.imageUrl)}?t=${Date.now()}`}
    className="absolute inset-0 w-full h-full object-cover blur-md scale-110"
  />

  {/* OVERLAY */}
  <div className="absolute inset-0 bg-black/50" />

  {/* CONTENT */}
  <div className="relative h-full flex flex-col justify-center px-16 text-white">

    <p className="text-sm opacity-80 mb-2">
      Home Page / Food Court / {food.name}
    </p>

    <h1 className="text-4xl font-bold drop-shadow">
      {food.name}
    </h1>

  </div>

  {/* LOGO KHÔNG BỊ BỂ */}
  {/* <div className="absolute right-16 bottom-[-40px] bg-white p-4 rounded-xl shadow-lg">
    <img
      src={food.imageUrl}
      className="h-20 w-20 object-contain"
    />
  </div> */}

</div>

    {/* MAIN */}
    <div className="px-16 py-12 grid md:grid-cols-3 gap-12">

      {/* LEFT CONTENT */}
      <div className="md:col-span-2">

        <h2 className="text-2xl font-bold mb-6">
          Overview
        </h2>

        <p className="text-gray-700 whitespace-pre-line leading-7 text-[15px]">
          {food.description || "No description available"}
        </p>

      </div>

      {/* RIGHT SIDEBAR */}
      <div className="bg-white p-6 rounded-2xl shadow-lg">

        {/* IMAGE FIX KHÔNG CROP */}
        <div className="h-44 bg-gray-100 flex items-center justify-center rounded mb-4">
          <img
            //src={food.imageUrl}
            //src={`http://localhost:5184${food.imageUrl}?t=${Date.now()}`}
            src={`${getImageUrl(food.imageUrl)}?t=${Date.now()}`}
            className="max-h-full max-w-full object-contain"
          />
        </div>

        <div className="space-y-2 text-gray-600">
          <p>📍 ABCD Mall</p>
          <p>⭐ 4.5 (100+ reviews)</p>
          <p>⏰ 08:00 - 22:00</p>
        </div>

        <button className="mt-6 w-full bg-red-500 text-white py-3 rounded-xl hover:bg-red-600 transition">
          View Menu
        </button>

      </div>

    </div>
  </div>
);
}
