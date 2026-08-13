# Movie Feedback Feature Checklist

## 1. Mục tiêu

Tạo feature thu thập feedback cho từng bộ phim đã xem, với các rule nghiệp vụ:

- Chỉ gửi link feedback qua email cho người mua vé
- Chỉ gửi sau `24h` kể từ khi suất chiếu kết thúc
- Link feedback hết hạn sau `7 ngày` kể từ lúc gửi email
- Link feedback bị invalid ngay sau khi submit thành công `1 lần`

## 2. Assumption cần chốt

- Một booking movie hiện gắn với `1 showtime` và do đó gắn với `1 movie`
- Người nhận email feedback là `email người mua vé` đã nhập ở checkout
- Một booking/showtime chỉ có `1 feedback link` hợp lệ tại một thời điểm
- Mỗi link chỉ tạo ra `1 feedback submission`
- Thời điểm bắt đầu đếm `24h` là từ `showtime end time`, không phải thời điểm thanh toán
- Thời điểm hết hạn `7 ngày` là từ `sentAt` của email feedback

## 3. Checklist nghiệp vụ

- [ ] Xác định điều kiện một booking đủ điều kiện nhận feedback:
  - `booking` đã thanh toán thành công
  - `ticket` đã issued
  - `showtime` đã kết thúc
  - chưa gửi feedback link trước đó cho booking/showtime này
- [ ] Chốt scope feedback là `theo movie/showtime` hay `theo booking`
- [ ] Chốt rule chống gửi trùng nếu người mua đặt lại cùng phim nhiều lần
- [ ] Chốt nội dung feedback tối thiểu:
  - rating
  - comment
  - optional tags như `story`, `visual`, `sound`, `service`
- [ ] Chốt policy ẩn danh hay lưu thông tin purchaser/email trong feedback record

## 4. Checklist thiết kế dữ liệu

- [ ] Tạo entity/bảng `MovieFeedbackRequest`
- [ ] Các field đề xuất cho `MovieFeedbackRequest`:
  - `Id`
  - `BookingId`
  - `MovieId`
  - `ShowtimeId`
  - `PurchaserEmail`
  - `TokenHash`
  - `Status` (`Pending`, `Sent`, `Submitted`, `Expired`, `Cancelled`)
  - `AvailableAt`
  - `SentAt`
  - `ExpiresAt`
  - `SubmittedAt`
  - `InvalidatedAt`
  - `EmailRetryCount`
  - `LastEmailError`
- [ ] Tạo entity/bảng `MovieFeedback`
- [ ] Các field đề xuất cho `MovieFeedback`:
  - `Id`
  - `FeedbackRequestId`
  - `BookingId`
  - `MovieId`
  - `ShowtimeId`
  - `Rating`
  - `Comment`
  - `CreatedAt`
  - `CreatedByEmail`
  - metadata cần thiết cho moderation/report
- [ ] Tạo unique constraint để chặn `1 request -> nhiều feedback`
- [ ] Tạo unique/index để chặn duplicate request cho cùng `BookingId` hoặc `BookingId + ShowtimeId`
- [ ] Index cho các job quét theo:
  - `Status`
  - `AvailableAt`
  - `ExpiresAt`

## 5. Checklist backend flow

- [ ] Sau khi booking completed, tạo `MovieFeedbackRequest` ở trạng thái `Pending`
- [ ] Tính `AvailableAt = ShowtimeEndAt + 24h`
- [ ] Chưa gửi email ngay tại thời điểm booking success
- [ ] Tạo background job/service quét các request đến thời điểm gửi
- [ ] Chỉ gửi khi:
  - `Status = Pending`
  - `AvailableAt <= now`
  - chưa expired
  - chưa submitted
- [ ] Khi gửi email thành công:
  - generate token ngẫu nhiên
  - chỉ lưu `TokenHash`, không lưu plain token
  - set `SentAt`
  - set `ExpiresAt = SentAt + 7 days`
  - chuyển trạng thái `Sent`
- [ ] Tạo endpoint public để mở form feedback bằng token
- [ ] Tạo endpoint submit feedback bằng token
- [ ] Khi submit thành công:
  - insert `MovieFeedback`
  - update request sang `Submitted`
  - set `SubmittedAt`
  - set `InvalidatedAt`
  - invalidate token ngay lập tức
- [ ] Nếu token đã `Submitted` thì trả lỗi `link already used`
- [ ] Nếu token quá `ExpiresAt` thì trả lỗi `link expired`
- [ ] Nếu token không hợp lệ thì trả lỗi `invalid link`

## 6. Checklist email và background job

