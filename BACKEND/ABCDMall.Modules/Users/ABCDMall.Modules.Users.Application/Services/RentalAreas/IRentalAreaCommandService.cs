using ABCDMall.Modules.Users.Application.Common;
using ABCDMall.Modules.Users.Application.DTOs.Common;
using ABCDMall.Modules.Users.Application.DTOs.RentalAreas;
using ABCDMall.Modules.Users.Application.DTOs;

namespace ABCDMall.Modules.Users.Application.Services.RentalAreas;

public interface IRentalAreaCommandService
{
    Task<ApplicationResult<CreateRentalAreaResponseDto>> CreateRentalAreaAsync(CreateRentalAreaDto dto, CancellationToken cancellationToken = default);

    Task<ApplicationResult<MessageResponseDto>> RegisterTenantAsync(string rentalAreaId, RegisterTenantDto dto, CancellationToken cancellationToken = default);

    Task<ApplicationResult<MessageResponseDto>> UpdateMonthlyBillAsync(string rentalAreaId, UpdateMonthlyBillDto dto, CancellationToken cancellationToken = default);

    Task<ApplicationResult<MessageResponseDto>> CancelTenantAsync(string rentalAreaId, CancellationToken cancellationToken = default);
}
