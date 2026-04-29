import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { FaqFeature } from "../FaqFeature";

const supportUiAdapterMock = vi.hoisted(() => ({
  loadSupportFaqs: vi.fn(),
}));

vi.mock("../../support/api/supportUiAdapter", () => supportUiAdapterMock);

describe("FaqFeature", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("renders shared-loader categories and faq items", async () => {
    supportUiAdapterMock.loadSupportFaqs.mockResolvedValue({
      categories: [
        { id: "booking", name: "Booking" },
        { id: "showtimes", name: "Showtimes & Arrival" },
      ],
      itemsByCategory: {
        booking: [{ id: "b1", q: "How do I book?", a: "Choose movie and seats." }],
        showtimes: [{ id: "s1", q: "How early should I arrive?", a: "15 minutes early." }],
      },
      featuredItems: [{ id: "b1", q: "How do I book?", a: "Choose movie and seats." }],
    });

    render(
      <MemoryRouter>
        <FaqFeature />
      </MemoryRouter>,
    );

    expect(await screen.findByText(/movies faq/i)).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /^showtimes$/i })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /^support$/i })).toBeInTheDocument();
    expect(screen.getByText(/how do i book/i)).toBeInTheDocument();
    expect(screen.queryByText(/mall hours/i)).not.toBeInTheDocument();

    await userEvent.click(
      screen.getByRole("button", { name: /showtimes & arrival/i }),
    );

    await waitFor(() => {
      expect(screen.getByText(/how early should i arrive/i)).toBeInTheDocument();
    });
  });
});