- [ ] Chuẩn bị email template feedback cho movie
- [ ] Email nên có:
  - tên phim
  - ngày giờ suất chiếu
  - CTA `Gửi đánh giá`
  - thông báo link chỉ dùng `1 lần`
  - thông báo link hết hạn sau `7 ngày`
- [ ] Tạo background service hoặc scheduler để:
  - gửi email feedback đến hạn
  - expire request quá hạn
- [ ] Có retry policy cho lỗi gửi email tạm thời
- [ ] Có log/audit cho:
  - tạo request
  - gửi email
  - email fail
  - submit feedback
  - expire link

## 7. Checklist bảo mật

- [ ] Token phải đủ dài và random cryptographically secure
- [ ] Chỉ lưu hash token trong database
- [ ] Endpoint submit phải validate token, trạng thái và thời gian hết hạn
- [ ] Chống replay: submit xong thì request không dùng lại được
- [ ] Không để lộ `bookingId`, `movieId`, `email` nhạy cảm trong URL nếu không cần
- [ ] Cân nhắc rate limit cho endpoint validate/submit token
- [ ] Có anti-spam / anti-bot tối thiểu nếu endpoint public

## 8. Checklist API đề xuất

- [ ] `POST /api/internal/movie-feedback-requests`
  - dùng nội bộ khi booking hoàn tất, hoặc trigger từ domain service
- [ ] `GET /api/movie-feedback/public/{token}`
  - validate link
  - trả metadata form: movie title, showtime, expiresAt, trạng thái link
- [ ] `POST /api/movie-feedback/public/{token}`
  - submit rating/comment
- [ ] `GET /api/admin/movie-feedback`
  - list feedback cho admin/staff
- [ ] `GET /api/movies/{movieId}/feedbacks`
  - public aggregate feedback nếu cần hiển thị ở movie detail

## 9. Checklist frontend

- [ ] Tạo trang public feedback từ email link
- [ ] Hiển thị rõ:
  - tên phim
  - thời gian suất chiếu
  - thời gian hết hạn link
- [ ] Form có:
  - rating bắt buộc
  - comment tùy chọn hoặc bắt buộc theo business
- [ ] Sau submit thành công:
  - hiển thị thank-you page
  - không cho submit lại bằng refresh/back
- [ ] Với link expired/used/invalid:
  - hiển thị message rõ ràng
  - không render form submit

## 10. Checklist test cases

- [ ] Không gửi email trước `24h` sau khi showtime kết thúc
- [ ] Gửi đúng email khi đủ `24h`
- [ ] Không gửi nếu booking chưa thanh toán thành công
- [ ] Không gửi trùng cho cùng request
- [ ] Link mở được trong thời gian hợp lệ
- [ ] Link bị expired sau `7 ngày` kể từ `SentAt`
- [ ] Submit thành công lần đầu
- [ ] Submit lần thứ hai với cùng link bị từ chối
- [ ] Link invalid khi token giả mạo
- [ ] Job expire cập nhật trạng thái đúng
- [ ] Race condition:
  - 2 request submit cùng lúc chỉ ghi nhận `1 feedback`
- [ ] Retry email fail không tạo thêm token/request trùng

## 11. Acceptance criteria

- [ ] Người mua vé chỉ nhận email feedback sau khi đã xem phim xong ít nhất `24h`
- [ ] Mỗi booking/showtime chỉ có tối đa `1 link feedback` còn hiệu lực
- [ ] Link feedback chỉ submit thành công `1 lần`
- [ ] Sau submit thành công, truy cập lại link sẽ thấy trạng thái invalid
- [ ] Nếu không submit, link tự hết hạn sau `7 ngày` kể từ lúc email được gửi
- [ ] Admin có thể xem feedback đã gửi về theo từng phim

## 12. Đề xuất chia việc

- [ ] Backend Movies/Booking:
  - xác định booking đủ điều kiện
  - tạo feedback request
- [ ] Backend Feedbacks:
  - token flow
  - submit feedback
  - admin query
- [ ] Backend Infrastructure:
  - email sender
  - background job
  - retry/logging
- [ ] Frontend:
  - public feedback page
  - success/error states
- [ ] QA:
  - test time-based flow
  - test expire/use-once flow

## 13. Gợi ý implementation order

1. Chốt data model `MovieFeedbackRequest` và `MovieFeedback`
2. Chốt rule thời gian `showtime end + 24h`
3. Tạo background job gửi email
4. Tạo token flow và public endpoints
5. Tạo frontend feedback page
6. Tạo expire job và hardening cho race condition
7. Tạo admin/report queries
