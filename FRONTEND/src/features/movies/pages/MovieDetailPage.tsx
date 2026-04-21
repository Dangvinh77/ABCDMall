import { useEffect, useMemo, useState } from 'react';
import { useLocation, useNavigate, useParams, useSearchParams } from 'react-router-dom';
import {
  ArrowLeft,
  Star,
  Clock,
  Calendar,
  Film,
  Users,
  Globe,
  Shield,
  ChevronRight,
  Ticket,
  Play,
  MessageSquare,
  Send,
} from 'lucide-react';
import { getDefaultBookingDate } from '../data/promotions';
import { formatScheduleDateParam, getScheduleDates, type MovieSchedule } from '../data/schedules';
import { Button } from '../component/ui/button';
import { Badge } from '../component/ui/badge';
import { moviePaths } from '../routes/moviePaths';
import { loadMovieDetailUiData } from '../api/movieUiAdapter';
import { createMovieFeedback, fetchMovieFeedbacks } from '../api/moviesApi';
import type { Movie } from '../data/movie';

interface MovieFeedback {
  id: string;
  author: string;
  rating: number;
  comment: string;
  createdAt: string;
}

const ratingOptions = [5, 4, 3, 2, 1];

function getFeedbackStorageKey(movieId: string) {
  return `abcd-cinema-feedback:${movieId}`;
}

function buildDefaultMovieFeedback(movie: Movie): MovieFeedback[] {
  const leadGenre = movie.genre.split(', ')[0] ?? 'Movie';

  return [
    {
      id: `${movie.id}-review-1`,
      author: 'Minh Anh',
      rating: Math.min(5, Math.max(4, Math.round(movie.rating / 2))),
      comment: `${movie.title} co nhip phim cuon, hinh anh dep va phan ${leadGenre.toLowerCase()} duoc lam kha tron ven.`,
      createdAt: '2 ngay truoc',
    },
    {
      id: `${movie.id}-review-2`,
      author: 'Gia Huy',
      rating: Math.min(5, Math.max(3, Math.floor(movie.rating / 2))),
      comment: 'Trai nghiem xem rat on, am thanh tot. Minh thich cach phim giu cam xuc den doan cuoi.',
      createdAt: '5 ngay truoc',
    },
  ];
}

function formatFeedbackDate(value: string) {
  const parsed = new Date(value);
  if (Number.isNaN(parsed.getTime())) {
    return value;
  }

  return parsed.toLocaleDateString('en-GB', {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
  });
}

