SELECT
    Id,
    BookingId,
    ShowtimeId,
    PurchaserEmail,
    Status
FROM movies.MovieFeedbackRequests
WHERE PurchaserEmail = 'phamdangvinh2002@gmail.com'
ORDER BY CreatedAtUtc DESC;
