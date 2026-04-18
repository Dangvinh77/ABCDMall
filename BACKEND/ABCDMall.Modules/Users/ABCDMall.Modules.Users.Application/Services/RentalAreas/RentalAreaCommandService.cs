using System.Globalization;
using System.Text.RegularExpressions;
using ABCDMall.Modules.Users.Application.Common;
using ABCDMall.Modules.Users.Application.DTOs.Common;
using ABCDMall.Modules.Users.Application.DTOs.RentalAreas;
using ABCDMall.Modules.Users.Application.DTOs;
using ABCDMall.Modules.Users.Domain.Entities;

namespace ABCDMall.Modules.Users.Application.Services.RentalAreas;

public sealed class RentalAreaCommandService : IRentalAreaCommandService
{
    private readonly AutoMapper.IMapper _mapper;
    private readonly IRentalAreaCommandRepository _rentalAreaCommandRepository;
    private readonly IFileStorageService _fileStorageService;

    public RentalAreaCommandService(
        AutoMapper.IMapper mapper,
        IRentalAreaCommandRepository rentalAreaCommandRepository,
        IFileStorageService fileStorageService)
    {
        _mapper = mapper;
        _rentalAreaCommandRepository = rentalAreaCommandRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<ApplicationResult<CreateRentalAreaResponseDto>> CreateRentalAreaAsync(CreateRentalAreaDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.AreaCode)
            || string.IsNullOrWhiteSpace(dto.Floor)
            || string.IsNullOrWhiteSpace(dto.AreaName)
            || string.IsNullOrWhiteSpace(dto.Size))
        {
            return ApplicationResult<CreateRentalAreaResponseDto>.BadRequest("Area code, floor, area name, and size are required");
        }

        if (dto.MonthlyRent <= 0)
        {
            return ApplicationResult<CreateRentalAreaResponseDto>.BadRequest("Monthly rent must be greater than 0");
        }

        var normalizedAreaCode = dto.AreaCode.Trim();
        if (await _rentalAreaCommandRepository.ExistsRentalAreaByCodeAsync(normalizedAreaCode.ToLowerInvariant(), cancellationToken))
        {
            return ApplicationResult<CreateRentalAreaResponseDto>.BadRequest("Area code already exists");
        }

        var rentalArea = new RentalArea
        {
            AreaCode = normalizedAreaCode,
            Floor = dto.Floor.Trim(),
            AreaName = dto.AreaName.Trim(),
            Size = dto.Size.Trim(),
            MonthlyRent = dto.MonthlyRent,
            Status = "Available",
            TenantName = null,
            CreatedAt = DateTime.UtcNow
        };

        await _rentalAreaCommandRepository.AddRentalAreaAsync(rentalArea, cancellationToken);
        await _rentalAreaCommandRepository.SaveChangesAsync(cancellationToken);

