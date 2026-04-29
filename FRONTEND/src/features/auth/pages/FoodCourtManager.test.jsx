import React from "react";
import { cleanup, fireEvent, render, screen, waitFor } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import FoodCourtManager from "./FoodCourtManager";

const {
  getMyFoodCourtCreationStatusMock,
  getMyFoodStallsMock,
  createMyFoodStallMock,
  updateMyFoodStallMock,
  deleteMyFoodStallMock,
} = vi.hoisted(() => ({
  getMyFoodCourtCreationStatusMock: vi.fn(),
  getMyFoodStallsMock: vi.fn(),
  createMyFoodStallMock: vi.fn(),
  updateMyFoodStallMock: vi.fn(),
  deleteMyFoodStallMock: vi.fn(),
}));

vi.mock("../../food/api/foodApi", () => ({
  getMyFoodCourtCreationStatus: getMyFoodCourtCreationStatusMock,
  getMyFoodStalls: getMyFoodStallsMock,
  createMyFoodStall: createMyFoodStallMock,
  updateMyFoodStall: updateMyFoodStallMock,
  deleteMyFoodStall: deleteMyFoodStallMock,
}));

describe("FoodCourtManager", () => {
  beforeEach(() => {
    getMyFoodCourtCreationStatusMock.mockReset();
    getMyFoodStallsMock.mockReset();
    createMyFoodStallMock.mockReset();
    updateMyFoodStallMock.mockReset();
    deleteMyFoodStallMock.mockReset();
  });

  afterEach(() => {
    cleanup();
  });

  it("renders creation quota and available food-court locations", async () => {
    getMyFoodCourtCreationStatusMock.mockResolvedValue({
      stallCount: 0,
      rentedFoodCourtCount: 1,
      canCreate: true,
      message: "You can create a food stall for an available food-court rental.",
      availableRentalLocations: [{ locationSlot: "FC-01", floor: "1", areaName: "Food Court East" }],
    });
    getMyFoodStallsMock.mockResolvedValue([]);

    render(
      <MemoryRouter>
        <FoodCourtManager />
      </MemoryRouter>,
    );

    expect(await screen.findByText(/food stall quota/i)).toBeInTheDocument();
    expect(screen.getByText(/food court east/i)).toBeInTheDocument();
  });

  it("submits menu items with the food stall form", async () => {
    getMyFoodCourtCreationStatusMock.mockResolvedValue({
      stallCount: 0,
      rentedFoodCourtCount: 1,
      canCreate: true,
      message: "You can create a food stall for an available food-court rental.",
      availableRentalLocations: [{ locationSlot: "FC-01", floor: "1", areaName: "Food Court East" }],
    });
    getMyFoodStallsMock.mockResolvedValue([]);
    createMyFoodStallMock.mockResolvedValue({ id: "stall-1", name: "Boba Bella", menuItems: [] });

    render(
      <MemoryRouter>
        <FoodCourtManager />
      </MemoryRouter>,
    );

    await screen.findByText(/food stall quota/i);

    fireEvent.change(screen.getByLabelText(/stall name/i), { target: { value: "Boba Bella" } });
    fireEvent.change(screen.getByLabelText(/category/i), { target: { value: "drinks" } });
    fireEvent.change(screen.getByLabelText(/menu item name/i), { target: { value: "Brown Sugar Milk Tea" } });
    fireEvent.change(screen.getByLabelText(/menu item price/i), { target: { value: "49000" } });
    fireEvent.click(screen.getByRole("button", { name: /create food stall/i }));

    await waitFor(() => {
      expect(createMyFoodStallMock).toHaveBeenCalledWith(expect.any(FormData));
    });
  });
});
