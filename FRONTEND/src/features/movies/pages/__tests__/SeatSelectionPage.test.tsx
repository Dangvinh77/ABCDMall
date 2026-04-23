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
  releaseBookingHold: vi.fn(),
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

    await user.click(getSeatMapSeatButton());

    expect(moviesApiMock.createBookingHold).toHaveBeenCalledWith(
      expect.objectContaining({
        showtimeId: "showtime-1",
        seatInventoryIds: ["seat-a1"],
      }),
    );
  });

  it("does not render the old page-level countdown in the header", async () => {
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

    expect(screen.queryByText("10:00")).not.toBeInTheDocument();
  });

  it("does not start a page-level timer loop before any seat is selected", async () => {
    const intervalSpy = vi.spyOn(window, "setInterval");

    render(
      <MemoryRouter initialEntries={["/movies/movie-1/booking?showtimeId=showtime-1&cinema=abcd-mall&showtime=11%3A30&hallType=2D&date=2026-04-22"]}>
        <Routes>
          <Route path="/movies/:movieId/booking" element={<SeatSelectionPage />} />
        </Routes>
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(getSeatMapSeatButton()).toBeInTheDocument();
    });

    expect(intervalSpy.mock.calls.some(([, delay]) => delay === 1000)).toBe(false);
    intervalSpy.mockRestore();
  });

  it("releases the specific seat hold when the selected seat is clicked again", async () => {
    const user = userEvent.setup();

    render(
      <MemoryRouter initialEntries={["/movies/movie-1/booking?showtimeId=showtime-1&cinema=abcd-mall&showtime=11%3A30&hallType=2D&date=2026-04-22"]}>
        <Routes>
          <Route path="/movies/:movieId/booking" element={<SeatSelectionPage />} />
        </Routes>
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(getSeatMapSeatButton()).toBeInTheDocument();
    });

    await user.click(getSeatMapSeatButton());
    await user.click(getSeatMapSeatButton());

    expect(moviesApiMock.createBookingHold).toHaveBeenCalledTimes(1);
    expect(moviesApiMock.releaseBookingHold).toHaveBeenCalledWith("hold-a1");
  });

  it("creates one hold for both seats in a couple pair when a couple seat is selected", async () => {
    const user = userEvent.setup();

    moviesApiMock.fetchSeatMap.mockResolvedValueOnce({
      showtimeId: "showtime-1",
      hallId: "hall-1",
      hallType: "2D",
      isBookable: true,
      seats: [
        {
          seatInventoryId: "seat-h1",
          seatCode: "H1",
          row: "H",
          col: 1,
          seatType: "couple",
          status: "available",
          price: 95000,
        },
        {
          seatInventoryId: "seat-h2",
          seatCode: "H2",
          row: "H",
          col: 2,
          seatType: "couple",
          status: "available",
          price: 95000,
        },
      ],
    });

    moviesApiMock.createBookingHold.mockResolvedValueOnce({
      holdId: "hold-h1h2",
      holdCode: "HOLD-H1H2",
      showtimeId: "showtime-1",
      status: "Active",
      expiresAtUtc: "2026-04-22T11:40:00Z",
      remainingSeconds: 600,
      seatSubtotal: 190000,
      comboSubtotal: 0,
      discountAmount: 0,
      grandTotal: 210000,
      seats: [
        {
          seatInventoryId: "seat-h1",
          seatCode: "H1",
          seatType: "couple",
          price: 95000,
        },
        {
          seatInventoryId: "seat-h2",
          seatCode: "H2",
          seatType: "couple",
          price: 95000,
        },
      ],
    });

    render(
      <MemoryRouter initialEntries={["/movies/movie-1/booking?showtimeId=showtime-1&cinema=abcd-mall&showtime=11%3A30&hallType=2D&date=2026-04-22"]}>
        <Routes>
          <Route path="/movies/:movieId/booking" element={<SeatSelectionPage />} />
        </Routes>
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(screen.getByRole("button", { name: /Seat H1/i })).toBeInTheDocument();
    });

    await user.click(screen.getByRole("button", { name: /Seat H1/i }));

    expect(moviesApiMock.createBookingHold).toHaveBeenCalledWith(
      expect.objectContaining({
        showtimeId: "showtime-1",
        seatInventoryIds: ["seat-h1", "seat-h2"],
      }),
    );
    expect(screen.getByText("H1")).toBeInTheDocument();
    expect(screen.getByText("H2")).toBeInTheDocument();
  });

  it("uses coupleGroupCode instead of a hardcoded row when selecting a couple seat", async () => {
    const user = userEvent.setup();

    moviesApiMock.fetchSeatMap.mockResolvedValueOnce({
      showtimeId: "showtime-1",
      hallId: "hall-1",
      hallType: "2D",
      isBookable: true,
      seats: [
        {
          seatInventoryId: "seat-j7",
          seatCode: "J7",
          row: "J",
          col: 7,
          seatType: "couple",
          status: "available",
          price: 95000,
          coupleGroupCode: "J-PAIR-4",
        },
        {
          seatInventoryId: "seat-j8",
          seatCode: "J8",
          row: "J",
          col: 8,
          seatType: "couple",
          status: "available",
          price: 95000,
          coupleGroupCode: "J-PAIR-4",
        },
      ],
    });

    moviesApiMock.createBookingHold.mockResolvedValueOnce({
      holdId: "hold-j7j8",
      holdCode: "HOLD-J7J8",
      showtimeId: "showtime-1",
      status: "Active",
      expiresAtUtc: "2026-04-22T11:40:00Z",
      remainingSeconds: 600,
      seatSubtotal: 190000,
      comboSubtotal: 0,
      discountAmount: 0,
      grandTotal: 210000,
      seats: [
        {
          seatInventoryId: "seat-j7",
          seatCode: "J7",
          seatType: "couple",
          price: 95000,
        },
        {
          seatInventoryId: "seat-j8",
          seatCode: "J8",
          seatType: "couple",
          price: 95000,
        },
      ],
    });

    render(
      <MemoryRouter initialEntries={["/movies/movie-1/booking?showtimeId=showtime-1&cinema=abcd-mall&showtime=11%3A30&hallType=2D&date=2026-04-22"]}>
        <Routes>
          <Route path="/movies/:movieId/booking" element={<SeatSelectionPage />} />
        </Routes>
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(screen.getByRole("button", { name: /Seat J7/i })).toBeInTheDocument();
    });

    await user.click(screen.getByRole("button", { name: /Seat J7/i }));

    expect(moviesApiMock.createBookingHold).toHaveBeenCalledWith(
      expect.objectContaining({
        showtimeId: "showtime-1",
        seatInventoryIds: ["seat-j7", "seat-j8"],
      }),
    );
    expect(screen.getByText("J7")).toBeInTheDocument();
    expect(screen.getByText("J8")).toBeInTheDocument();
  });
});

function getSeatMapSeatButton() {
  return screen.getAllByRole("button", { name: /Seat A1/i }).at(0)!;
}