        return ApplicationResult<CreateRentalAreaResponseDto>.Ok(new CreateRentalAreaResponseDto
        {
            Message = "Rental area created successfully",
            RentalArea = _mapper.Map<RentalAreaResponseDto>(rentalArea)
        });
    }

    public async Task<ApplicationResult<MessageResponseDto>> RegisterTenantAsync(string rentalAreaId, RegisterTenantDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.CCCD)
            || string.IsNullOrWhiteSpace(dto.Location)
            || dto.StartDate == default
            || dto.ElectricityFee <= 0
            || dto.WaterFee <= 0
            || dto.ServiceFee <= 0
            || dto.LeaseTermDays <= 0
            || dto.ContractImage is null)
        {
            return ApplicationResult<MessageResponseDto>.BadRequest("CCCD, location, start date, electricity fee, water fee, fee, rental duration, and contract image are required");
        }

        var minimumStartDate = DateTime.Today.AddDays(1);
        if (dto.StartDate.Date < minimumStartDate)
        {
            return ApplicationResult<MessageResponseDto>.BadRequest("Start date must be tomorrow or later");
        }

        if (dto.LeaseTermDays < 30 || dto.LeaseTermDays % 30 != 0)
        {
            return ApplicationResult<MessageResponseDto>.BadRequest("Rental duration must be at least 30 days and divisible by 30");
        }

        var rentalArea = await _rentalAreaCommandRepository.GetRentalAreaByIdAsync(rentalAreaId, cancellationToken);
        if (rentalArea is null)
        {
            return ApplicationResult<MessageResponseDto>.NotFound("Rental area does not exist");
        }

        if (rentalArea.Status == "Rented")
        {
            return ApplicationResult<MessageResponseDto>.BadRequest("This rental area already has a tenant");
        }

        var normalizedCccd = dto.CCCD.Trim();
        var manager = await _rentalAreaCommandRepository.GetManagerByCccdAsync(normalizedCccd, cancellationToken);
        if (manager is null)
        {
            return ApplicationResult<MessageResponseDto>.NotFound("Manager with this CCCD does not exist");
        }

        var shopInfo = await _rentalAreaCommandRepository.GetShopInfoByManagerAsync(manager, normalizedCccd, cancellationToken);
        if (shopInfo is null)
        {
            return ApplicationResult<MessageResponseDto>.NotFound("Shop info for this manager does not exist");
        }

        var contractPath = await _fileStorageService.SaveContractImageAsync(dto.ContractImage, cancellationToken);
        shopInfo.ManagerName = manager.FullName;
        shopInfo.CCCD = normalizedCccd;
        shopInfo.RentalLocation = dto.Location.Trim();
        shopInfo.Month = dto.StartDate.ToString("MMMM yyyy", CultureInfo.InvariantCulture);
        shopInfo.LeaseStartDate = dto.StartDate;
        shopInfo.ElectricityFee = dto.ElectricityFee;
        shopInfo.WaterFee = dto.WaterFee;
        shopInfo.ServiceFee = dto.ServiceFee;
        shopInfo.LeaseTermDays = dto.LeaseTermDays;
        shopInfo.TotalDue = dto.ElectricityFee + dto.WaterFee + dto.ServiceFee;
        shopInfo.ContractImage = contractPath;
        shopInfo.ContractImages = contractPath;

        await _rentalAreaCommandRepository.AddMonthlyBillAsync(CreateMonthlyBill(
            shopInfo,
            manager.FullName,
            dto.StartDate.ToString("yyyy-MM"),
            dto.StartDate.AddMonths(-1).ToString("yyyy-MM"),
            string.Empty,
            dto.ElectricityFee,
            string.Empty,
            dto.WaterFee,
            dto.ServiceFee,
            dto.LeaseTermDays,
            contractPath), cancellationToken);

        rentalArea.Status = "Rented";
        rentalArea.TenantName = shopInfo.ShopName;

        await _rentalAreaCommandRepository.SaveChangesAsync(cancellationToken);

        return ApplicationResult<MessageResponseDto>.Ok(new MessageResponseDto
        {
            Message = "Tenant registered successfully"
        });
    }

    public async Task<ApplicationResult<MessageResponseDto>> UpdateMonthlyBillAsync(string rentalAreaId, UpdateMonthlyBillDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.BillingMonth)
            || string.IsNullOrWhiteSpace(dto.UsageMonth)
            || string.IsNullOrWhiteSpace(dto.ElectricityUsage)
            || string.IsNullOrWhiteSpace(dto.WaterUsage))
        {
            return ApplicationResult<MessageResponseDto>.BadRequest("Billing month, usage month, electricity usage, and water usage are required");
        }

        var rentalArea = await _rentalAreaCommandRepository.GetRentalAreaByIdAsync(rentalAreaId, cancellationToken);
        if (rentalArea is null)
        {
            return ApplicationResult<MessageResponseDto>.NotFound("Rental area does not exist");
        }

        if (rentalArea.Status != "Rented")
        {
            return ApplicationResult<MessageResponseDto>.BadRequest("This rental area does not have a tenant");
        }

        var shopInfo = await _rentalAreaCommandRepository.GetShopInfoByRentalAreaAsync(rentalArea.AreaCode, rentalArea.TenantName, cancellationToken);
        if (shopInfo is null)
        {
            return ApplicationResult<MessageResponseDto>.NotFound("Shop info for this rental area does not exist");
        }

        await _rentalAreaCommandRepository.AddMonthlyBillAsync(CreateMonthlyBill(
            shopInfo,
            shopInfo.ManagerName,
            dto.BillingMonth.Trim(),
            dto.UsageMonth.Trim(),
            dto.ElectricityUsage.Trim(),
            shopInfo.ElectricityFee,
            dto.WaterUsage.Trim(),
            shopInfo.WaterFee,
            shopInfo.ServiceFee,
            shopInfo.LeaseTermDays,
            shopInfo.ContractImages ?? shopInfo.ContractImage), cancellationToken);

        await _rentalAreaCommandRepository.SaveChangesAsync(cancellationToken);

        return ApplicationResult<MessageResponseDto>.Ok(new MessageResponseDto
        {
            Message = "Monthly bill updated successfully"
        });
    }

    public async Task<ApplicationResult<MessageResponseDto>> CancelTenantAsync(string rentalAreaId, CancellationToken cancellationToken = default)
    {
        var rentalArea = await _rentalAreaCommandRepository.GetRentalAreaByIdAsync(rentalAreaId, cancellationToken);
        if (rentalArea is null)
        {
            return ApplicationResult<MessageResponseDto>.NotFound("Rental area does not exist");
        }

        if (rentalArea.Status != "Rented")
        {
            return ApplicationResult<MessageResponseDto>.BadRequest("This rental area does not have a tenant");
        }

        rentalArea.Status = "Available";
        rentalArea.TenantName = null;

        await _rentalAreaCommandRepository.SaveChangesAsync(cancellationToken);

        return ApplicationResult<MessageResponseDto>.Ok(new MessageResponseDto
        {
            Message = "Tenant rental cancelled successfully"
        });
    }

    private static ShopMonthlyBill CreateMonthlyBill(
        ShopInfo shopInfo,
        string? managerName,
        string billingMonthKey,
        string usageMonthKey,
        string electricityUsage,
        decimal electricityFee,
        string waterUsage,
        decimal waterFee,
        decimal serviceFee,
        int leaseTermDays,
        string? contractImage)
    {
        var normalizedBillingMonth = NormalizeMonthKey(billingMonthKey);
        var normalizedUsageMonth = NormalizeMonthKey(usageMonthKey);

        return new ShopMonthlyBill
        {
            ShopInfoId = shopInfo.Id ?? string.Empty,
            BillKey = $"{shopInfo.Id}:{normalizedBillingMonth}:{normalizedUsageMonth}:{Guid.NewGuid():N}",
            ShopName = shopInfo.ShopName,
            ManagerName = managerName,
            CCCD = shopInfo.CCCD,
            RentalLocation = shopInfo.RentalLocation,
            Month = FormatMonthLabel(normalizedBillingMonth),
            UsageMonth = FormatMonthLabel(normalizedUsageMonth),
            BillingMonthKey = normalizedBillingMonth,
            UsageMonthKey = normalizedUsageMonth,
            LeaseStartDate = shopInfo.LeaseStartDate,
            ElectricityUsage = electricityUsage,
            ElectricityFee = electricityFee,
            WaterUsage = waterUsage,
            WaterFee = waterFee,
            ServiceFee = serviceFee,
            LeaseTermDays = leaseTermDays,
            TotalDue = CalculateTotalDue(electricityUsage, electricityFee, waterUsage, waterFee, serviceFee),
            ContractImage = contractImage,
            ContractImages = contractImage,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static string NormalizeMonthKey(string monthKey)
    {
        if (DateTime.TryParseExact(monthKey, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var month))
        {
            return month.ToString("yyyy-MM");
        }

        if (DateTime.TryParse(monthKey, CultureInfo.InvariantCulture, DateTimeStyles.None, out month))
        {
            return month.ToString("yyyy-MM");
        }

        return monthKey;
    }

    private static string FormatMonthLabel(string monthKey)
        => DateTime.TryParseExact(monthKey, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var month)
            ? month.ToString("MMMM yyyy", CultureInfo.InvariantCulture)
            : monthKey;

    private static decimal CalculateTotalDue(
        string electricityUsage,
        decimal electricityFee,
        string waterUsage,
        decimal waterFee,
        decimal serviceFee)
    {
        var electricity = ParseUsageValue(electricityUsage);
        var water = ParseUsageValue(waterUsage);
        return electricity * electricityFee + water * waterFee + serviceFee;
    }

    private static decimal ParseUsageValue(string usage)
    {
        if (string.IsNullOrWhiteSpace(usage))
        {
            return 0;
        }

        var match = Regex.Match(usage, @"\d+([.,]\d+)?");
        if (!match.Success)
        {
            return 0;
        }

        var normalizedValue = match.Value.Replace(',', '.');
        return decimal.TryParse(normalizedValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var value)
            ? value
            : 0;
    }
}
