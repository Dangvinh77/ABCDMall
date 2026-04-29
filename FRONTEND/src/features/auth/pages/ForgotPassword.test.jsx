import React from "react";
import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import ForgotPassword from "./ForgotPassword";

const { navigateMock, postMock } = vi.hoisted(() => ({
  navigateMock: vi.fn(),
  postMock: vi.fn(),
}));

vi.mock("react-router-dom", async () => {
  const actual = await vi.importActual("react-router-dom");
  return {
    ...actual,
    useNavigate: () => navigateMock,
  };
});

vi.mock("../../../core/api/api", () => ({
  __esModule: true,
  default: {
    post: postMock,
  },
}));

describe("ForgotPassword", () => {
  beforeEach(() => {
    navigateMock.mockReset();
    postMock.mockReset();
  });

  it("falls back to a seed OTP lookup when email delivery fails for seeded accounts", async () => {
    postMock
      .mockRejectedValueOnce(new Error("Unable to send OTP by email"))
      .mockResolvedValueOnce({
        email: "manager1@abcdmall.local",
        otp: "123456",
      });

    render(
      <MemoryRouter>
        <ForgotPassword />
      </MemoryRouter>,
    );

    fireEvent.change(screen.getByPlaceholderText("Enter your email"), {
      target: { value: "manager1@abcdmall.local" },
    });
    fireEvent.change(screen.getByPlaceholderText("Enter your new password"), {
      target: { value: "Manager@456" },
    });
    fireEvent.change(screen.getByPlaceholderText("Re-enter your new password"), {
      target: { value: "Manager@456" },
    });

    fireEvent.click(screen.getByRole("button", { name: "Send OTP" }));

    await waitFor(() => {
      expect(postMock).toHaveBeenNthCalledWith(1, "/Auth/forgotpassword/request-otp", {
        email: "manager1@abcdmall.local",
        newPassword: "Manager@456",
      });
    });

    await waitFor(() => {
      expect(postMock).toHaveBeenNthCalledWith(2, "/Auth/forgotpassword/dev-otp", {
        email: "manager1@abcdmall.local",
      });
    });

    expect(await screen.findByText(/seeded account detected/i)).toBeInTheDocument();
    expect(screen.getByText(/123456/)).toBeInTheDocument();
    expect(screen.getByPlaceholderText("Enter the 6-digit OTP")).toBeInTheDocument();
  });
});
