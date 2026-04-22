import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, expect, it, vi, beforeEach } from "vitest";
import { SeatSelectionPage } from "../SeatSelectionPage";

const mockedNavigate = vi.fn();

const moviesApiMock = vi.hoisted(() => ({
  createBookingHold: vi.fn(),
  fetchMovieDetail: vi.fn(),
  fetchPromotions: vi.fn(),
  fetchSeatMap: vi.fn(),
  fetchSnackCombos: vi.fn(),
  fetchShowtimeDetail: vi.fn(),
  quoteBooking: vi.fn(),
  resolvePromotionApiIdFromUiId: vi.fn(),
}));

vi.mock("react-router-dom", async () => {
  const actual = await vi.importActual<typeof import("react-router-dom")>("react-router-dom");
  return {
    ...actual,
    useNavigate: () => mockedNavigate,
  };
});

vi.mock("../../api/moviesApi", () => moviesApiMock);

describe("SeatSelectionPage", () => {
  beforeEach(() => {
    vi.clearAllMocks();

    moviesApiMock.fetchMovieDetail.mockResolvedValue({
      id: "movie-1",
      title: "Cosmic Odyssey",
      genre: "Sci-Fi",
      rating: 4.8,
      duration: "120 min",
      imageUrl: "/poster.jpg",
      isComingSoon: false,
      ageRating: "P13",
      description: "Test movie",
      language: "English",
      director: "Jane Doe",
      cast: ["Actor A"],
    });

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
      businessDate: "2026-04-22",
      startAtUtc: "2026-04-22T11:30:00Z",
      language: "sub",
      basePrice: 85000,
      status: "Open",
      isBookable: true,
    });

    moviesApiMock.fetchSeatMap.mockResolvedValue({
      showtimeId: "showtime-1",
      hallId: "hall-1",
      hallType: "2D",
      isBookable: true,
      seats: [
        {
          seatInventoryId: "seat-a1",
          seatCode: "A1",
          row: "A",
          col: 1,
          seatType: "regular",
          status: "available",
          price: 85000,
        },
      ],
    });

    moviesApiMock.quoteBooking.mockResolvedValue({
      showtimeId: "showtime-1",
      seatSubtotal: 85000,
      serviceFeeTotal: 10000,
      comboSubtotal: 0,
      discountTotal: 0,
      grandTotal: 95000,
      lines: [],
    });

    moviesApiMock.resolvePromotionApiIdFromUiId.mockReturnValue(undefined);
    moviesApiMock.createBookingHold.mockResolvedValue({
      holdId: "hold-a1",
      holdCode: "HOLD-A1",
      showtimeId: "showtime-1",
      status: "Active",
      expiresAtUtc: "2026-04-22T11:40:00Z",
      remainingSeconds: 600,
      seatSubtotal: 85000,
      comboSubtotal: 0,
      discountAmount: 0,
      grandTotal: 95000,
      seats: [
        {
          seatInventoryId: "seat-a1",
          seatCode: "A1",
          seatType: "regular",
          price: 85000,
        },
      ],
    });
  });

  it("creates a hold immediately when an available seat is selected", async () => {
    const user = userEvent.setup();

    render(
      <MemoryRouter initialEntries={["/movies/movie-1/booking?showtimeId=showtime-1&cinema=abcd-mall&showtime=11%3A30&hallType=2D&date=2026-04-22"]}>
        <Routes>
          <Route path="/movies/:movieId/booking" element={<SeatSelectionPage />} />
        </Routes>
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(screen.getByRole("button", { name: /Seat A1/i })).toBeInTheDocument();
    });

    await user.click(screen.getByRole("button", { name: /Seat A1/i }));

    expect(moviesApiMock.createBookingHold).toHaveBeenCalledWith(
      expect.objectContaining({
        showtimeId: "showtime-1",
        seatInventoryIds: ["seat-a1"],
      }),
    );
  });
});
