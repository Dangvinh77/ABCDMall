type Props = {
  food: any;
  onClose: () => void;
};

export default function FoodDetailModal({ food, onClose }: Props) {
  return (
    <div className="fixed inset-0 bg-black/50 flex justify-center items-center z-50">

      <div className="bg-white rounded-xl w-[400px] p-6 relative animate-fadeIn">

        {/* CLOSE */}
        <button
          onClick={onClose}
          className="absolute top-3 right-3 text-gray-500"
        >
          ✕
        </button>

        {/* IMAGE */}
        <img
          src={food.imageUrl}
          className="w-full h-48 object-cover rounded-lg mb-4"
        />

        {/* NAME */}
        <h2 className="text-xl font-bold">{food.name}</h2>

        {/* PRICE */}
        <p className="text-red-500 font-bold mt-2">
          {food.price} VND
        </p>

        {/* DESC (fake) */}
        <p className="text-gray-600 mt-3 text-sm">
          Món ăn ngon tại food court. Đậm vị, chất lượng cao.
        </p>

      </div>
    </div>
  );
}