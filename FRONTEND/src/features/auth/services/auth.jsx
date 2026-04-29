import api from "../../../core/api/api";

export async function logoutUser() {
  const refreshToken = localStorage.getItem("refreshToken");

  try {
    if (refreshToken) {
      await api.post("/Auth/logout", { refreshToken });
    }
  } catch {
    // Clear local session even if backend logout fails.
  } finally {
    localStorage.removeItem("token");
    localStorage.removeItem("refreshToken");
    localStorage.removeItem("role");
    localStorage.removeItem("profile");
    window.dispatchEvent(new Event("auth:changed"));
  }
}
