// type Props = {
//   food: any;
// };

// export default function FoodCard({ food }: Props) {
//   return (
//     <div className="bg-white rounded-2xl shadow-md overflow-hidden hover:shadow-xl transition duration-300 cursor-pointer group">
      
//       <img
//         src={food.imageUrl || "https://via.placeholder.com/300"}
//         alt={food.name}
//         className="w-full h-48 object-cover group-hover:scale-105 transition duration-300"
//       />

//       <div className="p-4">
//         <h2 className="text-lg font-semibold">{food.name}</h2>

//         <p className="text-red-500 font-bold mt-2">
//           {food.price} VND
//         </p>

//         <button className="mt-3 w-full bg-orange-500 text-white py-2 rounded-lg hover:bg-orange-600">
//           Add to cart
//         </button>
//       </div>
//     </div>
//   );
// }

// type Props = {
//   food: any;
// };

// export default function FoodCard({ food }: Props) {
//   return (
//     <div className="bg-white border rounded-lg p-6 text-center hover:shadow-md transition relative">
      
//       {/* HEART */}
//       <div className="absolute top-3 right-3 text-gray-400 hover:text-red-500 cursor-pointer">
//         ❤️
//       </div>

//       {/* LOGO */}
//       <img
//         src={food.imageUrl || "https://via.placeholder.com/150"}
//         className="h-24 mx-auto object-contain"
//       />

//       {/* NAME */}
//       <p className="mt-4 font-semibold text-sm uppercase">
//         {food.name}
//       </p>

//       {/* PRICE */}
//       <p className="text-red-500 text-sm mt-1">
//         {food.price} VND
//       </p>
//     </div>
//   );
// }


// type Props = {
//   food: any;
// };

// export default function FoodCard({ food }: Props) {
//   return (
//     <div className="bg-white rounded-xl border p-5 text-center hover:shadow-lg transition duration-300 group relative">

//       {/* HEART */}
//       <div className="absolute top-3 right-3 text-gray-300 group-hover:text-red-500 cursor-pointer">
//         ♥
//       </div>

//       {/* IMAGE */}
//       <div className="h-24 flex items-center justify-center mb-3">
//         <img
//           src={food.imageUrl || "https://via.placeholder.com/150"}
//           className="max-h-full object-contain group-hover:scale-110 transition"
//         />
//       </div>

//       {/* NAME */}
//       <p className="font-semibold text-sm uppercase tracking-wide">
//         {food.name}
//       </p>

//       {/* PRICE */}
//       <p className="text-red-500 font-bold mt-1">
//         {food.price} VND
//       </p>

//       {/* HOVER BUTTON */}
//       <button className="mt-3 opacity-0 group-hover:opacity-100 bg-black text-white px-4 py-1 text-sm rounded transition">
//         Xem chi tiết
//       </button>
//     </div>
//   );
// }


// type Props = {
//   food: any;
//   onClick?: () => void;
// };

// export default function FoodCard({ food, onClick }: Props) {
//   return (
//     <div
//       onClick={onClick}
//       className="bg-white rounded-xl border p-5 text-center hover:shadow-lg cursor-pointer transition"
//     >
//       <img
//         src={food.imageUrl}
//         className="h-24 mx-auto object-contain mb-3"
//       />

//       <p className="font-semibold text-sm uppercase">{food.name}</p>

//       <p className="text-red-500 mt-1">{food.price} VND</p>
//     </div>
//   );
// }


// import { useNavigate } from "react-router-dom";

// export default function FoodCard({ food }: any) {
//   const navigate = useNavigate();

//   return (
//     <div
//       onClick={() => navigate(`/food/${food.id}`)}
//       className="relative rounded-2xl overflow-hidden shadow-md cursor-pointer group"
//     >
//       {/* IMAGE */}
//       <img
//         src={food.imageUrl || "https://via.placeholder.com/300"}
//         className="w-full h-44 object-cover transition duration-300 group-hover:scale-110"
//       />

//       {/* OVERLAY */}
//       <div className="absolute inset-0 bg-black/40 opacity-0 group-hover:opacity-100 transition flex flex-col justify-end p-4">
        
//         <h3 className="text-white font-bold text-lg">
//           {food.name}
//         </h3>

//         <p className="text-gray-200 text-sm">
//           Xem chi tiết cửa hàng →
//         </p>

//       </div>
//     </div>
//   );
// }

import { useNavigate } from "react-router-dom";

export default function FoodCard({ food }: any) {
  const navigate = useNavigate();

  return (
    <div
      onClick={() => navigate(`/food/${food.slug}`)}
      className="relative rounded-2xl overflow-hidden shadow-md cursor-pointer group"
    >
      <img
        src={food.imageUrl}
        className="w-full h-44 object-cover group-hover:scale-110 transition duration-300"
      />

      {/* overlay */}
      <div className="absolute inset-0 bg-black/40 opacity-0 group-hover:opacity-100 transition flex flex-col justify-end p-4">
        <h3 className="text-white font-bold">{food.name}</h3>
        <p className="text-gray-200 text-sm">
          Xem chi tiết →
        </p>
      </div>
    </div>
  );
}