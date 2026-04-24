import { Link, useSearchParams } from "react-router-dom";

export default function BiddingPaymentCancelPage() {
  const [searchParams] = useSearchParams();
  const bidId = searchParams.get("bidId");

  return (
    <div className="min-h-screen bg-[#07091a] px-4 py-12 text-white">
      <div className="mx-auto max-w-3xl rounded-[32px] border border-white/10 bg-white/[0.04] p-8 shadow-[0_30px_120px_rgba(15,23,42,0.4)]">
        <p className="text-sm uppercase tracking-[0.24em] text-amber-300">Payment Cancelled</p>
        <h1 className="mt-3 text-4xl font-black">Checkout was cancelled.</h1>
        <p className="mt-4 text-slate-300">
          Your winning bid remains unpaid. You can return to the bidding page and try the payment flow again before Sunday.
        </p>
        {bidId && (
          <div className="mt-6 rounded-3xl border border-white/10 bg-white/[0.03] p-5">
            <div className="text-sm text-slate-400">Bid Id</div>
            <div className="mt-1 font-mono text-sm text-white">{bidId}</div>
          </div>
        )}
        <Link to="/manager-bidding" className="mt-8 inline-flex rounded-full bg-white px-5 py-3 text-sm font-semibold text-slate-950 transition hover:-translate-y-0.5">
          Back to Bidding
        </Link>
      </div>
    </div>
  );
}
