import React from "react";
import { cleanup, fireEvent, render, screen, waitFor } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import UserManagement from "./UserManagement";

const { getMock, postMock, putMock, deleteMock } = vi.hoisted(() => ({
  getMock: vi.fn(),
  postMock: vi.fn(),
  putMock: vi.fn(),
  deleteMock: vi.fn(),
}));

vi.mock("../../../core/api/api", () => ({
  default: {
    get: getMock,
    post: postMock,
    put: putMock,
    delete: deleteMock,
  },
}));

describe("UserManagement", () => {
  beforeEach(() => {
    getMock.mockReset();
    postMock.mockReset();
    putMock.mockReset();
    deleteMock.mockReset();
    localStorage.setItem("role", "Admin");
  });

  afterEach(() => {
    cleanup();
  });

  it("approves a pending profile update request from the approval tab", async () => {
    getMock
      .mockResolvedValueOnce([
        {
          id: "user-1",
          email: "manager@example.com",
          role: "Manager",
          fullName: "Manager",
          shopName: "Shop",
          isActive: true,
        },
      ])
      .mockResolvedValueOnce([
        {
          id: "request-1",
          email: "manager@example.com",
          currentFullName: "Manager",
          requestedFullName: "Manager Updated",
          currentAddress: "Old address",
          requestedAddress: "New address",
          currentCCCD: "111111111",
          requestedCCCD: "222222222",
          status: "Pending",
          requestedAt: "2026-04-27T10:00:00Z",
        },
      ])
      .mockResolvedValueOnce([
        {
          id: "user-1",
          email: "manager@example.com",
          role: "Manager",
          fullName: "Manager Updated",
          shopName: "Shop",
          isActive: true,
        },
      ])
      .mockResolvedValueOnce([]);

    postMock.mockResolvedValueOnce({});

    render(
      <MemoryRouter>
        <UserManagement />
      </MemoryRouter>,
    );

    expect(await screen.findByText("manager@example.com")).toBeInTheDocument();

    fireEvent.click(screen.getByRole("button", { name: /profile approval/i }));
    fireEvent.click(await screen.findByRole("button", { name: /approve/i }));

    await waitFor(() => {
      expect(postMock).toHaveBeenCalledWith("/Auth/profile-update-requests/request-1/approve", {});
    });
  });

  it("reveals a regenerated initial password otp for a manager in the user modal", async () => {
    getMock
      .mockResolvedValueOnce([
        {
          id: "user-2",
          email: "manager.setup@example.com",
          role: "Manager",
          fullName: "Setup Manager",
          shopName: "Setup Shop",
          isActive: true,
          mustChangePassword: true,
        },
      ])
      .mockResolvedValueOnce([]);

    postMock.mockResolvedValueOnce({
      userId: "user-2",
      email: "manager.setup@example.com",
      initialPasswordOtp: "654321",
      initialPasswordOtpReissued: true,
      passwordSetupToken: "setup-token-123",
      changePasswordUrl: "http://localhost:5173/change-initial-password?token=setup-token-123",
    });

    render(
      <MemoryRouter>
        <UserManagement />
      </MemoryRouter>,
    );

    expect(await screen.findByText("manager.setup@example.com")).toBeInTheDocument();

    fireEvent.click(await screen.findByRole("button", { name: /view/i }));
    fireEvent.click(screen.getByRole("button", { name: /reveal new initial otp/i }));

    await waitFor(() => {
      expect(postMock).toHaveBeenCalledWith("/Auth/debug/otp", {
        userId: "user-2",
        regenerateInitialPasswordOtp: true,
      });
    });

    expect(await screen.findByText("654321")).toBeInTheDocument();
    expect(screen.getByText("setup-token-123")).toBeInTheDocument();
    expect(screen.getByText(/change-initial-password\?token=setup-token-123/i)).toBeInTheDocument();
  });

  it("renders summary cards above the account list table", async () => {
    getMock
      .mockResolvedValueOnce([
        {
          id: "user-3",
          email: "manager.layout@example.com",
          role: "Manager",
          fullName: "Layout Manager",
          shopName: "Layout Shop",
          isActive: true,
        },
      ])
      .mockResolvedValueOnce([
        {
          id: "request-2",
          email: "manager.layout@example.com",
          status: "Pending",
        },
      ]);

    render(
      <MemoryRouter>
        <UserManagement />
      </MemoryRouter>,
    );

    expect(await screen.findByText("manager.layout@example.com")).toBeInTheDocument();

    const totalLabel = screen.getByText("Total");
    const emailHeader = screen.getByRole("columnheader", { name: "Email" });

    expect(
      totalLabel.compareDocumentPosition(emailHeader) & Node.DOCUMENT_POSITION_FOLLOWING,
    ).toBeTruthy();
  });

  it("shows business type badges for manager accounts", async () => {
    getMock
      .mockResolvedValueOnce([
        {
          id: "user-4",
          email: "food.manager@example.com",
          role: "Manager",
          fullName: "Food Manager",
          shopName: "Ocean Blue Seafood Buffet",
          businessType: "FoodCourt",
          isActive: true,
        },
      ])
      .mockResolvedValueOnce([]);

    render(
      <MemoryRouter>
        <UserManagement />
      </MemoryRouter>,
    );

    expect(await screen.findByText("food.manager@example.com")).toBeInTheDocument();
    expect(screen.getByRole("columnheader", { name: /business type/i })).toBeInTheDocument();
    expect(screen.getByText("FoodCourt")).toBeInTheDocument();
  });

  it("sends business type when updating a manager account", async () => {
    getMock
      .mockResolvedValueOnce([
        {
          id: "user-5",
          email: "manager.update@example.com",
          role: "Manager",
          fullName: "Update Manager",
          shopName: "Update Shop",
          businessType: "Shop",
          cccd: "123456789",
          address: "Old address",
          isActive: true,
        },
      ])
      .mockResolvedValueOnce([]);

    putMock.mockResolvedValueOnce({});

    render(
      <MemoryRouter>
        <UserManagement />
      </MemoryRouter>,
    );

    expect(await screen.findByText("manager.update@example.com")).toBeInTheDocument();

    fireEvent.click(screen.getByRole("button", { name: /^view$/i }));
    fireEvent.click(screen.getByRole("button", { name: /update account/i }));
    fireEvent.change(screen.getByLabelText(/business type/i), { target: { value: "FoodCourt" } });
    fireEvent.click(screen.getByRole("button", { name: /save changes/i }));

    await waitFor(() => {
      expect(putMock).toHaveBeenCalledWith("/Auth/users/user-5", expect.objectContaining({
        businessType: "FoodCourt",
      }));
    });
  });

  it("locks business type to movies for movies admin accounts", async () => {
    getMock
      .mockResolvedValueOnce([
        {
          id: "user-6",
          email: "movies.admin@example.com",
          role: "MoviesAdmin",
          fullName: "Movies Admin",
          businessType: "Movies",
          isActive: true,
        },
      ])
      .mockResolvedValueOnce([]);

    render(
      <MemoryRouter>
        <UserManagement />
      </MemoryRouter>,
    );

    expect(await screen.findByText("movies.admin@example.com")).toBeInTheDocument();

    fireEvent.click(screen.getByRole("button", { name: /^view$/i }));
    fireEvent.click(screen.getByRole("button", { name: /update account/i }));

    const businessTypeSelect = screen.getByLabelText(/business type/i);
    expect(businessTypeSelect).toBeDisabled();
    expect(businessTypeSelect).toHaveValue("Movies");
  });
});
