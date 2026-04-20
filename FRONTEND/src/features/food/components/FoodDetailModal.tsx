// type Props = {
//   food: any;
//   onClose: () => void;
// };

// export default function FoodDetailModal({ food, onClose }: Props) {
//   return (
//     <div className="fixed inset-0 bg-black/50 flex justify-center items-center z-50">

//       <div className="bg-white rounded-xl w-[400px] p-6 relative animate-fadeIn">

//         {/* CLOSE */}
//         <button
//           onClick={onClose}
//           className="absolute top-3 right-3 text-gray-500"
//         >
//           ✕
//         </button>

//         {/* IMAGE */}
//         <img
//           src={food.imageUrl}
//           className="w-full h-48 object-cover rounded-lg mb-4"
//         />

//         {/* NAME */}
//         <h2 className="text-xl font-bold">{food.name}</h2>

//         {/* PRICE */}
//         <p className="text-red-500 font-bold mt-2">
//           {food.price} VND
//         </p>

//         {/* DESC (fake) */}
//         <p className="text-gray-600 mt-3 text-sm">
//           Món ăn ngon tại food court. Đậm vị, chất lượng cao.
//         </p>

//       </div>
//     </div>
//   );
// }


type Props = {
  food: any;
  onClose: () => void;
};

export default function FoodDetailModal({ food, onClose }: Props) {
  return (
    <div className="fixed inset-0 bg-black/60 z-50 overflow-auto">

      {/* CONTAINER */}
      <div className="bg-white min-h-screen">

        {/* HEADER IMAGE (BANNER) */}
        <div
          className="h-[260px] bg-cover bg-center relative"
          style={{ backgroundImage: `url(${food.imageUrl})` }}
        >
          <div className="absolute inset-0 bg-black/40" />

          {/* CLOSE */}
          <button
            onClick={onClose}
            className="absolute top-5 right-6 text-white text-2xl"
          >
            ✕
          </button>

          {/* TITLE */}
          <div className="absolute bottom-6 left-10">
            <h1 className="text-white text-3xl font-bold drop-shadow">
              {food.name}
            </h1>
          </div>
        </div>

        {/* CONTENT */}
        <div className="px-10 py-10 grid md:grid-cols-3 gap-10">

          {/* LEFT */}
          <div className="md:col-span-2">

            <h2 className="text-xl font-bold mb-4">
              Giới thiệu
            </h2>

            <p className="text-gray-600 whitespace-pre-line leading-7">
              {food.description || "Chưa có mô tả"}
            </p>

          </div>

          {/* RIGHT CARD */}
          <div className="bg-white p-6 rounded-xl shadow-lg">

            <div className="h-40 bg-gray-100 flex items-center justify-center mb-4 rounded">
              <img
                src={food.imageUrl}
                className="max-h-full max-w-full object-contain"
              />
            </div>

            <p className="mb-2">📍 Vincom Plaza</p>
            <p className="mb-2">⭐ 4.5</p>

            <button className="w-full mt-4 bg-red-500 text-white py-2 rounded-lg hover:bg-red-600 transition">
              See Menu
            </button>

          </div>

        </div>
      </div>
    </div>
  );
}