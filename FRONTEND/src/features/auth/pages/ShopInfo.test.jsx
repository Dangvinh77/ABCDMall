import React from "react";
import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import ShopInfo from "./ShopInfo";

const { getMock } = vi.hoisted(() => ({
  getMock: vi.fn(),
}));
const { postMock } = vi.hoisted(() => ({
  postMock: vi.fn(),
}));

vi.mock("../../../core/api/api", () => ({
  __esModule: true,
  default: {
    get: getMock,
    post: postMock,
    put: vi.fn(),
    delete: vi.fn(),
  },
}));

describe("ShopInfo", () => {
  beforeEach(() => {
    getMock.mockReset();
    postMock.mockReset();
    localStorage.setItem("role", "Manager");
    Object.defineProperty(window, "location", {
      writable: true,
      value: {
        ...window.location,
        href: "http://localhost/shop-info",
      },
    });
  });

  it("loads rental information separately from monthly bills", async () => {
    getMock
      .mockResolvedValueOnce({
        shopName: "Book World",
        rentalLocation: "B2-09",
        floor: "Floor 2",
        leaseStartDate: "2026-04-01",
        managerName: "Alice Manager",
        cccd: "123456789",
        electricityFee: 120000,
        waterFee: 50000,
        serviceFee: 80000,
        leaseTermDays: 365,
      })
      .mockResolvedValueOnce([
        {
          id: "bill-1",
          shopName: "Book World",
          rentalLocation: "B2-09",
          month: "2026-04",
          totalDue: 250000,
          paymentStatus: "Unpaid",
        },
      ]);

    render(
      <MemoryRouter>
        <ShopInfo />
      </MemoryRouter>,
    );

    expect(await screen.findByText("Current Rental Information")).toBeInTheDocument();
    expect(screen.getAllByText("Book World").length).toBeGreaterThan(0);
    expect(screen.getAllByText("B2-09").length).toBeGreaterThan(0);

    await waitFor(() => {
      expect(getMock).toHaveBeenNthCalledWith(1, "/ShopInfo/rental-information");
      expect(getMock).toHaveBeenNthCalledWith(2, "/ShopInfo");
    });
  });

  it("starts checkout for an unpaid bill from the rental detail modal", async () => {
    getMock
      .mockResolvedValueOnce({
        shopName: "Book World",
        rentalLocation: "B2-09",
        floor: "Floor 2",
        leaseStartDate: "2026-04-01",
        managerName: "Alice Manager",
        cccd: "123456789",
        electricityFee: 120000,
        waterFee: 50000,
        serviceFee: 80000,
        leaseTermDays: 365,
      })
      .mockResolvedValueOnce([
        {
          id: "bill-1",
          shopName: "Book World",
          rentalLocation: "B2-09",
          month: "2026-04",
          usageMonth: "2026-03",
          totalDue: 250000,
          paymentStatus: "Unpaid",
        },
      ]);
    postMock.mockResolvedValueOnce({
      checkoutUrl: "https://checkout.stripe.test/session",
    });

    render(
      <MemoryRouter>
        <ShopInfo />
      </MemoryRouter>,
    );

    fireEvent.click(await screen.findByRole("button", { name: "View Details" }));
    fireEvent.click(await screen.findByRole("button", { name: "Pay Now" }));

    await waitFor(() => {
      expect(postMock).toHaveBeenCalledWith("/RentalPayments/bill-1/checkout-session");
    });
    expect(window.location.href).toBe("https://checkout.stripe.test/session");
  });
});
