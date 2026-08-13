import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import api from "../../../core/api/api";

export default function ForgotPassword() {
  const [email, setEmail] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [otp, setOtp] = useState("");
  const [loading, setLoading] = useState(false);
  const [otpSent, setOtpSent] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const navigate = useNavigate();

  const handleSendOtp = async () => {
    try {
      setLoading(true);
      setError("");
      setSuccess("");

      if (newPassword !== confirmPassword) {
        setError("Confirm password does not match.");
        setLoading(false);
        return;
      }

      await api.post("/Auth/forgotpassword/request-otp", {
        email,
        newPassword,
      });

      setOtpSent(true);
      setSuccess("The OTP has been sent to your email.");
    } catch (err) {
      setError(err?.message || "Unable to send OTP.");
    } finally {
      setLoading(false);
    }
  };

  const handleConfirmOtp = async () => {
    try {
      setLoading(true);
      setError("");
      setSuccess("");

      await api.post("/Auth/forgotpassword/confirm-otp", {
        email,
        otp,
      });

      setSuccess("Your password has been reset successfully. Redirecting to the login page...");
      setTimeout(() => navigate("/login"), 1200);
    } catch (err) {
      setError(err?.message || "Invalid OTP.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-[linear-gradient(180deg,#fff8ef_0%,#fffdf8_42%,#f8fbff_100%)] text-slate-900">
      <div className="mx-auto flex min-h-screen w-full max-w-7xl flex-col px-4 py-6 sm:px-6 lg:px-8">
        <header className="rounded-[28px] border border-white/70 bg-white/80 px-5 py-4 shadow-[0_20px_80px_rgba(15,23,42,0.08)] backdrop-blur-xl sm:px-6">
          <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
            <div>
              <div className="inline-flex items-center gap-2 rounded-full bg-amber-100 px-3 py-1 text-xs font-semibold uppercase tracking-[0.28em] text-amber-700">
                Forgot Password
              </div>
              <h1 className="mt-3 text-3xl font-black tracking-tight sm:text-4xl">
                Forgot Password
              </h1>
              <p className="mt-2 max-w-2xl text-sm text-slate-600 sm:text-base">
                Enter your email, your new password, and verify it with the 6-digit OTP sent to your email.
              </p>
            </div>

            <Link
              to="/login"
              className="inline-flex items-center justify-center rounded-full bg-slate-950 px-5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5"
            >
              Back to Login
            </Link>
          </div>
        </header>

        <main className="mt-6 flex-1">
          <section className="relative overflow-hidden rounded-[34px] bg-slate-950 px-6 py-7 text-white shadow-[0_30px_120px_rgba(15,23,42,0.26)] sm:px-8 sm:py-9">
            <div className="absolute inset-0 bg-[radial-gradient(circle_at_top_left,_rgba(251,191,36,0.32),_transparent_28%),radial-gradient(circle_at_bottom_right,_rgba(14,165,233,0.24),_transparent_30%)]" />
            <div className="absolute right-[-40px] top-[-30px] h-40 w-40 rounded-full bg-amber-300/20 blur-3xl" />
            <div className="absolute bottom-[-50px] left-[-20px] h-44 w-44 rounded-full bg-cyan-400/15 blur-3xl" />

            <div className="relative grid gap-6 lg:grid-cols-[1fr_1fr]">
              <div>
                <p className="text-sm font-semibold uppercase tracking-[0.35em] text-amber-300">
                  Overview
                </p>
                <h2 className="mt-4 text-4xl font-black leading-tight sm:text-5xl">
                  Reset with OTP
                </h2>
                <p className="mt-4 max-w-2xl text-sm leading-7 text-slate-300 sm:text-base">
                  The system will send a 6-digit OTP to your registered email. Enter the correct OTP to set your new password.
                </p>
              </div>

              <div className="rounded-[28px] border border-white/10 bg-white/10 p-5 backdrop-blur-md">
                <div className="space-y-4">
                  {error && (
                    <div className="rounded-[20px] border border-red-400/20 bg-red-500/10 px-4 py-3 text-sm text-red-200">
                      {error}
                    </div>
                  )}

                  {success && (
                    <div className="rounded-[20px] border border-emerald-400/20 bg-emerald-500/10 px-4 py-3 text-sm text-emerald-200">
                      {success}
                    </div>
                  )}

                  <input
                    type="email"
                    value={email}
                    placeholder="Enter your email"
                    className="w-full rounded-[18px] border border-white/10 bg-black/10 px-4 py-3 text-white outline-none transition placeholder:text-slate-400 focus:border-amber-300 focus:ring-4 focus:ring-amber-200/10"
                    onChange={(e) => setEmail(e.target.value)}
                  />

                  <input
                    type="password"
                    value={newPassword}
                    placeholder="Enter your new password"
                    className="w-full rounded-[18px] border border-white/10 bg-black/10 px-4 py-3 text-white outline-none transition placeholder:text-slate-400 focus:border-amber-300 focus:ring-4 focus:ring-amber-200/10"
                    onChange={(e) => setNewPassword(e.target.value)}
                  />

                  <input
                    type="password"
                    value={confirmPassword}
                    placeholder="Re-enter your new password"
                    className="w-full rounded-[18px] border border-white/10 bg-black/10 px-4 py-3 text-white outline-none transition placeholder:text-slate-400 focus:border-amber-300 focus:ring-4 focus:ring-amber-200/10"
                    onChange={(e) => setConfirmPassword(e.target.value)}
                  />

                  {!otpSent ? (
                    <button
                      type="button"
                      onClick={handleSendOtp}
                      disabled={loading}
                      className="w-full rounded-[18px] bg-white py-3 text-sm font-semibold text-slate-950 transition hover:-translate-y-0.5 disabled:cursor-not-allowed disabled:opacity-50"
                    >
                      {loading ? "Sending..." : "Send OTP"}
                    </button>
                  ) : (
                    <>
                      <input
                        type="text"
                        value={otp}
                        inputMode="numeric"
                        maxLength={6}
                        placeholder="Enter the 6-digit OTP"
                        className="w-full rounded-[18px] border border-white/10 bg-black/10 px-4 py-3 text-white outline-none transition placeholder:text-slate-400 focus:border-amber-300 focus:ring-4 focus:ring-amber-200/10"
                        onChange={(e) => setOtp(e.target.value.replace(/\D/g, "").slice(0, 6))}
                      />

                      <div className="flex gap-3">
                        <button
                          type="button"
                          onClick={handleConfirmOtp}
                          disabled={loading}
                          className="flex-1 rounded-[18px] bg-white py-3 text-sm font-semibold text-slate-950 transition hover:-translate-y-0.5 disabled:cursor-not-allowed disabled:opacity-50"
                        >
                          {loading ? "Verifying..." : "Confirm OTP"}
                        </button>

                        <button
                          type="button"
                          onClick={() => {
                            setOtpSent(false);
                            setOtp("");
                            setError("");
                            setSuccess("");
                          }}
                          className="flex-1 rounded-[18px] border border-white/15 bg-white/5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5"
                        >
                          Back
                        </button>
                      </div>
                    </>
                  )}
                </div>
              </div>
            </div>
          </section>
        </main>
      </div>
    </div>
  );
}
