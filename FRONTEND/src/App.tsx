// import { Routes, Route } from "react-router-dom";
// import FoodPage from "@/features/food/pages/FoodPage";

// function App() {
//   return (
//     <Routes>
//       <Route path="/" element={<FoodPage />} />
//     </Routes>
//   );
// }

// export default App;


import { Routes, Route } from "react-router-dom";
import FoodPage from "./features/food/pages/FoodPage";
import FoodDetailPage from "./features/food/pages/FoodDetailPage";

function App() {
  return (
    
      <Routes>
        <Route path="/" element={<FoodPage />} />
        <Route path="/food/:slug" element={<FoodDetailPage />} />
        <Route path="/mall/:mall/:slug" element={<FoodDetailPage />} />
      </Routes>
    
  );
}

export default App;