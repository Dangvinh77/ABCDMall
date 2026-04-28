using ABCDMall.Modules.Events.Application.Common;
using ABCDMall.Modules.Events.Application.DTOs.Events;
using ABCDMall.Modules.Events.Domain.Entities;
using ABCDMall.Modules.Events.Domain.Enums;
using ABCDMall.Modules.Users.Application.Services;
using Microsoft.Extensions.Logging;

namespace ABCDMall.Modules.Events.Application.Services.Events;

public sealed class EventCommandService : IEventCommandService
{
    private readonly IEventRepository _eventRepository;
    private readonly IEmailNotificationService _emailNotificationService;
    private readonly ILogger<EventCommandService> _logger;

    public EventCommandService(IEventRepository eventRepository, IEmailNotificationService emailNotificationService, ILogger<EventCommandService> logger)
    {
        _eventRepository = eventRepository;
        _emailNotificationService = emailNotificationService;
        _logger = logger;
    }

    public Task<ApplicationResult<Guid>> CreateMallEventAsync(CreateEventRequestDto request, CancellationToken cancellationToken = default)
        => CreateInternalAsync(request, null, EventApprovalStatus.Approved, cancellationToken);

    public Task<ApplicationResult<Guid>> CreateShopEventAsync(string managerShopId, CreateEventRequestDto request, CancellationToken cancellationToken = default)
        => CreateInternalAsync(request, managerShopId, EventApprovalStatus.Pending, cancellationToken);

    public async Task<ApplicationResult<bool>> UpdateAsync(Guid id, UpdateEventRequestDto request, CancellationToken cancellationToken = default)
    {
        var existing = await _eventRepository.GetEventByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return ApplicationResult<bool>.NotFound("Event was not found.");
        }

        var locationType = (EventLocationType)request.LocationType;
        var conflict = await CheckFloorConflictAsync(locationType, request.StartDateTime, request.EndDateTime, id, cancellationToken);
        if (!string.IsNullOrWhiteSpace(conflict))
        {
            return ApplicationResult<bool>.BadRequest(conflict);
        }

        existing.Title = request.Title.Trim();
        existing.Description = request.Description?.Trim() ?? string.Empty;
        existing.ImageUrl = request.ImageUrl?.Trim() ?? string.Empty;
        existing.StartDateTime = request.StartDateTime;
        existing.EndDateTime = request.EndDateTime;
        existing.LocationType = locationType;
        existing.ShopId = string.IsNullOrWhiteSpace(request.ShopId) ? null : request.ShopId.Trim();
        existing.ApprovalStatus = (EventApprovalStatus)request.ApprovalStatus;
        existing.HasGiftRegistration = request.HasGiftRegistration;
        existing.GiftDescription = request.HasGiftRegistration ? request.GiftDescription?.Trim() : null;

