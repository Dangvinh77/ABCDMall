// import { useNavigate } from "react-router-dom";

// export default function FoodCard({ food }: any) {
//   const navigate = useNavigate();

//   return (
//     <div
//       onClick={() => navigate(`/food/${food.slug}`)}
//       className="relative rounded-2xl overflow-hidden shadow-md cursor-pointer group"
//     >
//       <img
//         src={food.imageUrl}
//         className="w-full h-44 object-cover group-hover:scale-110 transition duration-300"
//       />

//       {/* overlay */}
//       <div className="absolute inset-0 bg-black/40 opacity-0 group-hover:opacity-100 transition flex flex-col justify-end p-4">
//         <h3 className="text-white font-bold">{food.name}</h3>
//         <p className="text-gray-200 text-sm">
//           More Details →
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
      className="rounded-2xl overflow-hidden shadow-md cursor-pointer group bg-white"
    >
      {/* IMAGE */}
      <div className="h-44 bg-gray-100 flex items-center justify-center overflow-hidden">
        <img
          src={food.imageUrl}
         // src={`http://localhost:5184${food.imageUrl}?t=${Date.now()}`}
          className="max-h-full max-w-full object-contain group-hover:scale-105 transition duration-300"
        />
      </div>

      {/* TEXT LUÔN HIỆN */}
      <div className="p-4">
        <h3 className="font-bold text-gray-800">
          {food.name}
        </h3>

        <p className="text-sm text-gray-500">
          More Details →
        </p>
      </div>
    </div>
  );
}