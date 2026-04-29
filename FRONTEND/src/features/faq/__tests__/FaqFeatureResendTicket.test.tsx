import { cleanup, render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { FaqFeature } from "../FaqFeature";

const supportUiAdapterMock = vi.hoisted(() => ({
  loadSupportFaqs: vi.fn(),
}));

const moviesApiMock = vi.hoisted(() => ({
  resendTicketEmail: vi.fn(),
}));

vi.mock("../../support/api/supportUiAdapter", () => supportUiAdapterMock);
vi.mock("../../movies/api/moviesApi", () => moviesApiMock);

describe("FaqFeature resend ticket", () => {
  afterEach(() => {
    cleanup();
  });

  beforeEach(() => {
    vi.clearAllMocks();
    supportUiAdapterMock.loadSupportFaqs.mockResolvedValue({
      categories: [{ id: "booking", name: "Booking" }],
      itemsByCategory: {
        booking: [{ id: "b1", q: "How do I book?", a: "Choose movie and seats." }],
      },
      featuredItems: [{ id: "b1", q: "How do I book?", a: "Choose movie and seats." }],
    });
  });

  it("submits email and booking code then shows success", async () => {
    moviesApiMock.resendTicketEmail.mockResolvedValue({
      message: "Ticket email resent successfully.",
    });

    render(
      <MemoryRouter>
        <FaqFeature />
      </MemoryRouter>,
    );

    await screen.findByText(/didn't receive your ticket email/i);

    await userEvent.type(
      screen.getByLabelText(/email address/i),
      "guest@example.com",
    );
    await userEvent.type(screen.getByLabelText(/booking code/i), "BK-123");
    await userEvent.click(
      screen.getByRole("button", { name: /resend ticket email/i }),
    );

    await waitFor(() => {
      expect(
        screen.getByText(/ticket email resent successfully/i),
      ).toBeInTheDocument();
    });
  });

  it("shows backend business errors inline", async () => {
    moviesApiMock.resendTicketEmail.mockRejectedValue(
      new Error("Email does not match this booking."),
    );

    render(
      <MemoryRouter>
        <FaqFeature />
      </MemoryRouter>,
    );

    await screen.findByText(/didn't receive your ticket email/i);

    await userEvent.type(
      screen.getByLabelText(/email address/i),
      "wrong@example.com",
    );
    await userEvent.type(screen.getByLabelText(/booking code/i), "BK-123");
    await userEvent.click(
      screen.getByRole("button", { name: /resend ticket email/i }),
    );

    await waitFor(() => {
      expect(
        screen.getByText(/email does not match this booking/i),
      ).toBeInTheDocument();
    });
  });
});
