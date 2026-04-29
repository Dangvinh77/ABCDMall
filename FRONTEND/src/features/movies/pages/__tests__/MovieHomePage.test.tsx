import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { MovieHomePage } from "../MovieHomePage";

const movieUiAdapterMock = vi.hoisted(() => ({
  loadHomeUiData: vi.fn(),
}));

const supportUiAdapterMock = vi.hoisted(() => ({
  loadSupportFaqs: vi.fn(),
}));

vi.mock("../../api/movieUiAdapter", () => movieUiAdapterMock);
vi.mock("../../../support/api/supportUiAdapter", () => supportUiAdapterMock);

describe("MovieHomePage", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    movieUiAdapterMock.loadHomeUiData.mockResolvedValue({
      nowShowingMovies: [],
      comingSoonMovies: [],
      promos: [],
    });
    supportUiAdapterMock.loadSupportFaqs.mockResolvedValue({
      categories: [{ id: "cinema", name: "Cinema" }],
      itemsByCategory: {
        cinema: [{ id: "c1", q: "Online tickets?", a: "Yes." }],
      },
      featuredItems: [{ id: "c1", q: "Online tickets?", a: "Yes." }],
    });
  });

  it("renders a support header action, preview content, and navigates to /faq", async () => {
    render(
      <MemoryRouter initialEntries={["/movies"]}>
        <Routes>
          <Route path="/movies" element={<MovieHomePage />} />
          <Route path="/faq" element={<div>FAQ PAGE</div>} />
        </Routes>
      </MemoryRouter>,
    );

    expect(await screen.findByRole("button", { name: /^support$/i })).toBeInTheDocument();
    expect(await screen.findByText(/online tickets/i)).toBeInTheDocument();

    await userEvent.click(screen.getByRole("button", { name: /^support$/i }));

    expect(await screen.findByText("FAQ PAGE")).toBeInTheDocument();
  });
});
