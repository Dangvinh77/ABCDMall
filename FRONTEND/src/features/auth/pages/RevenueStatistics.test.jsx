import React from "react";
import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import RevenueStatistics from "./RevenueStatistics";

const { getMock } = vi.hoisted(() => ({
  getMock: vi.fn(),
}));

vi.mock("../../../core/api/api", () => ({
  __esModule: true,
  default: {
    get: getMock,
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  },
  api: {
    get: getMock,
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  },
}));

describe("RevenueStatistics", () => {
  beforeEach(() => {
    getMock.mockReset();
    localStorage.setItem("role", "Admin");
  });

  it("renders rental revenue rows from direct api payloads", async () => {
    getMock.mockResolvedValueOnce([
      {
        id: "bill-1",
        shopName: "Book World",
        rentalLocation: "1F-A01",
        month: "2026-04",
        usageMonth: "2026-03",
        leaseStartDate: "2026-01-01",
        electricityUsage: "100",
        electricityFee: 120000,
        waterUsage: "30",
        waterFee: 50000,
        serviceFee: 80000,
        leaseTermDays: 365,
        totalDue: 250000,
      },
    ]);

    render(
      <MemoryRouter>
        <RevenueStatistics />
      </MemoryRouter>,
    );

    expect(await screen.findByText("Book World")).toBeInTheDocument();
    expect(screen.getAllByText(/250\.000/).length).toBeGreaterThan(0);
  });
});
