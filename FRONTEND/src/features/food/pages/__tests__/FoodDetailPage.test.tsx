import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import FoodDetailPage from "../FoodDetailPage";

const foodApiMock = vi.hoisted(() => ({
  getFoodBySlug: vi.fn(),
}));

vi.mock("../../api/foodApi", () => foodApiMock);

describe("FoodDetailPage", () => {
  beforeEach(() => {
    vi.clearAllMocks();

    foodApiMock.getFoodBySlug.mockResolvedValue({
      id: "food-001",
      slug: "starbucks",
      name: "STARBUCKS",
      description: "Coffee and drinks in the food court.",
      imageUrl: "/img/starbuck/logo.png",
      categorySlug: "coffee",
      mallSlug: "ABCD Mall",
    });
  });

  it("opens the menu modal from the hero action", async () => {
    const user = userEvent.setup();

    render(
      <MemoryRouter initialEntries={["/food/starbucks"]}>
        <Routes>
          <Route path="/food/:slug" element={<FoodDetailPage />} />
        </Routes>
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(screen.getByRole("heading", { level: 1, name: /starbucks/i })).toBeInTheDocument();
    });

    await user.click(screen.getAllByRole("button", { name: /view menu/i }).at(0)!);

    expect(screen.getByRole("heading", { name: "Signature dishes and drinks" })).toBeInTheDocument();
    expect(screen.getAllByText("Menu preview").length).toBeGreaterThan(0);
  });
});
