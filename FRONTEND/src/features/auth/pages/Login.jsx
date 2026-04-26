import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import api from "../../../core/api/api";

export default function Login() {
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [otp, setOtp] = useState("");
    const [otpRequired, setOtpRequired] = useState(false);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState("");
    const [success, setSuccess] = useState("");
    const navigate = useNavigate();

    const handleLogin = async () => {
        try {
            setLoading(true);
            setError("");
            setSuccess("");

            const payload = { email, password };
            if (otpRequired || otp.trim()) {
                payload.otp = otp.trim();
            }

            const res = await api.post("/Auth/login", payload);
            const { accessToken, refreshToken } = res.data;

            localStorage.setItem("token", accessToken);
            localStorage.setItem("refreshToken", refreshToken);

            const profileRes = await api.get("/Auth/getprofile");
            localStorage.setItem("role", profileRes.data.role);
            localStorage.setItem("profile", JSON.stringify(profileRes.data));
            window.dispatchEvent(new Event("auth:changed"));

            setOtpRequired(false);
            setOtp("");
            navigate("/dashboard");
        } catch (err) {
            const responseData = err.response?.data;
            const nextRequiresOtp = Boolean(responseData?.requiresOtp);
            const message = typeof responseData === "string"
                ? responseData
                : responseData?.message || "Sign in failed.";

            setOtpRequired(nextRequiresOtp || otpRequired);
            setError(message);

            if (nextRequiresOtp) {
                setSuccess("An OTP has been sent to your sign-in email. Enter the 6-digit code to continue.");
            }
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="min-h-screen relative overflow-hidden bg-slate-950">
            <div className="absolute inset-0 bg-[radial-gradient(circle_at_top_left,_rgba(251,191,36,0.25),_transparent_30%),radial-gradient(circle_at_bottom_right,_rgba(59,130,246,0.22),_transparent_30%)]" />
            <div className="absolute -top-20 -left-20 h-72 w-72 rounded-full bg-amber-400/20 blur-3xl" />
            <div className="absolute -bottom-24 -right-20 h-80 w-80 rounded-full bg-cyan-400/20 blur-3xl" />

            <div className="relative z-10 min-h-screen flex items-center justify-center px-4 py-10">
                <div className="w-full max-w-6xl grid lg:grid-cols-2 rounded-[32px] overflow-hidden border border-white/10 bg-white/10 backdrop-blur-xl shadow-[0_20px_80px_rgba(0,0,0,0.45)]">

                    <div className="hidden lg:flex flex-col justify-between p-10 bg-gradient-to-br from-amber-400 via-orange-400 to-rose-500 text-slate-950">
                        <div>
                            <p className="text-sm font-semibold uppercase tracking-[0.3em]">
                                ABCD Mall
                            </p>
                            <h1 className="mt-6 text-5xl font-black leading-tight">
                                Welcome back
                            </h1>
                            <p className="mt-4 text-base font-medium text-slate-900/80 max-w-md">
                                Sign in to manage the shopping system, stores, and customer experience in a modern retail environment.
                            </p>
                        </div>

                        <div className="grid grid-cols-2 gap-4">
                            <div className="rounded-2xl bg-white/30 p-4 backdrop-blur-md">
                                <p className="text-sm font-semibold">Smart Retail</p>
                                <p className="mt-1 text-sm text-slate-900/75">
                                    Fast, visual, and professional management
                                </p>
                            </div>
                            <div className="rounded-2xl bg-white/30 p-4 backdrop-blur-md">
                                <p className="text-sm font-semibold">Premium Experience</p>
                                <p className="mt-1 text-sm text-slate-900/75">
                                    A refined interface tailored for mall operations
                                </p>
                            </div>
                        </div>
                    </div>

                    <div className="bg-white/95 p-6 sm:p-10 lg:p-12">
                        <div className="mx-auto w-full max-w-md">
                            <div className="mb-8">
                                <div className="inline-flex items-center gap-3 rounded-full bg-slate-100 px-4 py-2 text-sm font-semibold text-slate-700">
                                    <span className="h-2.5 w-2.5 rounded-full bg-emerald-500" />
                                    Member Login
                                </div>

                                <h2 className="mt-6 text-4xl font-black text-slate-900">
                                    Sign In
                                </h2>
                                <p className="mt-2 text-sm text-slate-500">
                                    Access the 1 Mall administration system
                                </p>
                            </div>

                            <div className="space-y-5">
                                {error && (
                                    <div className="rounded-2xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-600">
                                        {error}
                                    </div>
                                )}

                                {success && (
                                    <div className="rounded-2xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-700">
                                        {success}
                                    </div>
                                )}

                                <div>
                                    <label className="mb-2 block text-sm font-semibold text-slate-700">
                                        Email
                                    </label>
                                    <input
                                        type="email"
                                        placeholder="Enter your email"
                                        value={email}
                                        onChange={(e) => setEmail(e.target.value)}
                                        className="w-full rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3 text-slate-800 outline-none transition focus:border-amber-400 focus:bg-white focus:ring-4 focus:ring-amber-100"
                                    />
                                </div>

                                <div>
                                    <label className="mb-2 block text-sm font-semibold text-slate-700">
                                        Password
                                    </label>
                                    <input
                                        type="password"
                                        placeholder="Enter your password"
                                        value={password}
                                        onChange={(e) => setPassword(e.target.value)}
                                        className="w-full rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3 text-slate-800 outline-none transition focus:border-amber-400 focus:bg-white focus:ring-4 focus:ring-amber-100"
                                    />
                                </div>

                                {otpRequired && (
                                    <div>
                                        <label className="mb-2 block text-sm font-semibold text-slate-700">
                                            OTP
                                        </label>
                                        <input
                                            type="text"
                                            inputMode="numeric"
                                            maxLength={6}
                                            placeholder="Enter the 6-digit OTP from your email"
                                            value={otp}
                                            onChange={(e) => setOtp(e.target.value.replace(/\D/g, "").slice(0, 6))}
                                            className="w-full rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3 text-slate-800 outline-none transition focus:border-amber-400 focus:bg-white focus:ring-4 focus:ring-amber-100"
                                        />
                                    </div>
                                )}

                                <div className="flex justify-end">
                                    <Link
                                        to="/forgot-password"
                                        className="text-sm font-semibold text-amber-600 transition hover:text-amber-700"
                                    >
                                        Forgot password?
                                    </Link>
                                </div>

                                <button
                                    onClick={handleLogin}
                                    disabled={loading}
                                    className="w-full rounded-2xl bg-gradient-to-r from-slate-900 via-slate-800 to-slate-900 py-3.5 text-white font-semibold shadow-lg shadow-slate-900/20 transition hover:-translate-y-0.5 hover:from-amber-500 hover:to-orange-500 disabled:cursor-not-allowed disabled:opacity-60"
                                >
                                    {loading ? "Processing..." : "Sign In"}
                                </button>
                            </div>

                            <div className="mt-8 rounded-2xl bg-slate-100 p-4 text-sm text-slate-600">
                                This interface is designed to feel modern, premium, and suitable for a mall or retail center management system.
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}
