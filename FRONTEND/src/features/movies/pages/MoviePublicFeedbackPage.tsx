import { useEffect, useMemo, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { AlertCircle, ArrowLeft, MessageSquare, Send, Star } from 'lucide-react';
import { Button } from '../component/ui/button';
import {
  fetchMovieFeedbacks,
  fetchPublicMovieFeedbackRequest,
  submitMovieFeedbackByToken,
  type MovieFeedbackModel,
  type PublicMovieFeedbackRequestModel,
} from '../api/moviesApi';

const ratingOptions = [5, 4, 3, 2, 1];
const feedbackClosedMessage = 'Link feedback này đã đóng.';

function formatDateTime(value?: string | null) {
  if (!value) {
    return '';
  }

  const parsed = new Date(value);
  if (Number.isNaN(parsed.getTime())) {
    return value;
  }

  return parsed.toLocaleString('vi-VN', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}

function resolveFeedbackNotice(request?: PublicMovieFeedbackRequestModel | null) {
  if (!request || request.canSubmit) {
    return null;
  }

  if (request.status === 'Submitted' || request.expiredReason === 'SubmissionLimitReached') {
    return 'Link feedback đã đủ 3 lần gửi và đã tự động đóng.';
  }

  if (request.expiredReason === 'OpenedNoSubmission7Days') {
    return 'Link feedback đã hết hạn vì không gửi phản hồi trong 7 ngày kể từ lần mở đầu tiên.';
  }

  if (request.status === 'Pending') {
    return 'Form feedback sẽ mở sau khi suất chiếu bộ phim kết thúc.';
  }

  return request.message ?? feedbackClosedMessage;
}

export function MoviePublicFeedbackPage() {
  const { token } = useParams<{ token: string }>();
  const [request, setRequest] = useState<PublicMovieFeedbackRequestModel | null>(null);
  const [feedbacks, setFeedbacks] = useState<MovieFeedbackModel[]>([]);
  const [rating, setRating] = useState(5);
  const [displayName, setDisplayName] = useState('');
  const [comment, setComment] = useState('');
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [notice, setNotice] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const averageRating = useMemo(() => {
    if (feedbacks.length === 0) {
      return 0;
    }

    return feedbacks.reduce((total, feedback) => total + feedback.rating, 0) / feedbacks.length;
  }, [feedbacks]);

  useEffect(() => {
    if (!token) {
      setLoading(false);
      setError('Feedback link không hợp lệ.');
      return;
    }

    const feedbackToken = token;
    let active = true;

    async function loadFeedbackContext() {
      setLoading(true);
      setError(null);

      try {
        const publicRequest = await fetchPublicMovieFeedbackRequest(feedbackToken);
        const movieFeedbacks = await fetchMovieFeedbacks(publicRequest.movieId);

        if (!active) return;

        setRequest(publicRequest);
        setFeedbacks(movieFeedbacks.items);
        setNotice(resolveFeedbackNotice(publicRequest));
      } catch (loadError) {
        if (!active) return;
        setError(loadError instanceof Error ? loadError.message : 'Không thể mở link feedback.');
      } finally {
        if (active) {
          setLoading(false);
        }
      }
    }

    void loadFeedbackContext();

    return () => {
      active = false;
    };
  }, [token]);

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!token || !request?.canSubmit || !comment.trim()) {
      return;
    }

    setSubmitting(true);
    setError(null);

    try {
      const created = await submitMovieFeedbackByToken(token, {
        rating,
        comment: comment.trim(),
        displayName: displayName.trim() || undefined,
      });
      const refreshedRequest = await fetchPublicMovieFeedbackRequest(token);

      setFeedbacks((current) => [created, ...current]);
      setRequest(refreshedRequest);
      setDisplayName('');
      setComment('');
      setRating(5);
      setNotice(refreshedRequest.canSubmit ? 'Cảm ơn bạn, feedback đã được gửi.' : resolveFeedbackNotice(refreshedRequest));
    } catch (submitError) {
      setNotice(resolveFeedbackNotice(request));
      setError(submitError instanceof Error ? submitError.message : feedbackClosedMessage);
    } finally {
      setSubmitting(false);
    }
  }

  if (loading) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-gray-950 px-6 text-center text-gray-300">
        Đang mở feedback phim...
      </div>
    );
  }

  if (error && !request) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-gray-950 px-6 text-center">
        <div>
          <AlertCircle className="mx-auto mb-4 size-12 text-red-400" />
          <h1 className="text-2xl font-bold text-white">Không thể mở feedback</h1>
          <p className="mt-2 text-gray-400">{error}</p>
          <Button asChild className="mt-6 bg-purple-600 hover:bg-purple-700">
            <Link to="/movies">
              <ArrowLeft className="mr-2 size-4" />
              Về trang phim
            </Link>
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-950 px-4 py-10 text-white sm:px-6 lg:px-8">
      <main className="mx-auto max-w-5xl">
        <Link to="/movies" className="inline-flex items-center gap-2 text-sm font-semibold text-gray-400 hover:text-white">
          <ArrowLeft className="size-4" />
          ABCD Cinema
        </Link>

        <section className="mt-8 rounded-2xl border border-gray-800 bg-gray-900/80 p-6 shadow-2xl shadow-black/30 sm:p-8">
          <div className="flex flex-col gap-6 lg:flex-row lg:items-start lg:justify-between">
            <div>
              <div className="flex items-center gap-2 text-purple-300">
                <MessageSquare className="size-5" />
                <span className="text-sm font-semibold uppercase tracking-wider">Movie feedback</span>
              </div>
              <h1 className="mt-3 text-3xl font-black sm:text-4xl">{request?.movieTitle}</h1>
              <p className="mt-3 max-w-2xl text-gray-400">
                Form feedback mở từ {formatDateTime(request?.availableAtUtc)}
                {request?.expiresAtUtc ? ` đến ${formatDateTime(request.expiresAtUtc)}` : ''}.
              </p>
            </div>

            <div className="rounded-xl bg-gray-950/70 p-4 ring-1 ring-gray-800">
              <p className="text-xs font-semibold uppercase tracking-wider text-gray-500">Audience score</p>
              <div className="mt-2 flex items-end gap-2">
                <span className="text-4xl font-black">{averageRating.toFixed(1)}</span>
                <span className="pb-1 text-gray-400">/5</span>
              </div>
              <div className="mt-2 flex gap-1">
                {ratingOptions.map((option) => (
                  <Star
                    key={option}
                    className={[
                      'size-4',
                      option <= Math.round(averageRating)
                        ? 'fill-yellow-400 text-yellow-400'
                        : 'text-gray-700',
                    ].join(' ')}
                  />
                ))}
              </div>
              <p className="mt-2 text-sm text-gray-500">{feedbacks.length} feedback</p>
            </div>
          </div>

          {notice && (
            <p className="mt-6 rounded-xl border border-amber-400/30 bg-amber-400/10 px-4 py-3 text-sm font-semibold text-amber-100">
              {notice}
            </p>
          )}

          {error && request && (
            <p className="mt-4 rounded-xl border border-red-400/30 bg-red-400/10 px-4 py-3 text-sm font-semibold text-red-100">
              {error}
            </p>
          )}

          <div className="mt-8 grid gap-8 lg:grid-cols-[1fr_360px]">
            <div className="space-y-4">
              {feedbacks.length === 0 && (
                <p className="rounded-xl border border-gray-800 bg-gray-950/60 px-4 py-5 text-gray-400">
                  Chưa có feedback nào cho phim này.
                </p>
              )}

              {feedbacks.map((feedback) => (
                <article key={feedback.id} className="rounded-xl border border-gray-800 bg-gray-950/60 p-4">
                  <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
                    <div>
                      <h2 className="font-bold">{feedback.displayName}</h2>
                      <p className="text-xs text-gray-500">{formatDateTime(feedback.createdAtUtc)}</p>
                    </div>
                    <div className="flex gap-1">
                      {ratingOptions.map((option) => (
                        <Star
                          key={option}
                          className={[
                            'size-4',
                            option <= feedback.rating ? 'fill-yellow-400 text-yellow-400' : 'text-gray-700',
                          ].join(' ')}
                        />
                      ))}
                    </div>
                  </div>
                  <p className="mt-3 leading-relaxed text-gray-300">{feedback.comment}</p>
                </article>
              ))}
            </div>

            {request?.canSubmit && (
              <form onSubmit={handleSubmit} className="rounded-xl border border-gray-800 bg-gray-950/60 p-5">
                <h2 className="text-lg font-bold">Gửi feedback</h2>
                <p className="mt-1 text-sm text-gray-400">
                  Còn lại {request.remainingSubmissions} lần gửi feedback từ link này.
                </p>

                <label className="mt-5 block text-sm font-semibold text-gray-300" htmlFor="public-feedback-name">
                  Tên hiển thị
                </label>
                <input
                  id="public-feedback-name"
                  value={displayName}
                  onChange={(event) => setDisplayName(event.target.value)}
                  placeholder="Tên của bạn"
                  className="mt-2 w-full rounded-lg border border-gray-700 bg-gray-900 px-3 py-2 text-sm outline-none focus:border-purple-500 focus:ring-2 focus:ring-purple-500/20"
                />

                <fieldset className="mt-5">
                  <legend className="text-sm font-semibold text-gray-300">Đánh giá</legend>
                  <div className="mt-2 flex gap-2">
                    {[...ratingOptions].reverse().map((option) => (
                      <button
                        key={option}
                        type="button"
                        onClick={() => setRating(option)}
                        className="rounded-lg p-1 transition hover:bg-yellow-400/10 focus:outline-none focus:ring-2 focus:ring-yellow-400/40"
                        aria-label={`${option} sao`}
                      >
                        <Star
                          className={[
                            'size-7',
                            option <= rating ? 'fill-yellow-400 text-yellow-400' : 'text-gray-700',
                          ].join(' ')}
                        />
                      </button>
                    ))}
                  </div>
                </fieldset>

                <label className="mt-5 block text-sm font-semibold text-gray-300" htmlFor="public-feedback-comment">
                  Nội dung
                </label>
                <textarea
                  id="public-feedback-comment"
                  value={comment}
                  onChange={(event) => setComment(event.target.value)}
                  required
                  rows={5}
                  placeholder="Chia sẻ cảm nhận của bạn về phim..."
                  className="mt-2 w-full resize-none rounded-lg border border-gray-700 bg-gray-900 px-3 py-2 text-sm outline-none focus:border-purple-500 focus:ring-2 focus:ring-purple-500/20"
                />

                <Button
                  type="submit"
                  disabled={submitting}
                  className="mt-5 w-full bg-gradient-to-r from-purple-600 to-pink-600 hover:from-purple-700 hover:to-pink-700"
                >
                  <Send className="mr-2 size-4" />
                  {submitting ? 'Đang gửi...' : 'Gửi feedback'}
                </Button>
              </form>
            )}
          </div>
        </section>
      </main>
    </div>
  );
}
