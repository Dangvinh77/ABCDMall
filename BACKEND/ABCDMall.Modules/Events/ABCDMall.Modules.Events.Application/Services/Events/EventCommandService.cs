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
        await _eventRepository.UpdateEventAsync(existing, cancellationToken);
        return ApplicationResult<bool>.Ok(true);
    }

    public async Task<ApplicationResult<bool>> RejectAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var existing = await _eventRepository.GetEventByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return ApplicationResult<bool>.NotFound("Event was not found.");
        }

        existing.ApprovalStatus = EventApprovalStatus.Rejected;
        await _eventRepository.UpdateEventAsync(existing, cancellationToken);
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
        return $"Floor {locationType} at this time is already booked by {bookedBy}.";
    }
}
