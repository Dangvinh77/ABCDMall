import { render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { MovieDetailPage } from "../MovieDetailPage";

const loadMovieDetailUiDataMock = vi.hoisted(() => vi.fn());
const fetchMovieFeedbacksMock = vi.hoisted(() => vi.fn());

vi.mock("../../api/movieUiAdapter", () => ({
  loadMovieDetailUiData: loadMovieDetailUiDataMock,
}));

vi.mock("../../api/moviesApi", () => ({
  fetchMovieFeedbacks: fetchMovieFeedbacksMock,
  createMovieFeedback: vi.fn(),
}));

describe("MovieDetailPage", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    loadMovieDetailUiDataMock.mockResolvedValue({
      movie: {
        id: "movie-now-1",
        apiId: "movie-api-1",
        title: "Cosmic Odyssey",
        imageUrl: "/poster.jpg",
        genre: "Sci-Fi, Drama",
        rating: 8.4,
        duration: "128 min",
        language: "English",
        ageRating: "T16",
        description: "A long journey through deep space.",
        director: "Jane Doe",
        cast: ["Actor One", "Actor Two"],
        isComingSoon: false,
        releaseDate: "24/04/2026",
      },
      movieSchedule: undefined,
    });
    fetchMovieFeedbacksMock.mockResolvedValue({
      movieId: "movie-api-1",
      totalCount: 1,
      averageRating: 4.5,
      ratingBreakdown: { 5: 1, 4: 0, 3: 0, 2: 0, 1: 0 },
      items: [
        {
          id: "feedback-1",
          movieId: "movie-api-1",
          displayName: "Alex",
          rating: 5,
          comment: "Great movie.",
          createdAtUtc: "2026-04-24T10:00:00Z",
        },
      ],
    });
  });

  it("renders feedback browsing content without a submit form", async () => {
    renderPage();

    expect(await screen.findByText(/ratings & feedback/i)).toBeInTheDocument();
    expect(screen.getByText(/audience score/i)).toBeInTheDocument();
    expect(screen.getByText(/feedback for this movie/i)).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: /submit feedback/i })).not.toBeInTheDocument();
    expect(screen.queryByLabelText(/^name$/i)).not.toBeInTheDocument();
    expect(
      screen.getByText(/to join feedbacks, book now and watch for the feedback link in your email after the showtime ends\./i),
    ).toBeInTheDocument();
  });

  function renderPage() {
    render(
      <MemoryRouter initialEntries={["/movies/movie-api-1"]}>
        <Routes>
          <Route path="/movies/:movieId" element={<MovieDetailPage />} />
        </Routes>
      </MemoryRouter>,
    );
  }
});
