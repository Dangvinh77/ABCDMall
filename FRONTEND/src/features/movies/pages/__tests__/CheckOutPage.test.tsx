import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { CheckoutPage } from "../CheckOutPage";

const moviesApiMock = vi.hoisted(() => ({
  createBooking: vi.fn(),
  createStripeCheckoutSession: vi.fn(),
  fetchBookingHold: vi.fn(),
  fetchPromotions: vi.fn(),
  fetchSnackCombos: vi.fn(),
  fetchShowtimeDetail: vi.fn(),
  quoteBooking: vi.fn(),
  releaseBookingHold: vi.fn(),
  resolvePromotionApiIdFromUiId: vi.fn(),
}));

vi.mock("../../api/moviesApi", () => moviesApiMock);

describe("CheckoutPage", () => {
  beforeEach(() => {
    vi.clearAllMocks();

    moviesApiMock.fetchPromotions.mockResolvedValue([]);
    moviesApiMock.fetchSnackCombos.mockResolvedValue([]);
    moviesApiMock.fetchShowtimeDetail.mockResolvedValue({
      showtimeId: "showtime-1",
      movieId: "movie-1",
      movieTitle: "Cosmic Odyssey",
      cinemaId: "cinema-1",
      cinemaName: "ABCD Mall",
      hallId: "hall-1",
      hallName: "Hall 1",
      hallType: "2D",
      businessDate: "2026-04-23",
      startAtUtc: "2026-04-23T11:30:00Z",
      language: "sub",
      basePrice: 85000,
      status: "Open",
      isBookable: true,
    });
    moviesApiMock.quoteBooking.mockResolvedValue({
      showtimeId: "showtime-1",
      seatSubtotal: 170000,
      serviceFeeTotal: 20000,
      comboSubtotal: 0,
      discountTotal: 0,
      grandTotal: 190000,
      lines: [],
    });
    moviesApiMock.resolvePromotionApiIdFromUiId.mockReturnValue(undefined);
    moviesApiMock.createBooking.mockResolvedValue({
      bookingId: "booking-1",
      bookingCode: "BK-1",
      showtimeId: "showtime-1",
      holdIds: ["hold-a1", "hold-a2"],
      status: "PendingPayment",
      grandTotal: 190000,
      currency: "VND",
      paymentRequired: true,
    });
    moviesApiMock.createStripeCheckoutSession.mockResolvedValue({
      bookingId: "booking-1",
      bookingCode: "BK-1",
      sessionId: "session-1",
      checkoutUrl: "https://checkout.test/session-1",
      expiresAtUtc: "2026-04-23T12:00:00Z",
    });

    Object.defineProperty(window, "location", {
      configurable: true,
      value: {
        ...window.location,
        assign: vi.fn(),
      },
    });
  });

  it("creates a booking from all selected hold ids", async () => {
    const user = userEvent.setup();

    render(
      <MemoryRouter
        initialEntries={[
          {
            pathname: "/movies/movie-1/checkout",
            search: "?showtimeId=showtime-1&showtime=11%3A30&hallType=2D&date=2026-04-23",
            state: {
              seats: [
                {
                  id: "A1",
                  type: "regular",
                  seatInventoryId: "seat-a1",
                  hold: {
                    holdId: "hold-a1",
                    expiresAtUtc: "2026-04-23T11:40:00Z",
                    remainingSeconds: 600,
                  },
                },
                {
                  id: "A2",
                  type: "regular",
                  seatInventoryId: "seat-a2",
                  hold: {
                    holdId: "hold-a2",
                    expiresAtUtc: "2026-04-23T11:41:00Z",
                    remainingSeconds: 601,
                  },
                },
              ],
              serviceFee: 20000,
              subtotal: 170000,
              total: 190000,
              bookingDate: "2026-04-23",
            },
          },
        ]}
      >
        <Routes>
          <Route path="/movies/:movieId/checkout" element={<CheckoutPage />} />
        </Routes>
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(screen.getByPlaceholderText(/Nguyen Van A/i)).toBeInTheDocument();
    });

    await user.type(screen.getByPlaceholderText(/Nguyen Van A/i), "Alice");
    await user.type(screen.getByPlaceholderText(/example@email.com/i), "alice@example.com");
    await user.type(screen.getByPlaceholderText(/0901 234 567/i), "0900000000");

    await user.click(screen.getByRole("button", { name: /pay with stripe/i }));

    await waitFor(() => {
      expect(moviesApiMock.createBooking).toHaveBeenCalledWith(
        expect.objectContaining({
          holdIds: ["hold-a1", "hold-a2"],
        }),
      );
    });
  });
});
