import { useEffect, useMemo, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { biddingApi, type ManagerCarouselBid } from "../api/biddingApi";

export default function BiddingPaymentSuccessPage() {
  const [searchParams] = useSearchParams();
  const bidId = searchParams.get("bidId") || "";
  const [bid, setBid] = useState<ManagerCarouselBid | null>(null);
  const [error, setError] = useState("");

  useEffect(() => {
    let cancelled = false;

    async function loadBid() {
      try {
        const bids = await biddingApi.getManagerBids();
        if (!cancelled) {
          setBid(bids.find((item) => item.id === bidId) ?? null);
        }
      } catch (err) {
        if (!cancelled) {
          setError(err instanceof Error ? err.message : "Unable to load bid status.");
        }
      }
    }

    loadBid();
    const intervalId = window.setInterval(loadBid, 4000);
    return () => {
      cancelled = true;
      window.clearInterval(intervalId);
    };
  }, [bidId]);

  const statusText = useMemo(() => {
    if (bid?.status === "Paid" || bid?.status === "Active") {
      return "Payment has been confirmed.";
    }

    return "Stripe returned successfully. The backend may still be processing the webhook.";
  }, [bid]);

  return (
    <div className="min-h-screen bg-[#07091a] px-4 py-12 text-white">
      <div className="mx-auto max-w-3xl rounded-[32px] border border-white/10 bg-white/[0.04] p-8 shadow-[0_30px_120px_rgba(15,23,42,0.4)]">
        <p className="text-sm uppercase tracking-[0.24em] text-emerald-300">Payment Success</p>
        <h1 className="mt-3 text-4xl font-black">Thank you.</h1>
        <p className="mt-4 text-slate-300">{error || statusText}</p>

        <div className="mt-6 rounded-3xl border border-white/10 bg-white/[0.03] p-5">
          <div className="text-sm text-slate-400">Bid Id</div>
          <div className="mt-1 font-mono text-sm text-white">{bidId || "Unavailable"}</div>
          {bid && (
            <>
              <div className="mt-4 text-sm text-slate-400">Current Status</div>
              <div className="mt-1 text-lg font-semibold text-emerald-300">{bid.status}</div>
            </>
          )}
        </div>

        <div className="mt-8 flex flex-wrap gap-3">
          <Link to="/manager-bidding" className="inline-flex rounded-full bg-white px-5 py-3 text-sm font-semibold text-slate-950 transition hover:-translate-y-0.5">
            Back to Bidding
          </Link>
          <button
            type="button"
            onClick={() => window.location.reload()}
            className="inline-flex rounded-full border border-white/20 px-5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5 hover:bg-white/10"
          >
            Refresh Status
          </button>
        </div>
      </div>
    </div>
  );
}
