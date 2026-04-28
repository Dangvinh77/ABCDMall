import React from "react";
import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import Login from "./Login";

const { navigateMock, postMock, getMock } = vi.hoisted(() => ({
  navigateMock: vi.fn(),
  postMock: vi.fn(),
  getMock: vi.fn(),
}));

vi.mock("react-router-dom", async () => {
  const actual = await vi.importActual("react-router-dom");
  return {
    ...actual,
    useNavigate: () => navigateMock,
  };
});

vi.mock("../../../core/api/api", () => ({
  default: {
    post: postMock,
    get: getMock,
  },
}));

describe("Login", () => {
  beforeEach(() => {
    navigateMock.mockReset();
    postMock.mockReset();
    getMock.mockReset();
    localStorage.clear();
  });

  it("redirects to initial password change when backend requires it", async () => {
    postMock.mockResolvedValueOnce({
      accessToken: "access-token",
      refreshToken: "refresh-token",
      requiresPasswordChange: true,
      passwordSetupToken: "setup-token",
    });
    getMock.mockResolvedValueOnce({
      role: "Manager",
    });

    render(
      <MemoryRouter>
        <Login />
      </MemoryRouter>,
    );

    fireEvent.change(screen.getByPlaceholderText("Enter your email"), {
      target: { value: "manager@example.com" },
    });
    fireEvent.change(screen.getByPlaceholderText("Enter your password"), {
      target: { value: "123456" },
    });
    fireEvent.click(screen.getByRole("button", { name: "Sign In" }));

    await waitFor(() => {
      expect(navigateMock).toHaveBeenCalledWith("/change-initial-password?token=setup-token");
    });
    expect(localStorage.getItem("profile")).toBe(JSON.stringify({ role: "Manager" }));
  });
});