        await _eventRepository.UpdateEventAsync(existing, cancellationToken);
        return ApplicationResult<bool>.Ok(true);
    }

    public async Task<ApplicationResult<bool>> ApproveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var existing = await _eventRepository.GetEventByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return ApplicationResult<bool>.NotFound("Event was not found.");
        }

        existing.ApprovalStatus = EventApprovalStatus.Approved;
        existing.ApprovedAt = DateTime.UtcNow;
        await _eventRepository.UpdateEventAsync(existing, cancellationToken);

        // Send approval email notification if event is from a shop
        if (!string.IsNullOrWhiteSpace(existing.ShopId))
        {
            var body = $"""
                        <div style="font-family:Arial,sans-serif; padding: 20px;">
                          <h2 style="color: #059669;">✓ Sự kiện đã được phê duyệt</h2>
                          <p>Kính gửi quản lý cửa hàng,</p>
                          <p>Sự kiện của bạn <strong>"{existing.Title}"</strong> đã được phê duyệt thành công bởi quản trị viên mall!</p>
                          <p><strong>Thông tin sự kiện:</strong></p>
                          <ul>
                            <li>Tên: {existing.Title}</li>
                            <li>Thời gian: {existing.StartDateTime:dd/MM/yyyy HH:mm} - {existing.EndDateTime:dd/MM/yyyy HH:mm}</li>
                            <li>Vị trí: {(existing.LocationType == EventLocationType.AtShop ? "Cửa hàng" : $"Sảnh tầng {(int)existing.LocationType - 1}")}</li>
                          </ul>
                          <p>Sự kiện của bạn hiện đã xuất hiện trên bản đồ sự kiện của mall.</p>
                          <p>Cảm ơn!</p>
                        </div>
                        """;
            _logger.LogInformation("Event {EventId} approved — shop manager email lookup not yet wired; skipping notification.", id);
            // TODO: Inject IShopRepository to resolve manager email by existing.ShopId, then call:
            // await _emailNotificationService.SendEventRegistrationSuccessEmailAsync(managerEmail, null, "Sự kiện được phê duyệt", body);
            _ = body;
        }

        return ApplicationResult<bool>.Ok(true);
    }

    public async Task<ApplicationResult<bool>> RejectAsync(Guid id, string reason, CancellationToken cancellationToken = default)
    {
        var existing = await _eventRepository.GetEventByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return ApplicationResult<bool>.NotFound("Event was not found.");
        }

        existing.ApprovalStatus = EventApprovalStatus.Rejected;
        existing.RejectionReason = reason?.Trim();
        await _eventRepository.UpdateEventAsync(existing, cancellationToken);

        // Send rejection email notification if event is from a shop
        if (!string.IsNullOrWhiteSpace(existing.ShopId))
        {
            var body = $"""
                        <div style="font-family:Arial,sans-serif; padding: 20px;">
                          <h2 style="color: #dc2626;">✗ Sự kiện bị từ chối</h2>
                          <p>Kính gửi quản lý cửa hàng,</p>
                          <p>Rất tiếc, sự kiện <strong>"{existing.Title}"</strong> đã không được phê duyệt.</p>
                          <p><strong>Lý do:</strong></p>
                          <p style="background-color: #fee2e2; padding: 10px; border-left: 4px solid #dc2626;">{reason}</p>
                          <p>Vui lòng liên hệ với quản trị viên mall để biết thêm chi tiết.</p>
                        </div>
                        """;
            _logger.LogInformation("Event {EventId} rejected — shop manager email lookup not yet wired; skipping notification.", id);
            // TODO: Inject IShopRepository to resolve manager email by existing.ShopId, then call:
            // await _emailNotificationService.SendEventRegistrationSuccessEmailAsync(managerEmail, null, "Sự kiện bị từ chối", body);
            _ = body;
        }

        return ApplicationResult<bool>.Ok(true);
    }

    public async Task<ApplicationResult<EventRegistrationResultDto>> RegisterAsync(Guid eventId, RegisterEventRequestDto request, CancellationToken cancellationToken = default)
    {
        var ev = await _eventRepository.GetEventByIdAsync(eventId, cancellationToken);
        if (ev is null || ev.ApprovalStatus != EventApprovalStatus.Approved)
        {
            return ApplicationResult<EventRegistrationResultDto>.NotFound("Event was not found.");
        }

        if (!ev.HasGiftRegistration)
        {
            return ApplicationResult<EventRegistrationResultDto>.BadRequest("This event does not support online gift registration.");
        }

        var redeemCode = Random.Shared.Next(100000, 1000000).ToString();
        var registration = new EventRegistration
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            CustomerName = request.CustomerName.Trim(),
            CustomerEmail = request.CustomerEmail.Trim(),
            CustomerPhone = request.CustomerPhone.Trim(),
            RedeemCode = redeemCode,
            RegisteredAt = DateTime.UtcNow
        };

        await _eventRepository.CreateRegistrationAsync(registration, cancellationToken);

        var body = $"""
                    <div style="font-family:Arial,sans-serif">
                      <h2>Registration Successful</h2>
                      <p>Hello {registration.CustomerName}, your event registration is confirmed.</p>
                      <p><strong>Event:</strong> {ev.Title}</p>
                      <p><strong>Time:</strong> {ev.StartDateTime:dd/MM/yyyy HH:mm} - {ev.EndDateTime:dd/MM/yyyy HH:mm}</p>
                      <p><strong>Gift:</strong> {ev.GiftDescription ?? "Gift at check-in counter"}</p>
                      <p><strong>Redeem Code:</strong> {redeemCode}</p>
                    </div>
                    """;
        await _emailNotificationService.SendEventRegistrationSuccessEmailAsync(registration.CustomerEmail, registration.CustomerName, "Registration Successful", body);

        return ApplicationResult<EventRegistrationResultDto>.Ok(new EventRegistrationResultDto
        {
            RegistrationId = registration.Id,
            RedeemCode = redeemCode,
            RegisteredAt = registration.RegisteredAt
        });
    }

    private async Task<ApplicationResult<Guid>> CreateInternalAsync(CreateEventRequestDto request, string? shopId, EventApprovalStatus approvalStatus, CancellationToken cancellationToken)
    {
        var locationType = (EventLocationType)request.LocationType;
        var conflict = await CheckFloorConflictAsync(locationType, request.StartDateTime, request.EndDateTime, null, cancellationToken);
        if (!string.IsNullOrWhiteSpace(conflict))
        {
            return ApplicationResult<Guid>.BadRequest(conflict);
        }

        var ev = new Event
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            ImageUrl = request.ImageUrl?.Trim() ?? string.Empty,
            StartDateTime = request.StartDateTime,
            EndDateTime = request.EndDateTime,
            LocationType = locationType,
            ShopId = string.IsNullOrWhiteSpace(shopId) ? request.ShopId?.Trim() : shopId.Trim(),
            ApprovalStatus = approvalStatus,
            HasGiftRegistration = request.HasGiftRegistration,
            GiftDescription = request.HasGiftRegistration ? request.GiftDescription?.Trim() : null,
            CreatedAt = DateTime.UtcNow
        };

        await _eventRepository.CreateEventAsync(ev, cancellationToken);
        _logger.LogInformation("Created event {EventId}", ev.Id);
        return ApplicationResult<Guid>.Ok(ev.Id);
    }

    private async Task<string?> CheckFloorConflictAsync(
        EventLocationType locationType,
        DateTime startDateTime,
        DateTime endDateTime,
        Guid? excludeEventId,
        CancellationToken cancellationToken)
    {
        if (locationType == EventLocationType.AtShop)
        {
            return null;
        }

        var conflicts = await _eventRepository.GetFloorConflictsAsync(locationType, startDateTime, endDateTime, excludeEventId, cancellationToken);
        var conflict = conflicts.FirstOrDefault();
        if (conflict is null)
        {
            return null;
        }

        var bookedBy = string.IsNullOrWhiteSpace(conflict.ShopId) ? "Mall" : conflict.ShopId;
        var floorDisplay = locationType switch
        {
            EventLocationType.HallFloor1 => "Hall Floor 1",
            EventLocationType.HallFloor2 => "Hall Floor 2",
            EventLocationType.HallFloor3 => "Hall Floor 3",
            EventLocationType.HallFloor4 => "Hall Floor 4",
            _ => locationType.ToString()
        };
        return $"{floorDisplay} at this time is already booked by {bookedBy}.";
    }
}
