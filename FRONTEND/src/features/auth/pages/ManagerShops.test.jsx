import React from "react";
import { render, screen, waitFor } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import ManagerShops from "./ManagerShops";

const {
  getMyManagedShopsMock,
  getMyShopCreationStatusMock,
  createMyManagedShopMock,
  updateMyManagedShopMock,
  deleteMyManagedShopMock,
} = vi.hoisted(() => ({
  getMyManagedShopsMock: vi.fn(),
  getMyShopCreationStatusMock: vi.fn(),
  createMyManagedShopMock: vi.fn(),
  updateMyManagedShopMock: vi.fn(),
  deleteMyManagedShopMock: vi.fn(),
}));

vi.mock("../../shops/api/shopApi", () => ({
  getMyManagedShops: getMyManagedShopsMock,
  getMyShopCreationStatus: getMyShopCreationStatusMock,
  createMyManagedShop: createMyManagedShopMock,
  updateMyManagedShop: updateMyManagedShopMock,
  deleteMyManagedShop: deleteMyManagedShopMock,
}));

describe("ManagerShops", () => {
  beforeEach(() => {
    getMyManagedShopsMock.mockReset();
    getMyShopCreationStatusMock.mockReset();
  });

  it("auto-selects the only available rented location for new shop creation", async () => {
    getMyManagedShopsMock.mockResolvedValueOnce([]);
    getMyShopCreationStatusMock.mockResolvedValueOnce({
      shopCount: 0,
      rentedAreaCount: 1,
      canCreate: true,
      message: "You can create one shop.",
      availableRentalLocations: [
        {
          locationSlot: "B2-09",
          floor: "Floor 2",
          areaName: "North Wing",
        },
      ],
    });

    render(
      <MemoryRouter>
        <ManagerShops />
      </MemoryRouter>,
    );

    expect(await screen.findByText("Add new shop")).toBeInTheDocument();

    await waitFor(() => {
      expect(screen.getByLabelText("Floor")).toHaveValue("Floor 2");
      expect(screen.getByLabelText("Location")).toHaveValue("B2-09");
    });
  });
});
