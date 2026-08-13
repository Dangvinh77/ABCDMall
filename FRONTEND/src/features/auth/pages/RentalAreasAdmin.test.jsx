import React from "react";
import { cleanup, fireEvent, render, screen, waitFor } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import RentalAreasAdmin from "./RentalAreasAdmin";

const { getMock, putMock, postMock } = vi.hoisted(() => ({
  getMock: vi.fn(),
  putMock: vi.fn(),
  postMock: vi.fn(),
}));

vi.mock("../../../core/api/api", () => ({
  __esModule: true,
  default: {
    get: getMock,
    put: putMock,
    post: postMock,
    delete: vi.fn(),
  },
  BASE_URL: "http://localhost:5184/api",
}));

describe("RentalAreasAdmin", () => {
  beforeEach(() => {
    getMock.mockReset();
    putMock.mockReset();
    postMock.mockReset();
    localStorage.setItem("role", "Admin");
  });

  afterEach(() => {
    cleanup();
  });

  it("loads rental area detail when opening the view modal", async () => {
    getMock
      .mockResolvedValueOnce([
        {
          id: "101",
          areaCode: "A1-01",
          floor: "1",
          areaName: "North Wing",
          size: "30m2",
          monthlyRent: 10000000,
          status: "Rented",
          tenantName: "Book World",
          remainingLeaseDays: 180,
          remainingLeaseLabel: "180 days left",
        },
      ])
      .mockResolvedValueOnce({
        id: "101",
        areaCode: "A1-01",
        floor: "1",
        areaName: "North Wing",
        size: "30m2",
        monthlyRent: 10000000,
        status: "Rented",
        tenantName: "Book World",
        managerName: "Alice Manager",
        shopName: "Book World",
        rentalLocation: "A1-01",
        leaseStartDate: "2026-04-01",
        leaseTermDays: 365,
      });

    render(
      <MemoryRouter>
        <RentalAreasAdmin />
      </MemoryRouter>,
    );

    expect(await screen.findByText("Rental Area List")).toBeInTheDocument();

    fireEvent.click(await screen.findByRole("button", { name: /view details/i }));

    expect(await screen.findByText("Rental Area Details")).toBeInTheDocument();
    expect(screen.getByText("Alice Manager")).toBeInTheDocument();

    await waitFor(() => {
      expect(getMock).toHaveBeenNthCalledWith(2, "/RentalArea/101");
    });
  });

  it("does not render the legacy add rental area form", async () => {
    getMock.mockResolvedValueOnce([]);

    render(
      <MemoryRouter>
        <RentalAreasAdmin />
      </MemoryRouter>,
    );

    expect(await screen.findByText("Rental Area List")).toBeInTheDocument();
    expect(screen.queryByText("Add Rental Area")).not.toBeInTheDocument();
    expect(screen.queryByRole("button", { name: /add area/i })).not.toBeInTheDocument();
  });
});
