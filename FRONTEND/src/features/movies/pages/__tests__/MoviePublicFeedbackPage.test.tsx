import { render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { MoviePublicFeedbackPage } from "../MoviePublicFeedbackPage";

const moviesApiMock = vi.hoisted(() => ({
  fetchMovieFeedbacks: vi.fn(),
  fetchPublicMovieFeedbackRequest: vi.fn(),
  submitMovieFeedbackByToken: vi.fn(),
}));

vi.mock("../../api/moviesApi", () => moviesApiMock);

describe("MoviePublicFeedbackPage", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    moviesApiMock.fetchMovieFeedbacks.mockResolvedValue({
      movieId: "movie-1",
      totalCount: 0,
      averageRating: 0,
      ratingBreakdown: { 5: 0, 4: 0, 3: 0, 2: 0, 1: 0 },
      items: [],
    });
  });

  it("renders an expired-after-open message without the feedback form", async () => {
    moviesApiMock.fetchPublicMovieFeedbackRequest.mockResolvedValue({
      feedbackRequestId: "request-1",
      movieId: "movie-1",
      showtimeId: "showtime-1",
      movieTitle: "Cosmic Odyssey",
      availableAtUtc: "2026-04-24T10:00:00Z",
      firstOpenedAtUtc: "2026-04-24T11:00:00Z",
      expiresAtUtc: null,
      status: "Expired",
      expiredReason: "OpenedNoSubmission7Days",
      remainingSubmissions: 3,
      canSubmit: false,
      message: "Feedback link expired after 7 days without a submission.",
    });

    renderPage();

    expect(
      await screen.findByText(/Link feedback đã hết hạn sau 7 ngày kể từ lần mở đầu tiên\./i),
    ).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: /gửi feedback/i })).not.toBeInTheDocument();
  });

  it("renders the feedback form only when the request can still submit", async () => {
    moviesApiMock.fetchPublicMovieFeedbackRequest.mockResolvedValue({
      feedbackRequestId: "request-2",
      movieId: "movie-1",
      showtimeId: "showtime-1",
      movieTitle: "Cosmic Odyssey",
      availableAtUtc: "2026-04-24T10:00:00Z",
      firstOpenedAtUtc: "2026-04-24T11:00:00Z",
      expiresAtUtc: null,
      status: "Sent",
      expiredReason: null,
      remainingSubmissions: 2,
      canSubmit: true,
      message: null,
    });

    renderPage();

    expect(await screen.findByRole("button", { name: /gửi feedback/i })).toBeInTheDocument();
    expect(screen.getByText(/Còn lại 2 lần gửi feedback/i)).toBeInTheDocument();
  });

  function renderPage() {
    render(
      <MemoryRouter initialEntries={["/movies/feedback/token-123"]}>
        <Routes>
          <Route path="/movies/feedback/:token" element={<MoviePublicFeedbackPage />} />
        </Routes>
      </MemoryRouter>,
    );
  }
});
