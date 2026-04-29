import React from "react";
import { cleanup, render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import DashboardMall from "./Dashboard";

const { logoutUserMock, getManagerBusinessRouteMock } = vi.hoisted(() => ({
  logoutUserMock: vi.fn(),
  getManagerBusinessRouteMock: vi.fn(),
}));

vi.mock("../services/auth", () => ({
  logoutUser: logoutUserMock,
}));

vi.mock("../services/managerBusinessApi", () => ({
  getManagerBusinessRoute: getManagerBusinessRouteMock,
}));

describe("DashboardMall", () => {
  beforeEach(() => {
    logoutUserMock.mockReset();
    getManagerBusinessRouteMock.mockReset();
    localStorage.clear();
  });

  afterEach(() => {
    cleanup();
  });

  it("shows one dynamic business management entry for manager accounts", async () => {
    localStorage.setItem("role", "Manager");
    getManagerBusinessRouteMock.mockResolvedValue({
      businessType: "FoodCourt",
      targetPath: "/food-court-manager",
      hasEligibleRental: true,
    });

    render(
      <MemoryRouter>
        <DashboardMall />
      </MemoryRouter>,
    );

    const businessLink = await screen.findByRole("link", { name: /my business management/i });
    expect(businessLink).toHaveAttribute("href", "/food-court-manager");
    expect(screen.queryByRole("link", { name: /manage my shop/i })).not.toBeInTheDocument();
  });
});
