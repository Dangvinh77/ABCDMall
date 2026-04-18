import { useState } from "react";
import { Link } from "react-router-dom";
import api from "../../../core/api/api";

export default function RegisterModern() {
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [confirmPassword, setConfirmPassword] = useState("");
    const [fullName, setFullName] = useState("");
    const [shopName, setShopName] = useState("");
    const [cccd, setCccd] = useState("");
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState("");
    const [success, setSuccess] = useState("");

    const handleRegister = async () => {
        try {
            setLoading(true);
            setError("");
            setSuccess("");

            if (password !== confirmPassword) {
                setError("Confirm password does not match.");
                setLoading(false);
                return;
            }

            const res = await api.post("/Auth/register", {
                email,
                password,
                fullName,
                shopName,
                cccd,
            });

            setSuccess(
                res.data.emailSent
                    ? "User created successfully and the notification email was sent."
                    : "User created successfully, but the notification email could not be sent."
            );
            setEmail("");
            setPassword("");
            setConfirmPassword("");
            setFullName("");
            setShopName("");
            setCccd("");
        } catch (err) {
            setError(err.response?.data || "Server error");
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
                                Register
                            </div>
                            <h1 className="mt-3 text-3xl font-black tracking-tight sm:text-4xl">
                                Create a New Account
                            </h1>
                            <p className="mt-2 max-w-2xl text-sm text-slate-600 sm:text-base">
                                Fill in the information below to create a new user in the system.
                            </p>
                        </div>

                        <Link
                            to="/dashboard"
                            className="inline-flex items-center justify-center rounded-full bg-slate-950 px-5 py-3 text-sm font-semibold text-white transition hover:-translate-y-0.5"
                        >
                            Back to Dashboard
                        </Link>
                    </div>
                </header>

                <main className="mt-6 flex-1">
                    <section className="relative overflow-hidden rounded-[34px] bg-slate-950 px-6 py-7 text-white shadow-[0_30px_120px_rgba(15,23,42,0.26)] sm:px-8 sm:py-9">
                        <div className="absolute inset-0 bg-[radial-gradient(circle_at_top_left,_rgba(251,191,36,0.32),_transparent_28%),radial-gradient(circle_at_bottom_right,_rgba(14,165,233,0.24),_transparent_30%)]" />
                        <div className="absolute right-[-40px] top-[-30px] h-40 w-40 rounded-full bg-amber-300/20 blur-3xl" />
                        <div className="absolute bottom-[-50px] left-[-20px] h-44 w-44 rounded-full bg-cyan-400/15 blur-3xl" />

                        <div className="relative grid gap-6 lg:grid-cols-[1.05fr_0.95fr]">
                            <div>
                                <p className="text-sm font-semibold uppercase tracking-[0.35em] text-amber-300">
                                    Overview
                                </p>
                                <h2 className="mt-4 text-4xl font-black leading-tight sm:text-5xl">
                                    Register
                                </h2>
                                <p className="mt-4 max-w-2xl text-sm leading-7 text-slate-300 sm:text-base">
                                    Use this form to create a manager account and connect it to a shop profile.
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

                                    <div>
                                        <label className="mb-2 block text-sm font-semibold text-white/80">
                                            Full Name
                                        </label>
                                        <input
                                            value={fullName}
                                            placeholder="Enter full name"
                                            className="w-full rounded-[18px] border border-white/10 bg-black/10 px-4 py-3 text-white outline-none transition placeholder:text-slate-400 focus:border-amber-300 focus:ring-4 focus:ring-amber-200/10"
                                            onChange={(e) => setFullName(e.target.value)}
                                        />
                                    </div>

                                    <div>
                                        <label className="mb-2 block text-sm font-semibold text-white/80">
                                            Email
                                        </label>
                                        <input
                                            value={email}
                                            placeholder="Enter email"
                                            className="w-full rounded-[18px] border border-white/10 bg-black/10 px-4 py-3 text-white outline-none transition placeholder:text-slate-400 focus:border-amber-300 focus:ring-4 focus:ring-amber-200/10"
                                            onChange={(e) => setEmail(e.target.value)}
                                        />
                                    </div>

                                    <div>
                                        <label className="mb-2 block text-sm font-semibold text-white/80">
                                            Shop Name
                                        </label>
                                        <input
                                            value={shopName}
                                            placeholder="Enter shop name"
                                            className="w-full rounded-[18px] border border-white/10 bg-black/10 px-4 py-3 text-white outline-none transition placeholder:text-slate-400 focus:border-amber-300 focus:ring-4 focus:ring-amber-200/10"
                                            onChange={(e) => setShopName(e.target.value)}
                                        />
                                    </div>

                                    <div>
                                        <label className="mb-2 block text-sm font-semibold text-white/80">
                                            CCCD
                                        </label>
                                        <input
                                            value={cccd}
                                            placeholder="Enter CCCD"
                                            className="w-full rounded-[18px] border border-white/10 bg-black/10 px-4 py-3 text-white outline-none transition placeholder:text-slate-400 focus:border-amber-300 focus:ring-4 focus:ring-amber-200/10"
                                            onChange={(e) => setCccd(e.target.value)}
                                        />
                                    </div>

                                    <div>
                                        <label className="mb-2 block text-sm font-semibold text-white/80">
                                            Password
                                        </label>
                                        <input
                                            type="password"
                                            value={password}
                                            placeholder="Enter password"
                                            className="w-full rounded-[18px] border border-white/10 bg-black/10 px-4 py-3 text-white outline-none transition placeholder:text-slate-400 focus:border-amber-300 focus:ring-4 focus:ring-amber-200/10"
                                            onChange={(e) => setPassword(e.target.value)}
                                        />
                                    </div>

                                    <div>
                                        <label className="mb-2 block text-sm font-semibold text-white/80">
                                            Confirm Password
                                        </label>
                                        <input
                                            type="password"
                                            value={confirmPassword}
                                            placeholder="Re-enter password"
                                            className="w-full rounded-[18px] border border-white/10 bg-black/10 px-4 py-3 text-white outline-none transition placeholder:text-slate-400 focus:border-amber-300 focus:ring-4 focus:ring-amber-200/10"
                                            onChange={(e) => setConfirmPassword(e.target.value)}
                                        />
                                    </div>

                                    <button
                                        onClick={handleRegister}
                                        disabled={loading}
                                        className="w-full rounded-[18px] bg-white py-3 text-sm font-semibold text-slate-950 transition hover:-translate-y-0.5 disabled:cursor-not-allowed disabled:opacity-50"
                                    >
                                        {loading ? "Creating..." : "Create User"}
                                    </button>
                                </div>
                            </div>
                        </div>
                    </section>
                </main>
            </div>
        </div>
    );
}