export function MovieDetailPage() {
  const { movieId } = useParams<{ movieId: string }>();
  const navigate = useNavigate();
  const location = useLocation();
  const [searchParams] = useSearchParams();
  const availableDates = getScheduleDates(7);
  const initialBookingDate = searchParams.get('date') ?? getDefaultBookingDate();
  const bookingDate =
    availableDates.find((dateOption) => formatScheduleDateParam(dateOption.date) === initialBookingDate)
      ? initialBookingDate
      : formatScheduleDateParam(availableDates[0].date);
  const [apiMovie, setApiMovie] = useState<Movie | undefined>();
  const [apiMovieSchedule, setApiMovieSchedule] = useState<MovieSchedule | undefined>();
  const [isLoading, setIsLoading] = useState(Boolean(movieId));
  const [submittedFeedback, setSubmittedFeedback] = useState<MovieFeedback[]>([]);
  const [reviewerName, setReviewerName] = useState('');
  const [reviewRating, setReviewRating] = useState(5);
  const [reviewComment, setReviewComment] = useState('');
  const [feedbackRatingFilter, setFeedbackRatingFilter] = useState<number | null>(null);
  const [apiAverageRating, setApiAverageRating] = useState<number | null>(null);
  const [apiRatingBreakdown, setApiRatingBreakdown] = useState<Record<number, number> | null>(null);
  const [feedbackSubmitted, setFeedbackSubmitted] = useState(false);
  const movie = apiMovie;
  const movieSchedule = apiMovieSchedule;
  const scheduleMovieId =
    movieSchedule?.movie.id.replace(/-(now|soon)-\d+$/, '') ??
    movie?.id.replace(/-(now|soon)-\d+$/, '') ??
    movieId?.replace(/-(now|soon)-\d+$/, '');
  const feedbackMovieId = movie?.apiId ?? movieId;
  const feedbackStorageKey = movie ? getFeedbackStorageKey(movie.id) : undefined;
  const defaultFeedback = useMemo(() => (movie ? buildDefaultMovieFeedback(movie) : []), [movie]);
  const feedbacks = useMemo(
    () => {
      const source = submittedFeedback.length > 0 ? submittedFeedback : defaultFeedback;
      return feedbackRatingFilter ? source.filter((feedback) => feedback.rating === feedbackRatingFilter) : source;
    },
    [defaultFeedback, feedbackRatingFilter, submittedFeedback],
  );
  const averageFeedbackRating = useMemo(() => {
    if (apiAverageRating !== null) {
      return apiAverageRating;
    }

    if (feedbacks.length === 0) {
      return movie?.rating ? movie.rating / 2 : 0;
    }

    return feedbacks.reduce((total, feedback) => total + feedback.rating, 0) / feedbacks.length;
  }, [apiAverageRating, feedbacks, movie?.rating]);

  const buildShowtimesUrl = () => {
    const params = new URLSearchParams(location.search);
    params.set('movie', scheduleMovieId ?? movieId ?? '');
    params.set('date', bookingDate);

    return `${moviePaths.showtimes()}?${params.toString()}`;
  };

  useEffect(() => {
    if (!movieId) return;

    let active = true;
    const currentMovieId = movieId;

    async function loadMovieFromApi() {
      if (active) {
        setIsLoading(true);
      }

      try {
        const data = await loadMovieDetailUiData(currentMovieId, bookingDate);
        if (!active) return;

        setApiMovie(data.movie);
        setApiMovieSchedule(data.movieSchedule);
      } catch (error) {
        if (active) {
          setApiMovie(undefined);
          setApiMovieSchedule(undefined);
        }
        console.warn('Movie detail API failed.', error);
      } finally {
        if (active) {
          setIsLoading(false);
        }
      }
    }

    void loadMovieFromApi();

    return () => {
      active = false;
    };
  }, [bookingDate, movieId]);

  useEffect(() => {
    if (!feedbackStorageKey || !feedbackMovieId) {
      setSubmittedFeedback([]);
      return;
    }

    let active = true;
    const storageKey = feedbackStorageKey;

    async function loadFeedbacks() {
      try {
        const response = await fetchMovieFeedbacks(feedbackMovieId as string, feedbackRatingFilter);
        if (!active) return;

        setSubmittedFeedback(response.items.map((feedback) => ({
          id: feedback.id,
          author: feedback.displayName,
          rating: feedback.rating,
          comment: feedback.comment,
          createdAt: formatFeedbackDate(feedback.createdAtUtc),
        })));
        setApiAverageRating(response.averageRating);
        setApiRatingBreakdown(response.ratingBreakdown);
      } catch (error) {
        if (!active) return;

        try {
          const raw = localStorage.getItem(storageKey);
          setSubmittedFeedback(raw ? (JSON.parse(raw) as MovieFeedback[]) : []);
        } catch {
          setSubmittedFeedback([]);
        }
        setApiAverageRating(null);
        setApiRatingBreakdown(null);
        console.warn('Movie feedback API failed; using bundled/local fallback feedback.', error);
      }
    }

    void loadFeedbacks();
    setFeedbackSubmitted(false);
    setReviewerName('');
    setReviewRating(5);
    setReviewComment('');
    return () => {
      active = false;
    };
  }, [feedbackMovieId, feedbackRatingFilter, feedbackStorageKey]);

  const handleFeedbackSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!feedbackStorageKey || !reviewComment.trim()) {
      return;
    }

    try {
      if (!feedbackMovieId) {
        throw new Error('Movie id is missing.');
      }

      const created = await createMovieFeedback(feedbackMovieId, {
        rating: reviewRating,
        comment: reviewComment.trim(),
        displayName: reviewerName.trim() || undefined,
      });

      const newFeedback: MovieFeedback = {
        id: created.id,
        author: created.displayName,
        rating: created.rating,
        comment: created.comment,
        createdAt: formatFeedbackDate(created.createdAtUtc),
      };

      setSubmittedFeedback((currentFeedback) => [newFeedback, ...currentFeedback]);
      setApiAverageRating(null);
    } catch (error) {
      console.warn('Create feedback API failed; saving feedback locally for this browser.', error);
      const newFeedback: MovieFeedback = {
        id:
          typeof crypto !== 'undefined' && 'randomUUID' in crypto
            ? crypto.randomUUID()
            : `${Date.now()}`,
        author: reviewerName.trim() || 'Khach hang ABCD',
        rating: reviewRating,
        comment: reviewComment.trim(),
        createdAt: 'Vua xong',
      };

      setSubmittedFeedback((currentFeedback) => {
        const nextFeedback = [newFeedback, ...currentFeedback];
        localStorage.setItem(feedbackStorageKey, JSON.stringify(nextFeedback));
        return nextFeedback;
      });
    }

    setReviewerName('');
    setReviewRating(5);
    setReviewComment('');
    setFeedbackSubmitted(true);
  };

  if (isLoading) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-gray-950">
        <div className="text-center text-gray-300">Loading movie details...</div>
      </div>
    );
  }

  if (!movie) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-gray-950">
        <div className="text-center">
          <Film className="mx-auto mb-4 size-16 text-gray-600" />
          <h2 className="mb-2 text-2xl font-bold text-white">Movie not found</h2>
          <p className="mb-6 text-gray-400">
            The movie you are looking for does not exist or has been removed.
          </p>
          <Button
            onClick={() => navigate(moviePaths.home())}
            className="bg-gradient-to-r from-purple-600 to-pink-600 hover:from-purple-700 hover:to-pink-700"
          >
            <ArrowLeft className="mr-2 size-4" />
            Back to homepage
          </Button>
        </div>
      </div>
    );
  }

  const ageRatingColors: Record<string, string> = {
    P: 'bg-green-600',
    T13: 'bg-yellow-600',
    T16: 'bg-orange-600',
    T18: 'bg-red-600',
  };

  return (
    <div className="min-h-screen bg-gradient-to-b from-gray-950 via-gray-900 to-gray-950">
      <header className="relative z-40 border-b border-gray-800 bg-gray-950/90 backdrop-blur-lg">
        <div className="container mx-auto px-4 py-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <Button
                variant="ghost"
                size="sm"
                onClick={() => navigate(moviePaths.home())}
                className="text-gray-300 hover:text-white"
              >
                <ArrowLeft className="mr-2 size-4" />
                <span className="hidden sm:inline">Back</span>
              </Button>
              <div className="h-5 w-px bg-gray-700" />
              <div className="flex items-center gap-2">
                <div className="rounded-full bg-gradient-to-br from-purple-600 to-pink-600 p-1.5">
                  <Film className="size-4 text-white" />
                </div>
                <span className="font-bold text-white">ABCD Cinema</span>
              </div>
            </div>
          </div>
        </div>
      </header>

      <div className="relative z-0 h-72 overflow-hidden sm:h-96 md:h-[480px]">
        <img src={movie.imageUrl} alt={movie.title} className="size-full object-cover object-center" />
        <div className="absolute inset-0 bg-gradient-to-t from-gray-950 via-gray-950/60 to-transparent" />
        <div className="absolute inset-0 bg-gradient-to-r from-gray-950/70 via-transparent to-transparent" />

        <div className="absolute inset-0 flex items-center justify-center">
          <button className="group flex size-16 items-center justify-center rounded-full border-2 border-white/40 bg-white/10 backdrop-blur-sm transition-all hover:scale-110 hover:border-white/80 hover:bg-white/20 sm:size-20">
            <Play className="size-7 translate-x-0.5 fill-white text-white sm:size-9" />
          </button>
        </div>
      </div>

      <div className="relative z-10 container mx-auto px-4 pb-16">
        <div className="relative z-10 -mt-32 grid gap-8 md:-mt-40 lg:grid-cols-[300px_1fr] lg:-mt-52 xl:grid-cols-[340px_1fr]">
          <div className="flex justify-center lg:justify-start">
            <div className="relative w-48 overflow-hidden rounded-2xl shadow-2xl ring-2 ring-purple-500/30 sm:w-56 md:w-64 lg:w-full">
              <img src={movie.imageUrl} alt={movie.title} className="aspect-[2/3] w-full object-cover" />
              <div className="absolute left-3 top-3">
                <span
                  className={`rounded-md px-2 py-1 text-xs font-bold text-white ${ageRatingColors[movie.ageRating] ?? 'bg-gray-600'}`}
                >
                  {movie.ageRating}
                </span>
              </div>
              {movie.isComingSoon && (
                <div className="absolute inset-x-0 bottom-0 bg-gradient-to-t from-blue-900/90 to-transparent p-4 text-center">
                  <span className="text-sm font-semibold text-blue-300">Coming soon</span>
                  <p className="text-xs text-blue-200">{movie.releaseDate}</p>
                </div>
              )}
            </div>
          </div>

          <div className="space-y-6 pt-4 lg:pt-8">
            <div className="space-y-3">
              {movie.isComingSoon && (
                <Badge className="bg-gradient-to-r from-blue-600 to-cyan-600 text-white">
                  Coming soon &bull; {movie.releaseDate}
                </Badge>
              )}
              <h1 className="text-3xl font-bold text-white sm:text-4xl md:text-5xl">{movie.title}</h1>

              <div className="flex flex-wrap items-center gap-4">
                <div className="flex items-center gap-2 rounded-full bg-yellow-500/10 px-3 py-1.5 ring-1 ring-yellow-500/30">
                  <Star className="size-5 fill-yellow-400 text-yellow-400" />
                  <span className="font-bold text-yellow-400">{movie.rating}</span>
                  <span className="text-sm text-yellow-400/70">/10</span>
                </div>
                <div className="flex flex-wrap gap-2">
                  {movie.genre.split(', ').map((g) => (
                    <Badge
                      key={g}
                      variant="secondary"
                      className="bg-gray-800 text-gray-200 hover:bg-gray-700"
                    >
                      {g}
                    </Badge>
                  ))}
                </div>
              </div>
            </div>

            <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
              <div className="rounded-xl bg-gray-800/60 p-3 text-center ring-1 ring-gray-700/50">
                <Clock className="mx-auto mb-1 size-5 text-purple-400" />
                <p className="text-xs text-gray-400">Duration</p>
                <p className="text-sm font-semibold text-white">{movie.duration}</p>
              </div>
              <div className="rounded-xl bg-gray-800/60 p-3 text-center ring-1 ring-gray-700/50">
                <Globe className="mx-auto mb-1 size-5 text-pink-400" />
                <p className="text-xs text-gray-400">Language</p>
                <p className="text-sm font-semibold text-white">{movie.language}</p>
              </div>
              <div className="rounded-xl bg-gray-800/60 p-3 text-center ring-1 ring-gray-700/50">
                <Shield className="mx-auto mb-1 size-5 text-orange-400" />
                <p className="text-xs text-gray-400">Age rating</p>
                <p className="text-sm font-semibold text-white">{movie.ageRating}</p>
              </div>
              {movie.releaseDate && (
                <div className="rounded-xl bg-gray-800/60 p-3 text-center ring-1 ring-gray-700/50">
                  <Calendar className="mx-auto mb-1 size-5 text-cyan-400" />
                  <p className="text-xs text-gray-400">Release date</p>
                  <p className="text-sm font-semibold text-white">{movie.releaseDate}</p>
                </div>
              )}
            </div>

            <div className="space-y-2">
              <h2 className="text-lg font-semibold text-white">Synopsis</h2>
              <p className="leading-relaxed text-gray-300">{movie.description}</p>
            </div>

            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <div className="flex items-center gap-2 text-gray-400">
                  <Film className="size-4" />
                  <span className="text-sm font-medium">Director</span>
                </div>
                <p className="font-semibold text-white">{movie.director}</p>
              </div>
              <div className="space-y-2">
                <div className="flex items-center gap-2 text-gray-400">
                  <Users className="size-4" />
                  <span className="text-sm font-medium">Cast</span>
                </div>
                <p className="font-semibold text-white">{movie.cast.join(', ')}</p>
              </div>
            </div>

            {!movie.isComingSoon && (
              <Button
                size="lg"
                onClick={() => navigate(buildShowtimesUrl())}
                className="w-full bg-gradient-to-r from-purple-600 to-pink-600 hover:from-purple-700 hover:to-pink-700 sm:w-auto"
              >
                <Ticket className="mr-2 size-5" />
                Book now
                <ChevronRight className="ml-2 size-4" />
              </Button>
            )}
            {movie.isComingSoon && (
              <Button
                size="lg"
                variant="outline"
                onClick={() => navigate(buildShowtimesUrl())}
                className="w-full border-purple-500/50 text-purple-300 hover:border-purple-400 hover:bg-purple-500/10 sm:w-auto"
              >
                <Calendar className="mr-2 size-5" />
                Remind me at release
              </Button>
            )}
          </div>
        </div>

        <div className="mt-12 space-y-6">
          <div className="flex items-center gap-4">
            <div className="h-px flex-1 bg-gradient-to-r from-transparent via-gray-700 to-transparent" />
            <div className="flex items-center gap-2 text-gray-400">
              <MessageSquare className="size-4 text-purple-400" />
              <span className="text-sm font-medium uppercase tracking-wider">Ratings & feedback</span>
            </div>
            <div className="h-px flex-1 bg-gradient-to-r from-transparent via-gray-700 to-transparent" />
          </div>

          <div className="grid gap-6 lg:grid-cols-[320px_1fr]">
            <div className="rounded-2xl bg-gray-800/50 p-5 ring-1 ring-gray-700/50">
              <p className="text-sm font-medium uppercase tracking-wider text-gray-500">Audience score</p>
              <div className="mt-4 flex items-end gap-2">
                <span className="text-5xl font-black text-white">{averageFeedbackRating.toFixed(1)}</span>
                <span className="pb-2 text-lg font-semibold text-gray-400">/5</span>
              </div>
              <div className="mt-3 flex gap-1">
                {ratingOptions.map((rating) => (
                  <Star
                    key={rating}
                    className={[
                      'size-5',
                      rating <= Math.round(averageFeedbackRating)
                        ? 'fill-yellow-400 text-yellow-400'
                        : 'text-gray-600',
                    ].join(' ')}
                  />
                ))}
              </div>
              <p className="mt-3 text-sm text-gray-400">{feedbacks.length} feedback for this movie</p>

              <div className="mt-6 space-y-3">
                {ratingOptions.map((rating) => {
                  const count = apiRatingBreakdown?.[rating] ?? feedbacks.filter((feedback) => feedback.rating === rating).length;
                  const breakdownTotal = apiRatingBreakdown
                    ? Object.values(apiRatingBreakdown).reduce((total, value) => total + value, 0)
                    : feedbacks.length;
                  const percentage = breakdownTotal > 0 ? (count / breakdownTotal) * 100 : 0;

                  return (
                    <div key={rating} className="grid grid-cols-[36px_1fr_28px] items-center gap-2 text-xs">
                      <span className="font-semibold text-gray-400">{rating} star</span>
                      <div className="h-2 overflow-hidden rounded-full bg-gray-700">
                        <div className="h-full rounded-full bg-yellow-400" style={{ width: `${percentage}%` }} />
                      </div>
                      <span className="text-right text-gray-500">{count}</span>
                    </div>
                  );
                })}
              </div>
            </div>

            <div className="grid gap-6 xl:grid-cols-[1fr_360px]">
              <div className="space-y-3">
                <div className="flex flex-wrap gap-2">
                  <button
                    type="button"
                    onClick={() => setFeedbackRatingFilter(null)}
                    className={[
                      'rounded-lg border px-3 py-1.5 text-xs font-semibold transition',
                      feedbackRatingFilter === null
                        ? 'border-purple-400 bg-purple-500/20 text-purple-100'
                        : 'border-gray-700 bg-gray-900/60 text-gray-400 hover:border-gray-500 hover:text-white',
                    ].join(' ')}
                  >
                    All
                  </button>
                  {ratingOptions.map((rating) => (
                    <button
                      key={rating}
                      type="button"
                      onClick={() => setFeedbackRatingFilter(rating)}
                      className={[
                        'rounded-lg border px-3 py-1.5 text-xs font-semibold transition',
                        feedbackRatingFilter === rating
                          ? 'border-yellow-400 bg-yellow-400/15 text-yellow-100'
                          : 'border-gray-700 bg-gray-900/60 text-gray-400 hover:border-gray-500 hover:text-white',
                      ].join(' ')}
                    >
                      {rating} star
                    </button>
                  ))}
                </div>
                {feedbacks.map((feedback) => (
                  <article
                    key={feedback.id}
                    className="rounded-2xl bg-gray-800/50 p-4 ring-1 ring-gray-700/50"
                  >
                    <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
                      <div>
                        <h3 className="font-bold text-white">{feedback.author}</h3>
                        <p className="text-xs text-gray-500">{feedback.createdAt}</p>
                      </div>
                      <div className="flex gap-1">
                        {ratingOptions.map((rating) => (
                          <Star
                            key={rating}
                            className={[
                              'size-4',
                              rating <= feedback.rating ? 'fill-yellow-400 text-yellow-400' : 'text-gray-600',
                            ].join(' ')}
                          />
                        ))}
                      </div>
                    </div>
                    <p className="mt-3 leading-relaxed text-gray-300">{feedback.comment}</p>
                  </article>
                ))}
              </div>

              <form
                onSubmit={handleFeedbackSubmit}
                className="rounded-2xl bg-gray-800/50 p-5 ring-1 ring-gray-700/50"
              >
                <h3 className="text-lg font-bold text-white">Share your feedback</h3>
                <p className="mt-1 text-sm text-gray-400">Your review will be saved for this movie.</p>

                <label className="mt-5 block text-sm font-semibold text-gray-300" htmlFor="reviewer-name">
                  Name
                </label>
                <input
                  id="reviewer-name"
                  value={reviewerName}
                  onChange={(event) => setReviewerName(event.target.value)}
                  placeholder="Your name"
                  className="mt-2 w-full rounded-xl border border-gray-700 bg-gray-950/60 px-3 py-2 text-sm text-white outline-none transition focus:border-purple-500 focus:ring-2 focus:ring-purple-500/20"
                />

                <fieldset className="mt-5">
                  <legend className="text-sm font-semibold text-gray-300">Rating</legend>
                  <div className="mt-2 flex gap-2">
                    {ratingOptions.toReversed().map((rating) => (
                      <button
                        key={rating}
                        type="button"
                        onClick={() => setReviewRating(rating)}
                        className="rounded-lg p-1 transition hover:bg-yellow-400/10 focus:outline-none focus:ring-2 focus:ring-yellow-400/40"
                        aria-label={`${rating} star`}
                      >
                        <Star
                          className={[
                            'size-7',
                            rating <= reviewRating ? 'fill-yellow-400 text-yellow-400' : 'text-gray-600',
                          ].join(' ')}
                        />
                      </button>
                    ))}
                  </div>
                </fieldset>

                <label className="mt-5 block text-sm font-semibold text-gray-300" htmlFor="review-comment">
                  Feedback
                </label>
                <textarea
                  id="review-comment"
                  value={reviewComment}
                  onChange={(event) => setReviewComment(event.target.value)}
                  required
                  rows={5}
                  placeholder="Tell us what you think about this movie..."
                  className="mt-2 w-full resize-none rounded-xl border border-gray-700 bg-gray-950/60 px-3 py-2 text-sm text-white outline-none transition focus:border-purple-500 focus:ring-2 focus:ring-purple-500/20"
                />

                {feedbackSubmitted && (
                  <p className="mt-3 rounded-lg bg-green-500/10 px-3 py-2 text-sm font-medium text-green-300">
                    Thanks, your feedback has been added for this movie.
                  </p>
                )}

                <Button
                  type="submit"
                  className="mt-5 w-full bg-gradient-to-r from-purple-600 to-pink-600 hover:from-purple-700 hover:to-pink-700"
                >
                  <Send className="mr-2 size-4" />
                  Submit feedback
                </Button>
              </form>
            </div>
          </div>
        </div>
      </div>

    </div>
  );
}

