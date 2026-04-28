import { render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import ShopDetailPage from "./ShopDetailPage";

const { getShopBySlugMock, getShopsMock } = vi.hoisted(() => ({
  getShopBySlugMock: vi.fn(),
  getShopsMock: vi.fn(),
}));

vi.mock("../api/shopApi", () => ({
  getShopBySlug: getShopBySlugMock,
  getShops: getShopsMock,
}));

vi.mock("../../../core/utils/image", () => ({
  getImageUrl: (value: string) => value,
}));

describe("ShopDetailPage", () => {
  beforeEach(() => {
    getShopBySlugMock.mockReset();
    getShopsMock.mockReset();
  });

  it("shows similar brands and links back to /brands", async () => {
    getShopBySlugMock.mockResolvedValueOnce({
      id: "shop-1",
      name: "Book World",
      slug: "book-world",
      category: "Books",
      location: "East Wing",
      summary: "Books and gifts",
      description: "A detailed description",
      imageUrl: "/cover.jpg",
      logoUrl: "/logo.jpg",
      coverImageUrl: "/hero.jpg",
      openHours: "10:00 - 22:00",
      floor: "2F",
      locationSlot: "A-01",
      tags: ["books"],
      products: [],
      vouchers: [],
    });
    getShopsMock.mockResolvedValueOnce([
      {
        id: "shop-2",
        name: "Readers Corner",
        slug: "readers-corner",
        category: "Books",
        location: "West Wing",
        summary: "Readers club",
        description: "Readers club",
        imageUrl: "/reader-cover.jpg",
        logoUrl: "/reader-logo.jpg",
        openHours: "10:00 - 22:00",
        tags: ["books"],
      },
    ]);

    render(
      <MemoryRouter initialEntries={["/shops/book-world"]}>
        <Routes>
          <Route path="/shops/:slug" element={<ShopDetailPage />} />
        </Routes>
      </MemoryRouter>,
    );

    expect(await screen.findByText("Readers Corner")).toBeInTheDocument();
    expect(screen.getByRole("link", { name: /Back to all shops/i })).toHaveAttribute("href", "/brands");
  });
});
