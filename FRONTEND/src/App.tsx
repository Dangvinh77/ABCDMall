import { Route, Routes } from "react-router-dom";

import { Footer } from "./core/layouts/Footer";
import FoodDetailPage from "./features/food/pages/FoodDetailPage";
import FoodPage from "./features/food/pages/FoodPage";
import { MoviesAdminRoutes } from "./features/movies-admin/routes/MoviesAdminRoutes";
import { MoviesRoutes } from "./features/movies/routes/MovieRoutes";
import { HomePage } from "./pages/home/HomePage";

function App() {
  return (
    <div className="flex min-h-screen flex-col">
      {/* main.tsx already provides BrowserRouter; keeping App router-free avoids nested-router runtime errors. */}
      <div className="flex-grow">
        <Routes>
          <Route path="/" element={<HomePage />} />

          <Route path="/food-court" element={<FoodPage />} />
          <Route path="/food/:slug" element={<FoodDetailPage />} />
          <Route path="/mall/:mall/:slug" element={<FoodDetailPage />} />

          {/* Movies pages are the live test flow: home -> showtimes -> seats -> checkout. */}
          <Route path="/movies/admin/*" element={<MoviesAdminRoutes />} />
          <Route path="/movies/*" element={<MoviesRoutes />} />

          <Route
            path="/shops"
            element={
              <div className="min-h-[50vh] p-20 text-center text-2xl font-bold">
                Trang Cua hang (Sap ra mat)
              </div>
            }
          />
          <Route
            path="/gallery"
            element={
              <div className="min-h-[50vh] p-20 text-center text-2xl font-bold">
                Trang Thu vien (Sap ra mat)
              </div>
            }
          />
          <Route
            path="/map"
            element={
              <div className="min-h-[50vh] p-20 text-center text-2xl font-bold">
                Trang So do (Sap ra mat)
              </div>
            }
          />
          <Route
            path="/contact"
            element={
              <div className="min-h-[50vh] p-20 text-center text-2xl font-bold">
                Trang Lien he (Sap ra mat)
              </div>
            }
          />
        </Routes>
      </div>

      <Footer />
    </div>
  );
}

export default App;
