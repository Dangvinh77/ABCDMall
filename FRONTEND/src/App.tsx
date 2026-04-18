import { Footer } from "./core/layouts/Footer";
import { AppRoutes } from "./routes/AppRoutes";

function App() {
  return (
    <div className="flex min-h-screen flex-col">
      <div className="flex-grow">
        <AppRoutes />
      </div>

      <Footer />
    </div>
  );
}

export default App;
