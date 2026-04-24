import { Header } from "./core/layouts/Header";
import { Footer } from "./core/layouts/Footer";
import { AppRoutes } from "./routes/AppRoutes";
import { ChatbotWidget } from "./features/chatbot/ChatbotWidget";

function App() {
  return (
    <div className="flex min-h-screen flex-col">
      <Header />
      <div className="flex-grow">
        <AppRoutes />
      </div>

      <Footer />
      <ChatbotWidget />
    </div>
  );
}

export default App;
