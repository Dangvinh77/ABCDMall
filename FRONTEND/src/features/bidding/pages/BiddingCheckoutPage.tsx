import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { biddingApi } from "../api/biddingApi";

export default function BiddingCheckoutPage() {
  const { bidId } = useParams();
  const [error, setError] = useState("");

  useEffect(() => {
    async function beginCheckout() {
      if (!bidId) {
        setError("Bid id is missing.");
        return;
      }

      try {
        const session = await biddingApi.payBid(bidId);
        window.location.assign(session.checkoutUrl);
      } catch (err) {
        setError(err instanceof Error ? err.message : "Unable to create checkout session.");
      }
    }

    beginCheckout();
  }, [bidId]);

  return (
    <div className="flex min-h-screen flex-col items-center justify-center gap-5 bg-[#07091a] px-4 text-white">
      {!error ? (
        <>
          <div className="relative">
            <div className="h-20 w-20 rounded-full border-4 border-purple-500/20" />
            <div className="absolute inset-0 m-auto h-10 w-10 animate-spin rounded-full border-4 border-transparent border-t-purple-500" />
          </div>
          <div className="text-center">
            <p className="text-lg font-semibold text-white">Redirecting to Stripe secure checkout...</p>
            <p className="mt-1 text-sm text-gray-500">Please do not close your browser while the payment session is being prepared.</p>
          </div>
        </>
      ) : (
        <div className="max-w-xl rounded-[28px] border border-rose-500/30 bg-white/5 p-8 text-center shadow-2xl">
          <p className="text-sm uppercase tracking-[0.24em] text-rose-300">Payment Error</p>
          <h1 className="mt-3 text-3xl font-black">Unable to proceed to payment</h1>
          <p className="mt-3 text-sm text-slate-300">{error}</p>
          <Link to="/manager-bidding" className="mt-6 inline-flex rounded-full bg-white px-5 py-3 text-sm font-semibold text-slate-950 transition hover:-translate-y-0.5">
            Back to Bidding
          </Link>
        </div>
      )}
    </div>
  );
}
