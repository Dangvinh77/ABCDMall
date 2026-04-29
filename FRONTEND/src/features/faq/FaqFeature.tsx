import { useEffect, useState } from "react";
import { Link, useLocation, useNavigate } from "react-router-dom";
import { resendTicketEmail } from "../movies/api/moviesApi";
import { MovieHeader } from "../movies/component/MovieHeader";
import { moviePaths } from "../movies/routes/moviePaths";
import { loadSupportFaqs } from "../support/api/supportUiAdapter";
import type { SupportFaqCollection } from "../support/types/support";

export const FaqFeature = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const [activeTab, setActiveTab] = useState<string>("");
  const [activeQuestion, setActiveQuestion] = useState<string | null>(null);
  const [faqData, setFaqData] = useState<SupportFaqCollection | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [resendEmail, setResendEmail] = useState("");
  const [resendBookingCode, setResendBookingCode] = useState("");
  const [resendLoading, setResendLoading] = useState(false);
  const [resendError, setResendError] = useState<string | null>(null);
  const [resendSuccess, setResendSuccess] = useState<string | null>(null);

  useEffect(() => {
    let active = true;

    async function loadData() {
      try {
        const data = await loadSupportFaqs();
        if (!active) return;
        setFaqData(data);
        setActiveTab(data.categories[0]?.id ?? "");
      } catch (err) {
        if (!active) return;
        setError(err instanceof Error ? err.message : "Failed to load FAQs.");
      } finally {
        if (active) setLoading(false);
      }
    }

    void loadData();

    return () => {
      active = false;
    };
  }, []);

  if (loading) {
    return (
      <div className="min-h-screen bg-slate-50 pt-32 pb-20 text-center text-gray-500">
        Loading FAQs...
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen bg-slate-50 pt-32 pb-20 text-center text-red-500">
        {error}
      </div>
    );
  }

  if (!faqData || faqData.categories.length === 0 || !activeTab) {
    return (
      <div className="min-h-screen bg-slate-50 pt-32 pb-20 text-center text-gray-500">
        No support articles are available right now.
      </div>
    );
  }

  const activeItems = faqData.itemsByCategory[activeTab] ?? [];

  async function handleResendTicketEmail(
    event: React.FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault();
    setResendLoading(true);
    setResendError(null);
    setResendSuccess(null);

    try {
      const response = await resendTicketEmail({
        email: resendEmail.trim(),
        bookingCode: resendBookingCode.trim(),
      });
      setResendSuccess(response.message);
      setResendEmail("");
      setResendBookingCode("");
    } catch (submitError) {
      setResendError(
        submitError instanceof Error
          ? submitError.message
          : "Unable to resend ticket email right now. Please try again later.",
      );
    } finally {
      setResendLoading(false);
    }
  }

  return (
    <div className="min-h-screen bg-[radial-gradient(circle_at_top,_rgba(139,92,246,0.18),_transparent_28%),radial-gradient(circle_at_80%_18%,_rgba(6,182,212,0.14),_transparent_22%),linear-gradient(to_bottom,_#020617,_#111827,_#0f172a)] pb-20">
      <MovieHeader
        onShowtimes={() => navigate(moviePaths.showtimes())}
        onSupport={() => {
          if (location.pathname !== "/faq") {
            navigate("/faq");
          }
        }}
        onAdmin={() => navigate(moviePaths.admin())}
      />

      <div className="mx-auto max-w-5xl px-6 pt-16">
        <div className="text-center mb-12">
          <h1 className="mb-4 bg-gradient-to-r from-red-300 via-orange-100 to-cyan-200 bg-clip-text text-4xl font-black uppercase tracking-tight text-transparent md:text-5xl">
            Movies FAQ
          </h1>
          <p className="text-lg text-slate-300">
            Get quick answers about booking tickets, receiving ticket emails,
            arriving on time, and using cinema promotions at ABCD Cinema.
          </p>
        </div>

        <section className="mb-10 rounded-[2rem] border border-red-200/20 bg-white/95 p-6 shadow-xl md:p-8">
          <h2 className="text-2xl font-black text-slate-900 md:text-3xl">
            Didn&apos;t receive your ticket email?
          </h2>
          <p className="mt-3 text-sm text-slate-600 md:text-base">
            Enter the email address used for booking and your booking code. We
            will resend the ticket email if the details match.
          </p>

          <form
            className="mt-6 grid gap-4 md:grid-cols-2"
            onSubmit={handleResendTicketEmail}
          >
            <label className="flex flex-col gap-2 text-sm font-semibold text-slate-700">
              Email address
              <input
                value={resendEmail}
                onChange={(event) => setResendEmail(event.target.value)}
                className="rounded-2xl border border-slate-200 px-4 py-3 outline-none focus:border-red-400 focus:ring-4 focus:ring-red-100"
              />
            </label>

            <label className="flex flex-col gap-2 text-sm font-semibold text-slate-700">
              Booking code
              <input
                value={resendBookingCode}
                onChange={(event) => setResendBookingCode(event.target.value)}
                className="rounded-2xl border border-slate-200 px-4 py-3 outline-none focus:border-red-400 focus:ring-4 focus:ring-red-100"
              />
            </label>

            <div className="md:col-span-2">
              <button
                type="submit"
                disabled={resendLoading}
                className="rounded-full bg-gray-900 px-8 py-3 font-bold text-white transition hover:bg-red-600 disabled:opacity-60"
              >
                {resendLoading ? "Resending..." : "Resend ticket email"}
              </button>
            </div>

            {resendError ? (
              <p className="md:col-span-2 text-sm text-red-600">
                {resendError}
              </p>
            ) : null}
            {resendSuccess ? (
              <p className="md:col-span-2 text-sm text-emerald-600">
                {resendSuccess}
              </p>
            ) : null}
          </form>
        </section>

        {/* Tab Categories */}
        <div className="mb-10 flex flex-wrap justify-center gap-3">
          {faqData.categories.map((cat) => (
            <button
              key={cat.id}
              onClick={() => {
                setActiveTab(cat.id);
                setActiveQuestion(null);
              }}
              className={`px-6 py-3 rounded-full font-bold transition-all duration-300 shadow-sm ${
                activeTab === cat.id
                  ? "bg-gradient-to-r from-red-500 to-orange-500 text-white scale-105"
                  : "bg-white text-gray-600 hover:text-red-500 border border-gray-200"
              }`}
            >
              {cat.name}
            </button>
          ))}
        </div>

        {/* FAQ Accordion */}
        <div className="rounded-[2rem] border border-white/10 bg-white p-6 shadow-xl md:p-10">
          <div className="space-y-4">
            {activeItems.map((faq) => (
              <div
                key={faq.id}
                className="border border-gray-100 rounded-2xl overflow-hidden transition-all duration-300"
              >
                <button
                  onClick={() =>
                    setActiveQuestion(activeQuestion === faq.id ? null : faq.id)
                  }
                  className={`w-full text-left px-6 py-5 flex justify-between items-center transition-colors ${
                    activeQuestion === faq.id
                      ? "bg-red-50"
                      : "bg-white hover:bg-gray-50"
                  }`}
                >
                  <span
                    className={`font-bold text-lg pr-4 ${
                      activeQuestion === faq.id ? "text-red-600" : "text-gray-800"
                    }`}
                  >
                    {faq.q}
                  </span>
                  <span
                    className={`text-2xl transition-transform duration-300 ${
                      activeQuestion === faq.id
                        ? "rotate-180 text-red-500"
                        : "text-gray-400"
                    }`}
                  >
                    ↓
                  </span>
                </button>

                <div
                  className={`overflow-hidden transition-all duration-300 ease-in-out ${
                    activeQuestion === faq.id
                      ? "max-h-96 opacity-100"
                      : "max-h-0 opacity-0"
                  }`}
                >
                  <div className="px-6 pb-5 pt-2 text-gray-600 leading-relaxed border-t border-red-100">
                    {faq.a}
                  </div>
                </div>
              </div>
            ))}
          </div>

          {/* Call to action */}
          <div className="mt-12 pt-8 border-t border-gray-200 text-center">
            <p className="mb-4 text-gray-500">
              Couldn&apos;t find the answer you needed?
            </p>
            <Link to="/feedback" className="inline-block bg-gray-900 text-white font-bold px-8 py-3 rounded-full hover:bg-red-600 transition-colors shadow-md">
              Send Us Feedback
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
};
