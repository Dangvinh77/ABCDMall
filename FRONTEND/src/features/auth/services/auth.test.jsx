import { beforeEach, describe, expect, it, vi } from "vitest";

const postMock = vi.fn();

vi.mock("../../../core/api/api", () => ({
  default: {
    post: postMock,
  },
}));

describe("logoutUser", () => {
  beforeEach(() => {
    vi.resetModules();
    vi.clearAllMocks();
    localStorage.clear();
  });

  it("clears profile and dispatches auth:changed even if backend logout fails", async () => {
    postMock.mockRejectedValueOnce(new Error("logout failed"));
    localStorage.setItem("token", "token");
    localStorage.setItem("refreshToken", "refresh");
    localStorage.setItem("role", "Manager");
    localStorage.setItem("profile", JSON.stringify({ role: "Manager" }));

    const eventSpy = vi.fn();
    window.addEventListener("auth:changed", eventSpy);

    const { logoutUser } = await import("./auth");
    await logoutUser();

    expect(localStorage.getItem("token")).toBeNull();
    expect(localStorage.getItem("refreshToken")).toBeNull();
    expect(localStorage.getItem("role")).toBeNull();
    expect(localStorage.getItem("profile")).toBeNull();
    expect(eventSpy).toHaveBeenCalledTimes(1);
  });
});
