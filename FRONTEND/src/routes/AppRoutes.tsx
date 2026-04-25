import { lazy, Suspense } from "react";
import { Navigate, Route, Routes, useParams } from "react-router-dom";
import { HomePage } from "../pages/home/HomePage";

const AdminManagement = lazy(() => import("../features/auth/pages/AdminManagement"));
const AdminBiddingPage = lazy(() => import("../features/bidding/pages/AdminBiddingPage"));
const BiddingCheckoutPage = lazy(() => import("../features/bidding/pages/BiddingCheckoutPage"));
const BiddingPaymentCancelPage = lazy(() => import("../features/bidding/pages/BiddingPaymentCancelPage"));
const BiddingPaymentSuccessPage = lazy(() => import("../features/bidding/pages/BiddingPaymentSuccessPage"));
const Dashboard = lazy(() => import("../features/auth/pages/Dashboard"));
const ForgotPassword = lazy(() => import("../features/auth/pages/ForgotPassword"));
const Login = lazy(() => import("../features/auth/pages/Login"));
const ManagerBiddingPage = lazy(() => import("../features/bidding/pages/ManagerBiddingPage"));
const ManagerShops = lazy(() => import("../features/auth/pages/ManagerShops"));
const Profile = lazy(() => import("../features/auth/pages/Profile"));
const Register = lazy(() => import("../features/auth/pages/Register"));
const RentalAreasAdmin = lazy(() => import("../features/auth/pages/RentalAreasAdmin"));
const RevenueStatistics = lazy(() => import("../features/auth/pages/RevenueStatistics"));
const ShopInfo = lazy(() => import("../features/auth/pages/ShopInfo"));
const UserManagement = lazy(() => import("../features/auth/pages/UserManagement"));
const FoodRoutes = lazy(() => import("../features/food/routes/FoodRoutes").then((module) => ({ default: module.FoodRoutes })));
const MoviesAdminRoutes = lazy(() =>
  import("../features/movies-admin/routes/MoviesAdminRoutes").then((module) => ({ default: module.MoviesAdminRoutes })),
);
const MoviesRoutes = lazy(() => import("../features/movies/routes/MovieRoutes").then((module) => ({ default: module.MoviesRoutes })));
const ShopsRoutes = lazy(() => import("../features/shops/routes/ShopsRoutes").then((module) => ({ default: module.ShopsRoutes })));
const AmenitiesPage = lazy(() => import("../pages/amenities/AmenitiesPage").then((module) => ({ default: module.AmenitiesPage })));
const BrandsPage = lazy(() => import("../pages/brands/BrandsPage").then((module) => ({ default: module.BrandsPage })));
const ContactPage = lazy(() => import("../pages/contact/ContactPage").then((module) => ({ default: module.ContactPage })));
const MapPage = lazy(() => import("../pages/directory/MapPage").then((module) => ({ default: module.MapPage })));
const EventsPage = lazy(() => import("../pages/events/EventsPage").then((module) => ({ default: module.EventsPage })));
const EventDetailPage = lazy(() => import("../pages/events/EventDetailPage").then((module) => ({ default: module.EventDetailPage })));
const FeedbackPage = lazy(() => import("../pages/feedbacks/FeedbackPage").then((module) => ({ default: module.FeedbackPage })));
const FaqPage = lazy(() => import("../pages/support/FaqPage").then((module) => ({ default: module.FaqPage })));

function RouteFallback() {
  return (
    <div className="flex min-h-[40vh] items-center justify-center px-6 py-20 text-center text-gray-500">
      Loading content...
    </div>
  );
}

function LegacyFoodRedirect() {
  const { slug } = useParams();
  return <Navigate to={slug ? `/food-court/${slug}` : "/food-court"} replace />;
}

export function AppRoutes() {
  return (
    <Suspense fallback={<RouteFallback />}>
      <Routes>
        <Route path="/" element={<HomePage />} />
        <Route path="/login" element={<Login />} />
        <Route path="/dashboard" element={<Dashboard />} />
        <Route path="/manager-bidding" element={<ManagerBiddingPage />} />
        <Route path="/manager-bidding/checkout/:bidId" element={<BiddingCheckoutPage />} />
        <Route path="/manager-bidding/payment/success" element={<BiddingPaymentSuccessPage />} />
        <Route path="/manager-bidding/payment/cancel" element={<BiddingPaymentCancelPage />} />
        <Route path="/manager-shops" element={<ManagerShops />} />
        <Route path="/profile" element={<Profile />} />
        <Route path="/forgot-password" element={<ForgotPassword />} />
        <Route path="/register" element={<Register />} />
        <Route path="/shop-info" element={<ShopInfo />} />
        <Route path="/admin-management" element={<AdminManagement />} />
        <Route path="/admin-management/bidding" element={<AdminBiddingPage />} />
        <Route path="/admin-management/users" element={<UserManagement />} />
        <Route path="/admin-management/revenue" element={<RevenueStatistics />} />
        <Route path="/rental-areas" element={<RentalAreasAdmin />} />
        <Route path="/users" element={<Navigate to="/admin-management/users" replace />} />
        <Route path="/revenue-statistics" element={<Navigate to="/admin-management/revenue" replace />} />

        <Route path="/food-court/*" element={<FoodRoutes />} />
        <Route path="/food/:slug" element={<LegacyFoodRedirect />} />
        <Route path="/mall/:mall/:slug" element={<LegacyFoodRedirect />} />

        <Route path="/movies/*" element={<MoviesRoutes />} />
        <Route path="/movies/admin/*" element={<MoviesAdminRoutes />} />

        <Route path="/shops/*" element={<ShopsRoutes />} />
        <Route path="/events" element={<EventsPage />} />
        <Route path="/events/:id" element={<EventDetailPage />} />
        <Route
          path="/gallery"
          element={
            <div className="min-h-[50vh] p-20 text-center text-2xl font-bold">
              Gallery Page (Coming Soon)
            </div>
          }
        />
        <Route path="/map" element={<MapPage />} />
        <Route path="/contact" element={<ContactPage />} />
        <Route path="/brands" element={<BrandsPage />} />
        <Route path="/amenities" element={<AmenitiesPage />} />
        <Route path="/faq" element={<FaqPage />} />
        <Route path="/feedback" element={<FeedbackPage />} />

        <Route path="*" element={<div>404 Not Found</div>} />
      </Routes>
    </Suspense>
  );
}
