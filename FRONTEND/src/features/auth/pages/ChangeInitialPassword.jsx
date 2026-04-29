import React, { useMemo, useState } from "react";
import { Link, useNavigate, useSearchParams } from "react-router-dom";
import api from "../../../core/api/api";

export default function ChangeInitialPassword() {
  const [searchParams] = useSearchParams();
  const token = useMemo(() => searchParams.get("token") || "", [searchParams]);
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const navigate = useNavigate();

  const handleSubmit = async () => {
    if (!token) {
      setError("Invalid or missing password setup token.");
      return;
    }

    if (!newPassword || newPassword.length < 6) {
      setError("New password must be at least 6 characters.");
      return;
    }

    if (newPassword !== confirmPassword) {
      setError("Confirm password does not match.");
      return;
    }

    try {
      setLoading(true);
      setError("");
      setSuccess("");
      await api.post("/Auth/initial-password/change", {
        token,
        newPassword,
      });

      setSuccess("Password changed successfully. Please sign in again.");
      localStorage.removeItem("token");
      localStorage.removeItem("refreshToken");
      localStorage.removeItem("role");
      localStorage.removeItem("profile");
      window.dispatchEvent(new Event("auth:changed"));
      setTimeout(() => navigate("/login"), 900);
    } catch (err) {
      setError(err?.message || "Unable to change password.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-[linear-gradient(180deg,#fff8ef_0%,#fffdf8_42%,#f8fbff_100%)] px-4 pb-8 pt-28 text-slate-900 sm:px-6 lg:px-8">
      <div className="mx-auto max-w-xl rounded-[30px] border border-slate-200 bg-white/95 p-6 shadow-[0_24px_90px_rgba(15,23,42,0.08)] backdrop-blur-xl">
        <p className="text-xs font-semibold uppercase tracking-[0.28em] text-amber-700">
          Password Setup
        </p>
        <h1 className="mt-3 text-3xl font-black">Change Initial Password</h1>
        <p className="mt-2 text-sm leading-6 text-slate-600">
          Create your permanent password. This link is valid for 24 hours and expires after successful submission.
        </p>

        <div className="mt-6 space-y-4">
          {error && <div className="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-600">{error}</div>}
          {success && <div className="rounded-2xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-700">{success}</div>}

          <label className="block">
            <span className="mb-2 block text-sm font-semibold text-slate-700">New Password</span>
            <input
              type="password"
              value={newPassword}
              onChange={(event) => setNewPassword(event.target.value)}
              className="w-full rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3 text-slate-800 outline-none transition focus:border-amber-400 focus:bg-white focus:ring-4 focus:ring-amber-100"
            />
          </label>

          <label className="block">
            <span className="mb-2 block text-sm font-semibold text-slate-700">Confirm Password</span>
            <input
              type="password"
              value={confirmPassword}
              onChange={(event) => setConfirmPassword(event.target.value)}
              className="w-full rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3 text-slate-800 outline-none transition focus:border-amber-400 focus:bg-white focus:ring-4 focus:ring-amber-100"
            />
          </label>

          <button
            type="button"
            disabled={loading}
            onClick={handleSubmit}
            className="w-full rounded-2xl bg-slate-950 py-3.5 text-sm font-semibold text-white transition hover:-translate-y-0.5 disabled:cursor-not-allowed disabled:opacity-60"
          >
            {loading ? "Saving..." : "Change Password"}
          </button>

          <Link to="/login" className="block text-center text-sm font-semibold text-amber-600 hover:text-amber-700">
            Back to Login
          </Link>
        </div>
      </div>
    </div>
  );
}
